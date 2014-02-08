using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using RFC.Utilities;

namespace RFC.Commands
{
    class SerialSender
    {
        SerialPort _comPort;

        public SerialSender(int comNumber)
        {
            string port = "COM" + comNumber;
            _comPort = SerialPortManager.OpenSerialPort(port);
        }

        private void sendCommand(RobotCommand command)
        {
            byte[] packet = command.ToPacket();
            _comPort.Write(packet, 0, packet.Length);
        }
    }
}
