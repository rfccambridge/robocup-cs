using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RFC.Core;
using RFC.Messaging;
using RFC.Geometry;
using RFC.Utilities;

namespace RFC.Strategy
{
    public static class DestinationMatcher
    {
        // <summary>
        // assigns each robot to one of the destinations and sends messages.
        // Position and orientation come from Destinations, IDs come from Robots
        //</summary>
        public static void SendByDistance(List<RobotInfo> robots, List<RobotInfo> destinations)
        {
            int[] assignments = GetAssignments(robots, destinations);
            int n = robots.Count();
            ServiceManager msngr = ServiceManager.getServiceManager();

            if (robots.Count != destinations.Count)
                throw new Exception("different numbers of robots and destinations: " + robots.Count + ", " + destinations.Count);

            // sending dest messages for each one
            for (int i = 0; i < n; i++)
            {
                RobotInfo dest = new RobotInfo(destinations[assignments[i]]);
                dest.ID = robots[i].ID;

                msngr.SendMessage(new RobotDestinationMessage(dest, true, false, true));
            }
        }

        public static int[] GetAssignments(List<RobotInfo> robots, List<RobotInfo> destinations) 
        {
            int n = robots.Count();
            if (n != destinations.Count())
            {
                ServiceManager.getServiceManager().SendMessage(new LogMessage("ERROR: # of robots and destinations don't match"));
                return null;
            }

            // getting cost matrix
            int[,] costs = constructDistanceMatrix(robots, destinations);

            // using hungarian algorithm to find assignments
            // value at [i] is index of which destination it should be assigned to
            int[] assignments = HungarianAlgorithm.FindAssignments(costs);

            return assignments;
        }


        // builds cost matrix from robots and destinations
        // each row is one robot
        // each column is one destination
        // ordering is by their position in the list
        public static int[,] constructDistanceMatrix(List<RobotInfo> robots, List<RobotInfo> destinations)
        {
            double exponent = 3;
            int n = robots.Count();
            int[,] costs = new int[n, n];

            for (int r = 0; r < n; r++)
            {
                for (int d = 0; d < n; d++)
                {
                    costs[r, d] = (int)Math.Round(Math.Pow(robots[r].Position.distance(destinations[d].Position),exponent));
                }
            }

            return costs;
        }
    }
}
