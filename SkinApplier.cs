using System.Threading;
using Newtonsoft.Json.Linq;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace OstoraWeaponSkins;

public sealed partial class OstoraWeaponSkins
{
    internal static void GivePlayerWeaponSkin(IPlayer player, CEntityInstance weaponEntity)
    {
        if (!_config.SkinEnabled) return;
        if (!GPlayerWeaponsInfo.TryGetValue(player.Slot, out _))
        {
            LogDebug($"[OstoraWeaponSkins] GivePlayerWeaponSkin: No skins for {player.Name} slot={player.Slot}");
            return;
        }

        var controller = player.Controller;
        if (controller == null) return;

        int team = controller.TeamNum;

        // Only process weapons that have the econ entity hierarchy
        if (weaponEntity is not CEconEntity econWeapon || !econWeapon.IsValid)
        {
            LogDebug($"[OstoraWeaponSkins] GivePlayerWeaponSkin: Not CEconEntity: {weaponEntity.GetType().Name} ({weaponEntity.DesignerName})");
            return;
        }

        var defIndex = econWeapon.AttributeManager?.Item?.ItemDefinitionIndex ?? 0;
        LogDebug($"[OstoraWeaponSkins] GivePlayerWeaponSkin: {player.Name} {weaponEntity.DesignerName} defindex={defIndex}");

        var item = econWeapon.AttributeManager.Item;

        bool isKnife = weaponEntity.DesignerName.Contains("knife") || weaponEntity.DesignerName.Contains("bayonet");

        if (isKnife)
        {
            if (!HasChangedKnife(player, out var knifeValue) || knifeValue == null)
                return;

            var newDefIndex = WeaponDefindex.FirstOrDefault(x => x.Value == knifeValue);
            if (newDefIndex.Key == 0) return;

            if (item.ItemDefinitionIndex != newDefIndex.Key)
            {
                SubclassChange(weaponEntity, (ushort)newDefIndex.Key);
            }

            item.ItemDefinitionIndex = (ushort)newDefIndex.Key;
            item.EntityQuality = 3;

            item.AttributeList.Attributes.RemoveAll();
            item.NetworkedDynamicAttributes.Attributes.RemoveAll();

            item.ItemID = NextItemId();
            item.ItemIDLow = (uint)item.ItemID & 0xFFFFFFFF;
            item.ItemIDHigh = (uint)(item.ItemID >> 32);

            // Fall through to paint application below (CS# uses break, not return)
        }
        else
        {
            item.EntityQuality = 0;
        }

        item.ItemID = NextItemId();
        item.ItemIDLow = (uint)item.ItemID & 0xFFFFFFFF;
        item.ItemIDHigh = (uint)(item.ItemID >> 32);

        int weaponDefIndex = item.ItemDefinitionIndex;

        item.AccountID = (uint)player.SteamID;

        if (!HasChangedPaint(player, weaponDefIndex, out var weaponInfo) || weaponInfo == null)
        {
            LogDebug($"[OstoraWeaponSkins] GivePlayerWeaponSkin: No paint for defindex={weaponDefIndex} team={team}");
            return;
        }

        LogDebug($"[OstoraWeaponSkins] GivePlayerWeaponSkin: Applying paint={weaponInfo.Paint} seed={weaponInfo.Seed} to {weaponEntity.DesignerName}");

        item.AttributeList.Attributes.RemoveAll();
        item.NetworkedDynamicAttributes.Attributes.RemoveAll();

        item.ItemID = NextItemId();
        item.ItemIDLow = (uint)item.ItemID & 0xFFFFFFFF;
        item.ItemIDHigh = (uint)(item.ItemID >> 32);

        item.CustomName = weaponInfo.Nametag;
        econWeapon.FallbackPaintKit = weaponInfo.Paint;
        econWeapon.FallbackSeed = weaponInfo.Seed;
        econWeapon.FallbackWear = weaponInfo.Wear;

        int fallbackPaintKit = econWeapon.FallbackPaintKit;
        if (fallbackPaintKit == 0)
            return;

        // Paint and seed use regular float conversion (CS# passes int→float directly).
        // Only kill eater, sticker IDs, and keychain IDs use IntAsFloat bit-reinterpretation.
        item.NetworkedDynamicAttributes.SetOrAddAttribute("set item texture prefab", (float)fallbackPaintKit);
        item.NetworkedDynamicAttributes.SetOrAddAttribute("set item texture seed", (float)econWeapon.FallbackSeed);
        item.NetworkedDynamicAttributes.SetOrAddAttribute("set item texture wear", econWeapon.FallbackWear);
        item.AttributeList.SetOrAddAttribute("set item texture prefab", (float)fallbackPaintKit);
        item.AttributeList.SetOrAddAttribute("set item texture seed", (float)econWeapon.FallbackSeed);
        item.AttributeList.SetOrAddAttribute("set item texture wear", econWeapon.FallbackWear);

        if (weaponInfo.StatTrak)
        {
            item.EntityQuality = 9;
            var stCount = BitConverter.Int32BitsToSingle(weaponInfo.StatTrakCount);
            item.NetworkedDynamicAttributes.SetOrAddAttribute("kill eater", stCount);
            item.NetworkedDynamicAttributes.SetOrAddAttribute("kill eater score type", 0);
            item.AttributeList.SetOrAddAttribute("kill eater", stCount);
            item.AttributeList.SetOrAddAttribute("kill eater score type", 0);
        }

        if (weaponInfo.KeyChain != null && (weaponInfo.KeyChain.Id > 0))
            SetKeychain(econWeapon, weaponInfo);
        if (weaponInfo.Stickers.Count > 0)
            SetStickers(econWeapon, weaponInfo);

        var skinInfo = SkinsList
            .Where(w =>
                w["weapon_defindex"]?.ToObject<int>() == weaponDefIndex &&
                w["paint"]?.ToObject<int>() == fallbackPaintKit)
            .ToList();

        bool isLegacyModel = skinInfo.Count <= 0 || skinInfo[0].Value<bool>("legacy_model");
        UpdateWeaponMeshGroupMask(weaponEntity, isLegacyModel);
    }

