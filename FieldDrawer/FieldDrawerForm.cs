using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Windows.Forms;
using RFC.Core;
using System.Drawing;

namespace RFC.FieldDrawer
{
    public partial class FieldDrawerForm : Form
    {
        private delegate void VoidDelegate();

        private FieldDrawer _fieldDrawer;
        private Color[] _colors = { Color.Cyan, Color.Red, Color.White, Color.Pink, Color.Purple};
        private int _currentColorIdx = 0;

        public FieldDrawerForm(FieldDrawer fieldDrawer)
        {
            _fieldDrawer = fieldDrawer;
            _fieldDrawer.Dock = System.Windows.Forms.DockStyle.Top;
            _fieldDrawer.Location = new System.Drawing.Point(0, 0);
            _fieldDrawer.Name = "glField";
            _fieldDrawer.Size = new System.Drawing.Size(599, 384);
            _fieldDrawer.DragDrop += FieldDrawer_DragDrop;
            _fieldDrawer.DragEnter += FieldDrawer_DragEnter;
            _fieldDrawer.MouseDown += FieldDrawer_MouseDown;
            _fieldDrawer.MouseMove += FieldDrawer_MouseMove;
            _fieldDrawer.MouseUp += FieldDrawer_MouseUp;
            Controls.Add(_fieldDrawer);

            InitializeComponent();
            lblMarker.BackColor = _colors[_currentColorIdx];

            fieldDrawer.StateUpdated += FieldDrawer_StateUpdated;
            fieldDrawer.PropertyChanged += FieldDrawer_PropertyChanged;
        }

        private void FieldDrawer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(FieldDrawer.Team):
                    this.BeginInvoke(new VoidDelegate(delegate
                    {
                        lblTeam.Text = _fieldDrawer.Team.ToString();
                        lblTeam.ForeColor = _fieldDrawer.Team == Team.Yellow ? Color.Yellow : Color.Blue;
                        label3.Visible = true;
                        lblTeam.Visible = true;
                    }));
                    break;

                case nameof(FieldDrawer.PlayType):
                    this.BeginInvoke(new VoidDelegate(delegate
                    {
                        lblPlayType.Text = _fieldDrawer.PlayType.ToString();
                        lblPlayType.Visible = true;
                    }));
                    break;

                case nameof(FieldDrawer.PlayName):
                    this.BeginInvoke(new VoidDelegate(delegate
                    {
                        lblPlayName.Text = _fieldDrawer.PlayName;
                        lblPlayName.Visible = true;
                    }));
                    break;

                case nameof(FieldDrawer.RefBoxCmd):
                    this.BeginInvoke(new VoidDelegate(delegate
                    {
                        lblRefBoxCmd.Text = _fieldDrawer.RefBoxCmd;
                        label1.Visible = true;
                        lblRefBoxCmd.Visible = true;
                    }));
                    break;
            }
        }

        private void FieldDrawer_StateUpdated(object sender, EventArgs e)
        {
            _fieldDrawer.Invalidate();
        }

        private void FieldDrawerForm_Resize(object sender, EventArgs e)
        {
            _fieldDrawer.Height = panGameStatus.Top;
        }

        private void FieldDrawer_MouseDown(object sender, MouseEventArgs e)
        {
            _fieldDrawer.HandleMouseDown(e.Location);
        }

        private void FieldDrawer_MouseUp(object sender, MouseEventArgs e)
        {
            _fieldDrawer.HandleMouseUp(e.Location);
        }

        private void FieldDrawer_MouseMove(object sender, MouseEventArgs e)
        {
            _fieldDrawer.HandleMouseMove(e.Location);
        }

        private void FieldDrawer_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Color)))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void FieldDrawer_DragDrop(object sender, DragEventArgs e)
        {
            // TODO: Add ability to set orientation (and id?)
            if (e.Data.GetDataPresent(typeof(Color)))
                _fieldDrawer.HandleDragDrop(new EventArgs<WaypointInfo>(new WaypointInfo(
                                        new RobotInfo(null, 0, Team.Yellow, -1), _colors[_currentColorIdx])),
                                        _fieldDrawer.PointToClient(new Point(e.X, e.Y)));
        }

        private void lblMarker_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                lblMarker.DoDragDrop(lblMarker.BackColor, DragDropEffects.All);
        }

        private void lblMarker_Click(object sender, EventArgs e)
        {
            _currentColorIdx = (_currentColorIdx + 1) % _colors.Length;
            lblMarker.BackColor = _colors[_currentColorIdx];
        }

    }
}