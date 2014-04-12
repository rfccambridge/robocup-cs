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
        Team team;
        public Goalie(Team team, int ID)
        {
            this.team = team;
        }
        public RobotInfo getGoalie(FieldVisionMessage msg)
        {
            List<RobotInfo> ours = msg.GetRobots(team);
            return null;
        }
    }
}
