using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RFC.Messaging;

namespace RFC.Logging
{
    public class LogHandler
    {
        public LogHandler()
        {
            ServiceManager.getServiceManager().RegisterListener<LogMessage>(writeLog, new object());
        }

        public void writeLog(LogMessage LM)
        {
            // append message to log file
            System.IO.StreamWriter writer = new System.IO.StreamWriter(@"C:\log.txt", true);
            writer.WriteLine("\n" + LM.getMessage());
        }
    }
}
