using RFC.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace RFC.Geometry
{
    public class LatticeSpec
    {
        public readonly Rectangle Bounds;
        public readonly int Samples;

        /// <returns>the index of the cell containing v</returns>
        public void vectorToIndex(Vector2 v, out int x, out int y, out bool inRange)
        {
            double relx = (v.X - Bounds.XMin) / Bounds.Width;
            double rely = (v.Y - Bounds.YMin) / Bounds.Height;

            inRange = !(relx < 0 || relx > 1 || rely < 0 || rely > 1);

            x = (int)Math.Floor(relx * Samples);
            y = (int)Math.Floor(rely * Samples);

            // this deals with the case where the vector lies on the edge
            if (inRange)
            {
                if (x >= Samples) x = Samples - 1;
                if (y >= Samples) y = Samples - 1;
            }
        }
        public void vectorToIndex(Vector2 v, out int x, out int y)
        {
            bool inRange;
            vectorToIndex(v, out x, out y, out inRange);
            if (!inRange) throw new IndexOutOfRangeException();
        }

        /// <returns>the vector at the center of the cell (x,y)</returns>
        public Vector2 indexToVector(int x, int y)
        {
            // return a vector in the center of sample
            return new Vector2(
                this.Bounds.XMin + (x + 0.5) / Samples * this.Bounds.Width,
                this.Bounds.YMin + (y + 0.5) / Samples * this.Bounds.Height
            );
        }

        /// <returns>the rectangle for the cell (x,y)</returns>
        public Rectangle indexToRect(int x, int y)
        {
            return new Rectangle(
                this.Bounds.XMin + this.Bounds.Width * (x) / Samples,
                this.Bounds.XMin + this.Bounds.Width * (x + 1) / Samples,
                this.Bounds.YMin + this.Bounds.Height * (y) / Samples,
                this.Bounds.YMin + this.Bounds.Height * (y + 1) / Samples
            );
        }



        public LatticeSpec(Rectangle bounds, int samples)
        {
            this.Bounds = bounds;
            this.Samples = samples;
        }

        /// <summary> Maps a function over all points in the lattice </summary>
        public Lattice<T> Create<T>(Func<Vector2, T> filler) => Create<T>((v, x, y) => filler(v));
        public Lattice<T> Create<T>(Func<Vector2, int, int, T> filler)
        {
            Lattice<T> lattice = new Lattice<T>(this);
            for (int i = 0; i < Samples; i++)
            {
                for (int j = 0; j < Samples; j++)
                {
                    Vector2 v = indexToVector(i, j);
                    lattice.data[i, j] = filler(v, i, j);
                }
            }
            return lattice;
        }

        /// <summary> Maps a function over all points in the lattice, passing only the raw indices </summary>
        public Lattice<T> Create<T>(Func<int, int, T> filler)
        {
            Lattice<T> lattice = new Lattice<T>(this);
            for (int i = 0; i < Samples; i++)
            {
                for (int j = 0; j < Samples; j++)
                {
                    lattice.data[i, j] = filler(i, j);
                }
            }
            return lattice;
        }

        public override string ToString()
        {
            return string.Format("LatticeSpec({}, {})", Bounds, Samples);
        }
    }

    /// <summary>
    /// A class representing a lattice of T values calculated over a vector grid
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Lattice<T> : IEnumerable<KeyValuePair<Vector2, T>>
    {

        internal T[,] data;
        public readonly LatticeSpec Spec;

        /// <summary>
        /// Construct a lattice within a <paramref name="bounds"/>, with <paramref name="samples"/> cells in each axis
        /// </summary>
        public Lattice(LatticeSpec spec)
        {
            this.Spec = spec;
            this.data = new T[spec.Samples, spec.Samples];
        }

        /// <summary>
        /// Access the nearest lattice value to a coordinate. Gives an out of bounds error if the vector is not in Bounds
        /// </summary>
        public T this[Vector2 pos]
        {
            get
            {
                int x, y;
                Spec.vectorToIndex(pos, out x, out y);
                return data[x, y];
            }

            set
            {
                int x, y;
                Spec.vectorToIndex(pos, out x, out y);
                data[x, y] = value;
            }
        }

        /// <summary>
        /// Raw accessor for underlying double array, with default value
        /// </summary>
        public T Get(int x, int y, T def = default(T))
        {
            if (x < 0 || x >= Spec.Samples || y < 0 || y >= Spec.Samples) return def;
            return data[x, y];
        }

        // For ease of iteration / backwards compatibility
        public IEnumerator<KeyValuePair<Vector2, T>> GetEnumerator()
        {
            for (int i = 0; i < Spec.Samples; i++)
                for (int j = 0; j < Spec.Samples; j++)
                    yield return new KeyValuePair<Vector2, T>(Spec.indexToVector(i, j), data[i, j]);
        }
        public static explicit operator T[,] (Lattice<T> l)
        {
            return l.data;
        }

        public override String ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Lattice over ");
            sb.Append(Spec);
            sb.AppendLine();

            for (int i = 0; i < data.GetLength(0); i++)
            {
                for (int j = 0; j < data.GetLength(1); j++)
                {
                    sb.AppendFormat("\t{0}", data[i, j]);
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public Lattice<T2> Map<T2>(Func<T, T2> func) => Spec.Create((x, y) => func(data[x, y]));
        public Lattice<T2> Map<T2>(Func<T, Vector2, T2> func) => Spec.Create((v, x, y) => func(data[x, y], v));
        public Lattice<T2> Map<T2>(Func<T, int, int, T2> func) => Spec.Create((x, y) => func(data[x, y], x, y));

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
