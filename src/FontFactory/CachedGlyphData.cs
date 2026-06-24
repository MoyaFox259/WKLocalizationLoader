using System;
using Newtonsoft.Json;
using UnityEngine.TextCore;

namespace WKLocalizationLoader.FontFactory
{
    public class CachedGlyphData
    {
        [JsonProperty]
        public int AtlasIndex;
        [JsonProperty]
        public uint Index;
        [JsonProperty]
        public float Scale;
        [JsonProperty]
        public CachedGlyphMetricsData GlyphMetrics;
        [JsonProperty]
        public CachedGlyphRectData GlyphRect;

        public CachedGlyphData()
        {
        }

        public CachedGlyphData(Glyph glyph)
        {
            AtlasIndex = glyph.atlasIndex;
            Index = glyph.index;
            Scale = glyph.scale;
            GlyphMetrics = new CachedGlyphMetricsData(glyph.metrics);
            GlyphRect = new CachedGlyphRectData(glyph.glyphRect);
        }

        public Glyph ToGlyph() => new Glyph(
            index: Index,
            metrics: GlyphMetrics.ToGlyphMetrics(),
            glyphRect: GlyphRect.ToGlyphRect(),
            scale: Scale,
            atlasIndex: AtlasIndex
        );
    }
}

