using System;
using System.Collections.Generic;
using System.Text;
/*
 * This file stores all the code for determining the intersection of two lines/circles.  Since there's
 * the arbitrary decision of how to label the intersections, it's good that it's handled in one place
 * rather than both in the designer and the interpreter.
 * 
 * There are also these things that I've called "ImplicitAssumptions."  For instance, what do you do if
 * a point defined by the intersection of two circles doesn't exist, because the two circles don't intersect?
 * And what if that point is used in other things...you get the idea.  If that happens, then an implicit
 * assumption has been failed, which counts as a condition failing.  These may also be included in actions;
 * for instance, maybe you have an action which is to shoot the ball.  What if that robot isn't anywhere
 * near the ball?  So implicit assumptions are basically conditions that the designer/interpreter will handle
 * for you.  Eventually, maybe there'll be a list of them so people will know what they do and don't have to
 * worry about in actions (for instance, do people have to worry about having a clear shot before shooting?).
 */
namespace RFC.Geometry
{
    public static class GeomFuncs
    {
        #region INTERSECTIONS
        //INTERSECTIONS----------------------------------------------------------------------------

        /// <summary>
        /// Tests if two Geom objects intersect.
        /// If one entirely contains another, this also counts as intersection.
        /// </summary>
        static public bool intersects(Geom g0, Geom g1)
        {
            if (g0 is Line)
            {
                if (g1 is Line)             return intersects((Line)g0, (Line)g1);
                else if (g1 is LineSegment) return intersects((Line)g0, (LineSegment)g1);
                else if (g1 is Rectangle)   return intersects((Line)g0, (Rectangle)g1);
                else if (g1 is Circle)      return intersects((Line)g0, (Circle)g1);
                else if (g1 is Arc)         return intersects((Line)g0, (Arc)g1);
            }
            else if (g0 is LineSegment)
            {
                if (g1 is Line)             return intersects((LineSegment)g0, (Line)g1);
                else if (g1 is LineSegment) return intersects((LineSegment)g0, (LineSegment)g1);
                else if (g1 is Rectangle)   return intersects((LineSegment)g0, (Rectangle)g1);
                else if (g1 is Circle)      return intersects((LineSegment)g0, (Circle)g1);
                else if (g1 is Arc)         return intersects((LineSegment)g0, (Arc)g1);
            }
            else if (g0 is Rectangle)
            {
                if (g1 is Line)             return intersects((Rectangle)g0, (Line)g1);
                else if (g1 is LineSegment) return intersects((Rectangle)g0, (LineSegment)g1);
                else if (g1 is Rectangle)   return intersects((Rectangle)g0, (Rectangle)g1);
                else if (g1 is Circle)      return intersects((Rectangle)g0, (Circle)g1);
                else if (g1 is Arc)         return intersects((Rectangle)g0, (Arc)g1);
            }
            else if (g0 is Circle)
            {
                if (g1 is Line)             return intersects((Circle)g0, (Line)g1);
                else if (g1 is LineSegment) return intersects((Circle)g0, (LineSegment)g1);
                else if (g1 is Rectangle)   return intersects((Circle)g0, (Rectangle)g1);
                else if (g1 is Circle)      return intersects((Circle)g0, (Circle)g1);
                else if (g1 is Arc)         return intersects((Circle)g0, (Arc)g1);
            }
            else if (g0 is Arc)
            {
                if (g1 is Line)             return intersects((Arc)g0, (Line)g1);
                else if (g1 is LineSegment) return intersects((Arc)g0, (LineSegment)g1);
                else if (g1 is Rectangle)   return intersects((Arc)g0, (Rectangle)g1);
                else if (g1 is Circle)      return intersects((Arc)g0, (Circle)g1);
                else if (g1 is Arc)         return intersects((Arc)g0, (Arc)g1);
            }

            throw new NotImplementedException();
        }

