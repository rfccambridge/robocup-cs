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
        private bool _glFieldLoaded = false;
        private Color[] _colors = { Color.Cyan, Color.Red, Color.White, Color.Pink, Color.Purple};
        private int _currentColorIdx = 0;

        OpenTK.GLControl glField;

        public FieldDrawerForm(FieldDrawer fieldDrawer)
        {
            _fieldDrawer = fieldDrawer;
            InitializeComponent();
            InitGL();
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
            glField.Invalidate();
        }

        private void InitGL()
        {
            glField = new OpenTK.GLControl(new OpenTK.Graphics.GraphicsMode(32, 24, 0, 8)) {
                AllowDrop = true,
                BackColor = System.Drawing.Color.Black,
                Dock = System.Windows.Forms.DockStyle.Top,
                Location = new System.Drawing.Point(0, 0),
                Name = "glField",
                Size = new System.Drawing.Size(599, 384),
                TabIndex = 0,
                VSync = false
            };
            glField.Load += glField_Load;
            glField.Paint += glField_Paint;
            glField.Resize += glField_Resize;
            glField.DragDrop += glField_DragDrop;
            glField.DragEnter += glField_DragEnter;
            glField.MouseDown += glField_MouseDown;
            glField.MouseMove += glField_MouseMove;
            glField.MouseUp += glField_MouseUp;


            this.Controls.Add(this.glField);
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
            _fieldDrawer.Init();
            _fieldDrawer.Resize(glField.Width, glField.Height);
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
                                        new RobotInfo(null, 0, Team.Yellow, -1), _colors[_currentColorIdx])), 
                                        glField.PointToClient(new Point(e.X, e.Y)));
        }

        private void lblMarker_Click(object sender, EventArgs e)
        {
            _currentColorIdx = (_currentColorIdx + 1) % _colors.Length;
            lblMarker.BackColor = _colors[_currentColorIdx];
        }

    }
}