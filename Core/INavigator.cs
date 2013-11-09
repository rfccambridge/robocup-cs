using System;
using System.Collections.Generic;
using System.Text;
using RFC.Geometry;

namespace RFC.Core
{
	/// <summary>
	/// A simple data structure for holding the various data that a navigation algorithm will return.
	/// </summary>
	public class NavigationResults
	{
		public NavigationResults(Vector2 waypoint)
		{
			this.waypoint = waypoint;
		}
		/// <summary>
		/// The point to which the robot should start moving, to get to the final destination
		/// </summary>
		public Vector2 waypoint;
	}

	/// <summary>
	/// The interface to any navigation class.
	/// Each navigation object is responsible for navigating all the robots on one team
	/// (though not the other team, in the case that we are playing against ourselves).
	/// 
	/// Each navigator must implement these two functions: navigate() and drawLast().
	/// navigate() is the main method of the navigator--it takes the destination, and gives back
	/// an intermediate destination to avoid obstacles.
	/// drawLast() is a method for convenience -- after running navigate(), the racer will run drawLast().
	/// You can use this to draw stuff onto the screen, to show you what's going on.  Since the screen
	/// and the field use different coordinate systems, use the coordinate converter that is passed to
	/// convert your field values into pixel values.
	/// Don't count on drawLast() being run, though.  It's designed for debug purposes, so during normal
	/// running it probably won't be on.\
	/// drawLast() also will be run before navigate() is run for the first time, so be careful if you
	/// expect fields to have saved data from the previous run.
	/// </summary>
	public interface INavigator
	{
		/// <summary>
		/// Allows you to draw things onto the screen.
		/// </summary>
		/// <param name="g">The Graphics object that you should draw to, which corresponds to the screen.</param>
		/// <param name="c">The coordinate converter that will provide methods to convert between
		/// field-space and pixel-space (and back, if you really need it for some reason)</param>
		void drawLast(System.Drawing.Graphics g, ICoordinateConverter c);

		/// <summary>
		/// Returns a waypoint that a robot should aim for, as it is going towards its destination.
		/// None of the fields get altered (ie the arrays)
		/// </summary>
		/// <param name="id">The id of the robot.  It can have any value</param>
		/// <param name="position">The position of the robot being navigated.</param>
		/// <param name="destination">The final destination of the robot.</param>
		/// <param name="teamPositions">The positions of the friendly robots (including the one being navigated).  This array has at most 5 elements.</param>
		/// <param name="enemyPositions">The positions of the enemy robots.  This array can have any size.</param>
		/// <param name="ballPosition">The position of the ball.</param>
		/// <param name="avoidBall">How much to avoid the ball.  If it is zero, the ball is not avoided.</param>
		/// <returns>A NavigationResults item containing the necessary data (see NavigationResults)</returns>
		NavigationResults navigate(int id, Vector2 position, Vector2 destination,
		                           RobotInfo[] teamPositions, RobotInfo[] enemyPositions, BallInfo ballPosition, double avoidBallRadius);
	}
}