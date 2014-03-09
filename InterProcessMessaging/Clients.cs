using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;

namespace RFC.InterProcessMessaging
{
    public class ClientMessageSender<T> : IMessageSender<T> where T : IByteSerializable<T>
    {
        readonly BasicMessageSender<T> sender;
        public ClientMessageSender(string hostname, int portNum)
        {
            try
            {
                TcpClient client = new TcpClient(hostname, portNum);
                sender = new BasicMessageSender<T>(client);
            }
            catch (System.Net.Sockets.SocketException e)
            {
                if (e.SocketErrorCode == System.Net.Sockets.SocketError.ConnectionRefused)
                {
                    throw new ConnectionRefusedException("the connection was refused", e);
                }
                else
                    throw e;
            }
        }
        public void Post(T t)
        {
            sender.Post(t);
        }

        public void Close()
        {
            if (sender != null)
                sender.Close();
            GC.SuppressFinalize(this);
        }
        ~ClientMessageSender()
        {
            Console.WriteLine("~ClientMessageSender");
            this.Close();
        }
    }
    class ClientMessageReceiver<T> : IMessageReceiver<T> where T : IByteSerializable<T>, new()
    {
        readonly BasicMessageReceiver<T> receiver;
        public ClientMessageReceiver(string hostname, int portNum)
        {
            try
            {
                TcpClient client = new TcpClient(hostname, portNum);
                receiver = new BasicMessageReceiver<T>(client);
                receiver.MessageReceived += OnMessageReceived;
                receiver.OnDone += delegate(BasicMessageReceiver<T> rec)
                {
                    this.Close();
                };
                receiver.Start();
            }
            catch (System.Net.Sockets.SocketException e)
            {
                if (e.SocketErrorCode == System.Net.Sockets.SocketError.ConnectionRefused)
                {
                    throw new ConnectionRefusedException("the connection was refused", e);
                }
                else
                    throw e;
            }
        }
        object receive_lock = new object();
        volatile bool receiving = false;
        List<T> buffer = new List<T>();
        private void OnMessageReceived(T t)
        {
            //The receiver will put incoming messages in a buffer, so that it will never call
            //the listeners concurrently
            lock (receive_lock)
            {

                //If we are currently processing receive requests, put this one into the buffer
                if (receiving)
                {
                    buffer.Add(t);
                    Console.WriteLine(buffer.Count + " elements in the buffer");
                    return;
                }
                receiving = true;
            }
            MessageReceived(t);
            bool processbuffer = true;
            while (processbuffer)
            {
                T next = default(T);
                //TODO this could probably be sped up by making a copy of the entire buffer,
                //to avoid having to acquired the lock on every iteration.
                lock (receive_lock)
                {
                    if (buffer.Count > 0)
                    {
                        processbuffer = true;
                        next = buffer[0];
                        buffer.RemoveAt(0);
                        Console.WriteLine(buffer.Count + " elements in the buffer");
                    }
                    else
                    {
                        processbuffer = false;
                        receiving = false;
                    }
                }
                if (processbuffer)
                    MessageReceived(next);
            }
        }

        public void Close()
        {
            if (receiver != null)
                receiver.Close();
            GC.SuppressFinalize(this);
        }
        ~ClientMessageReceiver()
        {
            Console.WriteLine("~ClientMessageReceiver");
            this.Close();
        }

        public event ReceiveMessageDelegate<T> MessageReceived;
    }
}
