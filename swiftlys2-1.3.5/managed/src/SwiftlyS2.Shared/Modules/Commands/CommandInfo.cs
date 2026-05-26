namespace SwiftlyS2.Shared.Commands;

/// <summary>
/// Contains information about a registered command.
/// </summary>
public record CommandInfo
{
    /// <summary>
    /// The name of the command.
    /// </summary>
    public required string CommandName { get; init; }

    /// <summary>
    /// Whether the command is registered without the 'sw_' prefix.
    /// </summary>
    public required bool RegisterRaw { get; init; }

    /// <summary>
    /// The permission required to use this command.
    /// </summary>
    public required string Permission { get; init; }

    /// <summary>
    /// The help text for this command.
    /// </summary>
    public required string HelpText { get; init; }
}
