using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RFC.Messaging;
using RFC.Core;
using RFC.Geometry;

namespace RFC.Strategy
{
    public class SetupTest
    {
        Team team;
        bool stopped = false;
        KickOffBehavior testing;
        ServiceManager msngr;
        HowOffensive judge;

        public SetupTest(Team team)
        {
            this.team = team;
            testing = new KickOffBehavior(team);
            object lockObject = new object();
            new QueuedMessageHandler<FieldVisionMessage>(Handle, lockObject);
            msngr = ServiceManager.getServiceManager();
            msngr.RegisterListener<StopMessage>(stopMessageHandler, lockObject);
            judge = new HowOffensive(team);

        }

        public void Handle(FieldVisionMessage fieldVision)
        {
            if (!stopped && fieldVision.GetRobots().Count > 0)
            {
                msngr.db("score: " + judge.Evaluate(fieldVision));
                testing.OursSetup(fieldVision);
            }
        }

        public void stopMessageHandler(StopMessage message)
        {
            stopped = true;
        }

    }
}
