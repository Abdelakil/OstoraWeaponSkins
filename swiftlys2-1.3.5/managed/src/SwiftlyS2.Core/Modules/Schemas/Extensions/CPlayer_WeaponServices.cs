using SwiftlyS2.Shared.Misc;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.Schemas;

namespace SwiftlyS2.Shared.SchemaDefinitions;

public partial interface CPlayer_WeaponServices
{
    /// <summary>
    /// Drop a weapon.
    ///
    /// Thread unsafe, use async variant instead for non-main thread context.
    /// </summary>
    /// <param name="weapon">The weapon to drop.</param>
    [ThreadUnsafe]
    public void DropWeapon( CBasePlayerWeapon weapon );

    /// <summary>
    /// Drop a weapon asynchronously.
    /// </summary>
    /// <param name="weapon">The weapon to drop.</param>
    public Task DropWeaponAsync( CBasePlayerWeapon weapon );

    /// <summary>
    /// Drop a weapon.
    ///
    /// Thread unsafe, use async variant instead for non-main thread context.
    /// </summary>
    /// <param name="weapon">The weapon to drop.</param>
    /// <param name="momentum">The momentum to apply to the dropped weapon.</param>
    [ThreadUnsafe]
    public void DropWeapon( CBasePlayerWeapon weapon, Vector momentum );

    /// <summary>
    /// Drop a weapon asynchronously.
    /// </summary>
    /// <param name="weapon">The weapon to drop.</param>
    /// <param name="momentum">The momentum to apply to the dropped weapon.</param>
    public Task DropWeaponAsync( CBasePlayerWeapon weapon, Vector momentum );

    /// <summary>
    /// Drop and remove a weapon.
    ///
    /// Thread unsafe, use async variant instead for non-main thread context.
    /// </summary>
    /// <param name="weapon">The weapon to remove.</param>
    [ThreadUnsafe]
    public void RemoveWeapon( CBasePlayerWeapon weapon );

    /// <summary>
    /// Remove a weapon asynchronously.
    /// </summary>
    /// <param name="weapon">The weapon to remove.</param>
    public Task RemoveWeaponAsync( CBasePlayerWeapon weapon );

    /// <summary>
    /// Make player select a weapon.
    ///
    /// Thread unsafe, use async variant instead for non-main thread context.
    /// </summary>
    /// <param name="weapon">The weapon to select.</param>
    public void SelectWeapon( CBasePlayerWeapon weapon );

    /// <summary>
    /// Make player select a weapon asynchronously.
    /// </summary>
    /// <param name="weapon">The weapon to select.</param>
    public Task SelectWeaponAsync( CBasePlayerWeapon weapon );

    /// <summary>
    /// Drop a weapon by slot.
    /// 
    /// Thread unsafe, use async variant instead for non-main thread context.
    /// </summary>
    /// <param name="slot">The slot to drop the weapon from.</param>
    [ThreadUnsafe]
    public void DropWeaponBySlot( gear_slot_t slot );

    /// <summary>
    /// Drop a weapon by slot.
    /// 
    /// Thread unsafe, use async variant instead for non-main thread context.
    /// </summary>
    /// <param name="slot">The slot to drop the weapon from.</param>
    /// <param name="momentum">The momentum to apply to the dropped weapon.</param>
    [ThreadUnsafe]
    public void DropWeaponBySlot( gear_slot_t slot, Vector momentum );

    /// <summary>
    /// Drop a weapon by slot asynchronously.
    /// </summary>
    /// <param name="slot">The slot to drop the weapon from.</param>
    public Task DropWeaponBySlotAsync( gear_slot_t slot );

    /// <summary>
    /// Drop a weapon by slot asynchronously.
    /// </summary>
    /// <param name="slot">The slot to drop the weapon from.</param>
    /// <param name="momentum">The momentum to apply to the dropped weapon.</param>
    public Task DropWeaponBySlotAsync( gear_slot_t slot, Vector momentum );

    /// <summary>
    /// Remove a weapon by slot.
    /// 
    /// Thread unsafe, use async variant instead for non-main thread context.
    /// </summary>
    /// <param name="slot">The slot to remove the weapon from.</param>
    [ThreadUnsafe]
    public void RemoveWeaponBySlot( gear_slot_t slot );

    /// <summary>
    /// Remove a weapon by slot asynchronously.
    /// </summary>
    /// <param name="slot">The slot to remove the weapon from.</param>
    public Task RemoveWeaponBySlotAsync( gear_slot_t slot );

    /// <summary>
    /// Select a weapon by slot.
    /// 
    /// Thread unsafe, use async variant instead for non-main thread context.
    /// </summary>
    /// <param name="slot">The slot to select the weapon from.</param>
    [ThreadUnsafe]
    public void SelectWeaponBySlot( gear_slot_t slot );

    /// <summary>
    /// Select a weapon by slot asynchronously.
    /// </summary>
    /// <param name="slot">The slot to select the weapon from.</param>
    public Task SelectWeaponBySlotAsync( gear_slot_t slot );

