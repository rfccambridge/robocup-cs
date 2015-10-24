using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RFC.Geometry;

namespace RFC.Utilities
{
    public static class Probability
    {
        public static double gaussianPDF(double x, double mean, double stdDev)
        {
            return Math.Exp(-(x - mean) * (x - mean) / (2 * stdDev * stdDev)) / (stdDev * Math.Sqrt(2 * Math.PI));
        }

        public static double gaussianPDF(Point2 v, Point2 mean, double stdDev)
        {
            return Math.Sqrt(gaussianPDF(v.X, mean.X, stdDev) * gaussianPDF(v.X, mean.X, stdDev)
                + gaussianPDF(v.Y, mean.X, stdDev) * gaussianPDF(v.Y, mean.X, stdDev));
        }
    }
}
