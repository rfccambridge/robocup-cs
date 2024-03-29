﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RFC.Geometry;
using RFC.Core;
using RFC.Utilities;
using RFC.Messaging;

namespace RFC.PathPlanning
{
    public class SmoothRRTPlanner : IMessageHandler<RobotDestinationMessage>
    {
        /// <summary>
        /// Used to specify whether or how the motion planner should stay out of the defense region around a goal.
        /// </summary>
        public enum DefenseAreaAvoid { NONE, NORMAL, FULL };


        //Constants-------------------------------------
        double TIME_STEP; //Time step in secs, with velocity determines RRT extension length
        double ROBOT_VELOCITY; //Assume our robot can move this fast
        double MAX_ACCEL_PER_STEP; //And that it can accelerate this fast.
        double MAX_OBSERVABLE_VELOCITY; //Pretend that the current robot is moving at most this fast

        //RRT parameters
        int MAX_TREE_SIZE; //Max nodes in tree before we give up
        int MAX_PATH_TRIES; //Max number of attempts to extend paths before we give up
        double CLOSE_ENOUGH_TO_GOAL; //We're completely done when we get this close to the goal.
        double DIST_FOR_SUCCESS; //We're done for now when we get this much closer to the goal than we are

        double DODGE_OBS_DIST;

        //Path extension
        double EXTRA_EXTENSION_ROTATE_ANGLE;

        //Motion extrapolation
        double ROBOT_MAX_TIME_EXTRAPOLATED; //Extrapolate other robots' movements up to this amount of seconds.

        //Collisions
        double ROBOT_RADIUS;      //Radius of single robot
        double ROBOT_AVOID_DIST;  //Avoid robot distance
        double ROBOT_FAR_AVOID_DIST; //Avoid robot distance when not close to goal
        double ROBOT_FAR_DIST; //What is considered "far", for the purposes of robot avoidance? 

        //Scoring
        double DIST_FROM_GOAL_SCORE; //Penalty per m of distance from goal.
        double EXCESS_LEN_SCORE; //Penalty for each m of path length >= the straightline distance
        double PER_BEND_SCORE; //Bonus/Penalty per bend in the path based on bend sharpness
        double VELOCITY_AGREEMENT_SCORE; //Bonus for agreeing with current velocity, per m/s velocity
        double OLDPATH_AGREEMENT_SCORE; //Bonus for agreeing with the old path, per m/s velocity
        double OLDPATH_AGREEMENT_DIST; //Score nothing for points that differ by this many meters from the old path
        double MIN_WEIGHT_VEL = 0.1; // min vel for weight; if less than this velocity, take velocity equal to it

        Rectangle LEGAL_RECTANGLE; //The basic rectangle determining what points in the field are legal to move to

        int NUM_PATHS_TO_SCORE; //How many paths do we generate and score?

        int goalie_id;

        public void ReloadConstants()
        {
            TIME_STEP = ConstantsRaw.get<double>("motionplanning", "SRRT_TIME_STEP");
            ROBOT_VELOCITY = ConstantsRaw.get<double>("motionplanning", "SRRT_ROBOT_VELOCITY");
            MAX_ACCEL_PER_STEP = ConstantsRaw.get<double>("motionplanning", "SRRT_MAX_ACCEL_PER_STEP");
            MAX_OBSERVABLE_VELOCITY = ConstantsRaw.get<double>("motionplanning", "SRRT_MAX_OBSERVABLE_VELOCITY");

            MAX_TREE_SIZE = ConstantsRaw.get<int>("motionplanning", "SRRT_MAX_TREE_SIZE");
            MAX_PATH_TRIES = ConstantsRaw.get<int>("motionplanning", "SRRT_MAX_PATH_TRIES");

            CLOSE_ENOUGH_TO_GOAL = ConstantsRaw.get<double>("motionplanning", "SRRT_CLOSE_ENOUGH_TO_GOAL");
            DIST_FOR_SUCCESS = ConstantsRaw.get<double>("motionplanning", "SRRT_DIST_FOR_SUCCESS");
            DODGE_OBS_DIST = ConstantsRaw.get<double>("motionplanning", "SRRT_DODGE_OBS_DIST");
            EXTRA_EXTENSION_ROTATE_ANGLE = (ConstantsRaw.get<double>("motionplanning", "SRRT_EXTRA_EXTENSION_ROTATE_ANGLE") * Math.PI / 180.0);
            ROBOT_MAX_TIME_EXTRAPOLATED = ConstantsRaw.get<double>("motionplanning", "SRRT_ROBOT_MAX_TIME_EXTRAPOLATED");

            ROBOT_RADIUS = Constants.Basic.ROBOT_RADIUS;
            ROBOT_AVOID_DIST = ConstantsRaw.get<double>("motionplanning", "SRRT_ROBOT_AVOID_DIST");
            ROBOT_FAR_AVOID_DIST = ConstantsRaw.get<double>("motionplanning", "SRRT_ROBOT_FAR_AVOID_DIST");
            ROBOT_FAR_DIST = ConstantsRaw.get<double>("motionplanning", "SRRT_ROBOT_FAR_DIST");

            DIST_FROM_GOAL_SCORE = ConstantsRaw.get<double>("motionplanning", "SRRT_DIST_FROM_GOAL_SCORE");
            EXCESS_LEN_SCORE = ConstantsRaw.get<double>("motionplanning", "SRRT_EXCESS_LEN_SCORE");
            PER_BEND_SCORE = ConstantsRaw.get<double>("motionplanning", "SRRT_PER_BEND_SCORE");
            VELOCITY_AGREEMENT_SCORE = ConstantsRaw.get<double>("motionplanning", "SRRT_VELOCITY_AGREEMENT_SCORE");
            OLDPATH_AGREEMENT_SCORE = ConstantsRaw.get<double>("motionplanning", "SRRT_OLDPATH_AGREEMENT_SCORE");
            OLDPATH_AGREEMENT_DIST = ConstantsRaw.get<double>("motionplanning", "SRRT_OLDPATH_AGREEMENT_DIST");

            NUM_PATHS_TO_SCORE = ConstantsRaw.get<int>("motionplanning", "SRRT_NUM_PATHS_TO_SCORE");

            LEGAL_RECTANGLE = ExpandRectangle(Constants.FieldPts.EXTENDED_FIELD_RECT, -ROBOT_RADIUS);
        }

