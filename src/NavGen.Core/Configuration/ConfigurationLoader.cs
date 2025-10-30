using System.Text.Json;

namespace NavGen.Core.Configuration;

public static class ConfigurationLoader
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerOptions.Default)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReadCommentHandling = JsonCommentHandling.Skip,
        WriteIndented = true
    };

    public static AppConfiguration Load(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return new AppConfiguration();
        }

        using var stream = File.OpenRead(path);
        var configuration = JsonSerializer.Deserialize<AppConfiguration>(stream, Options);
        return configuration ?? new AppConfiguration();
    }

    public static void Save(string path, AppConfiguration configuration)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(configuration, Options);
        File.WriteAllText(path, json);
    }
}
