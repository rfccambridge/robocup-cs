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
        bool first = true;

        public SetupTest(Team team)
        {
            this.team = team;
            testing = new KickOffBehavior(team);
            object lockObject = new object();
            new QueuedMessageHandler<FieldVisionMessage>(Handle, lockObject);
            msngr = ServiceManager.getServiceManager();
            msngr.RegisterListener<StopMessage>(stopMessageHandler, lockObject);

            // static debug

            
        }

        public void Handle(FieldVisionMessage fieldVision)
        {
            if (!first && fieldVision.GetRobots(team).Count() > 0)
            {
                RobotInfo rob = fieldVision.GetRobots(team)[0];
                msngr.SendMessage(new KickMessage(rob, Constants.FieldPts.THEIR_GOAL));
            }
            first = false;
            
        }

        public void stopMessageHandler(StopMessage message)
        {
            stopped = true;
        }

    }
}
