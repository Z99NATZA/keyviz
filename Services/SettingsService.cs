using System.IO;
using System.Text.Json;

namespace KeyViz.Services;

internal sealed record AppSettings
{
    internal const int DefaultMaxTextLength = 20;
    internal const int DefaultMaxSpecialKeys = 5;
    internal const string DefaultBubblePosition = "center";

    public int MaxTextLength { get; init; } = DefaultMaxTextLength;

    public int MaxSpecialKeys { get; init; } = DefaultMaxSpecialKeys;

    public bool ShowControls { get; init; } = true;

    public string BubblePosition { get; init; } = DefaultBubblePosition;

    public AppSettings()
    {
    }

    internal AppSettings Normalize()
    {
        return this with
        {
            MaxTextLength = Math.Clamp(MaxTextLength, 0, 200),
            MaxSpecialKeys = Math.Clamp(MaxSpecialKeys, 0, 20),
            BubblePosition = BubblePosition?.Trim().ToLowerInvariant() switch
            {
                "left" => "left",
                "right" => "right",
                _ => DefaultBubblePosition
            }
        };
    }
}

internal static class SettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReadCommentHandling = JsonCommentHandling.Skip,
        WriteIndented = true
    };

    internal static string SettingsPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "KeyViz",
        "settings.json");

    internal static AppSettings Load()
    {
        var defaults = new AppSettings();

        try
        {
            var directory = Path.GetDirectoryName(SettingsPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (!File.Exists(SettingsPath))
            {
                File.WriteAllText(SettingsPath, JsonSerializer.Serialize(defaults, JsonOptions));
                return defaults;
            }

            var json = File.ReadAllText(SettingsPath);
            return (JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? defaults).Normalize();
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException)
        {
            return defaults;
        }
    }
}
