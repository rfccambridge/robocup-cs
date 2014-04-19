using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RFC.Messaging;
using RFC.Core;
using RFC.Geometry;
using RFC.Strategy;
using RFC.Utilities;

namespace RFC.Strategy
{
    public class DefenseStrategy
    {
        Team myTeam;
        AssessThreats assessThreats;
        double otherPassRisk;
        ServiceManager msngr;
        int goalieID;
        Goalie goalieBehavior;

        public DefenseStrategy(Team myTeam, int goalie_id)
        {
            this.myTeam = myTeam;
            otherPassRisk = 1;
            assessThreats = new AssessThreats(myTeam, 1);
            msngr = ServiceManager.getServiceManager();
            goalieID = goalie_id; //need to fetch
            goalieBehavior = new Goalie(myTeam, goalie_id);
        }

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
            return GetShadowPositions(totalThreats, n);
        }

        public void DefenseCommand(FieldVisionMessage msg)
        {
            List<Threat> totalThreats = assessThreats.getThreats(msg);
            List<RobotInfo> topThreats = new List<RobotInfo>();

            // n - 1 threats, because leave one out for goalie
            for (int i = 0; i < msg.GetRobots(myTeam).Count() - 1; i++)
            {
                topThreats[i] = new RobotInfo(totalThreats[i].position, 0, 0);
            }

            List<RobotInfo> fieldPlayers = msg.GetRobots(myTeam);
            RobotInfo goalie = msg.GetRobot(myTeam, goalieID);

            // goalie is not a field player
            fieldPlayers.Remove(goalie);


            // assigning positions for field players
            List<RobotInfo> destinations = new List<RobotInfo>();
            for (int i = 0; i < fieldPlayers.Count; i++)
            {
                // want to go right for the ball, not shadow it like a player
                if (topThreats[i].Position == msg.Ball.Position)
                {
                    destinations[i] = new RobotInfo(topThreats[i].Position, 0, 0);
                }
                else
                {
                    Vector2 difference = Constants.FieldPts.OUR_GOAL - topThreats[i].Position;
                    difference.normalizeToLength(3 * Constants.Basic.ROBOT_RADIUS);
                    destinations[i] = new RobotInfo(topThreats[i].Position + difference, 0, 0);
                }

            }
            
            DestinationMatcher.SendByDistance(fieldPlayers, destinations);

            // assigning position for goalie
            RobotInfo goalie_dest = goalieBehavior.getGoalie(msg);
            goalie_dest.ID = goalieID;
            msngr.SendMessage(new RobotDestinationMessage(goalie_dest, false, true, true));

        }
    }
}
