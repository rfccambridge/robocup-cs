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
            //loghandler listens for log messages via servicemanager (registerlistener)
            ServiceManager.getServiceManager().RegisterListener(todo);
        }

        public void todo(LogMessage LM)
        {
            // append message to log file
            System.IO.StreamWriter writer = new System.IO.StreamWriter(@"C:\log.txt",true);
            writer.WriteLine(LM);
        }
    }
}
