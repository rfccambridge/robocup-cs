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
		private UdpClient client;
		private bool connected;
        IPEndPoint endPoint;

		private const int MAX_BUF_SIZE = 65536;

		public void Connect(string address, int port)
		{
            client = new UdpClient();
            client.ExclusiveAddressUse = false;
            endPoint = new IPEndPoint(IPAddress.Any, port);

            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client.ExclusiveAddressUse = false;

			try
			{
				client.Client.Bind(endPoint);
			}
			catch (SocketException)
			{
				Console.WriteLine("Another receiver already bound to SSL Vision, not connecting.");
			}

            client.JoinMulticastGroup(IPAddress.Parse(address));

			connected = true;
		}

		public void Disconnect()
		{
            client.Close();

			connected = false;
		}

		public SSL_WrapperPacket Receive()
		{
			if (!connected)
				throw new ApplicationException("Trying to receive from vision SSLClient socket that wasn't open");

			byte[] buf = client.Receive(ref endPoint);

			MemoryStream stream = new MemoryStream(buf);
			SSL_WrapperPacket packet = ProtoBuf.Serializer.Deserialize<SSL_WrapperPacket>(stream);

			return packet;
		}
	}
}
