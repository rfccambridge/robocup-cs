using RFC.Core;
using RFC.Geometry;
using RFC.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RFC.Strategy
{
    public class TimeoutBehavior
    {
        private enum State { Timeout, Victory };
        private DateTime startTime;
        private Team team;
        private int goalie_id;
        private ServiceManager msngr;
        private object lockObject;

        private static readonly State STATE = State.Timeout;
        // private static readonly State STATE = State.Victory;

        public TimeoutBehavior(Team team, int goalie_id)
        {
            this.startTime = DateTime.Now;
            this.team = team;
            this.goalie_id = goalie_id;
            this.msngr = ServiceManager.getServiceManager();
            this.lockObject = new object();
            new QueuedMessageHandler<FieldVisionMessage>(Handle, lockObject);
        }

        private void timeoutHandle(FieldVisionMessage fieldVision)
        {
            List<RobotInfo> robots = fieldVision.GetRobots(team);
            int numRobots = robots.Count;
            for (int i = 0; i < numRobots; i++)
            {
                double xDisp = ((i + 1 - numRobots / 2.0) * Constants.Basic.ROBOT_RADIUS * 3.0);
                Vector2 pos = Constants.FieldPts.TOP - (new Vector2(xDisp, 0));
                RobotInfo ri = new RobotInfo(pos, 0, team, i);
                msngr.SendMessage(new RobotDestinationMessage(ri, true));
            }
        }

        private void victoryHandle(FieldVisionMessage fieldVision)
        {
            List<RobotInfo> robots = fieldVision.GetRobots(team);
            int numRobots = robots.Count;
            double phase = 2 * Math.PI / robots.Count;
            double r = 0.5;
            int t = (int)(DateTime.Now - startTime).TotalMilliseconds;
            double w = 1.0 / 1000.0;
            for (int i = 0; i < numRobots; i++)
            {
                Vector2 pos = new Vector2(r * Math.Cos(w * t + phase * i), r * Math.Sin(w * t + phase * i));
                RobotInfo ri = new RobotInfo(pos, 0, team, i);
                msngr.SendMessage(new RobotDestinationMessage(ri, true));
            }
        }

        public void Handle(FieldVisionMessage fieldVision)
        {
            switch (STATE)
            {
                case State.Timeout:
                    timeoutHandle(fieldVision);
                    break;

                case State.Victory:
                    victoryHandle(fieldVision);
                    break;
            }
        }
    }
}
