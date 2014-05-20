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
    public class GoalieTest
    {

        Team team;
        bool stopped = false;
        bool first = true;
        Goalie goalie;
        ServiceManager msngr;
        

        public GoalieTest(Team team, int id)
        {
            this.team = team;
            goalie = new Goalie(team, id);
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
                goalie.getGoalie(fieldVision);
                //KickMessage kmg = new KickMessage(fieldVision.GetRobot(team, 1), Constants.FieldPts.OUR_GOAL);
                //msngr.SendMessage(kmg);
            }
            first = false;

        }

        public void stopMessageHandler(StopMessage message)
        {
            stopped = true;
        }

    }
}
