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

        public KickInBehavior(Team team)
        {
            this.team = team;
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
