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
    public class AvoidGoalTest
    {
        Team team;
        ServiceManager msngr; //handles the messages of objects of the class
        int goalie_id;
        const double epsilon = 0.01;
        bool hitGoal = false;
        
        public AvoidGoalTest(Team team, int goalie)
        {
            this.team = team;
            this.goalie_id = goalie;
            this.msngr = ServiceManager.getServiceManager();
            msngr.RegisterListener<FieldVisionMessage>(Handle, new Object());    
        }

        public void Handle(FieldVisionMessage msg)
        {
            RobotDestinationMessage ds;
            RobotInfo ri;

            /*
             * if we haven't hit the middle of the goal, travel towards it. After we have, move to the top right corner
             * */
            if (!hitGoal)
            {
                ri = new RobotInfo(Constants.FieldPts.THEIR_GOAL, 0.0, team, goalie_id);

                ds = new RobotDestinationMessage(ri, false);
                msngr.SendMessage(ds);    
            }
            if (hitGoal || msg.GetRobot(team, goalie_id).Position.distance(Constants.FieldPts.THEIR_GOAL) < epsilon)
            {
                hitGoal = true;
                ri = new RobotInfo(Constants.FieldPts.TOP_RIGHT, 0.0, team, goalie_id);
                ds = new RobotDestinationMessage(ri, false);
                msngr.SendMessage(ds);
                
            }
             

        }
    }
}
