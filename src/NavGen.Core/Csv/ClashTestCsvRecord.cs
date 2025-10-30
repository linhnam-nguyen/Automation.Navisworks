using CsvHelper.Configuration.Attributes;

namespace NavGen.Core.Csv;

public sealed class ClashTestCsvRecord
{
    [Name("TestName")]
    public string TestName { get; set; } = string.Empty;

    [Name("LeftSet")]
    public string LeftSet { get; set; } = string.Empty;

    [Name("RightSet")]
    public string RightSet { get; set; } = string.Empty;

    [Name("ToleranceMM")]
    public double ToleranceMillimeters { get; set; }

    [Name("Type")]
    public string Type { get; set; } = string.Empty;

    [Optional]
    [Name("Active")]
    public string? Active { get; set; }

    [Optional]
    [Name("Severity")]
    public string? Severity { get; set; }

    [Optional]
    [Name("Grouping")]
    public string? Grouping { get; set; }

    [Optional]
    [Name("Description")]
    public string? Description { get; set; }
}
