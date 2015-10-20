using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace RFC.Geometry
{
    /// <summary>
    /// An immutable class that represents a point in 2D space, or a vector in 2D space.
    /// </summary>
    [Serializable]
    public class Vector2 : IEquatable<Vector2>
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
        public Vector2() : this(0, 0) { }
        
        /// <summary>
        /// Creates a new Vector2
        /// </summary>
        /// <param name="x">the x-coordinate</param>
        /// <param name="y">the y-coordinate</param>
        public Vector2(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }
        
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
        static public Vector2 GetUnitVector(double orientation)
        {
            return new Vector2(Math.Cos(orientation), Math.Sin(orientation));
        }

        /// <summary>
        /// Checks for equality between two Vector2's.  Since the class is
        /// immutable, does value-equality.
        /// </summary>
        public static bool operator ==(Vector2 p1, Vector2 p2)
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
        public static bool operator !=(Vector2 p1, Vector2 p2)
        {
            return !(p1 == p2);
        }

        /// <summary>
        /// Checks for value equality between this and another object.  Returns
        /// false if the other object is null or not a Vector2.
        /// </summary>
        public override bool Equals(object obj)
        {
            Vector2 v = obj as Vector2;
            if (v == null)
                return false;
            return (X == v.X && Y == v.Y);
        }
        
        /// <summary>
        /// Checks for value equality between this and another object.  Returns
        /// false if the other object is null.
        /// </summary>
        public bool Equals(Vector2 obj)
        {
            return this == obj;
        }
        
        /// <summary>
        /// Returns a hash code of this Vector2.
        /// </summary>
        public override int GetHashCode()
        {
            return 43 * X.GetHashCode() + 37 * Y.GetHashCode();
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
        /// Returns the squared distance between this point and another point.
        /// Returns the same value (within tolerance) as (p1-p2).magnitudeSq()
        /// </summary>
        public double distanceSq(Vector2 p2)
        {
            if (p2 == null)
                return double.PositiveInfinity;
    
            return (X - p2.X) * (X - p2.X) + (Y - p2.Y) * (Y - p2.Y);
        }
        
        /// <summary>
        /// Returns the distance between this point and another point.
        /// Returns the same value (within tolerance) as (p1-p2).magnitude()
        /// </summary>
        public double distance(Vector2 p2)
        {
            if (p2 == null)
                return double.PositiveInfinity;

            return Math.Sqrt((X - p2.X) * (X - p2.X) + (Y - p2.Y) * (Y - p2.Y));
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
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
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
        /// Returns the translation of this point by the given vector.
        /// </summary>
        public Vector2 translate(Vector2 v)
        {
            return this + v;
        }

        /// <summary>
        /// Returns a point that is this point rotated a given number of radians in the
        /// counterclockwise direction around p.
        /// </summary>
        public Vector2 rotateAroundPoint(Vector2 p, double angle)
        {
            return (this - p).rotate(angle) + p;
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
        /// </summary>
        static public double crossproduct(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (p1.X - p2.X) * (p3.Y - p2.Y) - (p3.X - p2.X) * (p1.Y - p2.Y);
        }
        /// <summary>
        /// Returns the dotproduct of P2P1 and P2P3
        /// </summary>
        static public double dotproduct(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (p1.X - p2.X) * (p3.X - p2.X) + (p1.Y - p2.Y) * (p3.Y - p2.Y);
        }

        /// <summary>
        /// Provides a string representation of this Vector2.
        /// </summary>
        public override string ToString()
        {
            return String.Format("<{0:G4},{1:G4}>", X, Y);
        }

        /// <summary>
        /// Parses a Vector2 from the string format of ToString().  There is not much guarantee about
        /// how constant the string representation will be, however.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        static public Vector2 Parse(string s)
        {
            string[] split = s.Trim('<', '>', ' ').Split(',');
            if (split.Length != 2)
                throw new FormatException("invalid format for Vector2: " + s);
            return new Vector2(double.Parse(split[0]), double.Parse(split[1]));
        }
    }
}
