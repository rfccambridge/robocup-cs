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

}
