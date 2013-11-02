using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization;

namespace RFC.SSLVisionLib
{
	public class SSLVisionClient
	{
		private Socket socket;
		private bool connected;

		private const int MAX_BUF_SIZE = 65536;

		public void Connect(string address, int port)
		{
			socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);

			try
			{
				socket.Bind(endPoint);
			}
			catch (SocketException)
			{
				Console.WriteLine("Another receiver already bound to SSL Vision, not connecting.");
			}
			socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(IPAddress.Parse(address)));

			connected = true;
		}

		public void Disconnect()
		{
			socket.Close();
			socket.Dispose();

			connected = false;
		}

		public SSL_WrapperPacket Receive()
		{
			if (!connected)
				throw new ApplicationException("Trying to receive from vision SSLClient socket that wasn't open");

			byte[] buf = new byte[MAX_BUF_SIZE];
			int received = socket.Receive(buf);
			if (received == 0)
				return null;

			MemoryStream stream = new MemoryStream(buf);
			SSL_WrapperPacket packet = ProtoBuf.Serializer.Deserialize<SSL_WrapperPacket>(stream);

			return packet;
		}
	}
}
