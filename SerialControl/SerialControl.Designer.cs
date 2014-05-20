namespace RFC.SerialControl
{
    partial class SerialControl
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
            this.idSelector = new System.Windows.Forms.NumericUpDown();
            this.idLabel = new System.Windows.Forms.Label();
            this.COMLabel = new System.Windows.Forms.Label();
            this.COMSelector = new System.Windows.Forms.NumericUpDown();
            this.connectButton = new System.Windows.Forms.Button();
            this.speedLabel = new System.Windows.Forms.Label();
            this.speedSelector = new System.Windows.Forms.NumericUpDown();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.idSelector)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.COMSelector)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.speedSelector)).BeginInit();
            this.SuspendLayout();
            // 
            // idSelector
            // 
            this.idSelector.Location = new System.Drawing.Point(115, 55);
            this.idSelector.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.idSelector.Name = "idSelector";
            this.idSelector.Size = new System.Drawing.Size(103, 22);
            this.idSelector.TabIndex = 0;
            // 
            // idLabel
            // 
            this.idLabel.AutoSize = true;
            this.idLabel.Location = new System.Drawing.Point(36, 58);
            this.idLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.idLabel.Name = "idLabel";
            this.idLabel.Size = new System.Drawing.Size(67, 17);
            this.idLabel.TabIndex = 1;
            this.idLabel.Text = "Robot ID:";
            // 
            // COMLabel
            // 
            this.COMLabel.AutoSize = true;
            this.COMLabel.Location = new System.Drawing.Point(33, 26);
            this.COMLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.COMLabel.Name = "COMLabel";
            this.COMLabel.Size = new System.Drawing.Size(72, 17);
            this.COMLabel.TabIndex = 2;
            this.COMLabel.Text = "COM port:";
            // 
            // COMSelector
            // 
            this.COMSelector.Location = new System.Drawing.Point(115, 23);
            this.COMSelector.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.COMSelector.Name = "COMSelector";
            this.COMSelector.Size = new System.Drawing.Size(103, 22);
            this.COMSelector.TabIndex = 3;
            // 
            // connectButton
            // 
            this.connectButton.Location = new System.Drawing.Point(13, 136);
            this.connectButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(100, 28);
            this.connectButton.TabIndex = 4;
            this.connectButton.Text = "Connect";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
            // 
            // speedLabel
            // 
            this.speedLabel.AutoSize = true;
            this.speedLabel.Location = new System.Drawing.Point(52, 90);
            this.speedLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.speedLabel.Name = "speedLabel";
            this.speedLabel.Size = new System.Drawing.Size(53, 17);
            this.speedLabel.TabIndex = 5;
            this.speedLabel.Text = "Speed:";
            // 
            // speedSelector
            // 
            this.speedSelector.Location = new System.Drawing.Point(115, 87);
            this.speedSelector.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.speedSelector.Name = "speedSelector";
            this.speedSelector.Size = new System.Drawing.Size(103, 22);
            this.speedSelector.TabIndex = 6;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(16, 272);
            this.textBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(345, 22);
            this.textBox1.TabIndex = 7;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(121, 136);
            this.button1.Margin = new System.Windows.Forms.Padding(4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(100, 28);
            this.button1.TabIndex = 8;
            this.button1.Text = "Charge";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.chargeButton_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(121, 172);
            this.button2.Margin = new System.Windows.Forms.Padding(4);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(100, 28);
            this.button2.TabIndex = 9;
            this.button2.Text = "Kick";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.KickButton_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(121, 208);
            this.button3.Margin = new System.Windows.Forms.Padding(4);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(100, 28);
            this.button3.TabIndex = 10;
            this.button3.Text = "breakBeam";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.breakbeamButton_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(229, 136);
            this.button4.Margin = new System.Windows.Forms.Padding(4);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(100, 28);
            this.button4.TabIndex = 11;
            this.button4.Text = "Dribble";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.dribbler_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(229, 172);
            this.button5.Margin = new System.Windows.Forms.Padding(4);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(100, 28);
            this.button5.TabIndex = 12;
            this.button5.Text = "StopDribble";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.dribblerStop_Click);
            // 
            // SerialControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(379, 322);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.speedSelector);
            this.Controls.Add(this.speedLabel);
            this.Controls.Add(this.connectButton);
            this.Controls.Add(this.COMSelector);
            this.Controls.Add(this.COMLabel);
            this.Controls.Add(this.idLabel);
            this.Controls.Add(this.idSelector);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "SerialControl";
            this.Text = "SerialControl";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SerialControl_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.SerialControl_KeyUp);
            ((System.ComponentModel.ISupportInitialize)(this.idSelector)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.COMSelector)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.speedSelector)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown idSelector;
        private System.Windows.Forms.Label idLabel;
        private System.Windows.Forms.Label COMLabel;
        private System.Windows.Forms.NumericUpDown COMSelector;
        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.Label speedLabel;
        private System.Windows.Forms.NumericUpDown speedSelector;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
    }
}