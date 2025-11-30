namespace HalloweenMouseMover.Interfaces
{
    public interface ICursorManager
    {
        void SetHorrorCursor(string cursorFilePath);
        void RestoreCursor();
        void RestoreCursorAfterDelay(int delayMs = 2000);
    }
}
