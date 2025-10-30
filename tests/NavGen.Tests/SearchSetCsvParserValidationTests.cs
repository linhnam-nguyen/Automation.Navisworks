using System.IO;
using NavGen.Core.Configuration;
using NavGen.Core.Services;
using Xunit;

namespace NavGen.Tests;

public class SearchSetCsvParserValidationTests
{
    [Fact]
    public void Parse_ShouldReturnErrorsForInvalidRecords()
    {
        var configuration = new AppConfiguration();
        var parser = new SearchSetCsvParser(configuration);
        var csv = "SetName,Category,PropertyName,Operator,Value,MatchLogic
,,Property,Equals,Value,All
Name,Category,PropertyName,Equals,Value,Invalid";
        var path = Path.GetTempFileName();
        File.WriteAllText(path, csv);

        try
        {
            var result = parser.Parse(path);
            Assert.True(result.HasErrors);
            Assert.Contains(result.Errors, error => error.ColumnName.Contains("SetName"));
            Assert.Contains(result.Errors, error => error.ColumnName.Contains("MatchLogic"));
        }
        finally
        {
            File.Delete(path);
        }
    }
}
