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
    class MidfieldPlay
    {
        Team team;
        int goalie_id;

        public MidfieldPlay(Team t, int g)
        {
            team = t;
            goalie_id = g;

        }

        public void WallMidfield(FieldVisionMessage message)
        {
           
            int n = message.GetRobots(team).Count;
            RobotInfo[] destinations = new RobotInfo[n];
            Vector2 wallcentre = message.Ball.Position - 1 / 5 * (message.Ball.Position - Constants.FieldPts.OUR_GOAL);
            Vector2 towardsgoal = message.Ball.Position - Constants.FieldPts.OUR_GOAL;
            Vector2 perp = towardsgoal.rotatePerpendicular();
            double offset = 3 * Constants.Basic.ROBOT_RADIUS;

            if (n % 2 == 0)
            {
                destinations[0] = new RobotInfo(wallcentre - offset / (2 * perp.magnitude()) * perp, 0, 0);
                if (n > 1)
                {
                    destinations[1] = new RobotInfo(wallcentre + offset / (2 * perp.magnitude()) * perp, 0, 0);
                    for (int i = 2; i < n; i++)
                    {
                        if (i % 2 == 0)
                        {
                            Vector2 position = wallcentre - (i + 2) / 2 * offset / (2 * perp.magnitude()) * perp;
                            destinations[i] = new RobotInfo(position, 0, 0);
                        }

                        else
                        {
                            Vector2 position = wallcentre + (i + 1) / 2 * (offset / (2 * perp.magnitude()) * perp);
                            destinations[i] = new RobotInfo(position, 0, 0);
                        }

                    }
                }
            }
            else
            {
                destinations[0] = new RobotInfo(wallcentre, 0, 0);
                for (int i = 1; i < n; i++)
                {

                    if (i % 2 == 0)
                    {
                        Vector2 position = wallcentre + (i / 2) * offset / (perp.magnitude()) * perp;
                        destinations[i] = new RobotInfo(position, 0, 0);
                    }

                    else
                    {
                        Vector2 position = wallcentre - (i + 1) / 2 * offset / perp.magnitude() * perp;
                        destinations[i] = new RobotInfo(position, 0, 0);
                    }

                }
            }

            /// DestinationMatcher.SendByDistance(team, destinations);



        }
    }
}
