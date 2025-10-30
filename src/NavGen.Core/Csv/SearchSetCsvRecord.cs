using CsvHelper.Configuration.Attributes;

namespace NavGen.Core.Csv;

public sealed class SearchSetCsvRecord
{
    [Name("SetName")]
    public string SetName { get; set; } = string.Empty;

    [Name("Category")]
    public string Category { get; set; } = string.Empty;

    [Name("PropertyName")]
    public string PropertyName { get; set; } = string.Empty;

    [Name("Operator")]
    public string Operator { get; set; } = string.Empty;

    [Name("Value")]
    public string Value { get; set; } = string.Empty;

    [Name("MatchLogic")]
    public string MatchLogic { get; set; } = string.Empty;

    [Optional]
    [Name("ColorHex")]
    public string? ColorHex { get; set; }

    [Optional]
    [Name("Description")]
    public string? Description { get; set; }

    [Optional]
    [Name("Folder")]
    public string? Folder { get; set; }
}
