using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using RFC.Geometry;
using RFC.Core;
using RFC.SSLVisionLib;
using RFC.RefBox;
using RFC.Utilities;
using RFC.InterProcessMessaging;

namespace RFC.Simulator
{
    public class PhysicsEngine
    {
        const double BALL_ROBOT_ELASTICITY = 0.5; //The fraction of the speed kept when bouncing off a robot
        const double BALL_WALL_ELASTICITY = 0.9; //The fraction of the speed kept when bouncing off a wall
        const double BREAKBEAM_TIMEOUT = 10; //Seconds
        const double DELAY_ADJUSTMENT = 0.01; //Add this much to dt to additionally compensate for any slowness 
        const double DRIBBLER_SPEED = .1;
        //TODO this is still the old 0-5 scale. Change this to the new 0-25 scale and remeasure kick speeds to retune
        static double[] KICK_SPEED = new double[RobotCommand.MAX_KICKER_STRENGTH / 5 + 1] { 0, 0, 1.81, 2.88, 3.33, 4.25 };

        // Friction model. All in SI units. See http://robocup.mi.fu-berlin.de/buch/rolling.pdf
        // Possible todo: look into adding rotational momentum to the simulation?
        const double BALL_FRICTION_PHASE_2 = 0.305; // The constant force against the ball in phase 2
        const double BALL_FRICTION_PHASE_2_STOP_THRESHOLD = 0.02; // The speed at which the ball can no longer climb the carpet

        private bool _marking = false;
        private Vector2 _markedPosition;

        //Running data-----------------------------------------------------------
        FunctionLoop runLoop;

        //Random-----------------------------------------------------------------
        private Random myRand = new Random();

        //Getting robot commands--------------------------------------------------
        private IMessageReceiver<RobotCommand> cmdReceiver;

        //Vision-------------------------------------------------------------------
        private bool visionStarted = false;
        SSLVisionServer sslVisionServer;

        private bool noisyVision = false;
        const double noisyVisionStdev = 0.004; //Standard deviation of gaussian noise in meters

        //Every frame, with probability (noisyVisionBallLoseProb), generate a random gaussian distributed value
        // with given mean and stdev. If the ball is closer to the surface of any robot than that value, lose the ball.
        const double noisyVisionBallLoseProb = 0.3;
        const double noisyVisionBallLoseMean = 0;
        const double noisyVisionBallLoseStdev = 0.05;
        const double noisyVisionBallUnloseRadius = 0.10; //Un-lose the ball when it goes this far away from every robot
        private bool noisyVision_BallLost = false;

        //Refbox and scenarios----------------------------------------------------
        private bool refBoxStarted = false;
        private MulticastRefboxSender refBoxSender = new MulticastRefboxSender();
        public Team LastTouched = Team.Blue;
        public IVirtualReferee Referee;

        private SimulatedScenario scenario;

        //Status of field and teams------------------------------------

        //Lock for updating anything below
        private Object updateLock = new Object();

        private int numYellow;
        private int numBlue;

        public int NumYellow { get { return numYellow; } }
        public int NumBlue { get { return numBlue; } }

        private BallInfo ball = null;
        private Dictionary<Team, List<RobotInfo>> robots;

        private Dictionary<Team, Dictionary<int, MovementModeler>> movement_modelers;
        private Dictionary<Team, Dictionary<int, WheelSpeeds>> speeds;
        private Dictionary<Team, Dictionary<int, int>> break_beam_frames;
        private Dictionary<Team, Dictionary<int, int>> kick_strengths;
        private Dictionary<Team, Dictionary<int, bool>> dribblers_on;

        public PhysicsEngine()
        {
            robots = new Dictionary<Team, List<RobotInfo>>();
            movement_modelers = new Dictionary<Team, Dictionary<int, MovementModeler>>();
            speeds = new Dictionary<Team, Dictionary<int, WheelSpeeds>>();
            break_beam_frames = new Dictionary<Team, Dictionary<int, int>>();
            kick_strengths = new Dictionary<Team, Dictionary<int, int>>();
            dribblers_on = new Dictionary<Team,Dictionary<int,bool>>();
            foreach (Team team in Enum.GetValues(typeof(Team)))
            {
                robots[team] = new List<RobotInfo>();
                movement_modelers[team] = new Dictionary<int, MovementModeler>();
                speeds[team] = new Dictionary<int, WheelSpeeds>();
                break_beam_frames[team] = new Dictionary<int, int>();
                kick_strengths[team] = new Dictionary<int, int>();
                dribblers_on[team] = new Dictionary<int,bool>();
                for (int i = 0; i < 12; i++)
                {
                    dribblers_on[team][i] = false;
                }
            }

            LoadConstants();

            Referee = new SimpleReferee();

            runLoop = new FunctionLoop(runLoopFunction, new object());
        }

