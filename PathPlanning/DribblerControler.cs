using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RFC.Core;
using RFC.Messaging;
using RFC.Geometry;

namespace RFC.PathPlanning
{
    public class DribblerControler : IMessageHandler<FieldVisionMessage>
    {
        private ServiceManager msngr;
        Team team;
        double threshold;
        Dictionary<int, double> dists;

        public DribblerControler(Team team)
        {
            this.msngr = ServiceManager.getServiceManager();
            this.team = team;
            object lockObj = new object();
            msngr.RegisterListener(this.Queued<FieldVisionMessage>(lockObj));
            this.threshold = Constants.Basic.ROBOT_RADIUS * 1.5;
            this.threshold = this.threshold * this.threshold;
            this.dists = new Dictionary<int, double>();
        }

        public void HandleMessage(FieldVisionMessage msg)
        {
            BallInfo ball = msg.Ball;
            Dictionary<int, double> new_dists = new Dictionary<int,double>();

            try
            {

                // calcing new distances
                if (ball == null)
                {
                    foreach (RobotInfo rob in msg.GetRobots(team))
                    {
                        new_dists[rob.ID] = 100;
                    }
                }
                else
                {
                    foreach (RobotInfo rob in msg.GetRobots(team))
                    {
                        new_dists[rob.ID] = rob.Position.distanceSq(ball.Position);
                    }
                }

                // sending dribbler commands
                foreach (RobotInfo rob in msg.GetRobots(team))
                {
                    int id = rob.ID;
                    if (dists[id] > threshold && new_dists[id] < threshold)
                    {
                        //now close
                        RobotCommand cmd = new RobotCommand(id, RobotCommand.Command.STOP_DRIBBLER);
                        msngr.SendMessage(new CommandMessage(cmd));
                    }
                    else if (dists[id] < threshold && new_dists[id] > threshold)
                    {
                        // now far
                        RobotCommand cmd = new RobotCommand(id, RobotCommand.Command.START_DRIBBLER);
                        msngr.SendMessage(new CommandMessage(cmd));
                    }
                }
                dists = new_dists;
            
            }
            catch
            {
                //
            }
                
        }

    }
}
