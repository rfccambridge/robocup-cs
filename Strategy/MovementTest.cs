using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RFC.Messaging;
using RFC.Core;
using RFC.Geometry;
using System.Drawing;

namespace RFC.Strategy
{
    public class MovementTest
    {
        const double TOLERANCE = .1;

        int robotId;
        Team team;

        int currentWaypointIndex = 0;
        Vector2[] waypoints = new Vector2[] { new Vector2(1,2) };
        bool firstRun = true;

        bool stopped = false;

        ServiceManager msngr;

        public MovementTest(Team team)
        {
            this.team = team;

            object lockObject = new object();
            new QueuedMessageHandler<RobotVisionMessage>(Handle, lockObject);
            msngr = ServiceManager.getServiceManager();
            msngr.RegisterListener<StopMessage>(stopMessageHandler, lockObject);
        }

        public void Handle(RobotVisionMessage robotVision)
        {
            if (!stopped && robotVision.GetRobots().Count > 0)
            {
                if (firstRun)
                {
                    robotId = robotVision.GetRobots(team)[0].ID; // take first robot
                }
            RobotInfo info = robotVision.GetRobot(team, robotId);

            if (info.Position.distanceSq(waypoints[currentWaypointIndex]) < TOLERANCE || firstRun)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            }

            RobotInfo destination = new RobotInfo(waypoints[currentWaypointIndex], 0, robotId);
            RobotDestinationMessage destinationMessage = new RobotDestinationMessage(destination, true, false);

            msngr.SendMessage(destinationMessage);
            firstRun = false;
            }
        }

        public void stopMessageHandler(StopMessage message)
        {
            stopped = true;
        }

    }
}
