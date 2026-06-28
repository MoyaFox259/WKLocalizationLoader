using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.TextCore.LowLevel;
using TMPro;

namespace WKLocalizationLoader.FontFactory
{
    public static class FontAssetBuilder
    {
        public static TMP_FontAsset CreateFontAsset(
            string filePath,
            string characters,
            FontAssetProperties fontAssetProperties,
            ManualLogSource logger = null
        )
        {
            var (
                fileName,
                fontName,
                fontVersion,
                scale,
                ascentLineOffset,
                descentLineOffset,
                pointSize,
                atlasWidth,
                atlasHeight,
                atlasPadding,
                singleAtlas,
                shaderName,
                glyphLoadFlags,
                textureFilterMode,
                glyphPackingMode,
                atlasRenderMode
            )
            = fontAssetProperties;
            return FontAssetBuilder.CreateFontAsset(
                filePath,
                fontName,
                fontVersion,
                characters,
                scale,
                ascentLineOffset,
                descentLineOffset,
                pointSize,
                atlasWidth,
                atlasHeight,
                atlasPadding,
                singleAtlas,
                shaderName,
                glyphLoadFlags,
                textureFilterMode,
                glyphPackingMode,
                atlasRenderMode,
                logger
            );
        }

        public static TMP_FontAsset CreateFontAsset(
            string filePath,
            string fontName,
            string fontVersion,
            string characters,
            float scale,
            float ascentLineOffset,
            float descentLineOffset,
            int pointSize,
            int atlasWidth,
            int atlasHeight,
            int atlasPadding,
            bool singleAtlas,
            string shaderName,
            GlyphLoadFlags glyphLoadFlags,
            FilterMode textureFilterMode,
            GlyphPackingMode glyphPackingMode,
            GlyphRenderMode atlasRenderMode,
            ManualLogSource logger = null
        )
        {
            logger?.LogInfo("[1/7] Initializing FontEngine.");
            if (FontEngine.InitializeFontEngine() != FontEngineError.Success)
            {
                logger?.LogError("Failed to initialize FontEngine.");
                return null;
            }
            logger?.LogInfo("[2/7] Loading FontFace.");
            if (
                FontEngine.LoadFontFace(
                    filePath,
                    pointSize
                )
                != FontEngineError.Success
            )
            {
                logger?.LogError("Failed to load FontFace.");
                return null;
            }
            var fontAsset = ScriptableObject.CreateInstance<TMP_FontAsset>();
            var faceInfo = FontEngine.GetFaceInfo();
            faceInfo.scale = scale;
            faceInfo.ascentLine += ascentLineOffset;
            faceInfo.descentLine += descentLineOffset;
            faceInfo.lineHeight = faceInfo.ascentLine - faceInfo.descentLine;
            fontAsset.faceInfo = faceInfo;
            fontAsset.atlasWidth = atlasWidth;
            fontAsset.atlasHeight = atlasHeight;
            fontAsset.atlasPadding = atlasPadding;
            fontAsset.atlasRenderMode = atlasRenderMode;
            logger?.LogInfo("[3/7] Collecting Glyphs.");
            var glyphInfos = CollectGlyphInfos(characters, glyphLoadFlags);
            if (glyphInfos.Count == 0)
            {
                logger?.LogError("Failed to collect any Glyphs.");
                return null;
            }
            logger?.LogInfo("[4/7] Rendering Glyphs to Atlas(es).");
            var atlases = new List<Texture2D>();
            if (singleAtlas)
            {
                var atlas = RenderGlyphsToAtlas(
                    fontAsset,
                    glyphInfos,
                    textureFilterMode,
                    glyphPackingMode,
                    out int renderedCount
                );
                if (atlas is null || renderedCount == 0)
                {
                    logger?.LogError(
                        "Failed to render any Glyphs to a single Atlas."
                    );
                    return null;
                }
                atlases.Add(atlas);
            }
            else
            {
                atlases = RenderGlyphsToAtlases(
                    fontAsset,
                    glyphInfos,
                    textureFilterMode,
                    glyphPackingMode,
                    out int totalRenderedCount
                );
                if (atlases is null || totalRenderedCount == 0)
                {
                    logger?.LogError(
                        "Failed to render any Glyphs to multiple Atlases."
                    );
                    return null;
                }
            }
            fontAsset.isMultiAtlasTexturesEnabled = atlases.Count > 1;
            fontAsset.atlasTextures = atlases.ToArray();
            logger?.LogInfo("[5/7] Initializing DictionaryLookupTables.");
            fontAsset.ReadFontAssetDefinition();
            logger?.LogInfo("[6/7] Adding Material.");
            AddMaterial(fontAsset, shaderName);
            logger?.LogInfo("[7/7] Finalizing.");
            fontAsset.atlasPopulationMode = AtlasPopulationMode.Static;
            fontAsset.name = fontName;
            fontAsset.version = fontVersion;
            FontEngine.UnloadFontFace();
            return fontAsset;
        }

