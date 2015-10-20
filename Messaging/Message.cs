using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RFC.Messaging
{
    /// <summary>
    /// Subclass this to make a message that can be sent through service manager.
    /// Subclasses should be immutable so they are safe to pass around.
    /// </summary>
    public class Message
    {
        // print information about what this message is
        // messages can override to provide more useful / readable information
        public virtual string bio()
        {
            return this.ToString();
        }
    }
}
