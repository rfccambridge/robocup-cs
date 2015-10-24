using System;
using System.Collections.Generic;
using System.Text;

namespace RFC.Geometry
{
    public class Circle : GeomBase<Circle>
    {
        public Point2 Center { get; private set; }
        public double Radius { get; private set; }

        /// <summary>
        /// Creates a circle with the given center and radius
        /// </summary>
        public Circle(double xCenter, double yCenter, double radius)
        {
            this.Center = new Point2(xCenter,yCenter);
            this.Radius = radius;
        }

        /// <summary>
        /// Creates a circle with the given center and radius
        /// </summary>
        public Circle(Point2 center, double radius)
        {
            this.Center = center;
            this.Radius = radius;
        }

        /// <summary>
        /// Creates a unit circle at the origin
        /// </summary>
        public Circle()
        {
            this.Center = new Point2(0, 0);
            this.Radius = 1.0;
        }

        /// <summary>
        /// Returns the translation of this circle by the given vector.
        /// </summary>
        public override Circle translate(Vector2 v) => new Circle(Center + v, Radius);

        /// <summary>
        /// Returns a circle that is this circle rotated a given number of radians in the
        /// counterclockwise direction around p.
        /// </summary>
        public override Circle rotateAroundPoint(Point2 p, double angle)
        {
            return new Circle(Center.rotateAroundPoint(p, angle), Radius);
        }

        /// <summary>
        /// Checks if this circle contains the given point. Points on the boundary are considered contained.
        /// </summary>
        public override bool contains(Point2 p)
        {
            return p.distanceSq(Center) <= Radius * Radius;
        }

        public override double distance(Point2 p)
        { 
            return p.distance(Center) - Radius;
        }

        public override string ToString()
        {
            return "Circle(" + Center + ", " + Radius + ")";
        }

    }
}
