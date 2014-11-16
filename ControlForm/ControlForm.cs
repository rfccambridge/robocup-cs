﻿using System;
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
using System.IO;

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
            
            PlayBox.DataSource = Enum.GetValues(typeof(Play))

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

        SetupTest bounce;

        public bool Run()
        {
            if (!running)
            {
                int com = (int)ComNumberChooser.Value;
                Team team = Team.Yellow;
                Enum.TryParse<Team>(TeamBox.SelectedValue.ToString(), out team);
                bool flip = flippedCheckBox.Checked;
                int maxRobotId = 12;
                bool simulator = simulatorCheckBox.Checked;
                int goalieNumber = (int)GoalieNumberChooser.Value;
                string play = PlayBox.Text;

                if (simulator)
                {
                    new SimulatorSender();
                }
                else
                {
                    try
                    {
                        new SerialSender(com);
                    }
                    catch (IOException ex)
                    {
                        MessageBox.Show("Could not open given serial port.");

                        return false;
                    }
                }

                new LogHandler();

                new SmoothRRTPlanner(true, maxRobotId, goalieNumber);
                new VelocityDriver();
                new KickPlanner();
                new BreakBeamEmulator(team);
                //new DribblerControler(team);

                TesterFactory.newTester(play, team, goalieNumber);

                // Do not use code hidden below! You can select a play after running ControlForm.
                // If you want you can comment out the TesterFactory line above and then use plays below as before.
                // To create a new permanent tester, make a behavior class implementing the (blank) interface Tester,
                // add it to TesterFactory, and add it to the play list in ControlForm.cs[Design].
                //new NormalTester(team, goalieNumber);
                //new AlexTest(team, goalieNumber);
                //new MovementTest(team);
                //new OffenseTester(team, goalieNumber);
                //new SetupTest(team, goalieNumber);
                //new GoalieTest(team, goalieNumber);
                //new SetupTest1(team, goalieNumber);
                //new PlaySwitcher(team, goalieNumber);
                //new SetupTest(team, goalieNumber);
                //new KickTester(team, goalieNumber);
                //new TimeoutBehavior(team, goalieNumber);
                
                MulticastRefBoxListener refbox = new MulticastRefBoxListener(team);

                refbox.Connect("224.5.23.1", 10001);
                refbox.Start();

                running = true;
                return true;
            }
            return false;
        }

        private void RunButton_Click(object sender, EventArgs e)
        {
            bool success = Run();

            if (success)
            {
                DisableControls();

                ServiceManager.getServiceManager().SendMessage(new LogMessage("started"));
            }
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            EnableControls();
            ServiceManager.getServiceManager().SendMessage(new StopMessage());
        }

        private void EnableControls()
        {
            TeamBox.Enabled = true;
            PlayBox.Enabled = true;
            ComNumberChooser.Enabled = true;
            flippedCheckBox.Enabled = true;
            simulatorCheckBox.Enabled = true;
        }

        private void DisableControls()
        {
            TeamBox.Enabled = false;
            PlayBox.Enabled = false;
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
            ComNumberChooser.Enabled = simulatorCheckBox.Checked;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}