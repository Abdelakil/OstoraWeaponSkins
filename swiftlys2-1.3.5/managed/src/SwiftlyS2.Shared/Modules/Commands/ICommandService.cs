using SwiftlyS2.Shared.Misc;

namespace SwiftlyS2.Shared.Commands;

public interface ICommandService
{
    /// <summary>
    /// The listener for the command.
    /// </summary>
    /// <param name="context">The command context.</param>
    public delegate void CommandListener( ICommandContext context );

    /// <summary>
    /// The handler for the client command hook.
    /// </summary>
    /// <param name="playerId">The player id.</param>
    /// <param name="commandLine">The command line.</param>
    /// <returns>Whether the command should continue to be sent.</returns>
    public delegate HookResult ClientCommandHandler( int playerId, string commandLine );

    /// <summary>
    /// The handler for the client chat hook.
    /// </summary>
    /// <param name="playerId">The player id.</param>
    /// <param name="text">The text.</param>
    /// <param name="teamonly">Whether the text is for team only.</param>
    /// <returns>Whether the text should continue to be sent.</returns>
    public delegate HookResult ClientChatHandler( int playerId, string text, bool teamonly );

    /// <summary>
    /// Registers a command (backward compatibility overload).
    /// </summary>
    /// <param name="commandName">The command name.</param>
    /// <param name="handler">The handler callback for the command.</param>
    /// <param name="registerRaw">If set to true, the command will not starts with a `sw_` prefix.</param>
    /// <param name="permission">The permission required to use the command.</param>
    /// <returns>The guid of the command.</returns>
    public Guid RegisterCommand( string commandName, CommandListener handler, bool registerRaw, string permission );

    /// <summary>
    /// Registers a command.
    /// </summary>
    /// <param name="commandName">The command name.</param>
    /// <param name="handler">The handler callback for the command.</param>
    /// <param name="registerRaw">If set to true, the command will not starts with a `sw_` prefix.</param>
    /// <param name="permission">The permission required to use the command.</param>
    /// <param name="helpText">The help text of the command.</param>
    /// <returns>The guid of the command.</returns>
    public Guid RegisterCommand( string commandName, CommandListener handler, bool registerRaw = false, string permission = "", string helpText = "SwiftlyS2 registered command" );

    /// <summary>
    /// Registers a command alias.
    /// </summary>
    /// <param name="commandName">The command name.</param>
    /// <param name="alias">The alias.</param>
    /// <param name="registerRaw">If set to true, the alias will not starts with a `sw_` prefix.</param>
    public void RegisterCommandAlias( string commandName, string alias, bool registerRaw = false );

    /// <summary>
    /// Unregisters a command.
    /// </summary>
    /// <param name="guid">The guid of the command.</param>
    public void UnregisterCommand( Guid guid );

    /// <summary>
    /// Unregisters all command listeners with the specified command name.
    /// </summary>
    /// <param name="commandName">The command name.</param>
    public void UnregisterCommand( string commandName );

    /// <summary>
    /// Checks if a command is registered.
    /// </summary>
    /// <param name="commandName">The command name.</param>
    /// <returns>Whether the command is registered.</returns>
    public bool IsCommandRegistered( string commandName );

    /// <summary>
    /// Hooks client commands, will be fired when a player sends any command.
    /// </summary>
    /// <param name="handler">The handler callback for the client command.</param>
    public Guid HookClientCommand( ClientCommandHandler handler );

    /// <summary>
    /// Unhooks a client command.
    /// </summary>
    /// <param name="guid">The guid of the client command.</param>
    public void UnhookClientCommand( Guid guid );

    /// <summary>
    /// Hooks client chat, will be fired when a player sends any chat message.
    /// </summary>
    /// <param name="handler">The handler callback for the client chat.</param>
    public Guid HookClientChat( ClientChatHandler handler );

    /// <summary>
    /// Unhooks a client chat.
    /// </summary>
    /// <param name="guid">The guid of the client chat.</param>
    public void UnhookClientChat( Guid guid );

    /// <summary>
    /// Gets all registered commands by plugins.
    /// </summary>
    public List<string> GetAllCommands();

    /// <summary>
    /// Gets all commands registered by a specific plugin.
    /// </summary>
    /// <param name="pluginName">The name of the plugin.</param>
    /// <returns>List of command information registered by the plugin.</returns>
    public List<CommandInfo> GetCommandsByPlugin( string pluginName );

    /// <summary>
    /// Gets all plugins that have registered commands.
    /// </summary>
    /// <returns>Dictionary mapping plugin names to their registered command information.</returns>
    public Dictionary<string, List<CommandInfo>> GetAllCommandsByPlugin();

    /// <summary>
    /// Gets all commands info for the commands registered by plugins.
    /// </summary>
    /// <returns>List of command information registered by the plugin.</returns>
    public List<CommandInfo> GetAllCommandsInfo();
}