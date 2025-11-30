using System;
using System.Windows.Forms;
using HalloweenMouseMover.Models;
using HalloweenMouseMover.Utils;

namespace HalloweenMouseMover
{
    public partial class SettingsForm : Form
    {
        private readonly ConfigurationManager _configManager;
        private CheckBox _enableSoundCheckBox = null!;
        private CheckBox _enableCursorChangeCheckBox = null!;
        private NumericUpDown _pollingIntervalNumeric = null!;
        private NumericUpDown _cursorMovementDurationNumeric = null!;
        private NumericUpDown _cursorRestoreDelayNumeric = null!;
        private Button _saveButton = null!;
        private Button _cancelButton = null!;

        public SettingsForm()
        {
            _configManager = ConfigurationManager.Instance;
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form properties
            this.Text = "Halloween Mouse Mover - Settings";
            this.ClientSize = new System.Drawing.Size(450, 320);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Enable Sound CheckBox
            _enableSoundCheckBox = new CheckBox
            {
                Text = "Enable Halloween Sound",
                Location = new System.Drawing.Point(20, 20),
                Size = new System.Drawing.Size(400, 24),
                Checked = true
            };
            this.Controls.Add(_enableSoundCheckBox);

            // Enable Cursor Change CheckBox
            _enableCursorChangeCheckBox = new CheckBox
            {
                Text = "Enable Horror Cursor Change",
                Location = new System.Drawing.Point(20, 55),
                Size = new System.Drawing.Size(400, 24),
                Checked = true
            };
            this.Controls.Add(_enableCursorChangeCheckBox);

            // Polling Interval Label
            var pollingIntervalLabel = new Label
            {
                Text = "Dialog Polling Interval (ms):",
                Location = new System.Drawing.Point(20, 100),
                Size = new System.Drawing.Size(250, 20),
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            this.Controls.Add(pollingIntervalLabel);

            // Polling Interval NumericUpDown
            _pollingIntervalNumeric = new NumericUpDown
            {
                Location = new System.Drawing.Point(280, 98),
                Size = new System.Drawing.Size(150, 23),
                Minimum = 10,
                Maximum = 1000,
                Value = 50,
                Increment = 10
            };
            this.Controls.Add(_pollingIntervalNumeric);

            // Cursor Movement Duration Label
            var cursorMovementLabel = new Label
            {
                Text = "Cursor Movement Duration (ms):",
                Location = new System.Drawing.Point(20, 140),
                Size = new System.Drawing.Size(250, 20),
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            this.Controls.Add(cursorMovementLabel);

            // Cursor Movement Duration NumericUpDown
            _cursorMovementDurationNumeric = new NumericUpDown
            {
                Location = new System.Drawing.Point(280, 138),
                Size = new System.Drawing.Size(150, 23),
                Minimum = 50,
                Maximum = 2000,
                Value = 200,
                Increment = 50
            };
            this.Controls.Add(_cursorMovementDurationNumeric);

            // Cursor Restore Delay Label
            var cursorRestoreLabel = new Label
            {
                Text = "Cursor Restore Delay (ms):",
                Location = new System.Drawing.Point(20, 180),
                Size = new System.Drawing.Size(250, 20),
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            this.Controls.Add(cursorRestoreLabel);

            // Cursor Restore Delay NumericUpDown
            _cursorRestoreDelayNumeric = new NumericUpDown
            {
                Location = new System.Drawing.Point(280, 178),
                Size = new System.Drawing.Size(150, 23),
                Minimum = 500,
                Maximum = 10000,
                Value = 2000,
                Increment = 500
            };
            this.Controls.Add(_cursorRestoreDelayNumeric);

            // Info Label
            var infoLabel = new Label
            {
                Text = "Note: Changes will take effect after restarting the monitoring service.",
                Location = new System.Drawing.Point(20, 220),
                Size = new System.Drawing.Size(410, 40),
                ForeColor = System.Drawing.Color.Gray
            };
            this.Controls.Add(infoLabel);

            // Save Button
            _saveButton = new Button
            {
                Text = "Save",
                Location = new System.Drawing.Point(260, 270),
                Size = new System.Drawing.Size(80, 30),
                DialogResult = DialogResult.OK
            };
            _saveButton.Click += SaveButton_Click;
            this.Controls.Add(_saveButton);

            // Cancel Button
            _cancelButton = new Button
            {
                Text = "Cancel",
                Location = new System.Drawing.Point(350, 270),
                Size = new System.Drawing.Size(80, 30),
                DialogResult = DialogResult.Cancel
            };
            this.Controls.Add(_cancelButton);

            // Set Accept and Cancel buttons
            this.AcceptButton = _saveButton;
            this.CancelButton = _cancelButton;

            this.ResumeLayout(false);
        }

        private void LoadSettings()
        {
            try
            {
                var config = _configManager.Configuration;

                _enableSoundCheckBox.Checked = config.EnableSound;
                _enableCursorChangeCheckBox.Checked = config.EnableCursorChange;
                _pollingIntervalNumeric.Value = config.PollingIntervalMs;
                _cursorMovementDurationNumeric.Value = config.CursorMovementDurationMs;
                _cursorRestoreDelayNumeric.Value = config.CursorRestoreDelayMs;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to load settings: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void SaveButton_Click(object? sender, EventArgs e)
        {
            try
            {
                var config = _configManager.Configuration;

                // Update configuration with form values
                config.EnableSound = _enableSoundCheckBox.Checked;
                config.EnableCursorChange = _enableCursorChangeCheckBox.Checked;
                config.PollingIntervalMs = (int)_pollingIntervalNumeric.Value;
                config.CursorMovementDurationMs = (int)_cursorMovementDurationNumeric.Value;
                config.CursorRestoreDelayMs = (int)_cursorRestoreDelayNumeric.Value;

                // Save configuration to JSON file
                _configManager.SaveConfiguration();

                MessageBox.Show(
                    "Settings saved successfully!\n\nPlease restart the monitoring service for changes to take effect.",
                    "Settings Saved",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to save settings: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