        //One node in the tree
        private class RRTNode
        {
            public RobotInfo info;
            public RRTNode parent;
            public double time;

            public RRTNode(RobotInfo info, RRTNode parent, double dt)
            {
                this.info = info;
                this.parent = parent;
                this.time = (parent == null ? 0 : parent.time) + dt;
            }
        }


        private bool includeCurStateInPath;

        private RobotPath[] _last_successful_path;
        private Object[] _path_locks;

        private ServiceManager msngr;

        public SmoothRRTPlanner(bool includeCurStateInPath, int numberRobots, int goalie_id)
        {
            this.includeCurStateInPath = includeCurStateInPath;
            this.goalie_id = goalie_id;
            ReloadConstants();

            _last_successful_path = new RobotPath[numberRobots];
            _path_locks = new object[numberRobots];

            for (int i = 0; i < numberRobots; i++)
            {
                _path_locks[i] = new object();
                _last_successful_path[i] = new RobotPath();
            }

            msngr = ServiceManager.getServiceManager();
            //msngr.RegisterListener<RobotDestinationMessage>(handleRobotDestinationMessage, new object());
            //new QueuedMessageHandler<RobotDestinationMessage>(handleRobotDestinationMessage, new object());
            msngr.RegisterListener<RobotDestinationMessage>(
                new MultiChannelQueuedMessageHandler<RobotDestinationMessage, int>(
                    this, (message) => message.Destination.ID, new object()
                )
            );
        }

        public void HandleMessage(RobotDestinationMessage message)
        {
            msngr.db("handling destination message");
            if (message.Destination == null || double.IsNaN(message.Destination.Position.X) || double.IsNaN(message.Destination.Position.Y))
            {
                msngr.db("invalid destination");
                return;
            }

            int id = message.Destination.ID;

            double avoidBallDist = (message.AvoidBall ? Constants.Motion.BALL_AVOID_DIST : 0f);
            RobotPath oldPath;
            lock (_path_locks[id])
            {
                oldPath = _last_successful_path[id];
            }

            // making sure destination is valid
            // approximating the defense areas as two circles of radius .8m
            Vector2 defense_offset = new Vector2(0, .35 / 2);
            double defense_radius = .7 + Constants.Basic.ROBOT_RADIUS; //changed to .9 to avoid robot getting too close to half circle
            Point2 LT = Constants.FieldPts.OUR_GOAL + defense_offset;
            Point2 LB = Constants.FieldPts.OUR_GOAL - defense_offset;
            Point2 RT = Constants.FieldPts.THEIR_GOAL + defense_offset;
            Point2 RB = Constants.FieldPts.THEIR_GOAL - defense_offset;

            

            // avoiding
            RobotInfo destinationCopy = new RobotInfo(message.Destination);

            // considers 9 arguments instead of 4 to consider both half circles, to revert change definition 
            // in Avoider.cs
            if (message.Destination.ID != goalie_id)
                destinationCopy = Avoider.avoid(destinationCopy, LT, defense_radius * 1.25, LB, defense_radius * 1.25,
                    RT, defense_radius * 1.25, RB, defense_radius * 1.25);

            destinationCopy.Team = message.Destination.Team;
            destinationCopy.ID = id;


            //Plan a path
            RobotPath newPath;

            try
            {
                DefenseAreaAvoid leftAvoid = (message.Destination.ID != goalie_id) ? DefenseAreaAvoid.NONE : DefenseAreaAvoid.FULL;
                RefboxStateMessage refMessage = ServiceManager.getServiceManager().GetLastMessage<RefboxStateMessage>();
                PlayType[] types = new PlayType[4];
                types[0] = PlayType.Direct_Ours;
                types[1] = PlayType.Direct_Theirs;
                types[2] = PlayType.Indirect_Ours;
                types[3] = PlayType.Indirect_Theirs;
                DefenseAreaAvoid rightAvoid = DefenseAreaAvoid.NONE;//(refMessage == null || types.Contains(refMessage.PlayType)) ? DefenseAreaAvoid.FULL : DefenseAreaAvoid.NONE;
                

                // debug info
                newPath = GetPath(destinationCopy, avoidBallDist, oldPath,
                    leftAvoid, rightAvoid);
                // if path is empty, don't move, else make sure path contains desired state
                if (!newPath.isEmpty())
                {
                    newPath.Waypoints.Add(destinationCopy);
                    newPath.setFinalState(destinationCopy);
                }
            }
            catch (Exception e)
            {
                msngr.db("PlanMotion failed. Dumping exception:\n" + e.ToString());
                return;
            }

            lock (_path_locks[id])
            {
                if (newPath != null)
                    _last_successful_path[id] = newPath;
                newPath.Slow = message.Slow;
            }

            /*
            #region Drawing
            if (_fieldDrawer != null)
            {
                //Path commited for following
                if (DRAW_PATH)
                    _fieldDrawer.DrawPath(newPath);
                //Arrow showing final destination
                _fieldDrawer.DrawArrow(_team, id, ArrowType.Destination, destination.Position);
            }
            #endregion
            */
            msngr.SendMessage(new RobotPathMessage(newPath));
        }

