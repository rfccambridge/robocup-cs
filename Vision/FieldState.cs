using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RFC.Geometry;
using RFC.Core;
using RFC.Messaging;
using RFC.Utilities;

namespace RFC.Vision
{
    // The state of the field believed in by a camera
    class FieldState
    {
        // The believed state is kept here
        private BallInfo ball = null;
        private Dictionary<Team, List<RobotInfo>> robots = new Dictionary<Team, List<RobotInfo>>();

        // Tools for velocity measurement
        double ballDtStart;
        BallInfo ballAtDtStart = null;
        Dictionary<Team, List<RobotInfo>> robotsAtDtStart = new Dictionary<Team, List<RobotInfo>>();
        Dictionary<Team, List<double>> velocityDtStart = new Dictionary<Team, List<double>>();

        // For assigning IDs to unidentified robots; one per team
        Dictionary<Team, int> nextID = new Dictionary<Team, int>();

        public FieldState()
        {
            foreach (Team team in Enum.GetValues(typeof(Team)))
            {
                robots.Add(team, new List<RobotInfo>());
                robotsAtDtStart.Add(team, new List<RobotInfo>());
                velocityDtStart.Add(team, new List<double>());
                nextID.Add(team, -1);
            }
        }

        // Update the believed state with new observations
        public void Update(VisionMessage msg)
        {
            double time = HighResTimer.SecondsSinceStart();

            double WEIGHT_OLD = Constants.Predictor.WEIGHT_OLD;
            double WEIGHT_NEW = Constants.Predictor.WEIGHT_NEW;
            double DELTA_DIST_SQ_MERGE = Constants.Predictor.DELTA_DIST_SQ_MERGE;
            double VELOCITY_DT = Constants.Predictor.VELOCITY_DT;

            // flip coordinates in message if necessary
            if (Constants.Predictor.FLIP_COORDINATES)
            {
                VisionMessage newMessage = new VisionMessage(msg.CameraID);
                if (msg.Ball != null)
                    newMessage.Ball = new BallInfo(msg.Ball.Position.rotate(Math.PI));
                foreach (VisionMessage.RobotData robotInfo in msg.Robots)
                    newMessage.Robots.Add(new VisionMessage.RobotData(robotInfo.ID, robotInfo.Team,
                                                                        robotInfo.Position.rotate(Math.PI), Angle.AngleModTwoPi(robotInfo.Orientation + Math.PI)));
                msg = newMessage;
            }

            #region Update ball
            // Ball can time out for a single camera (such as bots)
            if (msg.Ball == null)
            {
                if (ball == null ||
                    (ball != null && (time - ball.LastSeen <= Constants.Predictor.MAX_SECONDS_TO_KEEP_INFO)))
                    // stop tracking ball
                    ball = null;
            }
            // If we see the ball for the first time, just record it
            else if (ball == null)
            {
                ball = new BallInfo(msg.Ball.Position, new Vector2(0, 0)); // Don't know velocity yet
                ballDtStart = time;
                ballAtDtStart = new BallInfo(ball);

                // We have just seen the ball                    
                ball.LastSeen = time;
            }
            // otherwise update ball state
            else
            {
                BallInfo newBall = msg.Ball;

                // Update position
                ball.Position = ball.Position.lerp(newBall.Position, WEIGHT_NEW);

                // Project velocity to compensate for delays that we are sure of
                ball.Position += ball.Velocity * msg.Delay;

                // Update velocity if a reasonable interval has passed
                double dt = time - ballDtStart;
                if (dt > VELOCITY_DT) // lower limit; avoid jitter
                {
                    Vector2 d = newBall.Position - ballAtDtStart.Position;
                    ball.Velocity = WEIGHT_OLD * ball.Velocity + WEIGHT_NEW * d / dt;

                    // Reset velocity interval
                    ballDtStart = time;
                    ballAtDtStart = new BallInfo(ball);
                }

                // We have just seen the ball                    
                ball.LastSeen = time;
            }
            #endregion

            #region Update robots
            foreach (RFC.Messaging.VisionMessage.RobotData newRobotData in msg.Robots)
            {
                RobotInfo newRobot = new RobotInfo(newRobotData.Position, new Vector2(0, 0), 0,
                                                    newRobotData.Orientation, newRobotData.Team, newRobotData.ID);

                // Keep track of nextID
                if (newRobot.ID > nextID[newRobot.Team])
                {
                    nextID[newRobot.Team] = newRobot.ID;
                }

                // Match with existing info either by ID (if vision gave it one) or by position
                Predicate<RobotInfo> matchByPosPredicate;
                matchByPosPredicate = new Predicate<RobotInfo>(delegate(RobotInfo robot)
                {
                    // TODO: Figure this out, temporarily forcing to look by ID
                    // distance close enough OR ID matches
                    return (robot.Position.distanceSq(newRobot.Position) < DELTA_DIST_SQ_MERGE ||
                            robot.ID == newRobot.ID);
                });

                // Find the matching robot
                int oldRobotIdx = -1;
                oldRobotIdx = robots[newRobot.Team].FindIndex(matchByPosPredicate);
                if (oldRobotIdx >= 0 && newRobot.ID >= 0 && newRobot.ID != robots[newRobot.Team][oldRobotIdx].ID)
                {
                    continue;
                }

                int newRobotIdx = -1;

                // If never seen this robot before, then add it; otherwise, update
                if (oldRobotIdx < 0)
                {
                    // On first frame don't know velocity                            
                    robots[newRobot.Team].Add(new RobotInfo(newRobot));
                    robotsAtDtStart[newRobot.Team].Add(new RobotInfo(newRobot));
                    velocityDtStart[newRobot.Team].Add(time);
                    newRobotIdx = robots[newRobot.Team].Count - 1;
                }
                else
                {
                    RobotInfo oldRobot = robots[newRobot.Team][oldRobotIdx];

                    // Update position and orientation
                    oldRobot.Position = oldRobot.Position.lerp(newRobot.Position, WEIGHT_NEW);
                    oldRobot.Orientation = WEIGHT_OLD * oldRobot.Orientation + WEIGHT_NEW * newRobot.Orientation;

                    // Project velocity to compensate for delays that we are sure of
                    oldRobot.Position += oldRobot.Velocity * msg.Delay;

                    // Update velocity if a reasonable interval has passed                    
                    double dt = time - velocityDtStart[newRobot.Team][oldRobotIdx];

                    if (dt > VELOCITY_DT)
                    {
                        Vector2 d = newRobot.Position - robotsAtDtStart[newRobot.Team][oldRobotIdx].Position;
                        oldRobot.Velocity = WEIGHT_OLD * oldRobot.Velocity + WEIGHT_NEW * d / dt;

                        double r = newRobot.Orientation - robotsAtDtStart[newRobot.Team][oldRobotIdx].Orientation;
                        oldRobot.AngularVelocity = WEIGHT_OLD * oldRobot.AngularVelocity + WEIGHT_NEW * r / dt;

                        // Reset velocity dt interval
                        velocityDtStart[newRobot.Team][oldRobotIdx] = time;
                        robotsAtDtStart[newRobot.Team][oldRobotIdx] = new RobotInfo(oldRobot);
                    }

                    newRobotIdx = oldRobotIdx;
                }

                // We have just seen this robot
                robots[newRobot.Team][newRobotIdx].LastSeen = time;
            }
            #endregion
        }

        public BallInfo GetBall()
        {
            BallInfo retBall;
            // Copy data for returning
            retBall = (ball != null) ? new BallInfo(ball) : null;
            // It's ok to return null if we don't know anything about the ball
            return retBall;
        }

        public List<RobotInfo> GetRobots(Team team)
        {
            List<RobotInfo> retRobots;
            double time = HighResTimer.SecondsSinceStart();

            // Reconsider our belief
            List<RobotInfo> tempRobots = new List<RobotInfo>(robots[team].Count);
            for (int i = 0; i < robots[team].Count; i++)
            {
                if (time - robots[team][i].LastSeen < Constants.Predictor.MAX_SECONDS_TO_KEEP_INFO)
                {
                    tempRobots.Add(robots[team][i]);
                }
            }
            robots[team].Clear();
            robots[team].AddRange(tempRobots);

            // Copy data for returning               
            retRobots = new List<RobotInfo>(robots[team].Count);
            foreach (RobotInfo robot in robots[team])
            {
                retRobots.Add(new RobotInfo(robot));
            }
            // It's ok to return an empty list if we see no robots
            return retRobots;
        }
    }
}
