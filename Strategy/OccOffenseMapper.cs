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
using System.Collections;

namespace RFC.Strategy
{
    public class OccOffenseMapper
    {
        // normalize map to this number
        public const double NORM_TO = 100.0;
        // higher num -> better resolution
        public const int LAT_NUM = 60;
        // in degrees
        public const double BOUNCE_ANGLE = 20.0;
        // how far away from the line of sight should we ignore other robots?
        public const double IGN_THRESH = .15;
        // how important is good line of sight to the goal for our other robots?
        public const double SHOT_EXP = 2.0;
        Team team;
        
        private LatticeSpec latticeSpec;
        private Lattice<double> shotLattice;
  
        private ServiceManager msngr;
        
        public OccOffenseMapper(Team team)
        {
            this.team = team;
            msngr = ServiceManager.getServiceManager();

            this.latticeSpec = new LatticeSpec(
                new Geometry.Rectangle(
                    Constants.Field.XMIN, Constants.Field.XMAX,
                    Constants.Field.YMIN, Constants.Field.YMAX
                ),
                LAT_NUM
            );
        }

        public OccOffenseMapper(Team team, double xmin, double xmax)
        {
            this.team = team;
            msngr = ServiceManager.getServiceManager();

            this.latticeSpec = new LatticeSpec(
                new Geometry.Rectangle(
                    xmin, xmax,
                    Constants.Field.YMIN, Constants.Field.YMAX
                ),
                LAT_NUM
            );
        }
        
        private static double normalize(double score)
        {
            // values might be too small as Double.MaxValue is super big...
            // return (NORM_TO / Double.MaxValue) * score;
            return score;
        }

        // calculate how good a position is for making a goal shot
        public void update(List<RobotInfo> ourTeam, List<RobotInfo> theirTeam, BallInfo ball, FieldVisionMessage fmsg)
        {
            shotLattice = latticeSpec.Create(pos => {
                // not needed since we now have ShotOpportunity
                /*
                // find angle of opening for position
                Vector2 vecBotGoal = Constants.FieldPts.THEIR_GOAL_BOTTOM - pos;
                Vector2 vecTopGoal = Constants.FieldPts.THEIR_GOAL_TOP - pos;
                double goalAngle = Math.Acos(vecBotGoal.cosineAngleWith(vecTopGoal));

                // iterate through other robots, adding distance between (line of sight between position and goal) and other robot
                Vector2 vecCentGoal = pos - Constants.FieldPts.THEIR_GOAL;
                double distSum = 1;
                foreach (RobotInfo rob in theirTeam)
                {

                    double dist = (rob.Position - pos).perpendicularComponent(vecCentGoal).magnitude();
                    if (dist < IGN_THRESH && ((vecCentGoal - rob.Position).magnitude() > (vecCentGoal - pos).magnitude()))
                    {
                        distSum -= 1 * Math.Exp(-dist);
                    }
                }

                if (distSum < 0)
                    distSum = 0;
                */

                // make nonlinear (put in threshold)
                // subtract (so that number of robots doesn't factor in)
                //]return normalize(goalAngle * distSum);
                ShotOpportunity shot = Shot1.evaluate(fmsg, team, pos);
                return shot.arc;
            });
        }

        // dribble to better place--doesn't depend on bounce angle
        // should only depend on how good shot is?
        public Lattice<double> getDrib(List<RobotInfo> ourTeam, List<RobotInfo> theirTeam, BallInfo ball)
        {
            return shotLattice;
            // distance from ball to pos?
        }

