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
        private Vector2 p0;
        private Vector2 p1;

        /// <summary>
        /// Creates a line extending from p0 to p1
        /// </summary>
        public Line(Vector2 p0, Vector2 p1)
        {
            this.p0 = p0;
            this.p1 = p1;
        }

        /// <summary>
        /// Creates a line extending from p0 in the desired direction in radians.
        /// </summary>
        public Line(Vector2 p0, double direction)
        {
            this.p0 = p0;
            this.p1 = p0 + Vector2.GetUnitVector(direction);
        }

        /// <summary>
        /// The starting point defining this line
        /// </summary>
        public Vector2 P0
        { get { return p0; } }

        /// <summary>
        /// The ending point defining this line
        /// </summary>
        public Vector2 P1
        { get { return p1; } }

        /// <summary>
        /// The vector from the starting point to the ending point of this line
        /// </summary>
        public Vector2 Direction
        { get { return p1 - p0; } }

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
            return new Line(l.p1, l.p0);
        }

        /// <summary>
        /// Returns a line translated by the added vector
        /// </summary>
        public static Line operator +(Line l, Vector2 v)
        {
            return new Line(l.p0 + v, l.p1 + v);
        }

        /// <summary>
        /// Returns a line translated by the added vector
        /// </summary>
        public static Line operator +(Vector2 v, Line l)
        {
            return new Line(v + l.p0, v + l.p1);
        }

        /// <summary>
        /// Returns a line translated by the negative of the vector
        /// </summary>
        public static Line operator -(Line l, Vector2 v)
        {
            return new Line(l.p0 - v, l.p1 - v);
        }

        /// <summary>
        /// Computes the minimum distance between this line and a point
        /// </summary>
        public double distance(Vector2 p)
        {
            double mag = p1.distance(p0);
            double crossp = Vector2.crossproduct(p0, p1, p);
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
            double mag = p1.distance(p0);
            double crossp = Vector2.crossproduct(p1, p0, p);
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
            Vector2 tangent = (p1 - p0).normalize();
            double dotp = tangent * (p - p0);
            return p0 + dotp * tangent;
        }

        /// <summary>
        /// Computes the distance of the projection of p from the starting point of this line.
        /// </summary>
        public double distanceAlongLine(Vector2 p)
        {
            return (p - p0).projectionLength(Direction);
        }

        /// <summary>
        /// Returns the translation of this line by the given vector.
        /// </summary>
        public Line translate(Vector2 v)
        {
            return this + v;
        }
        Geom Geom.translate(Vector2 v)
        { return translate(v); }

        /// <summary>
        /// Returns a line that is this line rotated a given number of radians in the
        /// counterclockwise direction around p.
        /// </summary>
        public Line rotateAroundPoint(Vector2 p, double angle)
        {
            return new Line(p0.rotateAroundPoint(p, angle), p1.rotateAroundPoint(p, angle));
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
            return "Line(" + p0 + ", " + p1 + ")";
        }
    }

}