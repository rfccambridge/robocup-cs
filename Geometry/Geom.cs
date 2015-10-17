using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RFC.Geometry
{
    /// <summary>
    /// Standard interface for geometric objects
    /// </summary>
    public interface Geom
    {
        /// <summary>
        /// Returns the translation of this object by the given vector
        /// </summary>
        Geom translate(Vector2 v);

        /// <summary>
        /// Returns the rotation of this object around the given point by the given angle
        /// </summary>
        Geom rotateAroundPoint(Vector2 p, double angle);

        /// <summary>
        /// Returns the distance to the edge of this shape. If the shape contains an area, and the point lies inside, the result is negative
        /// </summary>
        double distance(Vector2 p);

        /// <summary>
        /// Returns true if the point is in the geometry, or on its boundary
        /// </summary>
        bool contains(Vector2 target);
    }

    public abstract class Geom<T> : Geom where T : Geom<T>
    {
        public abstract T translate(Vector2 v);
        public abstract T rotateAroundPoint(Vector2 p, double angle);
        public abstract double distance(Vector2 p);

        Geom Geom.translate(Vector2 v) { return translate(v);  }

        Geom Geom.rotateAroundPoint(Vector2 p, double angle) { return rotateAroundPoint(p, angle); }

        public virtual bool contains(Vector2 v)
        {
            return distance(v) <= 0;
        }
    }

    public static partial class GeomExtensions
    {
        /// <summary>
        /// Returns the shortest distance to any geometry in the list
        /// </summary>
        public static double distance(this IEnumerable<Geom> list, Vector2 p)
        {
            return list.Min(g => g.distance(p));
        }

        /// <summary>
        /// Returns whether any geometry in the list contains the point
        /// </summary>
        public static bool contains(this IEnumerable<Geom> list, Vector2 p)
        {
            return list.Any(g => g.contains(p));
        }
    }
}
