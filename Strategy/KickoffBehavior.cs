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
        int GoalieID;

        public RobotInfo[] KickoffPositions(int n)
        {
            //generates symmetric starting positions given number of non-goalie robots
            //here, n is the number of robots NOT INCLUDING GOALIE. 
            RobotInfo[] destinations = new RobotInfo[n];
            if (n > 2)
            {



                Vector2 position0 = new Vector2( -Constants.Basic.ROBOT_RADIUS, 0);
                destinations[0] = new RobotInfo(position0, 0, 0);

                Vector2 position1 = new Vector2( - Constants.Basic.ROBOT_RADIUS, -5 * Constants.Field.HEIGHT / 12);
                destinations[1] = new RobotInfo(position1, 0, 0);

                Vector2 position2 = new Vector2( - Constants.Basic.ROBOT_RADIUS, 5 * Constants.Field.HEIGHT / 12);
                destinations[2] = new RobotInfo(position2, 0, 0);

                for (int i = 1; i < n - 2; i++)
                {
                    Vector2 position = new Vector2(Constants.Field.WIDTH / 6, Constants.Field.HEIGHT * i / (n - 2)) + Constants.FieldPts.BOTTOM_LEFT;
                    destinations[i + 2] = new RobotInfo(position, 0, 0);
                }
            }
            else if (n == 2)
            {
                Vector2 position0 = new Vector2(-Constants.Basic.ROBOT_RADIUS, 0);
                destinations[0] = new RobotInfo(position0, 0, 0);

                Vector2 position1 = new Vector2(-Constants.Basic.ROBOT_RADIUS, -5 * Constants.Field.HEIGHT / 6);
                destinations[1] = new RobotInfo(position1, 0, 0);

            }
            else if (n == 1)
            {
                Vector2 position0 = new Vector2(-Constants.Basic.ROBOT_RADIUS, 0);
                destinations[0] = new RobotInfo(position0, 0, 0);

            }
            return destinations;
        }

        public KickOffBehavior(Team team, int GoalieID)

        {
            this.team = team;
            this.msngr = ServiceManager.getServiceManager();
            this.GoalieID = GoalieID;
        }

        public void Ours(FieldVisionMessage msg)
        {
            //TODO
            // center robot does initial kick 
            // when DetectTouch(center robot) is true, transition to normal play
            
            
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
         
            Vector2 positionGoalie = Constants.FieldPts.OUR_GOAL + new Vector2(Constants.Basic.ROBOT_RADIUS,0);
                RobotInfo robot = new RobotInfo(positionGoalie,0,GoalieID);
            RobotDestinationMessage msg2 = new RobotDestinationMessage(robot,true,true);
            msngr.SendMessage(msg2);

            List<RobotInfo> destinations = new List<RobotInfo>(KickoffPositions(n));

            DestinationMatcher.SendByDistance(ours, destinations);

        }

        public void Theirs(FieldVisionMessage msg)
        {
            //TODO
            // detect when play has started, then switch to normal play

        }

        public void TheirsSetup(FieldVisionMessage msg)
        {
            //TODO
            // probably just hardcoded positions
            // put one robot in the center, two on wings, a goalie, and the rest in back. 
            //reuse code from OursSetup
            List<RobotInfo> ours = msg.GetRobotsExcept(team, GoalieID);//msg.GetRobots(team);
            //We deal with Goalie separately
            int n = ours.Count();

            // getting destinations we want to go to

            Vector2 positionGoalie = Constants.FieldPts.OUR_GOAL + new Vector2(Constants.Basic.ROBOT_RADIUS, 0);
            RobotInfo robot = new RobotInfo(positionGoalie, 0, GoalieID);
            RobotDestinationMessage msg2 = new RobotDestinationMessage(robot, true, true);
            msngr.SendMessage(msg2);

            List<RobotInfo> destinations = new List<RobotInfo>(KickoffPositions(n));

            
            for (int i = 0; i < n; i++)
            {
                destinations[i] = RFC.PathPlanning.Avoider.avoid(destinations[i], msg.Ball.Position, .5+ Constants.Basic.ROBOT_RADIUS); 
                //also need to avoid other side of field
            }

            DestinationMatcher.SendByDistance(ours, destinations);
        }
    }
}
