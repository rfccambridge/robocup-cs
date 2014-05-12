using System;
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
        double clearThreshold = 1;

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
        public double guardRegime(double ballvel)
        {
            if (ballvel > 0.5)
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

            Vector2 goalpos = Constants.FieldPts.OUR_GOAL;
            Vector2 ballpos = ball.Position;
            Vector2 ballvel = ball.Velocity;
            double hold = Constants.Field.GOAL_HEIGHT/2; // robot distance from goal

            // if ball is close to goal, kick it out
            Vector2 goalToBall = ballpos - goalpos;
            Vector2 robotToBall = ballpos - msg.GetRobot(team, ID).Position;
            if (goalToBall.magnitude() < clearThreshold)
            {
                RobotInfo followThrough = new RobotInfo(ballpos + robotToBall.normalizeToLength(.3), robotToBall.cartesianAngle(), team, ID);
                // we are close enough
                RobotCommand cmd = new RobotCommand(ID, RobotCommand.Command.START_CHARGING);
                msngr.SendMessage<CommandMessage>(new CommandMessage(cmd));
                RobotCommand cmd2 = new RobotCommand(ID, RobotCommand.Command.FULL_BREAKBEAM_KICK);
                msngr.SendMessage<CommandMessage>(new CommandMessage(cmd2));

                RobotDestinationMessage dest_msg = new RobotDestinationMessage(followThrough, false, false, false);
                msngr.SendMessage<RobotDestinationMessage>(dest_msg);
                return;
            }


            double shadowAngle = (ballpos - goalpos).cartesianAngle(); // robot angle along semicircle if shadowing ball

            // calculating intersection/closest approach
            Line ballray = new Line(ballpos, ballvel.cartesianAngle());
            Circle guardcircle = new Circle(goalpos, hold);
            double leadAngle = shadowAngle; // robot angle along semicircle if leading ball movements
            Vector2[] intersects = LineCircleIntersection.BothIntersections(ballray, guardcircle);
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

            double regime = guardRegime(ballvel.magnitude());
            double angle = shadowAngle * (1 - regime) + leadAngle * regime;

            Vector2 pos = new Vector2(angle) * hold + goalpos;
            double orientation = (ballpos - pos).cartesianAngle();

            RobotInfo goalie_dest = new RobotInfo(pos, orientation, team, ID);

            msngr.SendMessage<RobotDestinationMessage>(new RobotDestinationMessage(goalie_dest, false, true, false));
        }
    }
}
