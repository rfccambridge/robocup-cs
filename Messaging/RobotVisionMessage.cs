using System;
using System.Collections.Generic;
using RFC.Core;

namespace RFC.Messaging
{
	public class RobotVisionMessage : Message
	{
        private Dictionary<Team, List<RobotInfo>> robots = new Dictionary<Team, List<RobotInfo>>();

		public RobotVisionMessage (List<RobotInfo> blueRobots, List<RobotInfo> yellowRobots)
		{
			robots.Add(Team.Blue, blueRobots);
			robots.Add(Team.Yellow, yellowRobots);
		}

		public List<RobotInfo> GetRobots(Team team)
		{
            return new List<RobotInfo>(robots[team]);
		}

        public List<RobotInfo> GetRobotsExcept(Team team, int id)
        {
            List<RobotInfo> rlist = new List<RobotInfo>();

            foreach (RobotInfo robot in robots[team])
            {
                if (robot.ID != id)
                {
                    rlist.Add(new RobotInfo(robot));
                }
            }
            return rlist;
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

            return new RobotInfo(robot);
		}
	}
}

