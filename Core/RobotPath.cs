using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using RFC.Geometry;

namespace RFC.Core
{
    /// <summary>
    /// Describes a path to a destination as a series of RobotInfo waypoints
    /// </summary>
    public class RobotPath
    {
        // Public interface for variables
        public int ID { get; private set; }
        public Team Team { get; private set; }
        public List<RobotInfo> Waypoints { get; private set; }
        public bool Slow { get; set; }

        // Store final goal for planners that do not store it as a waypoint
        RobotInfo _finalState;

        // Can be constructed multiple ways, depending on how the path is determined

        public RobotPath()
        {
            // Create empty path
            Waypoints = new List<RobotInfo>();
        }

        public RobotPath(Team team, int id)
        {
            // Create empty path with id
            Team = team;
            ID = id;
            Waypoints = new List<RobotInfo>();
        }

        /// <summary>
        /// Given a list of RobotInfo waypoints
        /// </summary>
        /// <param name="waypoints">RobotInfo waypoints along determined path</param>
        public RobotPath(List<RobotInfo> waypoints)
        {
            // take id and path from given waypoints
            if (waypoints.Count == 0)
                throw new Exception("Empty path given to path constructor");

            Team = waypoints[0].Team;
            ID = waypoints[0].ID;

            Waypoints = waypoints;
        }

        /// <summary>
        /// Given a single Vector2 waypoint
        /// </summary>
        /// <param name="waypoints"></param>
        public RobotPath(Team team, int id, Point2 waypoint)
        {
            Team = team;
            ID = id;

            List<RobotInfo> waypoints = new List<RobotInfo>();
            waypoints.Add(new RobotInfo(waypoint, 0, team, id));

            Waypoints = waypoints;
        }

        /// <summary>
        /// Given a single RobotInfo waypoint
        /// </summary>
        /// <param name="waypoint"></param>
        public RobotPath(Team team, int id, RobotInfo waypoint)
        {
            Team = team;
            ID = id;

            List<RobotInfo> waypoints = new List<RobotInfo>();
            waypoints.Add(waypoint);

            Waypoints = waypoints;
        }

        /// <summary>
        /// Constructed with a starting list of RobotInfo objects and ending list of Vector2 waypoints
        /// </summary>
        /// <param name="waypoints1">RobotInfo starting list of waypoints</param>
        /// <param name="waypoints2">Vector2 ending list of waypoints</param>
        public RobotPath(List<RobotInfo> waypoints1, List<Point2> waypoints2)
        {
            Team = waypoints1[0].Team;
            ID = waypoints1[0].ID;

            // Combine paths into a single waypoints list
            Waypoints = waypoints1;
            Waypoints.AddRange(makeRobotInfoList(ID, waypoints2));
        }

        /// <summary>
        /// Can be initialized with a single list of vectors and a Robot ID, which are
        /// converted into a list of RobotInfo objects
        /// </summary>
        /// <param name="id">ID of robot</param>
        /// <param name="waypoints">Vector2 list of waypoints along path</param>
        public RobotPath(Team team, int id, List<Point2> waypoints)
        {
            Team = team;
            ID = id;
            Waypoints = makeRobotInfoList(ID, waypoints);
        }


        /// <summary>
        /// Turn a list of Vector2 into a list of RobotInfo objects- each is oriented towards next waypoint.
        /// </summary>
        /// <param name="waypoints"></param>
        /// <returns></returns>
        private List<RobotInfo> makeRobotInfoList(int id, List<Point2> waypoints)
        {
            List<RobotInfo> retlst = new List<RobotInfo>();
            double orientation = 0;
            for (int i = 0; i < waypoints.Count; i++)
            {
                // Point orientation towards next waypoint if there is one- otherwise,
                // do not change it
                if (i < (waypoints.Count - 1))
                {
                    orientation = (waypoints[i + 1] - waypoints[i]).cartesianAngle();
                }
                retlst.Add(new RobotInfo(waypoints[i], orientation, Team, id));
            }

            return retlst;
        }

        /// <summary>
        /// Given a RobotInfo object, find a waypoint that's:
        /// - close enough (almost the nearest)
        /// - but still further towards the final goal, so that we don't slow down
        /// </summary>
        /// <param name="point">The position, relative to which we are searching</param>
        /// <returns></returns>
        public RobotInfo findNextNearestWaypoint(RobotInfo point)
        {
            // brute force over the waypoints to find the nearest
            int closestWaypointIndex = findNearestWaypointIndex(point);

            //Skip the nearest in the direction closer to the goal
            //But make sure we're still inside the path boundary
            if (closestWaypointIndex < Waypoints.Count - 1)
                closestWaypointIndex += 1;

            return Waypoints[closestWaypointIndex];
        }

        /// <summary>
        /// Given a RobotInfo object, return the nearest waypoint to that object
        /// </summary>
        /// <param name="point">A RobotInfo representing the current position</param>
        /// <returns></returns>
        public RobotInfo findNearestWaypoint(RobotInfo point)
        {
            return Waypoints[findNearestWaypointIndex(point)];
        }

        /// <summary>
        /// Given a RobotInfo object, return the index of the nearest waypoint to that object
        /// </summary>
        /// <param name="point">A RobotInfo representing the current position</param>
        /// <returns></returns>
        private int findNearestWaypointIndex(RobotInfo point)
        {
            // for now, brute force search

            int closestWaypointIndex = 0;
            double minDistSq = double.PositiveInfinity;

            for (int i = 0; i < Waypoints.Count; i++)
            {
                RobotInfo waypoint = Waypoints[i];
                double distSq = waypoint.Position.distanceSq(point.Position);
                if (distSq < minDistSq)
                {
                    closestWaypointIndex = i;
                    minDistSq = distSq;
                }
            }

            return closestWaypointIndex;
        }

        /// <summary>
        /// Get waypoint by index
        /// </summary>
        /// <param name="index">Index of desired waypoint</param>
        /// <returns></returns>
        public RobotInfo getWaypoint(int index)
        {
            return Waypoints[index];
        }

        /// <summary>
        /// Returns the i-th waypoint of the path
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public RobotInfo this[int i] => Waypoints[i];

        public static implicit operator List<RobotInfo>(RobotPath _this)
        {
            return _this.Waypoints;
        }

        /// <summary>
        /// Set final state, in case it is not one of the waypoints
        /// </summary>
        /// <param name="state"></param>
        public void setFinalState(RobotInfo state)
        {
            _finalState = state;
        }

        /// <summary>
        /// Get final state, which may not be stored as a waypoint
        /// </summary>
        /// <returns></returns>
        public RobotInfo getFinalState()
        {
            // if none is set, return the last waypoint in the path
            return _finalState ?? Waypoints.LastOrDefault();
        }

        /// <summary>
        /// return whether this is an empty path
        /// </summary>
        /// <returns></returns>
        public bool isEmpty()
        {
            return Waypoints.Count == 0;
        }

        public override string ToString()
        {
            StringBuilder str = new StringBuilder();
            foreach(RobotInfo info in Waypoints)
            {
                str.Append(info.ToString());
                str.Append(' ');
            }
            return str.ToString();
        }
    }
}
