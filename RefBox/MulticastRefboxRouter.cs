using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace RefBox
{
    public class MultiCastRefBoxRouter : RefBoxHandler
    {
        List<Socket> senderSockets;
        bool routing;

        public MultiCastRefBoxRouter()
        {
            senderSockets = new List<Socket>();
        }

        /// <summary>
        /// Router tries to connect to the actual refbox and opens
        /// several sender connections for multiple listeners to listen on.
        /// If it can't bind to listener socket, another router is running,
        /// so there's no need to run this one.
        /// </summary>
        /// <param name="address">Multicast address refbox is sending to</param>
        /// <param name="port">Port of the multicast</param>
        override public void Connect(string address, int port)
        {
            if (!ConnectReceiverSocket(address, port))
            {
                routing = false;
                return;
            }

            routing = true;
            for (int i = 0; i < Parameters.routerPorts.Count; i++)
            {
                Socket socket = null;
                ConnectSenderSocket(ref socket, Parameters.routerIP, Parameters.routerPorts[i]);
                senderSockets.Add(socket);
            }
        }

        public override void Disconnect()
        {
            base.Disconnect();

            foreach (Socket socket in senderSockets)
            {
                socket.Close();
                socket.Dispose();
            }
            senderSockets.Clear();
        }

        protected override void Loop()
        {
            if (!routing)
                return;

            RefBoxPacket packet = new RefBoxPacket();
            while (true)
            {
                byte[] message = new byte[packet.getSize()];
                int rcv = _socket.ReceiveFrom(message, 0, packet.getSize(), SocketFlags.None, ref _endPoint);

                if (rcv == packet.getSize())
                {
                    for (int i = 0; i < senderSockets.Count; i++)
                        senderSockets[i].Send(message);
                }
            }
        }

        protected void ConnectSenderSocket(ref Socket socket, IPAddress addr, int port)
        {
            if (socket != null)
                return;

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            IPEndPoint currEndPoint = new IPEndPoint(addr, port);
            socket.Connect(currEndPoint);
        }

        protected bool ConnectReceiverSocket(string addr, int port)
        {
            if (_socket != null)
                return false;

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _endPoint = new IPEndPoint(IPAddress.Any, port);

            try
            {
                _socket.Bind(_endPoint);
            }
            catch (SocketException)
            {
                Console.WriteLine("Another receiver already bound to refbox, not connecting.");
                return false;
            }
            _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(IPAddress.Parse(addr)));

            return true;
        }
    }
}
