using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using RFC.Core;
using RFC.Geometry;
using RFC.Messaging;

namespace RFC.PathPlanning
{
    public class VelocityDriver : IMessageHandler<RobotVisionMessage>, IMessageHandler<RobotPathMessage>, IMessageHandler<StopMessage>
    {
        private ServiceManager msngr;
        static int NUM_ROBOTS = ConstantsRaw.get<int>("default", "NUM_ROBOTS");

        //State - track the previous wheel speed sent so we don't send something too different
        private WheelSpeeds[] lastSpeeds = new WheelSpeeds[NUM_ROBOTS];

        //Base desired speeds for movement
        private double BASE_SPEED;          //In m/s
        private double MAX_ANGLULAR_SPEED;  //In rev/s
        private double MAX_ANGLULAR_LINEAR_SPEED;  //In rev/s/radian

        //When computing how fast to rotate so that we will be correct by the time
        //we get to the destination, assume we will get there this fast
        private double ROTATION_ASSUMED_SPEED; //In m/s

        //Conversion to wheel speed commands
        private double XY_BASIS_SCALE; //Wheel speeds required for 1 m/s movement
        private double R_BASIS_SCALE;  //Wheel speeds required for 1 rev/s movement 

        //Max wheel speed change per frame of control
        private double MAX_WHEEL_SPEED_CHANGE_PER_FRAME;

        //How much should we weight in the direction we will need to head for the next waypoint?
        private double NEXT_NEXT_PROP;

        //If the linear and angular errors are less then this, we're done.
        private double GOOD_ENOUGH_DIST;
        private double GOOD_ENOUGH_ANGLE; //In revolutions

        //How much should we correct for rotation throwing us off?
        private double PLANNED_ANG_SPEED_CORRECTION;
        private double CURRENT_ANG_SPEED_CORRECTION;

        //Scaling for speed based on distance from goal
        private Tuple<double, double>[] SCALE_BY_DISTANCE;
        //Slow version
        //private Pair<double, double>[] SCALE_BY_DISTANCE_SLOW;
        //Scaling for speed based on distance from obstacle
        private Tuple<double, double>[] SCALE_BY_OBSTACLE_DISTANCE;
        //Scaling for distance from obstacle based on cosine of angle
        private Tuple<double, double>[] AGREEMENT_EFFECTIVE_DISTANCE_FACTOR;

        Dictionary<int, RobotPath> lastPaths = new Dictionary<int, RobotPath>();

        bool stopped = false;
        Stopwatch sw;

        private static double interp(Tuple<double, double>[] pairs, double d)
        {
            for (int i = 0; i < pairs.Length; i++)
            {
                if (pairs[i].Item1 >= d)
                {
                    if (i == 0)
                        return pairs[0].Item2;
                    double lambda = (d - pairs[i - 1].Item1) / (pairs[i].Item1 - pairs[i - 1].Item1);
                    return pairs[i - 1].Item2 + lambda * (pairs[i].Item2 - pairs[i - 1].Item2);
                }
            }
            return pairs[pairs.Length - 1].Item2;
        }

        public VelocityDriver()
        {
            ReloadConstants();

            object lockObject = new object();
            msngr = ServiceManager.getServiceManager();
            new QueuedMessageHandler<RobotVisionMessage>(this, lockObject);
            new QueuedMessageHandler<RobotPathMessage>(this, lockObject);
            msngr.RegisterListener<StopMessage>(HandleMessage, lockObject);

            sw = Stopwatch.StartNew();
        }

        public void HandleMessage(RobotVisionMessage rvm)
        {
            foreach (RobotInfo robot in rvm.GetRobots()) {
                WheelSpeeds speeds;
                if (stopped)
                {
                    speeds = new WheelSpeeds();
                }
                else if (lastPaths.ContainsKey(robot.ID))
                {
                    speeds = followPath(lastPaths[robot.ID], rvm);
                }
                else
                {
                    speeds = new WheelSpeeds();
                }
                msngr.SendMessage(new CommandMessage(new RobotCommand(robot.ID, speeds)));
                sw.Restart();
            }
        }

