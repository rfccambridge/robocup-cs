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
        public enum Progress 
        {
            Far,
            Near,
            Kicked,
            Bouncing,
            Bounced
        }

        Team team;
        double critical_radius = .1;
        Progress progress;

        public BounceKicker (Team team)
        {
            this.team = team;
        }

        public void reset(Vector2 bounce_loc)
        {
            this.progress = Progress.Far;
        }

        

        public void arrange_kick(FieldVisionMessage msg, int kicker, int bouncer)
        {
            RobotInfo kick = msg.GetRobot(team,kicker);
            RobotInfo bounce = msg.GetRobot(team, bouncer);
            switch(progress)
            {
                case Progress.Far:
                    KickMessage msg1 = new KickMessage(msg.GetRobot(team, kicker), msg.GetRobot(team, bouncer).Position);
                    ServiceManager msngr = ServiceManager.getServiceManager();
                    msngr.SendMessage(msg1);
                    Vector2 vector1 = Constants.FieldPts.THEIR_GOAL - bounce.Position;
                    Vector2 vector2 = msg.Ball.Position - bounce.Position;
                    bounce.Orientation = (vector1.cartesianAngle() + vector2.cartesianAngle()) / 2.0;
                    RobotDestinationMessage dest_msg1 = new RobotDestinationMessage(bounce, false, false, true);
                    msngr.SendMessage<RobotDestinationMessage>(dest_msg1);

                    RobotCommand cmd1 = new RobotCommand(bouncer, RobotCommand.Command.START_CHARGING);
                    msngr.SendMessage<CommandMessage>(new CommandMessage(cmd1));

                    //changing state from far to near
                    if (kick.Position.distance(msg.Ball.Position) < critical_radius)
                    {
                        this.progress = Progress.Near;
                    }
                    break;
                case Progress.Near:
                    KickMessage msg2 = new KickMessage(msg.GetRobot(team, kicker), msg.GetRobot(team, bouncer).Position);
                    msngr = ServiceManager.getServiceManager();
                    msngr.SendMessage(msg2);
                    Vector2 vector3 = Constants.FieldPts.THEIR_GOAL - bounce.Position;
                    Vector2 vector4 = msg.Ball.Position - bounce.Position;
                    bounce.Orientation = (vector3.cartesianAngle() + vector4.cartesianAngle()) / 2.0;
                    RobotDestinationMessage dest_msg2 = new RobotDestinationMessage(bounce, false, false, true);
                    msngr.SendMessage<RobotDestinationMessage>(dest_msg2);

                    RobotCommand cmd2 = new RobotCommand(bouncer, RobotCommand.Command.START_CHARGING);
                    msngr.SendMessage<CommandMessage>(new CommandMessage(cmd2));

                    if (kick.Position.distance(msg.Ball.Position) > 1.5 * critical_radius)
                    {
                        this.progress = Progress.Kicked;
                    }
                    break;
                case Progress.Kicked:
                    KickMessage msg3 = new KickMessage(msg.GetRobot(team, kicker), msg.GetRobot(team, bouncer).Position);
                    msngr = ServiceManager.getServiceManager();
                    msngr.SendMessage(msg3);
                    Vector2 vector5 = Constants.FieldPts.THEIR_GOAL - bounce.Position;
                    Vector2 vector6 = msg.Ball.Position - bounce.Position;
                    bounce.Orientation = (vector5.cartesianAngle() + vector6.cartesianAngle()) / 2.0;
                    RobotDestinationMessage dest_msg3 = new RobotDestinationMessage(bounce, false, false, true);
                    msngr.SendMessage<RobotDestinationMessage>(dest_msg3);

                    RobotCommand cmd3 = new RobotCommand(bouncer, RobotCommand.Command.START_CHARGING);
                    msngr.SendMessage<CommandMessage>(new CommandMessage(cmd3));

                    RobotCommand cmd4 = new RobotCommand(bouncer, RobotCommand.Command.FULL_BREAKBEAM_KICK);
                    msngr.SendMessage<CommandMessage>(new CommandMessage(cmd4));
                    
                    if (bounce.Position.distance(msg.Ball.Position) < critical_radius)
                    {
                        this.progress = Progress.Bouncing;
                    }
                    break;
                case Progress.Bouncing:
                    if (bounce.Position.distance(msg.Ball.Position) > 1.5 * critical_radius)
                    {
                        this.progress = Progress.Bounced;
                    }
                    break;
                case Progress.Bounced:
                    break;
            }

            
            //KickMessage msg2 = new KickMessage(msg.GetRobot(team, kicker), msg.GetRobot(team, bouncer).Position);
            //ServiceManager msngr = ServiceManager.getServiceManager();
            //msngr.SendMessage(msg2);
            //Vector2 vector1 = Constants.FieldPts.THEIR_GOAL - bounce.Position;
            //Vector2 vector2 = msg.Ball.Position - bounce.Position;
            //bounce.Orientation = (vector1.cartesianAngle() + vector2.cartesianAngle()) / 2.0;
            //RobotDestinationMessage dest_msg = new RobotDestinationMessage(bounce, false, false, true);
            //msngr.SendMessage<RobotDestinationMessage>(dest_msg);
            
            //move bouncer to intercept ball


            
            //start charger and turning on break beam sensor
            //RobotCommand cmd1 = new RobotCommand(bouncer, RobotCommand.Command.START_CHARGING);
            //msngr.SendMessage<CommandMessage>(new CommandMessage(cmd1));

            //RobotCommand cmd2 = new RobotCommand(bouncer, RobotCommand.Command.FULL_BREAKBEAM_KICK);
            //msngr.SendMessage<CommandMessage>(new CommandMessage(cmd2));
        }
    }
}

