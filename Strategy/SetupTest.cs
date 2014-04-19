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

        public SetupTest(Team team, int goalie_id)
        {
            this.team = team;
            testing = new KickOffBehavior(team,0);
            pen_behave = new PenaltyKickBehavior(team, goalie_id);
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
                pen_behave.Ours(fieldVision);
            }
            first = false;
            Console.WriteLine("in loop");
            
        }

        public void stopMessageHandler(StopMessage message)
        {
            stopped = true;
        }

    }
}
