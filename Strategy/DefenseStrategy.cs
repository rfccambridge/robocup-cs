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
                
        public void DefenseCommand(FieldVisionMessage msg, int sparePlayers, bool blitz)
        {
            List<Threat> totalThreats = assessThreats.getThreats(msg); //List of priotized Threats
          
            List<RobotInfo> topThreats = new List<RobotInfo>();//need to truncate and recast totalThreats as RobotInfo for DestinationMatcher

            // n - 1 threats, because leave one out for goalie
            List<RobotInfo> fieldPlayers = msg.GetRobots(myTeam);
            //truncated and recast totalThreats
            for (int i = 0; i <fieldPlayers.Count-1; i++)
            {
                topThreats.Add(new RobotInfo(totalThreats[i].position, 0, 0));
            
            }
        
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
            int ballIndex=0;
            for (int i = 0; i < fieldPlayers.Count; i++)
            {
                // want to go right for the ball, not shadow it like a player
                if (topThreats[i].Position == msg.Ball.Position)
                {
                    ballIndex = i;
                    destinations.Add(new RobotInfo(topThreats[i].Position, 0, 0));
                }
                // for robotThreats
                else
                {
                    //Console.WriteLine("Subtracting " + Constants.FieldPts.OUR_GOAL + " and " + topThreats[i].Position);
                    Vector2 difference = Constants.FieldPts.OUR_GOAL-topThreats[i].Position;
                    difference=difference.normalizeToLength(3 * Constants.Basic.ROBOT_RADIUS);
                    destinations.Add(new RobotInfo(topThreats[i].Position + difference, 0, 0));
                }
            }
            //adds positions behind ball for midFieldPlay
            Vector2 goalToBall = Constants.FieldPts.OUR_GOAL - msg.Ball.Position;
            //double angleGoal = Math.Cos(ballToTopGoalPost.cosineAngleWith(ballToBottomGoalPost));
            double incrementAngle = .6;//angleGoal / (sparePlayers + 1);
            double centerAngle = goalToBall.cartesianAngle();
            ServiceManager msngr = ServiceManager.getServiceManager();

            // sometimes just make wall without charging ball
            /*
            if (!blitz)
            {
                // removing ball
                destinations.RemoveAt(0);
                //adding one at end to be removed
                destinations.Add(new RobotInfo(new Vector2(), 0,0));
            }*/
            
            for (int i = 0; i < sparePlayers; i++)
            {
                double positionAngle = centerAngle + incrementAngle * (i - (sparePlayers-1.0)/2.0);
                Console.WriteLine(positionAngle);
                Vector2 unNormalizedDirection = new Vector2(positionAngle);
                Vector2 normalizedDirection = unNormalizedDirection.normalizeToLength(Constants.Basic.ROBOT_RADIUS * 4);
                Vector2 robotPosition = normalizedDirection + msg.Ball.Position;
                destinations.Insert(1,new RobotInfo(robotPosition, 0, 0)); //adds positions behind ball after ball in List
                destinations.RemoveAt(destinations.Count - 1); //removes bottom priority Threats
                msngr.vdb(robotPosition);
                
            }
            midFieldPositions = destinations;//for MidFieldPlay only
                      
            
                   
            int[] assignments = DestinationMatcher.GetAssignments(fieldPlayers, destinations);
            int n = fieldPlayers.Count();
            

            if (fieldPlayers.Count != destinations.Count)
                throw new Exception("different numbers of robots and destinations: " + fieldPlayers.Count + ", " + destinations.Count);

            // sending dest messages for each one
            for (int i = 0; i <n; i++)
            {
                if (destinations[assignments[i]].Position == msg.Ball.Position)
                {
                    KickMessage km = new KickMessage(fieldPlayers[i], Constants.FieldPts.THEIR_GOAL);
                    msngr.SendMessage(km);
                }
                else
                {
                    RobotInfo dest = new RobotInfo(destinations[assignments[i]]);
                    dest.ID = fieldPlayers[i].ID;

                    msngr.SendMessage(new RobotDestinationMessage(dest, true, false, true));
                }
            }
        

            // assigning position for goalie
            RobotInfo goalie_dest = goalieBehavior.getGoalie(msg);
            goalie_dest.ID = goalieID;
            msngr.SendMessage<RobotDestinationMessage>(new RobotDestinationMessage(goalie_dest, false, true, true));
            //Console.WriteLine(new RobotDestinationMessage(goalie_dest, false, true, true));


        }
    }
}
