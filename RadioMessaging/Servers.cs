using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace RFC.RadioMessaging
{

    /// <summary>
    /// A server for listening for messages.
    /// </summary>
    /// <typeparam name="T">The type of message to receive.</typeparam>
    class ServerMessageReceiver<T> : IMessageReceiver<T> where T : IByteSerializable<T>, new()
    {
        public event ReceiveMessageDelegate<T> MessageReceived;
        readonly TcpListener listener;
        IAsyncResult beginAcceptResult;
        IList<BasicMessageReceiver<T>> receivers = new List<BasicMessageReceiver<T>>();
        public ServerMessageReceiver(int portNum)
        {
            Console.Write("creating new MessageListener...");

            listener = new TcpListener(System.Net.IPAddress.Any, portNum);
            listener.Start();

            Console.WriteLine("created!");

            Console.WriteLine("listening for connections...");
            beginAcceptResult = listener.BeginAcceptTcpClient(Run, null);
        }
        private void OnMessageReceived(T t)
        {
            if (MessageReceived != null)
                MessageReceived(t);
        }
        private void Run(object o)
        {
            TcpClient client;
            lock (listener)
            {
                if (beginAcceptResult == null)
                    return;
                client = listener.EndAcceptTcpClient(beginAcceptResult);
            }
            Console.WriteLine("connection received!");
            BasicMessageReceiver<T> receiver = new BasicMessageReceiver<T>(client);
            receiver.MessageReceived += OnMessageReceived;
            receiver.Start();
            receiver.OnDone += delegate(BasicMessageReceiver<T> doneItem)
            {
                Console.WriteLine("receiver done");
                doneItem.Close();
                receivers.Remove(doneItem);
            };
            receivers.Add(receiver);
            beginAcceptResult = listener.BeginAcceptTcpClient(Run, null);
            Console.WriteLine("listening for more connections...");
        }
        public void Close()
        {
            lock (listener)
            {
                listener.Stop();
                beginAcceptResult = null;
            }
            foreach (BasicMessageReceiver<T> receiver in receivers)
            {
                receiver.Close();
            }
            receivers.Clear();
            GC.SuppressFinalize(this);
        }
        ~ServerMessageReceiver()
        {
            this.Close();
        }
    }

    /// <summary>
    /// A server for broadcasting messages.
    /// </summary>
    /// <typeparam name="T">The type of message to send.</typeparam>
    class ServerMessageSender<T> : IMessageSender<T> where T : IByteSerializable<T>
    {
        readonly TcpListener listener;
        IAsyncResult beginAcceptResult;
        IList<BasicMessageSender<T>> senders = new List<BasicMessageSender<T>>();
        public ServerMessageSender(int portNum)
        {
            Console.Write("creating new MessageSender...");
            listener = new TcpListener(System.Net.IPAddress.Any, portNum);
            listener.Start();

            Console.WriteLine("created!");

            Console.WriteLine("listening for connections...");
            beginAcceptResult = listener.BeginAcceptTcpClient(Run, null);
        }
        private void Run(object o)
        {
            TcpClient client;
            lock (listener)
            {
                if (beginAcceptResult == null)
                    return;
                client = listener.EndAcceptTcpClient(beginAcceptResult);
            }
            Console.WriteLine("connection received!");
            BasicMessageSender<T> sender = new BasicMessageSender<T>(client);
            sender.OnDone += delegate(BasicMessageSender<T> doneItem)
            {
                doneItem.Close();
                lock (senders)
                {
                    senders.Remove(doneItem);
                }
            };
            lock (senders)
            {
                senders.Add(sender);
            }
            beginAcceptResult = listener.BeginAcceptTcpClient(Run, null);
            Console.WriteLine("listening for more connections...");
        }
        public void Close()
        {
            lock (listener)
            {
                listener.Stop();
                beginAcceptResult = null;
            }
            lock (senders)
            {
                foreach (BasicMessageSender<T> sender in senders)
                {
                    sender.Close();
                }
                senders.Clear();
            }
            GC.SuppressFinalize(this);
        }
        ~ServerMessageSender()
        {
            this.Close();
        }

        public void Post(T t)
        {
            lock (senders)
            {
                foreach (BasicMessageSender<T> sender in senders)
                {
                    sender.Post(t);
                }
            }
        }
    }
}