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
        public Vector2 Target;

        public KickMessage(RobotInfo source, Vector2 target)
        {
            Target = target;
            Source = source;
        }
    }
}