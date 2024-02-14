namespace open_dust_monitor.forms
{
    partial class MainForm
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            label1 = new Label();
            button1 = new Button();
            timer1 = new System.Timers.Timer();
            notifyIcon1 = new NotifyIcon(components);
            panel1 = new Panel();
            ((System.ComponentModel.ISupportInitialize)timer1).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label1.ForeColor = Color.White;
            label1.Location = new Point(22, 26);
            label1.Margin = new Padding(2, 0, 2, 0);
            label1.Name = "label1";
            label1.Size = new Size(88, 25);
            label1.TabIndex = 0;
            label1.Text = "Loading..";
            // 
            // button1
            // 
            button1.AutoSize = true;
            button1.FlatStyle = FlatStyle.Flat;
            button1.Location = new Point(11, 499);
            button1.Margin = new Padding(2);
            button1.Name = "button1";
            button1.Size = new Size(99, 27);
            button1.TabIndex = 1;
            button1.Text = "Take Snapshot";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click_1;
            // 
            // timer1
            // 
            timer1.Enabled = true;
            timer1.Interval = 300000D;
            timer1.SynchronizingObject = this;
            timer1.Elapsed += timer1_Elapsed_1;
            // 
            // notifyIcon1
            // 
            notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
            notifyIcon1.BalloonTipText = "test info";
            notifyIcon1.BalloonTipTitle = "test";
            notifyIcon1.Icon = (Icon)resources.GetObject("notifyIcon1.Icon");
            notifyIcon1.Text = "notifyIcon1";
            notifyIcon1.Visible = true;
            // 
            // panel1
            // 
            panel1.Location = new Point(15, 258);
            panel1.Name = "panel1";
            panel1.Size = new Size(760, 228);
            panel1.TabIndex = 2;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            BackColor = Color.FromArgb(64, 64, 64);
            ClientSize = new Size(787, 537);
            Controls.Add(panel1);
            Controls.Add(button1);
            Controls.Add(label1);
            ForeColor = Color.White;
            FormBorderStyle = FormBorderStyle.None;
            Margin = new Padding(2);
            Name = "MainForm";
            Text = "Open Dust Monitor";
            TopMost = true;
            Load += MainForm_Load_1;
            ((System.ComponentModel.ISupportInitialize)timer1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private System.Timers.Timer timer1;

        private System.Windows.Forms.Button button1;

        private System.Windows.Forms.Label label1;

        #endregion

        private NotifyIcon notifyIcon1;
        private Panel panel1;
    }
}