        //Return a random point biased in a certain distribution between the current and the desired location,
        //given the closest point the RRT has found so far to the desired location
        private Point2 GetRandomPoint(Point2 desiredLoc, Point2 currentLoc, double closestSoFar)
        {
            double factor = closestSoFar / 4.0 + 0.18;
            double rand1 = RandGen.NextGaussian();
            double rand2 = RandGen.NextGaussian();

            double fullDist = desiredLoc.distance(currentLoc);
            if (fullDist <= 1e-6)
            {
                return new Point2(desiredLoc.X + rand1 * factor, desiredLoc.Y + rand2 * factor);
            }

            double prop = closestSoFar / fullDist;
            prop *= 0.95;
            double centerX = desiredLoc.X * (1 - prop) + currentLoc.X * prop;
            double centerY = desiredLoc.Y * (1 - prop) + currentLoc.Y * prop;
            return new Point2(centerX + rand1 * factor, centerY + rand2 * factor);
        }

        //Does the extension in the direction of the given unit ray from p intersect the given obstacle?
        //Yes, if it geometrically intersects AND the direction is TOWARDS the obstacle
        private bool IntersectsObstacle(Point2 p, Point2 obsPos, double obsRadius, Vector2 rayUnit)
        {
            return rayUnit * (obsPos - p) >= 0 && obsPos.distanceSq(p) < obsRadius * obsRadius;
        }

        //Get the distance at which we should avoid other robots, given our current distance
        //from the goal.
        private double GetEffectiveRobotAvoidDist(double goalDist)
        {
            double avoidProp = (goalDist - ROBOT_AVOID_DIST) / (ROBOT_FAR_DIST - ROBOT_AVOID_DIST);
            if (avoidProp < 0) avoidProp = 0;
            if (avoidProp > 1) avoidProp = 1;
            double avoidDist = ROBOT_AVOID_DIST + avoidProp * (ROBOT_FAR_AVOID_DIST - ROBOT_AVOID_DIST);
            return avoidDist;
        }

        //Does the extension along the given ray from p intersect the given obstacle?
        //Yes, if it geometrically intersects AND the direction is TOWARDS the obstacle
        //Tests both the segment and the endpoints
        private bool IntersectsObstacle(Point2 src, Point2 dest, Vector2 rayUnit, double rayLen, Point2 obsPos, double obsRadius)
        {
            //We consider a step okay if it moves away from the obstacle, despite intersecting right now.
            if (rayUnit * (obsPos - src) <= 0)
                return false;

            //Test endpoints
            if (obsPos.distanceSq(src) < obsRadius * obsRadius)
                return true;
            if (obsPos.distanceSq(dest) < obsRadius * obsRadius)
                return true;

            //See if it intersects in the middle...
            Vector2 srcToObs = obsPos - src;
            double parallelDist = srcToObs * rayUnit;
            if (parallelDist <= 0 || parallelDist >= rayLen)
                return false;

            double perpDist = Math.Abs(Vector2.cross(srcToObs, rayUnit));
            if (perpDist < obsRadius)
                return true;

            return false;
        }

        //Get the future position of a robot, extrapolating based on its velocity
        private Point2 GetFuturePos(Point2 obsPos, Vector2 obsVel, double time)
        {
            time = Math.Max(time, ROBOT_MAX_TIME_EXTRAPOLATED);
            return obsPos + obsVel * time;
        }

