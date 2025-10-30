namespace NavGen.Core.Validation;

public sealed record ValidationError(int LineNumber, string ColumnName, string Message)
{
    public override string ToString() => $"Line {LineNumber}: [{ColumnName}] {Message}";
}