        public void LoadConstants()
        {

        }

        //INITIALIZATION AND CONTROL---------------------------------------------------------------

        private void InitState()
        {
            lock (updateLock)
            {
                foreach (Team team in Enum.GetValues(typeof(Team)))
                {
                    movement_modelers[team].Clear();
                    speeds[team].Clear();
                    break_beam_frames[team].Clear();
                    kick_strengths[team].Clear();
                }

                ResetScenarioScene();

                foreach (Team team in Enum.GetValues(typeof(Team)))
                {
                    foreach (RobotInfo info in robots[team])
                    {
                        movement_modelers[team].Add(info.ID, new MovementModeler());
                        speeds[team].Add(info.ID, new WheelSpeeds());
                        break_beam_frames[team].Add(info.ID, 0);
                        kick_strengths[team].Add(info.ID, 0);
                    }
                }
            }
        }

        public void StartCommander(int port)
        {
            if (cmdReceiver != null)
                throw new ApplicationException("Already listening.");
            cmdReceiver = Messages.CreateServerReceiver<RobotCommand>(port);
            if (cmdReceiver == null)
                throw new ApplicationException("Could not listen on port " + port.ToString());
            cmdReceiver.MessageReceived += cmdReceiver_MessageReceived;
        }

        public void StopCommander()
        {
            if (cmdReceiver == null)
                throw new ApplicationException("Not listening.");
            cmdReceiver.Close();
            cmdReceiver.MessageReceived -= cmdReceiver_MessageReceived;
            cmdReceiver = null;
        }

        public void StartVision(string host, int port)
        {
            if (visionStarted)
                throw new ApplicationException("Vision already running.");

            sslVisionServer = new SSLVisionServer();
            sslVisionServer.Connect(host, port);

            visionStarted = true;
        }

        public void StopVision()
        {
            if (!visionStarted)
                throw new ApplicationException("Vision not running.");

            sslVisionServer.Disconnect();

            visionStarted = false;
        }

        public void SetNoisyVision(bool b)
        {
            noisyVision = b;
        }

        public void StartReferee(string host, int port)
        {
            if (refBoxStarted)
                throw new ApplicationException("Referee already running.");

            refBoxSender.Connect(host, port);

            refBoxStarted = true;
        }

        public void StopReferee()
        {
            if (!refBoxStarted)
                throw new ApplicationException("Referee not running.");

            refBoxSender.Disconnect();
            refBoxStarted = false;
        }

        public void SetScenario(SimulatedScenario scenario)
        {
            if (runLoop.IsRunning())
                throw new ApplicationException("Cannot change scenario while running");
            this.scenario = scenario;
        }

        public void ResetScenarioScene()
        {
            SimulatorScene scene = scenario.GetScene();

            lock (updateLock)
            {
                ball = scene.Ball;
                foreach (Team team in Enum.GetValues(typeof(Team)))
                {
                    robots[team].Clear();
                    robots[team].AddRange(scene.Robots[team]);
                }
            }
        }

        public void Start(int numYellow, int numBlue)
        {
            if (runLoop.IsRunning())
                throw new ApplicationException("Already running.");

            this.numYellow = numYellow;
            this.numBlue = numBlue;

            InitState();

            double freq = Constants.Time.SIM_ENGINE_FREQUENCY;
            double period = 1.0 / freq * 1000; // in ms

            runLoop.SetPeriod(1.0 / Constants.Time.SIM_ENGINE_FREQUENCY /10);
            runLoop.Start();
        }

        public void Stop()
        {
            if (!runLoop.IsRunning())
                throw new ApplicationException("Not running.");
            runLoop.Stop();
        }

        //MAIN PHYSICS LOOP------------------------------------------------------------------

        private void runLoopFunction()
        {
            step(runLoop.GetPeriod() + DELAY_ADJUSTMENT);
        }