        //Check if the given extension by nextSegment would be allowed by all the obstacles
        private bool IsAllowedByObstacles(RobotInfo currentState, Point2 src, Vector2 nextSegment, double curTime,
            BallInfo ball, List<RobotInfo> robots, double avoidBallRadius, Point2 goal, List<IGeom> obstacles)
        {
            Point2 dest = src + nextSegment;
            Vector2 ray = nextSegment;
            if (ray.magnitudeSq() < 1e-16)
                return true;

            double goalDist = dest.distance(goal);
            double robotAvoidDist = GetEffectiveRobotAvoidDist(goalDist);

            Vector2 rayUnit = ray.normalizeToLength(1.0);
            double rayLen = ray.magnitude();

            //Test if destination is in bounds
            if (!LEGAL_RECTANGLE.contains(dest))
            {
                //If not in bounds, then we MUST be moving back inward to be legal
                if (dest.X >= LEGAL_RECTANGLE.XMax && rayUnit.X > 0)
                    return false;
                if (dest.X <= LEGAL_RECTANGLE.XMin && rayUnit.X < 0)
                    return false;
                if (dest.Y >= LEGAL_RECTANGLE.YMax && rayUnit.Y > 0)
                    return false;
                if (dest.Y <= LEGAL_RECTANGLE.YMin && rayUnit.Y < 0)
                    return false;
            }

            //Time to extrapolate forth
            double time = Math.Max(curTime, ROBOT_MAX_TIME_EXTRAPOLATED);

            //Avoid all robots, except myself
            int count = robots.Count;
            for (int i = 0; i < count; i++)
            {
                RobotInfo info = robots[i];
                if (info.Team != currentState.Team || info.ID != currentState.ID)
                {
                    //Extrapolate the robot's position into the future.
                    Point2 obsPos = GetFuturePos(info.Position, info.Velocity, curTime);

                    if (IntersectsObstacle(src, dest, rayUnit, rayLen, obsPos, robotAvoidDist))
                    { return false; }

                    //It's also bad if the obstacle would collide with us next turn, by virtue of moving...
                    //But it's still okay if we're moving away from it.
                    Point2 obsPosNext = obsPos + info.Velocity * TIME_STEP;
                    if (IntersectsObstacle(dest, obsPosNext, robotAvoidDist, rayUnit))
                    { return false; }
                }
            }

            //If needed, avoid ball
            if (avoidBallRadius > 0 &&
                ball != null &&
                ball.Position != null)
            {
                if (IntersectsObstacle(src, dest, rayUnit, rayLen, ball.Position, avoidBallRadius))
                { return false; }
            }

            //Test destination against all other obstacles
            LineSegment seg = new LineSegment(src, dest);
            foreach (IGeom g in obstacles)
            {
                if (g is Circle)
                {
                    Circle c = g as Circle;
                    if ((src - c.Center) * rayUnit <= 0 && GeomFuncs.intersects(seg, c))
                        return false;
                }
                else if (g is Rectangle)
                {
                    Rectangle r = g as Rectangle;
                    if (r.contains(src))
                    {
                        if (r.ShortestDirectionOut(src) * rayUnit <= 0)
                            return false;
                    }
                    else
                    {
                        if (GeomFuncs.intersects(seg, r))
                            return false;
                    }
                }
                else
                    throw new Exception("Smooth RRT only supports circles and rectangles currently!");
            }

            return true;
        }

        //Get the adjusted target direction using a tangentbug-like algorithm
        private Vector2 GetAdjustedTargetDir(Point2 cur, Vector2 targetDir, Point2 obsPos, Vector2 obsVel, double time, double avoidDist, double targetDist)
        {
            double obsDist = obsPos.distance(cur);
            time += (obsDist / ROBOT_VELOCITY);
            obsPos = GetFuturePos(obsPos, obsVel, time);
            Vector2 relObsPos = obsPos - cur;

            double parallelDist = targetDir * relObsPos;
            if (parallelDist <= 0 || parallelDist >= targetDist)
                return null;

            double perpDist = Vector2.cross(relObsPos, targetDir);
            if (perpDist >= 0 && perpDist < avoidDist)
            {
                Vector2 newTargetDir = relObsPos.parallelComponent(targetDir) + targetDir.rotatePerpendicular() * (avoidDist - perpDist) * 1.2;
                return newTargetDir.normalize();
            }
            else if (perpDist < 0 && perpDist > -avoidDist)
            {
                Vector2 newTargetDir = relObsPos.parallelComponent(targetDir) - targetDir.rotatePerpendicular() * (avoidDist + perpDist) * 1.2;
                return newTargetDir.normalize();
            }
            return null;
        }

