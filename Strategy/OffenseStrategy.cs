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
        public StateChecker sc;
        public enum State { Normal, Shot, BounceShot };
        private State state;

        private Team team;
        private Team oTeam;

        private bool stopped = false;
        private OccOffenseMapper offenseMap;
        private BounceKicker bounceKicker;
        private ServiceManager msngr;
        private Goalie goalie;

        // the radius in meters of a robot's zone
        private const double ZONE_RAD = 0.5;

        // how close the ball must be to a robot to recognize it as having possession
        private const double BALL_HANDLE_MIN = 0.2;

        // the lower the number, the more likely to make a shot
        // private const double SHOT_THRESH = .05;
        // private const double BSHOT_THRESH = 20;
        // equivalent to open shot from half field
        private const double SHOT_THRESH = 0.22;
        private const double BSHOT_THRESH = Double.MaxValue;

        // how long should a play continue before it times out (in milliseconds)?
        private const int SHOT_TIMEOUT = 2000;
        private const int BSHOT_TIMEOUT = 10000;
        private const int NORMAL_TIMEOUT = 10000; // then start decreasing thresholds

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
            bounceKicker = new BounceKicker(team, goalie_id);
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
            bounceKicker = new BounceKicker(team, goalie_id);
            goalie = new Goalie(team, goalie_id);
            msngr = ServiceManager.getServiceManager();

        }

        private QuantifiedPosition goodBounceShot(List<RobotInfo> ourTeam, RobotInfo ballCarrier, Lattice<double> map)
        {
            if (ballCarrier == null) return null;

            return ourTeam
                .Where(rob => ballCarrier.ID != rob.ID)
                .Select(rob =>
                {
                    try
                    {
                        return new QuantifiedPosition(rob, map[rob.Position]);
                    }
                    catch (IndexOutOfRangeException e)
                    {
                        return null;
                    }
                })
                .Where(qp => qp != null)
                .Max();
        }

        private void pickUpBall(RobotInfo rob, BallInfo ball)
        {
            RobotInfo destination = new RobotInfo(ball.Position, 0, rob.Team,rob.ID);
            RobotDestinationMessage destinationMessage = new RobotDestinationMessage(destination, true, false);
            msngr.SendMessage(destinationMessage);
        }


        private QuantifiedPosition getBestPos(Lattice<double> map)
        {
            var bestPair = map.Aggregate((l1, l2) => l1.Value > l2.Value ? l1 : l2);

            return new QuantifiedPosition(
                new RobotInfo(bestPair.Key, 0, team, -1),
                bestPair.Value
            );
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
            double ratio = (NORMAL_TIMEOUT - time) / NORMAL_TIMEOUT;
            return ratio * thresh;
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
            var dribMap = offenseMap.getDrib(ourTeam, theirTeam, ball); // map: how good a position is to make a goal shot "where to dribble the ball"
            var passMap = offenseMap.getPass(ourTeam, theirTeam, ball, fieldVision); // goal shot map accounting for bounce angle, distance, etc. "where to pass the ball"

            // offenseMap.drawMap(passMap);

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
            if (ballCarrier == null && closestToBall != null)
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
                // Console.WriteLine("swtiching to shot");
                playStartTime = DateTime.Now;
            }
            else if (shootingRobot != null && bounce_op.potential > adjust_thresh(BSHOT_THRESH))
            {
                // take a bounce shot

                bouncingRobot = bounce_op.position; 
                state = State.BounceShot;
                bounceKicker.reset(bounce_op.position.Position);
                // Console.WriteLine("switching to bounce shot");
                LineSegment between = new LineSegment(shootingRobot.Position, (bouncingRobot.Position - shootingRobot.Position).cartesianAngle());
                LineSegment toGoal = new LineSegment(bouncingRobot.Position, Shot1.evaluate(fieldVision, team, bouncingRobot.Position).target - bouncingRobot.Position);
                sc = new StateChecker();
                sc.addRule(between, Constants.Basic.ROBOT_RADIUS * 5);
                sc.addRule(toGoal, Constants.Basic.ROBOT_RADIUS * 5);
                playStartTime = DateTime.Now;
            }
            /*
            else if (false ) // put conditions to see if we should get rid of the ball ASAP
            {
                // just get rid of the ball
                // TODO
            }
            */
            /*
            else
            {
                // shoot on goal
                state = State.Shot;
                Console.WriteLine("swtiching to shot");
                playStartTime = DateTime.Now;
            }
            */
            else
            {
                // else just dribble the ball somewhere
                // TODO good dribbling
                RobotInfo destination = bestDrib.position;
                double orientation = (Constants.FieldPts.THEIR_GOAL - ball.Position).cartesianAngle();
                destination.Orientation = orientation;
                destination.ID = ballCarrier.ID;
                RobotDestinationMessage destinationMessage = new RobotDestinationMessage(destination, false, false);
                msngr.SendMessage(destinationMessage);
                 
            }


            // what should other robots do? -----------------------------------------------------------

            List<RobotInfo> rest = fieldVision.GetRobotsExceptTwo(team, closestToBall.ID, goalie_id);

            List<QuantifiedPosition> maxima = offenseMap.getLocalMaxima(passMap);
            maxima.Sort();
            maxima.Reverse();
            List<RobotInfo> passerDestinations = new List<RobotInfo>();
            List<RobotInfo> passerIDs = new List<RobotInfo>();
            while (rest.Count > 0 && maxima.Count > 0)
            {
                // get position of first maximum (which is the greatest remaining maximum)
                RobotInfo currentStation = maxima[0].position;
                maxima.RemoveAt(0); // remove this maximum

                // find closest robot to this maximum
                int closestBot = 0;
                double closestDist = rest[closestBot].Position.distance(currentStation.Position);
                for (int i = 1; i < rest.Count; i++)
                {
                    if (rest[i].Position.distance(currentStation.Position) < closestDist)
                    {
                        closestBot = i;
                    }
                }

                // calculate robot facing angle for pass
                Vector2 goalToStation = Constants.FieldPts.THEIR_GOAL - currentStation.Position;
                Vector2 ballToStation = ball.Position - currentStation.Position;
                currentStation.Orientation = BounceKicker.getBounceOrientation(goalToStation, ballToStation);

                // assign the closest robot to go to this maximum
                passerDestinations.Add(currentStation);
                passerIDs.Add(rest[closestBot]);
                rest.RemoveAt(closestBot); // do not assign this robot to anywhere else
            }

            // now command robot movements
            DestinationMatcher.SendByCorrespondence(passerIDs, passerDestinations);

            /*
            // do nothing
            foreach (RobotInfo rob in rest)
            {
                msngr.SendMessage(new RobotDestinationMessage(rob, true));
            }
            */

            // should be lined up in ideal passing locations
            /*
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
            
            // ROBOT ASSIGNMENT CODE BEGINS HERE
            // *********************************

            // produce list of maxima sorted highest to lowest
            List<QuantifiedPosition> maxima = offenseMap.getLocalMaxima(passMap);
            maxima.Sort();
            maxima.Reverse();
            */
            // Eric Anschuetz assignment code
            /*
            int[] bestInd = offenseMap.vecToInd(maxima[0].position.Position);
            if (passMap[bestInd[0], bestInd[1]] >= adjust_thresh(BSHOT_THRESH))
            {
                List<RobotInfo> passingDestinations = new List<RobotInfo>();
                for (int i = 0; i < Math.Min(passers.Count, maxima.Count); i++)
                {
                    RobotInfo current = maxima[i].position;
                    Vector2 vector1 = Constants.FieldPts.THEIR_GOAL - current.Position;
                    Vector2 vector2 = ball.Position - current.Position;
                    current.Orientation = BounceKicker.getBounceOrientation(vector1, vector2);
                    passingDestinations.Add(current);
                }
                // in case there are no maxima
                DestinationMatcher.SendByCorrespondence(passers.GetRange(0, passingDestinations.Count), passingDestinations);
            }
            */

            // Eric Lu assignment code
            /*
            List<RobotInfo> passerDestinations = new List<RobotInfo>();
            List<RobotInfo> passerIDs = new List<RobotInfo>();
            while (passers.Count > 0 && maxima.Count > 0)
            {
                // get position of first maximum (which is the greatest remaining maximum)
                RobotInfo currentStation = maxima[0].position;
                maxima.RemoveAt(0); // remove this maximum

                // find closest robot to this maximum
                int closestBot = 0;
                double closestDist = passers[closestBot].Position.distance(currentStation.Position);
                for (int i = 1; i < passers.Count; i++)
                {
                    if (passers[i].Position.distance(currentStation.Position) < closestDist)
                    {
                        closestBot = i;
                    }
                }

                // calculate robot facing angle for pass
                Vector2 goalToStation = Constants.FieldPts.THEIR_GOAL - currentStation.Position;
                Vector2 ballToStation = ball.Position - currentStation.Position;
                currentStation.Orientation = BounceKicker.getBounceOrientation(goalToStation, ballToStation);

                // assign the closest robot to go to this maximum
                passerDestinations.Add(currentStation);
                passerIDs.Add(passers[closestBot]);
                passers.RemoveAt(closestBot); // do not assign this robot to anywhere else
            }

            // now command robot movements
            DestinationMatcher.SendByCorrespondence(passerIDs, passerDestinations);
            */
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

            foreach (RobotInfo ri in fieldVision.GetRobotsExcept(team, goalie_id))
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
            if ((int)(DateTime.Now - playStartTime).TotalMilliseconds >= BSHOT_TIMEOUT || !sc.check(fieldVision.Ball.Position))
            {
                if (!sc.check(fieldVision.Ball.Position)) Console.WriteLine("TOO FAR");
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
            // Console.WriteLine("Offense: " + state);
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