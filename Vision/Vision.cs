using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using RFC.Core;
using RFC.Geometry;
using RFC.SSLVisionLib;
using RFC.Messaging;

namespace RFC.Vision
{
	public class Vision
	{
		public event EventHandler ErrorOccured;

		bool verbose = false;
		SSLVisionClient _client;
		bool _clientOpen = false;
		bool _running = false;
		Thread _visionThread;
        BallInfo last_ball;
        const double DT = 1.0 / 60.0;
        const double STDDEV = 1;

		public void Connect(string hostname, int port)
		{
			if (_clientOpen)
				throw new ApplicationException("Client already open.");

			_client = new SSLVisionClient();
			_client.Connect(hostname, port);
			_clientOpen = true;
            new QueuedMessageHandler<BallVisionMessage>(handleBallVisionMessage, new object());
		}

		public void Disconnect()
		{
			if (!_clientOpen)
				throw new ApplicationException("Client not open.");
			if (_running)
				throw new ApplicationException("Must stop before closing client.");

			_client.Disconnect();
			_clientOpen = false;
		}

		public void Start()
		{
			if (_running)
				throw new ApplicationException("Vision already running!");
			if (!_clientOpen)
				throw new ApplicationException("Must open client before starting.");

			// Have to create a new Thread object every time
			_visionThread = new Thread(new ThreadStart(loop));
			_visionThread.Start();
			_running = true;
		}

		public void Stop()
		{
			if (!_running)
				throw new ApplicationException("Vision not running!");

			_visionThread.Abort();
			_running = false;
		}

        public void handleBallVisionMessage(BallVisionMessage msg)
        {
            this.last_ball = msg.Ball;
        }

		private void loop()
		{
			while (true)
			{
				SSL_WrapperPacket packet = _client.Receive();
				if (packet == null)
					continue;

				//see if the packet contains a robot detection frame:
				if (packet.detection != null)
				{
					SSL_DetectionFrame detection = packet.detection;

					double t_processing = detection.t_sent - detection.t_capture;

					//Frame info:
					int balls_n = detection.balls.Count;
					int robots_blue_n = detection.robots_blue.Count;
					int robots_yellow_n = detection.robots_yellow.Count;

					VisionMessage msg = new VisionMessage((int)detection.camera_id);

					msg.Delay = t_processing;

					//Ball info:
                    Vector2 last_location;
                    if (last_ball == null)
                        last_location = new Vector2();
                    else
                        last_location = last_ball.Position + DT * last_ball.Velocity;

					float maxBallConfidence = float.MinValue;
					for (int i = 0; i < balls_n; i++)
					{
						SSL_DetectionBall sball = detection.balls[i];
                        BallInfo ball = new BallInfo(ConvertFromSSLVisionCoords(new Vector2(sball.x, sball.y)));

                        double dist_conf = 1 + Probability.gaussianPDF(ball.Position, last_location, STDDEV);

						if ((sball.confidence > 0.0) && (sball.confidence * dist_conf > maxBallConfidence))
						{
                            msg.Ball = ball;
                            
							maxBallConfidence = sball.confidence * (float)dist_conf;
						}
					}

					//Blue robots info:
					for (int i = 0; i < robots_blue_n; i++)
					{
						SSL_DetectionRobot robot = detection.robots_blue[i];
						msg.Robots.Add(new VisionMessage.RobotData((int)robot.robot_id, Team.Blue,
						                                           ConvertFromSSLVisionCoords(new Vector2(robot.x, robot.y)), robot.orientation));
					}

					//Yellow robots info:
					for (int i = 0; i < robots_yellow_n; i++)
					{
						SSL_DetectionRobot robot = detection.robots_yellow[i];
						msg.Robots.Add(new VisionMessage.RobotData((int)robot.robot_id, Team.Yellow,
						                                           ConvertFromSSLVisionCoords(new Vector2(robot.x, robot.y)), robot.orientation));
					}

					// sending vision message
                    ServiceManager.getServiceManager().SendMessage(msg);

				}

			}
		}


		private void loopErrorHandler(IAsyncResult result)
		{
			if (ErrorOccured != null)
				ErrorOccured(this, new EventArgs());
		}

		private Vector2 ConvertFromSSLVisionCoords(Vector2 v)
		{
			return new Vector2(v.X / 1000, v.Y / 1000);
		}

		private void printRobotInfo(SSL_DetectionRobot robot)
		{
			if (verbose) Console.Write(String.Format("CONF={0,4:F2} ", robot.confidence));
			if (robot.robot_id != default(uint))
			{
				if (verbose) Console.Write(String.Format("ID={0,3:G} ", robot.robot_id));
			}
			else
			{
				if (verbose) Console.Write(String.Format("ID=N/A "));
			}
			if (verbose) Console.Write(String.Format(" HEIGHT={0,6:F2} POS=<{1,9:F2},{2,9:F2}> ", robot.height, robot.x, robot.y));
			if (robot.orientation != default(float))
			{
				if (verbose) Console.Write(String.Format("ANGLE={0,6:F3} ", robot.orientation));
			}
			else
			{
				if (verbose) Console.Write(String.Format("ANGLE=N/A    "));
			}
			if (verbose) Console.Write(String.Format("RAW=<{0,8:F2},{1,8:F2}>\n", robot.pixel_x, robot.pixel_y));
		}
	}
}
