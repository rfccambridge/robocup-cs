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
            try
            {
                logger = new System.IO.StreamWriter(@"match_history.txt", true);
            }
            catch (Exception e)
            {
                // if first log already being written to, write in second log
                // this happens when two ControlForms are being used
                // should probably not make rest of code dependent on there being a log...
                logger = new System.IO.StreamWriter(@"match_history2.txt", true);
                logger.WriteLine("Exception: " + e);
            }
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