        //Get the extended point, using the acceleration model
        private Vector2 GetAcceleratedExtension(RRTNode node, Point2 target, BallInfo ball, List<RobotInfo> robots, Point2 goal, double ballAvoidRadius, bool adjust)
        {
            Vector2 targetDir = target - node.info.Position;
            double magnitude = targetDir.magnitude();
            if (magnitude < 1e-6)
                return targetDir;

            //Is it maybe possible that we could reach goal?
            if (magnitude <= node.info.Velocity.magnitude() + MAX_ACCEL_PER_STEP * TIME_STEP)
            {
                //Check if we can get there right away anywhere in our acceleration circle or the cone reaching to it.
                Point2 circleCenter = node.info.Position + node.info.Velocity * TIME_STEP;
                double circleRadius = MAX_ACCEL_PER_STEP * 2 * TIME_STEP;

                Vector2 toCircle = (circleCenter - node.info.Position);
                Vector2 closestOffsetTowardsCenter = toCircle.perpendicularComponent(targetDir);
                double closestDistToCenter = closestOffsetTowardsCenter.magnitude();
                if (closestDistToCenter < circleRadius)
                {
                    Point2 closestApproachToCenter = circleCenter - closestOffsetTowardsCenter;
                    double halfChordLength = Math.Sqrt(circleRadius * circleRadius - closestDistToCenter * closestDistToCenter);
                    double distToClosestApproach = (toCircle * targetDir >= 0) ?
                        (closestApproachToCenter - node.info.Position).magnitude() :
                        -(closestApproachToCenter - node.info.Position).magnitude();
                    double exitDist = distToClosestApproach + halfChordLength;
                    if (magnitude < exitDist)
                    {
                        return targetDir;
                    }
                }
            }

            //Okay, just extend then
            //Get the vector from us to the target
            targetDir /= magnitude;

            if (adjust)
            {
                double avoidDist = GetEffectiveRobotAvoidDist(node.info.Position.distance(goal));

                //Extend taking obstacles into account, move the target if there's an obstacle in the way
                double closestObsDistSq = 100000000;
                Vector2 adjustedTargetDir = targetDir;
                for (int i = 0; i < robots.Count; i++)
                {
                    RobotInfo info = robots[i];
                    if (info == null || info.Position == null || info.Velocity == null)
                        continue;
                    if (info.Team == node.info.Team && info.ID == node.info.ID)
                        continue;

                    double distSq = node.info.Position.distanceSq(info.Position);
                    if (DODGE_OBS_DIST <= 0 || distSq > DODGE_OBS_DIST * DODGE_OBS_DIST || distSq > closestObsDistSq)
                        continue;

                    Vector2 adj = GetAdjustedTargetDir(node.info.Position, targetDir, info.Position, info.Velocity, node.time, avoidDist, magnitude);
                    if (adj == null)
                        continue;
                    adjustedTargetDir = adj;
                    closestObsDistSq = distSq;
                }

                while (ball != null && ballAvoidRadius > 0)
                {
                    double distSq = node.info.Position.distanceSq(ball.Position);
                    if (distSq > DIST_FOR_SUCCESS * DIST_FOR_SUCCESS || distSq > closestObsDistSq)
                        break;

                    Vector2 adj = GetAdjustedTargetDir(node.info.Position, targetDir, ball.Position, ball.Velocity, node.time, ballAvoidRadius, magnitude);
                    if (adj == null)
                        break;
                    adjustedTargetDir = adj;
                    closestObsDistSq = distSq;
                    break;
                }

                targetDir = adjustedTargetDir;
            }
            Vector2 newVel = node.info.Velocity + targetDir * MAX_ACCEL_PER_STEP;
            double newVelMag = newVel.magnitude();
            if (newVelMag > ROBOT_VELOCITY)
            {
                newVel = newVel.normalizeToLength(ROBOT_VELOCITY);
                newVelMag = ROBOT_VELOCITY;
            }

            //Small hack - rotate the newVel towards the targetDir a little bit
            if (newVelMag > 1e-6)
            {
                double angleDiff = Angle.AngleDifference(newVel.cartesianAngle(), targetDir.cartesianAngle());
                if (angleDiff <= EXTRA_EXTENSION_ROTATE_ANGLE && angleDiff >= -EXTRA_EXTENSION_ROTATE_ANGLE)
                    newVel = newVel.rotate(angleDiff);
                else if (angleDiff < -EXTRA_EXTENSION_ROTATE_ANGLE)
                    newVel = newVel.rotate(-EXTRA_EXTENSION_ROTATE_ANGLE);
                else
                    newVel = newVel.rotate(EXTRA_EXTENSION_ROTATE_ANGLE);
            }

            return newVel * TIME_STEP;
        }

        //Check if extending according to nextSegment (the position offset vector) would hit obstacles.
        private RRTNode TryVsObstacles(RobotInfo currentState, RRTNode node, TwoDTreeMap<RRTNode> map, Vector2 nextSegment,
            BallInfo ball, List<RobotInfo> robots, double avoidBallRadius, Point2 goal, List<IGeom> obstacles)
        {
            Vector2 nextVel = nextSegment.normalizeToLength(ROBOT_VELOCITY);

            if (!IsAllowedByObstacles(currentState, node.info.Position, nextSegment, node.time, ball,
                robots, avoidBallRadius, goal, obstacles))
                return null;

            RobotInfo newInfo = new RobotInfo(
                node.info.Position + nextSegment,
                nextVel,
                0, 0, currentState.Team, currentState.ID);

            RRTNode newNode = new RRTNode(newInfo, node, nextSegment.magnitude() / ROBOT_VELOCITY);
            map.Add(newInfo.Position, newNode);
            return newNode;
        }

