using SwiftlyS2.Shared.SchemaDefinitions;

namespace SwiftlyS2.Core.SchemaDefinitions;

internal partial class CCSPlayerPawnImpl : CCSPlayerPawn
{
    public new CCSPlayer_WeaponServices? WeaponServices => base.WeaponServices?.As<CCSPlayer_WeaponServices>();
    public new CCSPlayer_ItemServices? ItemServices => base.ItemServices?.As<CCSPlayer_ItemServices>();
    public new CCSPlayer_UseServices? UseServices => base.UseServices?.As<CCSPlayer_UseServices>();
    public new CCSPlayer_WaterServices? WaterServices => base.WaterServices?.As<CCSPlayer_WaterServices>();
    public new CCSPlayer_MovementServices? MovementServices => base.MovementServices?.As<CCSPlayer_MovementServices>();
    public new CCSPlayer_CameraServices? CameraServices => base.CameraServices?.As<CCSPlayer_CameraServices>();
}