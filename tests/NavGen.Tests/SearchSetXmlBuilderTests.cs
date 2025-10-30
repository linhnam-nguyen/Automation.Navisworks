using System.IO;
using System.Xml.Linq;
using NavGen.Core.Configuration;
using NavGen.Core.Services;
using NavGen.Core.Xml;
using Xunit;

namespace NavGen.Tests;

public class SearchSetXmlBuilderTests
{
    private static string RootDirectory => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
    private static string SamplesDirectory => Path.Combine(RootDirectory, "samples");
    private static string XmlSamplesDirectory => Path.Combine(RootDirectory, "SamplesXML");

    [Fact]
    public void Build_ShouldMatchSampleStructure()
    {
        var configuration = ConfigurationLoader.Load(Path.Combine(RootDirectory, "src", "NavGen.Cli", "appsettings.json"));
        var parser = new SearchSetCsvParser(configuration);
        var builder = new SearchSetXmlBuilder(configuration);
        var csvPath = Path.Combine(SamplesDirectory, "search_sets.csv");
        var expectedPath = Path.Combine(XmlSamplesDirectory, "SearchSets.xml");

        var parseResult = parser.Parse(csvPath);
        Assert.False(parseResult.HasErrors);

        var document = builder.Build(parseResult.Items);
        var expected = XDocument.Load(expectedPath);

        NormalizeForComparison(document.Root!);
        NormalizeForComparison(expected.Root!);

        Assert.True(XNode.DeepEquals(expected.Root, document.Root));
    }

    private static void NormalizeForComparison(XElement root)
    {
        foreach (var guidAttribute in root.DescendantsAndSelf().Attributes("guid"))
        {
            guidAttribute.Value = string.Empty;
        }
    }
}
