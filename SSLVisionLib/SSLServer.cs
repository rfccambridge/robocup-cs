using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization;

namespace RFC.SSLVisionLib
{
	public class SSLVisionServer
	{
		private Socket socket;
		private IPEndPoint endPoint;
		private bool connected;

		private const int MAX_BUF_SIZE = 65536;

		public void Connect(string address, int port)
		{
			if (socket != null)
				throw new ApplicationException("Already connected.");

			socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			IPAddress mcastAddr = IPAddress.Parse(address);
			endPoint = new IPEndPoint(mcastAddr, port);
			socket.Connect(endPoint);

			socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(mcastAddr));

			connected = true;
		}

		public void Disconnect()
		{
			socket.Close();
			socket.Dispose();

			connected = false;
		}

		public void Send(SSL_WrapperPacket packet)
		{
			if (!connected)
				throw new ApplicationException("Trying to send to vision SSLClient socket that wasn't open");

			MemoryStream stream = new MemoryStream();
			ProtoBuf.Serializer.Serialize<SSL_WrapperPacket>(stream, packet);
			byte[] buf = stream.GetBuffer();

			socket.SendTo(buf, endPoint);
		}
	}
}