        /// <summary>
        /// Steps forward the given number of seconds
        /// </summary>
        private void step(double dt)
        {
            dt = dt/2;
            lock (updateLock)
            {
                //Grab constants for this iteration
                double BALL_RADIUS = Constants.Basic.BALL_RADIUS;
                double ROBOT_RADIUS = Constants.Basic.ROBOT_RADIUS;
                double ROBOT_FRONT_RADIUS = Constants.Basic.ROBOT_FRONT_RADIUS;
                double BALL_COLLISION_RADIUS = BALL_RADIUS + ROBOT_RADIUS;


                //Move robots
                foreach (Team team in Enum.GetValues(typeof(Team)))
                    foreach (RobotInfo info in robots[team])
                        UpdateRobot(info, movement_modelers[team][info.ID].ModelWheelSpeeds(info, speeds[team][info.ID], dt));

                //Build list of all robots
                List<RobotInfo> allRobots = new List<RobotInfo>();
                foreach (Team team in Enum.GetValues(typeof(Team)))
                    allRobots.AddRange(robots[team]);

                // Fix robot-robot collisions
                for (int i = 0; i < allRobots.Count; i++)
                {
                    for (int j = 0; j < allRobots.Count; j++)
                    {
                        if (i == j)
                            continue;
                        Vector2 p1 = allRobots[i].Position;
                        Vector2 p2 = allRobots[j].Position;
                        if (p1.distanceSq(p2) <= (2 * ROBOT_RADIUS) * (2 * ROBOT_RADIUS))
                        {
                            Vector2 t1 = p1 + .01 * (p1 - p2).normalize();
                            Vector2 t2 = p2 + .01 * (p2 - p1).normalize();
                            UpdateRobot(allRobots[i], new RobotInfo(t1, allRobots[i].Orientation,
                                allRobots[i].Team, allRobots[i].ID));
                            UpdateRobot(allRobots[j], new RobotInfo(t2, allRobots[j].Orientation,
                                allRobots[j].Team, allRobots[j].ID));
                        }
                    }
                }

                stepBall(dt);

                //Drain breakbeam timeout frames
                foreach (Team team in Enum.GetValues(typeof(Team)))
                {
                    foreach (RobotInfo robot in robots[team])
                    {
                        if (break_beam_frames[team][robot.ID] > 0)
                            break_beam_frames[team][robot.ID]--;
                    }
                }



                // Synchronously (at least for now) send out a vision message
                SSL_WrapperPacket packet = constructSSLVisionFrame();
                sslVisionServer.Send(packet);

                //Handle referee
                if (refBoxStarted)
                    refBoxSender.SendCommand(Referee.GetLastCommand());
            }
        }

