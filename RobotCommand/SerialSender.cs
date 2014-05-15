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
        private int[,] timesSent;
        private DateTime[,] msSent;
        // in ms
        private const int RESET_TIME = 5000;
        private const byte MAX_SEND = 3;

        public SerialSender(int comNumber)
        {
            string port = "COM" + comNumber;
            _comPort = SerialPortManager.OpenSerialPort(port);
            new QueuedMessageHandler<CommandMessage>(handleRobotCommandMessage, new object());
            // indexed [command, robot]
            timesSent = new int[Enum.GetNames(typeof(RobotCommand.Command)).Length,RFC.Core.Constants.Basic.NUM_ROBOTS];
            msSent = new DateTime[Enum.GetNames(typeof(RobotCommand.Command)).Length,RFC.Core.Constants.Basic.NUM_ROBOTS];
            for (int i = 0; i < Enum.GetNames(typeof(RobotCommand.Command)).Length; i++)
            {
                for (int j = 0; j < RFC.Core.Constants.Basic.NUM_ROBOTS; j++)
                {
                    timesSent[i,j] = 0;
                    msSent[i,j] = DateTime.Now;
                }
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
                    int cIndex = (int)command.command;
                    int rIndex = command.ID;
                    timesSent[cIndex,rIndex]++;
                    if ((int)(DateTime.Now - msSent[cIndex,rIndex]).TotalMilliseconds >= RESET_TIME)
                    {
                        timesSent[cIndex,rIndex] = 0;
                        msSent[cIndex,rIndex] = DateTime.Now;
                    }
                    if (timesSent[cIndex,rIndex] >= MAX_SEND)
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
