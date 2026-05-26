namespace SwiftlyS2.Shared.Commands;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class Command : Attribute
{
    public string Name { get; set; }

    public bool RegisterRaw { get; set; } = false;

    public string Permission { get; set; } = string.Empty;

    public string HelpText { get; set; } = "SwiftlyS2 registered command";

    public Command( string name, bool registerRaw, string permission ) : this(name, registerRaw, permission, "SwiftlyS2 registered command")
    {
    }

    public Command( string name, bool registerRaw = false, string permission = "", string helpText = "SwiftlyS2 registered command" )
    {
        Name = name;
        RegisterRaw = registerRaw;
        Permission = permission;
        HelpText = helpText;
    }
}