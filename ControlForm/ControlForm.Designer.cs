namespace ControlForm
{
    partial class ControlForm
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
            this.RunButton = new System.Windows.Forms.Button();
            this.ComLabel = new System.Windows.Forms.Label();
            this.ComNumberChooser = new System.Windows.Forms.NumericUpDown();
            this.TeamBox = new System.Windows.Forms.ComboBox();
            this.TeamLabel = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.flippedCheckBox = new System.Windows.Forms.CheckBox();
            this.simulatorCheckBox = new System.Windows.Forms.CheckBox();
            this.GoalieNumberChooser = new System.Windows.Forms.NumericUpDown();
            this.goalieLabel = new System.Windows.Forms.Label();
            this.PlayBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.ComNumberChooser)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.GoalieNumberChooser)).BeginInit();
            this.SuspendLayout();
            // 
            // RunButton
            // 
            this.RunButton.Location = new System.Drawing.Point(155, 249);
            this.RunButton.Name = "RunButton";
            this.RunButton.Size = new System.Drawing.Size(117, 32);
            this.RunButton.TabIndex = 0;
            this.RunButton.Text = "Run";
            this.RunButton.UseVisualStyleBackColor = true;
            this.RunButton.Click += new System.EventHandler(this.RunButton_Click);
            // 
            // ComLabel
            // 
            this.ComLabel.AutoSize = true;
            this.ComLabel.Location = new System.Drawing.Point(12, 14);
            this.ComLabel.Name = "ComLabel";
            this.ComLabel.Size = new System.Drawing.Size(56, 13);
            this.ComLabel.TabIndex = 1;
            this.ComLabel.Text = "COM Port:";
            // 
            // ComNumberChooser
            // 
            this.ComNumberChooser.Location = new System.Drawing.Point(74, 12);
            this.ComNumberChooser.Name = "ComNumberChooser";
            this.ComNumberChooser.Size = new System.Drawing.Size(67, 20);
            this.ComNumberChooser.TabIndex = 2;
            // 
            // TeamBox
            // 
            this.TeamBox.FormattingEnabled = true;
            this.TeamBox.Location = new System.Drawing.Point(53, 124);
            this.TeamBox.Name = "TeamBox";
            this.TeamBox.Size = new System.Drawing.Size(121, 21);
            this.TeamBox.TabIndex = 3;
            // 
            // TeamLabel
            // 
            this.TeamLabel.AutoSize = true;
            this.TeamLabel.Location = new System.Drawing.Point(12, 127);
            this.TeamLabel.Name = "TeamLabel";
            this.TeamLabel.Size = new System.Drawing.Size(37, 13);
            this.TeamLabel.TabIndex = 4;
            this.TeamLabel.Text = "Team:";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(15, 249);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(117, 32);
            this.button1.TabIndex = 5;
            this.button1.Text = "Stop";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.StopButton_Click);
            // 
            // flippedCheckBox
            // 
            this.flippedCheckBox.AutoSize = true;
            this.flippedCheckBox.Location = new System.Drawing.Point(12, 151);
            this.flippedCheckBox.Name = "flippedCheckBox";
            this.flippedCheckBox.Size = new System.Drawing.Size(60, 17);
            this.flippedCheckBox.TabIndex = 6;
            this.flippedCheckBox.Text = "Flipped";
            this.flippedCheckBox.UseVisualStyleBackColor = true;
            this.flippedCheckBox.CheckedChanged += new System.EventHandler(this.flippedCheckBox_CheckedChanged);
            // 
            // simulatorCheckBox
            // 
            this.simulatorCheckBox.AutoSize = true;
            this.simulatorCheckBox.Checked = true;
            this.simulatorCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.simulatorCheckBox.Location = new System.Drawing.Point(15, 38);
            this.simulatorCheckBox.Name = "simulatorCheckBox";
            this.simulatorCheckBox.Size = new System.Drawing.Size(122, 17);
            this.simulatorCheckBox.TabIndex = 7;
            this.simulatorCheckBox.Text = "Connect to simulator";
            this.simulatorCheckBox.UseVisualStyleBackColor = true;
            this.simulatorCheckBox.CheckedChanged += new System.EventHandler(this.simulatorCheckBox_CheckedChanged);
            // 
            // GoalieNumberChooser
            // 
            this.GoalieNumberChooser.Location = new System.Drawing.Point(72, 196);
            this.GoalieNumberChooser.Name = "GoalieNumberChooser";
            this.GoalieNumberChooser.Size = new System.Drawing.Size(65, 20);
            this.GoalieNumberChooser.TabIndex = 8;
            // 
            // goalieLabel
            // 
            this.goalieLabel.AutoSize = true;
            this.goalieLabel.Location = new System.Drawing.Point(12, 198);
            this.goalieLabel.Name = "goalieLabel";
            this.goalieLabel.Size = new System.Drawing.Size(54, 13);
            this.goalieLabel.TabIndex = 9;
            this.goalieLabel.Text = "Goalie ID:";
            // 
            // PlayBox
            // 
            this.PlayBox.FormattingEnabled = true;
            this.PlayBox.Items.AddRange(new object[] {
            "Goalie",
            "Movement",
            "Normal",
            "Offense",
            "Setup",
            "Timeout"});
            this.PlayBox.Location = new System.Drawing.Point(52, 80);
            this.PlayBox.Name = "PlayBox";
            this.PlayBox.Size = new System.Drawing.Size(121, 21);
            this.PlayBox.Sorted = true;
            this.PlayBox.TabIndex = 10;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 83);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(30, 13);
            this.label1.TabIndex = 11;
            this.label1.Text = "Play:";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // ControlForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(295, 302);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.PlayBox);
            this.Controls.Add(this.goalieLabel);
            this.Controls.Add(this.GoalieNumberChooser);
            this.Controls.Add(this.simulatorCheckBox);
            this.Controls.Add(this.flippedCheckBox);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.TeamLabel);
            this.Controls.Add(this.TeamBox);
            this.Controls.Add(this.ComNumberChooser);
            this.Controls.Add(this.ComLabel);
            this.Controls.Add(this.RunButton);
            this.Name = "ControlForm";
            this.Text = "Form1";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ControlForm_FormClosed);
            ((System.ComponentModel.ISupportInitialize)(this.ComNumberChooser)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.GoalieNumberChooser)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button RunButton;
        private System.Windows.Forms.Label ComLabel;
        private System.Windows.Forms.NumericUpDown ComNumberChooser;
        private System.Windows.Forms.ComboBox TeamBox;
        private System.Windows.Forms.Label TeamLabel;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.CheckBox flippedCheckBox;
        private System.Windows.Forms.CheckBox simulatorCheckBox;
        private System.Windows.Forms.NumericUpDown GoalieNumberChooser;
        private System.Windows.Forms.Label goalieLabel;
        private System.Windows.Forms.ComboBox PlayBox;
        private System.Windows.Forms.Label label1;
    }
}

