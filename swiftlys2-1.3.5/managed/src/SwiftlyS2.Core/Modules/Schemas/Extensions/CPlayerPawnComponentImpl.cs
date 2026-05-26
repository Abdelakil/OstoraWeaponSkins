using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace SwiftlyS2.Core.SchemaDefinitions;

internal partial class CPlayerPawnComponentImpl : CPlayerPawnComponent
{
    public CBasePlayerPawn Pawn => __m_pChainEntity.Entity.As<CBasePlayerPawn>();

    public IPlayer? ToPlayer()
    {
        return Pawn.ToPlayer();
    }
}