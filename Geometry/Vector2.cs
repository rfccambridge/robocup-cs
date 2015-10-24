using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace RFC.Geometry
{
    /// <summary>
    /// An immutable cartesian pair of (X, Y), with no prescribed use
    /// </summary>
    public abstract class CartesianPair
    {
        /// <summary>
        /// The x-coordinate of this vector
        /// </summary>
        public double X { get; }

        /// <summary>
        /// The y-coordinate of this vector
        /// </summary>
        public double Y { get; }

        /// <summary>
        /// Creates a zero Vector2
        /// </summary>
        public CartesianPair() : this(0, 0) { }

        /// <summary>
        /// Creates a new Vector2
        /// </summary>
        /// <param name="x">the x-coordinate</param>
        /// <param name="y">the y-coordinate</param>
        internal CartesianPair(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }

        /// <summary>
        /// Provides a string representation of this Vector2.
        /// </summary>
        public override string ToString()
        {
            return String.Format("<{0:G4},{1:G4}>", X, Y);
        }
    }

    /// <summary>
    /// A base class to implement equality within a type of cartesian pair
    /// This is separate from the non-generic class to ensure that:
    /// 
    ///     new Vector2(1, 2) != new Point2(1, 2)
    /// 
    /// Is a compile-time error, as points are not meaninfully comparable to vectors
    /// </summary>
    public abstract class CartesianPair<T> : CartesianPair, IEquatable<T> where T : CartesianPair<T>
    {
        public CartesianPair(double x, double y) : base(x, y) { }

        /// <summary>
        /// Checks for equality between two Vector2's.  Since the class is
        /// immutable, does value-equality.
        /// </summary>
        public static bool operator ==(CartesianPair<T> p1, CartesianPair<T> p2)
        {
            if (object.ReferenceEquals(p1, null))
                return (object.ReferenceEquals(p2, null));
            else if (object.ReferenceEquals(p2, null))
                return (object.ReferenceEquals(p1, null));
            return (p1.X == p2.X && p1.Y == p2.Y);
        }

        /// <summary>
        /// Checks for inequality between two Vector2's.  Since the class is
        /// immutable, does value-equality.
        /// </summary>
        public static bool operator !=(CartesianPair<T> p1, CartesianPair<T> p2)
        {
            return !(p1 == p2);
        }

        /// <summary>
        /// Checks for value equality between this and another object.  Returns
        /// false if the other object is null or not a Vector2.
        /// </summary>
        public override bool Equals(object obj)
        {
            CartesianPair<T> v = obj as CartesianPair<T>;
            if (v == null)
                return false;
            return (X == v.X && Y == v.Y);
        }

        /// <summary>
        /// Checks for value equality between this and another object.  Returns
        /// false if the other object is null.
        /// </summary>
        public bool Equals(T obj)
        {
            return this == obj;
        }

        /// <summary>
        /// Returns a hash code of this CartesianPair.
        /// </summary>
        public override int GetHashCode()
        {
            return typeof(T).GetHashCode() + 43 * X.GetHashCode() + 37 * Y.GetHashCode();
        }
    }

    /// <summary>
    /// A point, ie location, in 2D space
    /// </summary>
    [Serializable]
    public class Point2 : CartesianPair<Point2>, IGeom<Point2>
    {
        public Point2(double x, double y) : base(x, y) { }

        public static Point2 ORIGIN { get; } = new Point2(0, 0);
        /// <summary>returns the original point translated by a vector</summary>
        static public Point2  operator +(Point2 p1, Vector2 p2) => new Point2(p1.X + p2.X, p1.Y + p2.Y);
        /// <summary>returns the original point translated by a vector</summary>
        static public Point2  operator +(Vector2 p1, Point2 p2) => new Point2(p1.X + p2.X, p1.Y + p2.Y);

        /// <summary>returns the original point untranslated by a vector</summary>
        static public Point2  operator -(Point2 p1, Vector2 p2) => new Point2(p1.X - p2.X, p1.Y - p2.Y);
        /// <summary>returns the vector from the second point to the first point</summary>
        static public Vector2 operator -(Point2 p1, Point2 p2) => new Vector2(p1.X - p2.X, p1.Y - p2.Y);

        /// <summary>
        /// Returns the distance between this point and another point.
        /// Returns the same value (within tolerance) as (p1-p2).magnitude()
        /// </summary>
        public double distance(Point2 p2)
        {
            if (p2 == null)
                return double.PositiveInfinity;

            return Math.Sqrt((X - p2.X) * (X - p2.X) + (Y - p2.Y) * (Y - p2.Y));
        }

        /// <summary>
        /// Returns a point that is this point rotated a given number of radians in the
        /// counterclockwise direction around p.
        /// </summary>
        public Point2 rotateAroundPoint(Point2 p, double angle)
        {
            return (this - p).rotate(angle) + p;
        }

        /// <summary>
        /// Returns the squared distance between this point and another point.
        /// Returns the same value (within tolerance) as (p1-p2).magnitudeSq()
        /// </summary>
        public double distanceSq(Point2 p2)
        {
            if (p2 == null)
                return double.PositiveInfinity;

            return (X - p2.X) * (X - p2.X) + (Y - p2.Y) * (Y - p2.Y);
        }

        /// <summary>
        /// Linearly interpolate between this point and the target
        /// </summary>
        /// <param name="f">The amount to interpolate. 0 results in the first point being
        /// returned, 1 resutls in the second being returned</param>
        public Point2 lerp(Point2 other, double f)
        {
            return new Point2(X * (1 - f) + other.X * f, Y * (1 - f) + other.Y * f);
        }

        // Implement IGeom
        Point2 IGeom<Point2>.translate(Vector2 v) => this + v;
        IGeom IGeom.translate(Vector2 v) => this + v;
        IGeom IGeom.rotateAroundPoint(Point2 p, double angle) => rotateAroundPoint(p, angle);
        bool IGeom.contains(Point2 target) => this == target;
    }

    /// <summary>
    /// An immutable class that represents a vector in 2D space. Used to represent
    /// * Directions
    /// * Distances between two Point2s
    /// * Velocities
    /// </summary>
    [Serializable]
    public class Vector2 : CartesianPair<Vector2>
    {
        /// <summary>
        /// Creates a zero Vector2
        /// </summary>
        public Vector2() : this(0, 0) { }

        /// <summary>
        /// Creates a new Vector2
        /// </summary>
        /// <param name="x">the x-coordinate</param>
        /// <param name="y">the y-coordinate</param>
        public Vector2(double x, double y) : base(x, y) { }
        
        public PointF ToPointF()
        {
            return new PointF((float)X, (float)Y);
        }
        static public explicit operator Vector2(Point p)
        {
            return new Vector2(p.X, p.Y);
        }
        
        /// <summary>
        /// The Vector (0,0)
        /// </summary>
        static public Vector2 ZERO { get; } = new Vector2(0, 0);

        /// <summary>
        /// Gets a unit vector in the desired orientation, in radians
        /// </summary>
        /// <param name="orientation">The counter-clockwise angle from the vector [1, 0]</param>
        static public Vector2 GetUnitVector(double orientation)
        {
            return new Vector2(Math.Cos(orientation), Math.Sin(orientation));
        }
        
        /// <summary>
        /// Returns the square of the length of this vector.
        /// </summary>
        public double magnitudeSq()
        {
            return X * X + Y * Y;
        }

        /// <summary>
        /// Returns the the length of this vector.
        /// </summary>
        public double magnitude()
        {
            return Math.Sqrt(X * X + Y * Y);
        }

        /// <summary>
        /// Returns the angle, in radians, that you must rotate the
        /// vector (1,0) in the counter-clockwise direction until
        /// it points in the same direction as this Vector2.
        /// </summary>
        public double cartesianAngle()
        {
            return Math.Atan2(Y, X);
        }

        /// <summary>
        /// Adds two Vector2's and returns the result.  Addition is done
        /// component by component.
        /// </summary>
        static public Vector2 operator +(Vector2 p1, Vector2 p2)
        {
            return new Vector2(p1.X + p2.X, p1.Y + p2.Y);
        }
        
        /// <summary>
        /// Subtracts two Vector2's and returns the result.  Subtraction is done
        /// component by component.
        /// </summary>
        static public Vector2 operator -(Vector2 p1, Vector2 p2)
        {
            return new Vector2(p1.X - p2.X, p1.Y - p2.Y);
        }
        
        /// <summary>
        /// Returns the negation of this vector.
        /// </summary>
        static public Vector2 operator -(Vector2 p)
        {
            return new Vector2(-p.X, -p.Y);
        }
        

        /// <summary>
        /// Returns the dot product of two Vector2's
        /// </summary>
        static public double operator *(Vector2 p1, Vector2 p2)
        {
            return p1.X * p2.X + p1.Y * p2.Y;
        }
        
        /// <summary>
        /// Returns this vector scaled by a constant.
        /// </summary>
        static public Vector2 operator *(double f, Vector2 p)
        {
            return new Vector2(p.X * f, p.Y * f);
        }

        /// <summary>
        /// Returns this vector scaled by a constant.
        /// </summary>
        static public Vector2 operator *(Vector2 p, double f)
        {
            return new Vector2(p.X * f, p.Y * f);
        }

        /// <summary>
        /// Returns this vector divided by a constant.
        /// </summary>
        static public Vector2 operator /(Vector2 p, double f)
        {
            return new Vector2(p.X / f, p.Y / f);
        }

        /// <summary>
        /// Cross product
        /// </summary>
        static public double cross(Vector2 v1, Vector2 v2)
        {
            return v1.X * v2.Y - v1.Y * v2.X;
        }

        /// <summary>
        /// Returns a Vector2 that is parallel to this Vector2 and has length 1.
        /// Has no meaning for the zero Vector2, (and possibly vectors extremely close to zero?)
        /// and will return the existing vector unchanged.
        /// </summary>
        public Vector2 normalize()
        {
            double lengthSq = magnitudeSq();
            if (lengthSq == 0)
                return this;
            
            return (1 / Math.Sqrt(magnitudeSq())) * this;
        }
        
        /// <summary>
        /// Returns a vector in the same direction as this one, with the desired length.
        /// For the zero vector (and possibly vectors extremely close to zero), returns the
        /// existing vector, multiplied by newLength
        /// </summary>
        public Vector2 normalizeToLength(double newLength)
        {
            return newLength * (this.normalize());
        }
        
        /// <summary>
        /// Returns a vector that is this vector rotated a given number of radians in the
        /// counterclockwise direction.
        /// </summary>
        public Vector2 rotate(double angle)
        {
            double c = Math.Cos(angle);
            double s = Math.Sin(angle);
            return new Vector2(c * X - s * Y, c * Y + s * X); 
        }
        
        /// <summary>
        /// Returns a vector that is this vector rotated 1/4 of a turn in the
        /// counterclockwise direction.
        /// </summary>
        public Vector2 rotatePerpendicular()
        {
            return new Vector2(-Y, X);
        }

        /// <summary>
        /// Returns the length of this vector when projected on to v
        /// Returns an unspecified result if v is the zero vector.
        /// </summary>
        public double projectionLength(Vector2 v)
        {
            return this * v / v.magnitude();
        }

        /// <summary>
        /// Returns the component of this vector parallel to v
        /// Returns an unspecified result if v is the zero vector.
        /// </summary>
        public Vector2 parallelComponent(Vector2 v)
        {
            return (this * v / v.magnitudeSq()) * v;
        }

        /// <summary>
        /// Returns the component of this vector perpendicular to v
        /// Returns an unspecified result if v is the zero vector.
        /// </summary>
        public Vector2 perpendicularComponent(Vector2 v)
        {
            v = v.rotatePerpendicular();
            return (this * v / v.magnitudeSq()) * v;
        }

        /// <summary>
        /// Returns this vector mirror-reflected over v.
        /// If v is the zero vector, returns an unspecified result.
        /// </summary>
        public Vector2 reflectOver(Vector2 v)
        {
            return this - 2.0 * perpendicularComponent(v);
        }

        /// <summary>
        /// Returns the cosine of the angle between this vector and v,
        /// using the relationship cos(angle(v1,v2)) = (v1*v2) / (|v1||v2|)
        /// </summary>
        public double cosineAngleWith(Vector2 v)
        {
            return (this * v) / Math.Sqrt(this.magnitudeSq() * v.magnitudeSq());
        }

        /// <summary>
        /// Returns the z-component of the crossproduct P2P1 x P2P3
        /// Equivalent to <code>Vector2.cross(p1 - p2, p3 - p2)</code>
        /// </summary>
        static public double crossproduct(Point2 p1, Point2 p2, Point2 p3)
        {
            return (p1.X - p2.X) * (p3.Y - p2.Y) - (p3.X - p2.X) * (p1.Y - p2.Y);
        }
        /// <summary>
        /// Returns the dot product of P2P1 and P2P3
        /// Equivalent to (p1 - p2) * (p3 - p2)
        /// </summary>
        static public double dotproduct(Point2 p1, Point2 p2, Point2 p3)
        {
            return (p1.X - p2.X) * (p3.X - p2.X) + (p1.Y - p2.Y) * (p3.Y - p2.Y);
        }

        /// <summary>
        /// Parses a Vector2 from the string format of ToString().  There is not much guarantee about
        /// how constant the string representation will be, however.
        /// </summary>
        static public Vector2 Parse(string s)
        {
            string[] split = s.Trim('<', '>', ' ').Split(',');
            if (split.Length != 2)
                throw new FormatException("invalid format for Vector2: " + s);
            return new Vector2(double.Parse(split[0]), double.Parse(split[1]));
        }
    }
}
