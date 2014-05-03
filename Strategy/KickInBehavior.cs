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
            // call offense
        }

        public void DirectTheirs(FieldVisionMessage msg)
        {
            //TODO
            // call midfield wall
        }

        public void IndirectOurs(FieldVisionMessage msg)
        {
            //TODO
            // call offense
        }

        public void IndirectTheirs(FieldVisionMessage msg)
        {
            //TODO
            // call midfield wall
        }
    }
}
