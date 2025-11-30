using System.Collections.Generic;

namespace HalloweenMouseMover.Models
{
    public class AppConfiguration
    {
        public int PollingIntervalMs { get; set; } = 50;
        public int CursorMovementDurationMs { get; set; } = 200;
        public int CursorRestoreDelayMs { get; set; } = 2000;
        public string HalloweenSoundPath { get; set; } = "Scream03.mp3";
        public string HorrorCursorPath { get; set; } = "horror_mouse.cur";
        public bool EnableSound { get; set; } = true;
        public bool EnableCursorChange { get; set; } = true;
        public List<string> NegativeButtonPatterns { get; set; } = new List<string>
        {
            "No", "Cancel", "Close", "キャンセル", "いいえ", "閉じる"
        };
    }
}
