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
        };

        public static Color numToColor(double num, double min, double max)
        {
            double f = (num - min) / (max - min);

            double didx = f * (colors.Length - 1);

            int idx = (int) didx;
            if (idx < 0) idx = 0;
            if (idx > colors.Length - 2) idx = colors.Length - 2;

            Color c1 = colors[idx];
            Color c2 = colors[idx + 1];
            double p = didx - idx;

            return Color.FromArgb(
                (int)(c1.A * (1 - p) + c2.A * p),
                (int)(c1.R * (1 - p) + c2.R * p),
                (int)(c1.G * (1 - p) + c2.G * p),
                (int)(c1.B * (1 - p) + c2.B * p)
            );
        }
    }
}
