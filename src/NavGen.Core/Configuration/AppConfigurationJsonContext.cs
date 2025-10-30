using System.Text.Json;
using System.Text.Json.Serialization;

namespace NavGen.Core.Configuration;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    ReadCommentHandling = JsonCommentHandling.Skip,
    WriteIndented = true)]
[JsonSerializable(typeof(AppConfiguration))]
internal partial class AppConfigurationJsonContext : JsonSerializerContext;
