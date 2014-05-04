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
        Progress progress;
        ServiceManager msngr;
        double critical_radius = .3;
        Vector2 bounce_loc;

        public BounceKicker (Team team)
        {
            this.team = team;
            this.progress = Progress.Far;

            msngr = ServiceManager.getServiceManager();
        }

        public void reset(Vector2 bounce_loc)
        {
            this.progress = Progress.Far;
            this.bounce_loc = bounce_loc;
        }

        

        public void arrange_kick(FieldVisionMessage msg, int kicker_id, int bouncer_id)
        {
            RobotInfo kicker = msg.GetRobot(team, kicker_id);
            RobotInfo bounce = msg.GetRobot(team, bouncer_id);

            // common commands
            RobotCommand charge_cmd = new RobotCommand(bouncer_id, RobotCommand.Command.START_CHARGING);
            msngr.SendMessage<CommandMessage>(new CommandMessage(charge_cmd));
            RobotCommand bb_cmd = new RobotCommand(bouncer_id, RobotCommand.Command.FULL_BREAKBEAM_KICK);
            msngr.SendMessage<CommandMessage>(new CommandMessage(bb_cmd));

            // common vectors
            Vector2 toGoal = Shot1.evaluate(msg, team, msg.Ball.Position).target - bounce_loc;
            Vector2 toBall = msg.Ball.Position - bounce_loc;
            bounce.Orientation = (2*toGoal.cartesianAngle() + toBall.cartesianAngle())/3;

            Vector2 radVec = new Vector2(bounce.Orientation);
            radVec = radVec.normalizeToLength(Constants.Basic.ROBOT_FRONT_RADIUS);
            bounce.Position = bounce_loc - radVec;
            switch(progress)
            {
                case Progress.Far:
                    KickMessage msg1 = new KickMessage(kicker, bounce_loc);
                    msngr.SendMessage(msg1);
                  
                    //changing state from far to near
                    if (kicker.Position.distance(msg.Ball.Position) < critical_radius)
                    {
                        this.progress = Progress.Near;
                    } 
                    break;

                case Progress.Near:
                    KickMessage msg2 = new KickMessage(kicker, bounce_loc);
                    msngr.SendMessage(msg2);

                    // changing states
                    if (kicker.Position.distance(msg.Ball.Position) > 1.5 * critical_radius)
                    {
                        this.progress = Progress.Kicked;
                    }
                    break;
                case Progress.Kicked:
                    bounce.Position = msg.Ball.Position + (bounce.Position - msg.Ball.Position).projectionLength(msg.Ball.Velocity)*msg.Ball.Velocity;

                    // changing states
                    if (bounce.Position.distance(msg.Ball.Position) < critical_radius)
                    {
                        this.progress = Progress.Bouncing;
                    }
                    break;

                case Progress.Bouncing:
                    bounce.Position = msg.Ball.Position + (bounce.Position - msg.Ball.Position).projectionLength(msg.Ball.Velocity)*msg.Ball.Velocity; 

                    // changing states
                    if (bounce.Position.distance(msg.Ball.Position) > 1.5 * critical_radius)
                    {
                        this.progress = Progress.Bounced;
                    }
                    break;

                case Progress.Bounced:
                    break;
            }

            RobotDestinationMessage dest_bnc = new RobotDestinationMessage(bounce, false, false, true);
            msngr.SendMessage<RobotDestinationMessage>(dest_bnc);
        }
    }
}

