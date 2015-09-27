using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Reflection;

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
    class BasicMessageReceiver<T> : IMessageReceiver<T> where T : IByteSerializable<T>
    {
        public event ReceiveMessageDelegate<T> MessageReceived;
        readonly TcpClient client;
        readonly Thread thread;
        readonly NetworkStream stream;

        private MethodInfo deserializer;

        public BasicMessageReceiver(TcpClient client)
        {
            this.client = client;
            stream = client.GetStream();
            thread = new Thread(Run);
            thread.IsBackground = true;

            deserializer = typeof(T).GetMethod(
                name: "Deserialize",
                binder: null,
                bindingAttr: BindingFlags.Public | BindingFlags.Static,
                callConvention: CallingConventions.Any,
                types: new Type[] { },
                modifiers: null
            );

            if(deserializer == null) {
                throw new System.MissingMethodException(typeof(T).Name, "Deserialize")
            }
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
                T obj;
                try
                {
                    obj = (T)typeof(T).GetMethod(
                        name: "Deserialize",
                        types: new Type[] {}
                    ).Invoke(null, new object[] { });
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
            thread.Abort();
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
