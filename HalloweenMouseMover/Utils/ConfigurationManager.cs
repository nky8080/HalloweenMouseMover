using System;
using System.IO;
using System.Text.Json;
using HalloweenMouseMover.Models;

namespace HalloweenMouseMover.Utils
{
    public class ConfigurationManager
    {
        private const string ConfigFileName = "appsettings.json";
        private static ConfigurationManager? _instance;
        private static readonly object _lock = new object();
        private AppConfiguration _configuration;

        private ConfigurationManager()
        {
            _configuration = LoadConfiguration();
        }

        public static ConfigurationManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new ConfigurationManager();
                        }
                    }
                }
                return _instance;
            }
        }

        public AppConfiguration Configuration => _configuration;

        private AppConfiguration LoadConfiguration()
        {
            try
            {
                string configPath = GetConfigFilePath();
                
                if (File.Exists(configPath))
                {
                    string jsonContent = File.ReadAllText(configPath);
                    var loadedConfig = JsonSerializer.Deserialize<AppConfiguration>(jsonContent);
                    
                    if (loadedConfig != null)
                    {
                        return loadedConfig;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error but continue with defaults
                Console.WriteLine($"Failed to load configuration: {ex.Message}");
            }

            // Return default configuration if file doesn't exist or loading fails
            return new AppConfiguration();
        }

        public void SaveConfiguration()
        {
            try
            {
                string configPath = GetConfigFilePath();
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                
                string jsonContent = JsonSerializer.Serialize(_configuration, options);
                File.WriteAllText(configPath, jsonContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save configuration: {ex.Message}");
                throw;
            }
        }

        public void ReloadConfiguration()
        {
            _configuration = LoadConfiguration();
        }

        private string GetConfigFilePath()
        {
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(appDirectory, ConfigFileName);
        }
    }
}
