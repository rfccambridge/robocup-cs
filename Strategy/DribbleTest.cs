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
    public class DribbleTest
    {

        Team team;
        bool stopped = false;
        ServiceManager msngr;
        bool first = true;

        public DribbleTest(Team team, int goalie_id)
        {
            this.team = team;
            object lockObject = new object();
            new QueuedMessageHandler<FieldVisionMessage>(Handle, lockObject);
            msngr = ServiceManager.getServiceManager();
            msngr.RegisterListener<StopMessage>(stopMessageHandler, lockObject);

            // static debug

        }

        public void Handle(FieldVisionMessage msg)
        {

            if (!first && msg.GetRobots(team).Count() > 0)
            {
                msngr.SendMessage(new CommandMessage(new RobotCommand(1, RobotCommand.Command.START_DRIBBLER)));
                msngr.SendMessage(new RobotDestinationMessage(new RobotInfo(new Vector2(1,0), 0, team, 1), false, false, true));
                System.Threading.Thread.Sleep(100);

            }
            first = false;

        }

        public void stopMessageHandler(StopMessage message)
        {
            stopped = true;
        }

    }
}
