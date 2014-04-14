using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RFC.Messaging;
using RFC.Core;
using RFC.Geometry;
using RFC.Strategy;
using RFC.Utilities;

namespace RFC.Strategy
{
    public class DefenseStrategy
    {
        Team myTeam;
        AssessThreats assessThreats;
        double otherPassRisk;
        ServiceManager msngr;
        int goalieID;

        public DefenseStrategy(FieldVisionMessage msg, Team myTeam)
        {
            this.myTeam = myTeam;
            otherPassRisk = 1;
            assessThreats = new AssessThreats(myTeam, 1);
            msngr = ServiceManager.getServiceManager();
            goalieID=; //need to fetch
        }

        public void DefenseCommand(FieldVisionMessage msg)
        {
            List<Threat> totalThreats = assessThreats.getThreats(msg);
            List<RobotInfo> topThreats = new List<RobotInfo>();
            for (int i = 1; i < msg.GetRobots(myTeam).Count(); i++)
            {
                topThreats[i - 1] = new RobotInfo(totalThreats[i - 1].position, 0, 0);
            }
            List<RobotInfo> fieldPlayers = msg.GetRobots(myTeam);
            for(int i =0; i<fieldPlayers.Count; i++)
            {
                if(fieldPlayers[i].ID==goalieID)
                {
                    fieldPlayers.RemoveAt(i);
                }
            }
            List<RobotInfo> destinations = new List<RobotInfo>();
            for (int i = 0; i < fieldPlayers.Count; i++)
            {
                if (topThreats[i].Position == msg.Ball.Position)
                {
                    destinations[i] = new RobotInfo(topThreats[i].Position, 0, 0);
                }
                else
                {
                    Vector2 difference = Constants.FieldPts.OUR_GOAL - topThreats[i].Position;
                    difference.normalizeToLength(3 * Constants.Basic.ROBOT_RADIUS);
                    destinations[i] = new RobotInfo(topThreats[i].Position + difference, 0, 0);
                }

            }
            
            DestinationMatcher.SendByDistance(fieldPlayers, destinations);
        
            




        }



    }
}
