using System;
using System.IO;

namespace Aiplugs.PoshApp
{
    public class StorageHelper
    {
        private static string GetAppPath() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".poshapp");
        public static string GetConfigPath() => Path.Combine(GetAppPath(), "config.json");
        public static string GetScriptDirPath() => Path.Combine(GetAppPath(), "scripts");
        public static string GetScriptPath(string scriptId) => Path.Combine(GetScriptDirPath(), $"{scriptId}.ps1");
        private static void CreateAppDirIfNotExist()
        {
            var appPath = GetAppPath();

            if (!Directory.Exists(appPath)) 
                Directory.CreateDirectory(appPath);
        }
        public static void TouchConfigIfNotExist()
        {
            CreateAppDirIfNotExist();

            var configPath = GetConfigPath();

            if (!File.Exists(configPath))
                File.WriteAllBytes(configPath, new byte[0]);
        }
        public static void CreateScriptsDirIfNotExist()
        {
            var dirPath = GetScriptDirPath();

            if (!Directory.Exists(dirPath)) 
                Directory.CreateDirectory(dirPath);
        }
    }
}