using NavGen.Core.Configuration;
using NavGen.Core.Services;
using Xunit;

namespace NavGen.Tests;

public class SearchSetCsvParserTests
{
    private static string SamplesDirectory => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "samples"));

    [Fact]
    public void Parse_ShouldReadAllSearchSets()
    {
        var configuration = new AppConfiguration();
        var parser = new SearchSetCsvParser(configuration);
        var path = Path.Combine(SamplesDirectory, "search_sets.csv");

        var result = parser.Parse(path);

        Assert.False(result.HasErrors);
        Assert.Equal(9, result.Items.Count);
        Assert.Contains(result.Items, set => set.Name == "Arc - WallsFloorsPlumb" && set.Criteria.Count == 3);
        Assert.Contains(result.Items, set => set.Name == "Str - Foundations" && set.Criteria.Count == 1);
    }
}
