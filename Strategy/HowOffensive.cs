using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RFC.Core;
using RFC.Messaging;
using RFC.Geometry;

namespace RFC.Strategy

{
    class HowOffensive
    {
        Team Ourteam;
        public HowOffensive(Team team)
        {
            Ourteam= team;
        }

        public float Evaluate(FieldVisionMessage msg)
        {

            List<RobotInfo> OurInfo = msg.GetRobots(Ourteam);
            BallInfo ball = msg.Ball;
            List<double> OurDistances = new List<double>();
            foreach (RobotInfo info in OurInfo)
            {
                OurDistances.Add(info.Position.distance(ball.Position));
            }
            Team TheirTeam;
            if (Ourteam == Team.Yellow)
                TheirTeam = Team.Blue;
            else
                TheirTeam = Team.Yellow;
            List<RobotInfo> TheirInfo = msg.GetRobots(Ourteam);
            List<double> TheirDistances = new List<double>();
            foreach (RobotInfo info in OurInfo)
            {
                
                    TheirDistances.Add(info.Position.distance(ball.Position));
            } 

            return 0;
            
        }
    }
}


                

    }
}
