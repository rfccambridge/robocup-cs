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
        private byte[] timesSent;
        private DateTime[] msSent;
        // in ms
        private const int RESET_TIME = 5000;
        private const byte MAX_SEND = 3;

        public SerialSender(int comNumber)
        {
            string port = "COM" + comNumber;
            _comPort = SerialPortManager.OpenSerialPort(port);
            new QueuedMessageHandler<CommandMessage>(handleRobotCommandMessage, new object());
            timesSent = new byte[Enum.GetNames(typeof(RobotCommand.Command)).Length];
            msSent = new DateTime[Enum.GetNames(typeof(RobotCommand.Command)).Length];
            for (int i = 0; i < Enum.GetNames(typeof(RobotCommand.Command)).Length; i++)
            {
                timesSent[i] = 0;
                msSent[i] = DateTime.Now;
            }
        }

        public void handleRobotCommandMessage(CommandMessage message)
        {
            sendCommand(message.Command);
        }

        private void sendCommand(RobotCommand command)
        {
            switch (command.command)
            {
                case RobotCommand.Command.BREAKBEAM_KICK:
                case RobotCommand.Command.START_CHARGING:
                    int index = (int)command.command;
                    timesSent[index]++;
                    if ((int)(DateTime.Now - msSent[index]).TotalMilliseconds >= RESET_TIME)
                    {
                        timesSent[index] = 0;
                        msSent[index] = DateTime.Now;
                    }
                    if (timesSent[index] >= MAX_SEND)
                    {
                        return;
                    }
                    break;
                default:
                    break;
            }
            byte[] packet = command.ToPacket();
            _comPort.Write(packet, 0, packet.Length);
        }
    }
}
