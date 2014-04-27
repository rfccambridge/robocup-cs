using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RFC.Core;
using RFC.Messaging;

namespace RFC.PathPlanning
{
    public static class DribblePlanner
    {
        public static void GetPossession(RobotInfo closest, FieldVisionMessage msg)
        {
            double orientation = (msg.Ball.Position - closest.Position).cartesianAngle();
            RobotInfo destination = new RobotInfo(msg.Ball.Position, orientation, closest.ID);
            ServiceManager.getServiceManager().SendMessage(new RobotDestinationMessage(destination, false, false));
        }
    }
}
