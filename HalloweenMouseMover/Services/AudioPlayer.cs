using System;
using System.IO;
using System.Media;
using System.Threading.Tasks;
using NAudio.Wave;
using HalloweenMouseMover.Interfaces;

namespace HalloweenMouseMover.Services
{
    public class AudioPlayer : IAudioPlayer, IDisposable
    {
        private SoundPlayer? _wavPlayer;
        private WaveOutEvent? _mp3Player;
        private AudioFileReader? _mp3Reader;
        private bool _isPlaying;
        private readonly object _lock = new object();

        public void PlaySound(string soundFilePath)
        {
            Task.Run(() => PlaySoundAsync(soundFilePath));
        }

        private async Task PlaySoundAsync(string soundFilePath)
        {
            try
            {
                if (!File.Exists(soundFilePath))
                {
                    Console.WriteLine($"Audio file not found: {soundFilePath}");
                    return;
                }

                lock (_lock)
                {
                    if (_isPlaying)
                    {
                        StopSound();
                    }
                    _isPlaying = true;
                }

                string extension = Path.GetExtension(soundFilePath).ToLowerInvariant();

                if (extension == ".wav")
                {
                    await PlayWavAsync(soundFilePath);
                }
                else if (extension == ".mp3")
                {
                    await PlayMp3Async(soundFilePath);
                }
                else
                {
                    Console.WriteLine($"Unsupported audio format: {extension}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error playing audio: {ex.Message}");
            }
            finally
            {
                lock (_lock)
                {
                    _isPlaying = false;
                }
            }
        }

        private Task PlayWavAsync(string filePath)
        {
            return Task.Run(() =>
            {
                try
                {
                    lock (_lock)
                    {
                        _wavPlayer?.Dispose();
                        _wavPlayer = new SoundPlayer(filePath);
                        _wavPlayer.Play();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error playing WAV file: {ex.Message}");
                }
            });
        }

        private Task PlayMp3Async(string filePath)
        {
            return Task.Run(() =>
            {
                try
                {
                    lock (_lock)
                    {
                        _mp3Reader?.Dispose();
                        _mp3Player?.Dispose();

                        _mp3Reader = new AudioFileReader(filePath);
                        _mp3Player = new WaveOutEvent();
                        _mp3Player.Init(_mp3Reader);
                        _mp3Player.Play();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error playing MP3 file: {ex.Message}");
                }
            });
        }

        public void StopSound()
        {
            lock (_lock)
            {
                try
                {
                    _wavPlayer?.Stop();
                    _mp3Player?.Stop();
                    _isPlaying = false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error stopping audio: {ex.Message}");
                }
            }
        }

        public void Dispose()
        {
            StopSound();
            _wavPlayer?.Dispose();
            _mp3Player?.Dispose();
            _mp3Reader?.Dispose();
        }
    }
}
