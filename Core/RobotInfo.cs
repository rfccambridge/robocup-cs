using System;
using System.Collections.Generic;
using System.Text;
using RFC.Geometry;

namespace RFC.Core
{

	[Serializable]
	public class BallInfo
	{
		private Vector2 position;
		public Vector2 Position
		{
			get { return position; }
			set { position = value; }
		}
		private Vector2 velocity;
		public Vector2 Velocity
		{
			get { return velocity; }
			set { velocity = value; }
		}
		private double lastSeen;
		public double LastSeen
		{
			get { return lastSeen; }
			set { lastSeen = value; }
		}

		/// <summary>
		/// Creates a BallInfo with zero velocity.
		/// </summary>
		public BallInfo(Vector2 position) : this(position, Vector2.ZERO) { }
		public BallInfo(Vector2 position, Vector2 velocity) : this(position, velocity, -1) { }
		public BallInfo(Vector2 position, Vector2 velocity, double lastSeen)
		{
			this.position = position;
			this.velocity = velocity;
			this.lastSeen = lastSeen;
		}
		public BallInfo(BallInfo copy)
		{
			this.position = copy.position;
			this.velocity = copy.velocity;
			this.lastSeen = copy.lastSeen;
		}

		public override string ToString()
		{
			return "BallInfo: " + position;
		}
	}

	/// <summary>
	/// Contains info on position, orientation and velocity of a particular robot
	/// </summary>
	public class RobotInfo
	{        
		/// <summary>
		/// Creates a RobotInfo with zero velocity.
		/// </summary>
		public RobotInfo(Vector2 position, double orientation, Team team, int id)
			: this(position, Vector2.ZERO, 0, orientation, team, id, -1)
		{ }
		//public RobotInfo(Vector2 position, double orientation, int id)
		//	: this(position, Vector2.ZERO, 0, orientation, Team.Yellow, id, -1)
		//{ }
		public RobotInfo(Vector2 position, Vector2 velocity, double angularVelocity,
		                 double orientation, Team team, int id)
			: this(position, velocity, angularVelocity,
			       orientation, team, id, -1) { }
		//public RobotInfo(Vector2 position, Vector2 velocity, double angularVelocity,
		//                 double orientation, int id)
		//	: this(position, velocity, angularVelocity,
		//	       orientation, Team.Yellow, id, -1) { }

		public RobotInfo(Vector2 position, Vector2 velocity, double angularVelocity, 
		                 double orientation, Team team, int id, double lastSeen)
		{
			this.position = position;
			this.velocity = velocity;
			this.angularVelocity = angularVelocity;
			this.orientation = orientation;
			this.team = team;
			this.idnum = id;
			this.lastSeen = lastSeen;
		}
		/// <summary>
		/// Creates a RobotInfo that is a copy of another one.
		/// </summary>
		/// <param name="orig"></param>
		public RobotInfo(RobotInfo orig)
			: this(orig.Position, orig.Velocity, orig.AngularVelocity,
			       orig.Orientation, orig.Team, orig.ID, orig.LastSeen) { }

		/*private readonly List<string> tags = new List<string>();
        /// <summary>
        /// The list of strings that this particular robot has been tagged with.  Ex: "goalie" if this is the goalie bot.
        /// You can add and remove tags from this.
        /// </summary>
        public List<string> Tags
        {
            get { return tags; }
        }*/

		private Vector2 position;
		/// <summary>
		/// The position of the robot.
		/// </summary>
		public Vector2 Position
		{
			get { return position; }
			set { position = value; }
		}
		private Vector2 velocity;
		/// <summary>
		/// The velocity of the robot.
		/// </summary>
		public Vector2 Velocity
		{
			get { return velocity; }
			set { velocity = value; }
		}

		private double angularVelocity;
		/// <summary>
		/// The rotational velocity of this robot.  Units are rad/s, CCW direction is positive.
		/// </summary>
		public double AngularVelocity
		{
			get { return angularVelocity; }
			set { angularVelocity = value; }
		}


		private double orientation;
		public double Orientation
		{
			get { return orientation; }
			set { orientation = value; }
		}

		private int idnum;
		public int ID
		{
			get { return idnum; }
			set { idnum = value; }
		}

		private Team team;
		public Team Team
		{
			get { return team; }
			set { team = value; }
		}

		private double lastSeen;
		public double LastSeen
		{
			get { return lastSeen; }
			set { lastSeen = value; } 
		}

		public override string ToString()
		{
			return idnum + ": " + position + " (" + orientation + ")";
		}
	}
	/// <summary>
	/// This class is for use by the interpreter only.  The interpreter copies the data from RobotInfos into these objects,
	/// adding some extra data.
	/// </summary>

	public class InterpreterRobotInfo : RobotInfo
	{
		private RobotStates state;
		/// <summary>
		/// Whether this robot has an action assigned or not.
		/// </summary>
		public RobotStates State
		{
			get { return state; }
			set { state = value; }
		}
		private bool assigned = false;
		/// <summary>
		/// Whether this robot has a definition assigned or not, for this particular play.
		/// </summary>
		public bool Assigned
		{
			get { return assigned; }
			set { assigned = value; }
		}
		/// <summary>
		/// </summary>
		public void setFree()
		{
			this.State = RobotStates.Free;
			this.Assigned = false;
		}

		public InterpreterRobotInfo(RobotInfo info)
			: base(info.Position, info.Velocity, info.AngularVelocity, info.Orientation, info.Team, info.ID)
		{
			this.state = RobotStates.Free;
			this.assigned = false;
		}
	}

	public enum RobotStates
	{
		Free,
		Busy
	}
}
