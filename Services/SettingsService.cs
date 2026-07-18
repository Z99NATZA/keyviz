using System.IO;
using System.Text.Json;

namespace KeyViz.Services;

internal sealed record AppSettings
{
    internal const int DefaultMaxHistoryLength = 20;
    internal const string DefaultBubblePosition = "center";

    public int MaxHistoryLength { get; init; } = DefaultMaxHistoryLength;

    public bool ShowControls { get; init; } = true;

    public string BubblePosition { get; init; } = DefaultBubblePosition;

    public AppSettings()
    {
    }

    internal AppSettings Normalize()
    {
        return this with
        {
            MaxHistoryLength = Math.Max(1, MaxHistoryLength),
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

    internal static void Save(AppSettings settings)
    {
        try
        {
            var directory = Path.GetDirectoryName(SettingsPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(
                SettingsPath,
                JsonSerializer.Serialize(settings.Normalize(), JsonOptions));
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            // Keep the live setting even when it cannot be persisted.
        }
    }
}
