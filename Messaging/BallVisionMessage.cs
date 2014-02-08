using System;
using RFC.Core;

namespace RFC.Messaging
{
	public class BallVisionMessage : Message
	{
		public BallInfo Ball {get; private set;}

		public BallVisionMessage (BallInfo ball)
		{
			this.Ball = ball;
		}
	}
}

