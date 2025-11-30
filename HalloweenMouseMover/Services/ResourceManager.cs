using System;
using System.IO;
using HalloweenMouseMover.Interfaces;
using HalloweenMouseMover.Utils;

namespace HalloweenMouseMover.Services
{
    public class ResourceManager : IDisposable
    {
        private readonly IAudioPlayer _audioPlayer;
        private readonly ICursorManager _cursorManager;
        private readonly ConfigurationManager _configManager;
        private bool _isInitialized;

        public ResourceManager(IAudioPlayer audioPlayer, ICursorManager cursorManager)
        {
            _audioPlayer = audioPlayer ?? throw new ArgumentNullException(nameof(audioPlayer));
            _cursorManager = cursorManager ?? throw new ArgumentNullException(nameof(cursorManager));
            _configManager = ConfigurationManager.Instance;
        }

        public void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }

            try
            {
                var config = _configManager.Configuration;
                
                // Verify sound file exists
                string soundPath = GetResourcePath(config.HalloweenSoundPath);
                if (!File.Exists(soundPath))
                {
                    Console.WriteLine($"Warning: Sound file not found at {soundPath}");
                }
                else
                {
                    Console.WriteLine($"Sound file found: {soundPath}");
                }

                // Verify cursor file exists
                string cursorPath = GetResourcePath(config.HorrorCursorPath);
                if (!File.Exists(cursorPath))
                {
                    Console.WriteLine($"Warning: Cursor file not found at {cursorPath}");
                }
                else
                {
                    Console.WriteLine($"Cursor file found: {cursorPath}");
                }

                _isInitialized = true;
                Console.WriteLine("ResourceManager initialized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing ResourceManager: {ex.Message}");
                throw;
            }
        }

        public void TriggerHalloweenEffects()
        {
            if (!_isInitialized)
            {
                Console.WriteLine("ResourceManager not initialized. Call Initialize() first.");
                return;
            }

            try
            {
                var config = _configManager.Configuration;

                // Play sound if enabled
                if (config.EnableSound)
                {
                    string soundPath = GetResourcePath(config.HalloweenSoundPath);
                    if (File.Exists(soundPath))
                    {
                        _audioPlayer.PlaySound(soundPath);
                    }
                }

                // Change cursor if enabled
                if (config.EnableCursorChange)
                {
                    string cursorPath = GetResourcePath(config.HorrorCursorPath);
                    if (File.Exists(cursorPath))
                    {
                        _cursorManager.SetHorrorCursor(cursorPath);
                        _cursorManager.RestoreCursorAfterDelay(config.CursorRestoreDelayMs);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error triggering Halloween effects: {ex.Message}");
            }
        }

        public void StopEffects()
        {
            try
            {
                _audioPlayer.StopSound();
                _cursorManager.RestoreCursor();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping effects: {ex.Message}");
            }
        }

        private string GetResourcePath(string fileName)
        {
            // First try the application directory
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string fullPath = Path.Combine(appDirectory, fileName);
            
            if (File.Exists(fullPath))
            {
                return fullPath;
            }

            // If not found, return the original path (might be absolute)
            return fileName;
        }

        public void Dispose()
        {
            StopEffects();
            
            if (_audioPlayer is IDisposable disposableAudio)
            {
                disposableAudio.Dispose();
            }
        }
    }
}
