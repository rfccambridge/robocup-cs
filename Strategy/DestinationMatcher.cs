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
        // basically directs robots one-to-one to the destinations
        // i.e. robot 1 goes to destination 1; robot 2 goes to destination 2...
        // totally doesn't account for distance
        public static void SendByDistance(List<RobotInfo> robots, List<RobotInfo> destinations)
        {
            if (robots.Count != destinations.Count)
                throw new Exception("different numbers of robots and destinations: " + robots.Count + ", " + destinations.Count);

            int[] assignments = GetAssignments(robots, destinations);
            int n = robots.Count();
            ServiceManager msngr = ServiceManager.getServiceManager();

            // sending dest messages for each one
            for (int i = 0; i < n; i++)
            {
                RobotInfo dest = new RobotInfo(destinations[assignments[i]]);
                dest.ID = robots[i].ID;

                //Console.WriteLine("robot(" + robots[i].ID + ") " + robots[i].Position + " -> dests(" + assignments[i] + ") " + destinations[assignments[i]].Position);

                msngr.SendMessage(new RobotDestinationMessage(dest, true, false));
            }
        }

        public static int[] GetAssignments(List<RobotInfo> robots, List<RobotInfo> destinations) 
        {
            if (robots.Count != destinations.Count)
                throw new Exception("different numbers of robots and destinations: " + robots.Count + ", " + destinations.Count);
            int n = robots.Count();

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
                    costs[r, d] = (int)Math.Round(10000*Math.Pow(robots[r].Position.distance(destinations[d].Position),exponent));
                }
            }

            return costs;
        }
    }
}
