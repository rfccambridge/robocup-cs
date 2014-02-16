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

namespace ControlForm
{
    public partial class ControlForm : Form
    {
        public ControlForm()
        {
            InitializeComponent();

            TeamBox.DataSource = Enum.GetValues(typeof(Team));
        }

        public void Run()
        {
            int com = (int)ComNumberChooser.Value;
            Team team = Team.Yellow;
            Enum.TryParse<Team>(TeamBox.SelectedValue.ToString(), out team);
            bool flip = false;
            int robotId = 5;
            int maxRobotId = 6;

            new LogHandler();
            new MulticastRefBoxListener(team);
            Vision vision = new Vision();
            new AveragingPredictor(flip);
            new SerialSender(com);
            new SmoothRRTPlanner(true, maxRobotId);
            new VelocityDriver();
            new MovementTest(team, robotId);
            MulticastRefBoxListener refbox = new MulticastRefBoxListener(team);

            vision.Connect("224.5.23.2", 10002);
            vision.Start();
            
            refbox.Connect("224.5.92.12", 10100);
            refbox.Start();
        }

        private void RunButton_Click(object sender, EventArgs e)
        {
            Run();

            DisableControls();

            ServiceManager.getServiceManager().SendMessage(new LogMessage("started"));
        }

        private void DisableControls()
        {
            TeamBox.Enabled = false;
            ComNumberChooser.Enabled = false;
        }
    }
}
