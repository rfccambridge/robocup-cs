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
        // private static readonly State START_STATE = State.Victory;

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
            int numRobots =  5;
            for (int i = 0; i < numRobots; i++)
            {
                int robotId = fieldVision.GetRobots(team)[i].ID; // take first robot
                
                Vector2 center = new Vector2((Constants.Field.WIDTH /2) + (i * -1^(i)),0);
                RobotInfo destination = new RobotInfo(center, 0, team, robotId);
                RobotDestinationMessage destinationMessage = new RobotDestinationMessage(destination, true);

                msngr.SendMessage(destinationMessage);
                //firstRun = false;
            }
          //  }
        //}
            // end insert
        }

        private void victoryHandle(FieldVisionMessage fieldVision)
        {
            // TODO
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
