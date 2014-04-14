using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RFC.Messaging;
using RFC.Core;
using RFC.Geometry;

namespace RFC.Strategy
{
	public class AssessThreats : DefenseMapper
	{
		bool fieldSide;
		Vector2 midpointGoal;
		Vector2 goalPostTop;
		Vector2 goalPostBottom;
		Vector2 otherMidpointGoal;
		Vector2 centerLine; /*vector connecting midpoints of goals*/
		List<Threat> indexedThreats;
		Team myTeam;
        Team otherTeam;
		double cornerZone; /*length of zone at endgoal where player poses passing risk only, rather than shooting*/
		double otherPassRisk; /*adjustable probability of other team's passing ability*/
				
		public AssessThreats(Team myTeam, double otherPassRisk) 
		{
			this.fieldSide = true;
			this.myTeam = myTeam;
            if (myTeam == Team.Blue)
            {
                otherTeam = Team.Yellow;
            }
            else
            {
                otherTeam = Team.Blue;
            }
            goalPostTop = Constants.FieldPts.OUR_GOAL_TOP;
            goalPostBottom = Constants.FieldPts.OUR_GOAL_BOTTOM;
			centerLine = Constants.FieldPts.OUR_GOAL-Constants.FieldPts.THEIR_GOAL;
			indexedThreats = new List<Threat>();
			this.myTeam = myTeam;
			cornerZone= (Constants.Field.HEIGHT-Constants.Field.GOAL_WIDTH)/4;
            this.otherPassRisk = otherPassRisk;
		}
		public void analyzeThreats(FieldVisionMessage msg){
        
			indexedThreats.Add(new Threat(ballRiskRating(msg.Ball), msg.Ball));/*adds ball at index 0 to threat list*/
			for(int j=1; j<=msg.GetRobots(otherTeam).Count; j++)  /*adds each player on the opposite team*/
			{
				indexedThreats.Add(new Threat(playerShotRisk(msg.GetRobot(otherTeam,j-1), msg.Ball)
                       +playerPassRisk(msg.GetRobot(otherTeam,j-1),
                       msg), Threat.ThreatType.robot));
			}
			return;
		}
		public List<Threat> getThreats(FieldVisionMessage msg){/*creates list of Threats, then prioritizes them*/
			analyzeThreats(msg);/*create the list of indexed Threats*/
			int ballIndex=0;
            List<Threat> prioritizedThreats = indexedThreats;
            prioritizedThreats.Sort();
            
            for (int k = 0; k <msg.GetRobots(otherTeam).Count; k++) /*removes ball from list of threats if in possession of a player*/
            {
                if (ballPossess(msg.GetRobots(otherTeam)[k], msg.Ball))
                {
                    prioritizedThreats.RemoveAt(ballIndex);
                }
            }
			return prioritizedThreats;
		}
		
		double ballRiskRating(BallInfo ball)
			{
			Vector2 pathToGoal = midpointGoal-ball.Position;
			double distanceRisk = pathToGoal.magnitude()/Constants.Field.WIDTH*100;
			double angleRisk = Math.Abs(pathToGoal.cosineAngleWith(centerLine));
			double riskRating = distanceRisk*angleRisk;
			return riskRating;
			/*can expand to include velocity threats*/
		}
		
		double playerShotRisk(RobotInfo player, BallInfo ball){/*risk of robot taking a shot on goal*/
			double riskRating=0;
			if(ballPossess(player, ball)){
				riskRating+=ballRiskRating(ball);
			}
			if(Math.Abs(player.Position.X-midpointGoal.X)>Constants.Field.WIDTH/2||
                player.Position.distance(new Vector2(midpointGoal.X,Constants.Field.EXTENDED_HEIGHT))<cornerZone||
                player.Position.distance(new Vector2(midpointGoal.X,0))<cornerZone)/*checks if player is in a non-shooting zone*/
			/*could also do shot by preserving minimum angle using a circle*/
			{
				return riskRating;
			}
			else{
				Vector2 pathToGoal = midpointGoal-ball.Position;
				double distanceRisk = pathToGoal.magnitude()/Constants.Field.WIDTH*50;
				double angleRisk = Math.Abs(pathToGoal.cosineAngleWith(centerLine));
				riskRating += distanceRisk*angleRisk;
				return riskRating;
				/*can expand to include velocity threats*/
			}
						
		}
		
		double playerPassRisk(RobotInfo player, FieldVisionMessage msg) /*risk of team players a player can pass too*/
		{
		double passRisk=0;
		for(int i=1; i<=msg.GetRobots(otherTeam).Count; i++){/*prevents adding self-risk to passing risk, excluded index 0 for goalie*/
			if(indexedThreats[i].Equals(player))
			{
				if(i>msg.GetRobots(otherTeam).Count)
				{
					break;
				}
				else
				{
					i++;
				}
			}
		double passRiskFactor=otherPassRisk*(1-player.Position.distanceSq(msg.GetRobot(otherTeam, i).Position)/
                                (Math.Pow(Constants.Field.EXTENDED_HEIGHT,2)+Math.Pow(Constants.Field.EXTENDED_WIDTH,2)));
		passRisk += passRiskFactor*25/msg.GetRobots(otherTeam).Count;
		}
		return passRisk;
		}		
		
		bool ballPossess(RobotInfo player, BallInfo ball){/*determines if robot has the ball*/
			bool possess = false;
			if(player.Position.distance(ball.Position)<2*Constants.Basic.ROBOT_RADIUS){
				possess = true;
			}
			return possess;
		}
		
	}
}