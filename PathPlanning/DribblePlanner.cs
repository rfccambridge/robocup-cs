using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RFC.Core;
using RFC.Messaging;
using RFC.Geometry;

namespace RFC.PathPlanning
{
    public static class DribblePlanner
    {
        public static void GetPossession(RobotInfo closest, FieldVisionMessage msg)
        {
            Vector2 diff = Constants.FieldPts.THEIR_GOAL - msg.Ball.Position;
            double angle = diff.cartesianAngle();
            Vector2 offset = diff.normalizeToLength(2*Constants.Basic.ROBOT_RADIUS);

            Point2 position = msg.Ball.Position - offset;

            RobotInfo ideal = new RobotInfo(position, angle, closest.Team, closest.ID);

            ServiceManager.getServiceManager().SendMessage(new RobotDestinationMessage(ideal, true, false));
        }
    }
}
