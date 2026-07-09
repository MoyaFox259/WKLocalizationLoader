using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.TextCore.LowLevel;

namespace WKLocalizationLoader.FontFactory
{
    public static class FontBuilder
    {
        public static Font CreateFont(
            string filePath,
            string characters,
            FontProperties fontProperties,
            ManualLogSource logger = null
        )
        {
            var (
                fontName,
                pointSize,
                verticalOffset,
                atlasWidth,
                atlasHeight,
                atlasPadding,
                shaderName,
                glyphLoadFlags,
                textureFilterMode,
                glyphPackingMode,
                atlasRenderMode
            )
            = fontProperties;
            return FontBuilder.CreateFont(
                filePath,
                fontName,
                characters,
                pointSize,
                verticalOffset,
                atlasWidth,
                atlasHeight,
                atlasPadding,
                shaderName,
                glyphLoadFlags,
                textureFilterMode,
                glyphPackingMode,
                atlasRenderMode,
                logger
            );
        }

        public static Font CreateFont(
            string filePath,
            string fontName,
            string characters,
            int pointSize,
            float verticalOffset,
            int atlasWidth,
            int atlasHeight,
            int atlasPadding,
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
            var faceInfo = FontEngine.GetFaceInfo();
            verticalOffset += faceInfo.descentLine;
            logger?.LogInfo("[3/7] Collecting Glyphs.");
            var glyphInfos = CollectGlyphInfos(characters, glyphLoadFlags);
            if (glyphInfos.Count == 0)
            {
                logger?.LogError("Failed to collect any Glyphs.");
                return null;
            }
            logger?.LogInfo("[4/7] Rendering Glyphs to Atlas.");
            var atlas = RenderGlyphsToAtlas(
                glyphInfos,
                pointSize,
                verticalOffset,
                atlasWidth,
                atlasHeight,
                atlasPadding,
                textureFilterMode,
                glyphPackingMode,
                atlasRenderMode,
                out int renderedCount,
                out List<CharacterInfo> characterInfos
            );
            if (
                atlas is null
                || renderedCount == 0
                || characterInfos.Count == 0
            )
            {
                logger?.LogError("Failed to render any Glyphs to Atlas.");
                return null;
            }
            var font = new Font();
            logger?.LogInfo("[5/7] Populating CharacterInfos.");
            font.characterInfo = characterInfos.ToArray();
            logger?.LogInfo("[6/7] Adding Material.");
            AddMaterial(font, atlas, shaderName);
            logger?.LogInfo("[7/7] Finalizing.");
            font.name = fontName;
            FontEngine.UnloadFontFace();
            return font;
        }

        public static Font CreateFontFromDiskCache(
            string cacheDataPath,
            string atlasPath,
            JsonSerializerSettings jsonSerializerSettings,
            ManualLogSource logger = null
        )
        {
            if (
                string.IsNullOrWhiteSpace(cacheDataPath)
                || string.IsNullOrWhiteSpace(atlasPath)
                || jsonSerializerSettings is null
            )
            {
                return null;
            }
            logger?.LogInfo("[1/5] Loading CachedFontData.");
            var jsonText = File.ReadAllText(cacheDataPath, Encoding.UTF8);
            var (
                fontName,
                atlasWidth,
                atlasHeight,
                textureFilterMode,
                shaderName,
                characterInfos
            )
            = JsonConvert.DeserializeObject<CachedFontData>(
                jsonText,
                jsonSerializerSettings
            );
            logger?.LogInfo("[2/5] Loading cached Atlas.");
            var atlas = LoadAtlasFromDisk(
                atlasPath,
                atlasWidth,
                atlasHeight,
                textureFilterMode,
                logger
            );
            if (atlas is null)
            {
                logger?.LogError("Failed to load cached Atlas.");
                return null;
            }
            var font = new Font();
            logger?.LogInfo("[3/5] Populating CharacterInfos.");
            font.characterInfo = characterInfos;
            logger?.LogInfo("[4/5] Adding Material.");
            AddMaterial(font, atlas, shaderName);
            logger?.LogInfo("[5/5] Finalizing.");
            font.name = fontName;
            return font;
        }

        public static Font CreateFontFromOSFont(
            FontProperties fontProperties
        )
        => string.IsNullOrEmpty(fontProperties.DefaultOSFont)
            ? null
            : Font.CreateDynamicFontFromOSFont(
                fontProperties.DefaultOSFont,
                12
            );

