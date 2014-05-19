using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RFC.Messaging;
using RFC.Core;
using RFC.Geometry;

namespace RFC.Strategy
{
    public class PenaltyKickBehavior
    {
        Team team;
        Team oTeam;
        ServiceManager msngr;
        int kicker_id;
        int goalie_id;
        Goalie goalieBehave;
        double i;

        public PenaltyKickBehavior(Team team, int goalie_id)
        {
            this.team = team;
            oTeam = team == Team.Blue ? Team.Yellow : Team.Blue;
            this.msngr = ServiceManager.getServiceManager();
            this.kicker_id = 0;
            this.goalie_id = goalie_id;
            this.goalieBehave = new Goalie(team, goalie_id);
            i = 0;
        }

        public void Ours(FieldVisionMessage msg)
        {
            RobotInfo kicker = msg.GetRobot(team, kicker_id);
            List<RobotInfo> rest = msg.GetRobots(team);

            ShotOpportunity shot = Shot1.evaluate(msg, team, msg.Ball.Position);
            if (shot.target != null)
            {
                //msngr.vdbClear();
                //msngr.vdb(shot.target);
                KickMessage km = new KickMessage(kicker, shot.target);
                msngr.SendMessage(km);
            }
            

            // making sure the rest of ours are behind the line
            foreach (RobotInfo robot in rest)
            {
                if (robot.ID != kicker_id && robot.Position.X > Constants.FieldPts.THEIR_PENALTY_KICK_MARK.X - .8)
                {
                    // need to move the robot back
                    RobotInfo destination = new RobotInfo(robot);
                    destination.Position = new Vector2(Constants.FieldPts.THEIR_PENALTY_KICK_MARK.X - .8, robot.Position.Y);
                    msngr.SendMessage(new RobotDestinationMessage(destination, true, false, true));
                }
            }
            
        }

        public void OursSetup(FieldVisionMessage msg)
        {
            // set kicker id
            kicker_id = 0;

            RobotInfo kicker = msg.GetRobot(team, kicker_id);
            List<RobotInfo> rest = msg.GetRobots(team);

            // todo: send kicker into position
            Vector2 dest = msg.Ball.Position - new Vector2(Constants.Basic.ROBOT_RADIUS * 2, 0);
            msngr.SendMessage(new RobotDestinationMessage(new RobotInfo(dest,0,team,kicker_id), true, false, true));


            // making sure the rest of ours are behind the line
            foreach (RobotInfo robot in rest)
            {
                if (robot.ID != kicker_id && robot.Position.X > Constants.FieldPts.THEIR_PENALTY_KICK_MARK.X - .8)
                {
                    // need to move the robot back
                    RobotInfo destination = new RobotInfo(robot);
                    destination.Position = new Vector2(Constants.FieldPts.THEIR_PENALTY_KICK_MARK.X - .8, robot.Position.Y);
                    msngr.SendMessage(new RobotDestinationMessage(destination, true, false, true));
                }
            }

            
        }

        public void Theirs(FieldVisionMessage msg)
        {
            RobotInfo goalie = msg.GetRobot(team, goalie_id);
            List<RobotInfo> rest = msg.GetRobots(team);
            // getting desired position
            // goalieBehave.getGoalie(msg);
            // has to be on the goal line, so...
            // extend line from their kicker to ball to goalline, placing robot there
            RobotInfo pKicker = msg.GetClosest(oTeam);
            double angle = (pKicker.Position - msg.Ball.Position).cartesianAngle();
            double goalieXPos = Constants.Field.GOAL_XMIN + Constants.Field.GOAL_WIDTH + Constants.Basic.ROBOT_RADIUS;
            double goalY = pKicker.Position.Y + Math.Atan(angle) * (goalieXPos - pKicker.Position.X);
            if (goalY - Constants.Basic.ROBOT_RADIUS < Constants.Field.GOAL_YMIN)
            {
                goalY = Constants.Field.GOAL_YMIN + Constants.Basic.ROBOT_RADIUS;
            }
            else if (goalY + Constants.Basic.ROBOT_RADIUS > Constants.Field.GOAL_YMAX)
            {
                goalY = Constants.Field.GOAL_YMAX - Constants.Basic.ROBOT_RADIUS;
            }
            RobotInfo destRI = new RobotInfo(new Vector2(goalieXPos, goalY), goalie.Orientation, team, goalie.ID);
            msngr.SendMessage(new RobotDestinationMessage(destRI, false, true));

            // making sure the rest of ours are behind the line
            
            foreach (RobotInfo robot in rest)
            {
                if (robot.ID != goalie_id && robot.Position.X < Constants.FieldPts.OUR_PENALTY_KICK_MARK.X + .8)
                {
                    // need to move the robot back
                    RobotInfo destination = new RobotInfo(robot);
                    destination.Position = new Vector2(Constants.FieldPts.OUR_PENALTY_KICK_MARK.X + .8, robot.Position.Y);

                    msngr.SendMessage(new RobotDestinationMessage(destination, true, false, true));
                }
            }
            
        }
    }
}
