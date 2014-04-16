using RFC.Core;
using RFC.Geometry;
using RFC.Messaging;
using RFC.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RFC.Strategy
{
    public class OccOffenseMapper : OffenseMapper
    {
        // normalize map to this number
        public const double NORM_TO = 100.0;
        // higher num -> better resolution
        public const int LAT_NUM = 20;
        // in degrees
        public const double BOUNCE_ANGLE = 20.0;
        // how far away from the line of sight should we ignore other robots?
        public const double IGN_THRESH = 20.0;

        private readonly double LAT_HSTART;
        private readonly double LAT_VSTART;

        private readonly double LAT_HEND;
        private readonly double LAT_VEND;

        private readonly double LAT_HSIZE;
        private readonly double LAT_VSIZE;

        private Boolean fs;

        private double[,] shotMap = new double[LAT_NUM, LAT_NUM];

        public OccOffenseMapper(Boolean fieldSide, List<RobotInfo> ourTeam, List<RobotInfo> theirTeam, BallInfo ball)
        {
            this.fs = fieldSide;

            // always attack right
            LAT_HSTART = (Constants.Field.XMAX - Constants.Field.XMIN) / 2.0 + Constants.Field.XMIN;
            LAT_VSTART = Constants.Field.YMIN;

            LAT_HEND = Constants.Field.XMAX;
            LAT_VEND = Constants.Field.YMAX;

            LAT_HSIZE = (LAT_HEND - LAT_HSTART) / LAT_NUM;
            LAT_VSIZE = (LAT_VEND - LAT_VSTART) / LAT_NUM;
            // attack left
            update(ourTeam, theirTeam, ball);
        }

        public Vector2 indToVec(int i, int j)
        {
            return new Vector2(i * this.LAT_HSIZE + this.LAT_HSTART, j * this.LAT_VSIZE + this.LAT_VSTART);
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

        public void update(List<RobotInfo> ourTeam, List<RobotInfo> theirTeam, BallInfo ball)
        {
            for (double x = LAT_HSTART; x < LAT_HEND; x += LAT_HSIZE)
            {
                for (double y = LAT_VSTART; y < LAT_VEND; y += LAT_VSIZE)
                {
                    Vector2 pos = new Vector2(x, y);

                    // find angle of opening for position
                    Vector2 vecBotGoal = Constants.FieldPts.THEIR_GOAL_BOTTOM - pos;
                    Vector2 vecTopGoal = Constants.FieldPts.THEIR_GOAL_TOP - pos;
                    double goalAngle = Math.Acos(vecBotGoal.cosineAngleWith(vecTopGoal));

                    // iterate through other robots, adding distance between (line of sight between position and goal) and other robot
                    Vector2 vecCentGoal = pos - Constants.FieldPts.THEIR_GOAL;
                    double distSum = 200.0;
                    foreach (RobotInfo rob in theirTeam)
                    {
                        double m = vecCentGoal.Y / vecCentGoal.X;
                        double b = y - m * x;
                        double dist = Math.Abs(rob.Position.Y - m * rob.Position.X - b) / Math.Sqrt(m * m + 1);
                        if (dist < IGN_THRESH)
                        {
                            distSum -= IGN_THRESH * Math.Exp(-dist);
                        }
                    }

                    int i = (int)((x - LAT_HSTART) / LAT_HSIZE);
                    int j = (int)((y - LAT_VSTART) / LAT_VSIZE);
                    // shouldn't happen but just to be safe
                    if (i > LAT_NUM - 1)
                    {
                        i = LAT_NUM - 1;
                    }
                    if (j > LAT_NUM - 1)
                    {
                        j = LAT_NUM - 1;
                    }
                    // make nonlinear (put in threshold)
                    // subtract (so that number of robots doesn't factor in)
                    shotMap[i, j] = normalize(goalAngle * distSum);
                }
            }
        }

        // dribble to better place--doesn't depend on bounce angle
        // should only depend on how good shot is?
        public double[,] getDrib(List<RobotInfo> ourTeam, List<RobotInfo> theirTeam, BallInfo ball)
        {
            return shotMap;
            // distance from ball to pos?
        }

        // within bounce angle -> good
        // make sure pass between ball and position is good
        // make sure position has good shot
        public double[,] getPass(List<RobotInfo> ourTeam, List<RobotInfo> theirTeam, BallInfo ball)
        {
            double[,] map = new double[LAT_NUM, LAT_NUM];
            for (double x = LAT_HSTART; x < LAT_HEND; x += LAT_HSIZE)
            {
                for (double y = LAT_VSTART; y < LAT_VEND; y += LAT_VSIZE)
                {
                    Vector2 pos = new Vector2(x, y);
                    Vector2 vecToBall = ball.Position - pos;
                    Vector2 vecToGoal = Constants.FieldPts.THEIR_GOAL - pos;

                    // see if position has good line of sight with ball
                    double distSum = 200.0;
                    foreach (RobotInfo rob in theirTeam)
                    {
                        double m = vecToBall.Y / vecToBall.X;
                        double b = y - m * x;
                        double dist = Math.Abs(ball.Position.Y - m * ball.Position.X - b) / Math.Sqrt(m * m + 1);
                        if (dist < IGN_THRESH)
                        {
                            distSum -= IGN_THRESH * Math.Exp(-dist);
                        }
                    }

                    // calculate bounce score
                    // make .5(1+cos)
                    double currentBounceAngle = 180 * Math.Acos(vecToBall.cosineAngleWith(vecToGoal)) / Math.PI;
                    double bounceScore = 90 - Math.Abs(currentBounceAngle - 90);
                    double worstBounceScore = 90 - Math.Abs(BOUNCE_ANGLE - 90);
                    // if bounce score is worse than what robot can handle then position is pretty crappy
                    if (bounceScore < worstBounceScore)
                    {
                        bounceScore = 0;
                    }

                    int i = (int)((x - LAT_HSTART) / LAT_HSIZE);
                    int j = (int)((y - LAT_VSTART) / LAT_VSIZE);
                    // shouldn't happen but just to be safe
                    if (i > LAT_NUM - 1)
                    {
                        i = LAT_NUM - 1;
                    }
                    if (j > LAT_NUM - 1)
                    {
                        j = LAT_NUM - 1;
                    }
                    // make nonlinear (put in threshold)
                    // subtract (so that number of robots doesn't factor in)
                    map[i, j] = normalize(shotMap[i, j] * bounceScore * distSum);
                }
            }
            return map;
        }
    }
}