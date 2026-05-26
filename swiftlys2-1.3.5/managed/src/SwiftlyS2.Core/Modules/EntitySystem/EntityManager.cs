using SwiftlyS2.Core.SchemaDefinitions;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace SwiftlyS2.Core.EntitySystem;

internal static class EntityManager
{
    private static readonly CEntityInstance?[] _Entities = new CEntityInstance?[1 << 15];
    private static readonly List<uint> _ActiveEntityIndices = [];
    private static readonly Dictionary<nint, uint> _PtrToIndex = [];
    private static readonly CEntityInstanceImpl _Dummy = new(0);
    private static readonly Lock _lock = new();

    public static CEntityInstance OnEntityCreated( nint entityPtr )
    {
        var ent = GetEntityByAddress(entityPtr);
        if (ent != null) return ent;

        _Dummy.DangerousSetHandle(entityPtr);
        var index = _Dummy.Index;
        var designerName = _Dummy.DesignerName;
        var entity = ClassConvertor.ConvertEntityByDesignerName(entityPtr, designerName);
        lock (_lock)
        {
            try
            {
                _Entities[index] = entity;
                _ActiveEntityIndices.Add(index);
                _PtrToIndex.Add(entityPtr, index);
                return entity;
            }
            catch
            {
                return entity;
            }
        }
    }

    public static CEntityInstance? GetEntityByIndex( uint index )
    {
        lock (_lock)
        {
            try
            {
                return _Entities[index];
            }
            catch
            {
                return null;
            }
        }
    }

    public static CEntityInstance? GetEntityByAddress( nint address )
    {
        if (address == 0) return null;

        lock (_lock)
        {
            return !_PtrToIndex.TryGetValue(address, out var value) ? null : _Entities[value];
        }
    }

    public static void OnEntityDeleted( nint entityPtr )
    {
        if (entityPtr == 0) return;

        lock (_lock)
        {
            try
            {
                if (!_PtrToIndex.TryGetValue(entityPtr, out var index))
                {
                    return;
                }
                _Entities[index] = null;
                _ = _ActiveEntityIndices.Remove(index);
                _ = _PtrToIndex.Remove(entityPtr);
            }
            catch
            {
            }
        }
    }

    public static IEnumerable<CEntityInstance> GetAllEntities()
    {
        lock (_lock)
        {
            try
            {
                return _ActiveEntityIndices.Select(index => _Entities[index]!);
            }
            catch
            {
                return [];
            }
        }
    }

    public static bool IsAddressValid( nint address )
    {
        if (address == 0) return false;

        lock (_lock)
        {
            return _PtrToIndex.ContainsKey(address);
        }
    }

}
