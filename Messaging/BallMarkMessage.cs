using System;
using RFC.Core;

namespace RFC.Messaging
{
	public class BallMarkMessage : Message
	{
		public BallMarkAction action;

		public BallMarkMessage (BallMarkAction action)
		{
			this.action = action;
		}
	}
}

