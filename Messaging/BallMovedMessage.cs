using System;

namespace RFC.Messaging
{
	public class BallMovedMessage
	{
		public bool moved
		{
			get { return moved; }
		} 

		public BallMovedMessage (bool moved)
		{
			this.moved = moved;
		}
	}
}

