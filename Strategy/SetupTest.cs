using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RFC.Messaging;
using RFC.Core;
using RFC.Geometry;

namespace RFC.Strategy
{
    public class SetupTest
    {
        Team team;
        bool stopped = false;
        KickOffBehavior testing;
        ServiceManager msngr;

        public SetupTest(Team team)
        {
            this.team = team;
            testing = new KickOffBehavior(team);
            object lockObject = new object();
            new QueuedMessageHandler<FieldVisionMessage>(Handle, lockObject);
            msngr = ServiceManager.getServiceManager();
            msngr.RegisterListener<StopMessage>(stopMessageHandler, lockObject);

            // static debug
            
            List<RobotInfo> robs = new List<RobotInfo>();
            List<RobotInfo> dests = new List<RobotInfo>();
            robs.Add(new RobotInfo(new Vector2(0, 0), 0, 0));
            robs.Add(new RobotInfo(new Vector2(0, 1), 0, 2));
            robs.Add(new RobotInfo(new Vector2(0, 2), 0, 1));

            dests.Add(new RobotInfo(new Vector2(0, 1), 0, 0));
            dests.Add(new RobotInfo(new Vector2(0, 2), 0, 0));
            dests.Add(new RobotInfo(new Vector2(0, 3), 0, 0));

            int[,] mat = DestinationMatcher.constructDistanceMatrix(robs, dests);

            var rowCount = mat.GetLength(0);
            var colCount = mat.GetLength(1);
            for (int row = 0; row < rowCount; row++)
            {
                for (int col = 0; col < colCount; col++)
                    Console.Write(String.Format("{0}\t", mat[row, col]));
                Console.WriteLine();
            }

            int[] assignments = DestinationMatcher.GetAssignments(robs, dests);

            foreach (int i in assignments)
            {
                Console.Write(i + " ");
            }
            Console.WriteLine("");
            
        }

        public void Handle(FieldVisionMessage fieldVision)
        {
            if (!stopped && fieldVision.GetRobots().Count > 0)
            {
                msngr.db("handling field vision in SetupTest");
                testing.OursSetup(fieldVision);
            }
        }

        public void stopMessageHandler(StopMessage message)
        {
            stopped = true;
        }

    }
}
