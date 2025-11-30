using System;
using System.IO;

namespace HalloweenMouseMover.Utils
{
    public static class Logger
    {
        private static readonly string LogFilePath;
        private static readonly object LockObject = new object();

        static Logger()
        {
            string logDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "HalloweenMouseMover"
            );
            
            Directory.CreateDirectory(logDirectory);
            LogFilePath = Path.Combine(logDirectory, "debug.log");
        }

        public static void Log(string message)
        {
            try
            {
                lock (LockObject)
                {
                    string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}";
                    File.AppendAllText(LogFilePath, logMessage + Environment.NewLine);
                    System.Diagnostics.Debug.WriteLine(logMessage);
                }
            }
            catch
            {
                // Ignore logging errors
            }
        }

        public static string GetLogFilePath()
        {
            return LogFilePath;
        }

        public static void ClearLog()
        {
            try
            {
                lock (LockObject)
                {
                    File.WriteAllText(LogFilePath, string.Empty);
                }
            }
            catch
            {
                // Ignore errors
            }
        }
    }
}
