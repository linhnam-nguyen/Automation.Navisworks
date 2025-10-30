using NavGen.Core.Configuration;
using NavGen.Core.Services;
using Xunit;

namespace NavGen.Tests;

public class SearchSetCsvParserTests
{
    private static string SamplesDirectory => TestDirectories.Samples;

    [Fact]
    public void Parse_ShouldReadAllSearchSets()
    {
        var configuration = new AppConfiguration();
        var parser = new SearchSetCsvParser(configuration);
        var path = Path.Combine(SamplesDirectory, "search_sets.csv");

        var result = parser.Parse(path);

        var errors = string.Join(Environment.NewLine, result.Errors.Select(error => error.ToString()));
        Assert.False(result.HasErrors, errors);
        Assert.Equal(9, result.Items.Count);
        Assert.Contains(result.Items, set => set.Name == "Arc - WallsFloorsPlumb" && set.Criteria.Count == 3);
        Assert.Contains(result.Items, set => set.Name == "Str - Foundations" && set.Criteria.Count == 1);
    }
}
