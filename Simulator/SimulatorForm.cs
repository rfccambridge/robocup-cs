using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using RFC.Core;

namespace RFC.Simulator
{
    public partial class SimulatorForm : Form
    {
        private bool _formClosing = false;
        private bool _simRunning = false;
        private PhysicsEngine _physicsEngine = new PhysicsEngine();

        public SimulatorForm()
        {
            InitializeComponent();

            createScenarios();

            // Otherwise focus goes to the console window
            this.BringToFront();
        }

        private bool parseHost(string host, out string hostname, out int port)
        {
            string[] tokens = host.Split(new char[] { ':' });
            if (tokens.Length != 2 || !int.TryParse(tokens[1], out port) ||
                tokens[0].Length == 0 || tokens[1].Length == 0)
            {
                MessageBox.Show("Invalid format of host ('" + host + "'). It must be \"hostname:port\"");
                hostname = null;
                port = 0;
                return false;
            }
            hostname = tokens[0];
            return true;
        }

        private void createScenarios()
        {
            SimulatedScenario normal = new NormalGameScenario("Normal game", _physicsEngine);
            SimulatedScenario shootout = new ShootoutGameScenario("Shootout", _physicsEngine);

            lstScenarios.Items.Add(normal);
            lstScenarios.Items.Add(shootout);

            lstScenarios.SelectedIndex = 0;
        }

        private void btnSimStartStop_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_simRunning)
                {

                    string visionIp, refBoxIp;
                    int visionPort, refBoxPort;
                    int cmdPort = int.Parse(txtSimCmdPort.Text);
                    if (!parseHost(txtSimVisionHost.Text, out visionIp, out visionPort)) return;
                    if (!parseHost(txtSimRefereeHost.Text, out refBoxIp, out refBoxPort)) return;

					int numBlue = int.Parse(txtNumBlue.Text);
                    if(txtNumBlue.Enabled && (numBlue <= 0 || numBlue > 6))
                    {
                        MessageBox.Show("Number of blue robots must be between 1 and 6");
                        return;
                    }
					int numYellow = int.Parse(txtNumYellow.Text);
                    if (txtNumYellow.Enabled && (numYellow <= 0 || numYellow > 6))
                    {
                        MessageBox.Show("Number of yellow robots must be between 1 and 6");
                        return;
                    }
                    // For convenience reload constants on every restart
                    ConstantsRaw.Load();
                    _physicsEngine.LoadConstants();

                    _physicsEngine.SetScenario(lstScenarios.SelectedItem as SimulatedScenario);

                    try
                    {
                        _physicsEngine.StartCommander(cmdPort);
                        _physicsEngine.StartVision(visionIp, visionPort);

                        if (chkReferee.Checked)
                            _physicsEngine.StartReferee(refBoxIp, refBoxPort);
                    }
                    catch (System.Net.Sockets.SocketException sock_exc)
                    {
                        MessageBox.Show(sock_exc.ToString());
                        return;
                    }


                    _physicsEngine.Start(numYellow, numBlue);

                    _simRunning = true;
                    btnSimStartStop.Text = "Stop Sim";
                    lblSimListenStatus.BackColor = Color.Green;
                    chkReferee.Enabled = false;
                    lstScenarios.Enabled = false;
                }
                else
                {
                    _physicsEngine.Stop();
                    
                    if(chkReferee.Checked)
                        _physicsEngine.StopReferee();
                    
                    _physicsEngine.StopVision();
                    _physicsEngine.StopCommander();

                    _simRunning = false;
                    btnSimStartStop.Text = "Start Sim";
                    lblSimListenStatus.BackColor = Color.Red;
                    chkReferee.Enabled = true;
                    lstScenarios.Enabled = true;
                }
            }
            catch (ApplicationException except)
            {
                MessageBox.Show(except.Message + "\r\n" + except.StackTrace);
                return;
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            _physicsEngine.ResetScenarioScene();
        }

        private void SimulatorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_formClosing) return;

            // Cleanup before exiting
            if (_simRunning)
                btnSimStartStop_Click(null, null);

            // Need to give some time for the threads that the above calls spawn to 
            // complete, *without stalling this thread*, hence this bizzare code here
            _formClosing = true;
            e.Cancel = true;

            Thread shutdownThread = new Thread(delegate(object state)
            {
                Thread.Sleep(300);
                Application.Exit();
            });
            shutdownThread.Start();
        }

        private void lstScenarios_SelectedIndexChanged(object sender, EventArgs e)
        {
            SimulatedScenario selected = lstScenarios.SelectedItem as SimulatedScenario;

            txtNumBlue.Enabled = selected.SupportsNumbers;
            txtNumYellow.Enabled = selected.SupportsNumbers;
        }


        private void noisyVisionBox_CheckStateChanged(object sender, EventArgs e)
        {
            _physicsEngine.SetNoisyVision(noisyVisionBox.Checked);            
        }
    }
}