using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BepInEx;
using BepInEx.Logging;

namespace WKLocalizationLoader
{
    public static class FileManager
    {
        private static Plugin _plugin;
        private static ManualLogSource _logger;
        private static string _rootFolder;
        private static string _languageFolder;

        public static void Initialize(Plugin plugin)
        {
            _rootFolder = Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location
            );
            if (plugin is null) return;
            _plugin = plugin;
            var loggerName = _plugin.Info.Metadata.Name + "/FileManager";
            _logger = Logger.CreateLogSource(loggerName);
            SetLanguageFolder(_plugin.LanguageFolder);
        }

        public static string RootFolder
        {
            get
            {
                if (_rootFolder is null)
                {
                    _logger?.LogWarning("RootFolder is null.");
                }
                else if (!Directory.Exists(_rootFolder))
                {
                    _logger?.LogWarning(
                        $"RootFolder \"{_rootFolder}\" does not exist."
                    );
                }
                return _rootFolder ?? Path.GetDirectoryName(
                    Assembly.GetExecutingAssembly().Location
                );
            }
        }

        public static string LanguageFolder
        {
            get
            {
                if (_languageFolder is null)
                {
                    _logger?.LogWarning("LanguageFolder is null.");
                }
                else if (!Directory.Exists(_languageFolder))
                {
                    _logger?.LogWarning(
                        $"LanguageFolder \"{_languageFolder}\" does not exist."
                    );
                }
                return _languageFolder ?? RootFolder;
            }
        }

        public static void SetLanguageFolder(string languageFolder)
        {
            _languageFolder = Path.GetFullPath(
                Path.Combine(Paths.PluginPath, languageFolder)
            );
        }

        public static string FontsFolder => Path.Combine(
            LanguageFolder,
            "Fonts"
        );

        public static string CacheFolder => Path.Combine(
            RootFolder,
            "Cache"
        );

        public static string FontCacheFolder => Path.Combine(
            CacheFolder,
            "Fonts"
        );

        public static string FontAssetCacheFolder => Path.Combine(
            CacheFolder,
            "FontAssets"
        );

        public static bool TryGetModuleFilePath(
            string fileName,
            out string filePath
        )
        {
            filePath = null;
            return TryGetExistingFilePath(
                LanguageFolder,
                fileName,
                out filePath
            );
        }

        public static bool TryGetFontFilePath(
            string fileName,
            out string filePath
        )
        {
            filePath = null;
            return TryGetExistingFilePath(
                FontsFolder,
                fileName,
                out filePath
            );
        }

        public static bool TryGetFontCachePaths(
            string hash,
            out string cacheDataPath,
            out string atlasPath
        )
        {
            cacheDataPath = null;
            atlasPath = null;
            if (
                TryGetExistingFolderPath(
                    FontCacheFolder,
                    hash,
                    out string cacheFolder
                )
            )
            {
                return (
                    TryGetExistingFilePath(
                        cacheFolder,
                        "CachedFontData.json",
                        out cacheDataPath
                    )
                    && TryGetExistingFilePath(
                        cacheFolder,
                        "RawAtlasTextureData",
                        out atlasPath
                    )
                );
            }
            return false;
        }

        public static bool TryGetFontAssetCachePaths(
            string hash,
            out string cacheDataPath,
            out IEnumerable<PathMatchResult> atlasPathMatches
        )
        {
            cacheDataPath = null;
            atlasPathMatches = null;
            if (
                TryGetExistingFolderPath(
                    FontAssetCacheFolder,
                    hash,
                    out string cacheFolder
                )
            )
            {
                return (
                    TryGetExistingFilePath(
                        cacheFolder,
                        "CachedFontAssetData.json",
                        out cacheDataPath
                    )
                    && TrySearchFilePaths(
                        cacheFolder,
                        @"^RawAtlasTextureData_(\d+)",
                        out atlasPathMatches
                    )
                );
            }
            return false;
        }

        public static bool TryGetExistingFilePath(
            string folder,
            string fileName,
            out string filePath
        )
        {
            try
            {
                filePath = Path.Combine(folder, fileName);
                return File.Exists(filePath);
            }
            catch (Exception e)
            {
                _logger?.LogError(e.Message);
                filePath = null;
                return false;
            }
        }

        public static bool TryGetExistingFolderPath(
            string parentFolder,
            string folder,
            out string folderPath
        )
        {
            try
            {
                folderPath = Path.Combine(parentFolder, folder);
                return Directory.Exists(folderPath);
            }
            catch (Exception e)
            {
                _logger?.LogError(e.Message);
                folderPath = null;
                return false;
            }
        }

        public static bool TrySearchFilePaths(
            string folder,
            string fileNamePattern,
            out IEnumerable<PathMatchResult> matchResults
        )
        {
            try
            {
                var regex = new Regex(fileNamePattern);
                matchResults = Directory
                    .EnumerateFiles(folder)
                    .Select(
                        f => new PathMatchResult(
                            f,
                            regex.Match(Path.GetFileName(f))
                        )
                    )
                    .Where(p => p.MatchResult.Success);
                return matchResults.Count() > 0;
            }
            catch (Exception e)
            {
                _logger?.LogError(e.Message);
                matchResults = null;
                return false;
            }
        }
    }
}

