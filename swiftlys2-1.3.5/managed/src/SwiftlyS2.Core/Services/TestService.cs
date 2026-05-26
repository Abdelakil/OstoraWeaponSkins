using Microsoft.Extensions.Logging;
using SwiftlyS2.Shared;
using SwiftlyS2.Core.Natives;
using SwiftlyS2.Shared.Events;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Core.Extensions;
using SwiftlyS2.Shared.EntitySystem;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace SwiftlyS2.Core.Services;

internal class TestService
{
    // [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    // internal delegate nint DispatchSpawnHook( nint entity, nint kv );

    private readonly ISwiftlyCore core;

    public TestService( ILogger<TestService> logger, ISwiftlyCore core )
    {
        this.core = core;

        logger.LogWarning("TestService created");
        logger.LogWarning("TestService created");
        logger.LogWarning("TestService created");
        logger.LogWarning("TestService created");
        logger.LogWarning("TestService created");
        logger.LogWarning("TestService created");
        logger.LogWarning("TestService created");
        logger.LogWarning("TestService created");
        logger.LogWarning("TestService created");

        core.Registrator.Register(this);
        Test2();
    }

    public void Test()
    {
        _ = core.Scheduler.RepeatBySeconds(1.0f, () =>
        {
            var gameServer = NativeEngineHelpers.GetNetworkGameServer();
            unsafe
            {
                ref var array = ref gameServer.AsRef<CUtlVector<nint>>(624);
                foreach (var client in array)
                {
                    if (client == 0)
                    {
                        continue;
                    }
                    // ref var serversideClient = ref client.AsRef<CServerSideClient>();
                    // PrintStructFields(serversideClient.Base);
                }
            }
        });
    }

    public void Test2()
    {
        _ = core.Command.RegisterCommand("abc", ( ctx ) =>
        {
            ctx.Reply(ctx.Sender!.Name);
        });


    }



    // [EntityOutputHandler("*", "*")]
    // public void Test3( IOnEntityFireOutputHookEvent @event )
    // {
    //     Console.WriteLine("MFMFMFMFMFMFMFMFMF");
    //     Console.WriteLine($"HookEntityOutput -> designerName: {@event.DesignerName} output: {@event.OutputName}, activator: {@event.Activator?.As<CBaseEntity>()?.DesignerName}, caller: {@event.Caller?.As<CBaseEntity>()?.DesignerName}, value: {@event.VariantValue}, delay: {@event.Delay}");
    // }

    // [EntityInputHandler("*", "*")]
    // public void Test4( IOnEntityIdentityAcceptInputHookEvent @event )
    // {
    //     Console.WriteLine("FMFMFMFMFMFMFMFMFM");
    //     Console.WriteLine($"HookEntityInput -> designerName: {@event.DesignerName} output: {@event.InputName}, activator: {@event.Activator?.As<CBaseEntity>()?.DesignerName}, caller: {@event.Caller?.As<CBaseEntity>()?.DesignerName}, value: {@event.VariantValue}");
    // }

    [EntityOutputHandler<CPropDoorRotating>("*")]
    public void Test3( IOnEntityFireOutputHookEvent @event )
    {
        Console.WriteLine($"HookEntityOutput -> designerName: {@event.DesignerName} output: {@event.OutputName}, activator: {@event.Activator?.As<CBaseEntity>()?.DesignerName}, caller: {@event.Caller?.As<CBaseEntity>()?.DesignerName}, value: {@event.VariantValue}, delay: {@event.Delay}");
    }

    [EntityInputHandler<CCSPlayerPawn>("*")]
    public void Test4( IOnEntityIdentityAcceptInputHookEvent @event )
    {
        Console.WriteLine($"HookEntityInput -> designerName: {@event.DesignerName} output: {@event.InputName}, activator: {@event.Activator?.As<CBaseEntity>()?.DesignerName}, caller: {@event.Caller?.As<CBaseEntity>()?.DesignerName}, value: {@event.VariantValue}");
    }
}