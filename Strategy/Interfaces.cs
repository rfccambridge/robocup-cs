using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RFC.Core;
using RFC.Geometry;
using RFC.Messaging;

namespace RFC.Strategy
{
    interface OMDSwitcher
    {
        double getStatus(List<RobotInfo> ourTeam, List<RobotInfo> theirTeam, BallInfo ball);
    }

    interface OffenseMapper
    {
        void update(List<RobotInfo> ourTeam, List<RobotInfo> theirTeam, BallInfo ball);
        double[,] getDrib(List<RobotInfo> ourTeam, List<RobotInfo> theirTeam, BallInfo ball);
        double[,] getPass(List<RobotInfo> ourTeam, List<RobotInfo> theirTeam, BallInfo ball);
    }

    interface GoaliePlayer
    {
        RobotInfo getPosition(List<RobotInfo> ourTeam, List<RobotInfo> theirTeam, int goalieID, BallInfo ball);
    }

    interface DefenseMapper
    {
        List<Threat> getThreats(FieldVisionMessage msg);
    }

    interface ShotPlanner
    {
        Vector2 getTarget(List<RobotInfo> ourTeam, List<RobotInfo> theirTeam, BallInfo ball);
    }
}