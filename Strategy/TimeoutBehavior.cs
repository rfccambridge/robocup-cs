using RFC.Core;
using RFC.Geometry;
using RFC.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RFC.Strategy
{
    public class TimeoutBehavior
    {
        private enum State { Timeout, Victory };
        private DateTime startTime;
        private Team team;
        private int goalie_id;
        private ServiceManager msngr;
        private object lockObject;

        private static readonly State STATE = State.Timeout;
        // private static readonly State START_STATE = State.Victory;

        public TimeoutBehavior(Team team, int goalie_id)
        {
            this.startTime = DateTime.Now;
            this.team = team;
            this.goalie_id = goalie_id;
            this.msngr = ServiceManager.getServiceManager();
            this.lockObject = new object();
            new QueuedMessageHandler<FieldVisionMessage>(Handle, lockObject);
        }

        private void timeoutHandle(FieldVisionMessage fieldVision)
        {
            // Line up on the side of the field by ID
            List<RobotInfo> robots = fieldVision.GetRobots(this.team);
            foreach (RobotInfo robot in robots)
            {
                double xFromCenter = (robot.ID - 2) * Constants.Basic.ROBOT_RADIUS * 3;
                RobotInfo dest = new RobotInfo(Constants.FieldPts.TOP + new Vector2(xFromCenter, 0), 0.0, this.team, robot.ID);
                RobotDestinationMessage msg = new RobotDestinationMessage(dest, true);
                this.msngr.SendMessage(msg);
            }
        }

        private void victoryHandle(FieldVisionMessage fieldVision)
        {
            // Find a good place to dance (no enemy robots)
            //      Make a list of possible locations
            double danceRadius = Constants.Basic.ROBOT_RADIUS * 10;
            int intervalsWidth = 16, intervalsHeight = 10;
            Vector2 basis0 = Constants.FieldPts.TOP - Constants.FieldPts.CENTER,
                    basis1 = Constants.FieldPts.RIGHT_QUAD - Constants.FieldPts.CENTER;
            //          Resize the bases so the dance doesn't occur off the field
            basis0 = basis0.normalizeToLength(basis0.magnitude() - danceRadius);
            basis1 = basis1.normalizeToLength(basis1.magnitude() - danceRadius);
            //          Resize the bases so there will be the given number of intervals
            basis0 = basis0 * 2.0 / intervalsHeight;
            basis1 = basis1 * 2.0 / intervalsWidth;
            //          Create the span of the basis
            List<Vector2> span = new List<Vector2>();
            for (int i = -intervalsWidth / 2; i <= intervalsWidth / 2; i++)
            {
                for (int j = -intervalsHeight / 2; j <= intervalsHeight / 2; j++)
                {
                    span.Add(Constants.FieldPts.CENTER + j * basis0 + i * basis1);
                }
            }
            //      Check each location for distance to opponents
            foreach (RobotInfo opponent in fieldVision.GetRobots())
            {
                // Only care about opponents
                if (opponent.Team == this.team)
                {
                    continue;
                }
                // Remove all elements from the span if they are not good for dancing
                for (int i = 0; i < span.Count;)
                {
                    Vector2 proposedDanceLocation = span.ElementAt(i);
                    if ((proposedDanceLocation - opponent.Position).magnitude() < danceRadius)
                    {
                        span.RemoveAt(i);
                    }
                    else
                    {
                        i++;
                    }
                }
            }

            // Do a dance! Trace out the curve r=sin(2*theta) !
            if (span.Count > 0)
            {
                Vector2 danceCenter = span.ElementAt(0);
                List<RobotInfo> robots = fieldVision.GetRobots(this.team);
                double timeSinceStart = DateTime.Now.Subtract(this.startTime).TotalMilliseconds / 1000.0;
                foreach (RobotInfo robot in robots)
                {
                    double theta = 2 * Math.PI * ((robot.ID / 5.0 + timeSinceStart / 20.0) % 1.0);
                    double radius = danceRadius * Math.Sin(theta * 2);
                    RobotInfo dest = new RobotInfo(danceCenter + new Vector2(radius * Math.Cos(theta), radius * Math.Sin(theta)), theta, this.team, robot.ID);
                    RobotDestinationMessage msg = new RobotDestinationMessage(dest, false);
                    this.msngr.SendMessage(msg);
                }
            }
        }

        public void Handle(FieldVisionMessage fieldVision)
        {
            switch (STATE)
            {
                case State.Timeout:
                    timeoutHandle(fieldVision);
                    break;

                case State.Victory:
                    victoryHandle(fieldVision);
                    break;
            }
        }
    }
}
