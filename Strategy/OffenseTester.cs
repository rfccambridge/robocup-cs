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
    public class OffenseTester : IMessageHandler<FieldVisionMessage>
    {
        Team team;
        ServiceManager msngr;
        OffenseStrategy offense;
        int goalie_id;
        object lockObject;

        public OffenseTester(Team team, int goalie)
        {
            this.team = team;
            this.goalie_id = goalie;
            this.msngr = ServiceManager.getServiceManager();
            this.offense = new OffenseStrategy(team, goalie_id, Constants.Field.XMIN, 0);
            this.lockObject = new object();
            new QueuedMessageHandler<FieldVisionMessage>(this, lockObject);
        }

        public void HandleMessage(FieldVisionMessage msg)
        {
            offense.Handle(msg);
            System.Threading.Thread.Sleep(100);
        }
    }
}
