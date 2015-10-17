using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RFC.Geometry;
using RFC.Core;

namespace RFC.Strategy
{
    /// <summary>
    /// A StateChecker is used to determine whether the field is still within a given configuration
    /// It is initialized with some geometric constraints, and the position of the ball is checked
    /// against those constraints. If at least one condition is met, it returns true, else false.
    ///
    /// Currently supports circles and line segments
    /// </summary>
    public class StateChecker
    {
        List<Geom> rules;
        List<double> ranges;

        public StateChecker(Circle rule) : this()
        {
            addRule(rule);
        }

        public StateChecker(LineSegment line, double range) : this()
        {
            addRule(line, range);
        }

        public StateChecker()
        {
            this.rules = new List<Geom>();
            this.ranges = new List<double>();
        }

        public void addRule(Circle rule)
        {
            this.rules.Add(rule);
            this.ranges.Add(0);
        }

        public void addRule(LineSegment line, double range)
        {
            this.rules.Add(line);
            this.ranges.Add(range);
        }

        public bool check(Vector2 pos)
        {
            for (int i = 0; i < rules.Count; i++)
            {
                Geom rule = rules[i];
                double range = ranges[i];
                if (rule is Circle)
                {
                    // that constraint was within a circle
                    if (((Circle)rule).contains(pos))
                        return true;
                }
                else if (rule is LineSegment)
                {
                    // constraint was within distance of line
                    if (((LineSegment)rule).distance(pos) < range)
                        return true;
                }
                else
                {
                    throw new Exception("StateChecker only suppports circles and line segments");
                }
            }
            return false;
        }
    }
}
