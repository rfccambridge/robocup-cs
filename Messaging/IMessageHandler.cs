using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RFC.Messaging
{
    public interface IMessageHandler<T> where T : Message
    {
        void HandleMessage(T message);
    }
}
