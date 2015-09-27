﻿using System;
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

                ServiceManager.getServiceManager().RegisterListener<LogMessage>(writeLog, new object());

                // messages to be recorded
                //new QueuedMessageHandler<VisionMessage>(recordMessage, new object());
                //new QueuedMessageHandler<RobotVisionMessage>(recordMessage, new object());
                new QueuedMessageHandler<CommandMessage>(HandleMessage, new object());
                new QueuedMessageHandler<RobotDestinationMessage>(HandleMessage, new object());
                new QueuedMessageHandler<RobotPathMessage>(HandleMessage, new object());
                new QueuedMessageHandler<RefboxStateMessage>(HandleMessage, new object());
                //new QueuedMessageHandler<BallMarkMessage>(recordMessage, new object());
                //new QueuedMessageHandler<BallMovedMessage>(recordMessage, new object());
                //new QueuedMessageHandler<BallVisionMessage>(recordMessage, new object());
                new QueuedMessageHandler<KickMessage>(HandleMessage, new object());
                new QueuedMessageHandler<FieldVisionMessage>(HandleMessage, new object());
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
