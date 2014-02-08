using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;

namespace RFC.InterProcessMessaging
{
    public interface IByteSerializable<T>
    {
        void Deserialize(NetworkStream stream);
        void Serialize(NetworkStream stream);
    }

    public interface IMessageSender<T>
    {
        void Post(T t);
        void Close();
    }

    public delegate void ReceiveMessageDelegate<T>(T t);
    public interface IMessageReceiver<T>
    {
        /// <summary>
        /// This event is called when a message is received.  Do not modify the parameter, as the same
        /// reference will be passed to all the observers (in an asynchronous manner).
        /// </summary>
        event ReceiveMessageDelegate<T> MessageReceived;
        void Close();
    }

    /// <summary>
    /// All of these return PERSISTENT things that will never go away!
    /// The sockets they open will stay open until it's closed from the other side.
    /// </summary>
    public static partial class Messages
    {
        public static IMessageSender<T> CreateServerSender<T>(int portNum)
            where T : IByteSerializable<T>
        {
            return new ServerMessageSender<T>(portNum);
        }
        /// <returns>Returns null if the connection was refused, most likely because there was no process running on the other side.</returns>
        public static IMessageReceiver<T> CreateClientReceiver<T>(string hostname, int portNum)
            where T : IByteSerializable<T>, new()
        {
            try
            {
                return new ClientMessageReceiver<T>(hostname, portNum);
            }
            catch (ConnectionRefusedException cre)
            {
                Console.WriteLine("Connection Refused: " + cre.ToString());
                return null;
            }
        }

        public static IMessageReceiver<T> CreateServerReceiver<T>(int portNum)
            where T : IByteSerializable<T>, new()
        {
            return new ServerMessageReceiver<T>(portNum);
        }
        /// <returns>Returns null if the connection was refused, most likely because there was no process running on the other side.</returns>
        public static IMessageSender<T> CreateClientSender<T>(string hostname, int portNum)
            where T : IByteSerializable<T>
        {
            try
            {
                return new ClientMessageSender<T>(hostname, portNum);
            }
            catch (ConnectionRefusedException)
            {
                return null;
            }
        }
    }
}