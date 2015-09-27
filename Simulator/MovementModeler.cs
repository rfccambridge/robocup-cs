using System;
using System.Collections.Generic;
using System.Text;
using RFC.Core;
using RFC.Geometry;

namespace RFC.Simulator
{
    /// <summary>
    /// A simple proportional model for the relationship between commanded wheel speeds for a robot and
    /// actual rotation and translation velocity.
    /// </summary>
    public class MovementModeler
    {
        const double robotToWheelRadius = 0.0783; //Distance from robot center to wheel center
        const double wheelMSPerSpeed = 0.02952;   //Conversion from wheel speed [0,127] -> m/s

        const double wheelSpeedPerMS = 1.0 / wheelMSPerSpeed; //Wheel speed corresponding to 1 m/s rotation of a single wheel
        const double wheelSpeedPerRS = wheelSpeedPerMS * robotToWheelRadius; //Wheel speed corresponding to 1 rad/s robot rotation

        //The wheel speeds corresponding to 1 m/s to the upper right
        static WheelSpeeds urBasis = new WheelSpeeds(wheelSpeedPerMS, 0, -wheelSpeedPerMS, 0);
        //The wheel speeds corresponding to 1 m/s to the upper left
        static WheelSpeeds ulBasis = new WheelSpeeds(0, wheelSpeedPerMS, 0, -wheelSpeedPerMS);
        //The wheel speeds correspoding to 1 rad/s rotation
        static WheelSpeeds rotBasis = new WheelSpeeds(wheelSpeedPerRS, wheelSpeedPerRS, wheelSpeedPerRS, wheelSpeedPerRS);

        //When changing from the current speed to a new commanded speed, every 1 / (this number of seconds),
        //the difference between the actual wheel speed and the commanded will be multiplied by 1/e
        public double changeConstlf = 8;
        public double changeConstlb = 8;
        public double changeConstrf = 8;
        public double changeConstrb = 8;

        private double Accelerate(double command, double actual, double dt, double changek)
        {
            return actual + (command - actual) * (1 - Math.Exp(-changek * dt));
        }

        /// <summary>
        /// Compute new wheel speeds for the robot, given the commanded and the actual speeds, and the amount
        /// of time passed since the command
        /// </summary>
        private WheelSpeeds GetNewWheelSpeeds(WheelSpeeds command, WheelSpeeds actual, double dt)
        {
            return new WheelSpeeds(
                Accelerate(command.rf, actual.rf, dt, changeConstrf),
                Accelerate(command.lf, actual.lf, dt, changeConstlf),
                Accelerate(command.lb, actual.lb, dt, changeConstlb),
                Accelerate(command.rb, actual.rb, dt, changeConstrb));
        }

        /// <summary>
        /// Given a robot info (velocities), compute what the current wheel speeds are
        /// <summary>
        public WheelSpeeds GetWheelSpeedsFromInfo(RobotInfo info)
        {
            //Compute robot-frame velocity in the basis (x,y) = (UL, UR) instead of (x,y) = (left,up)
            Vector2 robotVel = info.Velocity.rotate(-info.Orientation - Math.PI / 4);
            return robotVel.X * urBasis + robotVel.Y * ulBasis + info.AngularVelocity * rotBasis;
        }

        public double GetAngularVelocityFromWheel(WheelSpeeds wheel)
        {
            //Project wheel speeds on to rotational basis
            return wheel.getProjectionFactor(rotBasis);
        }

        public Vector2 GetVelocityFromWheel(WheelSpeeds wheel, double orientation)
        {
            //Project wheel speeds on to each axis and add, to obtain the velocity in the robot frame's (UR,UL) basis.
            Vector2 robotVel = new Vector2(wheel.getProjectionFactor(urBasis), wheel.getProjectionFactor(ulBasis));
            return robotVel.rotate(Math.PI / 4 + orientation);
        }

        public Tuple<Vector2, double> GetInfoFromWheel(WheelSpeeds Wheel, double orientation)
        {
            double AngularVelocity = GetAngularVelocityFromWheel(Wheel);
            Vector2 newVelocity = GetVelocityFromWheel(Wheel, orientation);

            return new Tuple<Vector2, double>(newVelocity, AngularVelocity);
        }

        /// <summary>
        /// Given the current state of a robot, the command most recently sent to the robot,
        /// extrapolates the state of the robot forward a given amount of time.
        /// </summary>
        /// <param name="info">The current state of the robot.</param>
        /// <param name="command">The command last sent to the robot.</param>
        /// <param name="dt">The amount of time, in seconds, to extrapolate forward.
        /// It is assumed that dt is less than 1/10 of a second.</param>
        /// <returns>Returns the state of the robot, extrapolated a time dt into the future.</returns>       
        public RobotInfo ModelWheelSpeeds(RobotInfo info, WheelSpeeds command, double dt)
        {
            WheelSpeeds currentWheel = GetWheelSpeedsFromInfo(info);
            WheelSpeeds newWheel = GetNewWheelSpeeds(command, currentWheel, dt);
            double newAngularVelocity = GetAngularVelocityFromWheel(newWheel);
            double angle = info.Orientation + (info.AngularVelocity + newAngularVelocity) / 2 * dt;

            Tuple<Vector2, double> vwpair = GetInfoFromWheel(newWheel, angle);
            Vector2 newVelocity = vwpair.Item1;
            Vector2 newPosition = info.Position + 0.5 * dt * (newVelocity + info.Velocity);

            return new RobotInfo(newPosition, newVelocity, newAngularVelocity, angle, info.Team, info.ID);
        }
    }
}