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
        const double avoid_radius = .6;

        public KickInBehavior(Team team, int goalie_id)
        {
            this.team = team;
            this.msngr = ServiceManager.getServiceManager();
            this.defense = new DefenseStrategy(team, goalie_id);
            this.offense = new OffenseStrategy(team, goalie_id);
        }

        public void DirectOurs(FieldVisionMessage msg)
        {
            offense.Handle(msg);
        }

        public void DirectTheirs(FieldVisionMessage msg)
        {
            defense.DefenseCommand(msg, 3, false, avoid_radius);
        }

        public void IndirectOurs(FieldVisionMessage msg)
        {
            offense.Handle(msg);
        }

        public void IndirectTheirs(FieldVisionMessage msg)
        {
            defense.DefenseCommand(msg, 3, false, avoid_radius);
        }
    }
}
