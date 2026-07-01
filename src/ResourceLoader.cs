using System;
using System.IO;
using System.Text;
using BepInEx;
using BepInEx.Logging;
using TMPro;
using UnityEngine;
using WKLocalizationLoader.FontFactory;

namespace WKLocalizationLoader
{
    public static class ResourceLoader
    {
        private static Plugin _plugin;
        private static ManualLogSource _logger;

        public static void Initialize(Plugin plugin)
        {
            if (plugin is null) return;
            _plugin = plugin;
            var loggerName = _plugin.Info.Metadata.Name + "/ResourceLoader";
            _logger = BepInEx.Logging.Logger.CreateLogSource(loggerName);
        }

        public static bool TryGetOrCreateFont(
            string characters,
            FontProperties fontProperties,
            out Font font,
            bool isDiskCacheEnabled = false
        )
        {
            var hash = HashCalculator.GetHashString(
                characters,
                fontProperties
            );
            font = CacheManager.GetFontFromMemoryCache(hash);
            if (font != null) return true;
            var canWriteDiskCache = isDiskCacheEnabled;
            if (isDiskCacheEnabled)
            {
                font = CacheManager.CreateFontFromDiskCache(hash);
                canWriteDiskCache = font is null;
            }
            font ??= CreateFont(hash, characters, fontProperties);
            canWriteDiskCache = canWriteDiskCache && font != null;
            font ??= FontBuilder.CreateFontFromOSFont(fontProperties);
            if (font is null) return false;
            if (canWriteDiskCache)
            {
                CacheManager.WriteFontDiskCache(hash, font);
            }
            CacheManager.AddFontToMemoryCache(hash, font);
            return true;
        }

        public static Font CreateFont(
            string hash,
            string characters,
            FontProperties fontProperties
        )
        {
            try
            {
                if (
                    FileManager.TryGetFontFilePath(
                        fontProperties.FileName,
                        out string filePath
                    )
                )
                {
                    if (string.IsNullOrEmpty(fontProperties.FontName))
                    {
                        fontProperties.FontName = "SubstituteFont - "
                            + Path.GetFileNameWithoutExtension(filePath);
                    }
                    return FontBuilder.CreateFont(
                        filePath,
                        characters,
                        fontProperties,
                        _logger
                    );
                }
                return null;
            }
            catch (Exception e)
            {
                _logger?.LogError(
                    $"An error occurred while creating Font (hash: {hash})."
                );
                _logger?.LogError(e.Message);
                return null;
            }
        }

        public static bool TryGetOrCreateFontAsset(
            string characters,
            FontAssetProperties fontAssetProperties,
            out TMP_FontAsset fontAsset,
            bool isDiskCacheEnabled = false
        )
        {
            var hash = HashCalculator.GetHashString(
                characters,
                fontAssetProperties
            );
            fontAsset = CacheManager.GetFontAssetFromMemoryCache(hash);
            if (fontAsset != null) return true;
            var canWriteDiskCache = isDiskCacheEnabled;
            if (isDiskCacheEnabled)
            {
                fontAsset = CacheManager.CreateFontAssetFromDiskCache(hash);
                canWriteDiskCache = fontAsset is null;
            }
            fontAsset ??= CreateFontAsset(
                hash,
                characters,
                fontAssetProperties
            );
            if (fontAsset is null) return false;
            if (canWriteDiskCache)
            {
                CacheManager.WriteFontAssetDiskCache(hash, fontAsset);
            }
            CacheManager.AddFontAssetToMemoryCache(hash, fontAsset);
            return true;
        }

        public static TMP_FontAsset CreateFontAsset(
            string hash,
            string characters,
            FontAssetProperties fontAssetProperties
        )
        {
            try
            {
                if (
                    FileManager.TryGetFontFilePath(
                        fontAssetProperties.FileName,
                        out string filePath
                    )
                )
                {
                    if (string.IsNullOrEmpty(fontAssetProperties.FontName))
                    {
                        fontAssetProperties.FontName = "FallbackFontAsset - "
                            + Path.GetFileNameWithoutExtension(filePath);
                    }
                    return FontAssetBuilder.CreateFontAsset(
                        filePath,
                        characters,
                        fontAssetProperties,
                        _logger
                    );
                }
                return null;
            }
            catch (Exception e)
            {
                _logger?.LogError(
                    "An error occurred while creating FontAsset "
                    + $"(hash: {hash})."
                );
                _logger.LogError(e.Message);
                return null;
            }
        }
    }
}