    /// <summary>
    /// Drop a weapon by designer name.
    /// 
    /// Thread unsafe, use async variant instead for non-main thread context.
    /// </summary>
    /// <param name="designerName">The designer name of the weapon to drop.</param>
    [ThreadUnsafe]
    public void DropWeaponByDesignerName( string designerName );

    /// <summary>
    /// Drop a weapon by designer name.
    /// 
    /// Thread unsafe, use async variant instead for non-main thread context.
    /// </summary>
    /// <param name="designerName">The designer name of the weapon to drop.</param>
    /// <param name="momentum">The momentum to apply to the dropped weapon.</param>
    [ThreadUnsafe]
    public void DropWeaponByDesignerName( string designerName, Vector momentum );

    /// <summary>
    /// Drop a weapon by designer name asynchronously.
    /// </summary>
    /// <param name="designerName">The designer name of the weapon to drop.</param>
    public Task DropWeaponByDesignerNameAsync( string designerName );

    /// <summary>
    /// Drop a weapon by designer name asynchronously.
    /// </summary>
    /// <param name="designerName">The designer name of the weapon to drop.</param>
    /// <param name="momentum">The momentum to apply to the dropped weapon.</param>
    public Task DropWeaponByDesignerNameAsync( string designerName, Vector momentum );

    /// <summary>
    /// Remove a weapon by designer name.
    /// 
    /// Thread unsafe, use async variant instead for non-main thread context.
    /// </summary>
    /// <param name="designerName">The designer name of the weapon to remove.</param>
    [ThreadUnsafe]
    public void RemoveWeaponByDesignerName( string designerName );

    /// <summary>
    /// Remove a weapon by designer name asynchronously.
    /// </summary>
    /// <param name="designerName">The designer name of the weapon to remove.</param>
    public Task RemoveWeaponByDesignerNameAsync( string designerName );

    /// <summary>
    /// Select a weapon by designer name.
    /// 
    /// Thread unsafe, use async variant instead for non-main thread context.
    /// </summary>
    /// <param name="designerName">The designer name of the weapon to select.</param>
    [ThreadUnsafe]
    public void SelectWeaponByDesignerName( string designerName );

    /// <summary>
    /// Select a weapon by designer name asynchronously.
    /// </summary>
    /// <param name="designerName">The designer name of the weapon to select.</param>
    public Task SelectWeaponByDesignerNameAsync( string designerName );

    /// <summary>
    /// Drop all weapons with the specified class.
    /// 
    /// Thread unsafe, use async variant instead for non-main thread context.
    /// </summary>
    /// <typeparam name="T">The weapon class.</typeparam>
    [ThreadUnsafe]
    public void DropWeaponByClass<T>() where T : class, ISchemaClass<T>;

    /// <summary>
    /// Drop all weapons with the specified class.
    /// 
    /// Thread unsafe, use async variant instead for non-main thread context.
    /// </summary>
    /// <typeparam name="T">The weapon class.</typeparam>
    /// <param name="momentum">The momentum to apply to the dropped weapon.</param>
    [ThreadUnsafe]
    public void DropWeaponByClass<T>( Vector momentum ) where T : class, ISchemaClass<T>;

    /// <summary>
    /// Drop all weapons with the specified class asynchronously.
    /// </summary>
    /// <typeparam name="T">The weapon class.</typeparam>
    public Task DropWeaponByClassAsync<T>() where T : class, ISchemaClass<T>;

    /// <summary>
    /// Drop all weapons with the specified class asynchronously.
    /// </summary>
    /// <typeparam name="T">The weapon class.</typeparam>
    /// <param name="momentum">The momentum to apply to the dropped weapon.</param>
    public Task DropWeaponByClassAsync<T>( Vector momentum ) where T : class, ISchemaClass<T>;

    /// <summary>
    /// Drop and remove all weapons with the specified class.
    /// 
    /// Thread unsafe, use async variant instead for non-main thread context.
    /// </summary>
    /// <typeparam name="T">The weapon class.</typeparam>
    [ThreadUnsafe]
    public void RemoveWeaponByClass<T>() where T : class, ISchemaClass<T>;

    /// <summary>
    /// Remove all weapons with the specified class asynchronously.
    /// </summary>
    /// <typeparam name="T">The weapon class.</typeparam>
    public Task RemoveWeaponByClassAsync<T>() where T : class, ISchemaClass<T>;

    /// <summary>
    /// Select a weapon by class.
    /// 
    /// Thread unsafe, use async variant instead for non-main thread context.
    /// </summary>
    /// <typeparam name="T">The weapon class.</typeparam>
    [ThreadUnsafe]
    public void SelectWeaponByClass<T>() where T : class, ISchemaClass<T>;

    /// <summary>
    /// Select a weapon by class asynchronously.
    /// </summary>
    /// <typeparam name="T">The weapon class.</typeparam>
    public Task SelectWeaponByClassAsync<T>() where T : class, ISchemaClass<T>;

    public IEnumerable<CBasePlayerWeapon> MyValidWeapons { get; }
}