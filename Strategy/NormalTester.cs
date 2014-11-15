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
    public class NormalTester
    {
        Team team;
        ServiceManager msngr;
        NormalBehavior normal;
        int goalie_id;
        object lockObject;

        public NormalTester(Team team, int goalie)
        {
            this.team = team;
            this.goalie_id = goalie;
            this.msngr = ServiceManager.getServiceManager();
            this.normal = new NormalBehavior(team, goalie_id);
            this.lockObject = new object();
            new QueuedMessageHandler<FieldVisionMessage>(Handle, lockObject);
        }

        public void Handle(FieldVisionMessage msg)
        {
            normal.Play(msg);
            System.Threading.Thread.Sleep(100);
        }
    }
}
