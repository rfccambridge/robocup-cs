using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RFC.Messaging;
using RFC.Core;

namespace RFC.Strategy
{
    public class KickOffBehavior
    {
        Team team;

        public KickOffBehavior(Team team)
        {
            this.team = team;
        }

        public void Ours(FieldVisionMessage msg)
        {
            //TODO
            // initial kick, then transition to normal play
        }

        public void OursSetup(FieldVisionMessage msg)
        {
            //TODO
            // probably just hardcode in positions
            // assume that we start on the left side of the field
            List<RobotInfo> ours = msg.GetRobots(team);

        }

        public void Theirs(FieldVisionMessage msg)
        {
            //TODO
            // detect when play has started, then switch to normal play
        }

        public void TheirsSetup(FieldVisionMessage msg)
        {
            //TODO
            // probably just hardcoded positions
        }
    }
}
