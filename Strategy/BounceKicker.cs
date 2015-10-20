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
        Point2 bounce_loc;
        int goalie_id;

        // constants used to make sure robots far away from shots going on
        private double SHOOT_AVOID = Constants.Basic.ROBOT_RADIUS + 0.1;
        private double AVOID_RADIUS = 1.0;
        private const double SHOOT_AVOID_DT = 0.5;

        public BounceKicker (Team team, int goalie_id)
        {
            this.team = team;
            this.progress = Progress.Far;
            this.goalie_id = goalie_id;

            msngr = ServiceManager.getServiceManager();
        }

        public void reset(Point2 bounce_loc)
        {
            this.progress = Progress.Far;
            this.bounce_loc = bounce_loc;
        }

        private void getOutOfWay(RobotInfo ri, RobotInfo kicker, RobotInfo bouncer, Vector2 toGoal)
        {
            /*
            Vector2 fromBallSource = null;
            Vector2 perpVec = null;
            switch (progress)
            {
                case Progress.Far:
                    break;
                // when ball going to bouncer, make sure robot not close to ball
                case Progress.Near:
                case Progress.Kicked:
                    fromBallSource = (ri.Position + ri.Velocity * SHOOT_AVOID_DT) - kicker.Position;
                    perpVec = fromBallSource.perpendicularComponent(bouncer.Position - kicker.Position);
                    break;
                // when ball going to goal, make sure robot not close to ball
                case Progress.Bouncing:
                case Progress.Bounced:
                    fromBallSource = (ri.Position + ri.Velocity * SHOOT_AVOID_DT) - bouncer.Position;
                    perpVec = fromBallSource.perpendicularComponent(toGoal);
                    break;
                default:
                    break;
            }
            if (fromBallSource != null && perpVec != null && perpVec.magnitude() < SHOOT_AVOID)
            {
                Vector2 dest = perpVec * (SHOOT_AVOID / perpVec.magnitude());
                RobotInfo destRI = new RobotInfo(dest, ri.Orientation, team, ri.ID);
                msngr.SendMessage(new RobotDestinationMessage(destRI, true, false));
            }
             * */
            // This was edited 3/28/15 to include 4 extra argument due to new avoid implementation 
            // that considers both half circles. To revert check Avoider.cs
            RobotInfo dest = Avoider.avoid(ri, kicker.Position, AVOID_RADIUS, bouncer.Position, AVOID_RADIUS,
                kicker.Position, AVOID_RADIUS, bouncer.Position, AVOID_RADIUS);
            msngr.SendMessage(new RobotDestinationMessage(dest, true, false));
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

            foreach (RobotInfo ri in msg.GetRobotsExcept(team, goalie_id))
            {
                if (ri.ID != kicker_id && ri.ID != bouncer_id)
                {
                    getOutOfWay(ri, kicker, bounce, toGoal);
                }
            }

            Vector2 radVec = Vector2.GetUnitVector(bounce.Orientation);
            radVec = radVec.normalizeToLength(Constants.Basic.ROBOT_FRONT_RADIUS);
            bounce.Position = bounce_loc - radVec;
            // Console.WriteLine(progress);
            
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
                    //if (msg.Ball.Velocity.magnitude() > 0.5)
                    //{
                        bounce.Position = msg.Ball.Position + (bounce.Position - msg.Ball.Position).projectionLength(msg.Ball.Velocity) * msg.Ball.Velocity.normalize();
                    //}

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

            RobotDestinationMessage dest_bnc = new RobotDestinationMessage(bounce, false);
            msngr.SendMessage<RobotDestinationMessage>(dest_bnc);
             
        }

        public static double getBounceOrientation(Vector2 toBall, Vector2 toGoal)
        {
            return (2 * toGoal.cartesianAngle() + toBall.cartesianAngle()) / 3.0;
        }
    }
}

