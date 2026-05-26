using SwiftlyS2.Shared.SchemaDefinitions;
using SwiftlyS2.Core.Hooks;

namespace SwiftlyS2.Core.Datamaps;

internal partial class DatamapFunctionManager
{
    public HookManager HookManager { get; }


    public DatamapFunctionManager(HookManager hookManager)
    {
        HookManager = hookManager;
    }
}