using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace RFC.Utilities
{
    public class ColorUtils
    {
        // from lowest to highest
        private static Color[] colors = {
            Color.Black,
            Color.DarkRed,
            Color.Red,
            Color.OrangeRed,
            Color.Orange,
            Color.Yellow,
            Color.GreenYellow,
            Color.Green,
            Color.DarkCyan,
            Color.Blue
        };

        public static Color numToColor(double num, double min, double max)
        {
            double f = (num - min) / (max - min);

            int idx = (int)(f * colors.Length);
            if (idx < 0) idx = 0;
            if (idx >= colors.Length) idx = colors.Length - 1;

            return colors[idx];
        }
    }
}
