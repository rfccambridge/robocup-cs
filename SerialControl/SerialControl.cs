using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RFC.Utilities;
using RFC.Core;
using RFC.InterProcessMessaging;
using System.IO.Ports;
using System.IO;

namespace RFC.SerialControl
{
    public partial class SerialControl : Form
    {
        SerialPort port;
        Timer timer = new Timer();

        public SerialControl()
        {
            InitializeComponent();

            this.KeyPreview = true;
        }

        [Flags]
        enum KeyStates
        {
            Up = 1,
            Down = 2,
            Left = 4,
            Right = 8
        }
        KeyStates states;

        private void SerialControl_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Down:
                    states &= ~KeyStates.Down;
                    break;
                case Keys.Up:
                    states &= ~KeyStates.Up;
                    break;
                case Keys.Right:
                    states &= ~KeyStates.Right;
                    break;
                case Keys.Left:
                    states &= ~KeyStates.Left;
                    break;
            }
            textBox1.Text = states.ToString();
        }

        private void SerialControl_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Down:
                    states |= KeyStates.Down;
                    break;
                case Keys.Up:
                    states |= KeyStates.Up;
                    break;
                case Keys.Right:
                    states |= KeyStates.Right;
                    break;
                case Keys.Left:
                    states |= KeyStates.Left;
                    break;
            }
            textBox1.Text = states.ToString();
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            try
            {
                port = SerialPortManager.OpenSerialPort("COM" + COMSelector.Value);
                timer.Interval = 50;
                timer.Enabled = true;
                timer.Tick += timer_Tick;
            }
            catch (IOException ex)
            {
                MessageBox.Show("Could not open given serial port.");
            }
        }

        void timer_Tick(object s, EventArgs e)
        {
            int speed = (int)speedSelector.Value;
            WheelSpeeds speeds = new WheelSpeeds();
            if(states.HasFlag(KeyStates.Left)){
                speeds.lb += speed;
                speeds.rb -= speed;
                speeds.lf -= speed;
                speeds.rf += speed;
            }
            if (states.HasFlag(KeyStates.Right))
            {
                speeds.lb -= speed;
                speeds.rb += speed;
                speeds.lf += speed;
                speeds.rf -= speed;
            }
            if (states.HasFlag(KeyStates.Up))
            {
                speeds.lb += speed;
                speeds.rb += speed;
                speeds.lf += speed;
                speeds.rf += speed;
            }
            if (states.HasFlag(KeyStates.Down))
            {
                speeds.lb -= speed;
                speeds.rb -= speed;
                speeds.lf -= speed;
                speeds.rf -= speed;
            }
            byte[] packet = new RobotCommand((int)idSelector.Value, speeds).ToPacket();
            port.Write(packet, 0, packet.Length);
        }
    }
}
