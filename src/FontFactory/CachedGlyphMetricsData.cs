using System;
using Newtonsoft.Json;
using UnityEngine.TextCore;

namespace WKLocalizationLoader.FontFactory
{
    public class CachedGlyphMetricsData
    {
        [JsonProperty]
        public float Width;
        [JsonProperty]
        public float Height;
        [JsonProperty]
        public float HorizontalBearingX;
        [JsonProperty]
        public float HorizontalBearingY;
        [JsonProperty]
        public float HorizontalAdvance;

        public CachedGlyphMetricsData()
        {
        }

        public CachedGlyphMetricsData(GlyphMetrics glyphMetrics)
        {
            Width = glyphMetrics.width;
            Height = glyphMetrics.height;
            HorizontalBearingX = glyphMetrics.horizontalBearingX;
            HorizontalBearingY = glyphMetrics.horizontalBearingY;
            HorizontalAdvance = glyphMetrics.horizontalAdvance;
        }

        public GlyphMetrics ToGlyphMetrics() => new GlyphMetrics(
            width: Width,
            height: Height,
            bearingX: HorizontalBearingX,
            bearingY: HorizontalBearingY,
            advance: HorizontalAdvance
        );
    }
}

