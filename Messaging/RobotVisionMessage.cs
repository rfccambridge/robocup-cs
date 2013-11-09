using System;
using System.Collections.Generic;
using RFC.Core;

namespace RFC.Messaging
{
	public class RobotVisionMessage : Message
	{

		
		private Dictionary<Team, List<RobotInfo>> robots = new Dictionary<Team, List<RobotInfo>>();
		
		public RobotVisionMessage (Team teamB, List<RobotInfo> robotsB, Team teamY, List<RobotInfo> robotsY)
		{
			robots.Add(teamB, robotsB);
			robots.Add(teamY, robotsY);
		}

		public List<RobotInfo> GetRobots(Team team)
		{
			return robots[team];
		}

		public List<RobotInfo> GetRobots()
		{
			List<RobotInfo> combined = new List<RobotInfo>();
			foreach (Team team in Enum.GetValues(typeof(Team)))
				combined.AddRange(GetRobots(team));
			return combined;
		}

		public RobotInfo GetRobot(Team team, int id)
		{
			// TODO: this is frequently executed: change to use a dictionary
			List<RobotInfo> robots = GetRobots(team);
			RobotInfo robot = robots.Find((RobotInfo r) => r.ID == id);
			if (robot == null)
			{
				throw new ApplicationException("AveragingPredictor.GetRobot: no robot with id=" +
				                               id.ToString() + " found on team " + team.ToString());
			}
			return robot;
		}
	}
}

