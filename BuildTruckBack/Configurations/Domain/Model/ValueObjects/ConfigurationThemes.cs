namespace BuildTruckBack.Configurations.Domain.Model.ValueObjects;

public static class ConfigurationThemes
{
    public static readonly string[] ValidThemes = ["light", "dark", "auto"];

    public static bool IsValid(string theme)
    {
        return ValidThemes.Contains(theme?.ToLowerInvariant());
    }
}