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

        public WaitBehavior(Team team, int max_robots, DefenseStrategy defense, Goalie goalie)
        {
            this.team = team;
            this.msngr = ServiceManager.getServiceManager();
            this.max_robot_id = max_robots;
            this.defense = defense;
            this.goalieStrategy = goalie;
        }

        // completely stop. set wheel speeds to zero whether we can see it or not
        public void Halt(FieldVisionMessage msg)
        {
            WheelSpeeds speeds = new WheelSpeeds();
            for (int id = 0; id < max_robot_id; id++)
            {
                msngr.SendMessage(new CommandMessage(new RobotCommand(id, speeds)));
            }
        }

        // this could be waiting for a kickin or something
        // need to stay 500mm away from ball
        public void Stop(FieldVisionMessage msg)
        {
            List<RobotInfo> fieldPlayers = msg.GetRobots(team);
            RobotInfo goalie = msg.GetRobot(team, goalieStrategy.ID);

            // goalie is not a field player
            fieldPlayers.Remove(goalie);

            List<RobotInfo> avoidingDestinations = new List<RobotInfo>();
            foreach (RobotInfo rob in defense.GetShadowPositions(msg.GetRobots().Count - 1))
            {
                avoidingDestinations.Add(Avoider.avoid(rob, msg.Ball.Position, .50));
            }
            
            DestinationMatcher.SendByDistance(fieldPlayers, avoidingDestinations);

            // assigning position for goalie
            RobotInfo goalie_dest = goalieStrategy.getGoalie(msg);
            goalie_dest.ID = goalieStrategy.ID;
            msngr.SendMessage(new RobotDestinationMessage(goalie_dest, false, true, true));
        }
    }
}
