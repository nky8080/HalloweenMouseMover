using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using HalloweenMouseMover.Interfaces;
using HalloweenMouseMover.Utils;

namespace HalloweenMouseMover.Services
{
    public class CursorMover : ICursorMover
    {
        // P/Invoke declarations for cursor manipulation
        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        /// <summary>
        /// Checks if the cursor can be moved (tests for security restrictions)
        /// </summary>
        public bool CanMoveCursor()
        {
            try
            {
                // Try to get current cursor position
                if (!GetCursorPos(out POINT currentPos))
                {
                    return false;
                }

                // Try to set cursor to its current position (no-op test)
                return SetCursorPos(currentPos.X, currentPos.Y);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Moves the cursor smoothly to the target point over the specified duration
        /// </summary>
        public async Task MoveCursorAsync(Point targetPoint, int durationMs = 200)
        {
            if (!CanMoveCursor())
            {
                throw new UnauthorizedAccessException("Cursor movement is blocked by security restrictions");
            }

            // Get current cursor position
            if (!GetCursorPos(out POINT startPos))
            {
                throw new InvalidOperationException("Failed to get current cursor position");
            }

            Point startPoint = new Point(startPos.X, startPos.Y);
            
            // Create trajectory calculator
            var trajectory = new CursorTrajectory(startPoint, targetPoint, durationMs);
            
            // Create cancellation token source for potential cancellation
            using var cts = new CancellationTokenSource();
            
            try
            {
                await trajectory.AnimateAsync(SetCursorPos, cts.Token);
            }
            catch (OperationCanceledException)
            {
                // Movement was cancelled (e.g., dialog closed)
                // This is expected behavior, no need to throw
            }
        }
    }
}
