using System;
using UnityEngine.TextCore;

namespace WKLocalizationLoader.FontFactory
{
    public class GlyphInfo
    {
        public uint CharCode;
        public Glyph GlyphInstance;

        public GlyphInfo(uint charCode, Glyph glyphInstance)
        {
            CharCode = charCode;
            GlyphInstance = glyphInstance;
        }
    }
}

