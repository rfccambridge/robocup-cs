using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RFC.Geometry
{
    /// <summary>
    /// A robot shape. Circular arc, with a flat front part.
    /// </summary>
    public class RobotShape : Geom<RobotShape>
    {
        public Arc Arc { get; private set; }
        public LineSegment Segment { get; private set; }

        private RobotShape(Arc arc, LineSegment segment)
        {
            this.Arc = arc;
            this.Segment = segment;
        }

        /// <summary>
        /// Creates a robot shape. 
        /// </summary>
        public RobotShape(Vector2 center, double radius, double orientation, double frontPlateRadius)
        {
            double angle = Math.Acos(frontPlateRadius / radius);

            this.Arc = new Arc(center, radius, orientation - angle, orientation + angle);
            this.Segment = new LineSegment(Arc.StartPt, Arc.StopPt);
        }

        /// <summary>
        /// Returns a robot shape translated by the added vector
        /// </summary>
        public static RobotShape operator +(RobotShape rs, Vector2 v)
        {
            return new RobotShape(rs.Arc + v, rs.Segment + v);
        }

        /// <summary>
        /// Returns a line segment translated by the added vector
        /// </summary>
        public static RobotShape operator +(Vector2 v, RobotShape rs)
        {
            return new RobotShape(v + rs.Arc, v + rs.Segment);
        }

        /// <summary>
        /// Returns a line segment translated by the negative of the vector
        /// </summary>
        public static RobotShape operator -(RobotShape rs, Vector2 v)
        {
            return new RobotShape(rs.Arc - v, rs.Segment - v);
        }

        /// <summary>
        /// Returns the translation of this line segment by the given vector.
        /// </summary>
        public override RobotShape translate(Vector2 v)
        {
            return this + v;
        }

        /// <summary>
        /// Returns a line segment that is this line rotated a given number of radians in the
        /// counterclockwise direction around p.
        /// </summary>
        public override RobotShape rotateAroundPoint(Vector2 p, double angle)
        {
            return new RobotShape(Arc.rotateAroundPoint(p, angle), Segment.rotateAroundPoint(p, angle));
        }


        public override string ToString()
        {
            return "RobotShape[" + Arc + ", " + Segment + "]";
        }
        
        public override double distance(Vector2 p)
        {
            return Math.Min(Arc.distance(p), Segment.distance(p));
        }
    }
}
