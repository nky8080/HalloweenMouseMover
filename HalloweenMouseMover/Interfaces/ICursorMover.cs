using System.Drawing;
using System.Threading.Tasks;

namespace HalloweenMouseMover.Interfaces
{
    public interface ICursorMover
    {
        Task MoveCursorAsync(Point targetPoint, int durationMs = 200);
        bool CanMoveCursor();
    }
}
