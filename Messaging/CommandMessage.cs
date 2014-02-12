using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RFC.Core;

namespace RFC.Messaging
{
    public class CommandMessage : Message
    {
        public RobotCommand Command { get; private set; }

        public CommandMessage(RobotCommand command)
        {
            Command = command;
        }
    }
}