        public static TMP_FontAsset CreateFontAssetFromDiskCache(
            string cacheDataPath,
            List<string> atlasPaths,
            JsonSerializerSettings jsonSerializerSettings,
            ManualLogSource logger = null
        )
        {
            if (
                string.IsNullOrWhiteSpace(cacheDataPath)
                || atlasPaths.Any(
                    p => string.IsNullOrWhiteSpace(p)
                )
                || jsonSerializerSettings is null
            )
            {
                return null;
            }
            logger?.LogInfo("[1/6] Loading CachedFontAssetData.");
            var jsonText = File.ReadAllText(cacheDataPath, Encoding.UTF8);
            var (
                fontName,
                fontVersion,
                atlasWidth,
                atlasHeight,
                atlasPadding,
                textureFilterMode,
                atlasRenderMode,
                shaderName,
                faceInfo,
                glyphTable,
                characterTable
            )
            = JsonConvert.DeserializeObject<CachedFontAssetData>(
                jsonText,
                jsonSerializerSettings
            );
            var fontAsset = ScriptableObject.CreateInstance<TMP_FontAsset>();
            logger?.LogInfo("[2/6] Loading cached FaceInfo.");
            fontAsset.faceInfo = faceInfo;
            fontAsset.atlasWidth = atlasWidth;
            fontAsset.atlasHeight = atlasHeight;
            fontAsset.atlasPadding = atlasPadding;
            fontAsset.atlasRenderMode = atlasRenderMode;
            logger?.LogInfo("[3/6] Loading cached Atlas(es).");
            var atlases = LoadAtlasesFromDisk(
                atlasPaths,
                atlasWidth,
                atlasHeight,
                textureFilterMode,
                logger
            );
            if (atlases is null || atlases.Count == 0)
            {
                logger?.LogError("Failed to load cached Atlas(es).");
                return null;
            }
            fontAsset.isMultiAtlasTexturesEnabled = atlases.Count > 1;
            fontAsset.atlasTextures = atlases.ToArray();
            logger?.LogInfo("[4/6] Initializing DictionaryLookupTables.");
            fontAsset.glyphTable = glyphTable;
            fontAsset.characterTable = characterTable;
            fontAsset.ReadFontAssetDefinition();
            logger?.LogInfo("[5/6] Adding Material.");
            AddMaterial(fontAsset, shaderName);
            logger?.LogInfo("[6/6] Finalizing.");
            fontAsset.atlasPopulationMode = AtlasPopulationMode.Static;
            fontAsset.name = fontName;
            fontAsset.version = fontVersion;
            return fontAsset;
        }

        // public static TMP_FontAsset CreateFontAssetFromOSFont(
        //     FontAssetProperties fontAssetProperties
        // )
        // {
        //     if (string.IsNullOrEmpty(fontAssetProperties.DefaultOSFont))
        //     {
        //         return null;
        //     }
        //     return CreateFontAssetFromOSFont(
        //         fontAssetProperties.DefaultOSFont,
        //         fontAssetProperties.PointSize,
        //         fontAssetProperties.AtlasPadding,
        //         fontAssetProperties.AtlasRenderMode,
        //         fontAssetProperties.AtlasWidth,
        //         fontAssetProperties.AtlasHeight
        //     );
        // }

        // public static TMP_FontAsset CreateFontAssetFromOSFont(
        //     string installedFontName,
        //     int pointSize,
        //     int atlasPadding,
        //     GlyphRenderMode atlasRenderMode,
        //     int atlasWidth,
        //     int atlasHeight
        // )
        // {
        //     var font = Font.CreateDynamicFontFromOSFont(
        //         installedFontName,
        //         12
        //     );
        //     if (font != null)
        //     {
        //         Plugin.Logger?.LogWarning(
        //             "Successfully created OS Font."
        //         );
        //     }
        //     return TMP_FontAsset.CreateFontAsset(
        //         font,
        //         pointSize,
        //         atlasPadding,
        //         atlasRenderMode,
        //         atlasWidth,
        //         atlasHeight
        //     );
        // }

