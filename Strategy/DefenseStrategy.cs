using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RFC.Messaging;
using RFC.Core;
using RFC.Geometry;
using RFC.Strategy;
using RFC.Utilities;
using RFC.PathPlanning;

namespace RFC.Strategy
{
    public class DefenseStrategy
    {
        public enum PlayType
        {
            Defense,
            MidField,
            KickIn,
            KickOff
        }

        Team myTeam;
        AssessThreats assessThreats;
        double otherPassRisk;
        ServiceManager msngr;
        int goalieID;
        Goalie goalieBehavior;
        List<RobotInfo> midFieldPositions; //for MidFieldPlayOnly: list of positions including shadowBall
        public PlayType playType;
        const double default_radius = .35;
        const double ball_avoid_radius = .6;
        const double kickoff_boundary = -.1;

        public DefenseStrategy(Team myTeam, int goalie_id, PlayType playType)
        {
            this.myTeam = myTeam;
            otherPassRisk = 1;
            assessThreats = new AssessThreats(myTeam, 1);
            msngr = ServiceManager.getServiceManager();
            goalieID = goalie_id; 
            goalieBehavior = new Goalie(myTeam, goalie_id);
            this.playType = playType;
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
                results.Add(new RobotInfo(threat.position + difference, 0, myTeam, 0));
            }
            return results;
        }
                
        public void DefenseCommand(FieldVisionMessage msg, int playersOnBall, bool blitz, double avoid_radius = default_radius)
        {
            // assigning position for goalie
            goalieBehavior.getGoalie(msg);

            List<Threat> totalThreats = assessThreats.getThreats(msg); //List of priotized Threats
            List<RobotInfo> topThreats = new List<RobotInfo>();//need to truncate and recast totalThreats as RobotInfo for DestinationMatcher

            // n - 1 threats, because leave one out for goalie
            List<RobotInfo> fieldPlayers = msg.GetRobotsExcept(myTeam,goalieID);

            // adding positions for man to man defense
            List<RobotInfo> destinations = new List<RobotInfo>();
            //assigns positions based on threats (normal defense)
            /*
            for (int i = 0; destinations.Count() < fieldPlayers.Count - playersOnBall; i++)
            {
            // man to man
                if ((totalThreats[i].position - msg.Ball.Position).magnitude() > 0.01)
                {
                    //Console.WriteLine("Subtracting " + Constants.FieldPts.OUR_GOAL + " and " + topThreats[i].Position);
                    Vector2 difference = Constants.FieldPts.OUR_GOAL - totalThreats[i].position;
                    difference = difference.normalizeToLength(3 * Constants.Basic.ROBOT_RADIUS);
                    destinations.Add(new RobotInfo(totalThreats[i].position + difference, 0, myTeam, 0));
                }
             }
             */
            // dealing with ball, either by blitz or by wall

             if (blitz && playersOnBall > 0)
                {
                    destinations.Insert(0,new RobotInfo(msg.Ball.Position, 0, myTeam, 0));
                    // playersOnBall -= 1;
                }

                // rest of the robots on ball make a wall
                Vector2 goalToBall = Constants.FieldPts.OUR_GOAL - msg.Ball.Position;
                double incrementAngle = .6;
                double centerAngle = goalToBall.cartesianAngle();

                for (int i = 0; i < fieldPlayers.Count - playersOnBall; i++)
                {
                    double positionAngle = centerAngle + incrementAngle * (i - (playersOnBall - 1.0) / 2.0);
                    Vector2 unNormalizedDirection = new Vector2(positionAngle);
                    Vector2 normalizedDirection = unNormalizedDirection.normalizeToLength(avoid_radius);
                    Vector2 robotPosition = normalizedDirection + msg.Ball.Position;
                    destinations.Add(new RobotInfo(robotPosition, positionAngle + Math.PI, myTeam, 0)); //adds positions behind ball after ball in List
                }
            //truncate destinations to match fieldPlayers
                destinations.RemoveRange(fieldPlayers.Count, destinations.Count - fieldPlayers.Count);
           

            
            if (playType==PlayType.KickOff)
            {
                //restricts players from going past halfline for kickoffs
                for (int i = 0; i < destinations.Count; i++)
                {
                    if (destinations[i].Position.X > kickoff_boundary)
                    {
                        double tempStore = destinations[i].Position.Y;
                        destinations[i] = new RobotInfo(new Vector2(kickoff_boundary, tempStore),0,myTeam,0);
                    }
                }
            }
            if (playType == PlayType.KickIn || playType == PlayType.KickOff)
            {   //prevents players from getting within 50 cm of ball before being touched

                for (int i = 0; i < destinations.Count; i++)
                {
                    destinations[i] = Avoider.avoid(destinations[i], msg.Ball.Position, ball_avoid_radius);

                }
            }
                   
       
            int[] assignments = DestinationMatcher.GetAssignments(fieldPlayers, destinations);
            int n = fieldPlayers.Count();
            

            // sending dest messages for each one
            for (int i = 0; i <n; i++)
            {
                if ((destinations[assignments[i]].Position - msg.Ball.Position).magnitude() < 0.01)
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

                    msngr.SendMessage(new RobotDestinationMessage(dest, true));
                }
            }

        }
    }
}
