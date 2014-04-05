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
        const double heading_threshold = .15;
        const double dist_threshold = .05;
        ServiceManager msngr;

        // how far back to stand. slightly more than radius
        double kick_dist = Constants.Basic.ROBOT_RADIUS * 1.5;

        public KickPlanner()
        {
            object lockObject = new object();
            new QueuedMessageHandler<KickMessage>(Handle, lockObject);
            msngr = ServiceManager.getServiceManager();
        }

        public void Handle(KickMessage kick)
        {
            BallVisionMessage bvm = msngr.GetLastMessage<BallVisionMessage>();
            BallInfo ball = bvm.Ball;
            RobotInfo robot = kick.Source;

            // calculating ideal place for the robot to be
            Vector2 diff = kick.Target - ball.Position;
            double angle = diff.cartesianAngle();
            Vector2 offset = diff.normalizeToLength(kick_dist);
            Vector2 position = ball.Position - offset;

            RobotInfo ideal = new RobotInfo(position, angle, robot.ID);

            // checking if we're close enough to start the actual kick
            if (Math.Abs(angle - robot.Orientation) < heading_threshold && robot.Position.distance(ideal.Position) < dist_threshold)
            {
                // we are close enough
                RobotCommand cmd = new RobotCommand(robot.ID, RobotCommand.Command.FULL_BREAKBEAM_KICK);
                msngr.SendMessage<CommandMessage>(new CommandMessage(cmd));

                RobotInfo ball_loc = new RobotInfo(ball.Position, angle, robot.ID);
                RobotDestinationMessage dest_msg = new RobotDestinationMessage(ball_loc, false, false, true);
                msngr.SendMessage<RobotDestinationMessage>(dest_msg);
            }
            else
            {
                // not close enough
                RobotCommand cmd = new RobotCommand(robot.ID, RobotCommand.Command.START_CHARGING);
                msngr.SendMessage<CommandMessage>(new CommandMessage(cmd));

                RobotDestinationMessage dest_msg = new RobotDestinationMessage(ideal,false,false,true);
                msngr.SendMessage<RobotDestinationMessage>(dest_msg);
            }
        }
    }
}
