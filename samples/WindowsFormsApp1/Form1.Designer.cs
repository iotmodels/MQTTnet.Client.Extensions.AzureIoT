namespace WindowsFormsApp1
{
    partial class Form1
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
            this.components = new System.ComponentModel.Container();
            this.textBoxConnectionString = new System.Windows.Forms.TextBox();
            this.buttonConnect = new System.Windows.Forms.Button();
            this.labelConnectionStatus = new System.Windows.Forms.Label();
            this.textBoxTwin = new System.Windows.Forms.TextBox();
            this.textBoxDesired = new System.Windows.Forms.TextBox();
            this.textBoxCommands = new System.Windows.Forms.TextBox();
            this.textBoxTelemetry = new System.Windows.Forms.TextBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // textBoxConnectionString
            // 
            this.textBoxConnectionString.Location = new System.Drawing.Point(41, 61);
            this.textBoxConnectionString.Name = "textBoxConnectionString";
            this.textBoxConnectionString.Size = new System.Drawing.Size(798, 20);
            this.textBoxConnectionString.TabIndex = 0;
            this.textBoxConnectionString.Text = "Hostname=tests.azure-devices.net;DeviceId=sdklite_managed;SharedAccessKey=MDAwMDA" +
    "wMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDA=;SasMinutes=2;KeepAliveInSeconds=10";
            // 
            // buttonConnect
            // 
            this.buttonConnect.Location = new System.Drawing.Point(854, 61);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(93, 20);
            this.buttonConnect.TabIndex = 1;
            this.buttonConnect.Text = "Connect";
            this.buttonConnect.UseVisualStyleBackColor = true;
            this.buttonConnect.Click += new System.EventHandler(this.buttonConnect_Click);
            // 
            // labelConnectionStatus
            // 
            this.labelConnectionStatus.AutoSize = true;
            this.labelConnectionStatus.Location = new System.Drawing.Point(44, 94);
            this.labelConnectionStatus.Name = "labelConnectionStatus";
            this.labelConnectionStatus.Size = new System.Drawing.Size(35, 13);
            this.labelConnectionStatus.TabIndex = 2;
            this.labelConnectionStatus.Text = "label1";
            // 
            // textBoxTwin
            // 
            this.textBoxTwin.Location = new System.Drawing.Point(49, 138);
            this.textBoxTwin.Multiline = true;
            this.textBoxTwin.Name = "textBoxTwin";
            this.textBoxTwin.Size = new System.Drawing.Size(150, 130);
            this.textBoxTwin.TabIndex = 3;
            // 
            // textBoxDesired
            // 
            this.textBoxDesired.Location = new System.Drawing.Point(220, 138);
            this.textBoxDesired.Multiline = true;
            this.textBoxDesired.Name = "textBoxDesired";
            this.textBoxDesired.Size = new System.Drawing.Size(150, 130);
            this.textBoxDesired.TabIndex = 4;
            // 
            // textBoxCommands
            // 
            this.textBoxCommands.Location = new System.Drawing.Point(388, 138);
            this.textBoxCommands.Multiline = true;
            this.textBoxCommands.Name = "textBoxCommands";
            this.textBoxCommands.Size = new System.Drawing.Size(150, 130);
            this.textBoxCommands.TabIndex = 5;
            // 
            // textBoxTelemetry
            // 
            this.textBoxTelemetry.Location = new System.Drawing.Point(53, 303);
            this.textBoxTelemetry.Multiline = true;
            this.textBoxTelemetry.Name = "textBoxTelemetry";
            this.textBoxTelemetry.Size = new System.Drawing.Size(525, 117);
            this.textBoxTelemetry.TabIndex = 6;
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1055, 450);
            this.Controls.Add(this.textBoxTelemetry);
            this.Controls.Add(this.textBoxCommands);
            this.Controls.Add(this.textBoxDesired);
            this.Controls.Add(this.textBoxTwin);
            this.Controls.Add(this.labelConnectionStatus);
            this.Controls.Add(this.buttonConnect);
            this.Controls.Add(this.textBoxConnectionString);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxConnectionString;
        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.Label labelConnectionStatus;
        private System.Windows.Forms.TextBox textBoxTwin;
        private System.Windows.Forms.TextBox textBoxDesired;
        private System.Windows.Forms.TextBox textBoxCommands;
        private System.Windows.Forms.TextBox textBoxTelemetry;
        internal System.Windows.Forms.Timer timer1;
    }
}

