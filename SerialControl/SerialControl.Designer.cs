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
            ((System.ComponentModel.ISupportInitialize)(this.idSelector)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.COMSelector)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.speedSelector)).BeginInit();
            this.SuspendLayout();
            // 
            // idSelector
            // 
            this.idSelector.Location = new System.Drawing.Point(86, 45);
            this.idSelector.Name = "idSelector";
            this.idSelector.Size = new System.Drawing.Size(77, 20);
            this.idSelector.TabIndex = 0;
            // 
            // idLabel
            // 
            this.idLabel.AutoSize = true;
            this.idLabel.Location = new System.Drawing.Point(27, 47);
            this.idLabel.Name = "idLabel";
            this.idLabel.Size = new System.Drawing.Size(53, 13);
            this.idLabel.TabIndex = 1;
            this.idLabel.Text = "Robot ID:";
            // 
            // COMLabel
            // 
            this.COMLabel.AutoSize = true;
            this.COMLabel.Location = new System.Drawing.Point(25, 21);
            this.COMLabel.Name = "COMLabel";
            this.COMLabel.Size = new System.Drawing.Size(55, 13);
            this.COMLabel.TabIndex = 2;
            this.COMLabel.Text = "COM port:";
            // 
            // COMSelector
            // 
            this.COMSelector.Location = new System.Drawing.Point(86, 19);
            this.COMSelector.Name = "COMSelector";
            this.COMSelector.Size = new System.Drawing.Size(77, 20);
            this.COMSelector.TabIndex = 3;
            // 
            // connectButton
            // 
            this.connectButton.Location = new System.Drawing.Point(106, 114);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(75, 23);
            this.connectButton.TabIndex = 4;
            this.connectButton.Text = "Connect";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
            // 
            // speedLabel
            // 
            this.speedLabel.AutoSize = true;
            this.speedLabel.Location = new System.Drawing.Point(39, 73);
            this.speedLabel.Name = "speedLabel";
            this.speedLabel.Size = new System.Drawing.Size(41, 13);
            this.speedLabel.TabIndex = 5;
            this.speedLabel.Text = "Speed:";
            // 
            // speedSelector
            // 
            this.speedSelector.Location = new System.Drawing.Point(86, 71);
            this.speedSelector.Name = "speedSelector";
            this.speedSelector.Size = new System.Drawing.Size(77, 20);
            this.speedSelector.TabIndex = 6;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(12, 221);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(260, 20);
            this.textBox1.TabIndex = 7;
            // 
            // SerialControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.speedSelector);
            this.Controls.Add(this.speedLabel);
            this.Controls.Add(this.connectButton);
            this.Controls.Add(this.COMSelector);
            this.Controls.Add(this.COMLabel);
            this.Controls.Add(this.idLabel);
            this.Controls.Add(this.idSelector);
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
    }
}