﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RFC.Core;
using RFC.Messaging;

namespace RFC.Strategy
{
    public class KickTester : IMessageHandler<FieldVisionMessage>
    {
        Team team;
        int goalie_id;
        ServiceManager msngr;
        MidfieldPlay behave;
        int id;


        public KickTester(Team team, int goalie_id)
        {
            this.id = 2;
            this.team = team;
            this.goalie_id = goalie_id;
            msngr = ServiceManager.getServiceManager();
            object lockObject = new object();
            msngr.RegisterListener(this.LockingOn<FieldVisionMessage>(lockObject));

            behave = new MidfieldPlay(team, goalie_id);
        }

        public void HandleMessage(FieldVisionMessage msg)
        {
            KickMessage kick = new KickMessage(msg.GetRobot(team,id), Constants.FieldPts.THEIR_GOAL);
            msngr.SendMessage(kick);
        }
    }
}