        public static void WriteFontAssetDiskCache(
            string cacheFolder,
            TMP_FontAsset fontAsset,
            JsonSerializerSettings jsonSerializerSettings,
            ManualLogSource logger = null
        )
        {
            if (
                string.IsNullOrWhiteSpace(cacheFolder)
                || fontAsset is null
                || jsonSerializerSettings is null
            )
            {
                return;
            }
            logger?.LogInfo("[1/3] Creating CacheFolder.");
            Directory.CreateDirectory(cacheFolder);
            logger?.LogInfo("[2/3] Writing CachedFontAssetData.");
            var cachedFontAssetData = new CachedFontAssetData(fontAsset);
            var jsonText = JsonConvert.SerializeObject(
                cachedFontAssetData,
                jsonSerializerSettings
            );
            var cacheDataPath = Path.Combine(
                cacheFolder,
                "CachedFontAssetData.json"
            );
            File.WriteAllText(cacheDataPath, jsonText);
            logger?.LogInfo("[3/3] Writing Atlas(es) cache.");
            for (
                int atlasIndex = 0;
                atlasIndex < fontAsset.atlasTextures.Length;
                atlasIndex++
            )
            {
                var atlas = fontAsset
                    .atlasTextures[atlasIndex]
                    .GetRawTextureData();
                var atlasPath = Path.Combine(
                    cacheFolder,
                    $"RawAtlasTextureData_{atlasIndex}"
                );
                File.WriteAllBytes(atlasPath, atlas);
            }
        }

        public static List<GlyphInfo> CollectGlyphInfos(
            string characters,
            GlyphLoadFlags glyphLoadFlags
        )
        {
            var glyphInfos = new List<GlyphInfo>();
            if (string.IsNullOrEmpty(characters))
            {
                return glyphInfos;
            }
            var charCodes = characters
                .Distinct()
                .Select(c => (uint)c)
                .ToList();
            foreach (var charCode in charCodes)
            {
                if (
                    FontEngine.TryGetGlyphWithUnicodeValue(
                        charCode,
                        glyphLoadFlags,
                        out Glyph glyph
                    )
                )
                {
                    glyphInfos.Add(new GlyphInfo(charCode, glyph));
                }
            }
            return glyphInfos;
        }

        public static List<Texture2D> RenderGlyphsToAtlases(
            TMP_FontAsset fontAsset,
            List<GlyphInfo> glyphInfos,
            FilterMode textureFilterMode,
            GlyphPackingMode glyphPackingMode,
            out int totalRenderedCount
        )
        {
            totalRenderedCount = 0;
            if (glyphInfos is null || glyphInfos.Count == 0)
            {
                return null;
            }
            var atlases = new List<Texture2D>();
            var glyphInfoIndex = 0;
            while (glyphInfoIndex < glyphInfos.Count)
            {
                var atlas = RenderGlyphsToAtlas(
                    fontAsset,
                    glyphInfos,
                    textureFilterMode,
                    glyphPackingMode,
                    out int renderedCount,
                    glyphInfoIndex,
                    atlases.Count
                );
                if (atlas is null || renderedCount == 0) break;
                atlases.Add(atlas);
                totalRenderedCount += renderedCount;
                glyphInfoIndex += renderedCount;
            }
            return totalRenderedCount > 0 ? atlases : null;
        }

        public static Texture2D RenderGlyphsToAtlas(
            TMP_FontAsset fontAsset,
            List<GlyphInfo> glyphInfos,
            FilterMode textureFilterMode,
            GlyphPackingMode glyphPackingMode,
            out int renderedCount,
            int startGlyphInfoIndex = 0,
            int atlasIndex = 0
        )
        {
            renderedCount = 0;
            if (glyphInfos is null || glyphInfos.Count == 0)
            {
                return null;
            }
            startGlyphInfoIndex = Math.Min(
                glyphInfos.Count - 1,
                startGlyphInfoIndex
            );
            var atlas = new Texture2D(
                fontAsset.atlasWidth,
                fontAsset.atlasHeight,
                TextureFormat.Alpha8,
                false
            );
            atlas.filterMode = textureFilterMode;
            FontEngine.ResetAtlasTexture(atlas);
            FontEngine.SetFaceSize(fontAsset.faceInfo.pointSize);
            var freeGlyphRects = new List<GlyphRect>()
            {
                new GlyphRect(0, 0, atlas.width, atlas.height)
            };
            var usedGlyphRects = new List<GlyphRect>();
            for (
                int glyphInfoIndex = startGlyphInfoIndex;
                glyphInfoIndex < glyphInfos.Count;
                glyphInfoIndex++
            )
            {
                var glyphInfo = glyphInfos[glyphInfoIndex];
                if (
                    FontEngine.TryAddGlyphToTexture(
                        glyphInfo.GlyphInstance.index,
                        fontAsset.atlasPadding,
                        glyphPackingMode,
                        freeGlyphRects,
                        usedGlyphRects,
                        fontAsset.atlasRenderMode,
                        atlas,
                        out Glyph newGlyph
                    )
                )
                {
                    glyphInfo.GlyphInstance = newGlyph;
                    AppendGlyphTable(
                        fontAsset,
                        glyphInfo.GlyphInstance,
                        atlasIndex
                    );
                    AppendCharacterTable(
                        fontAsset,
                        glyphInfo.CharCode,
                        glyphInfo.GlyphInstance
                    );
                    renderedCount++;
                }
                else
                {
                    break;
                }
            }
            atlas.Apply();
            return renderedCount > 0 ? atlas : null;
        }

