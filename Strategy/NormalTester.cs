﻿using System;
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
    public class NormalTester : IMessageHandler<FieldVisionMessage>
    {
        Team team;
        ServiceManager msngr;
        NormalBehavior normal;
        int goalie_id;
        NormalBehavior.State currentState;
        FieldDrawer.FieldDrawer fd;
        object lockObject;

        public NormalTester(Team team, int goalie, FieldDrawer.FieldDrawer fd)
        {
            this.team = team;
            this.goalie_id = goalie;
            this.msngr = ServiceManager.getServiceManager();
            this.normal = new NormalBehavior(team, goalie_id);
            this.currentState = NormalBehavior.State.Unknown;
            this.fd = fd;
            this.lockObject = new object();
            msngr.RegisterListener(this.Queued<FieldVisionMessage>(lockObject));
        }

        public void HandleMessage(FieldVisionMessage msg)
        {
            NormalBehavior.State currentState = normal.Play(msg);
            fd.Team = team;
            fd.PlayType = PlayType.NormalPlay;
            string playName;
            switch (currentState)
            {
                case NormalBehavior.State.Offense:
                    playName = "Offense";
                    break;
                case NormalBehavior.State.Midfield:
                    playName = "Midfield";
                    break;
                case NormalBehavior.State.Defense:
                    playName = "Defense";
                    break;
                default:
                    playName = "Special";
                    break;
            }
            fd.UpdatePlayName(team, 0, playName);
            System.Threading.Thread.Sleep(100);
        }
    }
}
