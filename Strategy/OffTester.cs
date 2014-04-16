using RFC.Core;
using RFC.Geometry;
using RFC.Messaging;
using RFC.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strategy
{
    public class OffTester
    {
        private const double TOLERANCE = 0.1;

        private Team team;
        private Team oTeam;

        private bool stopped = false;
        private bool firstRun = true;
        private OccOffenseMapper offenseMap;

        private Vector2[] zoneList;
        private const double ZONE_RAD = 5.0;

        private const double BALL_HANDLE_MIN = 5.0;

        public OffTester(Team team)
        {
            this.team = team;
            if (team == Team.Blue)
                this.oTeam = Team.Yellow;
            else
                this.oTeam = Team.Blue;

            object lockObject = new object();
            new QueuedMessageHandler<FieldVisionMessage>(Handle, lockObject);
            ServiceManager.getServiceManager().RegisterListener<StopMessage>(stopMessageHandler, lockObject);
        }

        private void goToBestPos(RobotInfo rob, Vector2 zoneCent, double[,] map, bool hasBall)
        {
            double max = 0.0;
            int maxI = 0;
            int maxJ = 0;
            for (int j = (int)(zoneCent.X - ZONE_RAD); j < (int)(zoneCent.X + ZONE_RAD); j++)
            {
                for (int k = (int)(zoneCent.Y - ZONE_RAD); k < (int)(zoneCent.Y + ZONE_RAD); k++)
                {
                    if (map[j, k] > max)
                    {
                        max = map[j, k];
                        maxI = j;
                        maxJ = k;
                    }
                }
            }
            RobotInfo destination = new RobotInfo(OccOffenseMapper.indToVec(maxI, maxJ), 0, rob.ID);
            RobotDestinationMessage destinationMessage = new RobotDestinationMessage(destination, !hasBall, false);
            ServiceManager.getServiceManager().SendMessage(destinationMessage);
        }

        public void Handle(FieldVisionMessage fieldVision)
        {
            List<RobotInfo> ourTeam = fieldVision.GetRobots(team);
            List<RobotInfo> theirTeam = fieldVision.GetRobots(oTeam);
            BallInfo ball = fieldVision.Ball;

            if (firstRun)
            {
                for (int i = 0; i < ourTeam.Count; i++)
                {
                    zoneList[i] = OccOffenseMapper.getZone(i);
                }
                offenseMap = new OccOffenseMapper(true, ourTeam, theirTeam, ball);
                firstRun = false;
            }
            offenseMap.update(ourTeam, theirTeam, ball);
            double[,] dribMap = offenseMap.getDrib(ourTeam, theirTeam, ball);
            double[,] passMap = offenseMap.getPass(ourTeam, theirTeam, ball);

            RobotInfo ballCarrier = null;
            double rbd = BALL_HANDLE_MIN;
            foreach (RobotInfo rob in ourTeam)
            {
                double dist = rob.Position.distance(ball.Position);
                if (dist < rbd)
                {
                    ballCarrier = rob;
                    rbd = dist;
                }
            }

            for (int i = 0; i < ourTeam.Count; i++)
            {
                if ((i == 0 && ballCarrier == null) || ballCarrier.ID == ourTeam.ElementAt(i).ID)
                {
                    goToBestPos(ballCarrier, zoneList[i], dribMap, true);
                }
                else
                {
                    // first robot shouldn't avoid ball, thus i==0
                    goToBestPos(ourTeam.ElementAt(i), zoneList[i], passMap, i==0);
                }
            }
            /*
            int robotID = fieldVision.GetRobots(team)[0].ID;
            RobotInfo ri = fieldVision.GetRobot(team, robotID);
            if (!stopped)
            {
                firstRun = false;

                offenseMap = new OccOffenseMapper(true, ourTeam, theirTeam, ball);
                offenseMap.update(ourTeam, theirTeam, ball);
                double[,] dribMap = offenseMap.getDrib(ourTeam, theirTeam, ball);
                double[,] passMap = offenseMap.getPass(ourTeam, theirTeam, ball);

                double max = Double.MinValue;
                int maxI = 0;
                int maxJ = 0;

                /*
                for (int i = 0; i < dribMap.GetUpperBound(0); i++)
                {
                    for (int j = 0; j < dribMap.GetUpperBound(1); j++)
                    {
                        if (dribMap[i, j] > max)
                        {
                            max = dribMap[i, j];
                            maxI = i;
                            maxJ = j;
                        }
                    }
                }
                

                for (int i = 0; i < passMap.GetUpperBound(0); i++)
                {
                    for (int j = 0; j < passMap.GetUpperBound(1); j++)
                    {
                        if (passMap[i, j] > max)
                        {
                            max = passMap[i, j];
                            maxI = i;
                            maxJ = j;
                        }
                    }
                }

                if (ri.Position.distanceSq(offenseMap.indToVec(maxI, maxJ)) < TOLERANCE || firstRun)
                {
                    RobotInfo destination = new RobotInfo(offenseMap.indToVec(maxI, maxJ), 0, robotID);
                    RobotDestinationMessage destinationMessage = new RobotDestinationMessage(destination, true, false);
                    ServiceManager.getServiceManager().SendMessage(destinationMessage);
                }

                
                Console.Write("dribMap: ");
                OccOffenseMapper.printDoubleMatrix(dribMap);
                Console.Write("passMap: ");
                OccOffenseMapper.printDoubleMatrix(passMap);
                Console.Write("\n\n\n");
                
            } */
        }

        public void stopMessageHandler(StopMessage message)
        {
            stopped = true;
        }
    }
}