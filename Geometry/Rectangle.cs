﻿using System;
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
    public class Rectangle : GeomBase<Rectangle>
    {
        /// <summary>
        /// The left boundary of this rectangle
        /// </summary>
        public double XMin { get; private set; }

        /// <summary>
        /// The right boundary of this rectangle
        /// </summary>
        public double XMax { get; private set; }

        /// <summary>
        /// The bottom boundary of this rectangle
        /// </summary>
        public double YMin { get; private set; }

        /// <summary>
        /// The top boundary of this rectangle
        /// </summary>
        public double YMax { get; private set; }

        /// <summary>
        /// Constructs the rectangle with the given boundaries
        /// </summary>
        public Rectangle(double xMin, double xMax, double yMin, double yMax)
        {
            this.XMin = xMin;
            this.XMax = xMax;
            this.YMin = yMin;
            this.YMax = yMax;
        }

        /// <summary>
        /// Constructs the rectangle beginning at the given point with the given width and height
        /// </summary>
        public Rectangle(Vector2 p, double width, double height)
        {
            if (width >= 0)
            {
                XMin = p.X;
                XMax = p.X + width;
            }
            else
            {
                XMin = p.X + width;
                XMax = p.X;
            }

            if (height >= 0)
            {
                YMin = p.Y;
                YMax = p.Y + height;
            }
            else
            {
                YMin = p.Y + height;
                YMax = p.Y;
            }
        }


        public double Width => XMax - XMin;
        public double Height => YMax - YMin;

        /// <summary>
        /// Gets the bottom left point of this rectangle
        /// </summary>
        public Point2 BL => new Point2(XMin,YMin);

        /// <summary>
        /// Gets the bottom right point of this rectangle
        /// </summary>
        public Point2 BR => new Point2(XMax, YMin);

        /// <summary>
        /// Gets the top left point of this rectangle
        /// </summary>
        public Point2 TL => new Point2(XMin, YMax);

        /// <summary>
        /// Gets the top right point of this rectangle
        /// </summary>
        public Point2 TR => new Point2(XMax, YMax);

        /// <summary>
        /// Gets the center point of this rectangle
        /// </summary>
        public Point2 Center => new Point2((XMin+XMax)/2.0, (YMin+YMax)/2.0);

        /// <summary>
        /// Tests if the given point lies within this rectangle. Points on the boundary are considered contained.
        /// </summary>
        public override bool contains(Point2 p)
        {
            return p.X >= XMin && p.X <= XMax && p.Y >= YMin && p.Y <= YMax;
        }

        /// <summary>
        /// Returns the translation of this rectangle by the given vector.
        /// </summary>
        public override Rectangle translate(Vector2 v)
        {
            return new Rectangle(XMin + v.X, YMax + v.X, YMin + v.Y, YMax + v.Y);
        }

        /// <summary>
        /// Not implemented
        /// </summary>
        public override Rectangle rotateAroundPoint(Point2 p, double angle)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Compute the shortest unit vector direction leading out of this rectangle from p
        /// </summary>
        public Vector2 ShortestDirectionOut(Point2 p)
        {
            bool minXLeft;
            double minXDist;
            if (p.X - XMin < XMax - p.X)
            { minXLeft = true; minXDist = p.X - XMin; }
            else
            { minXLeft = false; minXDist = XMax - p.X; }

            bool minYDown;
            double minYDist;
            if (p.Y - YMin < YMax - p.Y)
            { minYDown = true; minYDist = p.Y - YMin; }
            else
            { minYDown = false; minYDist = YMax - p.Y; }

            if (minXDist < minYDist)
                return minXLeft ? new Vector2(-1, 0) : new Vector2(1, 0);
            else
                return minYDown ? new Vector2(0, -1) : new Vector2(0, 1);
        }

        public override double distance(Point2 p)
        {
            throw new NotImplementedException();
        }
    }
}
