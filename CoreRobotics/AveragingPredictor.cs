using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using RFC.Core;
using RFC.Utilities;
using RFC.CoreRobotics;
using RFC.Geometry;
using RFC.Messaging;

namespace RFC.CoreRobotics
{
	/// <summary>
	/// A basic implementation of IPredictor that averages values from multiple cameras
	/// </summary>
	public class AveragingPredictor : IPredictor, IVisionInfoAcceptor
	{
		const int NUM_CAMERAS = 2;

		// The state of the field believed in by a camera
		private class FieldState
		{
			// The believed state is kept here
			private BallInfo ball = null;
			private Dictionary<Team, List<RobotInfo>> robots = new Dictionary<Team, List<RobotInfo>>();

			// For synching the above
			private object ballLock = new object();
			private object robotsLock = new object();

			// Tools for velocity measurement
			double ballDtStart;
			BallInfo ballAtDtStart = null;
			Dictionary<Team, List<RobotInfo>> robotsAtDtStart = new Dictionary<Team, List<RobotInfo>>();
			Dictionary<Team, List<double>> velocityDtStart = new Dictionary<Team, List<double>>();

			// For assigning IDs to unidentified robots; one per team
			Dictionary<Team, int> nextID = new Dictionary<Team, int>();

			public FieldState()
			{
				foreach (Team team in Enum.GetValues(typeof(Team)))
				{
					robots.Add(team, new List<RobotInfo>());
					robotsAtDtStart.Add(team, new List<RobotInfo>());
					velocityDtStart.Add(team, new List<double>());
					nextID.Add(team, -1);
				}
			}