        static public bool intersects(Line a0, Line a1)
        {
            //Intersect if not parallel, or if parallel and each line contains the other's starting point.
            //Containment is tested both ways so in case of float imprecision, the function will at least
            //be symmetric.
            return !a0.isParallelTo(a1) || (a0.contains(a1.P0) && a1.contains(a0.P0));
        }

        static public bool intersects(Line a0, LineSegment a1)
        {
            //Test if the line segment's points are on different sides of the line, or are on the line.
            return a0.signedDistance(a1.P0) * a0.signedDistance(a1.P1) <= 0;
        }
        static public bool intersects(LineSegment a0, Line a1)
        { return intersects(a1, a0); }

        static public bool intersects(Line a0, Rectangle a1)
        {
            return intersects(a0, new LineSegment(a1.BL, a1.BR))
                || intersects(a0, new LineSegment(a1.TL, a1.TR))
                || intersects(a0, new LineSegment(a1.BL, a1.TL))
                || intersects(a0, new LineSegment(a1.BR, a1.TR));
        }
        static public bool intersects(Rectangle a0, Line a1)
        { return intersects(a1, a0); }

        static public bool intersects(Line a0, Circle a1)
        {
            //Test if the minimum distance of the center of the circle is closer than the radius
            return a0.distance(a1.Center) <= a1.Radius;
        }
        static public bool intersects(Circle a0, Line a1)
        { return intersects(a1, a0); }

        static public bool intersects(Line a0, Arc a1)
        {
            //Test if the minimum distance of the center of the arc is closer than the radius
            //and the closest point is on the correct side of the line between the arc endpoints.
            //Taking the line from start->stop, the point should be on the RIGHT side (signed dist <= 0)
            //of the arc, except if the arc goes the otherway, in which case we flip it.
            //Also test if we intersect the segment between the arc's endpoints.
            if (a0.distance(a1.Center) > a1.Radius)
                return false;
            if (a1.isFullCircle())
                return true;
            if (a1.Angle == 0)
                return false;
            Line endpointLine = new Line(a1.StartPt, a1.StopPt);
            if (endpointLine.Direction == Vector2.ZERO)
                return false;
            if (intersects(a0, endpointLine.Segment))
                return true;
            Vector2 closestPoint = a0.closestPointTo(a1.Center);
            return endpointLine.signedDistance(closestPoint) * a1.Angle <= 0;
        }
        static public bool intersects(Arc a0, Line a1)
        { return intersects(a1, a0); }

        static public bool intersects(LineSegment a0, LineSegment a1)
        {
            //Test if both line segments' points are on opposite sides of the other's line.
            return a0.Line.signedDistance(a1.P0) * a0.Line.signedDistance(a1.P1) <= 0 &&
                   a1.Line.signedDistance(a0.P0) * a1.Line.signedDistance(a0.P1) <= 0;
        }

        static public bool intersects(LineSegment a0, Rectangle a1)
        {
            return intersects(a0, new LineSegment(a1.BL, a1.BR))
                || intersects(a0, new LineSegment(a1.TL, a1.TR))
                || intersects(a0, new LineSegment(a1.BL, a1.TL))
                || intersects(a0, new LineSegment(a1.BR, a1.TR));
        }
        static public bool intersects(Rectangle a0, LineSegment a1)
        { return intersects(a1, a0); }

        static public bool intersects(LineSegment a0, Circle a1)
        {
            //Test if the minimum distance of the center of the circle is closer than the radius
            return a0.distance(a1.Center) <= a1.Radius;
        }
        static public bool intersects(Circle a0, LineSegment a1)
        { return intersects(a1, a0); }

