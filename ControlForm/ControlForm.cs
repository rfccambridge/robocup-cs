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
            int robotId = 0;
            int maxRobotId = 6;

            new MulticastRefBoxListener(team);
            new Vision();
            new AveragingPredictor(flip);
            new SerialSender(com);
            new SmoothRRTPlanner(true, maxRobotId);
            new VelocityDriver();
            new MovementTest(team, robotId);
        }

        private void RunButton_Click(object sender, EventArgs e)
        {
            Run();

            DisableControls();
        }

        private void DisableControls()
        {
            TeamBox.Enabled = false;
            ComNumberChooser.Enabled = false;
        }
    }
}
