using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RFC.Core;
using RFC.Messaging;

namespace RFC.Strategy
{
    class Goalie
    {
        Team team;
        int ID;
        public Goalie(Team team, int ID)
        {
            this.team = team;
            this.ID = ID;
        }
        public RobotInfo getGoalie(FieldVisionMessage msg)
        {
            BallInfo ball = msg.Ball;

            // calculate ball position: simply project onto a circle surrounding the goal
            Geometry.Vector2 projectedBallPosition = (ball.Position - Constants.FieldPts.OUR_GOAL).normalizeToLength(Constants.Field.GOAL_WIDTH/2) + Constants.FieldPts.OUR_GOAL;

            // robot velocity to follow: reduce velocity based on distance to goal; project to circle tangent
            Geometry.Vector2 reducedBallVelocity = ball.Velocity / ((ball.Position - Constants.FieldPts.OUR_GOAL).magnitude()/(Constants.Field.GOAL_WIDTH/2));
            Geometry.Vector2 projectedBallVelocity = reducedBallVelocity.perpendicularComponent(ball.Position - Constants.FieldPts.OUR_GOAL);

            return new RobotInfo(projectedBallPosition, projectedBallVelocity, 0, 0, ID);
        }
    }
}
