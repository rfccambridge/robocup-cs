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
		double cornerZone; /*legnth of zone at endgoal where player poses passing risk only, rather than shooting*/
		double otherPassRisk; /*adjustable probability of other team's passing ability*/
				
		public AssessThreats(Team myTeam)
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
            Vector2 leftGoalMid= new Vector2((Constants.Field.GOAL_XMIN+Constants.Field.GOAL_XMAX)/2,(Constants.Field.GOAL_YMIN+Constants.Field.GOAL_YMAX)/2);
			Vector2 rightGoalMid = new Vector2((Constants.Field.GOAL_XMIN+Constants.Field.GOAL_XMAX)/2+Constants.Field.WIDTH,(Constants.Field.GOAL_YMIN+Constants.Field.GOAL_YMAX)/2);
			if(fieldSide){
				midpointGoal = leftGoalMid;
				otherMidpointGoal = rightGoalMid;
			}
			else{
				midpointGoal= rightGoalMid;
				otherMidpointGoal = leftGoalMid;
			}
			goalPostTop = new Vector2(midpointGoal.X,Constants.Field.GOAL_YMIN);
			goalPostBottom = new Vector2(midpointGoal.X,Constants.Field.GOAL_YMAX);
			centerLine = midpointGoal-otherMidpointGoal;
			indexedThreats = new List<Threat>();
			this.myTeam = myTeam;
			cornerZone= (Constants.Field.HEIGHT-Constants.Field.GOAL_WIDTH)/4;
			otherPassRisk=1;
		}
		public void analyzeThreats(FieldVisionMessage msg){
        
			indexedThreats.Add(new Threat(ballRiskRating(msg.Ball), msg.Ball));/*adds ball at index 0 to threat list*/
			for(int j=1; j<=msg.GetRobots(otherTeam).Count; j++)  /*adds each player on the opposite team*/
			{
				indexedThreats.Add(new Threat(playerShotRisk(msg.GetRobot(otherTeam,j-1), msg.Ball)
                       +playerPassRisk(msg.GetRobot(otherTeam,j-1),
                       msg), Threat.ThreatType.robot));
			}
			/*field analysis for open positions*/
			return;
		}
		public List<Threat> getThreats(FieldVisionMessage msg){/*unfinished*/
			analyzeThreats(msg);
			int ballIndex=0;
			bool ballPossessing= false;
			List<Threat> compareThreats = indexedThreats;
			List<Threat> prioritizedThreats = new List<Threat>();
            for (int i = 0; i < compareThreats.Count; i++)
            {
                int maxRiskIndex = 0;
                for (int j = i; j < compareThreats.Count; j++)
                {
                    if (compareThreats[j].severity > maxRiskIndex)
                    {
                        maxRiskIndex = j;
                    }
                    Threat holder = compareThreats[i];
                    compareThreats[i] = compareThreats[j];
                    compareThreats[j] = holder;
                    if (compareThreats[j].GetType().Equals(Threat.ThreatType.ball))
                    {
                        ballIndex = maxRiskIndex;
                    }
                }
                prioritizedThreats = compareThreats;
            }
            for (int k = 0; k <msg.GetRobots(otherTeam).Count; k++)
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
			double angleRisk = Math.Abs(pathToGoal.cosineAngleWith(centerLine));/*doesn't seem like angle risk should fall with cosine*/
			/*can alternatively try 1-(pathToGoal.AngleWith(Centerline)/(3.141592653/2))^4*/
			double riskRating = distanceRisk*angleRisk;
			return riskRating;
			/*can expand to include velocity threats*/
		}
		
		double playerShotRisk(RobotInfo player, BallInfo ball){
			double riskRating=0;
			if(ballPossess(player, ball)){
				riskRating+=ballRiskRating(ball);
			}
			if(Math.Abs(player.Position.X-midpointGoal.X)>Constants.Field.WIDTH/2||player.Position.distance(new Vector2(midpointGoal.X,Constants.Field.EXTENDED_HEIGHT))<cornerZone||player.Position.distance(new Vector2(midpointGoal.X,0))<cornerZone)/*checks if player is in a non-shooting zone*/
			/*could also do shot by preserving minimum angle using a circle*/
			{
				return riskRating;
			}
			else{
				Vector2 pathToGoal = midpointGoal-ball.Position;
				double distanceRisk = pathToGoal.magnitude()/Constants.Field.WIDTH*50;
				double angleRisk = Math.Abs(pathToGoal.cosineAngleWith(centerLine));/*doesn't seem like angle risk should fall with cosine*/
				/*can alternatively try 1-(pathToGoal.AngleWith(Centerline)/(3.141592653/2))^4*/
				riskRating += distanceRisk*angleRisk;
				return riskRating;
				/*can expand to include velocity threats*/
			}
						
		}
		
		double playerPassRisk(RobotInfo player, FieldVisionMessage msg)
		{
		double passRisk=0;
		for(int i=1; i<=msg.GetRobots(otherTeam).Count; i++){
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
		
		bool ballPossess(RobotInfo player, BallInfo ball){
			bool possess = false;
			if(player.Position.distance(ball.Position)<2*Constants.Basic.ROBOT_RADIUS){
				possess = true;
			}
			return possess;
		}
		
	}
}