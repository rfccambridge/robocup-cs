using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RFC.Core;
using RFC.Messaging;

namespace RFC.Strategy
{
    class GoalieEric
    {
        Team team;
        int ID;
        public GoalieEric(Team team, int ID)
        {
            this.team = team;
            this.ID = ID;
        }
        public RobotInfo getGoalie(FieldVisionMessage msg)
        {
            BallInfo ball = msg.Ball;

            Geometry.Vector2 projectedBallPosition = (ball.Position - Constants.FieldPts.OUR_GOAL).normalizeToLength(Constants.Field.GOAL_WIDTH/2) + Constants.FieldPts.OUR_GOAL;

            return new RobotInfo(projectedBallPosition, 0, ID);
        }
    }
}
