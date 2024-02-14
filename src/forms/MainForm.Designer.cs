
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
            panel2 = new Panel();
            button3 = new Button();
            button2 = new Button();
            label2 = new Label();
            ((System.ComponentModel.ISupportInitialize)timer1).BeginInit();
            panel2.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label1.ForeColor = Color.White;
            label1.Location = new Point(16, 63);
            label1.Name = "label1";
            label1.Size = new Size(804, 410);
            label1.TabIndex = 0;
            label1.Text = "Loading..";
            // 
            // button1
            // 
            button1.AutoSize = true;
            button1.FlatStyle = FlatStyle.Flat;
            button1.Location = new Point(16, 785);
            button1.Name = "button1";
            button1.Size = new Size(141, 45);
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
            notifyIcon1.MouseDown += SysTray_Click;
            // 
            // panel1
            // 
            panel1.Location = new Point(16, 478);
            panel1.Margin = new Padding(4, 5, 4, 5);
            panel1.Name = "panel1";
            panel1.Size = new Size(804, 287);
            panel1.TabIndex = 2;
            // 
            // panel2
            // 
            panel2.BackColor = Color.DimGray;
            panel2.Controls.Add(button3);
            panel2.Controls.Add(button2);
            panel2.Controls.Add(label2);
            panel2.Location = new Point(-12, -22);
            panel2.Margin = new Padding(0);
            panel2.Name = "panel2";
            panel2.Size = new Size(898, 62);
            panel2.TabIndex = 3;
            panel2.MouseDown += panel2_MouseDown;
            // 
            // button3
            // 
            button3.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            button3.BackColor = Color.DimGray;
            button3.FlatAppearance.BorderSize = 0;
            button3.FlatStyle = FlatStyle.Flat;
            button3.Font = new Font("Segoe UI Symbol", 10F, FontStyle.Bold, GraphicsUnit.Point, 0);
            button3.ForeColor = Color.White;
            button3.Location = new Point(760, 21);
            button3.Name = "button3";
            button3.Size = new Size(48, 39);
            button3.TabIndex = 5;
            button3.Text = "__";
            button3.UseVisualStyleBackColor = false;
            button3.Click += button3_Click;
            // 
            // button2
            // 
            button2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            button2.BackColor = Color.DimGray;
            button2.FlatAppearance.BorderSize = 0;
            button2.FlatStyle = FlatStyle.Flat;
            button2.Font = new Font("Segoe UI Symbol", 10F, FontStyle.Bold, GraphicsUnit.Point, 0);
            button2.Location = new Point(807, 23);
            button2.Name = "button2";
            button2.Size = new Size(48, 39);
            button2.TabIndex = 4;
            button2.Text = "X";
            button2.UseVisualStyleBackColor = false;
            button2.Click += button2_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.FlatStyle = FlatStyle.Flat;
            label2.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label2.Location = new Point(22, 25);
            label2.Name = "label2";
            label2.Size = new Size(222, 32);
            label2.TabIndex = 4;
            label2.Text = "Open Dust Monitor";
            label2.MouseDown += panel2_MouseDown;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(64, 64, 64);
            ClientSize = new Size(845, 851);
            Controls.Add(panel2);
            Controls.Add(label1);
            Controls.Add(panel1);
            Controls.Add(button1);
            ForeColor = Color.White;
            FormBorderStyle = FormBorderStyle.None;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Open Dust Monitor";
            TopMost = true;
            Load += MainForm_Load_1;
            ((System.ComponentModel.ISupportInitialize)timer1).EndInit();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        private System.Timers.Timer timer1;

        private System.Windows.Forms.Button button1;

        private System.Windows.Forms.Label label1;

        #endregion

        private NotifyIcon notifyIcon1;
        private Panel panel1;
        private Panel panel2;
        private Label label2;
        private Button button3;
        private Button button2;
    }
}