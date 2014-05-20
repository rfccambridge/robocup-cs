using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RFC.Messaging;
namespace RFC.Logging
{
    public class MatchRecorder
    {
        ServiceManager msngr;
        private static System.IO.StreamWriter logger;
        DateTime last;

        public MatchRecorder()
        {
            this.msngr = ServiceManager.getServiceManager();
            logger = new System.IO.StreamWriter(@"match_history.txt", false);

        }

        public void Handle(FieldVisionMessage msg)
        {
            int period = (int)(DateTime.Now - last).TotalMilliseconds;
            double freq = 1000.0 / period; 
            logger.WriteLine(freq + ", " + msg.bio());
            last = DateTime.Now;
                
        }
    }
}
