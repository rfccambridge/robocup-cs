using System;
using System.Collections.Generic;
using RFC.Core;

namespace RFC.Messaging
{
    public class FieldVisionMessage : Message
    {
        
        public BallInfo Ball
        {
            get;
            private set;
        }

        private Dictionary<Team, List<RobotInfo>> robots = new Dictionary<Team, List<RobotInfo>>();

		public FieldVisionMessage (List<RobotInfo> blueRobots, List<RobotInfo> yellowRobots, BallInfo ball)
		{
			robots.Add(Team.Blue, blueRobots);
			robots.Add(Team.Yellow, yellowRobots);
            this.Ball = ball;
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
                if (robot.ID != id) {
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="team"></param>
        /// <param name="id"></param>
        /// <returns>a robot or null if this robot is not on field</returns>
		public RobotInfo GetRobot(Team team, int id)
		{
			// TODO: this is frequently executed: change to use a dictionary
			List<RobotInfo> robots = GetRobots(team);
			RobotInfo robot = robots.Find((RobotInfo r) => r.ID == id);
			if (robot == null)
			{
                return null;
			}

            return new RobotInfo(robot);
		}

        public RobotInfo GetClosest(Team team)
        {
            if (Ball == null)
            { return null; }
            else
            {
                RobotInfo closestToBall = null;
                double minToBall = 100.0;
                foreach (RobotInfo rob in robots[team])
                {
                    double dist = rob.Position.distance(Ball.Position);

                    if (dist < minToBall)
                    {
                        minToBall = dist;
                        closestToBall = rob;
                    }
                }
                return new RobotInfo(closestToBall);
            }
        }
    }
}
