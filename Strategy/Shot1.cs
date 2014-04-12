using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RFC.Core;
using RFC.Messaging;
using RFC.Geometry;

namespace RFC.Strategy
{
    class Shot1
    {
        Team team;

        public Shot1 (Team team2){
               team = team2;
        }
       
        // private class to represent the beginning or end of something
        // that blocks vision to the goal
        // false is the start of the blocked vision, true is end
        class edge{
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
        public List <double> evaluate (FieldVisionMessage fvm) {
            List <RobotInfo> locations = fvm.GetRobots();

            // finding beginning and end of all occlusions from other robots as edges
            List <edge> intervals = new List <edge> ();
            for (int i = 0; i < locations.Count(); i++){
                RobotInfo robot = locations[i]; 
                Vector2 difference = robot.Position - fvm.Ball.Position;
                double sweep = Math.Asin(Constants.Basic.ROBOT_RADIUS/ difference.magnitude());
                double angle = difference.cartesianAngle();
                intervals.Add(new edge(angle - sweep, false));
                intervals.Add(new edge(angle + sweep, true)); 
            }
            
            // adding in edges of goal
            Vector2 goal1 = Constants.FieldPts.THEIR_GOAL_BOTTOM - fvm.Ball.Position;
            Vector2 goal2 = Constants.FieldPts.THEIR_GOAL_TOP - fvm.Ball.Position;
            double angle1 = goal1.cartesianAngle();
            double angle2 = goal2.cartesianAngle();
            intervals.Add(new edge(angle1, true));
            intervals.Add(new edge(angle2, false));

            // sorting edges so we can sweep over it
            List <edge> Sort = intervals.OrderBy(o => o.d).ToList();

            // sweeping over 2pi to find arcs where there are no occlusions and include the goal
            int status = 0;
            List<edge> statuses = new List<edge>();
            List <double> open_arc = new List <double> ();
            for (int i = 0; i < Sort.Count(); i++){
                if (status == 1){
                    // in goal and no occlusion
                    statuses.Add(Sort[i]);
                    open_arc.Add(statuses[i].d - statuses[i - 1].d);
                }
                if (Sort[i].b){
                    // end of occlusion
                    status++;
                } else {
                    // start of occlusion
                    status--;
                }
                if (status == 1) statuses.Add(Sort[i]);          
            }

            // finding biggest open arc
            double maximum = 0;
            int index = 0;
            for (int i = 0; i < open_arc.Count(); i++)
            {
                if (open_arc[i] > maximum)
                {
                    maximum = open_arc[i];
                    index = i;
                }
            }

            // returning the angle in the middle and how wide an angle we have
            List<double> shot_arc = new List<double>();
            double shot = ((statuses[(index - 1) * 2 + 1].d + statuses[index * 2].d)/2.0);
            double arc = open_arc[2*index];  
            shot_arc.Add(shot);
            shot_arc.Add(arc);
            return shot_arc;
        }

    }
}
