using System;

namespace RFC.Messaging
{
	public class BallMovedMessage
	{
		public bool moved;

		public BallMovedMessage (bool moved)
		{
			this.moved = moved;
		}
	}
}

