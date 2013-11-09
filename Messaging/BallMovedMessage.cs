using System;

namespace RFC.Messaging
{
	public class BallMovedMessage : Message
	{
		public bool moved;

		public BallMovedMessage (bool moved)
		{
			this.moved = moved;
		}
	}
}

