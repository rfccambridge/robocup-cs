using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RFC.Geometry
{
    /// <summary>
    /// Represents a geometry expanded by a distance in all directions
    /// </summary>
    public class OffsetGeom<T> : GeomBase<OffsetGeom<T>> where T : IGeom<T>
    {
        private readonly T _inner;
        private readonly double _offset;
        public OffsetGeom(T inner, double offset)
        {
            _inner = inner;
            _offset = offset;
        }

        public override OffsetGeom<T> translate(Vector2 v)
        {
            return new OffsetGeom<T>(_inner.translate(v), _offset);
        }

        public override OffsetGeom<T> rotateAroundPoint(Vector2 p, double angle)
        {
            return new OffsetGeom<T>(_inner.rotateAroundPoint(p, angle), _offset);
        }

        public override double distance(Vector2 p)
        {
            return _inner.distance(p) - _offset;
        }
    }

    public static partial class GeomExtensions
    {
        /// <summary>
        /// Shorthand for creating a new OffsetGeom
        /// </summary>
        public static OffsetGeom<T> offset<T>(this T geom, double dist) where T : IGeom<T>
        {
            return new OffsetGeom<T>(geom, dist);
        }
    }
}
