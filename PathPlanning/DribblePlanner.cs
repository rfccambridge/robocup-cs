﻿using System;
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
            Vector2 offset = diff.normalizeToLength(1.5*Constants.Basic.ROBOT_RADIUS);

            Vector2 position = msg.Ball.Position - offset;

            RobotInfo ideal = new RobotInfo(position, angle, closest.ID);

            ServiceManager.getServiceManager().SendMessage(new RobotDestinationMessage(ideal, true, false));
        }
    }
}
