using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using HttpSample.Common.Models;

namespace HttpSample.Common.Extensions
{
    public static class JsonExtension
    {
        public static string ToJson<T>(this T o, NamingStrategyType namingStrategy = default)
        {
            return JsonConvert.SerializeObject(o, GetJsonSerializerSettings(namingStrategy));
        }

        public static T FromJson<T>(this string json, NamingStrategyType namingStrategy = default, bool ignoreError = false)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(json, GetJsonSerializerSettings(namingStrategy));
            }
            catch (Exception)
            {
                if (ignoreError)
                {
                    return default; //Silently swallow exception and return default value if the parameter is set to true
                }
                throw;
            }
        }

        #region Helper Funcs

        private static JsonSerializerSettings GetJsonSerializerSettings(NamingStrategyType namingStrategy = default)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = GetNamingStrategy(namingStrategy)
                },
                Formatting = Formatting.Indented,
            };

            return settings;
        }

        private static NamingStrategy GetNamingStrategy(NamingStrategyType namingStrategy = default)
        {
            return namingStrategy switch
            {
                NamingStrategyType.CamelCase => new CamelCaseNamingStrategy() { ProcessDictionaryKeys = true },
                NamingStrategyType.SnakeCase => new SnakeCaseNamingStrategy() { ProcessDictionaryKeys = true },

                _ => new SnakeCaseNamingStrategy() { ProcessDictionaryKeys = true },
            };
        }

        #endregion
    }
}
