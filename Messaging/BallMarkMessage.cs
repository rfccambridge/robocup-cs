using System;
using RFC.Core;

namespace RFC.Messaging
{
	public class BallMarkMessage
	{
		public BallMarkAction action
		{
			get { return action; }
		} 

		public BallMarkMessage (BallMarkAction action)
		{
			this.action = action;
		}
	}
}

