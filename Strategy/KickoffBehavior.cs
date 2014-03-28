using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RFC.Messaging;
using RFC.Core;

namespace RFC.Strategy
{
    public static class KickOffBehavior
    {

        public static void Ours(FieldVisionMessage msg, Team team)
        {
            //TODO
            // initial kick, then transition to normal play
        }

        public static void OursSetup(FieldVisionMessage msg, Team team)
        {
            //TODO
            // probably just hardcode in positions
        }

        public static void Theirs(FieldVisionMessage msg, Team team)
        {
            //TODO
            // detect when play has started, then switch to normal play
        }

        public static void TheirsSetup(FieldVisionMessage msg, Team team)
        {
            //TODO
            // probably just hardcoded positions
        }
    }
}
