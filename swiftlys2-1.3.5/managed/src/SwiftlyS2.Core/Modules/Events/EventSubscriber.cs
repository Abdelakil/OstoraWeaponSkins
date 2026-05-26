using Microsoft.Extensions.Logging;
using SwiftlyS2.Shared.Events;
using SwiftlyS2.Shared.Profiler;

namespace SwiftlyS2.Core.Events;

/// <summary>
/// Plugin-scoped custom event subscriber.
/// </summary>
internal class EventSubscriber : IEventSubscriber, IDisposable
{
    private readonly IContextedProfilerService profiler;
    private readonly ILogger<EventSubscriber> logger;

    private volatile bool disposed;

    public bool Disposed => disposed;

    public EventSubscriber( IContextedProfilerService profiler, ILogger<EventSubscriber> logger )
    {
        this.profiler = profiler;
        this.logger = logger;
        this.disposed = false;
        EventPublisher.Subscribe(this);
    }

    ~EventSubscriber()
    {
        Dispose();
    }

    public event EventDelegates.OnTick? OnTick;
    public event EventDelegates.OnWorldUpdate? OnWorldUpdate;
    public event EventDelegates.OnClientConnected? OnClientConnected;
    public event EventDelegates.OnClientDisconnected? OnClientDisconnected;
    public event EventDelegates.OnClientKeyStateChanged? OnClientKeyStateChanged;
    public event EventDelegates.OnClientPutInServer? OnClientPutInServer;
    public event EventDelegates.OnClientSteamAuthorize? OnClientSteamAuthorize;
    public event EventDelegates.OnClientSteamAuthorizeFail? OnClientSteamAuthorizeFail;
    public event EventDelegates.OnEntityCreated? OnEntityCreated;
    public event EventDelegates.OnEntityDeleted? OnEntityDeleted;
    public event EventDelegates.OnEntityParentChanged? OnEntityParentChanged;
    public event EventDelegates.OnEntitySpawned? OnEntitySpawned;
    public event EventDelegates.OnMapLoad? OnMapLoad;
    public event EventDelegates.OnMapUnload? OnMapUnload;
    public event EventDelegates.OnClientProcessUsercmds? OnClientProcessUsercmds;
    public event EventDelegates.OnConVarValueChanged? OnConVarValueChanged;
    public event EventDelegates.OnConCommandCreated? OnConCommandCreated;
    public event EventDelegates.OnConVarCreated? OnConVarCreated;
    public event EventDelegates.OnEntityTakeDamage? OnEntityTakeDamage;
    public event EventDelegates.OnPrecacheResource? OnPrecacheResource;
    public event EventDelegates.OnEntityStartTouch? OnEntityStartTouch;
    public event EventDelegates.OnEntityTouch? OnEntityTouch;
    public event EventDelegates.OnEntityEndTouch? OnEntityEndTouch;
    public event EventDelegates.OnItemServicesCanAcquireHook? OnItemServicesCanAcquireHook;
    public event EventDelegates.OnWeaponServicesCanUseHook? OnWeaponServicesCanUseHook;
    public event EventDelegates.OnWeaponServicesDropWeaponHook? OnWeaponServicesDropWeaponHook;
    public event EventDelegates.OnConsoleOutput? OnConsoleOutput;
    public event EventDelegates.OnCommandExecuteHook? OnCommandExecuteHook;
    public event EventDelegates.OnSteamAPIActivated? OnSteamAPIActivated;
    public event EventDelegates.OnMovementServicesRunCommandHook? OnMovementServicesRunCommandHook;
    public event EventDelegates.OnPlayerPawnPostThink? OnPlayerPawnPostThink;
    public event EventDelegates.OnEntityIdentityAcceptInputHook? OnEntityIdentityAcceptInputHook;
    public event EventDelegates.OnEntityFireOutputHookEvent? OnEntityFireOutputHook;
    public event EventDelegates.OnStartupServer? OnStartupServer;
    public event EventDelegates.OnClientVoice? OnClientVoice;

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }
        disposed = true;

        EventPublisher.Unsubscribe(this);
        GC.SuppressFinalize(this);
    }

    public void InvokeOnTick()
    {
        try
        {
            if (OnTick == null)
            {
                return;
            }
            profiler.StartRecording("Event::OnTick");
            OnTick.Invoke();
        }
        catch (Exception e)
        {
            if (GlobalExceptionHandler.Handle(ref e))
            {
                logger.LogError(e, "Error invoking OnTick.");
            }
        }
        finally
        {
            profiler.StopRecording("Event::OnTick");
        }
    }

    public void InvokeOnWorldUpdate()
    {
        try
        {
            if (OnWorldUpdate == null)
            {
                return;
            }
            profiler.StartRecording("Event::OnWorldUpdate");
            OnWorldUpdate.Invoke();
        }
        catch (Exception e)
        {
            if (GlobalExceptionHandler.Handle(ref e))
            {
                logger.LogError(e, "Error invoking OnWorldUpdate.");
            }
        }
        finally
        {
            profiler.StopRecording("Event::OnWorldUpdate");
        }
    }

    public void InvokeOnClientConnected( ref OnClientConnectedEvent @event )
    {
        try
        {
            if (OnClientConnected == null)
            {
                return;
            }
            profiler.StartRecording("Event::OnClientConnected");
            OnClientConnected.Invoke(@event);
        }
        catch (Exception e)
        {
            if (GlobalExceptionHandler.Handle(ref e))
            {
                logger.LogError(e, "Error invoking OnClientConnected.");
            }
        }
        finally
        {
            profiler.StopRecording("Event::OnClientConnected");
        }
    }

    public void InvokeOnClientDisconnected( ref OnClientDisconnectedEvent @event )
    {
        try
        {
            if (OnClientDisconnected == null)
            {
                return;
            }
            profiler.StartRecording("Event::OnClientDisconnected");
            OnClientDisconnected.Invoke(@event);
        }
        catch (Exception e)
        {
            if (GlobalExceptionHandler.Handle(ref e))
            {
                logger.LogError(e, "Error invoking OnClientDisconnected.");
            }
        }
        finally
        {
            profiler.StopRecording("Event::OnClientDisconnected");
        }
    }

    public void InvokeOnClientKeyStateChanged( ref OnClientKeyStateChangedEvent @event )
    {
        try
        {
            if (OnClientKeyStateChanged == null)
            {
                return;
            }
            profiler.StartRecording("Event::OnClientKeyStateChanged");
            OnClientKeyStateChanged.Invoke(@event);
        }
        catch (Exception e)
        {
            if (GlobalExceptionHandler.Handle(ref e))
            {
                logger.LogError(e, "Error invoking OnClientKeyStateChanged.");
            }
        }
        finally
        {
            profiler.StopRecording("Event::OnClientKeyStateChanged");
        }
    }

    public void InvokeOnClientPutInServer( ref OnClientPutInServerEvent @event )
    {
        try
        {
            if (OnClientPutInServer == null)
            {
                return;
            }
            profiler.StartRecording("Event::OnClientPutInServer");
            OnClientPutInServer.Invoke(@event);
        }
        catch (Exception e)
        {
            if (GlobalExceptionHandler.Handle(ref e))
            {
                logger.LogError(e, "Error invoking OnClientPutInServer.");
            }
        }
        finally
        {
            profiler.StopRecording("Event::OnClientPutInServer");
        }
    }

    public void InvokeOnClientSteamAuthorize( ref OnClientSteamAuthorizeEvent @event )
    {
        try
        {
            if (OnClientSteamAuthorize == null)
            {
                return;
            }
            profiler.StartRecording("Event::OnClientSteamAuthorize");
            OnClientSteamAuthorize.Invoke(@event);
        }
        catch (Exception e)
        {
            if (GlobalExceptionHandler.Handle(ref e))
            {
                logger.LogError(e, "Error invoking OnClientSteamAuthorize.");
            }
        }
        finally
        {
            profiler.StopRecording("Event::OnClientSteamAuthorize");
        }
    }

    public void InvokeOnClientSteamAuthorizeFail( ref OnClientSteamAuthorizeFailEvent @event )
    {
        try
        {
            if (OnClientSteamAuthorizeFail == null)
            {
                return;
            }
            profiler.StartRecording("Event::OnClientSteamAuthorizeFail");
            OnClientSteamAuthorizeFail.Invoke(@event);
        }
        catch (Exception e)
        {
            if (GlobalExceptionHandler.Handle(ref e))
            {
                logger.LogError(e, "Error invoking OnClientSteamAuthorizeFail.");
            }
        }
        finally
        {
            profiler.StopRecording("Event::OnClientSteamAuthorizeFail");
        }
    }

    public void InvokeOnEntityCreated( ref OnEntityCreatedEvent @event )
    {
        try
        {
            if (OnEntityCreated == null)
            {
                return;
            }
            profiler.StartRecording("Event::OnEntityCreated");
            OnEntityCreated.Invoke(@event);
        }
        catch (Exception e)
        {
            if (GlobalExceptionHandler.Handle(ref e))
            {
                logger.LogError(e, "Error invoking OnEntityCreated.");
            }
        }
        finally
        {
            profiler.StopRecording("Event::OnEntityCreated");
        }
    }

    public void InvokeOnClientVoice( ref OnClientVoiceEvent @event )
    {
        try
        {
            if (OnClientVoice == null)
            {
                return;
            }
            profiler.StartRecording("Event::OnClientVoice");
            OnClientVoice.Invoke(@event);
        }
        catch (Exception e)
        {
            if (GlobalExceptionHandler.Handle(ref e))
            {
                logger.LogError(e, "Error invoking OnClientVoice.");
            }
        }
        finally
        {
            profiler.StopRecording("Event::OnClientVoice");
        }
    }

    public void InvokeOnEntityDeleted( ref OnEntityDeletedEvent @event )
    {
        try
        {
            if (OnEntityDeleted == null)
            {
                return;
            }
            profiler.StartRecording("Event::OnEntityDeleted");
            OnEntityDeleted.Invoke(@event);
        }
        catch (Exception e)
        {
            if (GlobalExceptionHandler.Handle(ref e))
            {
                logger.LogError(e, "Error invoking OnEntityDeleted.");
            }
        }
        finally
        {
            profiler.StopRecording("Event::OnEntityDeleted");
        }
    }

    public void InvokeOnEntityParentChanged( ref OnEntityParentChangedEvent @event )
    {
        try
        {
            if (OnEntityParentChanged == null)
            {
                return;
            }
            profiler.StartRecording("Event::OnEntityParentChanged");
            OnEntityParentChanged.Invoke(@event);
        }
        catch (Exception e)
        {
            if (GlobalExceptionHandler.Handle(ref e))
            {
                logger.LogError(e, "Error invoking OnEntityParentChanged.");
            }
        }
        finally
        {
            profiler.StopRecording("Event::OnEntityParentChanged");
        }
    }

    public void InvokeOnEntitySpawned( ref OnEntitySpawnedEvent @event )
    {
        try
        {
            if (OnEntitySpawned == null)
            {
                return;
            }
            profiler.StartRecording("Event::OnEntitySpawned");
            OnEntitySpawned.Invoke(@event);
        }
        catch (Exception e)
        {
            if (GlobalExceptionHandler.Handle(ref e))
            {
                logger.LogError(e, "Error invoking OnEntitySpawned.");
            }
        }
        finally
        {
            profiler.StopRecording("Event::OnEntitySpawned");
        }
    }

    public void InvokeOnMapLoad( ref OnMapLoadEvent @event )
    {
        try
        {
            if (OnMapLoad == null)
            {
                return;
            }
            profiler.StartRecording("Event::OnMapLoad");
            OnMapLoad.Invoke(@event);
        }
        catch (Exception e)
        {
            if (GlobalExceptionHandler.Handle(ref e))
            {
                logger.LogError(e, "Error invoking OnMapLoad.");
            }
        }
        finally
        {
            profiler.StopRecording("Event::OnMapLoad");
        }
    }

    public void InvokeOnMapUnload( ref OnMapUnloadEvent @event )
    {
        try
        {
            if (OnMapUnload == null)
            {
                return;
            }
            profiler.StartRecording("Event::OnMapUnload");
            OnMapUnload.Invoke(@event);
        }
        catch (Exception e)
        {
            if (GlobalExceptionHandler.Handle(ref e))
            {
                logger.LogError(e, "Error invoking OnMapUnload.");
            }
        }
        finally
        {
            profiler.StopRecording("Event::OnMapUnload");
        }
    }

    public void InvokeOnClientProcessUsercmds( ref OnClientProcessUsercmdsEvent @event )
    {
        try
        {
            if (OnClientProcessUsercmds == null)
            {
                return;
            }
            profiler.StartRecording("Event::OnClientProcessUsercmds");
            OnClientProcessUsercmds.Invoke(@event);
        }
        catch (Exception e)
        {
            if (GlobalExceptionHandler.Handle(ref e))
            {
                logger.LogError(e, "Error invoking OnClientProcessUsercmds.");
            }
        }
        finally
        {
            profiler.StopRecording("Event::OnClientProcessUsercmds");
        }
    }

    public void InvokeOnEntityTakeDamage( ref OnEntityTakeDamageEvent @event )
    {
        try
        {
            if (OnEntityTakeDamage == null)
            {
                return;
            }
            profiler.StartRecording("Event::OnEntityTakeDamage");
            OnEntityTakeDamage.Invoke(@event);
        }
        catch (Exception e)
        {
            if (GlobalExceptionHandler.Handle(ref e))
            {
                logger.LogError(e, "Error invoking OnEntityTakeDamage.");
            }
        }
        finally
        {
            profiler.StopRecording("Event::OnEntityTakeDamage");
        }
    }

    public void InvokeOnPrecacheResource( ref OnPrecacheResourceEvent @event )
    {
        try
        {
            if (OnPrecacheResource == null)
            {
                return;
            }
            profiler.StartRecording("Event::OnPrecacheResource");
            OnPrecacheResource.Invoke(@event);
        }
        catch (Exception e)
        {
            if (GlobalExceptionHandler.Handle(ref e))
            {
                logger.LogError(e, "Error invoking OnPrecacheResource.");
            }
        }
        finally
        {
            profiler.StopRecording("Event::OnPrecacheResource");
        }
    }

    public void InvokeOnEntityStartTouch( ref OnEntityStartTouchEvent @event )
    {
        try
        {
            if (OnEntityStartTouch == null)
            {
                return;
            }
            profiler.StartRecording("Event::OnEntityStartTouch");
            OnEntityStartTouch.Invoke(@event);
        }
        catch (Exception e)
        {
            if (GlobalExceptionHandler.Handle(ref e))
            {
                logger.LogError(e, "Error invoking OnEntityStartTouch.");
            }
        }
        finally
        {
            profiler.StopRecording("Event::OnEntityStartTouch");
        }
    }

    public void InvokeOnEntityTouch( ref OnEntityTouchEvent @event )
    {
        try
        {
            if (OnEntityTouch == null)
            {
                return;
            }
            profiler.StartRecording("Event::OnEntityTouch");
            OnEntityTouch.Invoke(@event);
        }
        catch (Exception e)
        {
            if (GlobalExceptionHandler.Handle(ref e))
            {
                logger.LogError(e, "Error invoking OnEntityTouch.");
            }
        }
        finally
        {
            profiler.StopRecording("Event::OnEntityTouch");
        }
    }

    public void InvokeOnEntityEndTouch( ref OnEntityEndTouchEvent @event )
    {
        try
        {
            if (OnEntityEndTouch == null)
            {
                return;
            }
            profiler.StartRecording("Event::OnEntityEndTouch");
            OnEntityEndTouch.Invoke(@event);
        }
        catch (Exception e)
        {
            if (GlobalExceptionHandler.Handle(ref e))
            {
                logger.LogError(e, "Error invoking OnEntityEndTouch.");
            }
        }
        finally
        {
            profiler.StopRecording("Event::OnEntityEndTouch");
        }
    }

    public void InvokeOnSteamAPIActivatedHook()
    {
        try
        {
            if (OnSteamAPIActivated == null)
            {
                return;
            }
            profiler.StartRecording("Event::OnSteamAPIActivatedHook");
            OnSteamAPIActivated.Invoke();
        }
        catch (Exception e)
        {
            if (GlobalExceptionHandler.Handle(ref e))
            {
                logger.LogError(e, "Error invoking OnSteamAPIActivatedHook.");
            }
        }
        finally
        {
            profiler.StopRecording("Event::OnSteamAPIActivatedHook");
        }
    }

    public void InvokeOnItemServicesCanAcquireHook( ref OnItemServicesCanAcquireHookEvent @event )
    {
        try
        {
            if (OnItemServicesCanAcquireHook == null)
            {
                return;
            }
            profiler.StartRecording("Event::OnItemServicesCanAcquireHook");
            OnItemServicesCanAcquireHook.Invoke(@event);
        }
        catch (Exception e)
        {
            if (GlobalExceptionHandler.Handle(ref e))
            {
                logger.LogError(e, "Error invoking OnItemServicesCanAcquireHook.");
            }
        }
        finally
        {
            profiler.StopRecording("Event::OnItemServicesCanAcquireHook");
        }
    }

    public void InvokeOnWeaponServicesCanUseHook( ref OnWeaponServicesCanUseHookEvent @event )
    {
        try
        {
            if (OnWeaponServicesCanUseHook == null)
            {
                return;
            }
            profiler.StartRecording("Event::OnWeaponServicesCanUseHook");
            OnWeaponServicesCanUseHook.Invoke(@event);
        }
        catch (Exception e)
        {
            if (GlobalExceptionHandler.Handle(ref e))
            {
                logger.LogError(e, "Error invoking OnWeaponServicesCanUseHook.");
            }
        }
        finally
        {
            profiler.StopRecording("Event::OnWeaponServicesCanUseHook");
        }
    }

    public void InvokeOnConsoleOutput( ref OnConsoleOutputEvent @event )
    {
        try
        {
            if (OnConsoleOutput == null)
            {
                return;
            }
            profiler.StartRecording("Event::OnConsoleOutput");
            OnConsoleOutput.Invoke(@event);
        }
        catch (Exception e)
        {
            if (GlobalExceptionHandler.Handle(ref e))
            {
                logger.LogError(e, "Error invoking OnConsoleOutput.");
            }
        }
        finally
        {
            profiler.StopRecording("Event::OnConsoleOutput");
        }
    }

    public void InvokeOnConVarValueChanged( ref OnConVarValueChanged @event )
    {
        try
        {
            if (OnConVarValueChanged == null)
            {
                return;
            }
            profiler.StartRecording("Event::OnConVarValueChanged");
            OnConVarValueChanged.Invoke(@event);
        }
        catch (Exception e)
        {
            if (GlobalExceptionHandler.Handle(ref e))
            {
                logger.LogError(e, "Error invoking OnConVarValueChanged.");
            }
        }
        finally
        {
            profiler.StopRecording("Event::OnConVarValueChanged");
        }
    }

    public void InvokeOnConCommandCreated( ref OnConCommandCreated @event )
    {
        try
        {
            if (OnConCommandCreated == null)
            {
                return;
            }
            profiler.StartRecording("Event::OnConCommandCreated");
            OnConCommandCreated.Invoke(@event);
        }
        catch (Exception e)
        {
            if (GlobalExceptionHandler.Handle(ref e))
            {
                logger.LogError(e, "Error invoking OnConCommandCreated.");
            }
        }
        finally
        {
            profiler.StopRecording("Event::OnConCommandCreated");
        }
    }

    public void InvokeOnConVarCreated( ref OnConVarCreated @event )
    {
        try
        {
            if (OnConVarCreated == null)
            {
                return;
            }
            profiler.StartRecording("Event::OnConVarCreated");
            OnConVarCreated.Invoke(@event);
        }
        catch (Exception e)
        {
            if (GlobalExceptionHandler.Handle(ref e))
            {
                logger.LogError(e, "Error invoking OnConVarCreated.");
            }
        }
        finally
        {
            profiler.StopRecording("Event::OnConVarCreated");
        }
    }

    public void InvokeOnCommandExecuteHook( ref OnCommandExecuteHookEvent @event )
    {
        try
        {
            if (OnCommandExecuteHook == null)
            {
                return;
            }
            profiler.StartRecording("Event::OnCommandExecuteHook");
            OnCommandExecuteHook.Invoke(@event);
        }
        catch (Exception e)
        {
            if (GlobalExceptionHandler.Handle(ref e))
            {
                logger.LogError(e, "Error invoking OnCommandExecuteHook.");
            }
        }
        finally
        {
            profiler.StopRecording("Event::OnCommandExecuteHook");
        }
    }

    public void InvokeOnMovementServicesRunCommandHook( ref OnMovementServicesRunCommandHookEvent @event )
    {
        try
        {
            if (OnMovementServicesRunCommandHook == null)
            {
                return;
            }
            profiler.StartRecording("Event::OnMovementServicesRunCommandHook");
            OnMovementServicesRunCommandHook.Invoke(@event);
        }
        catch (Exception e)
        {
            if (GlobalExceptionHandler.Handle(ref e))
            {
                logger.LogError(e, "Error invoking OnMovementServicesRunCommandHook.");
            }
        }
        finally
        {
            profiler.StopRecording("Event::OnMovementServicesRunCommandHook");
        }
    }

    public void InvokeOnPlayerPawnPostThinkHook( ref OnPlayerPawnPostThinkHookEvent @event )
    {
        try
        {
            if (OnPlayerPawnPostThink == null)
            {
                return;
            }
            profiler.StartRecording("Event::OnPlayerPawnPostThink");
            OnPlayerPawnPostThink.Invoke(@event);
        }
        catch (Exception e)
        {
            if (GlobalExceptionHandler.Handle(ref e))
            {
                logger.LogError(e, "Error invoking OnPlayerPawnPostThink.");
            }
        }
        finally
        {
            profiler.StopRecording("Event::OnPlayerPawnPostThink");
        }
    }

    public void InvokeOnEntityIdentityAcceptInputHook( ref OnEntityIdentityAcceptInputHookEvent @event )
    {
        try
        {
            if (OnEntityIdentityAcceptInputHook == null)
            {
                return;
            }
            profiler.StartRecording("Event::OnEntityIdentityAcceptInput");
            OnEntityIdentityAcceptInputHook.Invoke(@event);
        }
        catch (Exception e)
        {
            if (GlobalExceptionHandler.Handle(ref e))
            {
                logger.LogError(e, "Error invoking OnEntityIdentityAcceptInput.");
            }
        }
        finally
        {
            profiler.StopRecording("Event::OnEntityIdentityAcceptInput");
        }
    }

    public void InvokeOnWeaponServicesDropWeaponHook( ref OnWeaponServicesDropWeaponHook @event )
    {
        try
        {
            if (OnWeaponServicesDropWeaponHook == null)
            {
                return;
            }
            profiler.StartRecording("Event::OnWeaponServicesDropWeaponHook");
            OnWeaponServicesDropWeaponHook.Invoke(@event);
        }
        catch (Exception e)
        {
            if (GlobalExceptionHandler.Handle(ref e))
            {
                logger.LogError(e, "Error invoking OnWeaponServicesDropWeaponHook.");
            }
        }
        finally
        {
            profiler.StopRecording("Event::OnWeaponServicesDropWeaponHook");
        }
    }

    public void InvokeOnEntityFireOutputHook( ref OnEntityFireOutputHookEvent @event )
    {
        try
        {
            if (OnEntityFireOutputHook == null)
            {
                return;
            }
            profiler.StartRecording("Event::OnEntityFireOutputHook");
            OnEntityFireOutputHook.Invoke(@event);
        }
        catch (Exception e)
        {
            if (GlobalExceptionHandler.Handle(ref e))
            {
                logger.LogError(e, "Error invoking OnEntityFireOutputHook.");
            }
        }
        finally
        {
            profiler.StopRecording("Event::OnEntityFireOutputHook");
        }
    }

    public void InvokeOnStartupServer()
    {
        try
        {
            if (OnStartupServer == null)
            {
                return;
            }
            profiler.StartRecording("Event::OnStartupServer");
            OnStartupServer.Invoke();
        }
        catch (Exception e)
        {
            if (GlobalExceptionHandler.Handle(ref e))
            {
                logger.LogError(e, "Error invoking OnStartupServer.");
            }
        }
        finally
        {
            profiler.StopRecording("Event::OnStartupServer");
        }
    }
}