        static public bool intersects(LineSegment a0, Arc a1)
        {
            if (a1.isFullCircle())
                return intersects(a0, a1.Circle);
            if (a1.Angle == 0)
                return false;

            //Just compute the intersections and test if they are within the desired angle range.
            Vector2[] intersections = LineCircleIntersection.BothIntersections(a0.Line, a1.Circle);
            for (int i = 0; i < intersections.Length; i++)
            {
                Vector2 dir = intersections[i] - a1.Center;
                if (dir == Vector2.ZERO)
                    return true;
                double angle = dir.cartesianAngle();
                if (a1.angleIsInArc(angle))
                    return true;
            }

            return false;
        }
        static public bool intersects(Arc a0, LineSegment a1)
        { return intersects(a1, a0); }

        static public bool intersects(Rectangle a0, Rectangle a1)
        {
            return (a0.XMin <= a1.XMin ? a0.XMax >= a1.XMin : a1.XMax >= a0.XMax)
                && (a0.YMin <= a1.YMin ? a0.YMax >= a1.YMin : a1.YMax >= a0.YMax);
        }
        static public bool intersects(Rectangle a0, Circle a1)
        {
            if (a1.Center.X <= a0.XMin)
            {
                if (a1.Center.Y <= a0.YMin) return a1.contains(a0.BL);
                if (a1.Center.Y >= a0.YMax) return a1.contains(a0.TL);
                return a1.Center.X + a1.Radius >= a0.XMin;
            }
            if (a1.Center.X >= a0.XMax)
            {
                if (a1.Center.Y <= a0.YMin) return a1.contains(a0.BR);
                if (a1.Center.Y >= a0.YMax) return a1.contains(a0.TR);
                return a1.Center.X - a1.Radius <= a0.XMax;
            }

            if (a1.Center.Y <= a0.YMin) return a1.Center.Y + a1.Radius >= a0.YMin;
            if (a1.Center.Y >= a0.YMax) return a1.Center.Y - a1.Radius <= a0.YMax;
            return true;
        }
        static public bool intersects(Circle a0, Rectangle a1)
        { return intersects(a1, a0); }

        static public bool intersects(Rectangle a0, Arc a1)
        {
            throw new NotImplementedException();
        }
        static public bool intersects(Arc a0, Rectangle a1)
        { return intersects(a1, a0); }

        static public bool intersects(Circle a0, Circle a1)
        {
            //Test if the circles' centers are closer than the sum of their radius.
            double radSum = a0.Radius + a1.Radius;
            return a0.Center.distanceSq(a1.Center) <= radSum * radSum;
        }

        static public bool intersects(Circle a0, Arc a1)
        {
            if (a1.isFullCircle())
                return intersects(a0, a1.Circle);
            if (a1.Angle == 0)
                return false;

            //Just compute the intersections and test if they are within the desired angle range.
            Vector2[] intersections = CircleCircleIntersection.BothIntersections(a0, a1.Circle);
            for (int i = 0; i < intersections.Length; i++)
            {
                Vector2 dir = intersections[i] - a1.Center;
                if (dir == Vector2.ZERO)
                    return true;
                double angle = dir.cartesianAngle();
                if (a1.angleIsInArc(angle))
                    return true;
            }
            return false;
        }
        static public bool intersects(Arc a0, Circle a1)
        { return intersects(a1, a0); }

        static public bool intersects(Arc a0, Arc a1)
        {
            if (a0.isFullCircle())
                return intersects(a0.Circle, a1);
            if (a1.isFullCircle())
                return intersects(a1.Circle, a0);
            if (a0.Angle == 0 || a1.Angle == 0)
                return false;

            //Just compute the intersections and test if they are within the desired angle range.
            Vector2[] intersections = CircleCircleIntersection.BothIntersections(a0.Circle, a1.Circle);
            for (int i = 0; i < intersections.Length; i++)
            {
                Vector2 dir0 = intersections[i] - a0.Center;
                Vector2 dir1 = intersections[i] - a1.Center;
                bool good0 = false;
                bool good1 = false;
                if (dir0 == Vector2.ZERO)
                    good0 = true;
                else if(a0.angleIsInArc(dir0.cartesianAngle()))
                    good0 = true;
                if (dir1 == Vector2.ZERO)
                    good1 = true;
                else if(a1.angleIsInArc(dir1.cartesianAngle()))
                    good1 = true;

                if(good0 && good1)
                    return true;
            }
            return false;
        }
        #endregion

    }



