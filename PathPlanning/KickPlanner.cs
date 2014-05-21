using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RFC.Messaging;
using RFC.Core;
using RFC.Geometry;
using RFC.Utilities;

namespace RFC.PathPlanning
{
    public class KickPlanner
    {
        // angle and distance to go from setting up to kick
        // to actually kicking
        const double heading_threshold = .10;
        const double dist_threshold = .02;
        const double kick_dist = .15;
        double follow_through_dist = Constants.Basic.ROBOT_RADIUS * .5;
        ServiceManager msngr;

        // how far back to stand. slightly more than radius
        
        

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
            double angle = diff.cartesianAngle();
            Vector2 offset = diff.normalizeToLength(kick_dist);
            
            Vector2 position = ball.Position - offset;
            Vector2 followThroughOffset = diff.normalizeToLength(follow_through_dist);
            Vector2 followThroughPosition = ball.Position + followThroughOffset;
            
            RobotInfo ideal = new RobotInfo(position, angle,robot.Team, robot.ID);
            //ideal = new RobotInfo(new Vector2(), 0, robot.ID);
            RobotInfo idealFollowThrough = new RobotInfo(followThroughPosition, angle,robot.Team, robot.ID);

            // checking if we're close enough to start the actual kick
            StateChecker isClose = new StateChecker(new LineSegment(ball.Position, position - 50*offset), dist_threshold);

            if (AngUtils.compare(angle, robot.Orientation) < heading_threshold && isClose.check(robot.Position))
            {
                // we are close enough
                RobotCommand cmd = new RobotCommand(robot.ID, RobotCommand.Command.START_CHARGING);
                msngr.SendMessage<CommandMessage>(new CommandMessage(cmd));
                RobotCommand cmd2 = new RobotCommand(robot.ID, RobotCommand.Command.FULL_BREAKBEAM_KICK);
                msngr.SendMessage<CommandMessage>(new CommandMessage(cmd2));

                RobotDestinationMessage dest_msg = new RobotDestinationMessage(idealFollowThrough, false);
                msngr.SendMessage<RobotDestinationMessage>(dest_msg);
            }
            else
            {
                // not close enough
                RobotCommand cmd = new RobotCommand(robot.ID, RobotCommand.Command.START_CHARGING);
                msngr.SendMessage<CommandMessage>(new CommandMessage(cmd));

                RobotDestinationMessage dest_msg = new RobotDestinationMessage(ideal,true);
                msngr.SendMessage<RobotDestinationMessage>(dest_msg);
            }
        }
    }
}
