using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace RFC.InterProcessMessaging
{
    class BasicMessageSender<T> : IMessageSender<T> where T : IByteSerializable<T>
    {
        readonly TcpClient client;
        readonly NetworkStream stream;
        private object post_lock = new object();
        volatile bool done = false;

        public BasicMessageSender(TcpClient client)
        {
            this.client = client;
            stream = client.GetStream();
        }
        public void Post(T t)
        {
            lock (post_lock)
            {
                if (!done)
                {
                    try
                    {
                        t.Serialize(stream);
                    }
                    catch (System.IO.IOException e)
                    {
                        SocketException se = e.InnerException as SocketException;
                        if (se == null)
                            throw e;
                        if (se.SocketErrorCode == SocketError.ConnectionReset
                             || se.SocketErrorCode == SocketError.ConnectionAborted)
                        {
                            done = true;
                            if (OnDone != null)
                                OnDone.BeginInvoke(this, null, null);
                        }
                        else
                            throw e;
                    }
                }
            }
        }

        public void Close()
        {
            stream.Close();
            client.Close();
            client.Client.Close();
            Console.WriteLine("worker closed");
            GC.SuppressFinalize(this);
        }
        ~BasicMessageSender()
        {
            this.Close();
        }

        public delegate void DoneHandler(BasicMessageSender<T> doneItem);
        public event DoneHandler OnDone;
    }
    class BasicMessageReceiver<T> : IMessageReceiver<T> where T : IByteSerializable<T>, new()
    {
        public event ReceiveMessageDelegate<T> MessageReceived;
        readonly TcpClient client;
        readonly Thread thread;
        readonly NetworkStream stream;

        public BasicMessageReceiver(TcpClient client)
        {
            this.client = client;
            stream = client.GetStream();
            thread = new Thread(Run);
            thread.IsBackground = true;
        }
        public void Start()
        {
            thread.Start();
        }
        private void Run(object o)
        {
            Console.WriteLine("worker instantiated, listening for data...");

            while (true)
            {
                T obj = new T();
                try
                {
                    obj.Deserialize(stream);
                }
                catch (System.IO.IOException e)
                {
                    SocketException inner = e.InnerException as SocketException;
                    if (inner == null)
                        throw e;
                    if (inner.SocketErrorCode == SocketError.ConnectionReset ||
                        inner.SocketErrorCode == SocketError.Interrupted)
                    {
                        if (OnDone != null)
                            OnDone.BeginInvoke(this, null, null);
                        return;
                    }
                    throw e;
                }
                if (MessageReceived != null)
                    MessageReceived(obj);
            }
        }

        public void Close()
        {
            stream.Close();
            client.Close();
            client.Client.Close();
            Console.WriteLine("worker closed");
            GC.SuppressFinalize(this);
        }
        ~BasicMessageReceiver()
        {
            this.Close();
        }

        public delegate void DoneHandler(BasicMessageReceiver<T> doneItem);
        public event DoneHandler OnDone;
    }
}
