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
        List<RobotInfo> midFieldPositions; //for MidFieldPlayOnly: list of positions including shadowBall

        public DefenseStrategy(Team myTeam, int goalie_id)
        {
            this.myTeam = myTeam;
            otherPassRisk = 1;
            assessThreats = new AssessThreats(myTeam, 1);
            msngr = ServiceManager.getServiceManager();
            goalieID = goalie_id; 
            goalieBehavior = new Goalie(myTeam, goalie_id);
            midFieldPositions = new List<RobotInfo>();
        }

        public List<RobotInfo> GetShadowPositions(int n)
        {
            List<Threat> totalThreats = assessThreats.getThreats(msngr.GetLastMessage<FieldVisionMessage>());
            return GetShadowPositions(totalThreats, n);
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
                
        public void DefenseCommand(FieldVisionMessage msg, int playersOnBall, bool blitz)
        {
            // assigning position for goalie
            RobotInfo goalie_dest = goalieBehavior.getGoalie(msg);
            goalie_dest.ID = goalieID;
            msngr.SendMessage<RobotDestinationMessage>(new RobotDestinationMessage(goalie_dest, false, true, true));

            List<Threat> totalThreats = assessThreats.getThreats(msg); //List of priotized Threats
            List<RobotInfo> topThreats = new List<RobotInfo>();//need to truncate and recast totalThreats as RobotInfo for DestinationMatcher

            // n - 1 threats, because leave one out for goalie
            List<RobotInfo> fieldPlayers = msg.GetRobotsExcept(myTeam,goalieID);

            // adding positions for man to man defense
            List<RobotInfo> destinations = new List<RobotInfo>();
            for (int i = 0; destinations.Count() < fieldPlayers.Count - playersOnBall; i++)
            {
                // man to man
                if (totalThreats[i].position != msg.Ball.Position)
                {
                    //Console.WriteLine("Subtracting " + Constants.FieldPts.OUR_GOAL + " and " + topThreats[i].Position);
                    Vector2 difference = Constants.FieldPts.OUR_GOAL - totalThreats[i].position;
                    difference=difference.normalizeToLength(3 * Constants.Basic.ROBOT_RADIUS);
                    destinations.Add(new RobotInfo(totalThreats[i].position + difference, 0, 0));
                }
            }

            // dealing with ball, either by blitz or by wall
            if (blitz && playersOnBall > 0)
            {
                destinations.Add(new RobotInfo(msg.Ball.Position, 0,0));
                playersOnBall -= 1;
            }

            // rest of the robots on ball make a wall
            Vector2 goalToBall = Constants.FieldPts.OUR_GOAL - msg.Ball.Position;
            double incrementAngle = .6;
            double centerAngle = goalToBall.cartesianAngle();

            for (int i = 0; i < playersOnBall; i++)
            {
                double positionAngle = centerAngle + incrementAngle * (i - (playersOnBall - 1.0) / 2.0);
                Console.WriteLine(positionAngle);
                Vector2 unNormalizedDirection = new Vector2(positionAngle);
                Vector2 normalizedDirection = unNormalizedDirection.normalizeToLength(Constants.Basic.ROBOT_RADIUS * 4);
                Vector2 robotPosition = normalizedDirection + msg.Ball.Position;
                destinations.Add(new RobotInfo(robotPosition, 0, 0)); //adds positions behind ball after ball in List
                msngr.vdb(robotPosition);  
            }
            midFieldPositions = destinations;//for MidFieldPlay only
            
                   
            int[] assignments = DestinationMatcher.GetAssignments(fieldPlayers, destinations);
            int n = fieldPlayers.Count();
            

            // sending dest messages for each one
            for (int i = 0; i <n; i++)
            {
                if (destinations[assignments[i]].Position == msg.Ball.Position)
                {
                    // if we can, shoot on goal
                    ShotOpportunity shot = Shot1.evaluateGoal(msg, myTeam, msg.Ball.Position);
                    Vector2 target;
                    if (shot.arc > 0)
                    {
                        target = shot.target;
                    }
                    // if the goal is not open, just shoot for the back wall
                    else
                    {
                        ShotOpportunity shot2 = Shot1.evaluateGeneral(msg, myTeam, msg.Ball.Position, Constants.FieldPts.BOTTOM_RIGHT, Constants.FieldPts.TOP_RIGHT);
                        target = shot2.target;
                    }

                    KickMessage km = new KickMessage(fieldPlayers[i], target);
                    msngr.SendMessage(km);
                }
                else
                {
                    RobotInfo dest = new RobotInfo(destinations[assignments[i]]);
                    dest.ID = fieldPlayers[i].ID;

                    msngr.SendMessage(new RobotDestinationMessage(dest, true, false, true));
                }
            }

        }
    }
}
