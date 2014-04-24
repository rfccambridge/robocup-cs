using System;
using System.Collections.Generic;
using System.Text;

namespace RFC.Geometry
{
    /// <summary>
    /// A circular arc. Arcs sweep around counterclockwise from some starting cartesian angle to some stopping 
    /// cartesian angle, in radians. Arcs are allowed to sweep out an angle of MORE than 2Pi, in which case for intersections, 
    /// they will essentially behave like circles, or negative angles, in which case the arc will go clockwise 
    /// instead of counterclockwise.
    /// </summary>
    public class Arc : Geom
    {
        const double TWOPI = Geometry.Angle.TWOPI;

        private Vector2 center;
        private double radius;
 
        private double angleStart;
        private double angleStop;

        private Vector2 startPt;
        private Vector2 stopPt;

        /// <summary>
        /// The center around which this arc revolves
        /// </summary>
        public Vector2 Center
        {get { return center; }}

        /// <summary>
        /// The radius around of this arc around the center
        /// </summary>
        public double Radius
        {get { return radius; }}

        /// <summary>
        /// The cartesian angle at which this arc starts
        /// </summary>
        public double AngleStart
        {get { return angleStart; }}

        /// <summary>
        /// The cartesian angle at which this arc stops
        /// </summary>
        public double AngleStop
        {get { return angleStop; }}

        /// <summary>
        /// The SIGNED angle of this arc, positive if counterclockwise, negative if clockwise 
        /// </summary>
        public double Angle
        {get { return angleStop - angleStart; }}

        /// <summary>
        /// The point where this arc starts
        /// </summary>
        public Vector2 StartPt
        {get { return startPt; }}

        /// <summary>
        /// The point where this arc stops
        /// </summary>
        public Vector2 StopPt
        {get { return stopPt; }}

        /// <summary>
        /// The circle that this arc is a segment of
        /// </summary>
        public Circle Circle
        { get { return new Circle(center, radius); } }

        private Arc(Vector2 center, double radius, double angleStart, double angleStop, Vector2 startPt, Vector2 stopPt)
        {
            this.center = center;
            this.radius = radius;
            this.angleStart = angleStart;
            this.angleStop = angleStop;
            this.startPt = startPt;
            this.stopPt = stopPt;
        }

        /// <summary>
        /// Creates an arc with the given center, radius, covering the circular arc from angleStart to angleStop.
        /// Angles are in counterclockwise radians, where zero is towards the right (positive X axis).
        /// </summary>
        public Arc(Vector2 center, double radius, double angleStart, double angleStop)
        {
            this.center = center;
            this.radius = radius;

            this.angleStart = angleStart;
            this.angleStop = angleStop;

            this.startPt = center + radius * Vector2.GetUnitVector(angleStart);
            this.stopPt = center + radius * Vector2.GetUnitVector(angleStop);
        }

        /// <summary>
        /// Creates an arc with the given center, starting at pt and sweeping out a 
        /// counterclockise angle in radians. If angle is negative, will produce an 
        /// arc going counterclockwise.
        /// </summary>
        public Arc(Vector2 center, Vector2 pt, double angle)
        {
            this.center = center;
            this.radius = pt.distance(center);

            this.angleStart = (pt-center).cartesianAngle();
            this.angleStop = angleStart + angle;

            this.startPt = pt;
            this.stopPt = pt.rotateAroundPoint(center, angle);
        }
               
        /// <summary>
        /// Returns an arc translated by the added vector
        /// </summary>
        public static Arc operator +(Arc a, Vector2 v)
        {
            return new Arc(a.center + v, a.radius, a.angleStart, a.angleStop, a.startPt + v, a.stopPt + v);
        }

        /// <summary>
        /// Returns an arc translated by the added vector
        /// </summary>
        public static Arc operator +(Vector2 v, Arc a)
        {
            return new Arc(v + a.center, a.radius, a.angleStart, a.angleStop, v + a.startPt, v + a.stopPt);
        }

        /// <summary>
        /// Returns an arc translated by the negative of the vector
        /// </summary>
        public static Arc operator -(Arc a, Vector2 v)
        {
            return new Arc(a.center - v, a.radius, a.angleStart, a.angleStop, a.startPt - v, a.stopPt - v);
        }

        /// <summary>
        /// Returns the translation of this arc by the given vector.
        /// </summary>
        public Arc translate(Vector2 v)
        {
            return this + v;
        }
        Geom Geom.translate(Vector2 v)
        { return translate(v); }

        /// <summary>
        /// Returns an arc that is this arc rotated a given number of radians in the
        /// counterclockwise direction around p.
        /// </summary>
        public Arc rotateAroundPoint(Vector2 p, double angle)
        {
            return new Arc(center.rotateAroundPoint(p,angle), radius, 
                angleStart + angle, angleStop + angle, 
                startPt.rotateAroundPoint(p,angle), stopPt.rotateAroundPoint(p,angle));
        }
        Geom Geom.rotateAroundPoint(Vector2 p, double angle)
        { return rotateAroundPoint(p, angle); }

        /// <summary>
        /// Checks if this arc is essentially a circle (because its angle is >= 2Pi).
        /// </summary>
        public bool isFullCircle()
        {
            //Allow tiny difference to handle imprecision
            return Math.Abs(Angle) >= TWOPI - 0.00001;
        }

        /// <summary>
        /// Checks if this angle, modulo 2Pi, is in this arc.
        /// </summary>
        public bool angleIsInArc(double angle)
        {
            double arcAngle = this.Angle;
            double dAngle = angle - this.angleStart;
            dAngle = ((dAngle % TWOPI) + TWOPI) % TWOPI; //Mathematical modulus, ensures positive
            if (arcAngle > 0 && dAngle <= arcAngle)
                return true;
            if (arcAngle < 0 && dAngle - TWOPI >= arcAngle)
                return true;
            return false;
        }

        public override string ToString()
        {
            return "Arc(" + center + ", " + radius + "," + (angleStart/TWOPI) + "*2pi, " + (angleStop/TWOPI) + "*2pi)";
        }

    }
}