    public static class Intersections
    {
        static public Vector2 intersection(Line l1, Line l2)
        {
            return LineLineIntersection.Intersection(l1, l2);
        }
        static public Vector2 intersection(Line l, Circle c, int whichintersection)
        {
            return LineCircleIntersection.Intersection(l, c, whichintersection);
        }
        static public Vector2 intersection(Circle c1, Circle c2, int whichintersection)
        {
            return CircleCircleIntersection.Intersection(c1, c2, whichintersection);
        }
    }

    /// <summary>
    /// This signifies that an assumption implicitly built into a play
    /// (such as the fact that a certain intersection exists) has failed.
    /// </summary>
    public class ImplicitAssumptionFailedException : ApplicationException
    {
        public ImplicitAssumptionFailedException(string s) : base(s) { }
    }
    public class NoIntersectionException : ImplicitAssumptionFailedException
    {
        public NoIntersectionException(string s) : base(s) { }
    }

    /* Since there are potentially two points of intersection, whichintersection is used to specify which one.
     * Let p0 and p1 be the centers of the circles.
     * whichintersection = 1 : Return the intersection on the left side of the line p0 -> p1
     * whichintersection = -1 : Return the intersection on the right side of the line p0 -> p1
     */
    static public class CircleCircleIntersection
    {
        static public Vector2[] BothIntersections(Circle c0, Circle c1)
        {
            return GetPoints(c0, c1);
        }

        static public Vector2 Intersection(Circle c0, Circle c1, int whichintersection)
        {
            Vector2[] bothpoints = GetPoints(c0, c1);
            if(bothpoints.Length == 0)
                throw new NoIntersectionException("No intersection!");

            Vector2 p0 = c0.Center;
            Vector2 p1 = c1.Center;

            if (whichintersection == 0)
                throw new ArgumentException("CircleCircleIntersection.getPoint() called, but the direction to find the point is not defined", "whichintersection");

            int sign = anglesign(p1, p0, bothpoints[0]);
            if (sign == whichintersection)
                return bothpoints[0];
            else
                return bothpoints[1];
        }
        /// <summary>
        /// Given two circles and a point of intersection, returns which intersection number it is closer to.
        /// </summary>
        static public int WhichIntersection(Circle c0, Circle c1, Vector2 p)
        {
            int sign = anglesign(c1.Center, c0.Center, p);
            if (sign == 0)
                sign = -1;
            return sign;
        }
        static private Vector2[] GetPoints(Circle c0, Circle c1)
        {
            Vector2 p0 = c0.Center;
            Vector2 p1 = c1.Center;
            double d = Math.Sqrt(p0.distanceSq(p1));
            double r0 = c0.Radius;
            double r1 = c1.Radius;
            if (d > r0 + r1 || d < Math.Abs(r1 - r0))
                return new Vector2[0]; 

            double a = (r0 * r0 - r1 * r1 + d * d) / (2 * d);

            Vector2[] bothpoints = new Vector2[2];
            double newx = p0.X + (p1.X - p0.X) * a / d;
            double newy = p0.Y + (p1.Y - p0.Y) * a / d;
            double h = Math.Sqrt(r0 * r0 - a * a);
            double dx = h * (p1.Y - p0.Y) / d;
            double dy = h * (p0.X - p1.X) / d;
            bothpoints[0] = new Vector2(newx + dx, newy + dy);
            bothpoints[1] = new Vector2(newx - dx, newy - dy);
            return bothpoints;
        }
        static private int anglesign(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            double crossp = Vector2.crossproduct(p1, p2, p3);
            return Math.Sign(crossp);
        }
    }

