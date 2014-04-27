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
            if (num < min + (max - min) / 5.0)
            {
                return Color.Red;
            }
            else if (num < min + 2.0 * (max - min) / 5.0)
            {
                return Color.Orange;
            }
            else if (num < min + 3.0 * (max - min) / 5.0)
            {
                return Color.Yellow;
            }
            else if (num < min + 4.0 * (max - min) / 5.0)
            {
                return Color.Green;
            }
            else
            {
                return Color.Blue;
            }
        }
    }
}
