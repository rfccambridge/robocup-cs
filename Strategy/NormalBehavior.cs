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
    public class NormalBehavior
    {
        Team team;
        ServiceManager msngr;

        public NormalBehavior(Team team, int goalie_id)
        {
            this.team = team;
            this.msngr = ServiceManager.getServiceManager();
        }

        public void Play(FieldVisionMessage msg)
        {
            // our main strategy
            msngr.SendMessage(new KickMessage(msg.GetRobot(team,1), Constants.FieldPts.THEIR_GOAL));
            //msngr.SendMessage(new RobotDestinationMessage(new RobotInfo(new Vector2(), 0, 1),false,false));
        }
    }
}