        public void HandleMessage(RobotPathMessage rpm)
        {
            lastPaths.Remove(rpm.Path.ID);
            lastPaths.Add(rpm.Path.ID, rpm.Path);
        }

        public void HandleMessage(StopMessage message)
        {
            stopped = true;
        }

        private Tuple<double, double>[] readDoublePairArray(string numPrefix, string prefix)
        {
            int numPairs = ConstantsRaw.get<int>("control", numPrefix);
            Tuple<double, double>[] pairs = new Tuple<double, double>[numPairs];

            for (int i = 0; i < numPairs; i++)
            {
                string str = ConstantsRaw.get<string>("control", prefix + i);

                //Remove spaces and split
                str = str.Replace(" ", "");
                string[] entries = str.Split(new char[] { ',' });
                pairs[i] = new Tuple<double, double>(Convert.ToDouble(entries[0]), Convert.ToDouble(entries[1]));
            }

            return pairs;
        }

        public void ReloadConstants()
        {
            BASE_SPEED = ConstantsRaw.get<double>("control", "VD_BASE_SPEED");
            MAX_ANGLULAR_SPEED = ConstantsRaw.get<double>("control", "VD_MAX_ANGLULAR_SPEED");
            MAX_ANGLULAR_LINEAR_SPEED = ConstantsRaw.get<double>("control", "VD_MAX_ANGLULAR_LINEAR_SPEED");
            ROTATION_ASSUMED_SPEED = ConstantsRaw.get<double>("control", "VD_ROTATION_ASSUMED_SPEED");
            XY_BASIS_SCALE = ConstantsRaw.get<double>("control", "VD_XY_BASIS_SCALE");
            R_BASIS_SCALE = ConstantsRaw.get<double>("control", "VD_R_BASIS_SCALE");
            MAX_WHEEL_SPEED_CHANGE_PER_FRAME = ConstantsRaw.get<double>("control", "VD_MAX_WHEEL_SPEED_CHANGE_PER_FRAME");
            NEXT_NEXT_PROP = ConstantsRaw.get<double>("control", "VD_NEXT_NEXT_PROP");
            GOOD_ENOUGH_DIST = ConstantsRaw.get<double>("control", "VD_GOOD_ENOUGH_DIST");
            GOOD_ENOUGH_ANGLE = (1.0 / 360.0) * ConstantsRaw.get<double>("control", "VD_GOOD_ENOUGH_ANGLE");
            PLANNED_ANG_SPEED_CORRECTION = ConstantsRaw.get<double>("control", "VD_PLANNED_ANG_SPEED_CORRECTION");
            CURRENT_ANG_SPEED_CORRECTION = ConstantsRaw.get<double>("control", "VD_CURRENT_ANG_SPEED_CORRECTION");


            SCALE_BY_DISTANCE = readDoublePairArray("VD_NUM_SCALE_BY_DISTANCE", "VD_SCALE_BY_DISTANCE_");
            //SCALE_BY_DISTANCE_SLOW = readDoublePairArray("VD_NUM_SCALE_BY_DISTANCE_SLOW", "VD_SCALE_BY_DISTANCE_SLOW_");
            SCALE_BY_OBSTACLE_DISTANCE = readDoublePairArray("VD_NUM_SCALE_BY_OBSTACLE_DISTANCE", "VD_SCALE_BY_OBSTACLE_DISTANCE_");
            AGREEMENT_EFFECTIVE_DISTANCE_FACTOR = readDoublePairArray("VD_NUM_AGREEMENT_EFFECTIVE_DISTANCE_FACTOR", "VD_AGREEMENT_EFFECTIVE_DISTANCE_FACTOR_");
        }