        /// <summary>
        /// Steps forward the given number of seconds
        /// </summary>
        delegate Vector2 kinematicPredictor(double timeFromNow);
        delegate double kinematicInversePredictor(Vector2 v);
        delegate void earliestReplacer(double newTime, ballInfoCreater newEventFunction);
        delegate BallInfo ballInfoCreater();
        private Vector2 ballPositionPredict(double timeFromNow)
        {
            double stopTime = ballVelocityInversePredict(Vector2.ZERO);
            timeFromNow = Math.Min(timeFromNow, stopTime);
            return ball.Position + ball.Velocity * timeFromNow - 0.5 * ball.Velocity.normalize() * BALL_FRICTION_PHASE_2 * timeFromNow * timeFromNow; // Use 0.5*a*t^2 + v_0*t + x*0
        }
        private double ballPositionInversePredict(Vector2 target)
        {
            double distanceToV = (target - ball.Position) * ball.Velocity.normalize(); // Get component in the direction of our motion
            double discriminant = ball.Velocity.magnitudeSq() - 2 * BALL_FRICTION_PHASE_2 * distanceToV;
            if (discriminant > 0)
                return (ball.Velocity.magnitude() - Math.Sqrt(discriminant)) / BALL_FRICTION_PHASE_2; // Ball will get to the point, use the quadratic formula to get the time
            else
                return Double.PositiveInfinity; // Ball will never get to the point
        }
        private Vector2 ballVelocityPredict(double timeFromNow)
        {
            double stopTime = ballVelocityInversePredict(Vector2.ZERO);
            timeFromNow = Math.Min(timeFromNow, stopTime);
            return ball.Velocity - ball.Velocity.normalize() * BALL_FRICTION_PHASE_2 * timeFromNow; // Use a*t + v_0
        }
        private double ballVelocityInversePredict(Vector2 target)
        {
            double parallel = target * ball.Velocity.normalize(); // Get component in the direction of our motion
            if (parallel < 0)
                return Double.PositiveInfinity; // Friction cannot reverse the ball
            else
                return (ball.Velocity.magnitude() - parallel) / BALL_FRICTION_PHASE_2;
        }
        private void stepBall(double remainingTime)
        {            
            // Collect results, find the first to occur
            // Default to the end of the step state
            double earliestEventTimeFromNow = remainingTime;
            BallInfo earliestEvent = new BallInfo(ball);
            earliestEvent.Position = ballPositionPredict(remainingTime);
            earliestEvent.Velocity = ballVelocityPredict(remainingTime);
            earliestReplacer replaceEarliestIfEarlierButNotNegative = (newTime, newEventFunction) => // Ask for the new time of the event, and a callback to get the actual event (to avoid extra computation)
                {
                    if (newTime < earliestEventTimeFromNow && newTime > 0)
                    {
                        earliestEventTimeFromNow = newTime;
                        earliestEvent = newEventFunction();
                    }
                };

            // Check for ball out of bounds, and in goal
            //      Top:
            Vector2 outOfBoundsTop = ball.Position + ball.Velocity * ((Constants.Field.YMAX + Constants.Basic.BALL_RADIUS - ball.Position.Y) / ball.Velocity.X);
            double outOfBoundsTopTime = ballPositionInversePredict(outOfBoundsTop);
            replaceEarliestIfEarlierButNotNegative(outOfBoundsTopTime, () =>
                {
                    BallInfo ev = new BallInfo(ball);
                    ev.Position = outOfBoundsTop;
                    ev.Velocity = Vector2.ZERO;
                    return ev;
                });
            //      Bottom:
            Vector2 outOfBoundsBottom = ball.Position + ball.Velocity * ((-Constants.Field.YMIN + Constants.Basic.BALL_RADIUS + ball.Position.Y) / ball.Velocity.X);
            double outOfBoundsBottomTime = ballPositionInversePredict(outOfBoundsBottom);
            replaceEarliestIfEarlierButNotNegative(outOfBoundsBottomTime, () =>
            {
                BallInfo ev = new BallInfo(ball);
                ev.Position = outOfBoundsBottom;
                ev.Velocity = Vector2.ZERO;
                return ev;
            });
            //      Left:
            Vector2 outOfBoundsLeft = ball.Position + ball.Velocity * ((-Constants.Field.XMIN + Constants.Basic.BALL_RADIUS + ball.Position.X) / ball.Velocity.Y);
            double outOfBoundsLeftTime = ballPositionInversePredict(outOfBoundsLeft);
            replaceEarliestIfEarlierButNotNegative(outOfBoundsLeftTime, () =>
            {
                BallInfo ev = new BallInfo(ball);
                ev.Position = outOfBoundsLeft;
                ev.Velocity = Vector2.ZERO;
                return ev;
            });
            //      Right:
            Vector2 outOfBoundsRight = ball.Position + ball.Velocity * ((Constants.Field.XMAX + Constants.Basic.BALL_RADIUS - ball.Position.X) / ball.Velocity.Y);
            double outOfBoundsRightTime = ballPositionInversePredict(outOfBoundsRight);
            replaceEarliestIfEarlierButNotNegative(outOfBoundsRightTime, () =>
            {
                BallInfo ev = new BallInfo(ball);
                ev.Position = outOfBoundsRight;
                ev.Velocity = Vector2.ZERO;
                return ev;
            });

            // Check for collisions with robots
            double robotCircleCollisionRadius = Constants.Basic.ROBOT_RADIUS + Constants.Basic.BALL_RADIUS, robotFrontCollisionRadius = Constants.Basic.ROBOT_FRONT_RADIUS + Constants.Basic.BALL_RADIUS;
            foreach (Team team in Enum.GetValues(typeof(Team)))
            {
                foreach (RobotInfo robot in robots[team])
                {
                    // TODO: throw out if the robot is really far away, for performance
                    // TODO: check if the robot jumped on top of our ball

                    Vector2 pathToRobot = (robot.Position - ball.Position).perpendicularComponent(ball.Velocity);
                    if (pathToRobot.magnitude() < robotCircleCollisionRadius) // If true, the ball may or may not contact the robot. If false, the ball definitely won't touch the robot
                    {
                        // First, consider the ball hitting the round part of the robot
                        Vector2 ballRobotCollisionPoint = ball.Position + (robot.Position - ball.Position).parallelComponent(ball.Velocity) - ball.Velocity.normalizeToLength(Math.Sqrt(robotCircleCollisionRadius * robotCircleCollisionRadius - pathToRobot.magnitudeSq()));
                        // In this case, the ball bounces away from the center of the robot
                        Vector2 impulseDirection = ballRobotCollisionPoint - robot.Position;

                        // Second, consider the ball hitting the flat part of the robot
                        // Limit the scope of the variables because there are a lot
                        {
                            // Represent the lines as homogenous vectors
                            double line1X = ball.Velocity.Y, line1Y = -ball.Velocity.X, line1W = Vector2.cross(ball.Velocity, ball.Position);
                            double line2X = Math.Cos(robot.Orientation), line2Y = Math.Sin(robot.Orientation), line2W = line2X * robot.Position.X + line2Y * robot.Position.Y + Constants.Basic.ROBOT_FRONT_RADIUS;

                            // Find the intersection as a homogenous vector
                            double intersectionX = line1Y * line2W - line1W * line2Y,
                                   intersectionY = line1W * line2X - line1X * line2W,
                                   intersectionW = -(line1X * line2Y - line1Y * line2X); // TODO: I don't know why this needs a negative, but it works...

                            // If intersectionW is zero, the lines are parallel, so they do not intersect
                            if (intersectionW != 0)
                            {
                                // The actual intersection is a 2d vector
                                Vector2 flatIntersectionPoint = new Vector2(intersectionX / intersectionW, intersectionY / intersectionW);

                                // This only matters if the ball hits the part of the line within the circle
                                if (flatIntersectionPoint.distance(robot.Position) < robotCircleCollisionRadius)
                                {
                                    ballRobotCollisionPoint = flatIntersectionPoint;
                                    // In this case, the ball bounces in the direction that the robot is facing
                                    impulseDirection = new Vector2(robot.Orientation);
                                    // TODO: handle kicking
                                }
                            }
                        }

                        double ballRobotCollisionTime = ballPositionInversePredict(ballRobotCollisionPoint);
                        replaceEarliestIfEarlierButNotNegative(ballRobotCollisionTime, () =>
                        {
                            BallInfo ev = new BallInfo(ball);
                            ev.Position = ballRobotCollisionPoint;
                            ev.Velocity = ball.Velocity - (1 + BALL_ROBOT_ELASTICITY) * ball.Velocity.parallelComponent(impulseDirection);
                            return ev;
                        });
                    }
                }
            }

            // Move the ball
            UpdateBall(earliestEvent);

            // Run the referee
            RefereeDeclaration decl = Referee.RunRef(this);
            if (decl == RefereeDeclaration.DECLARE_BALL_OUT)
            {
                scenario.BallOut(ball.Position);
                return;
            }
            if (decl == RefereeDeclaration.DECLARE_GOAL_SCORED)
            {
                scenario.GoalScored();
                return;
            }

            // Repeat if there is time remaining
            if (remainingTime - earliestEventTimeFromNow > 0.001) // TODO what to put instead of 0.001?
            {
                stepBall(remainingTime - earliestEventTimeFromNow);
            }
        }

