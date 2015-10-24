using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RFC.Core;
using RFC.Messaging;
using RFC.Geometry;

namespace RFC.PathPlanning
{
    public class BreakBeamEmulator : IMessageHandler<CommandMessage>, IMessageHandler<FieldVisionMessage>
    {
        private Dictionary<int, bool> beamKicking;
        private Dictionary<int, DateTime> timeOuts;
        private ServiceManager msngr;
        private int RESET_TIME = 10000;
        private double THRESHOLD = .04;
        Team team;

        public BreakBeamEmulator(Team team)
        {
            this.msngr = ServiceManager.getServiceManager();
            this.beamKicking = new Dictionary<int, bool>();
            this.timeOuts = new Dictionary<int, DateTime>();
            this.team = team;
            object lockObj = new object();
            msngr.RegisterListener(this.Queued<CommandMessage>(lockObj));
            msngr.RegisterListener(this.Queued<FieldVisionMessage>(lockObj));
        }

        public void HandleMessage(CommandMessage msg)
        {
            if (msg.Command.command == RobotCommand.Command.FULL_BREAKBEAM_KICK)
            {
                beamKicking[msg.Command.ID] = true;
                timeOuts[msg.Command.ID] = DateTime.Now;
            }
        }

        public void HandleMessage(FieldVisionMessage msg)
        {
            int[] keys = beamKicking.Keys.ToArray();
            foreach (int i in keys)
            {
                if (!beamKicking[i])
                    continue;
                if ((int)(DateTime.Now - timeOuts[i]).TotalMilliseconds >= RESET_TIME)
                {
                    beamKicking[i] = false;
                    continue;
                }
                // see if close enough to kick
                RobotInfo kicker = msg.GetRobot(team, i);

                if (kicker == null)
                    continue;

                Point2 ideal = kicker.Position + Vector2.GetUnitVector(kicker.Orientation).normalizeToLength(Constants.Basic.ROBOT_FRONT_RADIUS);
                if (ideal.distance(msg.Ball.Position) < THRESHOLD)
                {
                    // kick!
                    RobotCommand cmd = new RobotCommand(i, RobotCommand.Command.KICK);
                    msngr.SendMessage(new CommandMessage(cmd));
                }

            }

        }

    }
}
