using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using RFC.Core;
using RFC.Messaging;

namespace RFC.RefBox
{
    public class MulticastRefBoxListener : RefBoxHandler
    {
        protected MultiCastRefBoxRouter router = new MultiCastRefBoxRouter();

        /// <summary>
        /// Connects to refobx through a router. First, tries to setup a router listening
        /// to muticast addr:port.If it can't, a router is already running, so we can just try and connect to it.
        /// </summary>
        /// <param name="addr">Multicast address of refbox</param>
        /// <param name="port">Refbox port</param>
        override public void Connect(string addr, int port)
        {
            if (_socket != null)
                throw new ApplicationException("Already connected.");

            router.Connect(addr, port);

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            bool bound = false;

            for (int i = 0; i < Parameters.routerPorts.Count; i++)
            {
                _endPoint = new IPEndPoint(Parameters.routerIP, Parameters.routerPorts[i]);
                try
                {
                    _socket.Bind(_endPoint);
                    bound = true;
                }
                catch (SocketException)
                {
                    Console.WriteLine("RefBoxListener failed to connect to port {0}!", Parameters.routerPorts[i]);
                }

                if (bound)
                    break;
            }
            if (!_socket.IsBound)
                throw new ApplicationException("RefBoxListener failed to connect to all router ports!");
        }

        public override void Disconnect()
        {
            router.Disconnect();
            base.Disconnect();
        }

        public override void Start()
        {
            router.Start();
            base.Start();
        }

        public override void Stop()
        {
            router.Stop();
            base.Stop();
        }

        public bool IsReceiving()
        {
            const int MAX_ELAPSED = 3; // seconds
            lock (lastPacketLock)
            {
                TimeSpan elapsed = DateTime.Now - _lastReceivedTime;
                return elapsed.TotalSeconds <= MAX_ELAPSED;
            }
        }

        override protected void Loop()
        {
            RefBoxPacket packet = new RefBoxPacket();
            byte[] buffer = new byte[packet.getSize()];

            while (true)
            {
                int rcv = _socket.ReceiveFrom(buffer, 0, packet.getSize(), SocketFlags.None, ref _endPoint);

                packet = new RefBoxPacket();
                if (rcv == packet.getSize())
                {
                    packet.setVals(buffer);

                    char lastCommand;
                    lock (lastPacketLock)
                    {
                        lastCommand = _lastPacket.cmd;
                        _lastReceivedTime = DateTime.Now;
                        _lastPacket = packet;
                    }

                    /*Console.WriteLine("command: " + packet.cmd + " counter: " + packet.cmd_counter
                        + " blue: " + packet.goals_blue + " yellow: " + packet.goals_yellow+
                        " time left: " + packet.time_remaining);*/

                    Score score = new Score();
                    score.GoalsBlue = packet.goals_blue;
                    score.GoalsYellow = packet.goals_yellow;

                    RefboxStateMessage message = new RefboxStateMessage(score, packet.cmd, lastCommand);
                    ServiceManager.getServiceManager().SendMessage(message);
                }
                else
                {
                    Console.WriteLine("MulticastRefBoxListener: received a packet of wrong size:" + rcv +
                                      " (expecting " + packet.getSize() + ")");
                }
            }
        }

    }
}
