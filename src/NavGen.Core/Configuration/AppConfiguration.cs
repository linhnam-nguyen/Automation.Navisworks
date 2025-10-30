using System.Text.Json.Serialization;

namespace NavGen.Core.Configuration;

public sealed class AppConfiguration
{
    [JsonPropertyName("output")]
    public OutputConfiguration Output { get; init; } = new();

    [JsonPropertyName("defaults")]
    public DefaultsConfiguration Defaults { get; init; } = new();

    [JsonPropertyName("colors")]
    public ColorConfiguration Colors { get; init; } = new();
}

public sealed class OutputConfiguration
{
    [JsonPropertyName("folder")]
    public string Folder { get; init; } = "out";
}

public sealed class DefaultsConfiguration
{
    [JsonPropertyName("units")]
    public string Units { get; init; } = "ft";

    [JsonPropertyName("search")]
    public SearchDefaults Search { get; init; } = new();

    [JsonPropertyName("clash")]
    public ClashDefaults Clash { get; init; } = new();
}

public sealed class SearchDefaults
{
    [JsonPropertyName("categoryInternal")]
    public string CategoryInternal { get; init; } = "LcRevitData_Element";

    [JsonPropertyName("categoryDisplay")]
    public string CategoryDisplay { get; init; } = "Element";

    [JsonPropertyName("propertyInternal")]
    public string PropertyInternal { get; init; } = "LcRevitPropertyElementCategory";

    [JsonPropertyName("propertyDisplay")]
    public string PropertyDisplay { get; init; } = "Category";

    [JsonPropertyName("disjoint")]
    public int Disjoint { get; init; } = 0;
}

public sealed class ClashDefaults
{
    [JsonPropertyName("status")]
    public string Status { get; init; } = "ok";

    [JsonPropertyName("mergeComposites")]
    public int MergeComposites { get; init; } = 1;

    [JsonPropertyName("selfIntersect")]
    public int SelfIntersect { get; init; } = 0;

    [JsonPropertyName("primitiveTypes")]
    public int PrimitiveTypes { get; init; } = 1;

    [JsonPropertyName("linkageMode")]
    public string LinkageMode { get; init; } = "none";

    [JsonPropertyName("active")]
    public bool Active { get; init; } = true;

    [JsonPropertyName("grouping")]
    public string Grouping { get; init; } = string.Empty;

    [JsonPropertyName("severity")]
    public string Severity { get; init; } = string.Empty;
}

public sealed class ColorConfiguration
{
    [JsonPropertyName("fallback")]
    public string Fallback { get; init; } = "#FF9A00";
}
