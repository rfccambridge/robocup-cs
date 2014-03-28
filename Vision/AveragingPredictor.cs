using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using RFC.Core;
using RFC.Utilities;
using RFC.Geometry;
using RFC.Messaging;

namespace RFC.Vision
{
    /// <summary>
    /// A basic implementation of IPredictor that averages values from multiple cameras
    /// </summary>
    public class AveragingPredictor
    {
        const int NUM_CAMERAS = 2;

        private ServiceManager messenger;

        // Each camera believes in a state
        private FieldState[] fieldStates = new FieldState[NUM_CAMERAS];

        // Combined state (updated at a specified frequency)
        private BallInfo ball;
        private Dictionary<Team, List<RobotInfo>> robots = new Dictionary<Team, List<RobotInfo>>();

        private object listenerLock = new object();

        // For marking ball position
        private Vector2 markedPosition = null;

        private bool flipped;

        public AveragingPredictor(bool flipped)
        {
            this.flipped = flipped;

            for (int i = 0; i < NUM_CAMERAS; i++)
            {
                fieldStates[i] = new FieldState();
            }

            LoadConstants();
            messenger = ServiceManager.getServiceManager();
            new QueuedMessageHandler<VisionMessage>(Update, listenerLock);
            new QueuedMessageHandler<BallMarkMessage>(UpdateBallMark, listenerLock);
        }

        /// <summary>
        /// Creates a copy of the given RobotInfo, and flips the position, velocity, and orientation
        /// </summary>
        private RobotInfo flipRobotInfo(RobotInfo info)
        {
            return new RobotInfo(-info.Position, -info.Velocity, info.AngularVelocity,
                    info.Orientation + Math.PI, info.Team, info.ID);
        }

        public void Update(VisionMessage msg)
        {

            fieldStates[msg.CameraID].Update(msg);

            combineBall();
            foreach (Team team in Enum.GetValues(typeof(Team)))
                combineRobots(team);
            
            // preparing messages
            BallVisionMessage ball_msg = new BallVisionMessage(flipped ? new BallInfo(-ball.Position, -ball.Velocity) : ball);
            RobotVisionMessage robots_msg = new RobotVisionMessage(flipped ? getRobots(Team.Blue).ConvertAll(flipRobotInfo) : getRobots(Team.Blue), flipped ? getRobots(Team.Yellow).ConvertAll(flipRobotInfo) : getRobots(Team.Yellow));
            BallMovedMessage move_msg = new BallMovedMessage(hasBallMoved());
            FieldVisionMessage all_msg = new FieldVisionMessage(flipped ? getRobots(Team.Blue).ConvertAll(flipRobotInfo) : getRobots(Team.Blue), flipped ? getRobots(Team.Yellow).ConvertAll(flipRobotInfo) : getRobots(Team.Yellow), flipped ? new BallInfo(-ball.Position, -ball.Velocity) : ball);

            // sending message that new data is ready
            messenger.SendMessage<BallVisionMessage>(ball_msg);
            messenger.SendMessage<RobotVisionMessage>(robots_msg);
            messenger.SendMessage<FieldVisionMessage>(all_msg);

            if (move_msg.moved)
                messenger.SendMessage<BallMovedMessage>(move_msg);
        }

        public void UpdateBallMark(BallMarkMessage msg)
        {
            if (msg.action == BallMarkAction.Clear)
                clearBallMark();
            else if (msg.action == BallMarkAction.Set)
                setBallMark();
        }

        private void setBallMark()
        {
            BallInfo ball = getBall();
            if (ball == null)
            {
                //throw new ApplicationException("Cannot mark ball position because no ball is seen.");
                return;
            }
            markedPosition = ball != null ? new Vector2(ball.Position) : null;
        }

        private void clearBallMark()
        {
            markedPosition = null;
        }

        private bool hasBallMoved()
        {
            BallInfo ball = getBall();
            double BALL_MOVED_DIST = Constants.Plays.BALL_MOVED_DIST;
            bool ret = (ball != null && markedPosition == null) || (ball != null &&
                                                                    markedPosition.distanceSq(ball.Position) > BALL_MOVED_DIST * BALL_MOVED_DIST);
            return ret;
        }

        private void LoadConstants()
        {
            
        }

        private BallInfo getBall()
        {
            return ball;
        }

        private List<RobotInfo> getRobots(Team team)
        {
            return robots[team];
        }

        private BallInfo lastBall = null;
        // Return the average of info from all cameras weighed by the time since 
        // it was last updated
        private void combineBall()
        {
            double time = HighResTimer.SecondsSinceStart();

            // Return the average from all the cameras that see it weighted 
            // by the time since they last saw it                 

            Vector2 avgPosition = null;
            Vector2 avgVelocity = null;
            double avgLastSeen = 0;
            double sum = 0;
            for (int i = 0; i < fieldStates.Length; i++)
            {
                BallInfo ball = fieldStates[i].GetBall();
                if (ball != null)
                {
                    //double t = time - ball.LastSeen;
                    double t = 1;
                    // First time, we don't add, just initialize
                    if (avgPosition == null)
                    {
                        avgPosition = t * ball.Position;
                        avgVelocity = t * ball.Velocity;
                        avgLastSeen = t * ball.LastSeen;
                        sum = t;
                    }
                    avgPosition += t * ball.Position;
                    avgVelocity += t * ball.Velocity;
                    avgLastSeen += t * ball.LastSeen;
                    sum += t;
                }
            }

            BallInfo retBall = null;
            if (avgPosition != null) // if we saw at least one ball
            {
                avgPosition /= sum;
                avgVelocity /= sum;
                avgLastSeen /= sum;
                retBall = new BallInfo(avgPosition, avgVelocity, avgLastSeen);
                lastBall = new BallInfo(avgPosition, avgVelocity, avgLastSeen);
            }
            // Predict the latest position with zero velocity if all cameras have timed out
            else if (lastBall != null && (time - lastBall.LastSeen <= Constants.Predictor.MAX_SECONDS_TO_KEEP_BALL))
            {
                retBall = new BallInfo(lastBall.Position, Vector2.ZERO, lastBall.LastSeen);
            }

            this.ball = retBall;
        }

