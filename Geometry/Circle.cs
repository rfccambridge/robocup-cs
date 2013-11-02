using System;
using System.Collections.Generic;
using System.Text;

namespace RFC.Geometry
{
    public class Circle : Geom
    {
        private double radius;
        private Vector2 center;

        public Vector2 Center
        {get { return center; }}

        public double Radius
        {get { return radius; }}

        /// <summary>
        /// Creates a circle with the given center and radius
        /// </summary>
        public Circle(double xCenter, double yCenter, double radius)
        {
            this.center = new Vector2(xCenter,yCenter);
            this.radius = radius;
        }

        /// <summary>
        /// Creates a circle with the given center and radius
        /// </summary>
        public Circle(Vector2 center, double radius)
        {
            this.center = center;
            this.radius = radius;
        }

        /// <summary>
        /// Creates a unit circle at the origin
        /// </summary>
        public Circle()
        {
            this.center = new Vector2();
            this.radius = 1.0;
        }

        /// <summary>
        /// Returns a circle translated by the added vector
        /// </summary>
        public static Circle operator +(Circle c, Vector2 v)
        {
            return new Circle(c.center + v, c.radius);
        }

        /// <summary>
        /// Returns a circle translated by the added vector
        /// </summary>
        public static Circle operator +(Vector2 v, Circle c)
        {
            return new Circle(v + c.center, c.radius);
        }

        /// <summary>
        /// Returns a circle translated by the negative of the vector
        /// </summary>
        public static Circle operator -(Circle c, Vector2 v)
        {
            return new Circle(c.center - v, c.radius);
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
            return new Circle(center.rotateAroundPoint(p, angle), radius);
        }
        Geom Geom.rotateAroundPoint(Vector2 p, double angle)
        { return rotateAroundPoint(p, angle); }

        /// <summary>
        /// Checks if this circle contains the given point. Points on the boundary are considered contained.
        /// </summary>
        public bool contains(Vector2 p)
        {
            return p.distanceSq(center) <= radius * radius;
        }


        public override string ToString()
        {
            return "Circle(" + center + ", " + radius + ")";
        }

    }
}
