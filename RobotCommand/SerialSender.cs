using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using RFC.Utilities;
using RFC.Messaging;
using RFC.Core;

namespace RFC.Commands
{
    public class SerialSender
    {
        SerialPort _comPort;

        public SerialSender(int comNumber)
        {
            string port = "COM" + comNumber;
            _comPort = SerialPortManager.OpenSerialPort(port);

            ServiceManager.getServiceManager().RegisterListener<CommandMessage>(handleRobotCommandMessage, new object());
        }

        public void handleRobotCommandMessage(CommandMessage message)
        {
            sendCommand(message.Command);
        }

        private void sendCommand(RobotCommand command)
        {
            byte[] packet = command.ToPacket();
            _comPort.Write(packet, 0, packet.Length);
        }
    }
}