        public static void WriteFontDiskCache(
            string cacheFolder,
            Font font,
            JsonSerializerSettings jsonSerializerSettings,
            ManualLogSource logger = null
        )
        {
            if (
                string.IsNullOrWhiteSpace(cacheFolder)
                || font is null
                || jsonSerializerSettings is null
            )
            {
                return;
            }
            logger?.LogInfo("[1/3] Creating CacheFolder.");
            Directory.CreateDirectory(cacheFolder);
            logger?.LogInfo("[2/3] Writing CachedFontData.");
            var cachedFontData = new CachedFontData(font);
            var jsonText = JsonConvert.SerializeObject(
                cachedFontData,
                jsonSerializerSettings
            );
            var cacheDataPath = Path.Combine(
                cacheFolder,
                "CachedFontData.json"
            );
            File.WriteAllText(cacheDataPath, jsonText);
            logger?.LogInfo("[3/3] Writing Atlas cache.");
            var atlasTexture = font.material.mainTexture as Texture2D;
            var atlas = atlasTexture.GetRawTextureData();
            var atlasPath = Path.Combine(
                cacheFolder,
                "RawAtlasTextureData"
            );
            File.WriteAllBytes(atlasPath, atlas);
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

        public static Texture2D RenderGlyphsToAtlas(
            List<GlyphInfo> glyphInfos,
            int pointSize,
            float verticalOffset,
            int atlasWidth,
            int atlasHeight,
            int atlasPadding,
            FilterMode textureFilterMode,
            GlyphPackingMode glyphPackingMode,
            GlyphRenderMode atlasRenderMode,
            out int renderedCount,
            out List<CharacterInfo> characterInfos
        )
        {
            renderedCount = 0;
            characterInfos = new List<CharacterInfo>();
            if (glyphInfos is null || glyphInfos.Count == 0)
            {
                return null;
            }
            var atlas = new Texture2D(
                atlasWidth,
                atlasHeight,
                TextureFormat.Alpha8,
                false
            );
            atlas.filterMode = textureFilterMode;
            FontEngine.ResetAtlasTexture(atlas);
            FontEngine.SetFaceSize(pointSize);
            var freeGlyphRects = new List<GlyphRect>()
            {
                new GlyphRect(0, 0, atlasWidth, atlasHeight)
            };
            var usedGlyphRects = new List<GlyphRect>();
            foreach (var glyphInfo in glyphInfos)
            {
                if (
                    FontEngine.TryAddGlyphToTexture(
                        glyphInfo.GlyphInstance.index,
                        atlasPadding,
                        glyphPackingMode,
                        freeGlyphRects,
                        usedGlyphRects,
                        atlasRenderMode,
                        atlas,
                        out Glyph newGlyph
                    )
                )
                {
                    glyphInfo.GlyphInstance = newGlyph;
                    var characterInfo = CreateCharacterInfo(
                        verticalOffset,
                        atlasWidth,
                        atlasHeight,
                        glyphInfo
                    );
                    characterInfos.Add(characterInfo);
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

        public static CharacterInfo CreateCharacterInfo(
            float verticalOffset,
            int atlasWidth,
            int atlasHeight,
            GlyphInfo glyphInfo
        )
        {
            if (glyphInfo is null)
            {
                throw new ArgumentNullException("GlyphInfo is null.");
            }
            var rect = glyphInfo.GlyphInstance.glyphRect;
            var metrics = glyphInfo.GlyphInstance.metrics;
            return CreateCharacterInfo(
                charCode: (int)glyphInfo.CharCode,
                u: (float)rect.x / atlasWidth,
                v: (float)rect.y / atlasHeight,
                u2: (float)(rect.x + rect.width) / atlasWidth,
                v2: (float)(rect.y + rect.height) / atlasHeight,
                minX: (int)metrics.horizontalBearingX,
                maxX: (int)(metrics.horizontalBearingX + metrics.width),
                minY: (int)(
                    metrics.horizontalBearingY
                    - metrics.height
                    + verticalOffset
                ),
                maxY: (int)(metrics.horizontalBearingY + verticalOffset),
                advance: (int)metrics.horizontalAdvance
            );
        }

        public static CharacterInfo CreateCharacterInfo(
            int charCode,
            float u,
            float v,
            float u2,
            float v2,
            int minX,
            int maxX,
            int minY,
            int maxY,
            int advance
        )
        {
            return new CharacterInfo()
            {
                index = charCode,
                uvBottomLeft = new Vector2(u, v),
                uvBottomRight = new Vector2(u2, v),
                uvTopLeft = new Vector2(u, v2),
                uvTopRight = new Vector2(u2, v2),
                minX = minX,
                maxX = maxX,
                minY = minY,
                maxY = maxY,
                advance = advance
            };
        }

        public static void AddMaterial(
            Font font,
            Texture2D atlas,
            string shaderName = ""
        )
        {
            if (string.IsNullOrEmpty(shaderName))
            {
                shaderName = "GUI/Text Shader";
            }
            var shader = Shader.Find(shaderName)
                ?? Shader.Find("GUI/Text Shader")
                ?? Shader.Find("UI/Default");
            var material = new Material(shader);
            material.mainTexture = atlas;
            material.color = Color.white;
            font.material = material;
        }

        public static Texture2D LoadAtlasFromDisk(
            string atlasPath,
            int atlasWidth,
            int atlasHeight,
            FilterMode textureFilterMode,
            ManualLogSource logger = null
        )
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
                return atlas;
            }
            catch (Exception e)
            {
                logger?.LogError(
                    $"An error occurred while loading \"{atlasPath}\"."
                );
                logger?.LogError(e.Message);
                return null;
            }
        }
    }
}

