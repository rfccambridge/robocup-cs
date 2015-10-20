using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RFC.Core;
using RFC.Geometry;

namespace RFC.Messaging
{
    public class KickMessage : Message
    {
        public RobotInfo Source;
        public Point2 Target;

        public KickMessage(RobotInfo source, Point2 target)
        {
            Target = target;
            Source = source;
        }
    }
}