    private static void SetStickers(CEconEntity weapon, WeaponInfo weaponInfo)
    {
        var attr = weapon.AttributeManager.Item.NetworkedDynamicAttributes;

        foreach (var sticker in weaponInfo.Stickers)
        {
            int stickerSlot = weaponInfo.Stickers.IndexOf(sticker);

            attr.SetOrAddAttribute($"sticker slot {stickerSlot} id", IntAsFloat((int)sticker.Id));
            if (sticker.OffsetX != 0 || sticker.OffsetY != 0)
                attr.SetOrAddAttribute($"sticker slot {stickerSlot} schema", 0);
            attr.SetOrAddAttribute($"sticker slot {stickerSlot} offset x", sticker.OffsetX);
            attr.SetOrAddAttribute($"sticker slot {stickerSlot} offset y", sticker.OffsetY);
            attr.SetOrAddAttribute($"sticker slot {stickerSlot} wear", sticker.Wear);
            attr.SetOrAddAttribute($"sticker slot {stickerSlot} scale", sticker.Scale);
            attr.SetOrAddAttribute($"sticker slot {stickerSlot} rotation", sticker.Rotation);
        }
    }

    private static void SetKeychain(CEconEntity weapon, WeaponInfo weaponInfo)
    {
        if (weaponInfo.KeyChain == null) return;

        var keyChain = weaponInfo.KeyChain;
        var attr = weapon.AttributeManager.Item.NetworkedDynamicAttributes;

        attr.SetOrAddAttribute("keychain slot 0 id", IntAsFloat((int)keyChain.Id));
        attr.SetOrAddAttribute("keychain slot 0 offset x", keyChain.OffsetX);
        attr.SetOrAddAttribute("keychain slot 0 offset y", keyChain.OffsetY);
        attr.SetOrAddAttribute("keychain slot 0 offset z", keyChain.OffsetZ);
        attr.SetOrAddAttribute("keychain slot 0 seed", IntAsFloat((int)keyChain.Seed));
    }

