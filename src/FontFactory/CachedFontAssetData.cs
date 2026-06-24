using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.TextCore.LowLevel;
using TMPro;

namespace WKLocalizationLoader.FontFactory
{
    public class CachedFontAssetData
    {
        [JsonProperty]
        public string FontName;
        [JsonProperty]
        public string FontVersion;
        [JsonProperty]
        public int AtlasWidth;
        [JsonProperty]
        public int AtlasHeight;
        [JsonProperty]
        public int AtlasPadding;
        [JsonProperty]
        public FilterMode TextureFilterMode;
        [JsonProperty]
        public GlyphRenderMode AtlasRenderMode;
        [JsonProperty]
        public string ShaderName;
        [JsonProperty]
        public FaceInfo FontFaceInfo;
        [JsonProperty]
        public List<CachedGlyphData> GlyphTable;
        [JsonProperty]
        public List<uint> CharacterUnicodes;

        public CachedFontAssetData()
        {
        }

        public CachedFontAssetData(TMP_FontAsset fontAsset)
        {
            FontName = fontAsset.name;
            FontVersion = fontAsset.version;
            AtlasWidth = fontAsset.atlasWidth;
            AtlasHeight = fontAsset.atlasHeight;
            AtlasPadding = fontAsset.atlasPadding;
            TextureFilterMode = fontAsset.atlasTexture.filterMode;
            AtlasRenderMode = fontAsset.atlasRenderMode;
            ShaderName = fontAsset.material.shader.name;
            FontFaceInfo = fontAsset.faceInfo;
            GlyphTable = fontAsset.glyphTable
                .Select(g => new CachedGlyphData(g))
                .ToList();
            CharacterUnicodes = fontAsset.characterTable
                .Select(c => c.unicode)
                .ToList();
        }

        public void Deconstruct(
            out string fontName,
            out string fontVersion,
            out int atlasWidth,
            out int atlasHeight,
            out int atlasPadding,
            out FilterMode textureFilterMode,
            out GlyphRenderMode atlasRenderMode,
            out string shaderName,
            out FaceInfo faceInfo,
            out List<Glyph> glyphTable,
            out List<TMP_Character> characterTable
        )
        {
            fontName = FontName;
            fontVersion = FontVersion;
            atlasWidth = AtlasWidth;
            atlasHeight = AtlasHeight;
            atlasPadding = AtlasPadding;
            textureFilterMode = TextureFilterMode;
            atlasRenderMode = AtlasRenderMode;
            shaderName = ShaderName;
            faceInfo = FontFaceInfo;
            glyphTable = GlyphTable
                .Select(g => g.ToGlyph())
                .ToList();
            characterTable = glyphTable
                .Select((g, i) => new TMP_Character(CharacterUnicodes[i], g))
                .ToList();
        }
    }
}

