using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RFC.Messaging;
using RFC.InterProcessMessaging;
using RFC.Core;

namespace RFC.Commands
{
    public class SimulatorSender : IMessageHandler<CommandMessage>
    {
        const string host = "localhost";
        const int port = 50100;

        ClientMessageSender<RobotCommand> sender;

        public SimulatorSender()
        {
            sender = new ClientMessageSender<RobotCommand>(host, port);
            new QueuedMessageHandler<CommandMessage>(HandleMessage, new object());
        }

        public void HandleMessage(CommandMessage message)
        {
            sendCommand(message.Command);
        }

        private void sendCommand(RobotCommand command)
        {
            sender.Post(command);
        }
    }
}
