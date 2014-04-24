using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RFC.Messaging;
using RFC.Core;
using RFC.Geometry;
using RFC.PathPlanning;

namespace RFC.Strategy
{
    class BounceKicker
    {
        Team team;

        public BounceKicker (Team team)
        {
            this.team = team;
        }

        public void arrange_kick(FieldVisionMessage msg, int kicker, int bouncer)
        {
            RobotInfo kick = msg.GetRobot(team,kicker);
            RobotInfo bounce = msg.GetRobot(team, bouncer);
            KickMessage msg2 = new KickMessage(msg.GetRobot(team, kicker), msg.GetRobot(team, bouncer).Position);
            ServiceManager msngr = ServiceManager.getServiceManager();
            msngr.SendMessage(msg2);
            Vector2 vector1 = Constants.FieldPts.THEIR_GOAL - bounce.Position;
            Vector2 vector2 = kick.Position - bounce.Position;
            bounce.Orientation = (vector1.cartesianAngle() + vector2.cartesianAngle()) / 2.0;
            RobotDestinationMessage dest_msg = new RobotDestinationMessage(bounce, false, false, true);
            msngr.SendMessage<RobotDestinationMessage>(dest_msg);
            
            //start charger and turning on break beam sensor
            RobotCommand cmd = new RobotCommand(bouncer, RobotCommand.Command.START_CHARGING);
            msngr.SendMessage<CommandMessage>(new CommandMessage(cmd));
            cmd = new RobotCommand(bouncer, RobotCommand.Command.FULL_BREAKBEAM_KICK);
            msngr.SendMessage<CommandMessage>(new CommandMessage(cmd));
        }
    }
}

