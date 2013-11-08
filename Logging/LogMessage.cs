using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Messaging;

namespace Logging
{
    public class LogMessage : Message
    {
        string content;

        public LogMessage(string msgContent)
        {
            this.content = msgContent;
        }

        public string getMessage()
        {
            return content;
        }
    }
}