        //UTILITY----------------------------------------------------------------------

        public void UpdateRobot(RobotInfo old_info, RobotInfo new_info)
        {
            if (old_info.Team != new_info.Team)
                throw new ApplicationException("old robot team and new robot team dont match!");
            if (old_info.ID != new_info.ID)
                throw new ApplicationException("old robot id and new robot ids dont match!");

            int index = robots[old_info.Team].IndexOf(old_info);
            if (index >= 0)
            {
                robots[old_info.Team][index].Position = new_info.Position;
                robots[old_info.Team][index].Velocity = new_info.Velocity;
                robots[old_info.Team][index].AngularVelocity = new_info.AngularVelocity;
                robots[old_info.Team][index].Orientation = new_info.Orientation;
            }
            else
            {
                throw new ApplicationException("no robot found with id " + old_info.ID + " on team " + old_info.Team);
            }
        }

        public void UpdateBall(BallInfo new_info)
        {
            ball.Position = new_info.Position;
            ball.Velocity = new_info.Velocity;
            ball.LastSeen = new_info.LastSeen;
        }

        //KICKING-----------------------------------------------------------------------------

        private double getKickSpeed(int kickerStrength)
        {
            kickerStrength /= 5;
            if (kickerStrength < 0)
                kickerStrength = 0;
            if (kickerStrength >= KICK_SPEED.Length)
                kickerStrength = KICK_SPEED.Length - 1;

            return KICK_SPEED[kickerStrength];
        }