        // For each robot return the average of info from all cameras weighed by
        // the time since it was last updated
        private void combineRobots(Team team)
        {
            // Will store resulting list of robots
            List<RobotInfo> avgRobots = new List<RobotInfo>();

            // Infos from cameras    
            List<RobotInfo>[] fieldStateLists = new List<RobotInfo>[NUM_CAMERAS];

            // The outer list is has one entry per physical robot, each entry is a list made up of 
            // infos for that robot believed by different cameras. We later average over the inner list.
            // TODO: in the future, the parent predictor could keep it's own state so that paternless
            // ids stay stay steady
            List<List<RobotInfo>> robotSightings = new List<List<RobotInfo>>();

            // Technically, need to record acquisition time for each camera, but it's ok
            double time = HighResTimer.SecondsSinceStart();

            // For assigning IDs to unidentified robots
            int nextID = -1;

            // Acquire the info from all cameras (as concurrently as possible)
            for (int cameraID = 0; cameraID < NUM_CAMERAS; cameraID++)
            {
                fieldStateLists[cameraID] = fieldStates[cameraID].GetRobots(team);
            }

            // Construct the robot sightings list for later averaging
            for (int cameraID = 0; cameraID < NUM_CAMERAS; cameraID++)
            {
                // Iterate over robots seen by the camera
                foreach (RobotInfo fsRobot in fieldStateLists[cameraID])
                {

                    // Match with existing info either by ID (if vision gave it one) or by position
                    Predicate<RobotInfo> matchByIDPredicate, matchByPosPredicate;
                    matchByIDPredicate = new Predicate<RobotInfo>(delegate(RobotInfo robot)
                    {
                        return robot.ID == fsRobot.ID;
                    });
                    matchByPosPredicate = new Predicate<RobotInfo>(delegate(RobotInfo robot)
                    {
                        return robot.Position.distanceSq(fsRobot.Position) < Constants.Predictor.DELTA_DIST_SQ_MERGE;
                    });

                    // Find the matching robot: m*n search
                    int sightingsIdx;
                    bool doNotAdd = false;
                    for (sightingsIdx = 0; sightingsIdx < robotSightings.Count; sightingsIdx++)
                    {
                        int sIdx = -1;
                        sIdx = robotSightings[sightingsIdx].FindIndex(matchByIDPredicate);

                        // If position matches, but ID doesn't, the new robot is on top of one we already saw
                        // In this case, we ignore the new one completely (arbitrary choice -- old is not really 
                        // better than old)
                        if (sIdx >= 0 && fsRobot.ID >= 0 && fsRobot.ID != robotSightings[sightingsIdx][sIdx].ID)
                        {
                            doNotAdd = true;
                            continue;
                        }

                        // If at least one sighting was satisfactory, we merge with that list of sightings
                        if (sIdx >= 0)
                        {
                            break;
                        }
                    }

                    // Decided to ignore this robot because it was on top of another one
                    if (doNotAdd)
                    {
                        continue;
                    }

                    // If not yet seen anywhere else, add the robot; otherwise, add it to the 
                    // sightings for later averaging                    
                    if (sightingsIdx == robotSightings.Count)
                    {
                        List<RobotInfo> sList = new List<RobotInfo>();
                        sList.Add(fsRobot);
                        robotSightings.Add(sList);
                    }
                    else
                    {
                        robotSightings[sightingsIdx].Add(fsRobot);
                    }

                    // Keep track of maximum ID, so that we can assign IDs later
                    if (fsRobot.ID > nextID)
                    {
                        nextID = fsRobot.ID;
                    }
                }
            }

            // Now we have assembled information for each robot, just take the average
            foreach (List<RobotInfo> sList in robotSightings)
            {
                RobotInfo avgRobot = new RobotInfo(sList[0]);

                // Assign an ID if the robot came without one
                if (avgRobot.ID < 0)
                {
                    avgRobot.ID = ++nextID;
                }

                double t = time - avgRobot.LastSeen;
                t = 1;
                Vector2 avgPosition = t * (new Vector2(avgRobot.Position));
                Vector2 avgVelocity = t * (new Vector2(avgRobot.Velocity));
                double avgOrientation = t * avgRobot.Orientation;
                double avgAngVel = t * avgRobot.AngularVelocity;
                double avgLastSeen = t * avgRobot.LastSeen;
                double sum = t;
                for (int i = 1; i < sList.Count; i++)
                {
                    //t = time - sList[i].LastSeen;
                    t = 1;
                    avgPosition += t * sList[i].Position;
                    avgVelocity += t * sList[i].Velocity;
                    avgAngVel += t * sList[i].AngularVelocity;
                    avgOrientation += t * sList[i].Orientation;
                    avgLastSeen += t * sList[i].LastSeen;
                    sum += t;
                }
                avgPosition /= sum;
                avgVelocity /= sum;
                avgAngVel /= sum;
                avgOrientation /= sum;
                avgLastSeen /= sum;

                // Record result for returning
                avgRobots.Add(new RobotInfo(avgPosition, avgVelocity, avgAngVel, avgOrientation,
                                            team, avgRobot.ID, avgLastSeen));
            }

            robots[team] = avgRobots;
        }

    }

}
