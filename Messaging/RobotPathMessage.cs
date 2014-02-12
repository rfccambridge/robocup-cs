using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RFC.Core;

namespace RFC.Messaging
{
    public class RobotPathMessage : Message
    {
        public RobotPath Path { get; private set; }

        public RobotPathMessage(RobotPath path)
        {
            Path = path;
        }
    }
}
