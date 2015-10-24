using RFC.Commands;
using RFC.Core;
using RFC.InterProcessMessaging;
using RFC.Logging;
using RFC.Messaging;
using RFC.PathPlanning;
using RFC.RefBox;
using RFC.Strategy;
using RFC.FieldDrawer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using RFC.Vision;

namespace RFC.WpfControlForm
{
    public partial class MainWindow
    {
        FieldDrawer.FieldDrawer fd = null;

        public bool Run()
        {
            if (Running) return false;
            
            int maxRobotId = 12;

            if (SelectedConnection == "Simulator")
            {
                try
                {
                    new SimulatorSender();
                }
                catch (ConnectionRefusedException)
                {
                    MessageBox.Show("Is the simulator running?", "Could not connect");
                    return false;
                }
            }
            else
            {
                try
                {
                    new SerialSender(SelectedConnection);
                }
                catch (IOException)
                {
                    MessageBox.Show("Could not open given serial port.", "Could not connect");
                    return false;
                }
            }

            if (fd == null)
            {
                Vision.Vision vision = new Vision.Vision();

                vision.Connect("224.5.23.2", 10002);
                vision.Start();

                new AveragingPredictor();

                fd = new FieldDrawer.FieldDrawer();
                fd.Show();
            }

            new LogHandler();

            new SmoothRRTPlanner(true, maxRobotId, GoalieId);
            new VelocityDriver();
            new KickPlanner();
            new BreakBeamEmulator(SelectedTeam);
            //new DribblerControler(team);


            new NormalTester(SelectedTeam, GoalieId, fd);
            //new AlexTest(team, goalieNumber);
            //new MovementTest(team);
            //new OffenseTester(team, goalieNumber);
            //new SetupTest(team, goalieNumber);
            //new GoalieTest(team, goalieNumber);
            //new SetupTest1(team, goalieNumber);
            //new PlaySwitcher(team, goalieNumber, fd);
            //new SetupTest(team, goalieNumber);
            //new KickTester(team, goalieNumber);
            //new TimeoutBehavior(team, goalieNumber);
            //new PenaltyKickTester(team, goalieNumber);
            //new KickOffBehaviorTester(team, goalieNumber);

            MulticastRefBoxListener refbox = new MulticastRefBoxListener(SelectedTeam);

            refbox.Connect("224.5.23.1", 10001);
            refbox.Start();
            ServiceManager.getServiceManager().SendMessage(new LogMessage("started"));

            return true;
        }

        private void runButton_Click(object sender, RoutedEventArgs e)
        {
            Running = Run();
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            ServiceManager.getServiceManager().SendMessage(new StopMessage());
            Running = false;
        }
    }

}
