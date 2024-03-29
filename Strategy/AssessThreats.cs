﻿using System;
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
        Point2 goalPostTop;
        Point2 goalPostBottom;
		Vector2 otherMidpointGoal;
		Vector2 centerLine; /*vector connecting midpoints of goals*/
        List<Threat> shooterThreats;
        List<Threat> indexedThreats;
        Team myTeam;
        Team otherTeam;
		double cornerZone; /*length of zone at endgoal where player poses passing risk only, rather than shooting*/
		double otherPassRisk; /*adjustable probability of other team's passing ability*/
        double OWNERSHIP_THRESHOLD = 3 * Constants.Basic.ROBOT_RADIUS;
				
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
            shooterThreats = new List<Threat>();
            indexedThreats = new List<Threat>();
            this.myTeam = myTeam;
			cornerZone= (Constants.Field.HEIGHT-Constants.Field.GOAL_WIDTH)/4;
            this.otherPassRisk = otherPassRisk;
		}

        public List<Threat> getThreats(FieldVisionMessage msg) //returns list of threats in decreasing order of risk severity
        {/*creates list of Threats, then prioritizes them*/
            List<RobotInfo> getallRobots = msg.GetRobots(otherTeam);
            List<RobotInfo> getRobots = new List<RobotInfo>();
            foreach (RobotInfo robot in getallRobots)
            {
                if (robot.Position.X < .3)
                    getRobots.Add(robot);
            }

            double ballRisk = ballRiskRating(msg.Ball, getRobots);
            foreach(RobotInfo robot in getRobots)
            {
                shooterThreats.Add(new Threat(playerShotRisk(robot, msg.Ball, getRobots, ballRisk), robot));
            }
            indexedThreats = new List<Threat>();
            analyzeThreats(msg, getRobots, ballRisk);/*create the list of indexed Threats*/
           
            List<Threat> prioritizedThreats = indexedThreats;
            prioritizedThreats.Sort();
                    
            foreach (RobotInfo robot in getRobots) /*removes ball from list of threats if in possession of a player*/
            {
                if (ballPossess(robot, msg.Ball))
                {
                    prioritizedThreats.RemoveAt(0);
                    break;
                }
            }
            /*foreach (Threat threat in prioritizedThreats)
            {
                Console.WriteLine("Position of threat in prioritizedThreats after analyzeThreats is " + threat.position);
            }
            Console.WriteLine("End cycle");*/
            return prioritizedThreats;
            
        }

        public void analyzeThreats(FieldVisionMessage msg, List<RobotInfo> getRobots, double ballRisk)
        {
            indexedThreats.Add(new Threat(ballRisk, msg.Ball)); /*adds ball at index 0 to threat list*/
                //Console.WriteLine("Added ball to indexedThreats");
                //Console.WriteLine("length of getRobots is " + getRobots.Count);
            foreach(RobotInfo robot in getRobots)  /*adds each player on the opposite team to indexedThreats*/
			{
                //eliminates their goalie from tracking
                if(robot.Position.X<Constants.Field.WIDTH-Constants.Field.GOAL_WIDTH||Math.Abs(robot.Position.Y)<Constants.Field.GOAL_HEIGHT/2)
                {
                
                    Threat dummy = new Threat(playerShotRisk(robot, msg.Ball, getRobots, ballRisk)
                       + playerPassRisk(robot, msg, getRobots, ballRisk),
                       robot);
				indexedThreats.Add(dummy);
                }
                //Console.WriteLine("Added robot to indexedThreats with position " + dummy.position);
			}
            indexedThreats.Add(new Threat(-2, Constants.FieldPts.OUR_GOAL + new Vector2(1, 1), Threat.ThreatType.space));
            indexedThreats.Add(new Threat(-3, Constants.FieldPts.OUR_GOAL + new Vector2(1, -1), Threat.ThreatType.space));
            indexedThreats.Add(new Threat(-1, Constants.FieldPts.OUR_GOAL + new Vector2(1.5, 0), Threat.ThreatType.space));
			return;
		}
		
		
		double ballRiskRating(BallInfo ball, List<RobotInfo> getRobots)
			{
			Vector2 pathToGoal = Constants.FieldPts.OUR_GOAL-ball.Position;
			double distanceRisk = pathToGoal.magnitude()/Constants.Field.WIDTH*100;
			double angleRisk = Math.Abs(pathToGoal.cosineAngleWith(centerLine));
			double riskRating = distanceRisk*angleRisk;
			return riskRating;
			/*can expand to include velocity threats*/
		}
		
		double playerShotRisk(RobotInfo player, BallInfo ball,List<RobotInfo> getRobots, double ballRisk){/*risk of robot taking a shot on goal*/
			double riskRating=0;
			if(ballPossess(player, ball)){
				riskRating+=ballRisk;
			}
			if(Math.Abs(player.Position.X-Constants.FieldPts.OUR_GOAL.X)>Constants.Field.WIDTH/2||
                player.Position.distance(new Point2(Constants.FieldPts.OUR_GOAL.X,Constants.Field.EXTENDED_HEIGHT))<cornerZone||
                player.Position.distance(new Point2(Constants.FieldPts.OUR_GOAL.X,0))<cornerZone)/*checks if player is in a non-shooting zone*/
			/*could also do shot by preserving minimum angle using a circle*/
			{
				return riskRating;
			}
			else{
				Vector2 pathToGoal = Constants.FieldPts.OUR_GOAL-ball.Position;
				double distanceRisk = pathToGoal.magnitude()/Constants.Field.WIDTH*50;

				double angleRisk = Math.Abs(pathToGoal.cosineAngleWith(centerLine));
				riskRating += distanceRisk*angleRisk;
				return riskRating;
				/*can expand to include velocity threats*/
			}
						
		}
		
		double playerPassRisk(RobotInfo player, FieldVisionMessage msg, List<RobotInfo> getRobots,double ballRisk) /*risk of team players a player can pass too*/
		{
            double passRisk=0;
            Threat playerThreat= new Threat(playerShotRisk(player, msg.Ball,getRobots,ballRisk),msg.Ball);
            for (int i = 0; i < getRobots.Count; i++)/*prevents adding self-risk to passing risk, excluded index 0 for goalie*/
            {
			    if(shooterThreats[i].Equals(playerThreat))
			    {
				    if(i==getRobots.Count)
				    {
					    break;
				    }
				    else
				    {
					    i++;
				    }
			    }
                /*calibrates passRiskFactor based on distance between robots*/
		        double passRiskFactor=otherPassRisk*(1-player.Position.distanceSq(getRobots[i].Position)/
                                (Math.Pow(Constants.Field.EXTENDED_HEIGHT,2)+Math.Pow(Constants.Field.EXTENDED_WIDTH,2)));
		        passRisk += passRiskFactor*shooterThreats[i].severity/(2*getRobots.Count);
		    }

		    return passRisk;
		}		
		
		bool ballPossess(RobotInfo player, BallInfo ball){/*determines if robot has the ball*/
			bool possess = false;
			if(player.Position.distance(ball.Position) < OWNERSHIP_THRESHOLD){
				possess = true;
			}
			return possess;
		}
		
	}
}