			// Update the believed state with new observations
			public void Update(VisionMessage msg)
			{
				double time = HighResTimer.SecondsSinceStart();

				double WEIGHT_OLD = Constants.Predictor.WEIGHT_OLD;
				double WEIGHT_NEW = Constants.Predictor.WEIGHT_NEW;
				double DELTA_DIST_SQ_MERGE = Constants.Predictor.DELTA_DIST_SQ_MERGE;
				double VELOCITY_DT = Constants.Predictor.VELOCITY_DT;

				if (Constants.Predictor.FLIP_COORDINATES)
				{
					VisionMessage newMessage = new VisionMessage(msg.CameraID);
					if (msg.Ball != null)
						newMessage.Ball = new BallInfo(msg.Ball.Position.rotate(Math.PI));
					foreach (VisionMessage.RobotData robotInfo in msg.Robots)
						newMessage.Robots.Add(new VisionMessage.RobotData(robotInfo.ID, robotInfo.Team,
						                                                  robotInfo.Position.rotate(Math.PI), Geometry.Angle.AngleModTwoPi(robotInfo.Orientation + Math.PI)));
					msg = newMessage;
				}

				#region Update ball
				lock (ballLock)
				{
					// Ball can timeout for a signle camera (such as bots)
					if (msg.Ball == null)
					{
						if (ball == null ||
						    (ball != null && (time - ball.LastSeen <= Constants.Predictor.MAX_SECONDS_TO_KEEP_INFO)))
							ball = null;
					}
					// If we see the ball for the fist time, just record it; otherwise update
					else if (ball == null)
					{
						ball = new BallInfo(msg.Ball.Position, new Vector2(0, 0)); // Don't know velocity yet
						ballDtStart = time;
						ballAtDtStart = new BallInfo(ball);

						// We have just seen the ball                    
						ball.LastSeen = time;
					}
					else
					{
						BallInfo newBall = msg.Ball;

						// Update position
						ball.Position = new Vector2(WEIGHT_OLD * ball.Position + WEIGHT_NEW * newBall.Position);

						// Project velocity to compensate for delays that we are sure of
						ball.Position += ball.Velocity * msg.Delay;

						// Update velocity if a reasonable interval has passed
						double dt = time - ballDtStart;
						if (dt > VELOCITY_DT)
						{
							Vector2 d = msg.Ball.Position - ballAtDtStart.Position;
							ball.Velocity = WEIGHT_OLD * ball.Velocity + WEIGHT_NEW * d / dt;

							// Reset velocity interval
							ballDtStart = time;
							ballAtDtStart = new BallInfo(ball);
						}

						// We have just seen the ball                    
						ball.LastSeen = time;
					}
				}
				#endregion

				#region Update robots
				lock (robotsLock)
				{
					foreach (RFC.Core.VisionMessage.RobotData newRobotData in msg.Robots)
					{
						RobotInfo newRobot = new RobotInfo(newRobotData.Position, new Vector2(0, 0), 0,
						                                   newRobotData.Orientation, newRobotData.Team, newRobotData.ID);

						// Keep track of nextID
						if (newRobot.ID > nextID[newRobot.Team])
						{
							nextID[newRobot.Team] = newRobot.ID;
						}

						// Match with existing info either by ID (if vision gave it one) or by position
						Predicate<RobotInfo> matchByPosPredicate;
						matchByPosPredicate = new Predicate<RobotInfo>(delegate(RobotInfo robot)
						                                               {
							// TODO: Figure this out, temporarily forcing to look by ID
							return (robot.Position.distanceSq(newRobot.Position) < DELTA_DIST_SQ_MERGE ||
							        robot.ID == newRobot.ID);
						});

						// Find the matching robot
						int oldRobotIdx = -1;
						oldRobotIdx = robots[newRobot.Team].FindIndex(matchByPosPredicate);
						if (oldRobotIdx >= 0 && newRobot.ID >= 0 && newRobot.ID != robots[newRobot.Team][oldRobotIdx].ID)
						{
							continue;
						}

						int newRobotIdx = -1;

						// If never seen this robot before, then add it; otherwise, update
						if (oldRobotIdx < 0)
						{
							// On first frame don't know velocity                            
							robots[newRobot.Team].Add(new RobotInfo(newRobot));
							robotsAtDtStart[newRobot.Team].Add(new RobotInfo(newRobot));
							velocityDtStart[newRobot.Team].Add(time);
							newRobotIdx = robots[newRobot.Team].Count - 1;
						}
						else
						{
							RobotInfo oldRobot = robots[newRobot.Team][oldRobotIdx];

							// Update position and orientation
							oldRobot.Position = new Vector2(WEIGHT_OLD * oldRobot.Position + WEIGHT_NEW * newRobot.Position);
							oldRobot.Orientation = WEIGHT_OLD * oldRobot.Orientation + WEIGHT_NEW * newRobot.Orientation;

							// Project velocity to compensate for delays that we are sure of
							oldRobot.Position += oldRobot.Velocity * msg.Delay;

							// Update velocity if a reasonable interval has passed                    
							double dt = time - velocityDtStart[newRobot.Team][oldRobotIdx];

							if (dt > VELOCITY_DT)
							{
								Vector2 d = newRobot.Position - robotsAtDtStart[newRobot.Team][oldRobotIdx].Position;
								oldRobot.Velocity = WEIGHT_OLD * oldRobot.Velocity + WEIGHT_NEW * d / dt;

								double r = newRobot.Orientation - robotsAtDtStart[newRobot.Team][oldRobotIdx].Orientation;
								oldRobot.AngularVelocity = WEIGHT_OLD * oldRobot.AngularVelocity + WEIGHT_NEW * r / dt;

								// Reset velocity dt interval
								velocityDtStart[newRobot.Team][oldRobotIdx] = time;
								robotsAtDtStart[newRobot.Team][oldRobotIdx] = new RobotInfo(oldRobot);
							}

							newRobotIdx = oldRobotIdx;
						}

						// We have just seen this robot
						robots[newRobot.Team][newRobotIdx].LastSeen = time;
					}
				}
				#endregion
			}