        public static void AppendGlyphTable(
            TMP_FontAsset fontAsset,
            Glyph glyph,
            int atlasIndex
        )
        {
            fontAsset.glyphTable ??= new List<Glyph>();
            if (glyph is null) return;
            var atlasIndexedGlyph = new Glyph(glyph);
            atlasIndexedGlyph.atlasIndex = atlasIndex;
            fontAsset.glyphTable.Add(atlasIndexedGlyph);
        }

        public static void AppendCharacterTable(
            TMP_FontAsset fontAsset,
            uint charCode,
            Glyph glyph
        )
        {
            fontAsset.characterTable ??= new List<TMP_Character>();
            if (glyph is null) return;
            var character = new TMP_Character(charCode, glyph);
            fontAsset.characterTable.Add(character);
        }

        public static void AddMaterial(
            TMP_FontAsset fontAsset,
            string shaderName = ""
        )
        {
            if (string.IsNullOrEmpty(shaderName))
            {
                shaderName = fontAsset.atlasRenderMode switch
                {
                    GlyphRenderMode.SDF
                    or GlyphRenderMode.SDF8
                    or GlyphRenderMode.SDF16
                    or GlyphRenderMode.SDF32
                    or GlyphRenderMode.SDFAA_HINTED
                    or GlyphRenderMode.SDFAA => "TextMeshPro/Distance Field",
                    _ => "TextMeshPro/Bitmap"
                };
            }
            var shader =
                Shader.Find(shaderName) ?? Shader.Find("TextMeshPro/Bitmap");
            var material = new Material(shader);
            material.mainTexture = fontAsset.atlasTexture;
            material.SetFloat(
                ShaderUtilities.ID_TextureWidth,
                (float)fontAsset.atlasWidth
            );
            material.SetFloat(
                ShaderUtilities.ID_TextureHeight,
                (float)fontAsset.atlasHeight
            );
            if (shaderName == "TextMeshPro/Distance Field")
            {
                material.SetFloat(
                    ShaderUtilities.ID_GradientScale,
                    (float)(fontAsset.atlasPadding + 1)
                );
                material.SetFloat(ShaderUtilities.ID_WeightNormal, 0f);
                material.SetFloat(ShaderUtilities.ID_WeightBold, 0f);
            }
            fontAsset.material = material;
            fontAsset.materialHashCode = material.GetHashCode();
        }

        public static List<Texture2D> LoadAtlasesFromDisk(
            List<string> atlasPaths,
            int atlasWidth,
            int atlasHeight,
            FilterMode textureFilterMode,
            ManualLogSource logger = null
        )
        {
            var atlases = new List<Texture2D>();
            foreach (var atlasPath in atlasPaths)
            {
                try
                {
                    var atlasTexture = File.ReadAllBytes(atlasPath);
                    var atlas = new Texture2D(
                        atlasWidth,
                        atlasHeight,
                        TextureFormat.Alpha8,
                        false
                    );
                    atlas.filterMode = textureFilterMode;
                    atlas.LoadRawTextureData(atlasTexture);
                    atlas.Apply();
                    atlases.Add(atlas);
                }
                catch (Exception e)
                {
                    logger?.LogError(
                        $"An error occurred while loading \"{atlasPath}\"."
                    );
                    logger?.LogError(e.Message);
                }
            }
            return atlases.Count > 0 ? atlases : null;
        }
    }
}

