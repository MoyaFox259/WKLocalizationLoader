using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using BepInEx;
using BepInEx.Logging;
using TMPro;
using UnityEngine;
using WKLocalizationLoader.FontFactory;

namespace WKLocalizationLoader
{
    public static class CacheManager
    {
        private static JsonSerializerSettings _jsonSerializerSettings;
        private static Dictionary<string, Font> _fontCache;
        private static Dictionary<string, TMP_FontAsset> _fontAssetCache;
        private static Plugin _plugin;
        private static ManualLogSource _logger;

        public static void Initialize(Plugin plugin)
        {
            _jsonSerializerSettings = new JsonSerializerSettings()
            {
                ContractResolver = new DefaultContractResolver()
                {
                    NamingStrategy = new DefaultNamingStrategy()
                },
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.None,
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            };
            _fontCache = new Dictionary<string, Font>();
            _fontAssetCache = new Dictionary<string, TMP_FontAsset>();
            if (plugin is null) return;
            _plugin = plugin;
            var loggerName = _plugin.Info.Metadata.Name + "/CacheManager";
            _logger = BepInEx.Logging.Logger.CreateLogSource(loggerName);
        }

        public static void AddFontToMemoryCache(string hash, Font font)
        {
            _fontCache ??= new Dictionary<string, Font>();
            if (string.IsNullOrWhiteSpace(hash) || font is null) return;
            _fontCache[hash] = font;
        }

        public static Font GetFontFromMemoryCache(string hash)
        {
            if (
                !string.IsNullOrWhiteSpace(hash)
                && _fontCache != null
                && _fontCache.TryGetValue(hash, out Font font)
            )
            {
                return font;
            }
            return null;
        }

        public static Font CreateFontFromDiskCache(string hash)
        {
            if (
                string.IsNullOrWhiteSpace(hash)
                || _jsonSerializerSettings is null
            )
            {
                return null;
            }
            Font font = null;
            try
            {
                if (
                    FileManager.TryGetFontCachePaths(
                        hash,
                        out string cacheDataPath,
                        out string atlasPath
                    )
                )
                {
                    font = FontBuilder.CreateFontFromDiskCache(
                        cacheDataPath,
                        atlasPath,
                        _jsonSerializerSettings,
                        _logger
                    );
                }
            }
            catch (Exception e)
            {
                _logger?.LogWarning(
                    "An error occurred while creating Font "
                    + $"(hash: {hash}) from disk cache."
                );
                _logger?.LogWarning(e.Message);
                font = null;
            }
            return font;
        }

        public static void WriteFontDiskCache(string hash, Font font)
        {
            if (
                string.IsNullOrWhiteSpace(hash)
                || font is null
                || _jsonSerializerSettings is null
            )
            {
                return;
            }
            try
            {
                var cacheFolder = Path.Combine(
                    FileManager.FontCacheFolder,
                    hash
                );
                FontBuilder.WriteFontDiskCache(
                    cacheFolder,
                    font,
                    _jsonSerializerSettings,
                    _logger
                );
            }
            catch (Exception e)
            {
                _logger?.LogWarning(
                    "An error occurred while writing Font "
                    + $"(hash: {hash}) cache to disk."
                );
                _logger?.LogWarning(e.Message);
            }
        }

        public static void AddFontAssetToMemoryCache(
            string hash,
            TMP_FontAsset fontAsset
        )
        {
            _fontAssetCache ??= new Dictionary<string, TMP_FontAsset>();
            if (string.IsNullOrWhiteSpace(hash) || fontAsset is null) return;
            _fontAssetCache[hash] = fontAsset;
        }

        public static TMP_FontAsset GetFontAssetFromMemoryCache(string hash)
        {
            if (
                !string.IsNullOrWhiteSpace(hash)
                && _fontAssetCache != null
                && _fontAssetCache.TryGetValue(
                    hash,
                    out TMP_FontAsset fontAsset
                )
            )
            {
                return fontAsset;
            }
            return null;
        }

        public static TMP_FontAsset CreateFontAssetFromDiskCache(string hash)
        {
            if (
                string.IsNullOrWhiteSpace(hash)
                || _jsonSerializerSettings is null
            )
            {
                return null;
            }
            TMP_FontAsset fontAsset = null;
            try
            {
                if (
                    FileManager.TryGetFontAssetCachePaths(
                        hash,
                        out string cacheDataPath,
                        out IEnumerable<PathMatchResult> atlasPathMatches
                    )
                )
                {
                    var atlasPaths = atlasPathMatches
                        .OrderBy(
                            p => int.Parse(p.MatchResult.Groups[1].Value)
                        )
                        .Select(p => p.Path)
                        .ToList();
                    fontAsset = FontAssetBuilder.CreateFontAssetFromDiskCache(
                        cacheDataPath,
                        atlasPaths,
                        _jsonSerializerSettings,
                        _logger
                    );
                }
            }
            catch (Exception e)
            {
                _logger?.LogWarning(
                    "An error occurred while creating FontAsset "
                    + $"(hash: {hash}) from disk cache."
                );
                _logger?.LogWarning(e.Message);
                fontAsset = null;
            }
            return fontAsset;
        }

        public static void WriteFontAssetDiskCache(
            string hash,
            TMP_FontAsset fontAsset
        )
        {
            if (
                string.IsNullOrWhiteSpace(hash)
                || fontAsset is null
                || _jsonSerializerSettings is null
            )
            {
                return;
            }
            try
            {
                var cacheFolder = Path.Combine(
                    FileManager.FontAssetCacheFolder,
                    hash
                );
                FontAssetBuilder.WriteFontAssetDiskCache(
                    cacheFolder,
                    fontAsset,
                    _jsonSerializerSettings,
                    _logger
                );
            }
            catch (Exception e)
            {
                _logger?.LogWarning(
                    "An error occurred while writing FontAsset "
                    + $"(hash: {hash}) cache to disk."
                );
                _logger?.LogWarning(e.Message);
            }
        }
    }
}

