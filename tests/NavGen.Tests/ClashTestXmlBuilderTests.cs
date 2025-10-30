using System.IO;
using System.Xml.Linq;
using NavGen.Core.Configuration;
using NavGen.Core.Services;
using NavGen.Core.Xml;
using Xunit;

namespace NavGen.Tests;

public class ClashTestXmlBuilderTests
{
    private static string RootDirectory => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
    private static string SamplesDirectory => Path.Combine(RootDirectory, "samples");
    private static string XmlSamplesDirectory => Path.Combine(RootDirectory, "SamplesXML");

    [Fact]
    public void Build_ShouldMatchSampleStructure()
    {
        var configuration = ConfigurationLoader.Load(Path.Combine(RootDirectory, "src", "NavGen.Cli", "appsettings.json"));
        var parser = new ClashTestCsvParser();
        var builder = new ClashTestXmlBuilder(configuration);
        var csvPath = Path.Combine(SamplesDirectory, "clash_sets.csv");
        var expectedPath = Path.Combine(XmlSamplesDirectory, "ClashSets.xml");

        var parseResult = parser.Parse(csvPath);
        Assert.False(parseResult.HasErrors);

        var document = builder.Build(parseResult.Items, "ClashSets");
        var expected = XDocument.Load(expectedPath);

        Normalize(document.Root!);
        Normalize(expected.Root!);

        Assert.True(XNode.DeepEquals(expected.Root, document.Root));
    }

    private static void Normalize(XElement root)
    {
        foreach (var attribute in root.DescendantsAndSelf().Attributes())
        {
            if (attribute.Name == "priority" || attribute.Name == "group" || attribute.Name == "disabled")
            {
                attribute.Remove();
            }
        }
    }
}
