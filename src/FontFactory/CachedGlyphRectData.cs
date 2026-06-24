using System;
using Newtonsoft.Json;
using UnityEngine.TextCore;

namespace WKLocalizationLoader.FontFactory
{
    public class CachedGlyphRectData
    {
        [JsonProperty]
        public int X;
        [JsonProperty]
        public int Y;
        [JsonProperty]
        public int Width;
        [JsonProperty]
        public int Height;

        public CachedGlyphRectData()
        {
        }

        public CachedGlyphRectData(GlyphRect glyphRect)
        {
            X = glyphRect.x;
            Y = glyphRect.y;
            Width = glyphRect.width;
            Height = glyphRect.height;
        }

        public GlyphRect ToGlyphRect() => new GlyphRect(
            x: X,
            y: Y,
            width: Width,
            height: Height
        );
    }
}

