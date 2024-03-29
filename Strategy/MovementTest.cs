﻿using System;
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
    public class MovementTest : IMessageHandler<StopMessage>, IMessageHandler<RobotVisionMessage>
    {
        const double TOLERANCE = .02;

        int robotId;
        Team team;

        int currentWaypointIndex = 0;
        Point2[] waypoints = new Point2[] { new Point2(Constants.Field.FULL_XMIN, 0) };//, new Vector2(2, 0), new Vector2(2, 1), new Vector2(1, 1) };
        bool firstRun = true;

        bool stopped = false;

        ServiceManager msngr;

        public MovementTest(Team team)
        {
            this.team = team;

            object lockObject = new object();
            msngr = ServiceManager.getServiceManager();
            msngr.RegisterListener(this.LockingOn<StopMessage>(lockObject));
            msngr.RegisterListener(this.Queued<RobotVisionMessage>(lockObject));
        }

        public void HandleMessage(RobotVisionMessage robotVision)
        {
            if (!stopped && robotVision.GetRobots().Count > 0)
            {
                if (firstRun)
                {
                    robotId = robotVision.GetRobots(team)[0].ID; // take first robot
                }
            RobotInfo info = robotVision.GetRobot(team, robotId);

            if ((info.Position.distanceSq(waypoints[currentWaypointIndex]) < TOLERANCE && info.Velocity.magnitudeSq() < .01) || firstRun)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            }

            RobotInfo destination = new RobotInfo(waypoints[currentWaypointIndex], 0, team,robotId);
            RobotDestinationMessage destinationMessage = new RobotDestinationMessage(destination, true);

            msngr.SendMessage(destinationMessage);
            firstRun = false;
            }
        }

        public void HandleMessage(StopMessage message)
        {
            stopped = true;
        }

    }
}