			public BallInfo GetBall()
			{
				BallInfo retBall;
				lock (ballLock)
				{
					// Copy data for returning
					retBall = (ball != null) ? new BallInfo(ball) : null;
				}
				// It's ok to return null if we don't know anything about the ball
				return retBall;
			}

			public List<RobotInfo> GetRobots(Team team)
			{
				List<RobotInfo> retRobots;
				lock (robotsLock)
				{
					double time = HighResTimer.SecondsSinceStart();

					// Reconsider our belief
					List<RobotInfo> tempRobots = new List<RobotInfo>(robots[team].Count);
					for (int i = 0; i < robots[team].Count; i++)
					{
						if (time - robots[team][i].LastSeen < Constants.Predictor.MAX_SECONDS_TO_KEEP_INFO)
						{
							tempRobots.Add(robots[team][i]);
						}
					}
					robots[team].Clear();
					robots[team].AddRange(tempRobots);

					// Copy data for returning               
					retRobots = new List<RobotInfo>(robots[team].Count);
					foreach (RobotInfo robot in robots[team])
					{
						retRobots.Add(new RobotInfo(robot));
					}
				}
				// It's ok to return an empty list if we see no robots
				return retRobots;
			}
		}

		// Each camera believes in a state
		private FieldState[] fieldStates = new FieldState[NUM_CAMERAS];

		// Combined state (updated at a specified frequency)
		private BallInfo ball;
		private Dictionary<Team, List<RobotInfo>> robots = new Dictionary<Team, List<RobotInfo>>();

		// Using independent objects for locking to be able to change the object that 
		// the protected references point to
		private object ballLock = new object();
		private object robotsLock = new object();

		private System.Timers.Timer combineTimer = new System.Timers.Timer();
		private int combineTimerSync = 0;

		// For marking ball position
		private Vector2 markedPosition = null;

		public AveragingPredictor()
		{
			for (int i = 0; i < NUM_CAMERAS; i++)
			{
				fieldStates[i] = new FieldState();
			}

			LoadConstants();
			ServiceManager.getServiceManager().RegisterListener(Update);



			combineTimer.Elapsed += combineTimer_Elapsed;
			combineTimer.Start();
		}

		public void LoadConstants()
		{
			combineTimer.Interval = (1.0 / Constants.Time.COMBINE_FREQUENCY) * 1000; // Convert hz -> secs -> ms
		}

		public void Update(VisionMessage msg)
		{
			fieldStates[msg.CameraID].Update(msg);
		}

		public BallInfo GetBall()
		{
			lock (ballLock)
			{
				return ball;
			}
		}

		public List<RobotInfo> GetRobots(Team team)
		{
			lock (robotsLock)
			{
				return robots[team];
			}
		}

		public List<RobotInfo> GetRobots()
		{
			List<RobotInfo> combined = new List<RobotInfo>();
			foreach (Team team in Enum.GetValues(typeof(Team)))
				combined.AddRange(GetRobots(team));
			return combined;
		}
		public RobotInfo GetRobot(Team team, int id)
		{
			// TODO: this is frequently executed: change to use a dictionary
			List<RobotInfo> robots = GetRobots(team);
			RobotInfo robot = robots.Find((RobotInfo r) => r.ID == id);
			if (robot == null)
			{
				throw new ApplicationException("AveragingPredictor.GetRobot: no robot with id=" +
				                               id.ToString() + " found on team " + team.ToString());
			}
			return robot;
		}

		public void SetBallMark()
		{
			BallInfo ball = GetBall();
			if (ball == null)
			{
				//throw new ApplicationException("Cannot mark ball position because no ball is seen.");
				return;
			}
			markedPosition = ball != null ? new Vector2(ball.Position) : null;
		}

		public void ClearBallMark()
		{
			markedPosition = null;
		}

