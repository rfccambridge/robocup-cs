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
        bool firstRun = true;

        public MovementTest(Team team, int robotId)
        {
            this.robotId = robotId;
            this.team = team;

            ServiceManager.getServiceManager().RegisterListener<RobotVisionMessage>(Handle, new object());
        }

        public void Handle(RobotVisionMessage robotVision)
        {
            RobotInfo info = robotVision.GetRobot(team, robotId);

            if (info.Position.distanceSq(waypoints[currentWaypointIndex]) < TOLERANCE || firstRun)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;

                RobotInfo destination = new RobotInfo(waypoints[currentWaypointIndex], 0, robotId);
                RobotDestinationMessage destinationMessage = new RobotDestinationMessage(destination, true, false);
                ServiceManager.getServiceManager().SendMessage(destinationMessage);

                firstRun = false;
            }
        }
    }
}