        // within bounce angle -> good
        // make sure pass between ball and position is good
        // make sure position has good shot
        public Lattice<double> getPass(List<RobotInfo> ourTeam, List<RobotInfo> theirTeam, BallInfo ball, FieldVisionMessage fmsg)
        {
            return latticeSpec.Create(pos =>
            {
                Vector2 currToBall = ball.Position - pos;
                Vector2 vecToGoal = Constants.FieldPts.THEIR_GOAL - pos;


                // see if position has good line of sight with ball
                double distSum = 1.0;
                double tooClose = 1.0;

                foreach (RobotInfo rob in theirTeam)
                {

                    Vector2 dist = (rob.Position - pos);
                    double perpdist = dist.perpendicularComponent(currToBall).magnitude();
                    Vector2 robotToBall = ball.Position - rob.Position;
                    
                    if (perpdist < IGN_THRESH && currToBall.cosineAngleWith(currToBall) > 0 && currToBall * robotToBall > robotToBall * robotToBall)
                    {
                        distSum -= 1 * Math.Exp(-perpdist);
                    }

                    // also checking if too close
                    if (dist.magnitude() < Constants.Basic.ROBOT_RADIUS * 4)
                        tooClose = 0.0;

                }
                if (distSum < 0)
                {
                    distSum = 0;
                }

                // account for distance to ball
                double distScore = Math.Atan2(1, currToBall.magnitude());
                if (Constants.Basic.ROBOT_RADIUS * 4 > currToBall.magnitude())
                    distScore = 0;

                // account for distance to our robots
                double minRobotDist = 100.0;
                foreach (RobotInfo rob in ourTeam)
                {
                    double robotDist = (rob.Position - pos).magnitude();
                    if (robotDist < minRobotDist)
                        minRobotDist = robotDist;
                }
                double robDistScore = Math.Exp(-minRobotDist);

                // calculate bounce score
                // make .5(1+cos)
                double currentBounceAngle = 180 * Math.Acos(currToBall.cosineAngleWith(vecToGoal)) / Math.PI;
                if (double.IsNaN(currentBounceAngle))
                    currentBounceAngle = 0;
                double bounceScore = 180 - currentBounceAngle;
                double worstBounceScore = 90 - Math.Abs(BOUNCE_ANGLE - 90);
                // if bounce score is worse than what robot can handle then position is pretty crappy
                if (bounceScore < worstBounceScore)
                {
                    bounceScore = 0;
                }

                double isValid = 0;
                if (Avoider.isValid(pos, false))
                    isValid = 1;

                return normalize(Math.Pow(shotLattice[pos], SHOT_EXP) * bounceScore * distSum * distScore * tooClose * isValid * robDistScore);



                //Console.WriteLine("result: " + normalize(shotMap[i, j] * bounceScore * distSum * distScore));
                //ShotOpportunity shot = Shot1.evaluatePosition(fmsg, pos, team);
                //map[i, j] = normalize(shotMap[i, j] * bounceScore * shot.arc);
                //msngr.vdb(new Vector2(x,y), Utilities.ColorUtils.numToColor(map[i,j],0,20));
            });
        }

        // used for debugging drawn maps
        public void drawMap(Lattice<double> map)
        {
            int min = 101;
            int max = -1;

            foreach (var pair in map)
            {
                if (pair.Value < min)
                {
                    min = (int)(pair.Value);
                }
                if (pair.Value > max)
                {
                    max = (int)(pair.Value);
                }
            }

            foreach (var pair in map)
            {
                //Console.WriteLine("min: " + min + " max: " + max + " map: " + map[i, j]);
                msngr.vdb(pair.Key, RFC.Utilities.ColorUtils.numToColor(pair.Value, min, max));
            }
        }

        private bool isLocalMax(Lattice<double> map, double val, int i, int j)
        {
            if (val < map.Get(i + 1, j + 0, Double.NegativeInfinity))
                return false;
            if (val < map.Get(i + 1, j - 1, Double.NegativeInfinity))
                return false;
            if (val < map.Get(i + 0, j - 1, Double.NegativeInfinity))
                return false;
            if (val < map.Get(i - 1, j - 1, Double.NegativeInfinity))
                return false;
            if (val < map.Get(i - 1, j + 0, Double.NegativeInfinity))
                return false;
            if (val < map.Get(i - 1, j + 1, Double.NegativeInfinity))
                return false;
            if (val < map.Get(i + 0, j + 1, Double.NegativeInfinity))
                return false;
            if (val < map.Get(i + 1, j + 1, Double.NegativeInfinity))
                return false;
            return true;
        }

        // Nonmax suppression
        public Lattice<double> nonMaxSupression(Lattice<double> map)
        {
            return map.Map((val, i, j) => isLocalMax(map, val, i, j) ? val : 0);
        }

        // get list from Nonmax supression
        public List<QuantifiedPosition> getLocalMaxima(Lattice<double> map)
        {
            double[,] rawMap = (double[,])map;

            List<QuantifiedPosition> maxima = new List<QuantifiedPosition>();
            //msngr.vdbClear();
            for (int i = 1; i < rawMap.GetLength(0) - 1; i++)
            {
                for (int j = 1; j < rawMap.GetLength(1) - 1; j++)
                {
                    double curr = rawMap[i, j];
                    if(isLocalMax(map, curr, i, j))
                        maxima.Add(new QuantifiedPosition(new RobotInfo(map.Spec.indexToVector(i, j), 0, team, 0), curr));
                    //msngr.vdb(indToVec(i, j), Color.White);

                }
            }
            return maxima;
        }
    }
}