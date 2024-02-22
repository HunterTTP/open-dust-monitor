using open_dust_monitor.src.handler;
using open_dust_monitor.src.repositories;
using open_dust_monitor.src.services;

namespace open_dust_monitor.src.forms
{
    public partial class MainForm : Form
    {

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        private readonly TemperatureService _temperatureService = InstanceHandler.GetTemperatureService();
        private readonly Label LatestSnapshotLabel = new();
        private readonly Label RecentSnapshotsCountLabel = new();
        private readonly Label RecentSnapshotsTemperaturesLabel = new();
        private readonly Label BaselineSnapshotsCountLabel = new();
        private readonly Label AlertThresholdTemperaturesLabel = new();
        private readonly System.Windows.Forms.Timer MainTimer = new();
        private readonly NotifyIcon MainNotifyIcon = new();
        private readonly TableLayoutPanel topBar = new();

        public MainForm()
        {
            InitializeComponent();
            InitializeClassInstances();
            InitializeUi();
        }

        private static void InitializeClassInstances()
        {
            InstanceHandler.CreateAllInstances();
        }

        private void InitializeUi()
        {
            ConfigureMainForm();
            AddTopBar();
            AddLabelTable();
            AddMainNotifyIcon();
            AddMainTimer();
            LogHandler.Logger.Information("UI Initialized");
        }

        private void ConfigureMainForm()
        {
            this.Opacity = 0;
            this.Icon = Properties.Resources.logo_icon;
            this.Text = "Open Dust Monitor";
            this.AutoSize = true;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.BackColor = Color.FromArgb(95, 95, 95);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Load += MainFormLoad;
            LogHandler.Logger.Debug("ConfigureMainForm complete");
        }

