using System;
using System.ComponentModel;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace WKLocalizationLoader.FontFactory
{
    public class FontAssetProperties
    {
        [JsonProperty]
        public string FileName;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("")]
        public string FontName;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("1.1.0")]
        public string FontVersion;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(1f)]
        public float Scale;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(0f)]
        public float AscentLineOffset;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(0f)]
        public float DescentLineOffset;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(64)]
        public int PointSize;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(4096)]
        public int AtlasWidth;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(4096)]
        public int AtlasHeight;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(5)]
        public int AtlasPadding;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(false)]
        public bool SingleAtlas;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("")]
        public string ShaderName;

        // [DefaultValue("")]
        // public string DefaultOSFont;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(GlyphLoadFlags.LOAD_DEFAULT)]
        public GlyphLoadFlags FontGlyphLoadFlags;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(FilterMode.Point)]
        public FilterMode AtlasTextureFilterMode;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(GlyphPackingMode.BestShortSideFit)]
        public GlyphPackingMode AtlasGlyphPackingMode;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(GlyphRenderMode.SDFAA)]
        public GlyphRenderMode AtlasRenderMode;

        public void Deconstruct(
            out string fontName,
            out string fontVersion,
            out float scale,
            out float ascentLineOffset,
            out float descentLineOffset,
            out int pointSize,
            out int atlasWidth,
            out int atlasHeight,
            out int atlasPadding,
            out bool singleAtlas,
            out string shaderName,
            out GlyphLoadFlags glyphLoadFlags,
            out FilterMode textureFilterMode,
            out GlyphPackingMode glyphPackingMode,
            out GlyphRenderMode atlasRenderMode
        )
        {
            fontName = FontName;
            fontVersion = FontVersion;
            scale = Scale;
            ascentLineOffset = AscentLineOffset;
            descentLineOffset = DescentLineOffset;
            pointSize = PointSize;
            atlasWidth = AtlasWidth;
            atlasHeight = AtlasHeight;
            atlasPadding = AtlasPadding;
            singleAtlas = SingleAtlas;
            shaderName = ShaderName;
            glyphLoadFlags = FontGlyphLoadFlags;
            textureFilterMode = AtlasTextureFilterMode;
            glyphPackingMode = AtlasGlyphPackingMode;
            atlasRenderMode = AtlasRenderMode;
        }
    }
}

