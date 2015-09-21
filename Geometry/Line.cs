using System;
using System.Collections.Generic;
using System.Text;

namespace RFC.Geometry
{
    /// <summary>
    /// A line. Note that although the line is defined by 2 points, a starting and an ending point, 
    /// for intersections and distances, the line is treated as infinitely extended in both directions.
    /// For a line segment, see LineSegment.
    /// </summary>
    public class Line : Geom
    {
        /// <summary>
        /// The starting point defining this line
        /// </summary>
        public Vector2 P0 { get; private set; }

        /// <summary>
        /// The ending point defining this line
        /// </summary>
        public Vector2 P1 { get; private set; }

        /// <summary>
        /// Creates a line extending from p0 to p1
        /// </summary>
        public Line(Vector2 p0, Vector2 p1)
        {
            this.P0 = p0;
            this.P1 = p1;
        }

        /// <summary>
        /// Creates a line extending from p0 in the desired direction in radians.
        /// </summary>
        public Line(Vector2 p0, double direction)
        {
            this.P0 = p0;
            this.P1 = p0 + Vector2.GetUnitVector(direction);
        }


        /// <summary>
        /// The vector from the starting point to the ending point of this line
        /// </summary>
        public Vector2 Direction
        { get { return P1 - P0; } }

        /// <summary>
        /// The finite line segment defined by the same points as this line
        /// </summary>
        public LineSegment Segment
        { get { return new LineSegment(this); } }

        /// <summary>
        /// Returns a line with the start and end reversed.
        /// </summary>
        public static Line operator -(Line l)
        {
            return new Line(l.P1, l.P0);
        }

        /// <summary>
        /// Returns a line translated by the added vector
        /// </summary>
        public static Line operator +(Line l, Vector2 v)
        {
            return new Line(l.P0 + v, l.P1 + v);
        }

        /// <summary>
        /// Returns a line translated by the added vector
        /// </summary>
        public static Line operator +(Vector2 v, Line l)
        {
            return new Line(v + l.P0, v + l.P1);
        }

        /// <summary>
        /// Returns a line translated by the negative of the vector
        /// </summary>
        public static Line operator -(Line l, Vector2 v)
        {
            return new Line(l.P0 - v, l.P1 - v);
        }

        /// <summary>
        /// Computes the minimum distance between this line and a point
        /// </summary>
        public double distance(Vector2 p)
        {
            double mag = P1.distance(P0);
            double crossp = Vector2.crossproduct(P0, P1, p);
            double dist = crossp / mag;
            return Math.Abs(dist);
        }

        /// <summary>
        /// Computes the minimum signed distance between this line and a point, where
        /// (facing from p0 -> p1) positive indicates a point to the left of the line 
        /// and negative indicates a point to the right of the line.
        /// </summary>
        public double signedDistance(Vector2 p)
        {
            double mag = P1.distance(P0);
            double crossp = Vector2.crossproduct(P1, P0, p);
            double dist = crossp / mag;
            return dist;
        }

        /// <summary>
        /// Computes the point on the line that that is the closest to the given point
        /// </summary>
        public Vector2 closestPointTo(Vector2 p)
        {
            return projectionOntoLine(p);
        }

        /// <summary>
        /// Computes the point on the line that would result from projecting
        /// p perpendicularly towards the line.
        /// </summary>
        public Vector2 projectionOntoLine(Vector2 p)
        {
            Vector2 tangent = (P1 - P0).normalize();
            double dotp = tangent * (p - P0);
            return P0 + dotp * tangent;
        }

        /// <summary>
        /// Computes the distance of the projection of p from the starting point of this line.
        /// </summary>
        public double distanceAlongLine(Vector2 p)
        {
            return (p - P0).projectionLength(Direction);
        }

        /// <summary>
        /// Returns the translation of this line by the given vector.
        /// </summary>
        public Line translate(Vector2 v)
        {
            return this + v;
        }
        Geom Geom.translate(Vector2 v)
        {return translate(v);}

        /// <summary>
        /// Returns a line that is this line rotated a given number of radians in the
        /// counterclockwise direction around p.
        /// </summary>
        public Line rotateAroundPoint(Vector2 p, double angle)
        {
            return new Line(P0.rotateAroundPoint(p, angle), P1.rotateAroundPoint(p, angle));
        }
        Geom Geom.rotateAroundPoint(Vector2 p, double angle)
        { return rotateAroundPoint(p, angle); }

        /// <summary>
        /// Checks if this line is parallel OR antiparallel to l.
        /// </summary>
        public bool isParallelTo(Line l)
        {
            Vector2 dir = Direction;
            Vector2 dir2 = l.Direction;
            double dot = dir * dir2;
            return dot * dot >= dir.magnitudeSq() * dir2.magnitudeSq();
        }

        /// <summary>
        /// Checks if a point lies on the given line.
        /// Note: floating point imprecision may be a problem here!
        /// </summary>
        ///         
        public bool contains(Vector2 p)
        {
            return signedDistance(p) == 0;
        }

        public override string ToString()
        {
            return "Line(" + P0 + ", " + P1 + ")";
        }
    }

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


