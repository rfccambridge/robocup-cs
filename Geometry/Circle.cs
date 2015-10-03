using System;
using System.Collections.Generic;
using System.Text;

namespace RFC.Geometry
{
    public class Circle : AreaGeom
    {
        public Vector2 Center { get; private set; }
        public double Radius { get; private set; }

        /// <summary>
        /// Creates a circle with the given center and radius
        /// </summary>
        public Circle(double xCenter, double yCenter, double radius)
        {
            this.Center = new Vector2(xCenter,yCenter);
            this.Radius = radius;
        }

        /// <summary>
        /// Creates a circle with the given center and radius
        /// </summary>
        public Circle(Vector2 center, double radius)
        {
            this.Center = center;
            this.Radius = radius;
        }

        /// <summary>
        /// Creates a unit circle at the origin
        /// </summary>
        public Circle()
        {
            this.Center = new Vector2();
            this.Radius = 1.0;
        }

        /// <summary>
        /// Returns a circle translated by the added vector
        /// </summary>
        public static Circle operator +(Circle c, Vector2 v)
        {
            return new Circle(c.Center + v, c.Radius);
        }

        /// <summary>
        /// Returns a circle translated by the added vector
        /// </summary>
        public static Circle operator +(Vector2 v, Circle c)
        {
            return new Circle(v + c.Center, c.Radius);
        }

        /// <summary>
        /// Returns a circle translated by the negative of the vector
        /// </summary>
        public static Circle operator -(Circle c, Vector2 v)
        {
            return new Circle(c.Center - v, c.Radius);
        }

        /// <summary>
        /// Returns the translation of this circle by the given vector.
        /// </summary>
        public Circle translate(Vector2 v)
        {
            return this + v;
        }
        Geom Geom.translate(Vector2 v)
        { return translate(v); }

        /// <summary>
        /// Returns a circle that is this circle rotated a given number of radians in the
        /// counterclockwise direction around p.
        /// </summary>
        public Circle rotateAroundPoint(Vector2 p, double angle)
        {
            return new Circle(Center.rotateAroundPoint(p, angle), Radius);
        }
        Geom Geom.rotateAroundPoint(Vector2 p, double angle)
        { return rotateAroundPoint(p, angle); }

        /// <summary>
        /// Checks if this circle contains the given point. Points on the boundary are considered contained.
        /// </summary>
        public bool contains(Vector2 p)
        {
            return p.distanceSq(Center) <= Radius * Radius;
        }


        public override string ToString()
        {
            return "Circle(" + Center + ", " + Radius + ")";
        }

    }
}
