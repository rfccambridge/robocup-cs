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
        Team team;

        private static readonly double LAT_HSTART = (Constants.Field.XMAX - Constants.Field.XMIN) / 2.0 + Constants.Field.XMIN;
        private static readonly double LAT_VSTART = Constants.Field.YMIN;

        private static readonly double LAT_HEND = Constants.Field.XMAX;
        private static readonly double LAT_VEND = Constants.Field.YMAX;

        private static readonly double LAT_HSIZE = (LAT_HEND - LAT_HSTART) / LAT_NUM;
        private static readonly double LAT_VSIZE = (LAT_VEND - LAT_VSTART) / LAT_NUM;

        private ServiceManager msngr;

        private double[,] shotMap = new double[LAT_NUM, LAT_NUM];

        public OccOffenseMapper(Team team)
        {
            this.team = team;
            msngr = ServiceManager.getServiceManager();
            //update(ourTeam, theirTeam, ball);
        }

        public static int[] vecToInd(Vector2 v)
        {
            return new int[] {(int)Math.Round((v.X - LAT_HSTART) / LAT_HSIZE), (int)Math.Round((v.Y - LAT_VSTART) / LAT_VSIZE)};
        }

        public static Vector2 indToVec(int i, int j)
        {
            return new Vector2(i * LAT_HSIZE + LAT_HSTART, j * LAT_VSIZE + LAT_VSTART);
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

        // zones are indexed(x,y)
        public static Vector2 getZone(int xi, int yi, int zone_num)
        {
            /*
            // id zero indexed
            double x;
            double y;
            switch (id)
            {
                // top zone
                case 0:
                    x = 1.0 * (LAT_HEND - LAT_HSTART) / 2.0 + LAT_HSTART;
                    y = 1.0 * (LAT_VEND - LAT_VSTART) / 4.0 + LAT_VSTART;
                    break;
                // bottom zone
                case 1:
                    x = 1.0 * (LAT_HEND - LAT_HSTART) / 2.0 + LAT_HSTART;
                    y = 3.0 * (LAT_VEND - LAT_VSTART) / 4.0 + LAT_VSTART;
                    break;
                // center zone
                case 2:
                    x = 3.0 * (LAT_HEND - LAT_HSTART) / 4.0 + LAT_HSTART;
                    y = 1.0 * (LAT_VEND - LAT_VSTART) / 2.0 + LAT_VSTART;
                    break;
                // TODO: make far from other points
                default:
                    x = 7.0 * (LAT_HEND - LAT_HSTART) / 8.0 + LAT_HSTART;
                    y = 1.0 * (LAT_VEND - LAT_VSTART) / 2.0 + LAT_VSTART;
                    break;
            }
            return new Vector2(x, y);
            */
            double offsetX = (LAT_HEND - LAT_HSTART) / zone_num / 2.0;
            double x = LAT_HSTART + offsetX + (LAT_HEND - LAT_HSTART) / zone_num * xi;
            double offsetY = (LAT_VEND - LAT_VSTART) / zone_num / 2.0;
            double y = LAT_VSTART + offsetY + (LAT_VEND - LAT_VSTART) / zone_num * yi;
            return new Vector2(x, y);
        }

        private static double normalize(double score)
        {
            // values might be too small as Double.MaxValue is super big...
            // return (NORM_TO / Double.MaxValue) * score;
            return score;
        }

        public void update(List<RobotInfo> ourTeam, List<RobotInfo> theirTeam, BallInfo ball, FieldVisionMessage fmsg)
        {
            for (double x = LAT_HSTART; x < LAT_HEND; x += LAT_HSIZE)
            {
                for (double y = LAT_VSTART; y < LAT_VEND; y += LAT_VSIZE)
                {
                    
                    Vector2 pos = new Vector2(x, y);
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
                    int[] ind = vecToInd(new Vector2(x, y));
                    int i = ind[0];
                    int j = ind[1];
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
                    // shotMap[i, j] = normalize(goalAngle * distSum);
                    
                    ShotOpportunity shot = Shot1.evaluatePosition(fmsg, pos, team);
                    shotMap[i, j] = shot.arc;
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
        public double[,] getPass(List<RobotInfo> ourTeam, List<RobotInfo> theirTeam, BallInfo ball, FieldVisionMessage fmsg)
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
                    double distSum = 1.0;
                    double tooClose = 1.0;
                    foreach (RobotInfo rob in theirTeam)
                    {

                        Vector2 dist = (rob.Position - pos);
                        double perpdist = dist.perpendicularComponent(vecToBall).magnitude();
                        if (perpdist < IGN_THRESH && ((vecToBall - rob.Position).magnitude() < (vecToBall - pos).magnitude()))
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
                    double distScore = Math.Atan2(1,Math.Max(Constants.Basic.ROBOT_RADIUS*4,vecToBall.magnitude()));

                    // calculate bounce score
                    // make .5(1+cos)
                    double currentBounceAngle = 180 * Math.Acos(vecToBall.cosineAngleWith(vecToGoal)) / Math.PI;
                    if (double.IsNaN(currentBounceAngle))
                        currentBounceAngle = 0;
                    double bounceScore = 90 - Math.Abs(currentBounceAngle - 90);
                    double worstBounceScore = 90 - Math.Abs(BOUNCE_ANGLE - 90);
                    // if bounce score is worse than what robot can handle then position is pretty crappy
                    if (bounceScore < worstBounceScore)
                    {
                        bounceScore = 0;
                    }

                    int[] ind = vecToInd(new Vector2(x, y));
                    int i = ind[0];
                    int j = ind[1];
                    // shouldn't happen but just to be safe
                    if (i > LAT_NUM - 1)
                    {
                        i = LAT_NUM - 1;
                    }
                    if (j > LAT_NUM - 1)
                    {
                        j = LAT_NUM - 1;
                    }

                    double isValid = 0;
                    if (Avoider.isValid(pos))
                        isValid = 1;
                    
                    map[i, j] = normalize(shotMap[i, j] * bounceScore * distSum * distScore * tooClose * isValid);

                    

                    //Console.WriteLine("result: " + normalize(shotMap[i, j] * bounceScore * distSum * distScore));
                    //ShotOpportunity shot = Shot1.evaluatePosition(fmsg, pos, team);
                    //map[i, j] = normalize(shotMap[i, j] * bounceScore * shot.arc);
                    //msngr.vdb(new Vector2(x,y), Utilities.ColorUtils.numToColor(map[i,j],0,20));
                }
            }
            return map;
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
                    msngr.vdb(OccOffenseMapper.indToVec(i, j), RFC.Utilities.ColorUtils.numToColor(map[i, j], min, max));
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
            msngr.vdbClear();
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
                    maxima.Add(new QuantifiedPosition(new RobotInfo(indToVec(i, j), 0, 0), map[i, j]));
                    msngr.vdb(indToVec(i, j), Color.White);

                }
            }
            return maxima;
        }
    }
}