using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using HalloweenMouseMover.Interfaces;

namespace HalloweenMouseMover.Services
{
    public class CursorManager : ICursorManager
    {
        private const int OCR_NORMAL = 32512;
        private const int OCR_IBEAM = 32513;
        private const int OCR_WAIT = 32514;
        private const int OCR_CROSS = 32515;
        private const int OCR_UP = 32516;
        private const int OCR_SIZENWSE = 32642;
        private const int OCR_SIZENESW = 32643;
        private const int OCR_SIZEWE = 32644;
        private const int OCR_SIZENS = 32645;
        private const int OCR_SIZEALL = 32646;
        private const int OCR_NO = 32648;
        private const int OCR_HAND = 32649;
        private const int OCR_APPSTARTING = 32650;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr LoadCursorFromFile(string lpFileName);

        [DllImport("user32.dll")]
        private static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

        [DllImport("user32.dll")]
        private static extern bool SetSystemCursor(IntPtr hcur, uint id);

        [DllImport("user32.dll")]
        private static extern IntPtr CopyIcon(IntPtr hIcon);

        [DllImport("user32.dll")]
        private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, IntPtr pvParam, uint fWinIni);

        private const uint SPI_SETCURSORS = 0x0057;
        private const uint SPIF_UPDATEINIFILE = 0x01;
        private const uint SPIF_SENDCHANGE = 0x02;

        private IntPtr _originalCursor = IntPtr.Zero;
        private Timer? _restoreTimer;
        private readonly object _lock = new object();

        public void SetHorrorCursor(string cursorFilePath)
        {
            try
            {
                if (!File.Exists(cursorFilePath))
                {
                    Console.WriteLine($"Cursor file not found: {cursorFilePath}");
                    return;
                }

                lock (_lock)
                {
                    // Load the horror cursor
                    IntPtr horrorCursor = LoadCursorFromFile(cursorFilePath);
                    
                    if (horrorCursor == IntPtr.Zero)
                    {
                        Console.WriteLine($"Failed to load cursor from file: {cursorFilePath}");
                        return;
                    }

                    // Make a copy of the cursor handle
                    IntPtr cursorCopy = CopyIcon(horrorCursor);
                    
                    if (cursorCopy == IntPtr.Zero)
                    {
                        Console.WriteLine("Failed to copy cursor handle");
                        return;
                    }

                    // Set the system cursor (normal arrow)
                    if (!SetSystemCursor(cursorCopy, OCR_NORMAL))
                    {
                        Console.WriteLine("Failed to set system cursor");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting horror cursor: {ex.Message}");
            }
        }

        public void RestoreCursor()
        {
            try
            {
                lock (_lock)
                {
                    // Cancel any pending restore timer
                    _restoreTimer?.Dispose();
                    _restoreTimer = null;

                    // Restore system cursors by reloading them
                    SystemParametersInfo(SPI_SETCURSORS, 0, IntPtr.Zero, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error restoring cursor: {ex.Message}");
            }
        }

        public void RestoreCursorAfterDelay(int delayMs = 2000)
        {
            lock (_lock)
            {
                // Cancel any existing timer
                _restoreTimer?.Dispose();

                // Create a new timer to restore the cursor after the delay
                _restoreTimer = new Timer(_ => RestoreCursor(), null, delayMs, Timeout.Infinite);
            }
        }
    }
}
