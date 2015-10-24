using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace RFC.Messaging
{
    /// <summary>
    /// Handles messages but throws away old messages so it doesn't back up
    /// Holds the last message for each "channel", which can be specified to be any property of the message
    /// One use is for queueing a type of message that goes to multiple robots
    /// If you specify a channel key of robot id, this will keep and process the most recent message for each robot
    /// </summary>
    /// <typeparam name="T">message type</typeparam>
    /// <typeparam name="U">channel key type</typeparam>
    public class MultiChannelQueuedMessageHandler<T, U> : IMessageHandler<T> where T : Message
    {
        Dictionary<U, T> lastMessages =  new Dictionary<U,T>();
        List<U> channelOrder = new List<U>(); // tells us what channels to process, this is used to make sure we are fairly processing each channel, otherwise, we might just keep running the handler for one channel
        int channelOrderCurrentIndex = 0;
        object lastMessageLock = new object(); // if using lastMessages, channelOrder, or channelOrderCurrentIndexx, lock lastMessageLock

        IMessageHandler<T> handler;
        object lockObject;
        int handling = 0;

        public delegate U Selector(T message);
        Selector keySelector;

        /// <summary>
        /// example call: new MultiChannelQueuedMessageHandler(messageTHandlerFunction, (message) => message.RobotID, lockObject);
        /// </summary>
        /// <param name="handler">The handler to call for processing messages</param>
        /// <param name="selector">A function from message to channel key, where the most recent message for each key will be processed</param>
        /// <param name="lockObject">This will be locked when calling handler</param>
        public MultiChannelQueuedMessageHandler(IMessageHandler<T> handler, Selector selector, object lockObject)
        {
            this.handler = handler;
            this.keySelector = selector;
            this.lockObject = lockObject;
        }

        private T nextQueuedMessageToHandle()
        {
            T message = null;
            lock (lastMessageLock)
            {
                if (channelOrder.Count == 0)
                    return null;

                int startedAtChannel = channelOrderCurrentIndex;
                do
                {
                    U channel = channelOrder[channelOrderCurrentIndex];
                    channelOrderCurrentIndex += 1;
                    channelOrderCurrentIndex %= channelOrder.Count;

                    if (lastMessages.ContainsKey(channel))
                    {
                        message = lastMessages[channel];
                        lastMessages.Remove(channel);
                    }

                } while (message == null && channelOrderCurrentIndex != startedAtChannel);
            }

            return message;
        }

        public void HandleMessage(T message)
        {
            U key = keySelector(message);
            if (Interlocked.Exchange(ref handling, 1) == 0)
            {
                lock (lastMessageLock)
                {
                    lastMessages.Remove(key); // clear queue because we are handling most recent message for key
                }

                // run handler
                lock (lockObject)
                {
                    handler.HandleMessage(message);
                }

                // after handling, handle queue if things were added while we were running
                T messageToHandle = nextQueuedMessageToHandle();
                while (messageToHandle != null)
                {
                    lock (lockObject)
                    {
                        handler.HandleMessage(messageToHandle);
                    }

                    lock (lastMessageLock)
                    {
                        messageToHandle = nextQueuedMessageToHandle();
                    }
                }

                handling = 0;
            }
            else
            {
                // queue
                lock (lastMessageLock)
                {
                    if (!lastMessages.ContainsKey(key))
                    {
                        lastMessages.Add(key, message);
                    }

                    if (!channelOrder.Contains(key))
                    {
                        channelOrder.Add(key);
                    }
                }
            }
        }
    }
}
