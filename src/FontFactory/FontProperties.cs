using System;
using System.ComponentModel;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace WKLocalizationLoader.FontFactory
{
    public class FontProperties
    {
        [JsonProperty]
        public string FileName;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("")]
        public string FontName;

        [JsonProperty]
        public int PointSize;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(0f)]
        public float VerticalOffset;

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
        [DefaultValue("")]
        public string ShaderName;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("")]
        public string DefaultOSFont;

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
        [DefaultValue(GlyphRenderMode.RASTER_HINTED)]
        public GlyphRenderMode AtlasRenderMode;

        public void Deconstruct(
            out string fontName,
            out int pointSize,
            out float verticalOffset,
            out int atlasWidth,
            out int atlasHeight,
            out int atlasPadding,
            out string shaderName,
            out GlyphLoadFlags glyphLoadFlags,
            out FilterMode textureFilterMode,
            out GlyphPackingMode glyphPackingMode,
            out GlyphRenderMode atlasRenderMode
        )
        {
            fontName = FontName;
            pointSize = PointSize;
            verticalOffset = VerticalOffset;
            atlasWidth = AtlasWidth;
            atlasHeight = AtlasHeight;
            atlasPadding = AtlasPadding;
            shaderName = ShaderName;
            glyphLoadFlags = FontGlyphLoadFlags;
            textureFilterMode = AtlasTextureFilterMode;
            glyphPackingMode = AtlasGlyphPackingMode;
            atlasRenderMode = AtlasRenderMode;
        }
    }
}

