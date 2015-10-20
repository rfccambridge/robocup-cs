using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RFC.Geometry;
using RFC.Core;

namespace RFC.Strategy
{
    public class Threat : IComparable<Threat>
    {
        public enum ThreatType
        {
            robot,
            ball,
            space
        }

        public readonly double severity;
        public readonly Vector2 position;
        public readonly ThreatType type;

        public Threat(double sev, Vector2 pos, ThreatType typ)
        {
            severity = sev;
            position = pos;
            type = typ;
        }
        public Threat(double sev, RobotInfo robot) : this(sev, robot.Position, ThreatType.robot) { }
        public Threat(double sev, BallInfo ball) : this(sev, ball.Position, ThreatType.ball) { }

        public int CompareTo(Threat other)
        {
            // sort in reverse order, so .sort() puts the worst threats first
            return -1 * severity.CompareTo(other.severity);
        }
    }
}
