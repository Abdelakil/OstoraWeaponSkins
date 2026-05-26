using SwiftlyS2.Shared.Misc;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.Schemas;

namespace SwiftlyS2.Shared.SchemaDefinitions;

public partial interface CPlayer_ItemServices
{
    /// <summary>
    /// Give an item to the player.
    ///
    /// Thread unsafe, use async version instead for non-main thread context.
    /// </summary>
    /// <typeparam name="T">The type of the item to give.</typeparam>
    /// <returns>The item that was given.</returns>
    [ThreadUnsafe]
    public T GiveItem<T>() where T : ISchemaClass<T>;

    /// <summary>
    /// Give an item to the player asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the item to give.</typeparam>
    /// <returns>The item that was given.</returns>
    public Task<T> GiveItemAsync<T>() where T : ISchemaClass<T>;

    /// <summary>
    /// Give an item to the player.
    ///
    /// Thread unsafe, use async variant instead for non-main thread context.
    /// </summary>
    /// <param name="itemDesignerName">The designer name of the item to give.</param>
    /// <returns>The item that was given.</returns>
    [ThreadUnsafe]
    public T GiveItem<T>( string itemDesignerName ) where T : ISchemaClass<T>;

    /// <summary>
    /// Give an item to the player asynchronously.
    /// </summary>
    /// <param name="itemDesignerName">The designer name of the item to give.</param>
    /// <returns>The item that was given.</returns>
    public Task<T> GiveItemAsync<T>( string itemDesignerName ) where T : ISchemaClass<T>;

    /// <summary>
    /// Give an item to the player.
    ///
    /// Thread unsafe, use async variant instead for non-main thread context.
    /// </summary>
    /// <param name="itemDesignerName">The designer name of the item to give.</param>
    [ThreadUnsafe]
    public void GiveItem( string itemDesignerName );


    /// <summary>
    /// Give an item to the player asynchronously.
    /// </summary>
    /// <param name="itemDesignerName">The designer name of the item to give.</param>
    /// <returns>The item that was given.</returns>
    public Task GiveItemAsync( string itemDesignerName );

    /// <summary>
    /// Drop the item that player is holding.
    ///
    /// Thread unsafe, use async variant instead for non-main thread context.
    /// </summary>
    [ThreadUnsafe]
    public void DropActiveItem();

    /// <summary>
    /// Drop the item that player is holding asynchronously.
    /// </summary>
    public Task DropActiveItemAsync();

    /// <summary>
    /// Remove all items from the player.
    ///
    /// Thread unsafe, use async variant instead for non-main thread context.
    /// </summary>
    [ThreadUnsafe]
    public void RemoveItems();

    /// <summary>
    /// Remove all items from the player asynchronously.
    /// </summary>
    public Task RemoveItemsAsync();
}