        //Extract the full successful path to node
        List<Point2> GetPathFrom(RRTNode node)
        {
            List<Point2> list = new List<Point2>();
            while (node != null)
            {
                list.Add(node.info.Position);
                node = node.parent;
            }

            list.Reverse();
            return list;
        }

        //Get a path!
        private List<Point2> GetPathTo(RobotInfo currentState, Point2 desiredPosition, List<RobotInfo> robots,
            BallInfo ball, double avoidBallRadius, List<IGeom> obstacles)
        {
            double mapXMin = Math.Min(currentState.Position.X, desiredPosition.X) - 0.3;
            double mapYMin = Math.Min(currentState.Position.Y, desiredPosition.Y) - 0.3;
            double mapXMax = Math.Max(currentState.Position.X, desiredPosition.X) + 0.3;
            double mapYMax = Math.Max(currentState.Position.Y, desiredPosition.Y) + 0.3;

            TwoDTreeMap<RRTNode> map = new TwoDTreeMap<RRTNode>(mapXMin, mapXMax, mapYMin, mapYMax);

            RRTNode startNode = new RRTNode(currentState, null, 0);
            map.Add(currentState.Position, startNode);

            RRTNode successNode = null;

            RRTNode activeNode = startNode;
            Point2 currentTarget = desiredPosition;
            int stepsLeft = 1000;
            double closestSoFar = 1000;
            double closeEnoughToGoal = (desiredPosition - currentState.Position).magnitude() - DIST_FOR_SUCCESS;
            if (closeEnoughToGoal < CLOSE_ENOUGH_TO_GOAL)
                closeEnoughToGoal = CLOSE_ENOUGH_TO_GOAL;

            bool doRandomAgain = false;
            bool tryAgain = false;

            int tries = 0;
            //Repeat while tree not too large
            while (map.Size() < MAX_TREE_SIZE)
            {
                //Do we stop the current pursuit yet?
                if (tryAgain || (--stepsLeft <= 0))
                {
                    tries++;
                    if (tries >= MAX_PATH_TRIES)
                        break;

                    //Go to the goal directly
                    if (!doRandomAgain && currentTarget != desiredPosition)
                    {
                        msngr.db("to goal directly");
                        doRandomAgain = true;
                        currentTarget = desiredPosition;
                        stepsLeft = 1000;
                        tryAgain = false;
                    }
                    //Go randomly
                    else
                    {
                        msngr.db("to goal randomly");

                        doRandomAgain = true;
                        currentTarget = GetRandomPoint(desiredPosition, currentState.Position, closestSoFar);
                        activeNode = map.NearestNeighbor(currentTarget).Item2;
                        stepsLeft = 1;
                        tryAgain = false;
                    }

                }

                //If we're close enough to the goal, we're done!
                if (activeNode.info.Position.distanceSq(desiredPosition) < closeEnoughToGoal * closeEnoughToGoal)
                {
                    msngr.db("SUCCESS");
                    successNode = activeNode;
                    break;
                }

                //Try to generate an extension to our target
                Vector2 segment = GetAcceleratedExtension(activeNode, currentTarget, ball, robots, desiredPosition,
                    avoidBallRadius, currentTarget == desiredPosition);
                if (segment == null)
                {
                    msngr.db("new segment didn't work");
                    tryAgain = true;
                    continue;
                }

                //Make sure the extension doesn't hit obstacles
                RRTNode newNode = TryVsObstacles(currentState, activeNode, map, segment, ball, robots,
                    avoidBallRadius, desiredPosition, obstacles);
                if (newNode == null)
                {
                    msngr.db("DIDN'T WORK");
                    tryAgain = true;
                }
                else
                {
                    msngr.db("DID WORK");
                    //Make sure that we haven't already crossed past our target
                    if ((newNode.info.Position - activeNode.info.Position) * (currentTarget - newNode.info.Position) <= 0)
                    {
                        msngr.db("but overshot");
                        tryAgain = true;
                    }

                    //Update closestSoFar
                    double dist = newNode.info.Position.distance(desiredPosition);
                    if (dist < closestSoFar)
                        closestSoFar = dist;

                    //Continue the RRT! Back up to the top now!
                    activeNode = newNode;
                    doRandomAgain = false;
                }

            }

            //If we didn't succeed, take the closest node anyways
            if (!(map.Size() < MAX_TREE_SIZE && tries < MAX_PATH_TRIES))
            {
                successNode = map.NearestNeighbor(desiredPosition).Item2;
                List<Point2> path = GetPathFrom(successNode);
                return path;
            }
            else
            {
                //If we did succeed, take the succeeding node and tack on the
                //desired state if it's not there (such as because we got close enough and stopped early).
                List<Point2> path = GetPathFrom(successNode);
                if (path.Count > 0 && path[path.Count - 1].distance(desiredPosition) > 0.001)
                    path.Add(desiredPosition);
                return path;
            }
        }

