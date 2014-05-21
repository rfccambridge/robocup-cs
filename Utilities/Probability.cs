using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RFC.Geometry;

namespace RFC.Utilities
{
    static class Probability
    {
        public static double gaussianPDF(double x, double mean, double stdDev)
        {
            return Math.Exp(-(x - mean) * (x - mean) / (2 * stdDev * stdDev)) / (stdDev * Math.Sqrt(2 * Math.PI));
        }

        public static double gaussianPDF(Vector2 v, Vector2 mean, double stdDev)
        {
            return Math.Sqrt(gaussianPDF(v.X, mean.X, stdDev) * gaussianPDF(v.X, mean.X, stdDev)
                + gaussianPDF(v.Y, mean.X, stdDev) * gaussianPDF(v.Y, mean.X, stdDev));
        }
    }
}
