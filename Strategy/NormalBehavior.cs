using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RFC.Messaging;
using RFC.Core;

namespace RFC.Strategy
{
    public class NormalBehavior
    {
        Team team;

        public NormalBehavior(Team team)
        {
            this.team = team;
        }

        public void Play(FieldVisionMessage msg)
        {
            // our main strategy
        }
    }
}
