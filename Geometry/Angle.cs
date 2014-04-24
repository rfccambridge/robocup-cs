using System;
using System.Collections.Generic;
using System.Text;

namespace RFC.Geometry
{
    public static class Angle
    {
        public const double TWOPI = Math.PI * 2;

        /// <summary>
        /// Returns how many radians counter-clockwise the ray defined by angle1
        /// needs to be rotated to point in the direction angle2.
        /// Uses
        /// Returns a value in the range [-Pi,Pi)
        /// </summary>
        static public double AngleDifference(double angle1, double angle2)
        {
            double anglediff = angle2 - angle1;

            //mathematical modulo 2 pi
            anglediff = ((anglediff % TWOPI) + TWOPI) % TWOPI;

            //now we need to get the range to [-Pi, Pi):
            if (anglediff < Math.PI)
                return anglediff;
            else
                return anglediff - Math.PI * 2;
        }

        /// <summary>
        /// Ensures that an angle is in the [0; 2PI] range
        /// </summary>
        public static double AngleModTwoPi(double angle)
        {
            return angle = ((angle % TWOPI) + TWOPI) % TWOPI;
        }
    }


}
