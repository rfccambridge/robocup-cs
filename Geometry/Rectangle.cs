using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RFC.Geometry
{
    /// <summary>
    /// A half-plane, defined by a line. The half of the plane that is part of this object is the right side
    /// of the plane, when facing along the direction of the line. 
    /// A half-plane does not contain the points on the boundary itself.
    /// </summary>
    public class Rectangle : Geom
    {
        double xMin;
        double xMax;
        double yMin;
        double yMax;

        /// <summary>
        /// Constructs the rectangle with the given boundaries
        /// </summary>
        public Rectangle(double xMin, double xMax, double yMin, double yMax)
        {
            this.xMin = xMin;
            this.xMax = xMax;
            this.yMin = yMin;
            this.yMax = yMax;
        }

        /// <summary>
        /// Constructs the rectangle beginning at the given point with the given width and height
        /// </summary>
        public Rectangle(Vector2 p, double width, double height)
        {
            if (width >= 0)
            {
                xMin = p.X;
                xMax = p.X + width;
            }
            else
            {
                xMin = p.X + width;
                xMax = p.X;
            }

            if (height >= 0)
            {
                yMin = p.Y;
                yMax = p.Y + height;
            }
            else
            {
                yMin = p.Y + height;
                yMax = p.Y;
            }
        }

        /// <summary>
        /// Gets the left boundary of this rectangle
        /// </summary>
        public double XMin { get { return xMin; } }

        /// <summary>
        /// Gets the right boundary of this rectangle
        /// </summary>
        public double XMax { get { return xMax; } }

        /// <summary>
        /// Gets the bottom boundary of this rectangle
        /// </summary>
        public double YMin { get { return yMin; } }

        /// <summary>
        /// Gets the top boundary of this rectangle
        /// </summary>
        public double YMax { get { return yMax; } }

        /// <summary>
        /// Gets the bottom left point of this rectangle
        /// </summary>
        public Vector2 BL { get { return new Vector2(xMin,yMin); } }

        /// <summary>
        /// Gets the bottom right point of this rectangle
        /// </summary>
        public Vector2 BR { get { return new Vector2(xMax, yMin); } }

        /// <summary>
        /// Gets the top left point of this rectangle
        /// </summary>
        public Vector2 TL { get { return new Vector2(xMin, yMax); } }

        /// <summary>
        /// Gets the top right point of this rectangle
        /// </summary>
        public Vector2 TR { get { return new Vector2(xMax, yMax); } }

        /// <summary>
        /// Gets the center point of this rectangle
        /// </summary>
        public Vector2 Center { get { return new Vector2((xMin+xMax)/2.0, (yMin+yMax)/2.0); } }

        /// <summary>
        /// Tests if the given point lies within this rectangle. Points on the boundary are considered contained.
        /// </summary>
        public bool contains(Vector2 p)
        {
            return p.X >= xMin && p.X <= xMax && p.Y >= yMin && p.Y <= yMax;
        }

        /// <summary>
        /// Returns a rectangle translated by the added vector
        /// </summary>
        public static Rectangle operator +(Rectangle r, Vector2 v)
        {
            return new Rectangle(r.xMin + v.X, r.yMax + v.X, r.yMin + v.Y, r.yMax + v.Y);
        }

        /// <summary>
        /// Returns a rectangle translated by the added vector
        /// </summary>
        public static Rectangle operator +(Vector2 v, Rectangle r)
        {
            return new Rectangle(r.xMin + v.X, r.yMax + v.X, r.yMin + v.Y, r.yMax + v.Y);
        }

        /// <summary>
        /// Returns a rectangle translated by the negative of the vector
        /// </summary>
        public static Rectangle operator -(Rectangle r, Vector2 v)
        {
            return r + (-v);
        }

        /// <summary>
        /// Returns the translation of this rectangle by the given vector.
        /// </summary>
        public Rectangle translate(Vector2 v)
        {
            return this + v;
        }
        Geom Geom.translate(Vector2 v)
        { return translate(v); }

        /// <summary>
        /// Not implemented
        /// </summary>
        public Rectangle rotateAroundPoint(Vector2 p, double angle)
        {
            throw new NotImplementedException();
        }
        Geom Geom.rotateAroundPoint(Vector2 p, double angle)
        { return rotateAroundPoint(p, angle); }


        /// <summary>
        /// Compute the shortest unit vector direction leading out of this rectangle from p
        /// </summary>
        public Vector2 ShortestDirectionOut(Vector2 p)
        {
            bool minXLeft;
            double minXDist;
            if (p.X - xMin < xMax - p.X)
            { minXLeft = true; minXDist = p.X - xMin; }
            else
            { minXLeft = false; minXDist = xMax - p.X; }

            bool minYDown;
            double minYDist;
            if (p.Y - yMin < yMax - p.Y)
            { minYDown = true; minYDist = p.Y - yMin; }
            else
            { minYDown = false; minYDist = yMax - p.Y; }

            if (minXDist < minYDist)
                return minXLeft ? new Vector2(-1, 0) : new Vector2(1, 0);
            else
                return minYDown ? new Vector2(0, -1) : new Vector2(0, 1);
        }
    }
}
