namespace RFC.FieldDrawer
{
    partial class FieldDrawerForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.glField = new OpenTK.GLControl(new OpenTK.Graphics.GraphicsMode(32, 24, 0, 8));
            this.panGameStatus = new System.Windows.Forms.Panel();
            this.lblPlayName = new System.Windows.Forms.Label();
            this.lblMarker = new System.Windows.Forms.Label();
            this.lblRefBoxCmd = new System.Windows.Forms.Label();
            this.lblPlayType = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lblTeam = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.panGameStatus.SuspendLayout();
            this.SuspendLayout();
            // 
            // glField
            // 
            this.glField.AllowDrop = true;
            this.glField.BackColor = System.Drawing.Color.Black;
            this.glField.Dock = System.Windows.Forms.DockStyle.Top;
            this.glField.Location = new System.Drawing.Point(0, 0);
            this.glField.Name = "glField";
            this.glField.Size = new System.Drawing.Size(599, 384);
            this.glField.TabIndex = 0;
            this.glField.VSync = false;
            this.glField.Load += new System.EventHandler(this.glField_Load);
            this.glField.DragDrop += new System.Windows.Forms.DragEventHandler(this.glField_DragDrop);
            this.glField.DragEnter += new System.Windows.Forms.DragEventHandler(this.glField_DragEnter);
            this.glField.Paint += new System.Windows.Forms.PaintEventHandler(this.glField_Paint);
            this.glField.MouseDown += new System.Windows.Forms.MouseEventHandler(this.glField_MouseDown);
            this.glField.MouseMove += new System.Windows.Forms.MouseEventHandler(this.glField_MouseMove);
            this.glField.MouseUp += new System.Windows.Forms.MouseEventHandler(this.glField_MouseUp);
            this.glField.Resize += new System.EventHandler(this.glField_Resize);
            // 
            // panGameStatus
            // 
            this.panGameStatus.BackColor = System.Drawing.Color.Green;
            this.panGameStatus.Controls.Add(this.lblPlayName);
            this.panGameStatus.Controls.Add(this.lblMarker);
            this.panGameStatus.Controls.Add(this.lblRefBoxCmd);
            this.panGameStatus.Controls.Add(this.lblPlayType);
            this.panGameStatus.Controls.Add(this.label3);
            this.panGameStatus.Controls.Add(this.label1);
            this.panGameStatus.Controls.Add(this.lblTeam);
            this.panGameStatus.Controls.Add(this.label5);
            this.panGameStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panGameStatus.Location = new System.Drawing.Point(0, 390);
            this.panGameStatus.Name = "panGameStatus";
            this.panGameStatus.Size = new System.Drawing.Size(599, 140);
            this.panGameStatus.TabIndex = 8;
            // 
            // lblPlayName
            // 
            this.lblPlayName.AutoSize = true;
            this.lblPlayName.Location = new System.Drawing.Point(406, 92);
            this.lblPlayName.Name = "lblPlayName";
            this.lblPlayName.Size = new System.Drawing.Size(25, 13);
            this.lblPlayName.TabIndex = 17;
            this.lblPlayName.Text = "<?>";
            this.lblPlayName.Visible = false;
            // 
            // lblMarker
            // 
            this.lblMarker.BackColor = System.Drawing.Color.White;
            this.lblMarker.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMarker.Location = new System.Drawing.Point(403, 11);
            this.lblMarker.Name = "lblMarker";
            this.lblMarker.Size = new System.Drawing.Size(15, 15);
            this.lblMarker.TabIndex = 12;
            this.lblMarker.Visible = false;
            this.lblMarker.Click += new System.EventHandler(this.lblMarker_Click);
            this.lblMarker.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lblMarker_MouseDown);
            // 
            // lblRefBoxCmd
            // 
            this.lblRefBoxCmd.AutoSize = true;
            this.lblRefBoxCmd.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRefBoxCmd.Location = new System.Drawing.Point(84, 11);
            this.lblRefBoxCmd.Name = "lblRefBoxCmd";
            this.lblRefBoxCmd.Size = new System.Drawing.Size(32, 16);
            this.lblRefBoxCmd.TabIndex = 4;
            this.lblRefBoxCmd.Text = "<?>";
            this.lblRefBoxCmd.Visible = false;
            // 
            // lblPlayType
            // 
            this.lblPlayType.AutoSize = true;
            this.lblPlayType.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPlayType.Location = new System.Drawing.Point(84, 27);
            this.lblPlayType.Name = "lblPlayType";
            this.lblPlayType.Size = new System.Drawing.Size(32, 16);
            this.lblPlayType.TabIndex = 6;
            this.lblPlayType.Text = "<?>";
            this.lblPlayType.Visible = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(403, 72);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(37, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Team:";
            this.label3.Visible = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(12, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(66, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "RefBoxCmd:";
            this.label1.Visible = false;
            // 
            // lblTeam
            // 
            this.lblTeam.AutoSize = true;
            this.lblTeam.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTeam.Location = new System.Drawing.Point(446, 72);
            this.lblTeam.Name = "lblTeam";
            this.lblTeam.Size = new System.Drawing.Size(32, 16);
            this.lblTeam.TabIndex = 2;
            this.lblTeam.Text = "<?>";
            this.lblTeam.Visible = false;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.ForeColor = System.Drawing.Color.White;
            this.label5.Location = new System.Drawing.Point(24, 28);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(54, 13);
            this.label5.TabIndex = 5;
            this.label5.Text = "PlayType:";
            this.label5.Visible = false;
            // 
            // FieldDrawerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Green;
            this.ClientSize = new System.Drawing.Size(599, 530);
            this.Controls.Add(this.panGameStatus);
            this.Controls.Add(this.glField);
            this.ForeColor = System.Drawing.Color.White;
            this.Name = "FieldDrawerForm";
            this.Text = "FieldDrawer";
            this.Resize += new System.EventHandler(this.FieldDrawerForm_Resize);
            this.panGameStatus.ResumeLayout(false);
            this.panGameStatus.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private OpenTK.GLControl glField;
        private System.Windows.Forms.Panel panGameStatus;
        private System.Windows.Forms.Label lblRefBoxCmd;
        private System.Windows.Forms.Label lblPlayType;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblTeam;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblMarker;
        private System.Windows.Forms.Label lblPlayName;
    }
}