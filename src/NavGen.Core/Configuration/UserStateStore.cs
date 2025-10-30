using System.Text.Json;

namespace NavGen.Core.Configuration;

public sealed class UserState
{
    public string? LastSearchSetPath { get; set; }
    public string? LastClashSetPath { get; set; }
}

public static class UserStateStore
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerOptions.Default)
    {
        WriteIndented = true
    };

    public static UserState Load(string path)
    {
        if (!File.Exists(path))
        {
            return new UserState();
        }

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<UserState>(json, Options) ?? new UserState();
    }

    public static void Save(string path, UserState state)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(state, Options);
        File.WriteAllText(path, json);
    }
}