		public bool HasBallMoved()
		{
			BallInfo ball = GetBall();
			double BALL_MOVED_DIST = Constants.Plays.BALL_MOVED_DIST;
			bool ret = (ball != null && markedPosition == null) || (ball != null &&
			                                                        markedPosition.distanceSq(ball.Position) > BALL_MOVED_DIST * BALL_MOVED_DIST);
			return ret;
		}

		public void SetPlayType(PlayType newPlayType)
		{
			// Do nothing: this method is for assumed ball: returning clever values for the ball
			// based on game state -- i.e. center of field during kick-off
		}

		private BallInfo lastBall = null;
		// Return the average of info from all cameras weighed by the time since 
		// it was last updated
		private void combineBall()
		{
			double time = HighResTimer.SecondsSinceStart();

			// Return the average from all the cameras that see it weighted 
			// by the time since they last saw it                 

			Vector2 avgPosition = null;
			Vector2 avgVelocity = null;
			double avgLastSeen = 0;
			double sum = 0;
			for (int i = 0; i < fieldStates.Length; i++)
			{
				BallInfo ball = fieldStates[i].GetBall();
				if (ball != null)
				{
					//double t = time - ball.LastSeen;
					double t = 1;
					// First time, we don't add, just initialize
					if (avgPosition == null)
					{
						avgPosition = t * ball.Position;
						avgVelocity = t * ball.Velocity;
						avgLastSeen = t * ball.LastSeen;
						sum = t;
					}
					avgPosition += t * ball.Position;
					avgVelocity += t * ball.Velocity;
					avgLastSeen += t * ball.LastSeen;
					sum += t;
				}
			}

			BallInfo retBall = null;
			if (avgPosition != null) // if we saw at least one ball
			{
				avgPosition /= sum;
				avgVelocity /= sum;
				avgLastSeen /= sum;
				retBall = new BallInfo(avgPosition, avgVelocity, avgLastSeen);
				lastBall = new BallInfo(avgPosition, avgVelocity, avgLastSeen);
			}
			// Predict the latest position with zero velocity if all cameras have timed out
			else if (lastBall != null && (time - lastBall.LastSeen <= Constants.Predictor.MAX_SECONDS_TO_KEEP_BALL))
			{
				retBall = new BallInfo(lastBall.Position, Vector2.ZERO, lastBall.LastSeen);
			}

			lock (ballLock)
			{
				ball = retBall;
			}
		}

