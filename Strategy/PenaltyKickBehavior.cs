using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RFC.Messaging;
using RFC.Core;

namespace RFC.Strategy
{
    public class PenaltyKickBehavior
    {
        Team team;

        public PenaltyKickBehavior(Team team)
        {
            this.team = team;
        }

        public void Ours(FieldVisionMessage msg)
        {
            //TODO
        }

        public void OursSetup(FieldVisionMessage msg)
        {
            //TODO
        }

        public void Theirs(FieldVisionMessage msg)
        {
            //TODO
        }
    }
}