        private bool tryKick(RobotInfo robot, int kickerStrength)
        {
            const double CENTER_TO_KICKER_DIST = 0.07;
            const double KICKER_ACTIVITY_RADIUS = 0.04; //.04

            Vector2 robotFaceDir = new Vector2(robot.Orientation);
            Vector2 kickerPosition = robot.Position + CENTER_TO_KICKER_DIST * robotFaceDir;
            if (kickerPosition.distanceSq(ball.Position) < KICKER_ACTIVITY_RADIUS * KICKER_ACTIVITY_RADIUS)
            {
                Vector2 relative_veloc = ball.Velocity - robot.Velocity;
                Vector2 reflection = -relative_veloc.reflectOver(robotFaceDir);

                Vector2 newVel = robotFaceDir * getKickSpeed(kickerStrength);
                LastTouched = robot.Team;
                UpdateBall(new BallInfo(ball.Position, newVel + reflection));
                Console.WriteLine("Kick ! " + kickerStrength);
                return true;
            }
            return false;
        }


        //VISION-------------------------------------------------------------------------

        private Vector2 convertToSSLCoords(Vector2 pt)
        {
            return new Vector2(pt.X * 1000, pt.Y * 1000); // m to mm
        }

        private double getGaussianRandom()
        {
            double u1 = myRand.NextDouble();
            double u2 = myRand.NextDouble();
            return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        }

        private double applyVisionNoise(double d)
        {
            if (!noisyVision)
                return d;

            return d + getGaussianRandom() * noisyVisionStdev;
        }

        private Vector2 applyVisionNoise(Vector2 v)
        {
            return new Vector2(applyVisionNoise(v.X), applyVisionNoise(v.Y));
        }

        private double minDistFromBallToRobot()
        {
            double minDist = Double.PositiveInfinity;
            foreach (Team team in Enum.GetValues(typeof(Team)))
            {
                foreach (RobotInfo info in robots[team])
                {
                    double dist = info.Position.distance(ball.Position);
                    if (dist < minDist)
                        minDist = dist;
                }
            }
            return minDist - Constants.Basic.BALL_RADIUS + Constants.Basic.ROBOT_RADIUS;
        }

        private bool checkVisionBallLoss()
        {
            if (!noisyVision)
                return false;

            if (noisyVision_BallLost)
            {
                //See if we are far enough from robots to unlose the ball now
                if (minDistFromBallToRobot() >= noisyVisionBallUnloseRadius)
                    noisyVision_BallLost = false;
            }

            //Lose the ball, maybe
            if (!noisyVision_BallLost && myRand.NextDouble() < noisyVisionBallLoseProb)
            {
                double loseBallRadius = Math.Abs(noisyVisionBallLoseMean + noisyVisionBallLoseStdev * getGaussianRandom());
                if (minDistFromBallToRobot() < loseBallRadius)
                    noisyVision_BallLost = true;
            }

            return noisyVision_BallLost;
        }

