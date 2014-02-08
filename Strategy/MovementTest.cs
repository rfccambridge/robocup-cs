using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RFC.Messaging;
using RFC.Core;
using RFC.Geometry;

namespace RFC.Strategy
{
    public class MovementTest
    {
        const double TOLERANCE = .1;

        int robotId;
        Team team;

        int currentWaypointIndex = 0;
        Vector2[] waypoints = new Vector2[] { new Vector2(1, 1), new Vector2(1, 2) };

        public MovementTest(Team team, int robotId)
        {
            this.robotId = robotId;
            this.team = team;
        }

        public void Loop()
        {
            RobotVisionMessage robotVision = (RobotVisionMessage)ServiceManager.getServiceManager().GetLastMessage(typeof(RobotVisionMessage));
            RobotInfo info = robotVision.GetRobot(team, robotId);

            if (info.Position.distanceSq(waypoints[currentWaypointIndex]) < TOLERANCE)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            }
        }
    }
}