        //Try a bunch of paths and take the best one
        private List<Point2> GetBestPointPath(Team team, int id, RobotInfo desiredState,
            double avoidBallRadius, RobotPath oldPath, List<IGeom> obstacles)
        {
            ServiceManager sm = ServiceManager.getServiceManager();
            RobotVisionMessage robotVision = sm.GetLastMessage<RobotVisionMessage>();
            BallVisionMessage ballVision = sm.GetLastMessage<BallVisionMessage>();

            RobotInfo currentState;
            try
            { currentState = robotVision.GetRobot(team, id); }
            catch (ApplicationException)
            { return new List<Point2>(); }
            currentState = new RobotInfo(currentState);
            if (currentState.Velocity.magnitude() > MAX_OBSERVABLE_VELOCITY)
                currentState.Velocity = currentState.Velocity.normalizeToLength(MAX_OBSERVABLE_VELOCITY);

            BallInfo ball = ballVision.Ball;
            List<RobotInfo> robots = robotVision.GetRobots();

            if (ball != null)
            {
                ball = new BallInfo(ball);

                //Recenter the ball avoid position, and lower the radius
                Vector2 ballToDesired = desiredState.Position - ball.Position;
                if (ballToDesired.magnitude() < 1e-6)
                    avoidBallRadius = 0;
                /*else if (ballToDesired.magnitude() < avoidBallRadius)
                {
                    Vector2 ballToAlmostDesired = ballToDesired * 0.95;
                    Vector2 ballAwayFromDesired = (-ballToDesired).normalizeToLength(avoidBallRadius);

                    Vector2 radPos1 = ballToAlmostDesired + ball.Position;
                    Vector2 radPos2 = ballAwayFromDesired + ball.Position;
                    Vector2 average = (radPos1 + radPos2)/2.0;
                    avoidBallRadius = average.distance(radPos1);
                    ball.Position = average;
                }*/
            }

            List<Point2> bestPath = null;
            double bestPathScore = Double.NegativeInfinity;

            for (int i = 0; i < NUM_PATHS_TO_SCORE; i++)
            {
                msngr.db("trying a point path");
                List<Point2> path = GetPathTo(currentState, desiredState.Position, robots, ball, avoidBallRadius, obstacles);
                double score = 0;

                //Penalty based on distance from the goal, per meter
                if (path.Count >= 1)
                    score -= DIST_FROM_GOAL_SCORE * desiredState.Position.distance(path[path.Count - 1]);

                //Lose points per meter of path length greater than the start and end points
                double len = 0;
                for (int j = 0; j < path.Count - 1; j++)
                    len += path[j + 1].distance(path[j]);
                len += path[path.Count - 1].distance(desiredState.Position);
                len -= desiredState.Position.distance(path[0]);
                score -= len * EXCESS_LEN_SCORE;
                
                //Lose per node in the path if it's too sharp a bend.
                for (int j = 1; j < path.Count - 1; j++)
                {
                    Vector2 vec1 = path[j] - path[j - 1];
                    Vector2 vec2 = path[j + 1] - path[j];
                    if (vec1.magnitudeSq() < 1e-6 || vec2.magnitudeSq() < 1e-6)
                    { score += 1; continue; }
                    score += PER_BEND_SCORE * (vec1 * vec2) / vec1.magnitude() / vec2.magnitude();
                }

                
                //Win/lose points if the first segment of the path agrees 
                //with our current velocity, multiplied by the current speed
                

                int firstStep = 1;
                Vector2 firstVec = null;
                while (firstStep < path.Count && (firstVec = path[firstStep] - path[firstStep - 1]).magnitudeSq() < 1e-12)
                { firstStep++; }
                if (firstStep >= path.Count)
                    score += VELOCITY_AGREEMENT_SCORE;
                else
                {
                    Vector2 firstDir = firstVec.normalize();
                    double dScore = firstDir * currentState.Velocity * VELOCITY_AGREEMENT_SCORE;
                    score += dScore;
                }

                

                //Win/lose points if the path agrees with our old path, multiplied by the current speed
                if (oldPath != null && path.Count > 1 && oldPath.Waypoints.Count > 1)
                {
                    double distSum = 0;
                    for (int j = 1; j < path.Count; j++)
                    {
                        Point2 pathLoc = path[j];

                        double closestDist = OLDPATH_AGREEMENT_DIST;
                        for (int k = 0; k < oldPath.Waypoints.Count - 1; k++)
                        {
                            LineSegment seg = new LineSegment(oldPath.Waypoints[k + 1].Position, oldPath.Waypoints[k].Position);
                            double dist = seg.distance(pathLoc);
                            if (dist < closestDist)
                                closestDist = dist;
                        }

                        distSum += closestDist;
                    }

                    double avgDist = (distSum) / (path.Count - 1);
                    double velocity = currentState.Velocity.magnitude();
                    if (velocity < MIN_WEIGHT_VEL)
                    {
                        velocity = MIN_WEIGHT_VEL;
                    }
                    double dScore = (OLDPATH_AGREEMENT_DIST - avgDist) * OLDPATH_AGREEMENT_SCORE *
                        velocity;
                    score += dScore;
                }
                

                //Is it the best path so far?
                if (score > bestPathScore)
                {
                    bestPath = path;
                    bestPathScore = score;
                }
            }
            return bestPath;
        }


