using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RFC.Messaging;
using RFC.Core;

namespace RFC.Strategy
{
    public static class WaitBehavior
    {
        static ServiceManager msngr = ServiceManager.getServiceManager();

        // completely stop. set wheel speeds to zero
        public static void Halt(FieldVisionMessage msg)
        {
            int max_robot_id = 5;
            WheelSpeeds speeds = new WheelSpeeds();
            for (int id = 0; id < max_robot_id; id++)
            {
                msngr.SendMessage(new CommandMessage(new RobotCommand(id, speeds)));
            }
        }

        // this could be waiting for a kickin or something
        // need to stay 500mm away from ball
        public static void Stop(FieldVisionMessage msg)
        {
            //TODO
        }
    }
}
