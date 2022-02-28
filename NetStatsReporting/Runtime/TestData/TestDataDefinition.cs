using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Random = System.Random;

namespace Unity.Multiplayer.Tools.NetStatsReporting
{
    internal class TestDataDefinition
    {
        const string TestAssetPath =
            "Packages/com.unity.multiplayer.tools/NetStatsReporting/Runtime/TestData/Definitions/";

        readonly IReadOnlyList<string> m_Adjectives;
        readonly IReadOnlyList<string> m_Verbs;
        readonly IReadOnlyList<string> m_Nouns;
        readonly IReadOnlyList<string> m_VariableTypes = new[] { "count", "size", "state" };

        readonly Random m_Random;

        public TestDataDefinition(int seed)
        {
            m_Random = new Random(seed);

            // From https://github.com/dariusk/corpora CC0 license
            m_Adjectives = LoadJsonDefinition("adjs.json", "adjs");
            m_Verbs = LoadJsonDefinition("verbs.json", "verbs", "present");
            m_Nouns = LoadJsonDefinition("personal_nouns.json", "personalNouns");
        }

        public string GenerateGameObjectName()
        {
            return $"{Capitalize(GetRandomValue(m_Adjectives))} {Capitalize(GetRandomValue(m_Adjectives))} {Capitalize(GetRandomValue(m_Nouns))}";
        }

        public string GenerateComponentName()
        {
            return $"{Capitalize(GetRandomValue(m_Nouns))}{Capitalize(GetRandomValue(m_Verbs))}Component";
        }

        public string GenerateVariableName()
        {
            return $"{Capitalize(GetRandomValue(m_Nouns))}{Capitalize(GetRandomValue(m_Nouns))}{Capitalize(GetRandomValue(m_VariableTypes))}";
        }

        public string GenerateNamedMessageName()
        {
            return $"{Capitalize(GetRandomValue(m_Verbs))}{Capitalize(GetRandomValue(m_Nouns))}";
        }

        public string GenerateRpcName()
        {
            return GenerateNamedMessageName();
        }

        public long GenerateByteCount()
        {
            // Generate mostly byte-sized, a few kilo-sized and sometimes mega-sized
            var magnitudeSelector = m_Random.Next(0, 10);
            if (magnitudeSelector == 10)
            {
                return m_Random.Next(1000000, 2000000);
            }

            if (magnitudeSelector > 7)
            {
                return m_Random.Next(1000, 999999);
            }

            return m_Random.Next(1, 999);
        }

        public string GenerateSceneName()
        {
            return $"{Capitalize(GetRandomValue(m_Adjectives))}{Capitalize(GetRandomValue(m_Adjectives))}Scene";
        }

        string GetRandomValue(IReadOnlyList<string> collection)
        {
            return !collection.Any()
                ? string.Empty
                : collection[m_Random.Next(0, collection.Count)];
        }

        static IReadOnlyList<string> LoadJsonDefinition(string filename, string category, string subCategory = "")
        {
            #if !UNITY_EDITOR
                return System.Array.Empty<string>();
            #else
                var fileContent = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>($"{TestAssetPath}{filename}");
                var jObject = JObject.Parse(fileContent.text);

                var tokens = string.IsNullOrWhiteSpace(subCategory)
                    ? jObject[category]?.Select(x => x.ToObject<string>())
                    : jObject[category]?.Select(x => x[subCategory]).Select(x => x.ToObject<string>());

                return tokens?
                    .Where(x => x != null)
                    .Where(x => !x.Contains("-"))
                    .ToArray();
            #endif
        }

        static string Capitalize(string input)
        {
            return string.IsNullOrEmpty(input)
                ? string.Empty
                : $"{char.ToUpper(input[0])}{input.Substring(1)}";
        }
    }
}