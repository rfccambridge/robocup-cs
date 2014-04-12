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
    class Goalie
    {
        // goalie info
        Team team;
        int ID;

        public Goalie(Team team, int ID)
        {
            this.team = team;
            this.ID = ID;
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

        public RobotInfo getGoalie(FieldVisionMessage msg)
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
            double hold = Constants.Field.GOAL_WIDTH; // robot distance from goal

            double shadowAngle = (ballpos - goalpos).cartesianAngle(); // robot angle along semicircle if shadowing ball

            // calculating intersection/closest approach
            Line ballray = new Line(ballpos, ballvel.cartesianAngle());
            Circle guardcircle = new Circle(goalpos, hold);
            double leadAngle = shadowAngle; // robot angle along semicircle if leading ball movements
            try
            {
                // first intersection between ball ray and guard circle
                leadAngle = (LineCircleIntersection.Intersection(ballray, guardcircle, 0) - goalpos).cartesianAngle();
            }
            catch (NoIntersectionException e)
            {
                leadAngle = (ballray.closestPointTo(goalpos) - goalpos).cartesianAngle();
            }

            double regime = guardRegime(ballvel.magnitude());
            double angle = shadowAngle * (1 - regime) + leadAngle * regime;

            Vector2 pos = new Vector2(angle) * hold + goalpos;
            double orientation = (ballpos - pos).cartesianAngle();

            return new RobotInfo(pos, orientation, ID);
        }
    }
}
