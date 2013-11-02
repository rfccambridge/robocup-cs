using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace RefBox
{
    public class MulticastRefboxSender : RefBoxHandler
    {
        byte cmd_counter = 0;    // counter for current command
        int goals_blue = 0;      // current score for blue team
        int goals_yellow = 0;    // current score for yellow team
        int time_remaining = 0; // seconds remaining for current game stage (network byte order)

        override public void Connect(String addr, int port)
        {
            if (_socket != null)
                throw new ApplicationException("Already connected.");

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPAddress mcastAddr = IPAddress.Parse(addr);

            _endPoint = new IPEndPoint(mcastAddr, port);
            _socket.Connect(_endPoint);

            _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(mcastAddr));
        }

        protected override void Loop()
        {
            while (true)
            {
                RefBoxPacket lastPacket = GetLastPacket();
                SendPacket(lastPacket);
                System.Threading.Thread.Sleep(1000);
            }
        }

        public void SendCommand(char command)
        {
            lock (lastPacketLock)
            {
                if (command != _lastPacket.cmd)
                    cmd_counter++;
            }

            switch (command)
            {
                case 'g':
                    goals_yellow++;
                    break;
                case 'G':
                    goals_blue++;
                    break;
                default:
                    break;
            }

            RefBoxPacket packet = new RefBoxPacket();
            packet.goals_yellow = (byte)goals_yellow;
            packet.goals_blue = (byte)goals_blue;
            packet.cmd = command;
            packet.cmd_counter = cmd_counter;
            //TODO(davidwu): Why are we not setting packet.time_remaining ?

            lock (lastPacketLock)
            {
                _lastPacket = packet;
            }

            SendPacket(packet);
        }

        public void SendPacket(RefBoxPacket packet)
        {
            _lastPacket = packet;
            if (_socket == null)
                throw new ApplicationException("Socket not connected.");

            byte[] message = packet.toByte();
            _socket.SendTo(message, _endPoint);
        }
    }
}
