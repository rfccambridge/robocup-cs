﻿using System;
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
    public class AveragingPredictor : IMessageHandler<VisionMessage>
    {
        const int NUM_CAMERAS = 2;

        private ServiceManager messenger;

        // Each camera believes in a state
        private FieldState[] fieldStates = new FieldState[NUM_CAMERAS];

        // Combined state (updated at a specified frequency)
        private BallInfo ball;
        private Dictionary<Team, List<RobotInfo>> robots = new Dictionary<Team, List<RobotInfo>>();

        private object listenerLock = new object();

        private JitterFilter filter;
        Random r;


        public bool Flipped { get; set; }

        public AveragingPredictor()
        {
            Flipped = false;

            // make new field state memory for each camera
            for (int i = 0; i < NUM_CAMERAS; i++)
            {
                fieldStates[i] = new FieldState();
            }

            LoadConstants();
            messenger = ServiceManager.getServiceManager();
            // register handler to receive messages
            messenger.RegisterListener(this.Queued<VisionMessage>(listenerLock));

            filter = new JitterFilter();
            r = new Random();
        }

        /// <summary>
        /// Creates a copy of the given RobotInfo, and flips the position, velocity, and orientation
        /// </summary>
        private RobotInfo flipRobotInfo(RobotInfo info)
        {
            return new RobotInfo(
                Point2.ORIGIN - (info.Position - Point2.ORIGIN), -info.Velocity, info.AngularVelocity,
                    info.Orientation + Math.PI, info.Team, info.ID);
        }

        public void HandleMessage(VisionMessage message)
        {
            Update(message);
        }
        // update robot positions with new vision data
        public void Update(VisionMessage msg)
        {
            // update each FieldState with new vision data
            fieldStates[msg.CameraID].Update(msg);

            // calculates ball and robot positions, averaged over cameras
            combineBall();
            foreach (Team team in Enum.GetValues(typeof(Team)))
                combineRobots(team);
            
            // preparing messages
            if (ball != null)
            {
                if (Flipped) ball = new BallInfo(Point2.ORIGIN - (ball.Position - Point2.ORIGIN), -ball.Velocity);
                BallVisionMessage ball_msg = new BallVisionMessage(ball);
                messenger.SendMessage<BallVisionMessage>(ball_msg);
            }
            else
            {
                ball = new BallInfo(Point2.ORIGIN); // todo: mark that this is null
            }

            RobotVisionMessage robots_msg = new RobotVisionMessage(Flipped ? getRobots(Team.Blue).ConvertAll(flipRobotInfo) : getRobots(Team.Blue), Flipped ? getRobots(Team.Yellow).ConvertAll(flipRobotInfo) : getRobots(Team.Yellow));
            FieldVisionMessage all_msg = new FieldVisionMessage(Flipped ? getRobots(Team.Blue).ConvertAll(flipRobotInfo) : getRobots(Team.Blue), Flipped ? getRobots(Team.Yellow).ConvertAll(flipRobotInfo) : getRobots(Team.Yellow), ball);

            // sending message containing new data
            filter.Update(all_msg);
            //messenger.SendMessage(robots_msg);
            //messenger.SendMessage(all_msg);

                
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
        // combineBall() called for each frame of the game
        private void combineBall()
        {
            double time = HighResTimer.SecondsSinceStart();

            // Return the average from all the cameras that see it weighted 
            // by the time since they last saw it                 

            Vector2 avgPosFromOrigin = null;
            Vector2 avgVelocity = null;
            double avgLastSeen = 0;
            double sum = 0;
            double closestBallDistance = Double.MaxValue;
            BallInfo closestBall = null;
            //go through all the fieldStates (one for each camera)
            for (int i = 0; i < fieldStates.Length; i++)
            {
                BallInfo ball = fieldStates[i].GetBall();
                // if there is a ball
                if (ball != null)
                {
                    //double t = time - ball.LastSeen;
                    double t = 1;
                    // First time, we don't add, just initialize
                    // if there wasn't a ball beforehand or if the last ball is within a certain distance of the new one
                    if (lastBall == null || (ball.Position - lastBall.Position).magnitude() < closestBallDistance)
                    {
                        //resetting the average position to the latest position: we trust it's true
                        avgPosFromOrigin = t * (ball.Position - Point2.ORIGIN);
                        avgVelocity = t * ball.Velocity;
                        avgLastSeen = t * ball.LastSeen;
                        // set sum to t (here, 1)
                        sum = t;
                        // set the closest ball to the ball from the given field state
                        closestBall = ball;
                        // if there is a last ball (previous if statement means it's within some distance of previous ball)
                        // Q: doesn't this just make it closestBallDistance smaller each time
                        if (lastBall != null)
                        {
                            closestBallDistance = (ball.Position - lastBall.Position).magnitude();
                        }
                    }
                    /*
                    avgPosition += t * ball.Position;
                    avgVelocity += t * ball.Velocity;
                    avgLastSeen += t * ball.LastSeen;
                    sum += t;*/
                }
            }
            //Now we have information from all the cameras. Let's return a new ball
            BallInfo retBall = null;
            if (closestBall != null && avgPosFromOrigin != null) // if we saw at least one ball
            {
                // TODO: how is this not just dividing by 1?
                avgPosFromOrigin /= sum;;
                avgVelocity /= sum;
                avgLastSeen /= sum;
                retBall = new BallInfo(Point2.ORIGIN + avgPosFromOrigin, avgVelocity, avgLastSeen);
                lastBall = new BallInfo(Point2.ORIGIN + avgPosFromOrigin, avgVelocity, avgLastSeen);
            }
            
            // Predict the latest position with zero velocity if all cameras have timed out
            else if (lastBall != null && (time - lastBall.LastSeen <= Constants.Predictor.MAX_SECONDS_TO_KEEP_BALL))
            {
                retBall = new BallInfo(lastBall.Position, Vector2.ZERO, lastBall.LastSeen);
            }
            // defines what comes out
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
            // TODO: in the future, the parent predictor could keep its own state so that patternless
            // ids stay steady
            // ?
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
            // loop over the cameras
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
            //Q: avg over cameras or over many frames per camera?
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
                Vector2 avgPosition = t * (avgRobot.Position - Point2.ORIGIN);
                Vector2 avgVelocity = t * avgRobot.Velocity;
                double avgOrientation = t * avgRobot.Orientation;
                double avgAngVel = t * avgRobot.AngularVelocity;
                double avgLastSeen = t * avgRobot.LastSeen;
                double sum = t;
                for (int i = 1; i < sList.Count; i++)
                {
                    //t = time - sList[i].LastSeen;
                    // TODO: This should be modified to not just do the average over many robots.
                    //COMBINE with jitter filter
                    t = 1;
                    avgPosition += t * (sList[i].Position - Point2.ORIGIN);
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
                avgRobots.Add(new RobotInfo(Point2.ORIGIN + avgPosition, avgVelocity, avgAngVel, avgOrientation,
                                            team, avgRobot.ID, avgLastSeen));
            }

            robots[team] = avgRobots;
        }

    }

}