    internal static void GivePlayerGloves(IPlayer player)
    {
        if (!IsPlayerValid(player) || !player.IsAlive) return;

        var pawn = player.PlayerPawn;
        if (pawn == null || !pawn.IsValid)
            return;

        var controller = player.Controller;
        if (controller == null) return;

        int team = controller.TeamNum;

        CEconItemView item = pawn.EconGloves;

        item.NetworkedDynamicAttributes.Attributes.RemoveAll();
        item.AttributeList.Attributes.RemoveAll();

        // Force gloves model refresh to prevent model overlap (matches CS# reference)
        player.ExecuteCommand("lastinv");

        try
        {
            Core.Scheduler.DelayBySeconds(0.08f, () =>
            {
                if (!GPlayersGlove.TryGetValue(player.Slot, out var gloveInfo) ||
                    !gloveInfo.TryGetValue(team, out var gloveId) ||
                    gloveId == 0)
                    return;

                item.ItemDefinitionIndex = gloveId;

                item.ItemID = NextItemId();
                item.ItemIDLow = (uint)item.ItemID & 0xFFFFFFFF;
                item.ItemIDHigh = (uint)(item.ItemID >> 32);

                // Look up paint/seed/wear from weapon info
                WeaponInfo? gloveWeaponInfo = null;
                if (GPlayerWeaponsInfo.TryGetValue(player.Slot, out var teamDict) &&
                    teamDict.TryGetValue(team, out var defDict) &&
                    defDict.TryGetValue(gloveId, out var gi))
                    gloveWeaponInfo = gi;

                var paint = gloveWeaponInfo?.Paint ?? 0;
                var seed = gloveWeaponInfo?.Seed ?? 0;
                var wear = gloveWeaponInfo?.Wear ?? 0.0001f;

                item.NetworkedDynamicAttributes.Attributes.RemoveAll();
                item.NetworkedDynamicAttributes.SetOrAddAttribute("set item texture prefab", (float)paint);
                item.NetworkedDynamicAttributes.SetOrAddAttribute("set item texture seed", (float)seed);
                item.NetworkedDynamicAttributes.SetOrAddAttribute("set item texture wear", wear);

                item.AttributeList.Attributes.RemoveAll();
                item.AttributeList.SetOrAddAttribute("set item texture prefab", (float)paint);
                item.AttributeList.SetOrAddAttribute("set item texture seed", (float)seed);
                item.AttributeList.SetOrAddAttribute("set item texture wear", wear);

                item.Initialized = true;

                // Force gloves model refresh to apply
                player.ExecuteCommand("lastinv");
                pawn.AcceptInput("SetBodygroup", $"first_or_third_person,0");
                Core.Scheduler.DelayBySeconds(0.2f, () =>
                {
                    try
                    {
                        pawn.AcceptInput("SetBodygroup", $"first_or_third_person,1");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[OstoraWeaponSkins] Error setting glove bodygroup: {ex.Message}");
                    }
                });

                // Dump both teams' glove data for diagnostics
                if (GPlayerWeaponsInfo.TryGetValue(player.Slot, out var allTeams))
                {
                    foreach (var kvp in allTeams)
                        if (kvp.Value.TryGetValue(gloveId, out var twi))
                            LogDebug($"[OstoraWeaponSkins] Glove debug: team={kvp.Key} paint={twi.Paint} seed={twi.Seed}");
                }
                LogDebug($"[OstoraWeaponSkins] Gloves applied: team={team} defindex={gloveId} paint={paint} seed={seed}");
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OstoraWeaponSkins] Error applying gloves: {ex.Message}");
        }
    }

    internal static void GivePlayerAgent(IPlayer player)
    {
        if (!GPlayersAgent.TryGetValue(player.Slot, out var value)) return;

        var controller = player.Controller;
        if (controller == null) return;

        var model = controller.TeamNum == 3 ? value.CT : value.T;
        if (string.IsNullOrEmpty(model)) return;

        var pawn = player.PlayerPawn;
        if (pawn == null) return;

        try
        {
            _ = pawn.SetModelAsync($"agents/models/{model}.vmdl");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OstoraWeaponSkins] Error setting agent model: {ex.Message}");
        }

        // Re-apply after a short delay to override CS2's loadout system
        Core.Scheduler.DelayBySeconds(0.3f, () =>
        {
            try
            {
                if (!player.IsValid || player.PlayerPawn == null || !player.PlayerPawn.IsValid) return;
                if (!GPlayersAgent.TryGetValue(player.Slot, out var agents2)) return;
                var model2 = player.Controller.TeamNum == 3 ? agents2.CT : agents2.T;
                if (string.IsNullOrEmpty(model2)) return;
                _ = player.PlayerPawn.SetModelAsync($"agents/models/{model2}.vmdl");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OstoraWeaponSkins] Error reapplying agent: {ex.Message}");
            }
        });
    }

    internal static void GivePlayerMusicKit(IPlayer player)
    {
        if (player.IsFakeClient) return;

        var controller = player.Controller;
        if (controller == null) return;

        int team = controller.TeamNum;

        if (!GPlayersMusic.TryGetValue(player.Slot, out var musicInfo) ||
            !musicInfo.TryGetValue(team, out var musicId) || musicId == 0)
            return;

        if (controller.InventoryServices == null) return;

        controller.InventoryServices.MusicID = musicId;
    }

    internal static void RefreshWeapons(IPlayer player)
    {
        if (!_gBCommandsAllowed) return;

        var controller = player.Controller;
        if (controller == null) return;

        var pawn = player.PlayerPawn;
        if (pawn == null || !pawn.IsValid || !player.IsAlive)
            return;

        if (pawn.WeaponServices == null)
            return;

        var weapons = pawn.WeaponServices.MyWeapons;
        if (weapons.Count == 0)
            return;

        int team = controller.TeamNum;
        if (team == 0 || team == 1)
            return;

        bool hasKnife = false;
        Dictionary<string, List<(int, int)>> weaponsWithAmmo = [];

        // Track the player's active weapon so we can restore it after refresh
        gear_slot_t? activeGearSlot = null;
        string? activeWeaponName = null;

        foreach (var weaponHandle in weapons)
        {
            if (!weaponHandle.IsValid || weaponHandle.Value == null ||
                !weaponHandle.Value.IsValid || !weaponHandle.Value.DesignerName.Contains("weapon_"))
                continue;

            var weapon = weaponHandle.Value;

            try
            {
                var weaponVData = weapon.As<CCSWeaponBase>().WeaponBaseVData;
                if (weaponVData == null) continue;

                var gearSlot = weaponVData.GearSlot;

                if (gearSlot is gear_slot_t.GEAR_SLOT_RIFLE or gear_slot_t.GEAR_SLOT_PISTOL)
                {
                    if (!WeaponDefindex.TryGetValue(weapon.AttributeManager.Item.ItemDefinitionIndex, out var weaponByDefindex))
                        continue;

                    int clip1 = weapon.Clip1;
                    int reservedAmmo = weapon.ReserveAmmo[0];

                    if (!weaponsWithAmmo.TryGetValue(weaponByDefindex, out var value))
                    {
                        value = [];
                        weaponsWithAmmo.Add(weaponByDefindex, value);
                    }

                    value.Add((clip1, reservedAmmo));

                    // Record which weapon the player is currently holding
                    if (pawn.WeaponServices?.ActiveWeapon.Value == weapon)
                    {
                        activeGearSlot = gearSlot;
                        activeWeaponName = weaponByDefindex;
                    }

                    weapon.AddEntityIOEvent<string?>("Kill", null, activator: null, caller: null, delay: 0.1f);
                }

                if (gearSlot == gear_slot_t.GEAR_SLOT_KNIFE)
                {
                    weapon.AddEntityIOEvent<string?>("Kill", null, activator: null, caller: null, delay: 0.1f);
                    hasKnife = true;
                }
            }
            catch { }
        }

        Core.Scheduler.DelayBySeconds(0.23f, () =>
        {
            if (!_gBCommandsAllowed) return;

            var ctrl = player.Controller;
            if (ctrl == null) return;

            var playerPawn = player.PlayerPawn;
            if (playerPawn == null) return;

            if (!PlayerHasKnife(player) && hasKnife)
            {
                playerPawn.ItemServices?.GiveItem("weapon_knife");
            }

            foreach (var entry in weaponsWithAmmo)
            {
                foreach (var ammo in entry.Value)
                {
                    var newWeapon = playerPawn.ItemServices?.GiveItem<CBasePlayerWeapon>(entry.Key);
                    if (newWeapon != null)
                    {
                        LogDebug($"[OstoraWeaponSkins] RefreshWeapons: gave {entry.Key}, applying skin directly...");
                        GivePlayerWeaponSkin(player, newWeapon);

                        Core.Scheduler.NextTick(() =>
                        {
                            try
                            {
                                newWeapon.Clip1 = ammo.Item1;
                                newWeapon.ReserveAmmo[0] = ammo.Item2;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[OstoraWeaponSkins] Error setting ammo: {ex.Message}");
                            }
                        });
                    }
                }
            }

            // Restore the player's previously held weapon
            if (activeWeaponName != null && player.IsAlive)
            {
                string? slotCmd = activeGearSlot switch
                {
                    gear_slot_t.GEAR_SLOT_RIFLE => "slot1",
                    gear_slot_t.GEAR_SLOT_PISTOL => "slot2",
                    gear_slot_t.GEAR_SLOT_KNIFE => "slot3",
                    _ => null,
                };
                if (slotCmd != null)
                    player.ExecuteCommand(slotCmd);
            }
        });
    }

    private static bool PlayerHasKnife(IPlayer player)
    {
        var pawn = player.PlayerPawn;
        if (pawn == null || !pawn.IsValid || pawn.WeaponServices == null)
            return false;

        var weapons = pawn.WeaponServices.MyWeapons;

        foreach (var weaponHandle in weapons)
        {
            if (!weaponHandle.IsValid || weaponHandle.Value == null || !weaponHandle.Value.IsValid) continue;
            if (weaponHandle.Value.DesignerName.Contains("knife") || weaponHandle.Value.DesignerName.Contains("bayonet"))
                return true;
        }

        return false;
    }

    private static void SubclassChange(CEntityInstance weapon, ushort itemD)
    {
        try
        {
            weapon.AcceptInput("ChangeSubclass", value: itemD.ToString());
        }
        catch { }
    }

    private static void UpdateWeaponMeshGroupMask(CEntityInstance weapon, bool isLegacy = false)
    {
        try
        {
            weapon.AcceptInput("SetBodygroup", value: $"body,{(isLegacy ? 1 : 0)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OstoraWeaponSkins] Error in UpdateWeaponMeshGroupMask: {ex.Message}");
        }
    }

    /// <summary>
    /// Reinterprets an integer's bits as a float. Required because CS2 stores
    /// integer IDs (paint kit, seed, sticker ID, etc.) as float bit patterns
    /// in the attribute system. (int)5 → float with bits 0x00000005 = 7e-45.
    /// </summary>
    private static float IntAsFloat(int value) => BitConverter.Int32BitsToSingle(value);

    private static ulong NextItemId()
    {
        return Interlocked.Increment(ref _nextItemId);
    }
}
