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
    public class Arc : GeomBase<Arc>
    {
        const double TWOPI = Geometry.Angle.TWOPI;

        /// <summary>
        /// The center around which this arc revolves
        /// </summary>
        public Point2 Center { get; private set; }

        /// <summary>
        /// The radius around of this arc around the center
        /// </summary>
        public double Radius { get; private set; }

        /// <summary>
        /// The cartesian angle at which this arc starts
        /// </summary>
        public double AngleStart { get; private set; }

        /// <summary>
        /// The cartesian angle at which this arc stops
        /// </summary>
        public double AngleStop { get; private set; }

        /// <summary>
        /// The SIGNED angle of this arc, positive if counterclockwise, negative if clockwise 
        /// </summary>
        public double Angle { get { return AngleStop - AngleStart; } }

        /// <summary>
        /// The point where this arc starts
        /// </summary>
        public Point2 StartPt { get; private set; }

        /// <summary>
        /// The point where this arc stops
        /// </summary>
        public Point2 StopPt { get; private set; }

        /// <summary>
        /// The circle that this arc is a segment of
        /// </summary>
        public Circle Circle { get { return new Circle(Center, Radius); } }

        private Arc(Point2 center, double radius, double angleStart, double angleStop, Point2 startPt, Point2 stopPt)
        {
            this.Center = center;
            this.Radius = radius;
            this.AngleStart = angleStart;
            this.AngleStop = angleStop;
            this.StartPt = startPt;
            this.StopPt = stopPt;
        }

        /// <summary>
        /// Creates an arc with the given center, radius, covering the circular arc from angleStart to angleStop.
        /// Angles are in counterclockwise radians, where zero is towards the right (positive X axis).
        /// </summary>
        public Arc(Point2 center, double radius, double angleStart, double angleStop)
        {
            this.Center = center;
            this.Radius = radius;

            this.AngleStart = angleStart;
            this.AngleStop = angleStop;

            this.StartPt = center + radius * Vector2.GetUnitVector(angleStart);
            this.StopPt = center + radius * Vector2.GetUnitVector(angleStop);
        }

        /// <summary>
        /// Creates an arc with the given center, starting at pt and sweeping out a 
        /// counterclockise angle in radians. If angle is negative, will produce an 
        /// arc going counterclockwise.
        /// </summary>
        public Arc(Point2 center, Point2 pt, double angle)
        {
            this.Center = center;
            this.Radius = pt.distance(center);

            this.AngleStart = (pt-center).cartesianAngle();
            this.AngleStop = AngleStart + angle;

            this.StartPt = pt;
            this.StopPt = pt.rotateAroundPoint(center, angle);
        }

        /// <summary>
        /// Returns the translation of this arc by the given vector.
        /// </summary>
        public override Arc translate(Vector2 v)
        {
            return new Arc(Center + v, Radius, AngleStart, AngleStop, StartPt + v, StopPt + v);
        }

        /// <summary>
        /// Returns an arc that is this arc rotated a given number of radians in the
        /// counterclockwise direction around p.
        /// </summary>
        public override Arc rotateAroundPoint(Point2 p, double angle)
        {
            return new Arc(Center.rotateAroundPoint(p,angle), Radius, 
                AngleStart + angle, AngleStop + angle, 
                StartPt.rotateAroundPoint(p,angle), StopPt.rotateAroundPoint(p,angle));
        }

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
            double dAngle = angle - this.AngleStart;
            dAngle = ((dAngle % TWOPI) + TWOPI) % TWOPI; //Mathematical modulus, ensures positive
            if (arcAngle > 0 && dAngle <= arcAngle)
                return true;
            if (arcAngle < 0 && dAngle - TWOPI >= arcAngle)
                return true;
            return false;
        }

        public override string ToString()
        {
            return "Arc(" + Center + ", " + Radius + "," + (AngleStart/TWOPI) + "*2pi, " + (AngleStop/TWOPI) + "*2pi)";
        }

        public override double distance(Point2 p)
        {
            throw new NotImplementedException();
        }
    }
}
