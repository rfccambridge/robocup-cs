using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RFC.Messaging;
using System.IO;

namespace RFC.Logging
{
    public class LogHandler : IMessageHandler<Message>
    {
        private bool use_console = false;

        private object secretaryLock = new object();

        private static System.IO.StreamWriter logger;
        private static System.IO.StreamWriter secretary;

        public LogHandler()
        {
            try
            {
                logger = new System.IO.StreamWriter(@"log.txt", false);
                secretary = new System.IO.StreamWriter(@"message_logs.txt", false);

                ServiceManager mngr = ServiceManager.getServiceManager();
                mngr.RegisterListener<LogMessage>(this.LockingOn(new object()));

                // messages to be recorded
                //mngr.RegisterListener(this.Queued<VisionMessage>(new object()));
                //mngr.RegisterListener(this.Queued<RobotVisionMessage>(new object()));
                mngr.RegisterListener(this.Queued<CommandMessage>(new object()));
                mngr.RegisterListener(this.Queued<RobotDestinationMessage>(new object()));
                mngr.RegisterListener(this.Queued<RobotPathMessage>(new object()));
                mngr.RegisterListener(this.Queued<RefboxStateMessage>(new object()));
                //mngr.RegisterListener(this.Queued<BallMarkMessage>(new object()));
                //mngr.RegisterListener(this.Queued<BallMovedMessage>(new object()));
                //mngr.RegisterListener(this.Queued<BallVisionMessage>(new object()));
                mngr.RegisterListener(this.Queued<KickMessage>(new object()));
                mngr.RegisterListener(this.Queued<FieldVisionMessage>(new object()));
            }
            catch (IOException ex)
            {
                Console.WriteLine("Exception: " + ex);
            }
        }

        public void writeLog(LogMessage LM)
        {
            // append message to log file
            logger.WriteLine(LM.getMessage());
            if (use_console)   
                Console.WriteLine(LM.getMessage());
        }

        public void HandleMessage(Message m)
        {
            lock (secretaryLock)
            {
                secretary.WriteLine(DateTime.Now.Millisecond + ": " + m.bio());
            }
        }
    }
}
