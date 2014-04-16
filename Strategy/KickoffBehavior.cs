using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RFC.Messaging;
using RFC.Core;
using RFC.Geometry;
using RFC.PathPlanning;

namespace RFC.Strategy
{
    public class KickOffBehavior
    {
        Team team;
        ServiceManager msngr;

        public KickOffBehavior(Team team)
        {
            this.team = team;
            this.msngr = ServiceManager.getServiceManager();
        }

        public void Ours(FieldVisionMessage msg)
        {
            //TODO
            // initial kick, then transition to normal play
        }

        public void OursSetup(FieldVisionMessage msg)
        {
            //TODO
            // probably just hardcode in positions
            // assume that we start on the left side of the field

            // getting our robots
            List<RobotInfo> ours = msg.GetRobots(team);
            int n = ours.Count();

            // getting destinations we want to go to
            // for now just put them in a line
            List<RobotInfo> destinations = new List<RobotInfo>();
            for (int i = 0; i < n; i++)
            {
                Vector2 position = new Vector2(2-Constants.Basic.ROBOT_RADIUS*4 * i, 0);
                destinations.Add(new RobotInfo(position,1.5,0));
            }

            // this function matches the closest robot to closest destination and handles
            // sending messages to get there


            DestinationMatcher.SendByDistance(ours, destinations);


        }

        public void Theirs(FieldVisionMessage msg)
        {
            //TODO
            // detect when play has started, then switch to normal play
        }

        public void TheirsSetup(FieldVisionMessage msg)
        {
            //TODO
            // probably just hardcoded positions
        }
    }
}
