using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RFC.Core;
using RFC.Messaging;

namespace RFC.Strategy
{
    public class SetupTest1
    {
        Team team;
        int goalie_id;
        ServiceManager msngr;
        MidfieldPlay behave;
        

        public SetupTest1(Team team, int goalie_id)
        {
            this.team = team;
            this.goalie_id = goalie_id;
            this.msngr = ServiceManager.getServiceManager();
            object lockObject = new object();
            new QueuedMessageHandler<FieldVisionMessage>(Handle, lockObject);

            behave = new MidfieldPlay(team,goalie_id);
        }

        public void Handle(FieldVisionMessage msg)
        {
            KickMessage kick = new KickMessage(msg.GetRobots(team)[0],Constants.FieldPts.THEIR_GOAL);
            msngr.SendMessage(kick);
        }


    }
}
