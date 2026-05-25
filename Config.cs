namespace OstoraWeaponSkins;

public sealed class PluginConfig
{
	public string DatabaseConnection { get; set; } = "OstoraWeaponskins";
	public string SkinsLanguage { get; set; } = "en";
	public int CmdRefreshCooldownSeconds { get; set; } = 3;
	public bool KnifeEnabled { get; set; } = true;
	public bool SkinEnabled { get; set; } = true;
	public bool GloveEnabled { get; set; } = true;
	public bool AgentEnabled { get; set; } = true;
	public bool MusicEnabled { get; set; } = true;
	public bool DebugLogging { get; set; } = false;
}
