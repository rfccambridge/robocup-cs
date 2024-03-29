﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RFC.Geometry
{
    /// <summary>
    /// Standard interface for geometric objects
    /// </summary>
    public interface IGeom
    {
        /// <summary>
        /// Returns the translation of this object by the given vector
        /// </summary>
        IGeom translate(Vector2 v);

        /// <summary>
        /// Returns the rotation of this object around the given point by the given angle
        /// </summary>
        IGeom rotateAroundPoint(Point2 p, double angle);

        /// <summary>
        /// Returns the distance to the edge of this shape. If the shape contains an area, and the point lies inside, the result is negative
        /// </summary>
        double distance(Point2 p);

        /// <summary>
        /// Returns true if the point is in the geometry, or on its boundary
        /// </summary>
        bool contains(Point2 target);
    }

    /// <summary>
    /// A generic version of IGeom, which maintains types after movement
    /// </summary>
    public interface IGeom<T> : IGeom where T : IGeom<T>
    {
        new T translate(Vector2 v);
        new T rotateAroundPoint(Point2 p, double angle);
    }

    /// <summary>
    /// Abstract base class to easy conversion between generic and non-generic forms, from
    /// which most geometry objects are derived
    /// </summary>
    public abstract class GeomBase<T> : IGeom<T> where T : GeomBase<T>
    {
        public abstract T translate(Vector2 v);
        public abstract T rotateAroundPoint(Point2 p, double angle);
        public abstract double distance(Point2 p);

        IGeom IGeom.translate(Vector2 v) { return translate(v);  }

        IGeom IGeom.rotateAroundPoint(Point2 p, double angle) { return rotateAroundPoint(p, angle); }

        public virtual bool contains(Point2 v)
        {
            return distance(v) <= 0;
        }

        /// <summary>Returns the geometry translated by the added vector</summary>
        public static T operator +(Vector2 v, GeomBase<T> g) => g.translate(v);

        /// <summary>Returns the geometry translated by the added vector</summary>
        public static T operator +(GeomBase<T> g, Vector2 v) => g.translate(v);

        /// <summary>Returns the geometry translated by the negative of the vector</summary>
        public static T operator -(GeomBase<T> g, Vector2 v) => g.translate(-v);
    }

    public static partial class GeomExtensions
    {
        /// <summary>
        /// Returns the shortest distance to any geometry in the list
        /// </summary>
        public static double distance(this IEnumerable<IGeom> list, Point2 p)
        {
            return list.Min(g => g.distance(p));
        }

        /// <summary>
        /// Returns whether any geometry in the list contains the point
        /// </summary>
        public static bool contains(this IEnumerable<IGeom> list, Point2 p)
        {
            return list.Any(g => g.contains(p));
        }
    }
}
