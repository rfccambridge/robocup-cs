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
        public enum State { Normal, Shot, BounceShot };
        private State state;

        private Team team;
        private Team oTeam;

        private bool stopped = false;
        private bool firstRun = true;
        private OccOffenseMapper offenseMap;

        private Vector2[] zoneList;
        // the radius in meters of a robot's zone
        private const double ZONE_RAD = 0.5;

        // how close the ball must be to a robot to recognize it as having possession
        private const double BALL_HANDLE_MIN = 0.2;

        // the lower the number, the more likely to make a shot
        private const double SHOT_THRESH = 10;
        private const double BSHOT_THRESH = 20;

        // how long should a play continue before it times out (in milliseconds)?
        private const int SHOT_TIMEOUT = 5000;
        private const int BSHOT_TIMEOUT = 5000;

        // when did the current play start executing?
        private int playStartTime;

        // last known ball carrier before a special function has executed
        private RobotInfo shootingRobot = null;
        private RobotInfo bouncingRobot = null;

        private short numOcc = 0;

        private int teamSize = 0;

        public OffTester(Team team)
        {
            this.team = team;
            this.state = 0;
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
            Console.WriteLine(OccOffenseMapper.vecToInd(new Vector2()));
            */
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

        private Vector2[] getBestPos(double[,] map)
        {
            Vector2[] bestPos = new Vector2[teamSize];
            int i = 0;
            foreach (Vector2 zoneCent in zoneList) {
                double max = 0.0;
                int maxJ = 0;
                int maxK = 0;
                int[] indSt = OccOffenseMapper.vecToInd(zoneCent - new Vector2(ZONE_RAD, ZONE_RAD));
                int[] indEnd = OccOffenseMapper.vecToInd(zoneCent + new Vector2(ZONE_RAD, ZONE_RAD));
                for (int j = indSt[0]; j < indEnd[0]; j++)
                {
                    for (int k = indSt[1]; k < indEnd[1]; k++)
                    {
                        if (j >= 0 && j < map.GetLength(0) && k >= 0 && k < map.GetLength(1) && map[j, k] > max)
                        {
                            max = map[j, k];
                            maxJ = j;
                            maxK = k;
                        }
                    }
                }
                Vector2 destVect = OccOffenseMapper.indToVec(maxJ, maxK);
                bestPos[i] = destVect;
                i++;
            }
            return bestPos;
            // debugging
            /*
                Console.WriteLine("RobotID: " + rob.ID + "\nzoneCent: " + zoneCent + "\nGoing to: (" + destVect.X + ", " + destVect.Y + ")\n");
                Console.Write("Map: ");
                OccOffenseMapper.printDoubleMatrix(map);
            */
        }

        private void normalPlay(FieldVisionMessage fieldVision)
        {
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
                teamSize = ourTeam.Count;
                firstRun = false;
            }
            offenseMap.update(ourTeam, theirTeam, ball, fieldVision);
            double[,] dribMap = offenseMap.getDrib(ourTeam, theirTeam, ball);
            double[,] passMap = offenseMap.getPass(ourTeam, theirTeam, ball, fieldVision);

            /*
            ServiceManager.getServiceManager().vdbClear();
            for (int i = 0; i < passMap.GetLength(0); i++)
            {
                for (int j = 0; j < passMap.GetLength(1); j++)
                {
                    Console.WriteLine(dribMap[i, j]);
                    ServiceManager.getServiceManager().vdb(OccOffenseMapper.indToVec(i,j), RFC.Utilities.ColorUtils.numToColor(dribMap[i,j], 0, 0.5));
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
            // used for other play functions
            shootingRobot = ballCarrier;
            Vector2[] bestDrib = getBestPos(dribMap);
            Vector2[] bestPass = getBestPos(passMap);

            for (int i = 0; i < ourTeam.Count; i++)
            {
                int[] inds = OccOffenseMapper.vecToInd(ourTeam.ElementAt(i).Position);
                RobotInfo bpr = goodBounceShot(ourTeam, ballCarrier, passMap);
                // take a shot
                if (ballCarrier != null && ballCarrier.ID == ourTeam.ElementAt(i).ID && inds[0] >= 0 && inds[0] < dribMap.GetLength(0)
                    && inds[1] >= 0 && inds[1] < dribMap.GetLength(1) && dribMap[inds[0], inds[1]] > SHOT_THRESH)
                {
                    state = State.Shot;
                    playStartTime = DateTime.Now.Millisecond;
                }
                // do a bounce pass
                else if (bpr != null)
                {
                    // used for other play functions
                    bouncingRobot = bpr;

                    state = State.BounceShot;
                    playStartTime = DateTime.Now.Millisecond;
                }
                // go for ball
                else if (ballCarrier == null && closestToBall.ID == ourTeam.ElementAt(i).ID)
                {
                    pickUpBall(closestToBall, ball);
                }
                // dribble
                else if (ballCarrier != null && ballCarrier.ID == ourTeam.ElementAt(i).ID)
                {
                    Vector2 destVect = bestDrib[0];
                    double orientation = (Constants.FieldPts.THEIR_GOAL - ballCarrier.Position).cartesianAngle();
                    RobotInfo destination = new RobotInfo(destVect, orientation, ballCarrier.ID);
                    RobotDestinationMessage destinationMessage = new RobotDestinationMessage(destination, false, false);
                    ServiceManager.getServiceManager().SendMessage(destinationMessage);
                }
                // get open
                else if (ourTeam.ElementAt(i) != null)
                {
                    if (numOcc > bestPass.GetLength(0)) continue;
                    Vector2 destVect = bestPass[numOcc];
                    numOcc++;
                    RobotInfo rob = ourTeam.ElementAt(i);
                    double orientation = (ball.Position - rob.Position).cartesianAngle()
                            + 0.5 * Math.Acos((Constants.FieldPts.THEIR_GOAL - rob.Position).cosineAngleWith(ball.Position - rob.Position));
                    RobotInfo destination = new RobotInfo(destVect, orientation, rob.ID);
                    RobotDestinationMessage destinationMessage = new RobotDestinationMessage(destination, true, false);
                    ServiceManager.getServiceManager().SendMessage(destinationMessage);
                }
            }
        }

        public void setState(State s)
        {
            state = s;
        }

        public void shotPlay(FieldVisionMessage fieldVision)
        {
            // TODO: hopefully we don't play through midnight, otherwise I don't think this will work...
            if (shootingRobot.Position.distance(fieldVision.Ball.Position) > BALL_HANDLE_MIN && DateTime.Now.Millisecond - playStartTime >= SHOT_TIMEOUT)
            {
                state = State.Normal;
            }
        }

        public void bounceShotPlay(FieldVisionMessage fieldVision)
        {
            // TODO: hopefully we don't play through midnight, otherwise I don't think this will work...
            if (DateTime.Now.Millisecond - playStartTime >= BSHOT_TIMEOUT)
            {
                state = State.Normal;
            }
        }

        public void reset()
        {
            state = State.Normal;
        }

        public void Handle(FieldVisionMessage fieldVision)
        {
            // TODO: if timeout, make sure isn't affected by stoppage of play
            if (stopped) return;
            switch (state)
            {
                case State.Normal:
                    playStartTime = 0;
                    normalPlay(fieldVision);
                    break;
                case State.Shot:
                    shotPlay(fieldVision);
                    break;
                case State.BounceShot:
                    bounceShotPlay(fieldVision);
                    break;
                default:
                    this.reset();
                    break;
            }
        }

        public void stopMessageHandler(StopMessage message)
        {
            stopped = true;
        }
    }
}