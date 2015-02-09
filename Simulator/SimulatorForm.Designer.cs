namespace RFC.Simulator
{
    partial class SimulatorForm
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
            this.txtSimRefereeHost = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtSimVisionHost = new System.Windows.Forms.TextBox();
            this.btnSimStartStop = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.lblSimListenStatus = new System.Windows.Forms.Label();
            this.txtSimCmdPort = new System.Windows.Forms.TextBox();
            this.btnReset = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtNumYellow = new System.Windows.Forms.TextBox();
            this.txtNumBlue = new System.Windows.Forms.TextBox();
            this.chkReferee = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.lstScenarios = new System.Windows.Forms.ListBox();
            this.noisyVisionBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // txtSimRefereeHost
            // 
            this.txtSimRefereeHost.Location = new System.Drawing.Point(23, 56);
            this.txtSimRefereeHost.Name = "txtSimRefereeHost";
            this.txtSimRefereeHost.Size = new System.Drawing.Size(117, 20);
            this.txtSimRefereeHost.TabIndex = 60;
            this.txtSimRefereeHost.Text = "224.5.92.12:10100";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(11, 79);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(76, 13);
            this.label5.TabIndex = 59;
            this.label5.Text = "Vision (ip:port):";
            // 
            // txtSimVisionHost
            // 
            this.txtSimVisionHost.Location = new System.Drawing.Point(23, 95);
            this.txtSimVisionHost.Name = "txtSimVisionHost";
            this.txtSimVisionHost.Size = new System.Drawing.Size(117, 20);
            this.txtSimVisionHost.TabIndex = 58;
            this.txtSimVisionHost.Text = "224.5.23.2:10002";
            // 
            // btnSimStartStop
            // 
            this.btnSimStartStop.Location = new System.Drawing.Point(14, 208);
            this.btnSimStartStop.Name = "btnSimStartStop";
            this.btnSimStartStop.Size = new System.Drawing.Size(126, 39);
            this.btnSimStartStop.TabIndex = 54;
            this.btnSimStartStop.Text = "Start Sim";
            this.btnSimStartStop.UseVisualStyleBackColor = true;
            this.btnSimStartStop.Click += new System.EventHandler(this.btnSimStartStop_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(11, 13);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(52, 13);
            this.label4.TabIndex = 57;
            this.label4.Text = "Cmd port:";
            // 
            // lblSimListenStatus
            // 
            this.lblSimListenStatus.BackColor = System.Drawing.Color.Red;
            this.lblSimListenStatus.Location = new System.Drawing.Point(14, 192);
            this.lblSimListenStatus.Name = "lblSimListenStatus";
            this.lblSimListenStatus.Size = new System.Drawing.Size(126, 13);
            this.lblSimListenStatus.TabIndex = 55;
            this.lblSimListenStatus.Text = "                 ";
            // 
            // txtSimCmdPort
            // 
            this.txtSimCmdPort.Location = new System.Drawing.Point(69, 10);
            this.txtSimCmdPort.Name = "txtSimCmdPort";
            this.txtSimCmdPort.Size = new System.Drawing.Size(71, 20);
            this.txtSimCmdPort.TabIndex = 56;
            this.txtSimCmdPort.Text = "50101";
            // 
            // btnReset
            // 
            this.btnReset.Location = new System.Drawing.Point(14, 264);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(126, 23);
            this.btnReset.TabIndex = 62;
            this.btnReset.Text = "Reset";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 118);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 13);
            this.label1.TabIndex = 63;
            this.label1.Text = "Yellow bots:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(81, 118);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(54, 13);
            this.label2.TabIndex = 64;
            this.label2.Text = "Blue bots:";
            // 
            // txtNumYellow
            // 
            this.txtNumYellow.Location = new System.Drawing.Point(19, 134);
            this.txtNumYellow.Name = "txtNumYellow";
            this.txtNumYellow.Size = new System.Drawing.Size(44, 20);
            this.txtNumYellow.TabIndex = 65;
            this.txtNumYellow.Text = "5";
            // 
            // txtNumBlue
            // 
            this.txtNumBlue.Location = new System.Drawing.Point(84, 134);
            this.txtNumBlue.Name = "txtNumBlue";
            this.txtNumBlue.Size = new System.Drawing.Size(44, 20);
            this.txtNumBlue.TabIndex = 66;
            this.txtNumBlue.Text = "5";
            // 
            // chkReferee
            // 
            this.chkReferee.AutoSize = true;
            this.chkReferee.Location = new System.Drawing.Point(14, 36);
            this.chkReferee.Name = "chkReferee";
            this.chkReferee.Size = new System.Drawing.Size(105, 17);
            this.chkReferee.TabIndex = 67;
            this.chkReferee.Text = "Referee (ip:port):";
            this.chkReferee.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(165, 13);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(74, 13);
            this.label3.TabIndex = 68;
            this.label3.Text = "SimScenarios:";
            // 
            // lstScenarios
            // 
            this.lstScenarios.FormattingEnabled = true;
            this.lstScenarios.Location = new System.Drawing.Point(168, 41);
            this.lstScenarios.Name = "lstScenarios";
            this.lstScenarios.Size = new System.Drawing.Size(137, 238);
            this.lstScenarios.TabIndex = 69;
            this.lstScenarios.SelectedIndexChanged += new System.EventHandler(this.lstScenarios_SelectedIndexChanged);
            // 
            // noisyVisionBox
            // 
            this.noisyVisionBox.AutoSize = true;
            this.noisyVisionBox.Location = new System.Drawing.Point(14, 160);
            this.noisyVisionBox.Name = "noisyVisionBox";
            this.noisyVisionBox.Size = new System.Drawing.Size(123, 17);
            this.noisyVisionBox.TabIndex = 70;
            this.noisyVisionBox.Text = "Simulate noisy vision";
            this.noisyVisionBox.UseVisualStyleBackColor = true;
            this.noisyVisionBox.CheckStateChanged += new System.EventHandler(this.noisyVisionBox_CheckStateChanged);
            // 
            // SimulatorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(323, 308);
            this.Controls.Add(this.noisyVisionBox);
            this.Controls.Add(this.lstScenarios);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.chkReferee);
            this.Controls.Add(this.txtNumBlue);
            this.Controls.Add(this.txtNumYellow);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnReset);
            this.Controls.Add(this.txtSimRefereeHost);
            this.Controls.Add(this.btnSimStartStop);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtSimCmdPort);
            this.Controls.Add(this.txtSimVisionHost);
            this.Controls.Add(this.lblSimListenStatus);
            this.Controls.Add(this.label4);
            this.Name = "SimulatorForm";
            this.Text = "Simulator";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SimulatorForm_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtSimRefereeHost;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtSimVisionHost;
        private System.Windows.Forms.Button btnSimStartStop;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblSimListenStatus;
        private System.Windows.Forms.TextBox txtSimCmdPort;
        private System.Windows.Forms.Button btnReset;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox txtNumYellow;
		private System.Windows.Forms.TextBox txtNumBlue;
        private System.Windows.Forms.CheckBox chkReferee;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ListBox lstScenarios;
        private System.Windows.Forms.CheckBox noisyVisionBox;
    }
}