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
    public class LatticeSpec
    {
        public readonly Geometry.Rectangle Bounds;
        public readonly int Samples;

        /// <returns>the index of the cell containing v</returns>
        public void vectorToIndex(Vector2 v, out int x, out int y)
        {
            double relx = (v.X - Bounds.XMin) / Bounds.Width;
            double rely = (v.Y - Bounds.YMin) / Bounds.Height;

            if (relx < 0 || relx > 1 || rely < 0 || rely > 1) throw new IndexOutOfRangeException();

            x = (int)Math.Floor(relx * Samples);
            y = (int)Math.Floor(rely * Samples);

            // this deals with the case where the vector lies on the edge
            if (x >= Samples) x = Samples - 1;
            if (y >= Samples) y = Samples - 1;
        }

        /// <returns>the vector at the center of the cell (x,y)</returns>
        public Vector2 indexToVector(int x, int y)
        {
            // return a vector in the center of sample
            return new Vector2(
                this.Bounds.XMin + (x + 0.5) / Samples * this.Bounds.Width,
                this.Bounds.YMin + (y + 0.5) / Samples * this.Bounds.Height
            );
        }


        public LatticeSpec(Geometry.Rectangle bounds, int samples)
        {
            this.Bounds = bounds;
            this.Samples = samples;
        }


        /// <summary>
        /// Calls a function over all points in the lattice
        /// </summary>
        /// <param name="filler"></param>
        public Lattice<T> Create<T>(Func<Vector2, T> filler)
        {
            Lattice<T> lattice = new Lattice<T>(this);
            for (int i = 0; i < Samples; i++)
            {
                for (int j = 0; j < Samples; j++)
                {
                    Vector2 v = indexToVector(i, j);
                    lattice.data[i, j] = filler(v);
                }
            }
            return lattice;
        }
    }

    /// <summary>
    /// A class representing a lattice of T values calculated over a vector grid
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Lattice<T> : IEnumerable<T>
    {

        internal T[,] data;
        public readonly LatticeSpec Spec;

        /// <summary>
        /// Construct a lattice within a <paramref name="bounds"/>, with <paramref name="samples"/> cells in each axis
        /// </summary>
        public Lattice(LatticeSpec spec)
        {
            this.Spec = spec;
            this.data = new T[spec.Samples, spec.Samples];
        }

        public T this[Vector2 pos]
        {
            get
            {
                int x, y;
                Spec.vectorToIndex(pos, out x, out y);
                return data[x, y];
            }

            set
            {
                int x, y;
                Spec.vectorToIndex(pos, out x, out y);
                data[x, y] = value;
            }
        }

        // For ease of iteration / backwards compatibility
        public IEnumerator<T> GetEnumerator()
        {
            return data.Cast<T>().GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public static explicit operator T[,] (Lattice<T> l)
        {
            return l.data;
        }
    }

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

        [Obsolete]
        public int[] vecToInd(Vector2 v)
        {
            int x, y;
            latticeSpec.vectorToIndex(v, out x, out y);
            return new int[] { x, y };
        }

        [Obsolete]
        public Vector2 indToVec(int i, int j)
        {
            return latticeSpec.indexToVector(i, j);
        }

        public static void printDoubleMatrix(double[,] a)
        {
            for (int i = 0; i < a.GetLength(0); i++)
            {
                for (int j = 0; j < a.GetLength(1); j++)
                {
                    Console.Write(string.Format("{0} ", a[i, j]));
                }
                Console.Write(Environment.NewLine + Environment.NewLine);
            }
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
        public double[,] getDrib(List<RobotInfo> ourTeam, List<RobotInfo> theirTeam, BallInfo ball)
        {
            return (double[,]) shotLattice;
            // distance from ball to pos?
        }

        // within bounce angle -> good
        // make sure pass between ball and position is good
        // make sure position has good shot
        public double[,] getPass(List<RobotInfo> ourTeam, List<RobotInfo> theirTeam, BallInfo ball, FieldVisionMessage fmsg)
        {
            return (double[,]) latticeSpec.Create(pos =>
            {
                Vector2 vecToBall = ball.Position - pos;
                Vector2 vecToGoal = Constants.FieldPts.THEIR_GOAL - pos;


                // see if position has good line of sight with ball
                double distSum = 1.0;
                double tooClose = 1.0;

                foreach (RobotInfo rob in theirTeam)
                {

                    Vector2 dist = (rob.Position - pos);
                    double perpdist = dist.perpendicularComponent(vecToBall).magnitude();
                    if (perpdist < IGN_THRESH && (ball.Position - rob.Position).cosineAngleWith(vecToBall) > 0 && Vector2.dotproduct(pos, ball.Position, rob.Position) > Vector2.dotproduct(rob.Position, ball.Position, rob.Position))
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
                double distScore = Math.Atan2(1, vecToBall.magnitude());
                if (Constants.Basic.ROBOT_RADIUS * 4 > vecToBall.magnitude())
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
                double currentBounceAngle = 180 * Math.Acos(vecToBall.cosineAngleWith(vecToGoal)) / Math.PI;
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
        public void drawMap(double[,] map)
        {
            double max = map.Cast<double>().Max();
            double min = map.Cast<double>().Min();

            msngr.vdbClear();
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    //Console.WriteLine("min: " + min + " max: " + max + " map: " + map[i, j]);
                    msngr.vdb(indToVec(i, j), RFC.Utilities.ColorUtils.numToColor(map[i, j], min, max));
                }
            }
        }

        // Nonmax suppression
        public double[,] nonMaxSupression(double[,] map)
        {
            double[,] result = new double[map.GetLength(0),map.GetLength(1)];

            for (int i = 1; i < map.GetLength(0) - 1; i++)
            {
                for (int j = 1; j < map.GetLength(1) -1; j++)
                {
                    if (map[i, j] < map[i - 1, j])
                        break;
                    if (map[i, j] < map[i + 1, j])
                        break;
                    if (map[i, j] < map[i - 1, j - 1])
                        break;
                    if (map[i, j] < map[i - 0, j - 1])
                        break;
                    if (map[i, j] < map[i + 1, j - 1])
                        break;
                    if (map[i, j] < map[i - 1, j + 1])
                        break;
                    if (map[i, j] < map[i - 0, j + 1])
                        break;
                    if (map[i, j] < map[i + 1, j + 1])
                        break;
                    result[i, j] = map[i, j];
                }
            }
            return result;
        }

        // get list from Nonmax supression
        public List<QuantifiedPosition> getLocalMaxima(double[,] map)
        {
            List<QuantifiedPosition> maxima = new List<QuantifiedPosition>();
            //msngr.vdbClear();
            for (int i = 1; i < map.GetLength(0) - 1; i++)
            {
                for (int j = 1; j < map.GetLength(1) - 1; j++)
                {
                    if (map[i, j] <= map[i - 1, j])
                        continue;
                    if (map[i, j] <= map[i + 1, j])
                        continue;
                    if (map[i, j] <= map[i - 1, j - 1])
                        continue;
                    if (map[i, j] <= map[i - 0, j - 1])
                        continue;
                    if (map[i, j] <= map[i + 1, j - 1])
                        continue;
                    if (map[i, j] <= map[i - 1, j + 1])
                        continue;
                    if (map[i, j] <= map[i - 0, j + 1])
                        continue;
                    if (map[i, j] <= map[i + 1, j + 1])
                        continue;
                    maxima.Add(new QuantifiedPosition(new RobotInfo(indToVec(i, j), 0, team, 0), map[i, j]));
                    //msngr.vdb(indToVec(i, j), Color.White);

                }
            }
            return maxima;
        }
    }
}