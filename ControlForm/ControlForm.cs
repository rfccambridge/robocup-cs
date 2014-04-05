using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RFC.RefBox;
using RFC.PathPlanning;
using RFC.Vision;
using RFC.Strategy;
using RFC.Core;
using RFC.Commands;
using RFC.Logging;
using RFC.Messaging;
using RFC.Simulator;
using RFC.FieldDrawer;
using Strategy;

namespace ControlForm
{
    public partial class ControlForm : Form
    {
        bool running = false;

        public ControlForm()
        {
            InitializeComponent();

            TeamBox.DataSource = Enum.GetValues(typeof(Team));
        }

        public void Run()
        {
            if (!running)
            {
                running = true;
                int com = (int)ComNumberChooser.Value;
                Team team = Team.Yellow;
                Enum.TryParse<Team>(TeamBox.SelectedValue.ToString(), out team);
                bool flip = false;
                int maxRobotId = 12;
                bool simulator = true;

                //new LogHandler();
                Vision vision = new Vision();
                new AveragingPredictor(flip);
                new SmoothRRTPlanner(true, maxRobotId);
                new VelocityDriver();

                // new MovementTest(team);
                new OffTester(team);
                new KickPlanner();
                MulticastRefBoxListener refbox = new MulticastRefBoxListener(team);
                new FieldDrawer().Show();

                if (simulator)
                {
                    new SimulatorSender();
                }
                else
                {
                    new SerialSender(com);
                }

                vision.Connect("224.5.23.2", 10002);
                vision.Start();

                refbox.Connect("224.5.92.12", 10100);
                refbox.Start();
            }
        }

        private void RunButton_Click(object sender, EventArgs e)
        {
            Run();

            DisableControls();

            ServiceManager.getServiceManager().SendMessage(new LogMessage("started"));
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            ServiceManager.getServiceManager().SendMessage(new StopMessage());
        }


        private void DisableControls()
        {
            TeamBox.Enabled = false;
            ComNumberChooser.Enabled = false;
        }
    }
}
