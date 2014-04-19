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

        public KickInBehavior(Team team, int goalie_id)
        {
            this.team = team;
            this.msngr = ServiceManager.getServiceManager();
        }

        public void DirectOurs(FieldVisionMessage msg)
        {
            //TODO
        }

        public void DirectTheirs(FieldVisionMessage msg)
        {
            //TODO
        }

        public void IndirectOurs(FieldVisionMessage msg)
        {
            //TODO
        }

        public void IndirectTheirs(FieldVisionMessage msg)
        {
            //TODO
        }
    }
}
