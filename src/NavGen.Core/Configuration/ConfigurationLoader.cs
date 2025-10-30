using System.Text.Json;

namespace NavGen.Core.Configuration;

public static class ConfigurationLoader
{
    private static readonly AppConfigurationJsonContext Context = AppConfigurationJsonContext.Default;

    public static AppConfiguration Load(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return new AppConfiguration();
        }

        using var stream = File.OpenRead(path);
        var configuration = JsonSerializer.Deserialize(stream, Context.AppConfiguration);
        return configuration ?? new AppConfiguration();
    }

    public static void Save(string path, AppConfiguration configuration)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(configuration, Context.AppConfiguration);
        File.WriteAllText(path, json);
    }
}
