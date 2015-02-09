using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RFC.Messaging;
using RFC.InterProcessMessaging;
using RFC.Core;

namespace RFC.Commands
{
    public class SimulatorSender
    {
        const string host = "localhost";
        const int port = 50101;

        ClientMessageSender<RobotCommand> sender;

        public SimulatorSender()
        {
            sender = new ClientMessageSender<RobotCommand>(host, port);
            new QueuedMessageHandler<CommandMessage>(handleRobotCommandMessage, new object());
        }

        public void handleRobotCommandMessage(CommandMessage message)
        {
            sendCommand(message.Command);
        }

        private void sendCommand(RobotCommand command)
        {
            sender.Post(command);
        }
    }
}
