using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RFC.Utilities
{
    public static class AngUtils
    {
        // returns how far the two angles are from each other, including wrap around
        // unsigned
        public static double compare(double a, double b)
        {
            double diff = (a - b) % (2 * Math.PI);
            diff = Math.Abs(diff - Math.PI); // how far it is from pi off
            return Math.PI - diff;
        }
    }
}
