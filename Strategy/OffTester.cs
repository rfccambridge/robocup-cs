using RFC.Core;
using RFC.Geometry;
using RFC.Messaging;
using RFC.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Strategy
{
    public class OffTester
    {
        public const bool DEBUG = false;
        private const double TOLERANCE = 0.1;

        private Team team;
        private Team oTeam;

        private bool stopped = false;
        private bool firstRun = true;
        private OccOffenseMapper offenseMap;

        private Vector2[] zoneList;
        private const double ZONE_RAD = 0.5;

        private const double BALL_HANDLE_MIN = 0.2;

        private const double SHOT_THRESH = 10;
        private const double BSHOT_THRESH = 20;

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
            /*
            Console.WriteLine(OccOffenseMapper.vecToInd(OccOffenseMapper.indToVec(0, 0))[1]);
            Console.WriteLine(OccOffenseMapper.vecToInd(OccOffenseMapper.indToVec(1, 0))[1]);
            Console.WriteLine(OccOffenseMapper.vecToInd(OccOffenseMapper.indToVec(0, 1))[1]);
            Console.WriteLine(OccOffenseMapper.vecToInd(OccOffenseMapper.indToVec(1, 1))[1]);
            Console.WriteLine(OccOffenseMapper.vecToInd(OccOffenseMapper.indToVec(3, 2))[1]);
            Console.WriteLine(OccOffenseMapper.vecToInd(new Vector2()));*/
        }

        private RobotInfo goodBounceShot(List<RobotInfo> ourTeam, RobotInfo ballCarrier, double[,] map)
        {
            if (ballCarrier == null) return null;
            RobotInfo bestRob = null;
            double bestVal = 0.0;
            foreach (RobotInfo rob in ourTeam)
            {
                if (ballCarrier.ID != rob.ID)
                {
                    int[] inds = OccOffenseMapper.vecToInd(rob.Position);
                    if (inds[0] >= 0 && inds[0] < map.GetLength(0) && inds[1] >= 0 && inds[1] < map.GetLength(1) && map[inds[0], inds[1]] > bestVal)
                    {
                        bestRob = rob;
                        bestVal = map[inds[0], inds[1]];
                    }
                }
            }
            return (bestVal < BSHOT_THRESH) ? null : bestRob;
        }

        private void pickUpBall(RobotInfo rob, BallInfo ball)
        {
            RobotInfo destination = new RobotInfo(ball.Position, 0, rob.ID);
            RobotDestinationMessage destinationMessage = new RobotDestinationMessage(destination, false, false);
            ServiceManager.getServiceManager().SendMessage(destinationMessage);
        }

        private void goToBestPos(RobotInfo rob, Vector2 zoneCent, double[,] map, bool hasBall, BallInfo ball)
        {
            if (rob == null) return;
            double max = 0.0;
            int maxI = 0;
            int maxJ = 0;
            int[] indSt = OccOffenseMapper.vecToInd(zoneCent - new Vector2(ZONE_RAD, ZONE_RAD));
            int[] indEnd = OccOffenseMapper.vecToInd(zoneCent + new Vector2(ZONE_RAD, ZONE_RAD));
            for (int j = indSt[0]; j < indEnd[0]; j++)
            {
                for (int k = indSt[1]; k < indEnd[1]; k++)
                {
                    if (j >= 0 && j < map.GetLength(0) && k >= 0 && k < map.GetLength(1) && map[j, k] > max)
                    {
                        max = map[j, k];
                        maxI = j;
                        maxJ = k;
                    }
                }
            }
            Vector2 destVect = OccOffenseMapper.indToVec(maxI, maxJ);
            double orientation = 0.0;
            if (hasBall)
            {
                orientation = (Constants.FieldPts.THEIR_GOAL - rob.Position).cartesianAngle();
            }
            else
            {
                orientation = (ball.Position - rob.Position).cartesianAngle()
                    + 0.5 * Math.Acos((Constants.FieldPts.THEIR_GOAL - rob.Position).cosineAngleWith(ball.Position - rob.Position));
            }
            RobotInfo destination = new RobotInfo(destVect, orientation, rob.ID);
            RobotDestinationMessage destinationMessage = new RobotDestinationMessage(destination, !hasBall, false);
            ServiceManager.getServiceManager().SendMessage(destinationMessage);
            // debugging
            /*
                Console.WriteLine("RobotID: " + rob.ID + "\nzoneCent: " + zoneCent + "\nGoing to: (" + destVect.X + ", " + destVect.Y + ")\n");
                Console.Write("Map: ");
                OccOffenseMapper.printDoubleMatrix(map);
            */
        }

        public void Handle(FieldVisionMessage fieldVision)
        {
            if (stopped) return;
            List<RobotInfo> ourTeam = fieldVision.GetRobots(team);
            List<RobotInfo> theirTeam = fieldVision.GetRobots(oTeam);
            BallInfo ball = fieldVision.Ball;

            if (firstRun)
            {
                zoneList = new Vector2[ourTeam.Count];
                for (int i = 0; i < ourTeam.Count; i++)
                {
                    zoneList[i] = OccOffenseMapper.getZone(i);
                }
                offenseMap = new OccOffenseMapper(team);
                firstRun = false;
            }
            offenseMap.update(ourTeam, theirTeam, ball, fieldVision);
            double[,] dribMap = offenseMap.getDrib(ourTeam, theirTeam, ball);
            double[,] passMap = offenseMap.getPass(ourTeam, theirTeam, ball);
            /*
            ServiceManager.getServiceManager().vdbClear();
            for (int i = 0; i < passMap.GetLength(0); i++)
            {
                for (int j = 0; j < passMap.GetLength(1); j++)
                {
<<<<<<< HEAD
                    Console.WriteLine(dribMap[i, j]);
                    ServiceManager.getServiceManager().vdb(OccOffenseMapper.indToVec(i,j), RFC.Utilities.ColorUtils.numToColor(dribMap[i,j], 0, 0.5));
=======
                    ServiceManager.getServiceManager().vdb(OccOffenseMapper.indToVec(i,j), RFC.Utilities.ColorUtils.numToColor(passMap[i,j], 0, 40));
>>>>>>> 55d6cf80aa0db4818ed7a499cb580ac1d2e828b5
                }
            }
            */
            // TODO: can (and probably should) merge if statements
            RobotInfo ballCarrier = null;
            double rbd = BALL_HANDLE_MIN;
            RobotInfo closestToBall = null;
            double minToBall = 100.0;
            foreach (RobotInfo rob in ourTeam)
            {
                double dist = rob.Position.distance(ball.Position);
                if (dist < rbd)
                {
                    ballCarrier = rob;
                    rbd = dist;
                }
                if (dist < minToBall)
                {
                    minToBall = dist;
                    closestToBall = rob;
                }
            }

            for (int i = 0; i < ourTeam.Count; i++)
            {
                int[] inds = OccOffenseMapper.vecToInd(ourTeam.ElementAt(i).Position);
                RobotInfo bpr = goodBounceShot(ourTeam, ballCarrier, passMap);
                if (ballCarrier != null && ballCarrier.ID == ourTeam.ElementAt(i).ID && inds[0] >= 0 && inds[0] < dribMap.GetLength(0)
                    && inds[1] >= 0 && inds[1] < dribMap.GetLength(1) && dribMap[inds[0], inds[1]] > SHOT_THRESH)
                {
                    KickMessage kickM = new KickMessage(ballCarrier, Constants.FieldPts.THEIR_GOAL);
                    ServiceManager.getServiceManager().SendMessage(kickM);
                }
                else if (bpr != null)
                {
                    // do bounce pass
                }
                else if (ballCarrier == null && closestToBall.ID == ourTeam.ElementAt(i).ID)
                {
                    pickUpBall(closestToBall, ball);
                }
                else if (ballCarrier != null && ballCarrier.ID == ourTeam.ElementAt(i).ID)
                {
                    goToBestPos(ballCarrier, zoneList[i], dribMap, true, ball);
                }
                else if (ourTeam.ElementAt(i) != null)
                {
                    goToBestPos(ourTeam.ElementAt(i), zoneList[i], passMap, false, ball);
                }
            }
        }

        public void stopMessageHandler(StopMessage message)
        {
            stopped = true;
        }
    }
}