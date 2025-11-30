using System;
using System.Windows.Forms;
using HalloweenMouseMover.Interfaces;
using HalloweenMouseMover.Services;

namespace HalloweenMouseMover
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            try
            {
                // Initialize all services
                IDialogMonitor dialogMonitor = new DialogMonitorService(pollingIntervalMs: 50);
                IButtonDetector buttonDetector = new ButtonDetector();
                ICursorMover cursorMover = new CursorMover();
                
                // Initialize resource managers
                IAudioPlayer audioPlayer = new AudioPlayer();
                ICursorManager cursorManager = new CursorManager();
                ResourceManager resourceManager = new ResourceManager(audioPlayer, cursorManager);
                
                // Initialize application controller
                IApplicationController controller = new ApplicationController(
                    dialogMonitor,
                    buttonDetector,
                    cursorMover,
                    resourceManager
                );
                
                // Create and run main form (hidden, system tray only)
                using (var mainForm = new MainForm(controller))
                {
                    Application.Run(mainForm);
                }
                
                // Clean up
                controller.Exit();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to start Halloween Mouse Mover:\n\n{ex.Message}",
                    "Startup Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}
