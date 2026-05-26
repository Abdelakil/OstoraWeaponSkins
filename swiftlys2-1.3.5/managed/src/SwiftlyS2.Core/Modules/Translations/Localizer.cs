using SwiftlyS2.Shared.Translation;

namespace SwiftlyS2.Core.Translations;

internal class Localizer : ILocalizer
{
    private Dictionary<string, string> _Resource { get; init; }
    private Dictionary<string, string> _DefaultResource { get; init; }
    private Language _Language { get; init; }

    public Localizer( Language language, Dictionary<string, string> resource, Dictionary<string, string> defaultResource )
    {
        _Resource = resource;
        _DefaultResource = defaultResource;
        _Language = language;
    }

    public string this[string key] => Get(key);

    public string this[string key, params object[] args] => string.Format(this[key], args);

    public string Get( string key )
    {
        return _Resource.TryGetValue(key, out var value)
        ? value
        : _DefaultResource.TryGetValue(key, out var defaultValue)
        ? defaultValue
        : $"{_Language.Value}.{key}";
    }
}