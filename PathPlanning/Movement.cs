using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RFC.Core;

namespace RFC.PathPlanning
{
    class Movement
    {
        SmoothRRTPlanner _planner;
        VelocityDriver _driver;

        public Movement()
        {
            _planner = new SmoothRRTPlanner(true);
            _driver = new VelocityDriver();
            LoadConstants();
        }

        /// <summary>
        /// Reloads constants for planner and driver
        /// </summary>
        public virtual void LoadConstants()
        {
            _planner.ReloadConstants();
            _driver.ReloadConstants();
        }

        public RobotPath PlanMotion(RobotInfo desiredState, IPredictor predictor,
            double avoidBallRadius, RobotPath oldPath, SmoothRRTPlanner.DefenseAreaAvoid leftAvoid, SmoothRRTPlanner.DefenseAreaAvoid rightAvoid)
        {
            RobotPath path = _planner.GetPath(desiredState, avoidBallRadius, oldPath, leftAvoid, rightAvoid);

            // if path is empty, don't move
            if (path.isEmpty())
            {
                return path;
            }

            // Make sure path contains desired state
            path.setFinalState(desiredState);
            return path;
        }

        public WheelSpeeds FollowPath(RobotPath path, IPredictor predictor)
        {
            try
            {
                WheelSpeeds speeds = _driver.followPath(path, predictor);
                return speeds;
            }
            catch (ApplicationException e)
            {
                Console.WriteLine(e.Message + "\r\n" + e.StackTrace);
                return new WheelSpeeds();
            }
        }
    }
}
