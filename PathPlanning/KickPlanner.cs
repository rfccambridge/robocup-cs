using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RFC.Messaging;
using RFC.Core;
using RFC.Geometry;

namespace RFC.PathPlanning
{
    public class KickPlanner
    {
        // angle and distance to go from setting up to kick
        // to actually kicking
        const double heading_threshold = .10;
        const double dist_threshold = .05;
        const double kick_dist = .15;
        ServiceManager msngr;

        // how far back to stand. slightly more than radius
        
        double follow_through_dist = Constants.Basic.ROBOT_RADIUS * .5;

        public KickPlanner()
        {
            object lockObject = new object();
            new QueuedMessageHandler<KickMessage>(Handle, lockObject);
            msngr = ServiceManager.getServiceManager();
        }

        public void Handle(KickMessage kick)
        {
            
            BallVisionMessage bvm = msngr.GetLastMessage<BallVisionMessage>();
            if (bvm == null)
                return;

            BallInfo ball = bvm.Ball;
            RobotInfo robot = kick.Source;
            
            // calculating ideal place for the robot to be
            Vector2 diff = kick.Target - ball.Position;
            diff = new Vector2(0, 1);
            double angle = diff.cartesianAngle();
            Vector2 offset = diff.normalizeToLength(kick_dist);
            
            
            
            Vector2 position = ball.Position - offset;
            Vector2 followThroughOffset = diff.normalizeToLength(follow_through_dist);
            Vector2 followThroughPosition = ball.Position + followThroughOffset;
            
            RobotInfo ideal = new RobotInfo(position, angle, robot.ID);
            //ideal = new RobotInfo(new Vector2(), 0, robot.ID);
            RobotInfo idealFollowThrough = new RobotInfo(followThroughPosition, angle, robot.ID);
            msngr.db("dest: " + ideal.Position);
            msngr.db("dist: " + robot.Position.distance(ideal.Position));
            msngr.db("ang: " + Math.Abs(angle - robot.Orientation));

            // checking if we're close enough to start the actual kick
            if ((Math.Abs(angle - robot.Orientation) < heading_threshold) && (robot.Position.distance(ideal.Position) < dist_threshold))
            {
                msngr.db("close enough");
                // we are close enough
                RobotCommand cmd = new RobotCommand(robot.ID, RobotCommand.Command.FULL_BREAKBEAM_KICK);
                msngr.SendMessage<CommandMessage>(new CommandMessage(cmd));

                RobotDestinationMessage dest_msg = new RobotDestinationMessage(idealFollowThrough, false, false, true);
                msngr.SendMessage<RobotDestinationMessage>(dest_msg);
            }
            else
            {
                // not close enough
                msngr.db("not close enough");
                RobotCommand cmd = new RobotCommand(robot.ID, RobotCommand.Command.START_CHARGING);
                msngr.SendMessage<CommandMessage>(new CommandMessage(cmd));

                RobotDestinationMessage dest_msg = new RobotDestinationMessage(ideal,true,false,true);
                msngr.SendMessage<RobotDestinationMessage>(dest_msg);
            }
        }
    }
}
