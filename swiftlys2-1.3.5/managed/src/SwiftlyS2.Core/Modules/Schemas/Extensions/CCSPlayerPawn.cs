namespace SwiftlyS2.Shared.SchemaDefinitions;

public partial interface CCSPlayerPawn
{
    public new CCSPlayer_WeaponServices? WeaponServices { get; }
    public new CCSPlayer_ItemServices? ItemServices { get; }
    public new CCSPlayer_UseServices? UseServices { get; }
    public new CCSPlayer_WaterServices? WaterServices { get; }
    public new CCSPlayer_MovementServices? MovementServices { get; }
    public new CCSPlayer_CameraServices? CameraServices { get; }
}