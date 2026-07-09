using System;
using System.ComponentModel;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace WKLocalizationLoader.FontFactory
{
    public class FontAssetProperties
    {
        public string FileName;

        [DefaultValue("")]
        public string FontName;

        [DefaultValue("1.1.0")]
        public string FontVersion;

        [DefaultValue(1f)]
        public float Scale;

        [DefaultValue(0f)]
        public float AscentLineOffset;

        [DefaultValue(0f)]
        public float DescentLineOffset;

        [DefaultValue(64)]
        public int PointSize;

        [DefaultValue(4096)]
        public int AtlasWidth;

        [DefaultValue(4096)]
        public int AtlasHeight;

        [DefaultValue(5)]
        public int AtlasPadding;

        [DefaultValue(false)]
        public bool SingleAtlas;

        [DefaultValue("")]
        public string ShaderName;

        // [DefaultValue("")]
        // public string DefaultOSFont;

        [DefaultValue(GlyphLoadFlags.LOAD_DEFAULT)]
        public GlyphLoadFlags FontGlyphLoadFlags;

        [DefaultValue(FilterMode.Point)]
        public FilterMode AtlasTextureFilterMode;

        [DefaultValue(GlyphPackingMode.BestShortSideFit)]
        public GlyphPackingMode AtlasGlyphPackingMode;

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

