using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RFC.Geometry
{
    /// <summary>
    /// A line segment.
    /// </summary>
    public class LineSegment : Geom
    {
        private Line l;

        /// <summary>
        /// Creates a line segment extending from p0 to p1
        /// </summary>
        public LineSegment(Vector2 p0, Vector2 p1)
        {
            this.l = new Line(p0, p1);
        }

        /// <summary>
        /// Creates a line segment extending from p0 in the desired direction in radians.
        /// </summary>
        public LineSegment(Vector2 p0, double direction)
        {
            this.l = new Line(p0, direction);
        }

        /// <summary>
        /// Creates a line segment defined by the same points as the given line
        /// </summary>
        public LineSegment(Line l)
        {
            this.l = l;
        }

        /// <summary>
        /// The starting point defining this line segment
        /// </summary>
        public Vector2 P0
        { get { return l.P0; } }

        /// <summary>
        /// The ending point defining this line segment
        /// </summary>
        public Vector2 P1
        { get { return l.P1; } }

        /// <summary>
        /// The vector from the starting point to the ending point of this line segment
        /// </summary>
        public Vector2 Direction
        { get { return l.Direction; } }

        /// <summary>
        /// The infinitely extended line defined by the same points as this line segment
        /// </summary>
        public Line Line
        { get { return l; } }


        /// <summary>
        /// Returns a line segment with the start and end reversed.
        /// </summary>
        public static LineSegment operator -(LineSegment seg)
        {
            return new LineSegment(-(seg.l));
        }

        /// <summary>
        /// Returns a line segment translated by the added vector
        /// </summary>
        public static LineSegment operator +(LineSegment l, Vector2 v)
        {
            return new LineSegment(l.l + v);
        }

        /// <summary>
        /// Returns a line segment translated by the added vector
        /// </summary>
        public static LineSegment operator +(Vector2 v, LineSegment l)
        {
            return new LineSegment(v + l.l);
        }

        /// <summary>
        /// Returns a line segment translated by the negative of the vector
        /// </summary>
        public static LineSegment operator -(LineSegment l, Vector2 v)
        {
            return new LineSegment(l.l - v);
        }

        /// <summary>
        /// The length of this line segment
        /// </summary>
        public double length()
        { return l.P1.distance(l.P0); }

        /// <summary>
        /// The squared length of this line segment
        /// </summary>
        public double lengthSq()
        { return l.P1.distanceSq(l.P0); }

        /// <summary>
        /// Computes the minimum distance between this line segment and a point
        /// </summary>
        public double distance(Vector2 p)
        {
            if (Vector2.dotproduct(l.P0, l.P1, p) < 0)
                return l.P1.distance(p);

            else if (Vector2.dotproduct(l.P1, l.P0, p) < 0)
                return l.P0.distance(p);

            return l.distance(p);
        }

        /// <summary>
        /// Returns the translation of this line segment by the given vector.
        /// </summary>
        public LineSegment translate(Vector2 v)
        {
            return this + v;
        }
        Geom Geom.translate(Vector2 v)
        { return translate(v); }

        /// <summary>
        /// Returns a line segment that is this line rotated a given number of radians in the
        /// counterclockwise direction around p.
        /// </summary>
        public LineSegment rotateAroundPoint(Vector2 p, double angle)
        {
            return new LineSegment(l.rotateAroundPoint(p, angle));
        }
        Geom Geom.rotateAroundPoint(Vector2 p, double angle)
        { return rotateAroundPoint(p, angle); }


        public override string ToString()
        {
            return "LineSegment(" + l.P0 + ", " + l.P1 + ")";
        }
    }

}