    /* 
     * Since there are potentially two points of intersection, whichintersection is used to specify which one.
     * This is possible because Lines are directional (although they extend infinitely, they are defined by two
     * points p0 and p1 in some order). Letting the direction p0 -> p1 be positive:
     * whichintersection = 0 : Returns the most negative intersection
     * whichintersection = 1 : Returns the most positive intersection
     */
    static public class LineCircleIntersection
    {
        static public Vector2[] BothIntersections(Line line, Circle circle)
        {
            return getPoints(line, circle);
        }

        static public Vector2 Intersection(Line line, Circle circle, int whichintersection)
        {
            double[] dists = new double[2];
            Vector2[] points = getPoints(line, circle);
            if(points.Length == 0)
                throw new NoIntersectionException("no intersection!");

            dists[0] = distAlongLine(points[0], line);
            dists[1] = distAlongLine(points[1], line);

            if ((dists[0] < dists[1]) == (whichintersection == 0))
                return points[0];
            else
                return points[1];
        }
        static public int WhichIntersection(Line line, Circle circle, Vector2 p)
        {
            Vector2[] points = getPoints(line, circle);
            if (points.Length == 0)
                throw new NoIntersectionException("no intersection!");

            double dist = distAlongLine(p, line);
            double d0sq = points[0].distanceSq(p);
            double d1sq = points[1].distanceSq(p);
            Vector2 otherpoint = points[0];
            if (d1sq > d0sq)
                otherpoint = points[1];
            double dist2 = distAlongLine(otherpoint, line);
            if (dist < dist2)
                return 0;
            else
                return 1;
        }
        static private double distAlongLine(Vector2 p, Line line)
        {
            Vector2 dir = line.Direction;
            if (dir == Vector2.ZERO)
                throw new ImplicitAssumptionFailedException("Line has zero length!");
            return (p - line.P0).projectionLength(dir);

            /* TODO (davidwu) if everything appears to be working, remove this old code.
            double d0 = Math.Sqrt(UsefulFunctions.distancesq(p, line.P0));
            double d1 = Math.Sqrt(UsefulFunctions.distancesq(p, line.P1));
            double d = Math.Sqrt(UsefulFunctions.distancesq(line.P0, line.P1));
            if (d1 - d0 >= d * .99)
                return d0;
            else
                return -d0;
            */
        }

        static private Vector2[] getPoints(Line line, Circle circle)
        {
            Vector2 center = circle.Center;
            line = line - center;

            double dx = line.P1.X - line.P0.X;
            double dy = line.P1.Y - line.P0.Y;
            double drs = dx * dx + dy * dy;
            double dr = Math.Sqrt(drs);
            double D = line.P0.X * line.P1.Y - line.P1.X * line.P0.Y;
            double r = circle.Radius;
            double det = r * r * dr * dr - D * D;
            if (det < 0)
                return new Vector2[0]; 

            Vector2[] rtnpoints = new Vector2[2];

            double ddx = Math.Sqrt(det) * sign(dy) * dx;
            double ddy = Math.Sqrt(det) * Math.Abs(dy);
            double x0 = dy * D;
            double y0 = -dx * D;

            rtnpoints[0] = new Vector2((x0 - ddx) / drs, (y0 - ddy) / drs);
            rtnpoints[1] = new Vector2((x0 + ddx) / drs, (y0 + ddy) / drs);

            rtnpoints[0] += center;
            rtnpoints[1] += center;

            return rtnpoints;
        }
        static private int sign(double f)
        {
            if (f < 0)
                return -1;
            return 1;
        }
    }

