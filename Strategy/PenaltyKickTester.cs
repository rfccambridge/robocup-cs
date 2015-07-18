﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RFC.Core;
using RFC.Messaging;

namespace RFC.Strategy
{
    public class PenaltyKickTester
    {
        Team team;
        int goalie_id;
        ServiceManager msngr;
        PenaltyKickBehavior penaltyKickBehavior;

        public PenaltyKickTester(Team team, int goalie_id)
        {
            this.team = team;
            this.goalie_id = goalie_id;
            this.msngr = ServiceManager.getServiceManager();
            penaltyKickBehavior = new PenaltyKickBehavior(this.team, this.goalie_id);
            object lockObject = new object();
            new QueuedMessageHandler<FieldVisionMessage>(Handle, lockObject);
        }

        public void Handle(FieldVisionMessage msg)
        {
            //penaltyKickBehavior.OursSetup(msg);
            penaltyKickBehavior.Ours(msg);
            //penaltyKickBehavior.Theirs(msg);
        }


    }
}
