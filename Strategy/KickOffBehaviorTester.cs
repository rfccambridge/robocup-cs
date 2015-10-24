using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RFC.Core;
using RFC.Messaging;

namespace RFC.Strategy
{
    public class KickOffBehaviorTester : IMessageHandler<FieldVisionMessage>
    {
        Team team;
        int goalie_id;
        ServiceManager msngr;
        KickOffBehavior kickOffBehavior;

        public KickOffBehaviorTester(Team team, int goalie_id)
        {
            this.team = team;
            this.goalie_id = goalie_id;
            msngr = ServiceManager.getServiceManager();
            kickOffBehavior = new KickOffBehavior(this.team, this.goalie_id);
            object lockObject = new object();
            msngr.RegisterListener(this.Queued<FieldVisionMessage>(lockObject));
        }

        public void HandleMessage(FieldVisionMessage msg)
        {
            //kickOffBehavior.OursSetup(msg);
            //kickOffBehavior.Ours(msg);
            kickOffBehavior.Theirs(msg);
        }


    }
}
