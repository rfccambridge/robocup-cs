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
       
        class edge{
            public double d;
            public bool b;
            public edge(double d, bool b)
            {
                this.d = d;
                this.b = b;
            }
        }

        public List <double> evaluate (FieldVisionMessage fvm) {
            List <RobotInfo> location = fvm.GetRobots();
            List <edge> intervals = new List <edge> ();
            for (int i = 0; i < location.Count(); i++){
                RobotInfo robot = location[i]; 
                Vector2 difference = robot.Position - fvm.Ball.Position;
                double sweep = Math.Asin(Constants.Basic.ROBOT_RADIUS/ difference.magnitude());
                double angle = difference.cartesianAngle();
                intervals.Add(new edge(angle - sweep, false));
                intervals.Add(new edge(angle + sweep, true)); 
            }
            
            Vector2 goal1 = Constants.FieldPts.THEIR_GOAL_BOTTOM - fvm.Ball.Position;
            Vector2 goal2 = Constants.FieldPts.THEIR_GOAL_TOP - fvm.Ball.Position;
            double angle1 = goal1.cartesianAngle();
            double angle2 = goal2.cartesianAngle();
            intervals.Add(new edge(angle1, true));
            intervals.Add(new edge(angle2, false));
            List <edge> Sort = intervals.OrderBy(o => o.d).ToList();

            int status = 0;
            List<edge> statuses = new List<edge>();
            List <double> open_arc = new List <double> ();
            for (int i = 0; i < Sort.Count(); i++){
                if (status == 1){
                    statuses.Add(Sort[i]);
                    open_arc.Add(statuses[i].d - statuses[i - 1].d);
                }
                if (Sort[i].b){
                    status++;
                } else {
                    status--;
                }
                if (status == 1) statuses.Add(Sort[i]);          
            }

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

            List<double> shot_arc = new List<double>();
           double shot = ((statuses[(index - 1) * 2 + 1].d + statuses[index * 2].d)/2.0);
           double arc = open_arc[2*index];  
           shot_arc.Add(shot);
           shot_arc.Add(arc);
           return shot_arc;
        }

    }
}