        private SSL_WrapperPacket constructSSLVisionFrame()
        {
            SSL_WrapperPacket packet = new SSL_WrapperPacket();
            SSL_DetectionFrame frame = new SSL_DetectionFrame();

            if (!checkVisionBallLoss())
            {
                Vector2 ballPos = convertToSSLCoords(applyVisionNoise(ball.Position));
                SSL_DetectionBall newBall = new SSL_DetectionBall();
                newBall.confidence = 1;
                newBall.x = (float)ballPos.X;
                newBall.y = (float)ballPos.Y;
                frame.balls.Add(newBall);
            }
            foreach (RobotInfo robot in robots[Team.Yellow])
            {
                Vector2 pos = convertToSSLCoords(applyVisionNoise(robot.Position));
                SSL_DetectionRobot newBot = new SSL_DetectionRobot();
                newBot.confidence = 1;
                newBot.robot_id = (uint)robot.ID;
                newBot.x = (float)pos.X;
                newBot.y = (float)pos.Y;
                newBot.orientation = (float)robot.Orientation;

                frame.robots_yellow.Add(newBot);
            }
            foreach (RobotInfo robot in robots[Team.Blue])
            {
                Vector2 pos = convertToSSLCoords(applyVisionNoise(robot.Position));
                SSL_DetectionRobot newBot = new SSL_DetectionRobot();
                newBot.confidence = 1;
                newBot.robot_id = (uint)robot.ID;
                newBot.x = (float)pos.X;
                newBot.y = (float)pos.Y;
                newBot.orientation = (float)robot.Orientation;

                frame.robots_blue.Add(newBot);
            }

            // dummy-fill required fields
            frame.frame_number = 0;
            frame.camera_id = 0;
            frame.t_capture = 0;
            frame.t_sent = 0;

            packet.detection = frame;

            return packet;
        }


        //COMMAND HANDLER--------------------------------------------------------------------

        private void cmdReceiver_MessageReceived(RobotCommand command)
        {
            lock (updateLock)
            {
                // This is not the cleanest, but it's ok, because these IDs are also issued by this
                // engine -- so, the convention is contained. Adding a team into RobotCommand does 
                // *not* make much sense.
                Team team = command.ID < 5 ? Team.Yellow : Team.Blue;

                switch (command.command)
                {
                    case RobotCommand.Command.MOVE:
                        this.speeds[team][command.ID] = command.Speeds;
                        break;
                    case RobotCommand.Command.KICK:
                        RobotInfo robot = robots[team].Find(r => r.ID == command.ID);
                        break_beam_frames[team][command.ID] = 1;
                        kick_strengths[team][command.ID] = RobotCommand.MAX_KICKER_STRENGTH;
                        break;

                    //TODO: Make the simulator handle these differently/appropriately
                    case RobotCommand.Command.BREAKBEAM_KICK:
                    case RobotCommand.Command.FULL_BREAKBEAM_KICK:
                    case RobotCommand.Command.MIN_BREAKBEAM_KICK:
                        break_beam_frames[team][command.ID] = (int)(BREAKBEAM_TIMEOUT / runLoop.GetPeriod());
                        kick_strengths[team][command.ID] = command.KickerStrength;
                        break;

                    case RobotCommand.Command.START_DRIBBLER:
                        dribblers_on[team][command.ID] = true;
                        Console.WriteLine("turned on dribbler");
                        break;
                    case RobotCommand.Command.STOP_DRIBBLER:
                        dribblers_on[team][command.ID] = false;
                        break;
                }
            }
        }

        //IPREDICTOR---------------------------------------------------------------

        #region IPredictor
        public List<RobotInfo> GetRobots()
        {
            List<RobotInfo> result = new List<RobotInfo>();
            foreach (Team team in Enum.GetValues(typeof(Team)))
            {
                result.AddRange(robots[team]);
            }
            return result;
        }

        public List<RobotInfo> GetRobots(Team team)
        {
            return robots[team];
        }

        public RobotInfo GetRobot(Team team, int id)
        {
            RobotInfo result = robots[team].Find(robot => robot.ID == id);

            if (result == null)
                throw new ApplicationException(String.Format("Simulator can't find robot with id: {0} on team {1}", id, team));

            return result;
        }

        public BallInfo getBall()
        {
            return ball;
        }

        public void SetBallMark()
        {
            if (ball != null)
                _markedPosition = ball.Position;

            _marking = true;
        }

        public void ClearBallMark()
        {
            _markedPosition = null;
            _marking = false;
        }

        public bool HasBallMoved()
        {
            if (!_marking)
                return false;

            if (_markedPosition == null)
                return false;

            if (ball == null)
                return false;

            double BALL_MOVED_DIST = Constants.Plays.BALL_MOVED_DIST;
            return _markedPosition.distanceSq(ball.Position) > BALL_MOVED_DIST * BALL_MOVED_DIST;
        }

        public void SetPlayType(PlayType newPlayType)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}