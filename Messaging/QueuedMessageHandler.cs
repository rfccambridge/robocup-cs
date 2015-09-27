using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace RFC.Messaging
{
    public class QueuedMessageHandler<T> where T : Message
    {
        T lastMessage;
        object lastMessageLock = new object();

        ServiceManager.Handler<T> handler;
        object lockObject;
        int handling = 0;

        public QueuedMessageHandler(ServiceManager.Handler<T> handler, object lockObject)
        {
            this.handler = handler;
            this.lockObject = lockObject;

            // register myself to receive messages
            ServiceManager.getServiceManager().RegisterListener<T>(handleMessage, null);
        }

        public QueuedMessageHandler(IMessageHandler<T> handler, object lockObject) : this(handler.HandleMessage, lockObject)
        {
            
        }


        public void handleMessage(T message)
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
                    handler(message);
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
                        handler(messageToHandle);
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
}
