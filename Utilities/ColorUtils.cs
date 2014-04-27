using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace RFC.Utilities
{
    public class ColorUtils
    {
        public static Color numToColor(double num, double min, double max)
        {
            double n = 10.0;
            if (num < min + (max - min) / n)
            {
                return Color.Black;
            }
            else if (num < min + 2.0 * (max - min) / n)
            {
                return Color.DarkRed;
            }
            else if (num < min + 3.0 * (max - min) / n)
            {
                return Color.Red;
            }
            else if (num < min + 4.0 * (max - min) / n)
            {
                return Color.OrangeRed;
            }
            else if (num < min + 5.0 * (max - min) / n)
            {
                return Color.Orange;
            }
            else if (num < min + 6.0 * (max - min) / n)
            {
                return Color.Yellow;
            }
            else if (num < min + 7.0 * (max - min) / n)
            {
                return Color.GreenYellow;
            }
            else if (num < min + 8.0 * (max - min) / 5.0)
            {
                return Color.Green;
            }
            else if (num < min + 9.0 * (max - min) / 5.0)
            {
                return Color.DarkCyan;
            }
            else
            {
                return Color.Blue;
            }
        }
    }
}
