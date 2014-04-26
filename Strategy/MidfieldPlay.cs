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
        ServiceManager msngr;
        Goalie goalieBehave;
        AssessThreats assessThreats;

        public MidfieldPlay(Team t, int g)
        {
            team = t;
            goalie_id = g;
            assessThreats = new AssessThreats (t, 1);
        }

        public void WallMidfield(FieldVisionMessage message)
        {

            this.msngr = ServiceManager.getServiceManager();
            this.goalieBehave = new Goalie(team, goalie_id);

            RobotInfo goalie = goalieBehave.getGoalie(message);
            msngr.SendMessage(new RobotDestinationMessage(goalie, false, true, true));

            int n;
            List<RobotInfo> ours = message.GetRobotsExcept(team, goalie_id);
            if (ours.Count < 3)
            { n = ours.Count; }
            else
            { n = 3; }
            List<RobotInfo> destinations = new List<RobotInfo>();
            Vector2 wallcentre = message.Ball.Position - 1.0 / 5 * (message.Ball.Position - Constants.FieldPts.OUR_GOAL);
            Vector2 towardsgoal = message.Ball.Position - Constants.FieldPts.OUR_GOAL;
            Vector2 perp = towardsgoal.rotatePerpendicular();
            double offset = 4 * Constants.Basic.ROBOT_RADIUS;

            if (n % 2 == 0)
            {
                destinations.Add(new RobotInfo((wallcentre - offset / (2 * perp.magnitude()) * perp), 0, 0));
                if (n > 1)
                {
                    destinations.Add(new RobotInfo((wallcentre + offset / (2 * perp.magnitude()) * perp), 0, 0));

                    for (int i = 2; i < n; i++)
                    {
                        if (i % 2 == 0)
                        {
                            Vector2 position = wallcentre - (i + 2) / 2 * offset / (2*perp.magnitude()) * perp;
                            destinations.Add(new RobotInfo(position, 0, 0));
                        }

                        else
                        {
                            Vector2 position = wallcentre + (i + 1) / 2 * (offset / (2*perp.magnitude()) * perp);
                            destinations.Add(new RobotInfo(position, 0, 0));
                        }

                    }
                }
            }
            else
            {
                destinations.Add(new RobotInfo(wallcentre, 0, 0));
                for (int i = 1; i < n; i++)
                {

                    if (i % 2 == 0)
                    {
                        Vector2 position = wallcentre + (i / 2) * offset / (perp.magnitude()) * perp;
                        destinations.Add(new RobotInfo(position, 0, 0));
                    }

                    else
                    {
                        Vector2 position = wallcentre - (i + 1) / 2 * offset / perp.magnitude() * perp;
                        destinations.Add(new RobotInfo(position, 0, 0));
                    }

                }
            }
        
            /*if (ours.Count > 3)
            {   
                public List<RobotInfo> GetShadowPositions(List<Threat> threats, int n)
                {
                    List<RobotInfo> results = new List<RobotInfo>();
                    foreach (Threat threat in threats)
                {
                    Vector2 difference = Constants.FieldPts.OUR_GOAL - threat.position;
                    difference.normalizeToLength(3 * Constants.Basic.ROBOT_RADIUS);
                    results.Add(new RobotInfo(threat.position + difference, 0, 0));
                }
                return results;
                }

                public List<RobotInfo> GetShadowPositions(int n)
                {
                    List<Threat> totalThreats = assessThreats.getThreats(msngr.GetLastMessage<FieldVisionMessage>());
                    totalThreats.Insert(1,new Threat(totalThreats[0].severity, msngr.GetLastMessage<FieldVisionMessage>().Ball.Position+

                    return GetShadowPositions(totalThreats, n);
                }
                DestinationMatcher.SendByDistance(ours, destinations);
            /// DestinationMatcher.SendByDistance(team, destinations);
        */


        }
    }
}

