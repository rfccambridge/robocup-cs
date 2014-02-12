namespace Robocup.Utilities
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
            this.glField = new OpenTK.GLControl();
            this.panGameStatus = new System.Windows.Forms.Panel();
            this.lblPlayName = new System.Windows.Forms.Label();
            this.lblControllerDuration = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.lblLapDuration = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.lblMarker = new System.Windows.Forms.Label();
            this.lblInterpretDuration = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.lblInterpretFreq = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
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
            this.panGameStatus.Controls.Add(this.lblControllerDuration);
            this.panGameStatus.Controls.Add(this.label8);
            this.panGameStatus.Controls.Add(this.lblLapDuration);
            this.panGameStatus.Controls.Add(this.label4);
            this.panGameStatus.Controls.Add(this.lblMarker);
            this.panGameStatus.Controls.Add(this.lblInterpretDuration);
            this.panGameStatus.Controls.Add(this.label6);
            this.panGameStatus.Controls.Add(this.lblInterpretFreq);
            this.panGameStatus.Controls.Add(this.label2);
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
            // 
            // lblControllerDuration
            // 
            this.lblControllerDuration.AutoSize = true;
            this.lblControllerDuration.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblControllerDuration.Location = new System.Drawing.Point(119, 92);
            this.lblControllerDuration.Name = "lblControllerDuration";
            this.lblControllerDuration.Size = new System.Drawing.Size(32, 16);
            this.lblControllerDuration.TabIndex = 16;
            this.lblControllerDuration.Text = "<?>";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(12, 92);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(97, 13);
            this.label8.TabIndex = 15;
            this.label8.Text = "Controller Duration:";
            // 
            // lblLapDuration
            // 
            this.lblLapDuration.AutoSize = true;
            this.lblLapDuration.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLapDuration.Location = new System.Drawing.Point(119, 114);
            this.lblLapDuration.Name = "lblLapDuration";
            this.lblLapDuration.Size = new System.Drawing.Size(32, 16);
            this.lblLapDuration.TabIndex = 14;
            this.lblLapDuration.Text = "<?>";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(42, 115);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(71, 13);
            this.label4.TabIndex = 13;
            this.label4.Text = "Lap Duration:";
            // 
            // lblMarker
            // 
            this.lblMarker.BackColor = System.Drawing.Color.White;
            this.lblMarker.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMarker.Location = new System.Drawing.Point(403, 11);
            this.lblMarker.Name = "lblMarker";
            this.lblMarker.Size = new System.Drawing.Size(15, 15);
            this.lblMarker.TabIndex = 12;
            this.lblMarker.Click += new System.EventHandler(this.lblMarker_Click);
            this.lblMarker.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lblMarker_MouseDown);
            // 
            // lblInterpretDuration
            // 
            this.lblInterpretDuration.AutoSize = true;
            this.lblInterpretDuration.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInterpretDuration.Location = new System.Drawing.Point(119, 72);
            this.lblInterpretDuration.Name = "lblInterpretDuration";
            this.lblInterpretDuration.Size = new System.Drawing.Size(32, 16);
            this.lblInterpretDuration.TabIndex = 10;
            this.lblInterpretDuration.Text = "<?>";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 72);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(101, 13);
            this.label6.TabIndex = 9;
            this.label6.Text = "Interpreter Duration:";
            // 
            // lblInterpretFreq
            // 
            this.lblInterpretFreq.AutoSize = true;
            this.lblInterpretFreq.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInterpretFreq.Location = new System.Drawing.Point(119, 50);
            this.lblInterpretFreq.Name = "lblInterpretFreq";
            this.lblInterpretFreq.Size = new System.Drawing.Size(32, 16);
            this.lblInterpretFreq.TabIndex = 8;
            this.lblInterpretFreq.Text = "<?>";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(31, 51);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Interpreter Freq:";
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
        private System.Windows.Forms.Label lblInterpretDuration;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lblInterpretFreq;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblMarker;
        private System.Windows.Forms.Label lblLapDuration;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblControllerDuration;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label lblPlayName;
    }
}