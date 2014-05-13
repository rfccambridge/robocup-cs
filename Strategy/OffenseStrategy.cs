using RFC.Core;
using RFC.Geometry;
using RFC.Messaging;
using RFC.Strategy;
using RFC.PathPlanning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace RFC.Strategy
{
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

    public class OffenseStrategy
    {
        public enum State { Normal, Shot, BounceShot };
        private State state;

        private Team team;
        private Team oTeam;

        private bool stopped = false;
        private OccOffenseMapper offenseMap;
        private BounceKicker bounceKicker;
        private ServiceManager msngr;
        private Goalie goalie;

        private Vector2[,] zoneList;
        // the radius in meters of a robot's zone
        private const double ZONE_RAD = 0.5;

        // how close the ball must be to a robot to recognize it as having possession
        private const double BALL_HANDLE_MIN = 0.2;

        // the lower the number, the more likely to make a shot
        private const double SHOT_THRESH = .05;
        private const double BSHOT_THRESH = 20;

        // how long should a play continue before it times out (in milliseconds)?
        private const int SHOT_TIMEOUT = 2000;
        private const int BSHOT_TIMEOUT = 10000;
        private const int NORMAL_TIMEOUT = 5000; // then start decreasing thresholds

        // when did the current play start executing?
        private DateTime playStartTime;

        // last known ball carrier before a special function has executed
        private RobotInfo shootingRobot = null;
        private RobotInfo bouncingRobot = null;

        private short numOcc = 0;
        private int teamSize = 0;
        private int goalie_id;
        bool frame;

        // square root of number of zones
        public static int ZONE_NUM = 3;

        // constants used to make sure robots far away from shots going on
        private double SHOOT_AVOID = Constants.Basic.ROBOT_RADIUS + 0.1;
        private double AVOID_RADIUS = 1.0;
        private const double SHOOT_AVOID_DT = 0.5;

        public OffenseStrategy(Team team, int goalie_id)
        {
            this.team = team;
            this.state = State.Normal;
            if (team == Team.Blue)
                this.oTeam = Team.Yellow;
            else
                this.oTeam = Team.Blue;

            this.goalie_id = goalie_id;
            frame = false;
            this.playStartTime = DateTime.Now;

            offenseMap = new OccOffenseMapper(team);
            bounceKicker = new BounceKicker(team);
            goalie = new Goalie(team, goalie_id);
            msngr = ServiceManager.getServiceManager();

        }

        public OffenseStrategy(Team team, int goalie_id, double xmin, double xmax)
        {
            this.team = team;
            this.state = State.Normal;
            if (team == Team.Blue)
                this.oTeam = Team.Yellow;
            else
                this.oTeam = Team.Blue;

            this.goalie_id = goalie_id;
            frame = false;
            this.playStartTime = DateTime.Now;

            offenseMap = new OccOffenseMapper(team, xmin, xmax);
            bounceKicker = new BounceKicker(team);
            goalie = new Goalie(team, goalie_id);
            msngr = ServiceManager.getServiceManager();

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
                    int[] inds = offenseMap.vecToInd(rob.Position);
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
            RobotInfo destination = new RobotInfo(ball.Position, 0, rob.Team,rob.ID);
            RobotDestinationMessage destinationMessage = new RobotDestinationMessage(destination, false, false);
            msngr.SendMessage(destinationMessage);
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

            RobotInfo optimal_position = new RobotInfo(offenseMap.indToVec(best_x, best_y), 0, team, -1);
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
                    if (map[xi,yi] > best)
                    {
                        best = map[xi,yi];
                        best_x = xi;
                        best_y = yi;
                    }
                }
            }

            RobotInfo optimal_bouncer = new RobotInfo(offenseMap.indToVec(best_x, best_y), 0, team,-1);
            msngr.vdb(optimal_bouncer, Color.White);
            //Console.WriteLine("zx: " + zx + " zy: " + zy + " best_X: " + best_x + " best_y: " + best_y + " vec: " + optimal_bouncer.Position);
            return new QuantifiedPosition(optimal_bouncer, best);
        }

        private double adjust_thresh(double thresh)
        {
            int time = (int)(DateTime.Now - playStartTime).TotalMilliseconds;
            if (time >= 2 * NORMAL_TIMEOUT)
            {
                // way over, return 0
                return -1;
            }
            else if (time >= NORMAL_TIMEOUT)
            {
                // just over, start decreasing
                double ratio = time * 1.0 / NORMAL_TIMEOUT;
                ratio = 2 - ratio;
                return thresh * ratio;
            }
            else
                return thresh;
        }
        

        private void normalPlay(FieldVisionMessage fieldVision)
        {
            List<RobotInfo> ourTeam = fieldVision.GetRobotsExcept(team,goalie_id);
            List<RobotInfo> theirTeam = fieldVision.GetRobots(oTeam);
            BallInfo ball = fieldVision.Ball;
            teamSize = ourTeam.Count;

            int n_passers = teamSize - 2; // not the goalie, not the ball carrier 

            // have offenseMap recalculate position fitness for goal shots
            offenseMap.update(ourTeam, theirTeam, ball, fieldVision);
            double[,] dribMap = offenseMap.getDrib(ourTeam, theirTeam, ball); // map: how good a position is to make a goal shot "where to dribble the ball"
            double[,] passMap = offenseMap.getPass(ourTeam, theirTeam, ball, fieldVision); // goal shot map accounting for bounce angle, distance, etc. "where to pass the ball"

            //offenseMap.drawMap(passMap);

            // defining ball carrier
            RobotInfo ballCarrier = null;
            int ballCarrier_id = -1;
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
                    ballCarrier_id = rob.ID;
                }
                if (dist < minToBall)
                {
                    minToBall = dist;
                    closestToBall = rob;
                }
            }

            // used for other play functions
            shootingRobot = ballCarrier;
            QuantifiedPosition bestDrib = getBestPos(dribMap);

            // what should the robot with the ball do ? -------------------------------------------------
            QuantifiedPosition bounce_op = goodBounceShot(ourTeam, shootingRobot, passMap);
            ShotOpportunity shot_op = Shot1.evaluate(fieldVision, team, fieldVision.Ball.Position);
            if (ballCarrier == null)
            {
                // go get the ball
                DribblePlanner.GetPossession(closestToBall, fieldVision);
                ballCarrier_id = closestToBall.ID;
                playStartTime = DateTime.Now;
            }
            else if (shootingRobot != null && shot_op.arc > adjust_thresh(SHOT_THRESH))
            {
                // shoot on goal
                state = State.Shot;
                Console.WriteLine("swtiching to shot");
                playStartTime = DateTime.Now;
            }
            else if (shootingRobot != null && bounce_op.potential > adjust_thresh(BSHOT_THRESH))
            {
                // take a bounce shot
                bouncingRobot = bounce_op.position; 
                state = State.BounceShot;
                bounceKicker.reset(bounce_op.position.Position);
                Console.WriteLine("switching to bounce shot");
                playStartTime = DateTime.Now;
            }
            else if (false ) // put conditions to see if we should get rid of the ball ASAP
            {
                // just get rid of the ball
                // TODO
            }
            else
            {
                // else just dribble the ball somewhere
                //TODO good dribbling
                /*
                RobotInfo destination = bestDrib.position;
                double orientation = (Constants.FieldPts.THEIR_GOAL - ball.Position).cartesianAngle();
                destination.Orientation = orientation;
                destination.ID = ballCarrier.ID;
                RobotDestinationMessage destinationMessage = new RobotDestinationMessage(destination, false, false);
                msngr.SendMessage(destinationMessage);
                 * */
            }


            // what should other robots do? -----------------------------------------------------------
            // should be lined up in ideal passing locations
            
            // build list of robots available for stationing at passing positions
            List<RobotInfo> passers = new List<RobotInfo>();
            foreach (RobotInfo rob in ourTeam)
            {
                if (rob.ID != goalie_id && rob.ID != ballCarrier_id) // robot not the goalie or the ball carrier
                {
                    passers.Add(rob);
                }
            }

            //offenseMap.drawMap(passMap);
            
            // BEGIN EDITING CODE HERE
            // trying non max suppression

            // current solution does not care about robot distance to maximum...
            List<QuantifiedPosition> maxima = offenseMap.getLocalMaxima(passMap);
            maxima.Sort();
            maxima.Reverse();

            List<RobotInfo> passingDestinations = new List<RobotInfo>();
            for (int i = 0; i < Math.Min(passers.Count, maxima.Count); i++)
            {
                RobotInfo current = maxima[i].position;
                Vector2 vector1 = Constants.FieldPts.THEIR_GOAL - current.Position;
                Vector2 vector2 = ball.Position - current.Position;
                current.Orientation = BounceKicker.getBounceOrientation(vector1, vector2); // calculate robot facing angle for pass
                passingDestinations.Add(current);
            }

            // in case there are no maxima
            DestinationMatcher.SendByDistance(passers.GetRange(0, passingDestinations.Count), passingDestinations);
            
        }

        public void setState(State s)
        {
            state = s;
        }

        private void getOutOfWay(RobotInfo ri, RobotInfo kicker, Vector2 toGoal)
        {
            Vector2 fromBallSource = (ri.Position + ri.Velocity * SHOOT_AVOID_DT) - kicker.Position;
            Vector2 perpVec = fromBallSource.perpendicularComponent(toGoal);
            Vector2 paraVec = fromBallSource.parallelComponent(toGoal);
            if (perpVec.magnitude() < SHOOT_AVOID)
            {
                perpVec = perpVec.normalizeToLength(SHOOT_AVOID);
                Vector2 dest = kicker.Position + perpVec + paraVec;
                RobotInfo destRI = new RobotInfo(dest, ri.Orientation, team, ri.ID);
                destRI = Avoider.avoid(destRI, kicker.Position, AVOID_RADIUS);
                msngr.SendMessage(new RobotDestinationMessage(destRI, true, false));
            }
        }

        public void shotPlay(FieldVisionMessage fieldVision)
        {
            // escaping back to normal play
            if (shootingRobot.Position.distance(fieldVision.Ball.Position) > BALL_HANDLE_MIN || (int)(DateTime.Now - playStartTime).TotalMilliseconds >= SHOT_TIMEOUT)
            {
                Console.WriteLine("timed out of shotplay");
                state = State.Normal;
                this.playStartTime = DateTime.Now;
                return;
            }
            RobotInfo shooter = fieldVision.GetClosest(team);
            ShotOpportunity shot = Shot1.evaluateGoal(fieldVision, team, fieldVision.Ball.Position);

            foreach (RobotInfo ri in fieldVision.GetRobots(team))
            {
                if (ri.ID != shooter.ID)
                {
                    getOutOfWay(ri, shooter, shot.target - shooter.Position);
                }
            }

            msngr.SendMessage(new KickMessage(shooter, shot.target));
        }

        public void bounceShotPlay(FieldVisionMessage fieldVision)
        {
            // escape back to normal play
            if ((int)(DateTime.Now - playStartTime).TotalMilliseconds >= BSHOT_TIMEOUT)
            {
                state = State.Normal;
                Console.WriteLine("timed out of bounce shot");
                this.playStartTime = DateTime.Now;
            }

            if (shootingRobot != null && bouncingRobot != null)
            {
                bounceKicker.arrange_kick(fieldVision, shootingRobot.ID, bouncingRobot.ID);
            }
        }

        public void reset()
        {
            state = State.Normal;
            this.playStartTime = DateTime.Now;
        }

        public void Handle(FieldVisionMessage fieldVision)
        {
            if (stopped) return;
            Console.WriteLine("Offense: " + state);
            // handling goalie outside of state
            goalie.getGoalie(fieldVision);
            switch (state)
            {
                case State.Normal:
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