        public WheelSpeeds followPath(RobotPath path, RobotVisionMessage robotMessage)
        {

            Team team = path.Team;
            int id = path.ID;

            if (path == null)
                return new WheelSpeeds();

            RobotInfo desiredState = path.getFinalState();

            if (path.Waypoints.Count <= 0 || desiredState == null)
                return new WheelSpeeds();

            //Retrieve current robot info
            RobotInfo curInfo;
            try
            {
                curInfo = robotMessage.GetRobot(team, id);
            }
            catch (ApplicationException)
            {
                return new WheelSpeeds();
            }

            //Retrieve next waypoints
            int idx;
            RobotInfo nextWaypoint = null;
            int nextWaypointIdx = 0;

            //Find the point we should head towards
            for (idx = 1; idx < path.Waypoints.Count; idx++)
            {
                //End of the path? Then that's where we're going
                if (idx == path.Waypoints.Count - 1)
                {
                    nextWaypoint = path.Waypoints[idx];
                    nextWaypointIdx = idx;
                    break;
                }

                //Must be different from us
                if (path.Waypoints[idx].Position.distanceSq(curInfo.Position) <= 1e-10)
                {
                    continue;
                }

                //If we're too far along the path to this waypoint from the previous, then move to the next again.
                double distAlongTimesDistSegment = (curInfo.Position - path.Waypoints[idx - 1].Position) * (path.Waypoints[idx].Position - path.Waypoints[idx - 1].Position);
                double distSegmentSq = path.Waypoints[idx].Position.distanceSq(path.Waypoints[idx - 1].Position);
                if (distAlongTimesDistSegment >= 0.75 * distSegmentSq)
                {
                    continue;
                }

                //Otherwise, we stop here
                nextWaypoint = path.Waypoints[idx];
                nextWaypointIdx = idx;
                break;
            }


            //Find the next significantly different point after that
            RobotInfo nextNextWaypoint = null;
            for (idx++; idx < path.Waypoints.Count; idx++)
            {
                if ((path.Waypoints[idx].Position - path.Waypoints[idx - 1].Position).magnitude() > 1e-5)
                {
                    nextNextWaypoint = path.Waypoints[idx];
                    break;
                }
            }
            if (nextWaypoint == null)
            {
                return new WheelSpeeds();
            }

            Vector2 curToNext = (nextWaypoint.Position - curInfo.Position).normalize();
            Vector2 nextToNextNext = nextNextWaypoint != null ?
                (nextNextWaypoint.Position - nextWaypoint.Position).normalize() :
                null;

            //Compute desired direction
            Vector2 desiredVelocity = curToNext.rotate(-curInfo.Orientation); //In robot reference frame
            if (nextToNextNext != null)
                desiredVelocity += NEXT_NEXT_PROP * nextToNextNext.rotate(-curInfo.Orientation);

            //Compute distance left to go in the path
            double distanceLeft = 0.0;
            distanceLeft += (nextWaypoint.Position - curInfo.Position).magnitude();
            for (int i = nextWaypointIdx; i < path.Waypoints.Count - 1; i++)
                distanceLeft += (path[i + 1].Position - path[i].Position).magnitude();
            //distanceLeft += (desiredState.Position - path[path.Waypoints.Count - 1].Position).magnitude();

            //Compute distance to nearest obstacle
            //Adjusted for whether we are going towards them or not.
            List<RobotInfo> robots = robotMessage.GetRobots();
            double obstacleDist = 10000;
            Vector2 headingDirection = nextToNextNext != null ? nextToNextNext : curToNext;
            foreach (RobotInfo info in robots)
            {
                if (info.Team != team || info.ID != id)
                {
                    double dist = info.Position.distance(curInfo.Position);
                    if (dist > 0)
                    {
                        double headingTowards = (1.0 + headingDirection * (info.Position - curInfo.Position).normalize()) / 2;
                        dist *= interp(AGREEMENT_EFFECTIVE_DISTANCE_FACTOR, headingTowards);
                    }

                    if (dist < obstacleDist)
                        obstacleDist = dist;
                }
            }

            //Scale to desired speed
            double speed = BASE_SPEED * Math.Min(interp(SCALE_BY_DISTANCE, distanceLeft), interp(SCALE_BY_OBSTACLE_DISTANCE, obstacleDist));
            /*if (!path.Slow)
            {
                speed = BASE_SPEED * Math.Min(interp(SCALE_BY_DISTANCE, distanceLeft), interp(SCALE_BY_OBSTACLE_DISTANCE, obstacleDist));
            }
            else
            {
                speed = BASE_SPEED * Math.Min(interp(SCALE_BY_DISTANCE_SLOW, distanceLeft), interp(SCALE_BY_OBSTACLE_DISTANCE, obstacleDist));
            } */   
            bool linearDone = distanceLeft <= GOOD_ENOUGH_DIST;
            if (linearDone)
                speed = 0;
            if (desiredVelocity.magnitudeSq() > 1e-12)
                desiredVelocity = desiredVelocity.normalizeToLength(speed);

            //Smallest turn algorithm
            double dTheta = desiredState.Orientation - curInfo.Orientation;

            //Map dTheta to the equivalent angle in [-PI,PI]
            dTheta = dTheta % (2 * Math.PI);
            if (dTheta > Math.PI) dTheta -= 2 * Math.PI;
            if (dTheta < -Math.PI) dTheta += 2 * Math.PI;

            //Convert to revolutions
            double dRev = dTheta / (2 * Math.PI);

            //Figure out how long we will take to get there and spread the revolution out over that time
            double timeLeft = distanceLeft / ROTATION_ASSUMED_SPEED;
            if (timeLeft <= 1e-5)
                timeLeft = 1e-5;

            //Compute the speed we'd need to rotate at, in revs/sec, and cap it
            double angularVelocity = dRev / timeLeft;
            double maxAS = Math.Min(MAX_ANGLULAR_SPEED, Math.Abs(dRev) * MAX_ANGLULAR_LINEAR_SPEED * Math.PI * 2);
            if (angularVelocity > maxAS)
                angularVelocity = maxAS;
            if (angularVelocity < -maxAS)
                angularVelocity = -maxAS;

            bool angularDone = dRev >= -GOOD_ENOUGH_ANGLE && dRev <= GOOD_ENOUGH_ANGLE;
            if (angularDone)
                angularVelocity = 0;

            double rb = R_BASIS_SCALE;
            WheelSpeeds rbasis = new WheelSpeeds(rb, rb, rb, rb);
            WheelSpeeds angularSpeeds = rbasis * angularVelocity;

            //Adjust for rotational interaction
            desiredVelocity = desiredVelocity.rotate(-angularVelocity * (2 * Math.PI) * PLANNED_ANG_SPEED_CORRECTION
                - curInfo.AngularVelocity * CURRENT_ANG_SPEED_CORRECTION);

            //Convert linear desired velocity to wheel speeds
            double xyb = XY_BASIS_SCALE;
            WheelSpeeds xBasis = new WheelSpeeds(xyb, -xyb, -xyb, xyb);
            WheelSpeeds yBasis = new WheelSpeeds(xyb, xyb, -xyb, -xyb);
            WheelSpeeds linearSpeeds = xBasis * desiredVelocity.X + yBasis * desiredVelocity.Y;

            WheelSpeeds speeds = linearSpeeds + angularSpeeds;

            //Scale as desired
            speeds = speeds * (Constants.Motion.SPEED_SCALING_FACTOR_ALL * Constants.Motion.SPEED_SCALING_FACTORS[id]);

            //Make sure that if we're not done yet, we have some positive wheel speeds 
            if (!angularDone || !linearDone)
            {
                //The largest the magnitude can be while rounding to zero is 1, which occurs when all entries
                //are 0.5-eps
                double magnitude = speeds.magnitude();
                if (magnitude < 1.001 && magnitude > 0.001) //Guard against division by zero
                {
                    speeds = speeds * (1.001 / magnitude); //Scale to length 1.001
                }
            }

            if (path.Slow)
            {
                speeds.lb *= .5;
                speeds.rb *= .5;
                speeds.rf *= .5;
                speeds.lf *= .5;
            }

            //Adjust speeds to cope with a maximum acceleration limit
            WheelSpeeds old = lastSpeeds[id];
            if (old != null)
            {
                WheelSpeeds diff = speeds - old;
                double diffMagnitude = diff.magnitude() / 4;
                if (diffMagnitude > MAX_WHEEL_SPEED_CHANGE_PER_FRAME)
                    speeds = old + diff * (MAX_WHEEL_SPEED_CHANGE_PER_FRAME / diffMagnitude);
            }

            lastSpeeds[id] = speeds;

            return speeds;
        }
    }
}