        private void AddTopBar()
        {
            topBar.ColumnCount = 5;
            topBar.RowCount = 1;
            topBar.Dock = DockStyle.Top;
            topBar.AutoSize = true;
            topBar.BackColor = Color.FromArgb(70, 70, 70);
            topBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50F));
            topBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            topBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50F));
            topBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50F));
            topBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50F));
            topBar.MouseDown += new MouseEventHandler(DragHandler);

            PictureBox topBarLogo = new()
            {
                Image = Properties.Resources.logo_image,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Height = 30,
                Margin = new Padding(10)
            };
            topBar.Controls.Add(topBarLogo, 0, 0);
            topBarLogo.MouseDown += new MouseEventHandler(DragHandler);

            Label topBarTitle = new()
            {
                Text = "Open Dust Monitor",
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = true,
                Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point, 0)
            };
            topBar.Controls.Add(topBarTitle, 1, 0);
            topBarTitle.MouseDown += new MouseEventHandler(DragHandler);

            ImageList imageList = new ImageList
            {
                ImageSize = new Size(24, 24),
                ColorDepth = ColorDepth.Depth32Bit
            };
            imageList.Images.Add("minimize", Properties.Resources.minimizeButton);
            imageList.Images.Add("maximize", Properties.Resources.fullscreenButton);
            imageList.Images.Add("close", Properties.Resources.closeButton);

            Button minimizeButton = new()
            {
                ImageList = imageList,
                ImageIndex = imageList.Images.IndexOfKey("minimize"),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(70, 70, 70),
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat,
            };
            minimizeButton.FlatAppearance.BorderSize = 0;
            minimizeButton.Click += MinimizeButtonClick;
            topBar.Controls.Add(minimizeButton, 2, 0);

            Button maximizeButton = new()
            {
                ImageList = imageList,
                ImageIndex = imageList.Images.IndexOfKey("maximize"),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(70, 70, 70),
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat,
            };
            maximizeButton.FlatAppearance.BorderSize = 0;
            maximizeButton.Click += MaximizeButtonClick;
            topBar.Controls.Add(maximizeButton, 3, 0);

            Button closeButton = new()
            {
                ImageList = imageList,
                ImageIndex = imageList.Images.IndexOfKey("close"),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(70, 70, 70),
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat
            };
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.Click += CloseButtonClick;
            topBar.Controls.Add(closeButton, 4, 0);
            this.Controls.Add(topBar);
        }

        private void AddLabelTable()
        {
            var tableLayoutPanel = new TableLayoutPanel
            {
                ColumnCount = 2,
                RowCount = 5,
                Dock = DockStyle.Fill,
                AutoSize = true,
                Padding = new Padding(15, topBar.Height + 10, 15, 15)
            };

            for (int i = 0; i < tableLayoutPanel.RowCount; i++)
            {
                _ = tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }

            ConfigureLabelProperties(LatestSnapshotLabel);
            tableLayoutPanel.SetColumnSpan(LatestSnapshotLabel, 2);
            tableLayoutPanel.Controls.Add(LatestSnapshotLabel, 0, 0);

            ConfigureLabelProperties(RecentSnapshotsCountLabel);
            tableLayoutPanel.Controls.Add(RecentSnapshotsCountLabel, 0, 1);

            ConfigureLabelProperties(BaselineSnapshotsCountLabel);
            tableLayoutPanel.Controls.Add(BaselineSnapshotsCountLabel, 1, 1);

            ConfigureLabelProperties(RecentSnapshotsTemperaturesLabel);
            tableLayoutPanel.Controls.Add(RecentSnapshotsTemperaturesLabel, 0, 2);

            ConfigureLabelProperties(AlertThresholdTemperaturesLabel);
            tableLayoutPanel.Controls.Add(AlertThresholdTemperaturesLabel, 1, 2);

            this.Controls.Add(tableLayoutPanel);
            LogHandler.Logger.Debug("AddLabelTable complete");
        }

        private static void ConfigureLabelProperties(Label label)
        {
            label.AutoSize = true;
            label.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label.ForeColor = Color.White;
            label.Text = "Loading..";
            label.Padding = new Padding(10);
        }


        private void AddMainNotifyIcon()
        {
            MainNotifyIcon.Icon = Properties.Resources.logo_icon;
            MainNotifyIcon.Text = "Loading..";
            MainNotifyIcon.Visible = true;
            MainNotifyIcon.MouseClick += new MouseEventHandler(NotifyIcon_Clicked);
            AddMainNotifyContextMenu();
            LogHandler.Logger.Debug("AddMainNotifyIcon complete");
        }

        private void AddMainNotifyContextMenu()
        {
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            ToolStripMenuItem exitMenuItem = new ToolStripMenuItem("Exit Open Dust Monitor", null, ExitMenuItem_Click);
            contextMenu.Items.Add(exitMenuItem);
            MainNotifyIcon.ContextMenuStrip = contextMenu;
        }

        private void ExitMenuItem_Click(object? sender, EventArgs e)
        {
            Application.Exit();
        }


        private void AddMainTimer()
        {
            MainTimer.Enabled = true;
            MainTimer.Interval = TemperatureService.GetSnapshotIntervalMillis();
            MainTimer.Tick += new EventHandler(TimerTick);
            LogHandler.Logger.Debug("AddMainTimer complete");
        }

        private void MainFormLoad(object? sender, EventArgs e)
        {
            _ = UpdateFormWithCpuInfo();
            MinimizeToSysTray();
            LogHandler.Logger.Debug("MainFormLoad complete");
        }

        private void TimerTick(object? sender, EventArgs e)
        {
            _ = UpdateFormWithCpuInfo();
        }

        private async Task UpdateFormWithCpuInfo()
        {
            var latestTemperatureSnapshot = await Task.Run(() => _temperatureService.GetLatestTemperatureSnapshot());
            var recentSnapshots = TemperatureRepository.GetLoadedRecentSnapshots();
            var baselineSnapshots = TemperatureRepository.GetLoadedBaselineSnapshots();
            LatestSnapshotLabel.Text = TemperatureService.GetLatestSnapshotLabel(latestTemperatureSnapshot);
            RecentSnapshotsCountLabel.Text = TemperatureService.GetSnapshotCountsLabel(recentSnapshots, "Recent");
            RecentSnapshotsTemperaturesLabel.Text = TemperatureService.GetSnapshotTemperaturesLabel(recentSnapshots, "Recent");
            BaselineSnapshotsCountLabel.Text = TemperatureService.GetSnapshotCountsLabel(baselineSnapshots, "Baseline");
            AlertThresholdTemperaturesLabel.Text = TemperatureService.GetSnapshotAlertThresholdsLabel(baselineSnapshots);
            MainNotifyIcon.Text = TemperatureService.GetLatestSnapshotLabel(latestTemperatureSnapshot);
            AlertIfTemperatureIsOutsideThreshold();
            LogHandler.Logger.Debug("UpdateFormWithCpuInfo complete");
        }

        private void AlertIfTemperatureIsOutsideThreshold()
        {
            var areTemperaturesWithinThreshold = TemperatureService.AreRecentAverageTemperaturesWithinThreshold();
            var wasUserRecentlyNotified = _temperatureService.WasUserRecentlyNotified();
            LogHandler.Logger.Information("AlertIfTemperatureIsOutsideThreshold areTemperaturesWithinThreshold=" + areTemperaturesWithinThreshold + " wasUserRecentlyNotified=" + wasUserRecentlyNotified);
            if (!areTemperaturesWithinThreshold && !wasUserRecentlyNotified)
            {
                var temperatureNotification = MessageBox.Show(
                    "Your PC has been running hotter than usual for the past few days." +
                    " This is likely due to dust build up, please clean the fans and heatsinks with compressed air." +
                    "\n\nYou will not see this alert again as long as the temperatures return to normal within the next few days." +
                    "\n\nPlease select 'Yes' if this analysis was correct or 'No' if the PC is already clean and running normally.",
                    "Open Dust Monitor - PC Temperature Alert",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.DefaultDesktopOnly);
                _temperatureService.UserWasNotified(DateTime.Now);
                LogHandler.Logger.Warning("AlertIfTemperatureIsOutsideThreshold alerted for hot temperature");

                if (temperatureNotification == DialogResult.No)
                {
                    TemperatureService.ResetBaselineTemperatures();
                }
            }
        }

        private void MaximizeButtonClick(object? sender, EventArgs e)
        {
            MaximizeToggle();
        }

        private void CloseButtonClick(object? sender, EventArgs e)
        {
            MinimizeToSysTray();
        }

        private void NotifyIcon_Clicked(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                MinimizeToSysTray();
            }
        }

        private void MinimizeButtonClick(object? sender, EventArgs e)
        {
            this.ShowInTaskbar = true;
            this.WindowState = FormWindowState.Minimized;
        }

        private void MinimizeToSysTray()
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.ShowInTaskbar = true;
                this.WindowState = FormWindowState.Normal;
                this.Activate();
                this.Opacity = 100;
            }
            else
            {
                this.WindowState = FormWindowState.Minimized;
                this.ShowInTaskbar = false;
            }
        }

        private void MaximizeToggle()
        {
            this.WindowState = this.WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
        }

        private void DragHandler(object? sender, MouseEventArgs e)
        {
            if (e.Clicks == 2)
            {
                MaximizeToggle();
                return;
            }

            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                _ = SendMessage(this.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
    }
}