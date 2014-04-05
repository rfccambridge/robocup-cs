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
        Team team;
        Team oTeam;

        bool stopped = false;
        OccOffenseMapper offenseMap;

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

        public void Handle(FieldVisionMessage fieldVision)
        {
            if (!stopped)
            {
                List<RobotInfo> ourTeam = fieldVision.GetRobots(team);
                List<RobotInfo> theirTeam = fieldVision.GetRobots(oTeam);
                BallInfo ball = fieldVision.Ball;
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
                */
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

                RobotInfo destination = new RobotInfo(offenseMap.indToVec(maxI, maxJ), 0, fieldVision.GetRobots(team)[0].ID);
                RobotDestinationMessage destinationMessage = new RobotDestinationMessage(destination, true, false);
                ServiceManager.getServiceManager().SendMessage(destinationMessage);

                /*
                Console.Write("dribMap: ");
                OccOffenseMapper.printDoubleMatrix(dribMap);
                Console.Write("passMap: ");
                OccOffenseMapper.printDoubleMatrix(passMap);
                Console.Write("\n\n\n");
                */
            }
        }

        public void stopMessageHandler(StopMessage message)
        {
            stopped = true;
        }
    }
}