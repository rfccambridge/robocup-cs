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
    }

    public abstract class Geom<T> : Geom where T : Geom<T>
    {
        public abstract T translate(Vector2 v);
        public abstract T rotateAroundPoint(Vector2 p, double angle);
    }

    public interface AreaGeom : Geom
    {
        bool contains(Vector2 p);
    }
}
