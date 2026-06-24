using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;

namespace WKLocalizationLoader
{
    public static class LanguageScanner
    {
        public static List<string> Scan(
            int maxRecersionDepth = 5,
            ManualLogSource logger = null
        )
        {
            var languageFolders = new List<string>();
            ScanLanguageFolders(
                Paths.PluginPath,
                maxRecersionDepth,
                0,
                languageFolders,
                logger
            );
            return languageFolders;
        }

        public static void ScanLanguageFolders(
            string directory,
            int maxDepth,
            int currentDepth,
            List<string> results,
            ManualLogSource logger = null
        )
        {
            var files = Directory.EnumerateFiles(
                directory,
                "*",
                SearchOption.TopDirectoryOnly
            );
            foreach (var file in files)
            {
                if (!IsLanguageFolderMarkerFile(file)) continue;
                var relativePath =
                    "." + directory.Substring(Paths.PluginPath.Length);
                if (results.Contains(relativePath)) continue;
                logger?.LogInfo(
                    $"Auto-detected Language Folder \"{relativePath}\"."
                );
                results.Add(relativePath);
            }
            if (currentDepth >= maxDepth) return;
            var folders = Directory.EnumerateDirectories(
                directory,
                "*",
                SearchOption.TopDirectoryOnly
            );
            foreach (var folder in folders)
            {
                ScanLanguageFolders(
                    folder,
                    maxDepth,
                    currentDepth + 1,
                    results,
                    logger
                );
            }
        }

        public static bool IsLanguageFolderMarkerFile(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            return fileName switch
            {
                ".wklocalization" or "HawktuahLoadThis" => true,
                _ => false
            };
        }
    }
}

