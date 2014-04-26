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

namespace RFC.Strategy
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

        private Vector2[,] zoneList;
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
        private int goalie_id;

        // square root of number of zones
        public static int ZONE_NUM = 3;

        public class QuantifiedPosition : IComparable<QuantifiedPosition>
        {
            public RobotInfo position;
            public double potential;

            public QuantifiedPosition(RobotInfo bouncer, double potential)
            {
                this.position = bouncer;
                this.potential = potential;
            }

            // sortable
            public int CompareTo(QuantifiedPosition other)
            {
                return potential.CompareTo(other.potential);
            }
        }

        public OffTester(Team team, int goalie_id)
        {
            this.team = team;
            this.state = State.Normal;
            if (team == Team.Blue)
                this.oTeam = Team.Yellow;
            else
                this.oTeam = Team.Blue;

            this.goalie_id = goalie_id;

            object lockObject = new object();
            new QueuedMessageHandler<FieldVisionMessage>(Handle, lockObject);
            ServiceManager.getServiceManager().RegisterListener<StopMessage>(stopMessageHandler, lockObject);

            // moved here from "if first" ifstatement
            zoneList = new Vector2[ZONE_NUM, ZONE_NUM];
            for (int xi = 0; xi < ZONE_NUM; xi++)
            {
                for (int yi = 0; yi < ZONE_NUM; yi++)
                {
                    zoneList[xi, yi] = OccOffenseMapper.getZone(xi, yi, ZONE_NUM);
                }

            }
            offenseMap = new OccOffenseMapper(team);

            /*
            Console.WriteLine(OccOffenseMapper.vecToInd(OccOffenseMapper.indToVec(0, 0))[1]);
            Console.WriteLine(OccOffenseMapper.vecToInd(OccOffenseMapper.indToVec(1, 0))[1]);
            Console.WriteLine(OccOffenseMapper.vecToInd(OccOffenseMapper.indToVec(0, 1))[1]);
            Console.WriteLine(OccOffenseMapper.vecToInd(OccOffenseMapper.indToVec(1, 1))[1]);
            Console.WriteLine(OccOffenseMapper.vecToInd(OccOffenseMapper.indToVec(3, 2))[1]);
            Console.WriteLine(OccOffenseMapper.vecToInd(new Vector2()));
            */
        }

        private QuantifiedPosition goodBounceShot(List<RobotInfo> ourTeam, RobotInfo ballCarrier, double[,] map)
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
            return new QuantifiedPosition(bestRob, bestVal);
        }

        private void pickUpBall(RobotInfo rob, BallInfo ball)
        {
            RobotInfo destination = new RobotInfo(ball.Position, 0, rob.ID);
            RobotDestinationMessage destinationMessage = new RobotDestinationMessage(destination, false, false);
            ServiceManager.getServiceManager().SendMessage(destinationMessage);
        }


        private QuantifiedPosition getBestPos(double[,] map)
        {
            double best = 0.0;
            int best_x = 0;
            int best_y = 0;

            // looping over zone
            for (int xi = 0; xi < map.GetLength(0); xi++)
            {
                for (int yi = 0; yi < map.GetLength(1); yi++)
                {
                    if (map[xi, yi] < best)
                    {
                        best = map[xi, yi];
                        best_x = xi;
                        best_y = yi;
                    }
                }
            }

            RobotInfo optimal_position = new RobotInfo(OccOffenseMapper.indToVec(best_x, best_y), 0, -1);
            return new QuantifiedPosition(optimal_position, best);
        }

        // get best position within zone xi,yi
        private QuantifiedPosition getBestPosInZone(double[,] map, int zx, int zy)
        {
            // finding bounds to look in
            int min_xi = zx * map.GetLength(0) / ZONE_NUM;
            int max_xi = (zx + 1) * map.GetLength(0) / ZONE_NUM;
            int min_yi = zy * map.GetLength(1) / ZONE_NUM;
            int max_yi = (zy + 1) * map.GetLength(0) / ZONE_NUM;
            double best = 0.0;
            int best_x = 0;
            int best_y = 0;

            // looping over zone
            for (int xi = min_xi; xi < max_xi; xi++)
            {
                for (int yi = min_yi; yi < max_yi; yi++)
                {
                    if (map[xi,yi] < best)
                    {
                        best = map[xi,yi];
                        best_x = xi;
                        best_y = yi;
                    }
                }
            }

            RobotInfo optimal_bouncer = new RobotInfo(OccOffenseMapper.indToVec(best_x,best_y), 0, -1);
            return new QuantifiedPosition(optimal_bouncer, best);
        }

        private void normalPlay(FieldVisionMessage fieldVision)
        {
            List<RobotInfo> ourTeam = fieldVision.GetRobots(team);
            List<RobotInfo> theirTeam = fieldVision.GetRobots(oTeam);
            BallInfo ball = fieldVision.Ball;
            teamSize = ourTeam.Count;

            int n_passers = teamSize - 2; // not the goalie, not the ball carrier 

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

            // defining ball carrier
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
            int[] shooting_inds = OccOffenseMapper.vecToInd(shootingRobot.Position);
            QuantifiedPosition bestDrib = getBestPos(dribMap);

            // what should the robot with the ball do ? -------------------------------------------------
            QuantifiedPosition bounce_op = goodBounceShot(ourTeam, shootingRobot, passMap);
            ShotOpportunity shot_op = Shot1.evaluate(fieldVision, team);

            if (shootingRobot != null && shot_op.arc > SHOT_THRESH)
            {
                // shoot on goal
                state = State.Shot;
                playStartTime = DateTime.Now.Millisecond;
            }
            else if (shootingRobot != null && bounce_op.potential > BSHOT_THRESH)
            {
                // take a bounce shot
                bouncingRobot = bounce_op.position; 
                state = State.BounceShot;
                playStartTime = DateTime.Now.Millisecond;
            }
            else if (false /* put conditions to see if we should get rid of the ball ASAP */)
            {
                // just get rid of the ball
                // TODO
            }
            else
            {
                // else just dribble the ball somewhere
                RobotInfo destination = bestDrib.position;
                double orientation = (Constants.FieldPts.THEIR_GOAL - ballCarrier.Position).cartesianAngle();
                destination.Orientation = orientation;
                destination.ID = ballCarrier.ID;
                RobotDestinationMessage destinationMessage = new RobotDestinationMessage(destination, false, false);
                ServiceManager.getServiceManager().SendMessage(destinationMessage);
            }

            // what should other robots do? -----------------------------------------------------------
            
            List<RobotInfo> passers = new List<RobotInfo>();
            foreach (RobotInfo rob in ourTeam)
            {
                if (rob.ID != goalie_id && rob.ID != ballCarrier.ID)
                {
                    passers.Add(rob);
                }
            }

            // get best positions from each zone
            List<QuantifiedPosition> best_by_zone = new List<QuantifiedPosition>();
            for (int xi = 0; xi < ZONE_NUM; xi++)
            {
                for (int yi = 0; yi < ZONE_NUM; yi++)
                {
                    best_by_zone.Add(getBestPosInZone(passMap, xi, yi));
                }
            }
            
            // sorting
            best_by_zone.Sort();
            best_by_zone.Reverse();

            List<RobotInfo> passingDestinations = new List<RobotInfo>();
            for (int i = 0; i < n_passers; i++)
            {
                RobotInfo current = best_by_zone[i].position;
                current.Orientation = (ball.Position - current.Position).cartesianAngle()
                                + 0.5 * Math.Acos((Constants.FieldPts.THEIR_GOAL - current.Position).cosineAngleWith(ball.Position - current.Position));
                passingDestinations.Add(current);
            }

            // sending
            DestinationMatcher.SendByDistance(passers, passingDestinations);
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