using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Windows.Forms;
using RFC.Core;
using System.Drawing;

namespace Robocup.Utilities
{
    partial class FieldDrawerForm : Form
    {
        private delegate void VoidDelegate();

        private FieldDrawer _fieldDrawer;
        private bool _glFieldLoaded = false;
        private Color[] _colors = { Color.Cyan, Color.Red, Color.White, Color.Pink, Color.Purple};
        private int _currentColorIdx = 0;

        public FieldDrawerForm(FieldDrawer fieldDrawer, double heightToWidth)
        {
            _fieldDrawer = fieldDrawer;
            InitializeComponent();

            this.Width = (int)((double)glField.Height / heightToWidth);
            lblMarker.BackColor = _colors[_currentColorIdx];
        }

        public void InvalidateGLControl()
        {
            glField.Invalidate();
        }

        public void UpdateTeam(Team team)
        {
            this.BeginInvoke(new VoidDelegate(delegate
            {
                lblTeam.Text = team.ToString();
                lblTeam.ForeColor = team == Team.Yellow ? Color.Yellow : Color.Blue;
            }));
        }

        public void UpdateRefBoxCmd(string refBoxCmd)
        {
            this.BeginInvoke(new VoidDelegate(delegate
            {
                lblRefBoxCmd.Text = refBoxCmd;
            }));
        }

        public void UpdatePlayType(PlayType playType)
        {
            this.BeginInvoke(new VoidDelegate(delegate
            {
                lblPlayType.Text = playType.ToString();
            }));
        }

        public void UpdatePlayName(string name)
        {
            this.BeginInvoke(new VoidDelegate(delegate
            {
                lblPlayName.Text = name;
            }));
        }

        public void UpdateInterpretFreq(double freq)
        {
            this.BeginInvoke(new VoidDelegate(delegate
            {
                lblInterpretFreq.Text = String.Format("{0:F2} Hz", freq);
            }));
        }

        public void UpdateInterpretDuration(double duration)
        {
            this.BeginInvoke(new VoidDelegate(delegate
            {
                lblInterpretDuration.Text = String.Format("{0:F2} ms", duration);
            }));
        }

        public void UpdateControllerDuration(double duration)
        {
            this.BeginInvoke(new VoidDelegate(delegate
            {
                lblControllerDuration.Text = String.Format("{0:F2} ms", duration);
            }));
        }

        public void UpdateLapDuration(double duration)
        {
            this.BeginInvoke(new VoidDelegate(delegate
            {
                lblLapDuration.Text = String.Format("{0:F2} s", duration);
            }));
        }

        private void FieldDrawerForm_Resize(object sender, EventArgs e)
        {
            glField.Height = panGameStatus.Top;
        }

        private void glField_Paint(object sender, PaintEventArgs e)
        {
            if (!_glFieldLoaded)
                return;
            glField.MakeCurrent();
            _fieldDrawer.Paint();            
            glField.SwapBuffers();
        }

        private void glField_Load(object sender, EventArgs e)
        {
            _glFieldLoaded = true;
            _fieldDrawer.Init(glField.Width, glField.Height);
        }

        private void glField_Resize(object sender, EventArgs e)
        {
            _fieldDrawer.Resize(glField.Width, glField.Height);
            glField.Invalidate();
        }

        private void glField_MouseDown(object sender, MouseEventArgs e)
        {
            _fieldDrawer.MouseDown(e.Location);
        }

        private void glField_MouseUp(object sender, MouseEventArgs e)
        {
            _fieldDrawer.MouseUp(e.Location);
        }

        private void glField_MouseMove(object sender, MouseEventArgs e)
        {
            _fieldDrawer.MouseMove(e.Location);
        }

        private void glField_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Color)))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void lblMarker_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                lblMarker.DoDragDrop(lblMarker.BackColor, DragDropEffects.All);
        }

        private void glField_DragDrop(object sender, DragEventArgs e)
        {
            // TODO: Add ability to set orientation (and id?)
            if (e.Data.GetDataPresent(typeof(Color)))
                _fieldDrawer.DragDrop(new EventArgs<WaypointInfo>(new WaypointInfo(
                                        new RobotInfo(null, 0, -1), _colors[_currentColorIdx])), 
                                        glField.PointToClient(new Point(e.X, e.Y)));
        }

        private void lblMarker_Click(object sender, EventArgs e)
        {
            _currentColorIdx = (_currentColorIdx + 1) % _colors.Length;
            lblMarker.BackColor = _colors[_currentColorIdx];
        }

    }
}