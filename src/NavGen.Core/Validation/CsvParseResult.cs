namespace NavGen.Core.Validation;

public sealed class CsvParseResult<T>
{
    public CsvParseResult(IReadOnlyList<T> items, IReadOnlyList<ValidationError> errors)
    {
        Items = items;
        Errors = errors;
    }

    public IReadOnlyList<T> Items { get; }

    public IReadOnlyList<ValidationError> Errors { get; }

    public bool HasErrors => Errors.Count > 0;
}
