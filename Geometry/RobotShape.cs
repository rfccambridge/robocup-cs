using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RFC.Geometry
{
    /// <summary>
    /// A robot shape. Circular arc, with a flat front part.
    /// </summary>
    public class RobotShape : Geom
    {
        private Arc arc;
        private LineSegment segment;

        public Arc Arc
        { get { return arc; } }

        public LineSegment Segment
        { get { return segment; } }

        private RobotShape(Arc arc, LineSegment segment)
        {
            this.arc = arc;
            this.segment = segment;
        }

        /// <summary>
        /// Creates a robot shape. 
        /// </summary>
        public RobotShape(Vector2 center, double radius, double orientation, double frontPlateRadius)
        {
            double angle = Math.Acos(frontPlateRadius / radius);

            this.arc = new Arc(center, radius, orientation - angle, orientation + angle);
            this.segment = new LineSegment(arc.StartPt, arc.StopPt);
        }

        /// <summary>
        /// Returns a robot shape translated by the added vector
        /// </summary>
        public static RobotShape operator +(RobotShape rs, Vector2 v)
        {
            return new RobotShape(rs.arc + v, rs.segment + v);
        }

        /// <summary>
        /// Returns a line segment translated by the added vector
        /// </summary>
        public static RobotShape operator +(Vector2 v, RobotShape rs)
        {
            return new RobotShape(v + rs.arc, v + rs.segment);
        }

        /// <summary>
        /// Returns a line segment translated by the negative of the vector
        /// </summary>
        public static RobotShape operator -(RobotShape rs, Vector2 v)
        {
            return new RobotShape(rs.arc - v, rs.segment - v);
        }

        /// <summary>
        /// Returns the translation of this line segment by the given vector.
        /// </summary>
        public RobotShape translate(Vector2 v)
        {
            return this + v;
        }
        Geom Geom.translate(Vector2 v)
        { return translate(v); }

        /// <summary>
        /// Returns a line segment that is this line rotated a given number of radians in the
        /// counterclockwise direction around p.
        /// </summary>
        public RobotShape rotateAroundPoint(Vector2 p, double angle)
        {
            return new RobotShape(arc.rotateAroundPoint(p, angle), segment.rotateAroundPoint(p, angle));
        }
        Geom Geom.rotateAroundPoint(Vector2 p, double angle)
        { return rotateAroundPoint(p, angle); }


        public override string ToString()
        {
            return "RobotShape[" + arc + ", " + segment + "]";
        }
    }
}
