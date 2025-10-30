namespace NavGen.Core.Models;

public sealed class SearchSetModel
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Folder { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public string ColorHex { get; init; } = string.Empty;

    public MatchLogic MatchLogic { get; init; } = MatchLogic.All;

    public IReadOnlyList<SearchCriterionModel> Criteria { get; init; } = Array.Empty<SearchCriterionModel>();

    public int Order { get; init; }
}
