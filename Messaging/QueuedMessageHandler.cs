using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace RFC.Messaging
{
    public class QueuedMessageHandler<T> : IMessageHandler<T> where T : Message
    {
        T lastMessage;
        object lastMessageLock = new object();

        IMessageHandler<T> handler;
        object lockObject;
        int handling = 0;

        public QueuedMessageHandler(IMessageHandler<T> handler, object lockObject)
        {
            this.handler = handler;
            this.lockObject = lockObject;
        }

        public void HandleMessage(T message)
        {
            if (Interlocked.Exchange(ref handling, 1) == 0)
            {
                lock (lastMessageLock)
                {
                    lastMessage = null; // clear queue because we are handling most recent message
                }

                // run handler
                lock (lockObject)
                {
                    handler.HandleMessage(message);
                }

                // after handling, handle queue if things were added while we were running
                T messageToHandle;
                lock (lastMessageLock)
                {
                    messageToHandle = lastMessage;
                    lastMessage = null;
                }
                while (messageToHandle != null)
                {
                    lock (lockObject)
                    {
                        handler.HandleMessage(messageToHandle);
                    }

                    lock (lastMessageLock)
                    {
                        messageToHandle = lastMessage;
                        lastMessage = null;
                    }
                }

                handling = 0;
            }
            else
            {
                // queue
                lock (lastMessageLock)
                {
                    lastMessage = message;
                }
            }
        }
    }

    public static partial class MessageHandlerExtensions
    {
        /// <summary>
        /// Returns a modified handler than queues up message, locking on the given object before invoking the current handler
        /// See <see cref="QueuedMessageHandler{T}"/> for more details
        /// </summary>
        public static QueuedMessageHandler<T> Queued<T>(this IMessageHandler<T> handler, object obj) where T : Message
        {
            return new QueuedMessageHandler<T>(handler, obj);
        }
    }
}
