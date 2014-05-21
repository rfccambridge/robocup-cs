using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RFC.Core;
using RFC.Messaging;
using RFC.Geometry;

namespace RFC.PathPlanning
{
    public class BreakBeamEmulator
    {
        private Dictionary<int, bool> beamKicking;
        private Dictionary<int, DateTime> timeOuts;
        private ServiceManager msngr;
        private int RESET_TIME = 10000;
        private double THRESHOLD = .05;
        Team team;

        public BreakBeamEmulator(Team team)
        {
            this.msngr = ServiceManager.getServiceManager();
            this.beamKicking = new Dictionary<int, bool>();
            this.timeOuts = new Dictionary<int, DateTime>();
            this.team = team;
            object lockObj = new object();
            new QueuedMessageHandler<CommandMessage>(handleRobotCommandMessage, lockObj);
            new QueuedMessageHandler<FieldVisionMessage>(handleFieldVisionMessage, lockObj);
        }

        public void handleRobotCommandMessage(CommandMessage msg)
        {
            if (msg.Command.command == RobotCommand.Command.FULL_BREAKBEAM_KICK)
            {
                beamKicking[msg.Command.ID] = true;
                timeOuts[msg.Command.ID] = DateTime.Now;
            }
        }

        public void handleFieldVisionMessage(FieldVisionMessage msg)
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

                Vector2 ideal = kicker.Position + new Vector2(kicker.Orientation).normalizeToLength(Constants.Basic.ROBOT_FRONT_RADIUS);
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
