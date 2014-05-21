using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RFC.Core;

namespace RFC.Messaging
{
    public class RobotDestinationMessage : Message
    {
        public RobotInfo Destination {get; private set;}
        public bool AvoidBall { get; private set; }
        public bool Slow { get; private set; }

        public RobotDestinationMessage(RobotInfo destination, bool avoidBall, bool slow = false)
        {
            Destination = destination;
            AvoidBall = avoidBall;
            Slow = slow;
        }
    }
}
