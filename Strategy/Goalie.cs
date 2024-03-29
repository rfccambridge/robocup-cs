﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RFC.Core;
using RFC.Messaging;
using RFC.Geometry;

namespace RFC.Strategy
{
    public class Goalie
    {
        // goalie info
        Team team;
        public int ID {get; private set;}
        ServiceManager msngr;
        // increased from 1 to 1.2 for balls just outside semicircle
        double clearThreshold = 1.2;  
        int framesTowardGoal = 0;
        int FRAME_THRESH = 10;

        public Goalie(Team team, int ID)
        {
            this.team = team;
            this.ID = ID;
            this.msngr = ServiceManager.getServiceManager();
        }

        // computes ball guard regime (based on ball speed)
        // low number indicates shadowing ball
        // high number indicates predicting ball intersection/closest approach
        // output quantity used in weighted average
        public double guardRegime(BallInfo ball)
        {
            Vector2 ball2goal = Constants.FieldPts.OUR_GOAL - ball.Position;
            if (ball.Velocity.cosineAngleWith(ball2goal) > .9)
            {
                framesTowardGoal++;
            }
            else
            {
                
                framesTowardGoal = 0;
            }
            if (framesTowardGoal > FRAME_THRESH && ball.Velocity.magnitude() > 0.5)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public void getGoalie(FieldVisionMessage msg)
        {
            BallInfo ball = msg.Ball;

            // calculation:
            // robot remains on the semicircle shape that is all points some distance r from the center of the goal
            // r is the minimum of 1/2 the ball distance and half the goal length
            // robot's position is either: the intersection of the ball velocity vector with this shape,
            // or the point on the shape that is closest to the ball when the ball is closest to the shape
            // does not take enemy robots and thus possible bounce sources into account
            // if ball is moving slowly enough, then robot will simply shadow the ball

            Point2 goalpos = Constants.FieldPts.OUR_GOAL;
            Point2 ballpos = ball.Position;
            Vector2 ballvel = ball.Velocity;
            double hold = Constants.Field.GOAL_HEIGHT/2; // robot distance from goal 

            // if ball is close to goal, kick it out
            Vector2 goalToBall = ballpos - goalpos;
            RobotInfo robot = msg.GetRobot(team, ID);
            if (robot == null)
                return;
            Vector2 robotToBall = ballpos - robot.Position;
            if (goalToBall.magnitude() < clearThreshold)
            {

                RobotInfo followThrough = new RobotInfo(ballpos, robotToBall.cartesianAngle(), team, ID);
                /*
                // we are close enough
                RobotCommand cmd = new RobotCommand(ID, RobotCommand.Command.START_CHARGING);
                msngr.SendMessage<CommandMessage>(new CommandMessage(cmd));
                RobotCommand cmd2 = new RobotCommand(ID, RobotCommand.Command.FULL_BREAKBEAM_KICK);
                msngr.SendMessage<CommandMessage>(new CommandMessage(cmd2));

                RobotDestinationMessage dest_msg = new RobotDestinationMessage(followThrough, false, true);
                msngr.SendMessage<RobotDestinationMessage>(dest_msg);
                */
                KickMessage mkg = new KickMessage(followThrough, Constants.FieldPts.THEIR_GOAL);
                msngr.SendMessage(mkg);
                return;
            }


            double shadowAngle = (ballpos - goalpos).cartesianAngle(); // robot angle along semicircle if shadowing ball

            // calculating intersection/closest approach
            Line ballray = new Line(ballpos, ballvel.cartesianAngle());
            Circle guardcircle = new Circle(goalpos, hold);
            double leadAngle = shadowAngle; // robot angle along semicircle if leading ball movements
            Point2[] intersects = LineCircleIntersection.BothIntersections(ballray, guardcircle);
            if (intersects.Length != 0)
            {
                // intersection between ball ray and guard circle that is closest to the ball
                
                double distance = Double.MaxValue;
                int use = 0;
                for (int i = 0; i < intersects.Length; i++)
                {
                    if (intersects[i].distanceSq(ballpos) < distance)
                    {
                        use = i;
                        distance = intersects[i].distanceSq(ballpos);
                    }
                }

                leadAngle = (intersects[use] - goalpos).cartesianAngle();
            }
            else
            {
                leadAngle = (ballray.closestPointTo(goalpos) - goalpos).cartesianAngle();
            }

            double regime = guardRegime(ball);
            double angle = shadowAngle * (1 - regime) + leadAngle * regime;

            Point2 pos = Vector2.GetUnitVector(angle) * hold + goalpos;
            double orientation = (ballpos - pos).cartesianAngle();

            RobotInfo goalie_dest = new RobotInfo(pos, orientation, team, ID);

            msngr.SendMessage<RobotDestinationMessage>(new RobotDestinationMessage(goalie_dest, false));
        }
    }
}
