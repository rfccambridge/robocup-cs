using System;
using System.Collections.Generic;
using System.Text;
using RFC.Geometry;

namespace RFC.Core {

	public interface IReferee
	{
		PlayType GetCurrentPlayType();
		Score GetScore();
		void LoadConstants();
	}
	public interface IRefBoxHandler
	{
		void Connect(string addr, int port);
		void Disconnect();
		void Start();
		void Stop();        
		int GetCmdCounter();
		char GetLastCommand();
		Score GetScore();
	}
	public interface IRefBoxListener : IRefBoxHandler
	{
		event EventHandler<EventArgs<char>> PacketReceived;
		bool IsReceiving();
	}

	/// <summary>
	/// An interface for things that will take and handle info about the current state,
	/// usually to later give it back out.  
	/// 
	/// Implementations: FieldState, ISplitInfoAcceptor
	/// </summary>
	public interface IInfoAcceptor
	{
		void UpdateRobot(RobotInfo newInfo);
		void UpdateBallInfo(BallInfo ballInfo);
	}

	public interface IVisionInfoAcceptor {
		/// <summary>
		/// Updates the state of (usually) a Predictor based on a vision message.
		/// </summary>
		/// <param name="msg">The message received from Vision</param>
		void Update(VisionMessage msg);
	}

	/// <summary>
	/// An interface for things that will take and handle info about the current state,
	/// usually to later give it back out.  More specifically, for when there are multiple
	/// things that will be giving information (ie: two cameras).
	/// 
	/// Implementations: BasicPredictor
	/// </summary>
	public interface ISplitInfoAcceptor
	{
		void updatePartOurRobotInfo(List<RobotInfo> newInfos, string splitName);
		void updatePartTheirRobotInfo(List<RobotInfo> newInfos, string splitName);
		void updateBallInfo(BallInfo ballInfo);
		//void clearTheirRobotInfo(int offset);
	}

	/// <summary>
	/// Abstracts away a source of data on robot positions.
	/// The returned lists are copies -- any changes to them will not be seen anywhere else.
	/// Implementations: MRSPredictor, CameraPredictor, KalmanPredictor
	/// 
	/// <remarks>These operations could be expensive, and may change during the course of a single function.
	/// So it's best to create a copy of the results at the beginning and use the copy.</remarks>
	/// </summary>
	public interface IPredictor {       
		//returns information about the robots (position, velocity, orientation)
		//we don't care where it got its information from
		List<RobotInfo> GetRobots(Team team);
		List<RobotInfo> GetRobots();

		RobotInfo GetRobot(Team team, int id);
		/// <summary>
		/// Returns ball position
		/// </summary>
		/// <returns></returns>
		BallInfo GetBall();
		/// <summary>
		/// Marks used by hasBallMoved to track if ball has moved
		/// </summary>
		void SetBallMark();
		/// <summary>
		/// Marks used by hasBallMoved to track if ball has moved
		/// </summary>
		void ClearBallMark();
		/// <summary>
		/// Tracks if the ball has moved (does not use getBallInfo, of course); 
		/// use set/clearBallMark() to manage the tracking
		/// </summary>
		/// <returns></returns>
		bool HasBallMoved();
		/// <summary>
		/// Sets the type of play. A Predictor uses the PlayType if configured 
		/// to return an assumed ball position (based on referee box state).
		/// </summary>
		void SetPlayType(PlayType newPlayType);
		/// <summary>
		/// Re-initialize constant values from the Constants database
		/// </summary>
		void LoadConstants();
	}

	/// <summary>
	/// An interface for breaking the commands down from a mid-level tactical decision
	/// (ex: "kick the ball to here") to commands such as "move here" then "turn to face here"
	/// and then "kick".
	/// </summary>
	public interface IActionInterpreter
	{
		void Charge(int robotID);
		void Charge(int robotID, int strength);
		void Kick(int robotID, Vector2 target);
		void Kick(int robotID, Vector2 target, int stregnth);
		void Bump(int robotID, Vector2 target);
		void Move(int robotID, Vector2 target);
		void Move(int robotID, Vector2 target, Vector2 facing);
		void Move(int robotID, bool avoidBall, Vector2 target, Vector2 facing);
		void Stop(int robotID);
		void Dribble(int robotID, Vector2 target);
		void LoadConstants();
	}

	/// <summary>
	/// Takes any single order for a robot ("move to here", "kick the kicker", "stop"), and
	/// has the robot execute it.
	/// </summary>
	public interface IController {
		void Connect(string host, int port);
		void Disconnect();
		void StartControlling();
		void StopControlling();

		/// <summary>
		/// Move to desitnation while keeping current orientation; Velocity at destination = 0;
		/// </summary>
		void Move(int robotID, bool avoidBall, Vector2 dest);

		/// <summary>
		/// Move to (destination, orienatation); velocity at desitantion = 0;
		/// </summary>
		void Move(int robotID, bool avoidBall, Vector2 dest, double orientation);

		/// <summary>
		/// Most general; Move to (destination, orientation, velocity, angular_velocity); 
		/// Assumes these 4 are set to sensible values
		/// </summary>
		void Move(RobotInfo robotID, bool avoidBall);

		void Charge(int robotID);
		void Charge(int robotID, int strength);
		void BreakBeam(int robotID, int strength);        
		void Kick(int robotID, Vector2 target);
		void Stop(int robotID);
		void StartDribbling(int robotID);
		void StopDribbling(int robotID);
		void LoadConstants();
	}

	/** Abstraction for the robot movement controller
     *  
     *  Implementations: RFCMovement, JMovement
     * 
     */
	public interface IMovement {
		WheelSpeeds calculateWheelSpeeds(IPredictor predictor, int robotID, RobotInfo currentInfo, NavigationResults results);
		WheelSpeeds calculateWheelSpeeds(IPredictor predictor, int robotID, RobotInfo currentInfo, NavigationResults results, double desiredOrientation);
	}

	/// <summary>
	/// An interface for things that convert between field coordinates
	/// (ie (0,0) is the center of the field, (1,1) is up and to the right, and units are meters)
	/// and pixel coordinates (ie (0,0) is the top left corner, (1,1) is down and to the right, the unit is pixels)
	/// </summary>
	public interface ICoordinateConverter
	{
		int fieldtopixelX(double x);
		int fieldtopixelY(double y);
		double fieldtopixelDistance(double f);
		double pixeltofieldDistance(double f);
		Vector2 fieldtopixelPoint(Vector2 p);
		double pixeltofieldX(double x);
		double pixeltofieldY(double y);
		Vector2 pixeltofieldPoint(Vector2 p);
	}
}
