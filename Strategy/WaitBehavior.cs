using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RFC.Messaging;
using RFC.Core;
using RFC.PathPlanning;

namespace RFC.Strategy
{
    public class WaitBehavior
    {
        Team team;
        ServiceManager msngr;
        int max_robot_id;

        public WaitBehavior(Team team, int max_robots)
        {
            this.team = team;
            this.msngr = ServiceManager.getServiceManager();
            this.max_robot_id = max_robots;

        }

        // completely stop. set wheel speeds to zero whether we can see it or not
        public void Halt(FieldVisionMessage msg)
        {
            WheelSpeeds speeds = new WheelSpeeds();
            for (int id = 0; id < max_robot_id; id++)
            {
                msngr.SendMessage(new CommandMessage(new RobotCommand(id, speeds)));
            }
        }

        // this could be waiting for a kickin or something
        // need to stay 500mm away from ball
        public void Stop(FieldVisionMessage msg)
        {
            //TODO: go to intelligent places during this time

            foreach (RobotInfo rob in msg.GetRobots())
            {
                RobotInfo dest = Avoider.avoid(rob, msg.Ball.Position, .50);
                msngr.SendMessage(new RobotDestinationMessage(dest, true, false, true));
            }
        }
    }
}
