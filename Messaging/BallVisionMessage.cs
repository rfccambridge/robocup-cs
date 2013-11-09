using System;
using RFC.Core;

namespace RFC.Messaging
{
	public class BallVisionMessage : Message
	{
		public BallInfo ball
		{
			get { return ball; }
		}        

		public BallVisionMessage (BallInfo ball)
		{
			this.ball = ball;
		}
	}
}

