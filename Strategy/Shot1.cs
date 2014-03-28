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

        public void evaluate (FieldVisionMessage fvm) {
            List <RobotInfo> location = fvm.GetRobots();
            foreach (RobotInfo i in location) {

                Vector2 difference = i.Position - fvm.Ball.Position;
            }

        }

    }
}
