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
            /*foreach (Threat threat in totalThreats)
            {
                Console.WriteLine("Threat has position " + threat.position);
            }*/

            List<RobotInfo> topThreats = new List<RobotInfo>();

            // n - 1 threats, because leave one out for goalie
            List<RobotInfo> fieldPlayers = msg.GetRobots(myTeam);
            for (int i = 0; i <fieldPlayers.Count-1; i++)
            {
                topThreats.Add(new RobotInfo(totalThreats[i].position, 0, 0));
                //Console.WriteLine("Just added " + topThreats[i].Position + " to topThreats");
            }
            //Console.WriteLine("length of topThreats is " + topThreats.Count);

            
            RobotInfo goalie = msg.GetRobot(myTeam, goalieID);

            // Remove goalie from fieldPlayers
            for (int i = 0; i < fieldPlayers.Count; i++)
            {
                if (fieldPlayers[i].ID == goalieID)
                {
                    fieldPlayers.RemoveAt(i);
                    break;
                }
            }


            // assigning positions for field players
            List<RobotInfo> destinations = new List<RobotInfo>();
            for (int i = 0; i < fieldPlayers.Count; i++)
            {
                // want to go right for the ball, not shadow it like a player
                if (topThreats[i].Position == msg.Ball.Position)
                {
                    destinations.Add(new RobotInfo(topThreats[i].Position, 0, 0));
                }
                else
                {
                    //Console.WriteLine("Subtracting " + Constants.FieldPts.OUR_GOAL + " and " + topThreats[i].Position);
                    Vector2 difference = Constants.FieldPts.OUR_GOAL-topThreats[i].Position;
                    difference=difference.normalizeToLength(3 * Constants.Basic.ROBOT_RADIUS);
                    destinations.Add(new RobotInfo(topThreats[i].Position + difference, 0, 0));
                }
                //Console.WriteLine("Index " + i + " of destinations is " + destinations[i].Position);

            }
            msngr.vdbClear();
            foreach (RobotInfo rob in topThreats)
            {
                msngr.vdb(rob);
                //Console.WriteLine("position of a topThreat is " + rob.Position);
            }
            
            DestinationMatcher.SendByDistance(fieldPlayers, destinations);

            // assigning position for goalie
            RobotInfo goalie_dest = goalieBehavior.getGoalie(msg);
            goalie_dest.ID = goalieID;
            msngr.SendMessage<RobotDestinationMessage>(new RobotDestinationMessage(goalie_dest, false, true, true));
            Console.WriteLine(new RobotDestinationMessage(goalie_dest, false, true, true));

        }
    }
}
