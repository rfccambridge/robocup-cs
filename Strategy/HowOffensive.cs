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

        // takes in the field positions and outputs a scalar of how good our position is
        // basically should we be playing offense, midfield, or defense
        // high score is offensive
        public double Evaluate(FieldVisionMessage msg)
        {
            // take a linear combination of distances to the ball for the 1st, 2nd, 3rd closest robots to the ball, etc
            double scalar = 10;
            double sum = 0; 
            double max_distance = 2;
            int[] factors = {5,3,3,2,2,0,0}; //6 robots max, 1 for luck :)
            double position_coefficient = 3; // +x is towards our goal
            BallInfo ball = msg.Ball;

            List<RobotInfo> OurInfo = msg.GetRobots(Ourteam);
            List<double> OurDistances = new List<double>();
            foreach (RobotInfo info in OurInfo)
            {
                OurDistances.Add(info.Position.distance(ball.Position));
            }

            Team TheirTeam = Team.Yellow;
            if (Ourteam == Team.Yellow)
                TheirTeam = Team.Blue;
            
            List<RobotInfo> TheirInfo = msg.GetRobots(TheirTeam);
            List<double> TheirDistances = new List<double>();
            foreach (RobotInfo info in TheirInfo)
            {
                TheirDistances.Add(info.Position.distance(ball.Position));
            }
 
            // if the number of robots are different, add robots at the maximum distance
            int diff = TheirDistances.Count() - OurDistances.Count();
            if (diff > 0)
            {
                // they have more robots
                for (int i = 0; i < diff; i++)
                    OurDistances.Add(max_distance);
            }
            else if (diff < 0)
            {
                // we have more
                for (int i = 0; i < -diff; i++)
                    TheirDistances.Add(max_distance);
            }

            // clipping distances at max distance
            for (int i = 0; i < TheirDistances.Count(); i++)
            {
                TheirDistances[i] = Math.Min(TheirDistances[i],max_distance);
                OurDistances[i] = Math.Min(OurDistances[i],max_distance);
            }

            // sorting lists of distances to compare closest to closest, etc
            OurDistances.Sort();
            TheirDistances.Sort(); 

            // adding up distances by factors
            for (int i=0; i<OurDistances.Count(); i++)
            {
                sum += (TheirDistances[i]- OurDistances[i])*factors[i];
            }

            // adding field position
            sum += ball.Position.X * position_coefficient;

            return sum / scalar;
            
        }
    }
}