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
        public static Dictionary<int, StickerDefinition> Stickers { get; } = new(); // sticker index -> definition
        public static Dictionary<int, StickerCollectionDefinition> StickerCollections { get; } = new(); // collection index -> definition
        public static Dictionary<string, List<string>> ClientLootLists { get; } = new(StringComparer.OrdinalIgnoreCase); // loot list name -> item names
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
            ParseClientLootLists(root, logger);
            ParseStickers(root, logger);
            ParseStickerCollections(root, logger);

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

    private static void ParseClientLootLists(KVObject root, ILogger logger)
    {
        foreach (var keys in root.Children)
        {
            if (keys.Name != "client_loot_lists")
            {
                continue;
            }

            foreach (var lootList in keys.Children)
            {
                var items = new List<string>();
                foreach (var child in lootList.Children)
                {
                    items.Add(child.Name);
                }
                ParsedData.ClientLootLists[lootList.Name] = items;
            }
        }
    }

    private static void ParseStickers(KVObject root, ILogger logger)
    {
        foreach (var keys in root.Children)
        {
            if (keys.Name != "sticker_kits")
            {
                continue;
            }

            foreach (var stickerKit in keys.Children)
            {
                if (!stickerKit.HasSubKey("sticker_material"))
                {
                    continue;
                }

                var index = int.Parse(stickerKit.Name);
                var name = stickerKit.Value["name"].EToString();
                var stickerMaterial = stickerKit.Value["sticker_material"].EToString();
                var itemName = stickerKit.HasSubKey("item_name")
                    ? stickerKit.Value["item_name"].EToString()
                    : name;

                var definition = new StickerDefinition
                {
                    Name = name,
                    Index = index,
                    ItemName = itemName,
                    StickerMaterial = stickerMaterial
                };

                ParsedData.Stickers[index] = definition;
            }
        }

        if (OstoraWeaponSkins.DebugLogging)
            logger.LogInformation("[OSTORA] Parsed {Count} stickers", ParsedData.Stickers.Count);
    }

    // Signature packs need special handling - they reference rare/legendary variants
    private static readonly Dictionary<string, List<string>> SignaturePackPatches = new()
    {
        ["crate_signature_pack_eslcologne2015_group_1"] =
        [
            "crate_signature_pack_eslcologne2015_group_1_rare",
            "crate_signature_pack_eslcologne2015_group_1_legendary"
        ],
        ["crate_signature_pack_eslcologne2015_group_2"] =
        [
            "crate_signature_pack_eslcologne2015_group_2_rare",
            "crate_signature_pack_eslcologne2015_group_2_legendary"
        ],
        ["crate_signature_pack_eslcologne2015_group_3"] =
        [
            "crate_signature_pack_eslcologne2015_group_3_rare",
            "crate_signature_pack_eslcologne2015_group_3_legendary"
        ],
        ["crate_signature_pack_eslcologne2015_group_4"] =
        [
            "crate_signature_pack_eslcologne2015_group_4_rare",
            "crate_signature_pack_eslcologne2015_group_4_legendary"
        ],
        ["crate_signature_pack_cluj2015_group_1"] =
        [
            "crate_signature_pack_cluj2015_group_1_rare",
            "crate_signature_pack_cluj2015_group_1_legendary"
        ],
        ["crate_signature_pack_cluj2015_group_2"] =
        [
            "crate_signature_pack_cluj2015_group_2_rare",
            "crate_signature_pack_cluj2015_group_2_legendary"
        ],
    };

    private static void ParseStickerCollections(KVObject root, ILogger logger)
    {
        foreach (var keys in root.Children)
        {
            if (keys.Name != "items")
            {
                continue;
            }

            foreach (var item in keys.Children)
            {
                // Check for StickerCapsule tag
                if (item.HasSubKey("tags") && item.GetSubKey("tags")!.HasSubKey("StickerCapsule"))
                {
                    var name = item.Value["name"].EToString();
                    var index = int.Parse(item.Name);
                    var itemName = item.HasSubKey("item_name")
                        ? item.Value["item_name"].EToString()
                        : name;

                    // Find the loot list for this collection
                    var lootListName = name;
                    var lootListName2 = item.GetSubKey("tags")!.GetSubKey("StickerCapsule")!.Value["tag_value"]!.EToString();

                    List<string>? lootListItems = null;
                    if (ParsedData.ClientLootLists.TryGetValue(lootListName, out var ll1))
                    {
                        lootListItems = ll1;
                    }
                    else if (ParsedData.ClientLootLists.TryGetValue(lootListName2, out var ll2))
                    {
                        lootListItems = ll2;
                    }
                    // Apply signature pack patches if needed
                    else if (SignaturePackPatches.TryGetValue(name, out var patch))
                    {
                        lootListItems = new List<string>();
                        foreach (var patchListName in patch)
                        {
                            if (ParsedData.ClientLootLists.TryGetValue(patchListName, out var patchItems))
                            {
                                lootListItems.AddRange(patchItems);
                            }
                        }
                    }

                    if (lootListItems == null)
                    {
                        logger.LogWarning("[OSTORA] Sticker collection {Name} not found in ClientLootLists", name);
                        continue;
                    }

                    var stickers = new List<StickerDefinition>();
                    foreach (var stickerName in lootListItems)
                    {
                        // Find sticker by name
                        var sticker = ParsedData.Stickers.Values.FirstOrDefault(s => s.Name.Equals(stickerName, StringComparison.OrdinalIgnoreCase));
                        if (sticker != null)
                        {
                            stickers.Add(sticker);
                        }
                    }

                    var definition = new StickerCollectionDefinition
                    {
                        Name = name,
                        Index = index,
                        ItemName = itemName,
                        Stickers = stickers
                    };

                    ParsedData.StickerCollections[index] = definition;
                }
                // Check for capsule prefab
                else if (item.HasSubKey("prefab"))
                {
                    var prefab = item.Value["prefab"].EToString();
                    if (!prefab.Contains("_capsule_prefab"))
                    {
                        continue;
                    }

                    if (!item.HasSubKey("attributes"))
                    {
                        continue;
                    }

                    var attributes = item.GetSubKey("attributes")!;
                    if (!attributes.HasSubKey("set supply crate series"))
                    {
                        continue;
                    }

                    var revolvingIndex = attributes.GetSubKey("set supply crate series")!.Value["value"]!.EToString();

                    // Get revolving loot list
                    string? revolvingLootListName = null;
                    foreach (var rlKeys in root.Children)
                    {
                        if (rlKeys.Name == "revolving_loot_lists")
                        {
                            foreach (var rl in rlKeys.Children)
                            {
                                if (rl.Name == revolvingIndex)
                                {
                                    revolvingLootListName = rl.Value.EToString();
                                    break;
                                }
                            }
                        }
                    }

                    if (revolvingLootListName == null)
                    {
                        continue;
                    }

                    if (!ParsedData.ClientLootLists.TryGetValue(revolvingLootListName, out var revolvingLootList))
                    {
                        continue;
                    }

                    var name = item.Value["name"].EToString();
                    var index = int.Parse(item.Name);
                    var itemName = item.HasSubKey("item_name")
                        ? item.Value["item_name"].EToString()
                        : name;

                    var stickers = new List<StickerDefinition>();
                    foreach (var stickerName in revolvingLootList)
                    {
                        // Only include actual stickers (not other items)
                        // Sticker item names usually start with "sticker_"
                        if (!stickerName.StartsWith("sticker_", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        var sticker = ParsedData.Stickers.Values.FirstOrDefault(s => s.Name.Equals(stickerName, StringComparison.OrdinalIgnoreCase));
                        if (sticker != null)
                        {
                            stickers.Add(sticker);
                        }
                    }

                    var definition = new StickerCollectionDefinition
                    {
                        Name = name,
                        Index = index,
                        ItemName = itemName,
                        Stickers = stickers
                    };

                    ParsedData.StickerCollections[index] = definition;
                }
            }
        }

        if (OstoraWeaponSkins.DebugLogging)
            logger.LogInformation("[OSTORA] Parsed {Count} sticker collections", ParsedData.StickerCollections.Count);
    }
}
