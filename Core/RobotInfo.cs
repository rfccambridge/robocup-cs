using System;
using System.Collections.Generic;
using System.Text;
using RFC.Geometry;

namespace RFC.Core
{

	[Serializable]
	public class BallInfo
    {
        public Point2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public double LastSeen { get; set; }

		/// <summary>
		/// Creates a BallInfo with zero velocity.
		/// </summary>
		public BallInfo(Point2 position) : this(position, Vector2.ZERO) { }
		public BallInfo(Point2 position, Vector2 velocity) : this(position, velocity, -1) { }
		public BallInfo(Point2 position, Vector2 velocity, double lastSeen)
		{
			this.Position = position;
			this.Velocity = velocity;
			this.LastSeen = lastSeen;
		}
		public BallInfo(BallInfo copy)
		{
			this.Position = copy.Position;
			this.Velocity = copy.Velocity;
			this.LastSeen = copy.LastSeen;
		}

		public override string ToString()
		{
			return "BallInfo: " + Position;
		}

        // prints useful information about a robotinfo
        public string bio()
        {
            return "" + Position.X + ", " + Position.Y;
        }
	}

    /// <summary>
    /// Contains info on position, orientation and velocity of a particular robot
    /// </summary>
    public class RobotInfo
    {
        /// <summary>Creates a RobotInfo with zero velocity and angular velocity.</summary>
        public RobotInfo(Point2 position, double orientation, Team team, int id)
            : this(position, Vector2.ZERO, 0, orientation, team, id)
        { }

        public RobotInfo(Point2 position, Vector2 velocity, double angularVelocity,
                         double orientation, Team team, int id, double lastSeen = -1)
        {
            this.Position = position;
            this.Velocity = velocity;
            this.AngularVelocity = angularVelocity;
            this.Orientation = orientation;
            this.Team = team;
            this.ID = id;
            this.LastSeen = lastSeen;
        }
        /// <summary>
        /// Creates a RobotInfo that is a copy of another one.
        /// </summary>
        /// <param name="orig"></param>
        public RobotInfo(RobotInfo orig)
            : this(orig.Position, orig.Velocity, orig.AngularVelocity,
                   orig.Orientation, orig.Team, orig.ID, orig.LastSeen)
        { }

        /*
        /// <summary>
        /// The list of strings that this particular robot has been tagged with.  Ex: "goalie" if this is the goalie bot.
        /// You can add and remove tags from this.
        /// </summary>
        public List<string> Tags { get; } = new List<string>()
        */

        /// <summary>The position of the robot.</summary>
        public Point2 Position { get; set; }
        /// <summary>The velocity of the robot.</summary>
        public Vector2 Velocity { get; set; }
        /// <summary>The rotational velocity of this robot.  Units are rad/s, CCW direction is positive.</summary>
        public double AngularVelocity { get; set; }

        public double Orientation { get; set; }
        public int ID { get; set; }
        public Team Team { get; set; }
        public double LastSeen { get; set; }

        public override string ToString()
        {
            return ID + ": " + Position + " (" + Orientation + ")";
        }

        // prints useful information about a robotinfo
        public string bio()
        {
            string t = "1";
            if (Team == Team.Blue)
                t = "-1";
            return t + ", " + ID + ", " + Position.X + ", " + Position.Y + ", " + Orientation;
        }
    }
}
