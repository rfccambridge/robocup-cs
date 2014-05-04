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
    public class SetupTest
    {
        
        Team team;
        bool stopped = false;
        KickOffBehavior testing;
        ServiceManager msngr;
        HowOffensive judge;
        PenaltyKickBehavior pen_behave;
        bool first = true;
        BounceKicker bk;

        public SetupTest(Team team, int goalie_id)
        {
            this.team = team;
            testing = new KickOffBehavior(team, 0);
            object lockObject = new object();
            bk = new BounceKicker(team);
            Vector2 bounce_loc = new Vector2(-1,.5);
            bk.reset(bounce_loc);
            new QueuedMessageHandler<FieldVisionMessage>(Handle, lockObject);
            msngr = ServiceManager.getServiceManager();
            msngr.RegisterListener<StopMessage>(stopMessageHandler, lockObject);
            
            // static debug
            
        }

        public void Handle(FieldVisionMessage fieldVision)
        {
            bk.arrange_kick(fieldVision, 1, 2);
            System.Threading.Thread.Sleep(100);
            
        }

        public void stopMessageHandler(StopMessage message)
        {
            stopped = true;
        }

    }
}
