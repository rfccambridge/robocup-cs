using System;
using RFC.Messaging;

namespace RFC.CoreRobotics
{
	public interface IVisionInfoAcceptor {
		/// <summary>
		/// Updates the state of (usually) a Predictor based on a vision message.
		/// </summary>
		/// <param name="msg">The message received from Vision</param>
		void Update(VisionMessage msg);
	}
}

