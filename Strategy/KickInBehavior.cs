using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RFC.Messaging;
using RFC.Core;

namespace RFC.Strategy
{
    public class KickInBehavior
    {
        Team team;
        ServiceManager msngr;
        DefenseStrategy defense;
        OffenseStrategy offense;
        double avoid_radius;
        int goalie_id;

        public KickInBehavior(Team team, int goalie_id)
        {
            this.team = team;
            this.msngr = ServiceManager.getServiceManager();
            this.defense = new DefenseStrategy(team, goalie_id,DefenseStrategy.PlayType.KickIn);
            this.offense = new OffenseStrategy(team, goalie_id);
            this.avoid_radius = .5 + Constants.Basic.ROBOT_RADIUS;
            this.goalie_id = goalie_id;
        }

        public void DirectOurs(FieldVisionMessage msg)
        {
            offense.Handle(msg);
        }

        public void DirectTheirs(FieldVisionMessage msg)
        {
            int n = msg.GetRobotsExcept(team, goalie_id).Count;
            defense.DefenseCommand(msg, Math.Min(3, n), false, avoid_radius);
        }

        public void IndirectOurs(FieldVisionMessage msg)
        {
            offense.Handle(msg);
        }

        public void IndirectTheirs(FieldVisionMessage msg)
        {
            int n = msg.GetRobotsExcept(team, goalie_id).Count;
            defense.DefenseCommand(msg, Math.Min(3,n), false, avoid_radius);
        }
    }
}
