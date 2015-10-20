using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RFC.Core;
using RFC.Geometry;

namespace RFC.PathPlanning
{
    public static class Avoider
    {
        private static double convergence_threshold = .01;
        static object lockobj = new object();
        // checks if this point is in bounds and outside of defense areas
        public static bool isValid(Vector2 target, bool is_goalie)
        {
            if (!in_bounds(target))
                return false;

            // if its a goalie we skip over checking the left defense area
            List<IGeom> copy = new List<IGeom>(Constants.FieldPts.LEFT_EXTENDED_DEFENSE_AREA);
            if (!is_goalie && copy.Any(g => g.contains(target)))
                return false;


            List<IGeom> copy2 = new List<IGeom>(Constants.FieldPts.RIGHT_EXTENDED_DEFENSE_AREA);
            if (copy2.Any(g => g.contains(target)))
                return false;

            return true;

            
        }
        // given a desired location, and a place to stay away from, calculate the closest
        // place that's still within bounds. used for staying 50cm away from ball during
        // kickin and kickoff
        public static RobotInfo avoid(RobotInfo target, Vector2 obstacle, double radius)
        {
            Vector2 diff = target.Position - obstacle;
            if (diff.magnitude() > radius)
            {
                return target;
            }

            // too close to obstacle
            diff = diff.normalizeToLength(radius);

            RobotInfo dest = new RobotInfo(target);
            dest.Position = obstacle + diff;

            if (in_bounds(dest))
            {
                return dest;
            }

            // moving directly away put us out of bounds
            // finding intersection of circle with sidelines
            List<Vector2> intersects = get_intersections(obstacle, radius);
            double mindist = 100000;
            Vector2 minpt = target.Position;
            foreach (Vector2 pt in intersects)
            {
                if (pt.distance(target.Position) < mindist)
                {
                    minpt = pt;
                    mindist = pt.distance(target.Position);
                }
            }

            dest.Position = minpt;
            return dest;
        }


        // iteratively avoid both obstacles until result converges
        // kind of a hack, but whatever
        /*public static RobotInfo avoid(RobotInfo target, Vector2 obst1, double rad1, Vector2 obst2, double rad2)
        {
            RobotInfo lastResult = new RobotInfo(new Vector2(-10, -10), 0,target.Team,0);
            RobotInfo result = new RobotInfo(target);

            while (result.Position.distance(lastResult.Position) > convergence_threshold)
            {
                lastResult = new RobotInfo(result);
                result = avoid(result, obst1, rad1);
                result = avoid(result, obst2, rad2);
            }

            return result;
        } */

        //changed avoid so that it takes into account both half circles
        public static RobotInfo avoid(RobotInfo target, Vector2 obst1, double rad1, Vector2 obst2, double rad2, 
            Vector2 obst3, double rad3, Vector2 obst4, double rad4)
        {
            RobotInfo lastResult = new RobotInfo(new Vector2(-10, -10), 0, target.Team, 0);
            RobotInfo result = new RobotInfo(target);

            while (result.Position.distance(lastResult.Position) > convergence_threshold)
            {
                lastResult = new RobotInfo(result);
                result = avoid(result, obst1, rad1);
                result = avoid(result, obst2, rad2);
                result = avoid(result, obst3, rad3);
                result = avoid(result, obst4, rad4);
            }

            return result;
        }

        private static bool in_bounds(RobotInfo rob)
        {
            return in_bounds(rob.Position);
        }

        private static bool in_bounds(Vector2 Position)
        {
            return (Position.X <= Constants.Field.XMAX && Position.X >= Constants.Field.XMIN && Position.Y <= Constants.Field.YMAX && Position.Y >= Constants.Field.YMIN);
        }

        private static List<Vector2> get_intersections(Vector2 obst, double radius)
        {
            /*
             * x^2 + y^2 = r^2
             * x = +-sqrt(r^2 - y^2)
             */

            List<Vector2> pts = new List<Vector2>();

            // top
            if (obst.Y + radius >  Constants.Field.YMAX)
            {
                double y = Constants.Field.YMAX - obst.Y;
                double d = Math.Sqrt(radius*radius - y*y);
                pts.Add(new Vector2(obst.X + d, Constants.Field.YMAX));
                pts.Add(new Vector2(obst.X - d, Constants.Field.YMAX));
            }
            // bottom
            else if (obst.Y - radius < Constants.Field.YMIN)
            {
                double y = obst.Y - Constants.Field.YMIN;
                double d = Math.Sqrt(radius * radius - y * y);
                pts.Add(new Vector2(obst.X + d, Constants.Field.YMIN));
                pts.Add(new Vector2(obst.X - d, Constants.Field.YMIN));
            }

            // right
            if (obst.X + radius > Constants.Field.XMAX)
            {
                double y = Constants.Field.XMAX - obst.X;
                double d = Math.Sqrt(radius * radius - y * y);
                pts.Add(new Vector2(Constants.Field.XMAX,obst.Y + d));
                pts.Add(new Vector2(Constants.Field.XMAX, obst.Y - d));
            }
            // left
            else if (obst.X - radius < Constants.Field.XMIN)
            {
                double y = obst.X - Constants.Field.XMIN;
                double d = Math.Sqrt(radius * radius - y * y);
                pts.Add(new Vector2(Constants.Field.XMIN, obst.Y + d));
                pts.Add(new Vector2(Constants.Field.XMIN, obst.Y - d));
            }

            // checking that all intersections are in bounds
            List<Vector2> in_pts = new List<Vector2>();
            foreach (Vector2 pt in pts)
            {
                if (in_bounds(pt))
                {
                    in_pts.Add(pt);
                }
            }
            return in_pts;
        }
    }
}
