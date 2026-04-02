using System;
using System.IO;
using System.Diagnostics;

namespace RobloxMultiLauncher.Core
{
    public static class AppPaths
    {
        public static readonly string BasePath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RobloxMultiLauncher");

        public static readonly string Accounts =
            Path.Combine(BasePath, "accounts.json");

        public static readonly string Settings =
            Path.Combine(BasePath, "settings.json");

        public static void Init()
        {
            // Create data directory if it doesn't exist
            if (!Directory.Exists(BasePath))
            {
                Directory.CreateDirectory(BasePath);
            }

            // Migration logic: move existing accounts.json and settings.json if they exist in the root folder
            MigrateFile("accounts.json", Accounts);
            MigrateFile("settings.json", Settings);
        }

        private static void MigrateFile(string oldFileName, string newPath)
        {
            try
            {
                // Current executable directory
                string oldPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, oldFileName);

                // If old file exists but new one doesn't, move it
                if (File.Exists(oldPath) && !File.Exists(newPath))
                {
                    File.Move(oldPath, newPath);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to migrate {oldFileName}: {ex.Message}");
            }
        }

        public static void OpenDataFolder()
        {
            try
            {
                if (Directory.Exists(BasePath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = BasePath,
                        UseShellExecute = true,
                        Verb = "open"
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to open data folder: {ex.Message}");
            }
        }
    }
}
