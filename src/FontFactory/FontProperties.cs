using System;
using System.ComponentModel;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace WKLocalizationLoader.FontFactory
{
    public class FontProperties
    {
        public string FileName;

        [DefaultValue("")]
        public string FontName;

        public int PointSize;

        [DefaultValue(0f)]
        public float VerticalOffset;

        [DefaultValue(4096)]
        public int AtlasWidth;

        [DefaultValue(4096)]
        public int AtlasHeight;

        [DefaultValue(5)]
        public int AtlasPadding;

        [DefaultValue("")]
        public string ShaderName;

        [DefaultValue("")]
        public string DefaultOSFont;

        [DefaultValue(GlyphLoadFlags.LOAD_DEFAULT)]
        public GlyphLoadFlags FontGlyphLoadFlags;

        [DefaultValue(FilterMode.Point)]
        public FilterMode AtlasTextureFilterMode;

        [DefaultValue(GlyphPackingMode.BestShortSideFit)]
        public GlyphPackingMode AtlasGlyphPackingMode;

        [DefaultValue(GlyphRenderMode.RASTER_HINTED)]
        public GlyphRenderMode AtlasRenderMode;

        public void Deconstruct(
            out string fileName,
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
            fileName = FileName;
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

