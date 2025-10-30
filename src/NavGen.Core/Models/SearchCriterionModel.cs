namespace NavGen.Core.Models;

public sealed record SearchCriterionModel(
    string Category,
    string PropertyName,
    string Operator,
    string Value,
    int FlagsIndex);
