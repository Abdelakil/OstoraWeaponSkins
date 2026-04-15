using System.Diagnostics;

using Microsoft.Extensions.Logging;

using SwiftlyS2.Shared;

using ValveKeyValue;

namespace OstoraWeaponSkins.Econ;

public static class EconParser
{
    public static class ParsedData
    {
        public static Dictionary<int, string> AgentModels { get; } = new();
        public static Dictionary<int, string> Paintkits { get; } = new(); // paintkit index -> name
        public static Dictionary<string, int> WeaponDefinitions { get; } = new(StringComparer.OrdinalIgnoreCase); // weapon name -> definition index
        public static Dictionary<int, string> MusicKits { get; } = new(); // music kit index -> name
        public static Dictionary<string, int> GloveDefinitions { get; } = new(StringComparer.OrdinalIgnoreCase); // glove name -> definition index
        public static Dictionary<string, int> KnifeDefinitions { get; } = new(StringComparer.OrdinalIgnoreCase); // knife name -> definition index
    }

    public static void ParseAll(ISwiftlyCore core, ILogger logger)
    {
        try
        {
            var items = core.GameFileSystem.ReadFile("scripts/items/items_game.txt", "GAME");
            if (string.IsNullOrEmpty(items))
            {
                return;
            }

            var stream = new MemoryStream(items.Select(c => (byte)c).ToArray());
            var kv = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);
            var root = kv.Deserialize(stream);

            var stopwatch = Stopwatch.StartNew();

            ParsePaintkits(root, logger);
            ParseWeapons(root, logger);
            ParseMusicKits(root, logger);
            ParseGloves(root, logger);
            ParseKnives(root, logger);
            ParseAgents(root, logger);

            stopwatch.Stop();
        }
        catch (Exception e)
        {
            logger.LogError(e, "[OSTORA] Failed to parse econ data from items_game.txt");
        }
    }

    private static void ParsePaintkits(KVObject root, ILogger logger)
    {
        foreach (var keys in root.Children)
        {
            if (keys.Name != "paint_kits")
            {
                continue;
            }

            foreach (var paintkit in keys.Children)
            {
                var index = int.Parse(paintkit.Name);
                var name = paintkit.HasSubKey("name") ? paintkit.Value["name"].EToString() : string.Empty;
                if (!string.IsNullOrEmpty(name))
                {
                    ParsedData.Paintkits[index] = name;
                }
            }
        }
    }

    private static void ParseWeapons(KVObject root, ILogger logger)
    {
        foreach (var keys in root.Children)
        {
            if (keys.Name != "items")
            {
                continue;
            }

            foreach (var item in keys.Children)
            {
                var prefabName = item.HasSubKey("prefab") ? item.Value["prefab"].EToString() : string.Empty;

                // Skip agents, gloves, knives, music kits - we only want weapons
                if (prefabName.Equals("customplayertradable", StringComparison.OrdinalIgnoreCase) ||
                    prefabName.Equals("hands_paintable", StringComparison.OrdinalIgnoreCase) ||
                    prefabName.Equals("melee_unusual", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var name = item.HasSubKey("name") ? item.Value["name"].EToString() : string.Empty;
                if (!string.IsNullOrEmpty(name) && int.TryParse(item.Name, out var index))
                {
                    ParsedData.WeaponDefinitions[name] = index;
                }
            }
        }
    }

    private static void ParseMusicKits(KVObject root, ILogger logger)
    {
        foreach (var keys in root.Children)
        {
            if (keys.Name != "music_definitions")
            {
                continue;
            }

            foreach (var musicKit in keys.Children)
            {
                var index = musicKit.HasSubKey("id") ? musicKit.Value["id"].EToInt32() : 0;
                if (index == 0 && int.TryParse(musicKit.Name, out var parsedIndex))
                {
                    index = parsedIndex;
                }

                var name = musicKit.HasSubKey("loc_name") ? musicKit.Value["loc_name"].EToString() : string.Empty;
                if (string.IsNullOrEmpty(name))
                {
                    name = musicKit.HasSubKey("name") ? musicKit.Value["name"].EToString() : string.Empty;
                }

                if (!string.IsNullOrEmpty(name) && index != 0)
                {
                    ParsedData.MusicKits[index] = name;
                }
            }
        }
    }

    private static void ParseGloves(KVObject root, ILogger logger)
    {
        foreach (var keys in root.Children)
        {
            if (keys.Name != "items")
            {
                continue;
            }

            foreach (var item in keys.Children)
            {
                var prefabName = item.HasSubKey("prefab") ? item.Value["prefab"].EToString() : string.Empty;

                if (!prefabName.Equals("hands_paintable", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var name = item.HasSubKey("name") ? item.Value["name"].EToString() : string.Empty;
                if (!string.IsNullOrEmpty(name) && int.TryParse(item.Name, out var index))
                {
                    ParsedData.GloveDefinitions[name] = index;
                }
            }
        }
    }

    private static void ParseKnives(KVObject root, ILogger logger)
    {
        foreach (var keys in root.Children)
        {
            if (keys.Name != "items")
            {
                continue;
            }

            foreach (var item in keys.Children)
            {
                var prefabName = item.HasSubKey("prefab") ? item.Value["prefab"].EToString() : string.Empty;

                if (!prefabName.Equals("melee_unusual", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var name = item.HasSubKey("name") ? item.Value["name"].EToString() : string.Empty;
                if (!string.IsNullOrEmpty(name) && int.TryParse(item.Name, out var index))
                {
                    ParsedData.KnifeDefinitions[name] = index;
                }
            }
        }
    }

    private static void ParseAgents(KVObject root, ILogger logger)
    {
        KVObject? FindPrefab(string prefabName)
        {
            foreach (var keys in root.Children)
            {
                if (keys.Name == "prefabs")
                {
                    foreach (var prefab in keys.Children)
                    {
                        if (prefab.Name == prefabName)
                        {
                            return prefab;
                        }
                    }
                }
            }
            return null;
        }

        static string? FindAgentModelInChildren(KVObject obj)
        {
            foreach (var child in obj.Children)
            {
                var value = child.Value.EToString();

                if (!string.IsNullOrWhiteSpace(value))
                {
                    var s = value.Replace('\\', '/');
                    if (s.Contains("/tm_", StringComparison.OrdinalIgnoreCase) ||
                        s.Contains("/ctm_", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("tm_", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("ctm_", StringComparison.OrdinalIgnoreCase))
                    {
                        return s;
                    }
                }

                if (child.Children.Any())
                {
                    var nested = FindAgentModelInChildren(child);
                    if (!string.IsNullOrWhiteSpace(nested))
                    {
                        return nested;
                    }
                }
            }
            return null;
        }

        foreach (var section in root.Children)
        {
            if (section.Name != "items")
            {
                continue;
            }

            foreach (var item in section.Children)
            {
                var prefabName = item.HasSubKey("prefab") ? item.Value["prefab"].EToString() : string.Empty;

                if (!prefabName.Equals("customplayertradable", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var prefab = FindPrefab(prefabName);

                string? modelPath = FindAgentModelInChildren(item);
                if (string.IsNullOrWhiteSpace(modelPath) && prefab != null)
                {
                    modelPath = FindAgentModelInChildren(prefab);
                }

                if (string.IsNullOrWhiteSpace(modelPath))
                {
                    continue;
                }

                modelPath = modelPath.Replace('\\', '/');

                string fullModelPath = modelPath;
                if (!fullModelPath.StartsWith("characters/models/", StringComparison.OrdinalIgnoreCase))
                {
                    if (fullModelPath.Contains("characters/models/", StringComparison.OrdinalIgnoreCase))
                    {
                        var idx = fullModelPath.IndexOf("characters/models/", StringComparison.OrdinalIgnoreCase);
                        fullModelPath = fullModelPath.Substring(idx);
                    }
                }

                if (!fullModelPath.EndsWith(".vmdl", StringComparison.OrdinalIgnoreCase))
                {
                    fullModelPath += ".vmdl";
                }

                if (int.TryParse(item.Name, out var index))
                {
                    ParsedData.AgentModels[index] = fullModelPath;
                }
            }
        }
    }
}
