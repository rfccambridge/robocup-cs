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
        ServiceManager msngr;

        public PenaltyKickBehavior(Team team)
        {
            this.team = team;
            this.msngr = ServiceManager.getServiceManager();
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
