using SwiftlyS2.Shared.Misc;

namespace SwiftlyS2.Shared.Plugins;

public abstract class BasePlugin : IPlugin
{
    protected ISwiftlyCore Core { get; private init; }

    public BasePlugin( ISwiftlyCore core )
    {
        Core = core;
    }

    public virtual void ConfigureSharedInterface( IInterfaceManager interfaceManager ) { }

    public virtual void UseSharedInterface( IInterfaceManager interfaceManager ) { }

    public virtual void OnSharedInterfaceInjected( IInterfaceManager interfaceManager ) { }

    public virtual void OnAllPluginsLoaded() { }

    public abstract void Load( bool hotReload );

    public abstract void Unload();

    [Obsolete("Not supported.")]
    public virtual PluginReloadMethod ReloadMethod { get; set; } = PluginReloadMethod.Auto;
}