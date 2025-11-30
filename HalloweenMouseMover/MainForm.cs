using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using HalloweenMouseMover.Interfaces;

namespace HalloweenMouseMover
{
    public partial class MainForm : Form
    {
        private readonly IApplicationController _controller;
        private NotifyIcon _notifyIcon = null!;
        private ContextMenuStrip _contextMenu = null!;
        private ToolStripMenuItem _startMenuItem = null!;
        private ToolStripMenuItem _stopMenuItem = null!;
        private ToolStripMenuItem _settingsMenuItem = null!;
        private ToolStripMenuItem _exitMenuItem = null!;
        private bool _isMonitoring;

        public MainForm(IApplicationController controller)
        {
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
            
            InitializeComponent();
            InitializeSystemTray();
            
            // Hide the form - we only use system tray
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            this.Visible = false;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            // MainForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(0, 0);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "MainForm";
            this.Text = "Halloween Mouse Mover";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            
            this.ResumeLayout(false);
        }

        private void InitializeSystemTray()
        {
            // Create context menu
            _contextMenu = new ContextMenuStrip();
            
            _startMenuItem = new ToolStripMenuItem("Start", null, OnStartClicked);
            _stopMenuItem = new ToolStripMenuItem("Stop", null, OnStopClicked);
            _stopMenuItem.Enabled = false; // Initially disabled
            _settingsMenuItem = new ToolStripMenuItem("Settings", null, OnSettingsClicked);
            _exitMenuItem = new ToolStripMenuItem("Exit", null, OnExitClicked);
            
            _contextMenu.Items.Add(_startMenuItem);
            _contextMenu.Items.Add(_stopMenuItem);
            _contextMenu.Items.Add(new ToolStripSeparator());
            _contextMenu.Items.Add(_settingsMenuItem);
            _contextMenu.Items.Add(new ToolStripSeparator());
            _contextMenu.Items.Add(_exitMenuItem);
            
            // Create notify icon
            _notifyIcon = new NotifyIcon();
            
            // Load custom icon
            try
            {
                string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "horror_mouse.ico");
                if (File.Exists(iconPath))
                {
                    _notifyIcon.Icon = new Icon(iconPath);
                }
                else
                {
                    // Try alternative path (project root)
                    iconPath = Path.Combine(Directory.GetCurrentDirectory(), "horror_mouse.ico");
                    if (File.Exists(iconPath))
                    {
                        _notifyIcon.Icon = new Icon(iconPath);
                    }
                    else
                    {
                        MessageBox.Show($"Icon not found at:\n{AppDomain.CurrentDomain.BaseDirectory}\n{Directory.GetCurrentDirectory()}", "Debug");
                        _notifyIcon.Icon = SystemIcons.Application; // Fallback to default
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading icon: {ex.Message}", "Debug");
                _notifyIcon.Icon = SystemIcons.Application; // Fallback on error
            }
            
            _notifyIcon.Text = "Halloween Mouse Mover";
            _notifyIcon.ContextMenuStrip = _contextMenu;
            _notifyIcon.Visible = true;
            
            // Double-click to show settings (or toggle start/stop)
            _notifyIcon.DoubleClick += OnNotifyIconDoubleClick;
        }

        private void MainForm_Load(object? sender, EventArgs e)
        {
            // Auto-start monitoring when form loads
            StartMonitoring();
        }

        private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            // Clean up
            if (_isMonitoring)
            {
                StopMonitoring();
            }
            
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
        }

        private void OnStartClicked(object? sender, EventArgs e)
        {
            StartMonitoring();
        }

        private void OnStopClicked(object? sender, EventArgs e)
        {
            StopMonitoring();
        }

        private void OnSettingsClicked(object? sender, EventArgs e)
        {
            try
            {
                _controller.ShowSettings();
            }
            catch (Exception ex)
            {
                ShowError("Settings Error", $"Failed to open settings: {ex.Message}");
            }
        }

        private void OnExitClicked(object? sender, EventArgs e)
        {
            Application.Exit();
        }

        private void OnNotifyIconDoubleClick(object? sender, EventArgs e)
        {
            // Toggle monitoring on double-click
            if (_isMonitoring)
            {
                StopMonitoring();
            }
            else
            {
                StartMonitoring();
            }
        }

        private void StartMonitoring()
        {
            if (_isMonitoring)
            {
                return;
            }

            try
            {
                _controller.Start();
                _isMonitoring = true;
                
                // Update menu items
                _startMenuItem.Enabled = false;
                _stopMenuItem.Enabled = true;
                
                // Update icon tooltip
                _notifyIcon.Text = "Halloween Mouse Mover - Running";
                
                ShowNotification("Started", "Halloween Mouse Mover is now monitoring for dialogs.");
            }
            catch (Exception ex)
            {
                ShowError("Start Error", $"Failed to start monitoring: {ex.Message}");
            }
        }

        private void StopMonitoring()
        {
            if (!_isMonitoring)
            {
                return;
            }

            try
            {
                _controller.Stop();
                _isMonitoring = false;
                
                // Update menu items
                _startMenuItem.Enabled = true;
                _stopMenuItem.Enabled = false;
                
                // Update icon tooltip
                _notifyIcon.Text = "Halloween Mouse Mover - Stopped";
                
                ShowNotification("Stopped", "Halloween Mouse Mover has stopped monitoring.");
            }
            catch (Exception ex)
            {
                ShowError("Stop Error", $"Failed to stop monitoring: {ex.Message}");
            }
        }

        private void ShowNotification(string title, string message)
        {
            _notifyIcon.ShowBalloonTip(3000, title, message, ToolTipIcon.Info);
        }

        private void ShowError(string title, string message)
        {
            _notifyIcon.ShowBalloonTip(5000, title, message, ToolTipIcon.Error);
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _notifyIcon?.Dispose();
                _contextMenu?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
