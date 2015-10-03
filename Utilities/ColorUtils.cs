using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using RFC.Utilities;

namespace RFC.Utilities
{
    public class ColorMap
    {
        /// <summary>
        /// A list of color keypoints
        /// </summary>
        private readonly List<Tuple<double, Color>> _colorList;


        public static ColorMap Default = new ColorMap(
            Color.FromArgb(0, 0, 0, 0),
            Color.DarkRed,
            Color.Red,
            Color.OrangeRed,
            Color.Orange,
            Color.Yellow,
            Color.GreenYellow,
            Color.Green,
            Color.DarkCyan,
            Color.Blue
        );

        /// <summary>produce a color map from a list of ordered keypoints</summary>
        public ColorMap(List<Tuple<double, Color>> keypoints)
        {
            _colorList = keypoints;
        }

        /// <summary>produce a color map, where 0 maps to the first argument and 1 to the last</summary>
        public ColorMap(params Color[] colors)
        {
            _colorList = colors.Select(
                (color, i) => Tuple.Create((double) i / (colors.Count() - 1), color)
            ).ToList();
        }

        public ColorMap(Color c)
        {
            _colorList = new List<Tuple<double, Color>>(2);
            _colorList.Add(Tuple.Create(0.0, Color.FromArgb(0, c.R, c.G, c.B)));
            _colorList.Add(Tuple.Create(1.0, c));
        }

        public Color Get(double val)
        {
            double last = Double.NegativeInfinity;
            Color? lastC = null;
            for(int i = 0; i < _colorList.Count(); i++)
            {
                double curr = _colorList[i].Item1;
                Color currC = _colorList[i].Item2;

                if(val < curr)
                {
                    if (lastC == null)
                        return currC;

                    double x = (val - last) / (curr - last);
                    return lastC.Value.lerp(currC, x);
                }

                last = curr;
                lastC = currC;
            }
            return _colorList.Last().Item2;
        }

        public Color Get(double val, double min, double max)
        {
            return Get((val - min) / (max - min));
        }
    }
    
    public static class ColorUtils
    {
        public static Color lerp(this Color c1, Color c2, double p)
        {
            return Color.FromArgb(
                (int)(c1.A * (1 - p) + c2.A * p),
                (int)(c1.R * (1 - p) + c2.R * p),
                (int)(c1.G * (1 - p) + c2.G * p),
                (int)(c1.B * (1 - p) + c2.B * p)
            );
        }

        public static Color mix(this Color[] colors)
        {
            int a = 0, r = 0, g = 0, b = 0;
            foreach(Color c in colors)
            {
                a += c.A;
                r += c.R;
                g += c.G;
                b += c.B;
            }
            int n = colors.Count();
            return Color.FromArgb(a / n, r / n, g / n, b / n);
        }

        [Obsolete("Use ColorMap.Get instead")]
        public static Color numToColor(double num, double min, double max)
        {
            return ColorMap.Default.Get(num, min, max);
        }
    }
}