        //Top level function
        public RobotPath GetPath(RobotInfo desiredState, double avoidBallRadius,
            RobotPath oldPath, List<IGeom> obstacles)
        {
            Team team = desiredState.Team;
            int id = desiredState.ID;

            //Try to find myself
            RobotInfo curinfo;
            try
            {
                RobotVisionMessage msg = msngr.GetLastMessage<RobotVisionMessage>();
                curinfo = msg.GetRobot(team, id);
            }
            catch (ApplicationException)
            {
                return new RobotPath(team, id);
            }

            List<Point2> bestPath = GetBestPointPath(team, id, new RobotInfo(desiredState), avoidBallRadius, oldPath, obstacles);

            //Convert the path
            List<RobotInfo> robotPath = new List<RobotInfo>();

            //Overly simplistic conversion from planning on position vectors to RobotInfo that has orientation and velocity information
            //velocity at every waypoint just points to next one with constant speed
            double STEADY_STATE_SPEED = Constants.Motion.STEADY_STATE_SPEED;
            int pathStart = includeCurStateInPath ? 0 : 1;
            for (int i = pathStart; i < bestPath.Count; i++)
            {
                RobotInfo waypoint = new RobotInfo(bestPath[i], desiredState.Orientation, team, id);
                if (i < bestPath.Count - 1)
                    waypoint.Velocity = (bestPath[i + 1] - bestPath[i]).normalizeToLength(STEADY_STATE_SPEED);
                else
                    waypoint.Velocity = new Vector2(0, 0); //Stop at destination

                robotPath.Add(waypoint);
            }

            if (robotPath.Count <= 0)
                return new RobotPath(team, id);

            return new RobotPath(robotPath);
        }

        private Rectangle ExpandRectangle(Rectangle r, double d)
        {
            return new Rectangle(r.XMin - d, r.XMax + d, r.YMin - d, r.YMax + d);
        }
        private Circle ExpandCircle(Circle c, double d)
        {
            return new Circle(c.Center, c.Radius + d);
        }

        //Eeeewwwww TODO replace this
        private Rectangle ExpandLeft(Rectangle r)
        {
            return new Rectangle(r.XMax + (r.XMin - r.XMax) * 2, r.XMax, r.YMin, r.YMax);
        }
        //Eeeewwwww TODO replace this
        private Rectangle ExpandRight(Rectangle r)
        {
            return new Rectangle(r.XMin, r.XMin + (r.XMax - r.XMin) * 2, r.YMin, r.YMax);
        }

        //Top level function
        public RobotPath GetPath(RobotInfo desiredState, double avoidBallRadius,
            RobotPath oldPath, DefenseAreaAvoid leftAvoid, DefenseAreaAvoid rightAvoid)
        {
            //Build obstacle list
            List<IGeom> obstacles = new List<IGeom>();
            /*
            obstacles.Add(ExpandLeft(Constants.FieldPts.LEFT_GOAL_BOX));
            obstacles.Add(ExpandRight(Constants.FieldPts.RIGHT_GOAL_BOX));

            IList<Geom> range = new List<Geom>();
            if (leftAvoid == DefenseAreaAvoid.NORMAL)
                range = Constants.FieldPts.LEFT_DEFENSE_AREA;
            else if (leftAvoid == DefenseAreaAvoid.FULL)
                range = Constants.FieldPts.LEFT_EXTENDED_DEFENSE_AREA;

            for (int i = 0; i < range.Count; i++)
                if (range[i] is Rectangle)
                    range[i] = ExpandLeft((Rectangle)range[i]);
            obstacles.AddRange(range);

            range = new List<Geom>();
            if (rightAvoid == DefenseAreaAvoid.NORMAL)
                range = Constants.FieldPts.RIGHT_DEFENSE_AREA;
            else if (rightAvoid == DefenseAreaAvoid.FULL)
                range = Constants.FieldPts.RIGHT_EXTENDED_DEFENSE_AREA;
            for (int i = 0; i < range.Count; i++)
                if (range[i] is Rectangle)
                    range[i] = ExpandRight((Rectangle)range[i]);
            obstacles.AddRange(range);

            for (int i = 0; i < obstacles.Count; i++)
            {
                Geom g = obstacles[i];
                if (g is Rectangle)
                    obstacles[i] = ExpandRectangle((Rectangle)g, ROBOT_RADIUS);
                if (g is Circle)
                    obstacles[i] = ExpandCircle((Circle)g, ROBOT_RADIUS);
            }
            */
            //Error reporting
            // TODO: look at this
            /*for (int i = 0; i < obstacles.Count; i++)
            {
                Geom g = obstacles[i];
                if (g is Rectangle && ((Rectangle)g).contains(desiredState.Position))
                    Console.WriteLine("Warning: SmoothRRTPlanner desired state inside obstacle!");
                else if (g is Circle && ((Circle)g).contains(desiredState.Position))
                    Console.WriteLine("Warning: SmoothRRTPlanner desired state inside obstacle!");
            }*/


            return GetPath(desiredState, avoidBallRadius, oldPath, obstacles);
        }

    }

}
