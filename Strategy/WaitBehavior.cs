using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RFC.Messaging;
using RFC.Core;
using RFC.PathPlanning;
using RFC.Geometry;

namespace RFC.Strategy
{
    public class WaitBehavior
    {
        Team team;
        ServiceManager msngr;
        int max_robot_id;
        DefenseStrategy defense;
        Goalie goalieStrategy;
        int goalie_id;

        public WaitBehavior(Team team, int goalie_id, int max_robots)
        {
            this.team = team;
            this.msngr = ServiceManager.getServiceManager();
            this.max_robot_id = max_robots;
            this.defense = new DefenseStrategy(team, goalie_id);
            this.goalieStrategy = new Goalie(team, goalie_id);
            this.goalie_id = goalie_id;
        }

        // completely stop. set wheel speeds to zero whether we can see it or not
        public void Halt(FieldVisionMessage msg)
        {
            foreach (RobotInfo rob in msg.GetRobots(team))
            {
                msngr.SendMessage(new RobotDestinationMessage(rob, true, true, true));
            }
        }

        // this could be waiting for a kickin or something
        // need to stay 500mm away from ball
        public void Stop(FieldVisionMessage msg)
        {
            List<RobotInfo> fieldPlayers = msg.GetRobotsExcept(team, goalie_id);

            List<RobotInfo> avoidingDestinations = defense.GetShadowPositions(fieldPlayers.Count);
            
            avoidingDestinations = avoidingDestinations.GetRange(0,fieldPlayers.Count);
            DestinationMatcher.SendByDistance(fieldPlayers, avoidingDestinations);

            // assigning position for goalie
            RobotInfo goalie_dest = goalieStrategy.getGoalie(msg);
            goalie_dest.ID = goalie_id;
            msngr.SendMessage(new RobotDestinationMessage(goalie_dest, false, true, true));
        }
    }
}
