namespace NavGen.Core.Models;

public sealed class ClashTestModel
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string LeftSet { get; init; } = string.Empty;

    public string RightSet { get; init; } = string.Empty;

    public double ToleranceMillimeters { get; init; }

    public string Type { get; init; } = "Hard";

    public bool Active { get; init; } = true;

    public string Severity { get; init; } = string.Empty;

    public string Grouping { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;
}
