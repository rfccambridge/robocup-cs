using System;
using RFC.Core;

namespace RFC.Messaging
{
	public class BallVisionMessage : Message
	{
		public BallInfo ball;

		public BallVisionMessage (BallInfo ball)
		{
			this.ball = ball;
		}
	}
}

