using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RFC.Messaging;
using RFC.Core;
using RFC.Geometry;

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
            List<RobotInfo> ours = msg.GetRobots(team);

            foreach (RobotInfo rob in ours)
            {
                // make a copy of the robot's current information
                RobotInfo destination = new RobotInfo(rob);

                // changing the position of that RobotInfo
                destination.Position = new Vector2(0, 0);

                // sending that RobotInfo as a destination for that robot
                msngr.SendMessage(new RobotDestinationMessage(destination, true, false, true));

            }

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
