using System.IO;
using NavGen.Core.Services;
using Xunit;

namespace NavGen.Tests;

public class ClashTestCsvParserTests
{
    private static string SamplesDirectory => TestDirectories.Samples;

    [Fact]
    public void Parse_ShouldReadAllClashTests()
    {
        var parser = new ClashTestCsvParser();
        var path = Path.Combine(SamplesDirectory, "clash_sets.csv");

        var result = parser.Parse(path);

        var errors = string.Join(Environment.NewLine, result.Errors.Select(error => error.ToString()));
        Assert.False(result.HasErrors, errors);
        Assert.Equal(10, result.Items.Count);
        Assert.Contains(result.Items, test => test.Name == "MEP vs MEP" && test.LeftSet == "MEP");
    }

    [Fact]
    public void Parse_ShouldValidateType()
    {
        var parser = new ClashTestCsvParser();
        var csv = """
        TestName,LeftSet,RightSet,ToleranceMM,Type
        Name,Left,Right,-1,Soft
        """;
        var path = Path.GetTempFileName();
        File.WriteAllText(path, csv);

        try
        {
            var result = parser.Parse(path);
            Assert.True(result.HasErrors);
            Assert.Contains(result.Errors, error => error.ColumnName.Contains("Tolerance"));
            Assert.Contains(result.Errors, error => error.ColumnName.Contains("Type"));
        }
        finally
        {
            File.Delete(path);
        }
    }
}
