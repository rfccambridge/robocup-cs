using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RFC.Core;
using RFC.Messaging;
using RFC.Geometry;

namespace RFC.Strategy
{
    public class ShotOpportunity
    {
        public Vector2 target;
        public double arc;
        public ShotOpportunity(Vector2 t, double a)
        {
            this.target = t;
            this.arc = a;
        }
    }

    public static class Shot1
    {


        // private class to represent the beginning or end of something
        // that blocks vision to the goal
        // false is the start of the blocked vision, true is end
        class edge
        {
            public double d;
            public bool b;
            public edge(double d, bool b)
            {
                this.d = d;
                this.b = b;
            }
        }

        // Finds the best place to aim when taking a shot and judges how good
        // a shot it is
        public static ShotOpportunity evaluate(FieldVisionMessage fvm, Team team, Vector2 hypo_ball)
        {
            return evaluateGoal(fvm, team, hypo_ball);
        }

        public static ShotOpportunity evaluateGoal(FieldVisionMessage fvm, Team team, Vector2 hypo_ball)
        {
            return evaluateGeneral(fvm, team, hypo_ball, Constants.FieldPts.THEIR_GOAL_TOP, Constants.FieldPts.THEIR_GOAL_BOTTOM);
        }

        public static ShotOpportunity evaluateField(FieldVisionMessage fvm, Team team, Vector2 hypo_ball)
        {
            return evaluateGeneral(fvm, team, hypo_ball, Constants.FieldPts.TOP_RIGHT, Constants.FieldPts.BOTTOM_RIGHT);
        }

        
        public static ShotOpportunity evaluateGeneral(FieldVisionMessage fvm, Team team, Vector2 shot_position, Vector2 top, Vector2 bottom)
        {
            List<RobotInfo> locations = fvm.GetRobots(Team.Blue);

            // finding beginning and end of all occlusions from other robots as edges
            List<edge> intervals = new List<edge>();
            for (int i = 0; i < locations.Count(); i++)
            {
                RobotInfo robot = locations[i];
                Vector2 difference = robot.Position - shot_position;
                double sweep = Math.Asin((Constants.Basic.ROBOT_RADIUS + Constants.Basic.BALL_RADIUS) / difference.magnitude());
                double angle = difference.cartesianAngle();
                intervals.Add(new edge(angle - sweep, false));
                intervals.Add(new edge(angle + sweep, true));
            }

            // adding in edges of goal
            Vector2 goal1 = bottom - shot_position;
            Vector2 goal2 = top - shot_position;
            double angle1 = goal1.cartesianAngle();
            double angle2 = goal2.cartesianAngle();
            intervals.Add(new edge(angle1, true));
            intervals.Add(new edge(angle2, false));

            // sorting edges so we can sweep over it
            List<edge> Sort = intervals.OrderBy(o => o.d).ToList();

            // sweeping over 2pi to find arcs where there are no occlusions and include the goal
            int status = 0;
            List<edge> statuses = new List<edge>();
            List<double> open_arc = new List<double>();
            for (int i = 0; i < Sort.Count(); i++)
            {
                if (status == 1)
                {
                    // in goal and no occlusion
                    statuses.Add(Sort[i]);
                    open_arc.Add(statuses[statuses.Count() - 1].d - statuses[statuses.Count() - 2].d);
                }
                if (Sort[i].b)
                {
                    // end of occlusion
                    status++;
                }
                else
                {
                    // start of occlusion
                    status--;
                }
                if (status == 1) statuses.Add(Sort[i]);
            }

            // finding biggest open arc
            double maximum = 0;
            int index = -1;
            for (int i = 0; i < open_arc.Count(); i++)
            {
                if (open_arc[i] > maximum)
                {
                    maximum = open_arc[i];
                    index = i;
                    
                }
            }

            // returning the angle in the middle and how wide an angle we have
            if (index == -1) return new ShotOpportunity(null, 0);
            double shot_angle = ((statuses[2 * index + 1].d + statuses[index * 2].d) / 2.0);
            double arc = open_arc[index];

            // finding intersection of shot with goal line
            double dx = Constants.FieldPts.THEIR_GOAL.X - shot_position.X;

            Vector2 shot_vec = new Vector2(Constants.FieldPts.THEIR_GOAL.X, shot_position.Y + dx * Math.Tan(shot_angle));

            return new ShotOpportunity(shot_vec, arc);
        }

    }
}
