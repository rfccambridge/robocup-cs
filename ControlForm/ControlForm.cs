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

namespace ControlForm
{
    public partial class ControlForm : Form
    {
        bool running = false;

        AveragingPredictor predictor;
        FieldDrawer fd;

        public ControlForm()
        {
            InitializeComponent();

            TeamBox.DataSource = Enum.GetValues(typeof(Team));

            StartVision();
        }

        public void StartVision()
        {
            Vision vision = new Vision();

            vision.Connect("224.5.23.2", 10002);
            vision.Start();

            predictor = new AveragingPredictor();

            fd = new FieldDrawer();
            fd.Show();
        }

        public void Run()
        {
            if (!running)
            {
                running = true;
                int com = (int)ComNumberChooser.Value;
                Team team = Team.Yellow;
                Enum.TryParse<Team>(TeamBox.SelectedValue.ToString(), out team);
                bool flip = flippedCheckBox.Checked;
                int maxRobotId = 12;
                bool simulator = simulatorCheckBox.Checked;
                int goalieNumber = (int)GoalieNumberChooser.Value;

                new LogHandler();
                
                new SmoothRRTPlanner(true, maxRobotId);
                new VelocityDriver();

                //new AlexTest(team, goalieNumber);
                //new MovementTest(team);
                //new OffTester(team, goalieNumber);
                new SetupTest(team, goalieNumber);
                //new GoalieTest(team, goalieNumber);
                //new SetupTest1(team, goalieNumber);
                new KickPlanner();
                MulticastRefBoxListener refbox = new MulticastRefBoxListener(team);

                if (simulator)
                {
                    new SimulatorSender();
                }
                else
                {
                    new SerialSender(com);
                }

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
            flippedCheckBox.Enabled = false;
            simulatorCheckBox.Enabled = false;
        }

        private void ControlForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void flippedCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            predictor.Flipped = flippedCheckBox.Checked;
        }

        private void simulatorCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ComNumberChooser.Enabled = !simulatorCheckBox.Checked;
        }
    }
}