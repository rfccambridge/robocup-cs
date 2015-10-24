using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RFC.Geometry
{
    /// <summary>
    /// A robot shape. Circular arc, with a flat front part.
    /// </summary>
    public class RobotShape : GeomBase<RobotShape>
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
        public RobotShape(Point2 center, double radius, double orientation, double frontPlateRadius)
        {
            double angle = Math.Acos(frontPlateRadius / radius);

            this.Arc = new Arc(center, radius, orientation - angle, orientation + angle);
            this.Segment = new LineSegment(Arc.StartPt, Arc.StopPt);
        }

        /// <summary>
        /// Returns the translation of this line segment by the given vector.
        /// </summary>
        public override RobotShape translate(Vector2 v)
        {
            return new RobotShape(Arc.translate(v), Segment.translate(v));
        }

        /// <summary>
        /// Returns a line segment that is this line rotated a given number of radians in the
        /// counterclockwise direction around p.
        /// </summary>
        public override RobotShape rotateAroundPoint(Point2 p, double angle)
        {
            return new RobotShape(Arc.rotateAroundPoint(p, angle), Segment.rotateAroundPoint(p, angle));
        }

        public override string ToString()
        {
            return "RobotShape[" + Arc + ", " + Segment + "]";
        }
        
        public override double distance(Point2 p)
        {
            return Math.Min(Arc.distance(p), Segment.distance(p));
        }
    }
}
