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

        public double severity;
        public Vector2 position;
        public ThreatType type;
        private double p;
        private ThreatType threatType;

        public Threat(double sev, Vector2 pos, ThreatType typ)
        {
            severity = sev;
            position = pos;
            type = typ;
        }
        public Threat(double sev, RobotInfo robot)
        {
            severity = sev;
            position = robot.Position;
            type = ThreatType.robot;
        }
        public Threat(double sev, BallInfo ball)
        {
            severity = sev;
            position = ball.Position;
            type = ThreatType.ball;
        }

        public Threat(double p, ThreatType threatType)
        {
            // TODO: Complete member initialization
            this.p = p;
            this.threatType = threatType;
        }

        public int CompareTo(Threat other)
        {
            if (severity < other.severity) {
                return 1;
            }
            else if (severity == other.severity)
            {
                return 0;
            }
            else
            {
                return -1;
            }
        }
    }
}
