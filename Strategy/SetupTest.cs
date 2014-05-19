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
        ServiceManager msngr;
        KickOffBehavior ko;
        int goalie_id;
        object lockObject;
        BounceKicker bk;

        public SetupTest(Team team, int goalie)
        {
            this.team = team;
            this.goalie_id = goalie;
            this.msngr = ServiceManager.getServiceManager();
            this.ko = new KickOffBehavior(team, goalie);
            this.lockObject = new object();
            this.bk = new BounceKicker(team, goalie_id);
            new QueuedMessageHandler<FieldVisionMessage>(Handle, lockObject);
        }

        public void Handle(FieldVisionMessage msg)
        {
            //bk.arrange_kick(msg,3,5);
            ko.OursSetup(msg);
        }

    }
}
