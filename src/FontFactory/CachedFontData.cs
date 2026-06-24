using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace WKLocalizationLoader.FontFactory
{
    public class CachedFontData
    {
        [JsonProperty]
        public string FontName;
        [JsonProperty]
        public int AtlasWidth;
        [JsonProperty]
        public int AtlasHeight;
        [JsonProperty]
        public FilterMode TextureFilterMode;
        [JsonProperty]
        public string ShaderName;
        [JsonProperty]
        public List<CachedCharacterInfoData> CharacterInfos;

        public CachedFontData()
        {
        }

        public CachedFontData(Font font)
        {
            FontName = font.name;
            AtlasWidth = font.material.mainTexture.width;
            AtlasHeight = font.material.mainTexture.height;
            TextureFilterMode = font.material.mainTexture.filterMode;
            ShaderName = font.material.shader.name;
            CharacterInfos = font.characterInfo
                .Select(c => new CachedCharacterInfoData(c))
                .ToList();
        }

        public void Deconstruct(
            out string fontName,
            out int atlasWidth,
            out int atlasHeight,
            out FilterMode textureFilterMode,
            out string shaderName,
            out CharacterInfo[] characterInfos
        )
        {
            fontName = FontName;
            atlasWidth = AtlasWidth;
            atlasHeight = AtlasHeight;
            textureFilterMode = TextureFilterMode;
            shaderName = ShaderName;
            characterInfos = CharacterInfos
                .Select(c => c.ToCharacterInfo())
                .ToArray();
        }
    }
}

