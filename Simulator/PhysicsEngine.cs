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
        const double BALL_FRICTION = .76; //The amount of speed lost per second by the ball
        const double BREAKBEAM_TIMEOUT = 10; //Seconds
        const double DELAY_ADJUSTMENT = 0.01; //Add this much to dt to additionally compensate for any slowness 

        //TODO this is still the old 0-5 scale. Change this to the new 0-25 scale and remeasure kick speeds to retune
        static double[] KICK_SPEED = new double[RobotCommand.MAX_KICKER_STRENGTH / 5 + 1] { 0, 0, 1.81, 2.88, 3.33, 4.25 };

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

        public PhysicsEngine()
        {
            robots = new Dictionary<Team, List<RobotInfo>>();
            movement_modelers = new Dictionary<Team, Dictionary<int, MovementModeler>>();
            speeds = new Dictionary<Team, Dictionary<int, WheelSpeeds>>();
            break_beam_frames = new Dictionary<Team, Dictionary<int, int>>();
            kick_strengths = new Dictionary<Team, Dictionary<int, int>>();

            foreach (Team team in Enum.GetValues(typeof(Team)))
            {
                robots[team] = new List<RobotInfo>();
                movement_modelers[team] = new Dictionary<int, MovementModeler>();
                speeds[team] = new Dictionary<int, WheelSpeeds>();
                break_beam_frames[team] = new Dictionary<int, int>();
                kick_strengths[team] = new Dictionary<int, int>();
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

            runLoop.SetPeriod(1.0 / Constants.Time.SIM_ENGINE_FREQUENCY);
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

                //To get better resolution, accurate bounces, and prevent the ball from going through stuff, 
                //use dt/64 for the next part, 64 times.
                dt = dt / 64.0;
                bool ballOut = false;
                bool goalScored = false;
                for (int reps = 0; reps < 64; reps++)
                {
                    //Attempt kicking
                    foreach (Team team in Enum.GetValues(typeof(Team)))
                    {
                        foreach (RobotInfo robot in robots[team])
                        {
                            int frames = break_beam_frames[team][robot.ID];
                            if (frames > 0)
                            {
                                if (tryKick(robot, kick_strengths[team][robot.ID]))
                                    break_beam_frames[team][robot.ID] = 0;
                            }
                        }
                    }

                    // Update ball location
                    Vector2 newBallLocation = ball.Position + dt * ball.Velocity;
                    Vector2 newBallVelocity;
                    double ballSpeed = ball.Velocity.magnitude();
                    ballSpeed -= BALL_FRICTION * dt;
                    if (ballSpeed <= 0)
                    { ballSpeed = 0; newBallVelocity = Vector2.ZERO; }
                    else
                    { newBallVelocity = ball.Velocity.normalizeToLength(ballSpeed); }

                    // Check for collisions ball-robot and update ball position
                    foreach (RobotInfo r in allRobots)
                    {
                        Vector2 robotLoc = r.Position;

                        bool collided = false;
                        //Possible collision
                        if (newBallLocation.distanceSq(robotLoc) <= BALL_COLLISION_RADIUS * BALL_COLLISION_RADIUS)
                        {
                            //We have a virtual line (BALL_RADIUS+ROBOT_FRONT_RADIUS) in front of the robot
                            //Make sure the ball is on the robot-side of this line as well.
                            Vector2 robotToBall = newBallLocation - robotLoc;
                            Vector2 robotOrientationVec = Vector2.GetUnitVector(r.Orientation);
                            double projectionLen = robotToBall.projectionLength(robotOrientationVec);
                            if (projectionLen <= BALL_RADIUS + ROBOT_FRONT_RADIUS)
                                collided = true;
                        }

                        if (collided)
                        {
                            LastTouched = r.Team;

                            //Compute new position of ball
                            newBallLocation = robotLoc + (BALL_COLLISION_RADIUS + .005) * (ball.Position - robotLoc).normalize();

                            //Compute new velocity of ball
                            Vector2 relVel = newBallVelocity - r.Velocity; //The relative velocity of the ball to the robot
                            Vector2 normal = newBallLocation - robotLoc; //The normal to the collision

                            //If the relative velocity is away from the robot somehow, then leave it unchanged
                            if (relVel * normal >= 0)
                                break;

                            //If somehow the normal is zero or too small, just reflect the ball straight back...
                            if (normal.magnitudeSq() <= 1e-16)
                                relVel = -BALL_ROBOT_ELASTICITY * relVel;
                            else
                            {
                                //Perpendicular component is unaffected. Parallel component reverses and
                                //is discounted by BOUNCE_ELASTICITY
                                //This is wrong for when the ball hits the front of the robot, but hopefully
                                //it's not too horrendous of an approximation
                                relVel = -BALL_ROBOT_ELASTICITY * relVel.parallelComponent(normal)
                                    + relVel.perpendicularComponent(normal);
                            }

                            //Translate back to absolute velocity
                            newBallVelocity = relVel + r.Velocity;
                            break;
                        }
                    }

                    // Do ball-wall collisions
                    {
                        double ballX = newBallLocation.X;
                        double ballY = newBallLocation.Y;
                        double ballVx = newBallVelocity.X;
                        double ballVy = newBallVelocity.Y;
                        if (ballX < Constants.Field.FULL_XMIN)
                        { ballVx = Math.Abs(ballVx) * BALL_WALL_ELASTICITY; ballX = 2 * Constants.Field.FULL_XMIN - ballX; }
                        else if (ballX > Constants.Field.FULL_XMAX)
                        { ballVx = -Math.Abs(ballVx) * BALL_WALL_ELASTICITY; ballX = 2 * Constants.Field.FULL_XMAX - ballX; }
                        if (ballY < Constants.Field.FULL_YMIN)
                        { ballVy = Math.Abs(ballVy) * BALL_WALL_ELASTICITY; ballY = 2 * Constants.Field.FULL_YMIN - ballY; }
                        else if (ballY > Constants.Field.FULL_YMAX)
                        { ballVy = -Math.Abs(ballVy) * BALL_WALL_ELASTICITY; ballY = 2 * Constants.Field.FULL_YMAX - ballY; }

                        newBallVelocity = new Vector2(ballVx, ballVy);
                        newBallLocation = new Vector2(ballX, ballY);
                    }

                    UpdateBall(new BallInfo(newBallLocation, newBallVelocity));
                    RefereeDeclaration decl = Referee.RunRef(this);
                    if (decl == RefereeDeclaration.DECLARE_BALL_OUT)
                    { ballOut = true; break; }
                    if (decl == RefereeDeclaration.DECLARE_GOAL_SCORED)
                    { goalScored = true; break; }
                }

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
                if (ballOut)
                    scenario.BallOut(ball.Position);
                if (goalScored)
                    scenario.GoalScored();
                if (refBoxStarted)
                    refBoxSender.SendCommand(Referee.GetLastCommand());
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
            const double KICKER_ACTIVITY_RADIUS = 0.04;

            Vector2 robotFaceDir = new Vector2(robot.Orientation);
            Vector2 kickerPosition = robot.Position + CENTER_TO_KICKER_DIST * robotFaceDir;
            if (kickerPosition.distanceSq(ball.Position) < KICKER_ACTIVITY_RADIUS * KICKER_ACTIVITY_RADIUS)
            {
                Vector2 newVel = robotFaceDir * getKickSpeed(kickerStrength);
                LastTouched = robot.Team;
                UpdateBall(new BallInfo(ball.Position, newVel));
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