    /*
     * This class reproduces the old behavior of LineCircleIntersection, prior to a geometry refactor.
     * It is deprecated because the behavior for which intersection is chosen is undocumented and confusing.
     * 
     * TODO: convert everything to the new LineCircleIntersection!
     */
    static public class LineCircleIntersectionDeprecated
    {
        static public Vector2 Intersection(Line line, Circle circle, int whichintersection)
        {
            Vector2[] linepoints = {line.P0,line.P1};
            if (whichintersection == 1)
            {
                Vector2 temp = linepoints[0];
                linepoints[0] = linepoints[1];
                linepoints[1] = temp;
            }

            double[] dists = new double[2];
            Vector2[] points = getPoints(line, circle);
            dists[0] = distalongline(points[0], linepoints);
            dists[1] = distalongline(points[1], linepoints);

            if (dists[0] > dists[1])
                return points[0];
            else
                return points[1];
        }
        static public int WhichIntersection(Line line, Circle circle, Vector2 p)
        {
            Vector2[] linepoints = { line.P0, line.P1 };
            Vector2[] points = getPoints(line, circle);
            double dist = distalongline(p, linepoints);
            double d0 = points[0].distanceSq(p);
            double d1 = points[1].distanceSq(p);
            Vector2 otherpoint = points[0];
            if (d1 > d0)
                otherpoint = points[1];
            double dist2 = LineCircleIntersectionDeprecated.distalongline(otherpoint, linepoints);
            if (dist > dist2)
                return 0;
            else
                return 1;
        }
        private static double distalongline(Vector2 p, Vector2[] linepoints)
        {
            double d0 = p.distance(linepoints[0]);
            double d1 = p.distance(linepoints[1]);
            double d = linepoints[0].distance(linepoints[1]);
            if (d1 - d0 >= d * .99)
                return d0;
            else
                return -d0;
        }
        static private Vector2[] getPoints(Line line, Circle circle)
        {
            Vector2[] points = { line.P0, line.P1 };
            Vector2 center = circle.Center;
            points[0] = points[0] - center;
            points[1] = points[1] - center;

            double dx = points[1].X - points[0].X;
            double dy = points[1].Y - points[0].Y;
            double drs = dx * dx + dy * dy;
            double dr = Math.Sqrt(drs);
            double D = points[0].X * points[1].Y - points[1].X * points[0].Y;
            double r = circle.Radius;
            double det = r * r * dr * dr - D * D;
            if (det < 0)
            {
                throw new NoIntersectionException("no intersection!");
                //throw new Exception("There is no intersection of line " + line.getName() + " and circle " + circle.getName() + ".");
            }

            Vector2[] rtnpoints = new Vector2[2];

            double ddx = Math.Sqrt(det) * sign(dy) * dx;
            double ddy = Math.Sqrt(det) * Math.Abs(dy);
            double x0 = dy * D;
            double y0 = -dx * D;

            rtnpoints[0] = new Vector2((x0 - ddx) / drs, (y0 - ddy) / drs);
            rtnpoints[1] = new Vector2((x0 + ddx) / drs, (y0 + ddy) / drs);

            rtnpoints[0] += center;
            rtnpoints[1] += center;

            return rtnpoints;
        }
        static private int sign(double f)
        {
            if (f < 0)
                return -1;
            return 1;
        }
    }

    static public class LineLineIntersection
    {
        static public Vector2 Intersection(Line line0, Line line1)
        {
            Vector2[] l0 = { line0.P0, line0.P1 };
            Vector2[] l1 = { line1.P0, line1.P1 };
            double denom = (l1[1].Y - l1[0].Y) * (l0[1].X - l0[0].X) - (l1[1].X - l1[0].X) * (l0[1].Y - l0[0].Y);
            if (denom == 0) //the lines are parallel
            {
                throw new NoIntersectionException("no intersection!");
            }
            double numerator = (l1[1].X - l1[0].X) * (l0[0].Y - l1[0].Y) - (l1[1].Y - l1[0].Y) * (l0[0].X - l1[0].X);
            double x = l0[0].X + numerator * (l0[1].X - l0[0].X) / denom;
            double y = l0[0].Y + numerator * (l0[1].Y - l0[0].Y) / denom;
            return new Vector2(x, y);
        }
    }
}
