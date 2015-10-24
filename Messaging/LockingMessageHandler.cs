using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RFC.Messaging
{

    /// <summary>
    /// A handler that wraps an existing handler, but locks on the given object before invoking the original
    /// </summary>
    public class LockingMessageHandler<T> : IMessageHandler<T> where T : Message
    {
        private readonly object lockObj;
        private readonly IMessageHandler<T> inner;

        public LockingMessageHandler(IMessageHandler<T> inner, object lockObj)
        {
            this.inner = inner;
            this.lockObj = lockObj;
        }

        public void HandleMessage(T message)
        {
            lock(lockObj) inner.HandleMessage(message);
        }
    }
    
    public static partial class MessageHandlerExtensions
    {
        /// <summary>
        /// Returns a modified handler than locks on the given object before invoking the current handler
        /// </summary>
        public static LockingMessageHandler<T> LockingOn<T>(this IMessageHandler<T> handler, object obj) where T : Message
        {
            return new LockingMessageHandler<T>(handler, obj);
        }
    }
}
