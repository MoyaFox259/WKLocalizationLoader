using System;
using Newtonsoft.Json;
using UnityEngine;

namespace WKLocalizationLoader.FontFactory
{
    public class CachedCharacterInfoData
    {
        [JsonProperty]
        public int CharCode;
        [JsonProperty]
        public float U;
        [JsonProperty]
        public float V;
        [JsonProperty]
        public float U2;
        [JsonProperty]
        public float V2;
        [JsonProperty]
        public int MinX;
        [JsonProperty]
        public int MaxX;
        [JsonProperty]
        public int MinY;
        [JsonProperty]
        public int MaxY;
        [JsonProperty]
        public int Advance;

        public CachedCharacterInfoData()
        {
        }

        public CachedCharacterInfoData(CharacterInfo characterInfo)
        {
            CharCode = characterInfo.index;
            U = characterInfo.uvBottomLeft.x;
            V = characterInfo.uvBottomLeft.y;
            U2 = characterInfo.uvTopRight.x;
            V2 = characterInfo.uvTopRight.y;
            MinX = characterInfo.minX;
            MaxX = characterInfo.maxX;
            MinY = characterInfo.minY;
            MaxY = characterInfo.maxY;
            Advance = characterInfo.advance;
        }

        public CharacterInfo ToCharacterInfo() => new CharacterInfo()
        {
            index = CharCode,
            uvBottomLeft = new Vector2(U, V),
            uvBottomRight = new Vector2(U2, V),
            uvTopLeft = new Vector2(U, V2),
            uvTopRight = new Vector2(U2, V2),
            minX = MinX,
            maxX = MaxX,
            minY = MinY,
            maxY = MaxY,
            advance = Advance
        };
    }
}

