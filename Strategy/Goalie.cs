using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RFC.Core;
using RFC.Messaging;

namespace RFC.Strategy
{
    class Goalie
    {
        public Goalie(Team team, int ID);

        public RobotInfo getGoalie(FieldVisionMessage msg)
        {
            msg.GetRobots(team)
        }
    }
}