		// For each robot return the average of info from all cameras weighed by
		// the time since it was last updated
		private void combineRobots(Team team)
		{
			// Will store resulting list of robots
			List<RobotInfo> avgRobots = new List<RobotInfo>();

			// Infos from cameras    
			List<RobotInfo>[] fieldStateLists = new List<RobotInfo>[NUM_CAMERAS];

			// The outer list is has one entry per physical robot, each entry is a list made up of 
			// infos for that robot believed by different cameras. We later average over the inner list.
			// TODO: in the future, the parent predictor could keep it's own state so that paternless
			// ids stay stay steady
			List<List<RobotInfo>> robotSightings = new List<List<RobotInfo>>();

			// Technically, need to record acquisition time for each camera, but it's ok
			double time = HighResTimer.SecondsSinceStart();

			// For assigning IDs to unidentified robots
			int nextID = -1;

			// Acquire the info from all cameras (as concurrently as possible)
			for (int cameraID = 0; cameraID < NUM_CAMERAS; cameraID++)
			{
				fieldStateLists[cameraID] = fieldStates[cameraID].GetRobots(team);
			}

			// Construct the robot sightings list for later averaging
			for (int cameraID = 0; cameraID < NUM_CAMERAS; cameraID++)
			{
				// Iterate over robots seen by the camera
				foreach (RobotInfo fsRobot in fieldStateLists[cameraID])
				{

					// Match with existing info either by ID (if vision gave it one) or by position
					Predicate<RobotInfo> matchByIDPredicate, matchByPosPredicate;
					matchByIDPredicate = new Predicate<RobotInfo>(delegate(RobotInfo robot)
					                                              {
						return robot.ID == fsRobot.ID;
					});
					matchByPosPredicate = new Predicate<RobotInfo>(delegate(RobotInfo robot)
					                                               {
						return robot.Position.distanceSq(fsRobot.Position) < Constants.Predictor.DELTA_DIST_SQ_MERGE;
					});

					// Find the matching robot: m*n search
					int sightingsIdx;
					bool doNotAdd = false;
					for (sightingsIdx = 0; sightingsIdx < robotSightings.Count; sightingsIdx++)
					{
						int sIdx = -1;
						sIdx = robotSightings[sightingsIdx].FindIndex(matchByIDPredicate);

						// If position matches, but ID doesn't, the new robot is on top of one we already saw
						// In this case, we ignore the new one completely (arbitrary choice -- old is not really 
						// better than old)
						if (sIdx >= 0 && fsRobot.ID >= 0 && fsRobot.ID != robotSightings[sightingsIdx][sIdx].ID)
						{
							doNotAdd = true;
							continue;
						}

						// If at least one sighting was satisfactory, we merge with that list of sightings
						if (sIdx >= 0)
						{
							break;
						}
					}

					// Decided to ignore this robot because it was on top of another one
					if (doNotAdd)
					{
						continue;
					}

					// If not yet seen anywhere else, add the robot; otherwise, add it to the 
					// sightings for later averaging                    
					if (sightingsIdx == robotSightings.Count)
					{
						List<RobotInfo> sList = new List<RobotInfo>();
						sList.Add(fsRobot);
						robotSightings.Add(sList);
					}
					else
					{
						robotSightings[sightingsIdx].Add(fsRobot);
					}

					// Keep track of maximum ID, so that we can assign IDs later
					if (fsRobot.ID > nextID)
					{
						nextID = fsRobot.ID;
					}
				}
			}

			// Now we have assembled information for each robot, just take the average
			foreach (List<RobotInfo> sList in robotSightings)
			{
				RobotInfo avgRobot = new RobotInfo(sList[0]);

				// Assign an ID if the robot came without one
				if (avgRobot.ID < 0)
				{
					avgRobot.ID = ++nextID;
				}

				double t = time - avgRobot.LastSeen;
				t = 1;
				Vector2 avgPosition = t * (new Vector2(avgRobot.Position));
				Vector2 avgVelocity = t * (new Vector2(avgRobot.Velocity));
				double avgOrientation = t * avgRobot.Orientation;
				double avgAngVel = t * avgRobot.AngularVelocity;
				double avgLastSeen = t * avgRobot.LastSeen;
				double sum = t;
				for (int i = 1; i < sList.Count; i++)
				{
					//t = time - sList[i].LastSeen;
					t = 1;
					avgPosition += t * sList[i].Position;
					avgVelocity += t * sList[i].Velocity;
					avgAngVel += t * sList[i].AngularVelocity;
					avgOrientation += t * sList[i].Orientation;
					avgLastSeen += t * sList[i].LastSeen;
					sum += t;
				}
				avgPosition /= sum;
				avgVelocity /= sum;
				avgAngVel /= sum;
				avgOrientation /= sum;
				avgLastSeen /= sum;

				// Record result for returning
				avgRobots.Add(new RobotInfo(avgPosition, avgVelocity, avgAngVel, avgOrientation,
				                            team, avgRobot.ID, avgLastSeen));
			}

			lock (robotsLock)
			{
				robots[team] = avgRobots;
			}
		}

		void combineTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			// Throw away event if a previous event is still being handled
			if (Interlocked.CompareExchange(ref combineTimerSync, 1, 0) == 0)
			{
				combineBall();
				foreach (Team team in Enum.GetValues(typeof(Team)))
					combineRobots(team);

				combineTimerSync = 0;
			}
		}

	}

}
