using System.Runtime.InteropServices;
using SwiftlyS2.Shared.Misc;
using SwiftlyS2.Core.Natives;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.Profiler;
using SwiftlyS2.Shared.Commands;
using SwiftlyS2.Shared.Permissions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SwiftlyS2.Core.Models;
using SwiftlyS2.Core.Translations;

namespace SwiftlyS2.Core.Commands;

internal delegate void CommandCallbackDelegate( int playerId, nint args, nint commandName, nint prefix, byte slient );
internal delegate HookResult ClientCommandListenerCallbackDelegate( int playerId, nint commandLine );
internal delegate HookResult ClientChatListenerCallbackDelegate( int playerId, nint text, byte teamonly );

internal abstract class CommandCallbackBase : IDisposable
{
    public Guid Guid { get; protected init; }
    public string PluginName { get; protected init; }
    public IContextedProfilerService Profiler { get; }
    public ILoggerFactory LoggerFactory { get; }

    protected CommandCallbackBase( ILoggerFactory loggerFactory, IContextedProfilerService profiler, string pluginName )
    {
        LoggerFactory = loggerFactory;
        Profiler = profiler;
        PluginName = pluginName;
    }

    public abstract void Dispose();
}

internal class CommandCallback : CommandCallbackBase
{
    public string CommandName { get; protected init; }
    public bool RegisterRaw { get; protected init; }
    public string Permission { get; protected init; }
    public string HelpText { get; protected init; }

    private readonly ICommandService.CommandListener commandHandle;
    private readonly CommandCallbackDelegate commandCallback;

    private readonly nint commandCallbackPtr;
    private readonly ulong nativeListenerId;
    private readonly ILogger<CommandCallback> logger;

    public CommandCallback( string commandName, bool registerRaw, ICommandService.CommandListener handler, string permission, string helpText, IPlayerManagerService playerManagerService, IPermissionManager permissionManager, IOptionsMonitor<CommandOverrideConfig> commandOverrideOptions, ILoggerFactory loggerFactory, IContextedProfilerService profiler, string pluginName ) : base(loggerFactory, profiler, pluginName)
    {
        this.logger = LoggerFactory.CreateLogger<CommandCallback>();

        Guid = Guid.NewGuid();

        CommandName = commandName;
        RegisterRaw = registerRaw;
        Permission = permission;
        HelpText = helpText;
        commandHandle = handler;
        commandCallback = ( playerId, argsPtr, commandNamePtr, prefixPtr, slient ) =>
        {
            try
            {
                var category = "CommandCallback::" + CommandName;
                Profiler.StartRecording(category);
                var argsString = Marshal.PtrToStringUTF8(argsPtr)!;
                var commandNameString = Marshal.PtrToStringUTF8(commandNamePtr)!;
                var prefixString = Marshal.PtrToStringUTF8(prefixPtr)!;

                var args = argsString.Split('\x01').ToArray();
                if (args.Length < 2) args = [.. args.Where(s => !string.IsNullOrWhiteSpace(s))];
                var context = new CommandContext(playerId, args, commandNameString, prefixString, slient == 1);
                var hasOverride = commandOverrideOptions.CurrentValue.Permissions.TryGetValue(commandNameString, out var overriddenPermission);
                var requiredPermission = hasOverride ? overriddenPermission : Permission;
                if (!context.IsSentByPlayer || string.IsNullOrWhiteSpace(requiredPermission) || permissionManager.PlayerHasPermission(playerManagerService.GetPlayer(playerId)?.SteamID ?? 0, requiredPermission))
                {
                    commandHandle(context);
                }
                else
                {
                    context.Reply(GlobalLocalization.PermissionCommandDenied());
                }
                Profiler.StopRecording(category);
            }
            catch (Exception e)
            {
                if (!GlobalExceptionHandler.Handle(ref e)) return;
                logger.LogError(e, "Failed to handle command {CommandName}.", commandName);
            }
        };

        commandCallbackPtr = Marshal.GetFunctionPointerForDelegate(commandCallback);
        nativeListenerId = NativeCommands.RegisterCommand(commandName, commandCallbackPtr, registerRaw, helpText);
    }

    public override void Dispose()
    {
        NativeCommands.UnregisterCommand(nativeListenerId);
    }
}

internal class ClientCommandListenerCallback : CommandCallbackBase
{
    private readonly ICommandService.ClientCommandHandler commandHandle;
    private readonly ClientCommandListenerCallbackDelegate commandCallback;
    private readonly nint commandCallbackPtr;
    private readonly ulong nativeListenerId;
    private readonly ILogger<ClientCommandListenerCallback> logger;

    public ClientCommandListenerCallback( ICommandService.ClientCommandHandler handler, ILoggerFactory loggerFactory, IContextedProfilerService profiler, string pluginName ) : base(loggerFactory, profiler, pluginName)
    {
        logger = LoggerFactory.CreateLogger<ClientCommandListenerCallback>();
        Guid = Guid.NewGuid();

        commandHandle = handler;
        commandCallback = ( playerId, commandLinePtr ) =>
        {
            try
            {
                var category = "ClientCommandListenerCallback";
                Profiler.StartRecording(category);
                var commandLineString = Marshal.PtrToStringUTF8(commandLinePtr)!;
                var result = commandHandle(playerId, commandLineString);
                Profiler.StopRecording(category);
                return result;
            }
            catch (Exception e)
            {
                if (!GlobalExceptionHandler.Handle(ref e)) return HookResult.Continue;
                logger.LogError(e, "Failed to handle client command listener.");
                return HookResult.Continue;
            }
        };

        commandCallbackPtr = Marshal.GetFunctionPointerForDelegate(commandCallback);
        nativeListenerId = NativeCommands.RegisterClientCommandsListener(commandCallbackPtr);
    }

    public override void Dispose()
    {
        NativeCommands.UnregisterClientCommandsListener(nativeListenerId);
    }
}

internal class ClientChatListenerCallback : CommandCallbackBase
{
    private readonly ICommandService.ClientChatHandler commandHandle;
    private readonly ClientChatListenerCallbackDelegate commandCallback;
    private readonly nint commandCallbackPtr;
    private readonly ulong nativeListenerId;
    private readonly ILogger<ClientChatListenerCallback> logger;

    public ClientChatListenerCallback( ICommandService.ClientChatHandler handler, ILoggerFactory loggerFactory, IContextedProfilerService profiler, string pluginName ) : base(loggerFactory, profiler, pluginName)
    {
        logger = LoggerFactory.CreateLogger<ClientChatListenerCallback>();
        Guid = Guid.NewGuid();

        commandHandle = handler;
        commandCallback = ( playerId, textPtr, teamonly ) =>
        {
            try
            {
                var category = "ClientChatListenerCallback";
                Profiler.StartRecording(category);
                var textString = Marshal.PtrToStringUTF8(textPtr)!;
                var result = commandHandle(playerId, textString, teamonly == 1);
                Profiler.StopRecording(category);
                return result;
            }
            catch (Exception e)
            {
                if (!GlobalExceptionHandler.Handle(ref e)) return HookResult.Continue;
                logger.LogError(e, "Failed to handle client chat listener.");
                return HookResult.Continue;
            }
        };

        commandCallbackPtr = Marshal.GetFunctionPointerForDelegate(commandCallback);
        nativeListenerId = NativeCommands.RegisterClientChatListener(commandCallbackPtr);
    }

    public override void Dispose()
    {
        NativeCommands.UnregisterClientChatListener(nativeListenerId);
    }
}