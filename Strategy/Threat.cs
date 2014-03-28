using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RFC.Geometry;
using RFC.Core;

namespace RFC.Strategy
{
    class Threat
    {
        public enum ThreatType
        {
            robot,
            ball,
            space
        }

        public double severity;
        public Vector2 position;
        public ThreatType type;

        Threat(double sev, Vector2 pos, ThreatType typ)
        {
            severity = sev;
            position = pos;
            type = typ;
        }
        Threat(double sev, RobotInfo robot)
        {
            severity = sev;
            position = robot.Position;
            type = ThreatType.robot;
        }
    }
}
