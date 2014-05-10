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
        public enum PlayType
        {
            Defense,
            MidField,
            KickOff
        }

        private Team myTeam;
        public AssessThreats assessThreats;
        private double otherPassRisk;
        ServiceManager msngr;
        int goalieID;
        Goalie goalieBehavior;
        public PlayType playtype;
        public List<RobotInfo> specialPlayPositions; // list of destinations for MidFieldPlay and KickOffPlay: list of positions including shadowBall
        

        public DefenseStrategy(Team myTeam, int goalie_id, PlayType playtype)
        {
            this.myTeam = myTeam;
            otherPassRisk = 1;
            assessThreats = new AssessThreats(myTeam, 1);
            msngr = ServiceManager.getServiceManager();
            goalieID = goalie_id; 
            goalieBehavior = new Goalie(myTeam, goalie_id);
            this.playtype = playtype;
            specialPlayPositions = new List<RobotInfo>();    
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
                
        public void DefenseCommand(FieldVisionMessage msg, int sparePlayers)
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
            Vector2 ballToTopGoalPost = msg.Ball.Position - Constants.FieldPts.OUR_GOAL_TOP;
            Vector2 ballToBottomGoalPost = msg.Ball.Position - Constants.FieldPts.OUR_GOAL_BOTTOM;
            double angleGoal = Math.Cos(ballToTopGoalPost.cosineAngleWith(ballToBottomGoalPost));
            double incrementAngle = angleGoal / (sparePlayers + 1);
            if (playtype == PlayType.MidField)
            {
                for (int i = 0; i < 3; i++) //truncates list by 3 for 3 reserved MidFieldPlay positions
                {
                    destinations.RemoveAt(destinations.Count - 1);
                }

                for (int i = 1; i < sparePlayers + 1; i++) //only removes players if sparePlayers is not 0; destinations will not change if defense (sparePlayers=0) is called
                {
                double positionAngle = ballToTopGoalPost.cartesianAngle() + incrementAngle * i;
                Vector2 unNormalizedDirection = new Vector2(positionAngle);
                Vector2 normalizedDirection = unNormalizedDirection.normalizeToLength(Constants.Basic.ROBOT_RADIUS * 4);
                Vector2 robotPosition = normalizedDirection + msg.Ball.Position;
                destinations.Insert(1, new RobotInfo(robotPosition, 0, 0)); //adds positions behind ball after ball in List
                destinations.RemoveAt(destinations.Count - 1); //removes bottom priority Threats
                }
            }
            
            // restricts players from going past halfline for kickoffs
            else if (playtype == PlayType.KickOff)
            {
                for (int i = 1; i < destinations.Count; i++)
                {
                    if (destinations[i].Position.X > 0)
                    {
                        double tempStore = destinations[i].Position.Y;
                        destinations[i] = new RobotInfo(new Vector2(0, tempStore), 0, 0);
                    }
                }
            }

            specialPlayPositions=destinations;// variable called by midFieldPlay to assign destinations
                      
            //msngr.vdbClear();
            if (playtype != PlayType.MidField)
            {
                int[] assignments = DestinationMatcher.GetAssignments(fieldPlayers, destinations);
                int n = fieldPlayers.Count();
                ServiceManager msngr = ServiceManager.getServiceManager();

                if (fieldPlayers.Count != destinations.Count)
                    throw new Exception("different numbers of robots and destinations: " + fieldPlayers.Count + ", " + destinations.Count);

                // sending dest messages for each one
                for (int i = 0; i < n; i++)
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
}
