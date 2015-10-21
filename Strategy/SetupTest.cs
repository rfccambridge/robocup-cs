using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RFC.Messaging;
using RFC.Core;
using RFC.Geometry;
using RFC.PathPlanning;

namespace RFC.Strategy
{
    public class SetupTest : IMessageHandler<FieldVisionMessage>
    {
        Team team;
        ServiceManager msngr;
        KickOffBehavior ko;
        PenaltyKickBehavior pk;
        int goalie_id;
        object lockObject;
        BounceKicker bk;
        KickInBehavior dk;

        public SetupTest(Team team, int goalie)
        {
            this.team = team;
            this.goalie_id = goalie;
            this.msngr = ServiceManager.getServiceManager();
            this.ko = new KickOffBehavior(team, goalie);
            this.lockObject = new object();
            this.bk = new BounceKicker(team, goalie_id);
            this.bk.reset(new Vector2(1, 0));
            this.pk = new PenaltyKickBehavior(team, goalie);
            this.dk = new KickInBehavior(team, goalie);
            msngr.RegisterListener(this.Queued<FieldVisionMessage>(lockObject));
        }

        public void HandleMessage(FieldVisionMessage msg)
        {
            //bk.arrange_kick(msg,1,2);
            //pk.Ours(msg);
            //ko.TheirsSetup(msg);
            dk.DirectTheirs(msg);
        }

    }
}
