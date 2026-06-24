using System;
using System.Text;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WKLocalizationLoader.FontFactory;

namespace WKLocalizationLoader
{
    public static class HashCalculator
    {
        private static JsonSerializerSettings _jsonSerializerSettings;

        public static void Initialize()
        {
            _jsonSerializerSettings = new JsonSerializerSettings()
            {
                ContractResolver = new DefaultContractResolver()
                {
                    NamingStrategy = new DefaultNamingStrategy()
                },
                Formatting = Formatting.None,
                NullValueHandling = NullValueHandling.Include,
                DefaultValueHandling = DefaultValueHandling.Populate
            };
        }

        public static string GetHashString(
            string characters,
            FontProperties fontProperties,
            int hashStringLength = 6
        )
        {
            var data = new {
                Characters = characters,
                Properties = fontProperties
            };
            return GetHashString(data, hashStringLength);
        }

        public static string GetHashString(
            string characters,
            FontAssetProperties fontAssetProperties,
            int hashStringLength = 6
        )
        {
            var data = new {
                Characters = characters,
                Properties = fontAssetProperties
            };
            return GetHashString(data, hashStringLength);
        }

        public static string GetHashString(
            object targetObject,
            int hashStringLength
        )
        {
            if (_jsonSerializerSettings is null) Initialize();
            var jsonText = JsonConvert.SerializeObject(
                targetObject,
                _jsonSerializerSettings
            );
            using (var hashAlgorithm = SHA256.Create())
            {
                var hash = hashAlgorithm.ComputeHash(
                    Encoding.UTF8.GetBytes(jsonText)
                );
                var hashString = BitConverter.ToString(hash).Replace("-", "");
                return hashString.Substring(0, hashStringLength).ToLower();
            }
        }
    }
}

