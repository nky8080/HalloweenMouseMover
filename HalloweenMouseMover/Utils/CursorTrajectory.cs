using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace HalloweenMouseMover.Utils
{
    /// <summary>
    /// Calculates and animates smooth cursor movement from start to end point
    /// </summary>
    public class CursorTrajectory
    {
        private readonly Point _startPoint;
        private readonly Point _endPoint;
        private readonly int _durationMs;
        private const int SampleIntervalMs = 10;

        public CursorTrajectory(Point startPoint, Point endPoint, int durationMs)
        {
            _startPoint = startPoint;
            _endPoint = endPoint;
            _durationMs = durationMs;
        }

        /// <summary>
        /// Animates the cursor movement using the provided cursor positioning function
        /// </summary>
        /// <param name="setCursorFunc">Function to set cursor position (x, y) => bool</param>
        /// <param name="cancellationToken">Token to cancel the animation</param>
        public async Task AnimateAsync(Func<int, int, bool> setCursorFunc, CancellationToken cancellationToken)
        {
            int elapsedMs = 0;
            
            while (elapsedMs < _durationMs)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                // Calculate progress (0.0 to 1.0)
                double progress = (double)elapsedMs / _durationMs;
                
                // Apply ease-out cubic easing
                double easedProgress = EaseOutCubic(progress);
                
                // Calculate current position
                Point currentPos = CalculatePosition(easedProgress);
                
                // Set cursor position
                if (!setCursorFunc(currentPos.X, currentPos.Y))
                {
                    throw new InvalidOperationException("Failed to set cursor position");
                }
                
                // Wait for next sample
                await Task.Delay(SampleIntervalMs, cancellationToken);
                elapsedMs += SampleIntervalMs;
            }
            
            // Ensure we end exactly at the target position
            if (!setCursorFunc(_endPoint.X, _endPoint.Y))
            {
                throw new InvalidOperationException("Failed to set final cursor position");
            }
        }

        /// <summary>
        /// Calculates the cursor position at a given progress point
        /// </summary>
        private Point CalculatePosition(double progress)
        {
            int x = (int)(_startPoint.X + (_endPoint.X - _startPoint.X) * progress);
            int y = (int)(_startPoint.Y + (_endPoint.Y - _startPoint.Y) * progress);
            return new Point(x, y);
        }

        /// <summary>
        /// Ease-out cubic easing function for smooth deceleration
        /// Formula: 1 - (1 - t)^3
        /// </summary>
        private double EaseOutCubic(double t)
        {
            double oneMinusT = 1 - t;
            return 1 - (oneMinusT * oneMinusT * oneMinusT);
        }
    }
}
