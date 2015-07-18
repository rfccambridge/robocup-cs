using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RFC.Core;
using RFC.Messaging;

namespace RFC.Strategy
{
    public class KickOffBehaviorTester
    {
        Team team;
        int goalie_id;
        ServiceManager msngr;
        KickOffBehavior kickOffBehavior;

        public KickOffBehaviorTester(Team team, int goalie_id)
        {
            this.team = team;
            this.goalie_id = goalie_id;
            this.msngr = ServiceManager.getServiceManager();
            kickOffBehavior = new KickOffBehavior(this.team, this.goalie_id);
            object lockObject = new object();
            new QueuedMessageHandler<FieldVisionMessage>(Handle, lockObject);
        }

        public void Handle(FieldVisionMessage msg)
        {
            //kickOffBehavior.OursSetup(msg);
            //kickOffBehavior.Ours(msg);
            kickOffBehavior.Theirs(msg);
        }


    }
}
