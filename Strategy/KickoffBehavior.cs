using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RFC.Messaging;
using RFC.Core;
using RFC.Geometry;
using RFC.PathPlanning;

namespace RFC.Strategy
{
    public class KickOffBehavior
    {


        Team team;
        ServiceManager msngr;
        Goalie goalieBehave;
        OffenseStrategy offenseBack;
        int GoalieID;
        DefenseStrategy positionHelper;

        public RobotInfo[] KickoffPositions(int n)
        {
            //generates symmetric starting positions given number of non-goalie robots
            //here, n is the number of robots NOT INCLUDING GOALIE. 
            RobotInfo[] destinations = new RobotInfo[n];
            if (n > 2)
            {



                Point2 position0 = new Point2( -.5 -Constants.Basic.ROBOT_RADIUS, 0);
                destinations[0] = new RobotInfo(position0, 0, team, 0);

                Point2 position1 = new Point2( - 2*Constants.Basic.ROBOT_RADIUS, -3 * Constants.Field.HEIGHT / 12);
                destinations[1] = new RobotInfo(position1, 0, team, 0);

                Point2 position2 = new Point2( - 2*Constants.Basic.ROBOT_RADIUS, 3 * Constants.Field.HEIGHT / 12);
                destinations[2] = new RobotInfo(position2, 0, team, 0);

                for (int i = 1; i < n - 2; i++)
                {
                    Point2 position = new Vector2(Constants.Field.WIDTH / 6, Constants.Field.HEIGHT * i / (n - 2)) + Constants.FieldPts.BOTTOM_LEFT;
                    destinations[i + 2] = new RobotInfo(position, 0, team, 0);
                }
            }
            else if (n == 2)
            {
                Point2 position0 = new Point2(-.5 - Constants.Basic.ROBOT_RADIUS, 0);
                destinations[0] = new RobotInfo(position0, 0, team, 0);

                Point2 position1 = new Point2(-2*Constants.Basic.ROBOT_RADIUS, -3 * Constants.Field.HEIGHT / 12);
                destinations[1] = new RobotInfo(position1, 0, team, 0);

            }
            else if (n == 1)
            {
                Point2 position0 = new Point2(-.5 - Constants.Basic.ROBOT_RADIUS, 0);
                destinations[0] = new RobotInfo(position0, 0, team, 0);

            }
            return destinations;
        }

        public KickOffBehavior(Team team, int GoalieID)

        {
            this.team = team;
            this.msngr = ServiceManager.getServiceManager();
            this.GoalieID = GoalieID;
            this.goalieBehave = new Goalie(team, GoalieID);
            this.offenseBack = new OffenseStrategy(team, GoalieID, Constants.Field.XMIN, -Constants.Basic.ROBOT_RADIUS);
            positionHelper = new DefenseStrategy(team, GoalieID, DefenseStrategy.PlayType.KickOff);
        }

        public void Ours(FieldVisionMessage msg)
        {
            offenseBack.Handle(msg);
            
        }

        public void OursSetup(FieldVisionMessage msg)
        {
            //TODO
            //hardcode positions:
                //Center robot
                //2 wings
                //everyone else in back

            // assume that we start on the left side of the field

            // getting our robots
            List<RobotInfo> ours = msg.GetRobotsExcept(team, GoalieID);//msg.GetRobots(team);

            //We deal with Goalie separately
            int n = ours.Count();

            // getting destinations we want to go to

            Point2 positionGoalie = Constants.FieldPts.OUR_GOAL + new Vector2(Constants.Basic.ROBOT_RADIUS,0);
            RobotInfo robot = new RobotInfo(positionGoalie,0, team, GoalieID);
            RobotDestinationMessage msg2 = new RobotDestinationMessage(robot,true,true);
            msngr.SendMessage(msg2);

            List<RobotInfo> destinations = new List<RobotInfo>(KickoffPositions(n));

            DestinationMatcher.SendByCorrespondence(ours, destinations);

        }

        public void Theirs(FieldVisionMessage msg)
        {
            // probably just hardcoded positions
            // put one robot in the center, two on wings, a goalie, and the rest in back. 
            //reuse code from OursSetup
            List<RobotInfo> ours = msg.GetRobotsExcept(team, GoalieID);//msg.GetRobots(team);
            //We deal with Goalie separately
            int n = ours.Count();

            // sending goalie
            goalieBehave.getGoalie(msg);

            // getting destinations we want to go to
            positionHelper.DefenseCommand(msg, Math.Min(ours.Count,3), false);
        }

        public void TheirsSetup(FieldVisionMessage msg)
        {
            this.Theirs(msg);
            
        }
    }
}
