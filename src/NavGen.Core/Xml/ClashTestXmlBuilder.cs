using System.Globalization;
using System.Xml.Linq;
using NavGen.Core.Configuration;
using NavGen.Core.Models;

namespace NavGen.Core.Xml;

public sealed class ClashTestXmlBuilder
{
    private readonly AppConfiguration _configuration;

    public ClashTestXmlBuilder(AppConfiguration configuration)
    {
        _configuration = configuration;
    }

    public XDocument Build(IEnumerable<ClashTestModel> tests, string batchName)
    {
        var xsi = XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance");
        var document = new XDocument(new XDeclaration("1.0", "UTF-8", null));
        var exchange = new XElement("exchange",
            new XAttribute(XNamespace.Xmlns + "xsi", xsi),
            new XAttribute(xsi + "noNamespaceSchemaLocation", "http://download.autodesk.com/us/navisworks/schemas/nw-exchange-12.0.xsd"),
            new XAttribute("units", _configuration.Defaults.Units),
            new XAttribute("filename", string.Empty),
            new XAttribute("filepath", string.Empty));

        var batchTest = new XElement("batchtest",
            new XAttribute("name", batchName),
            new XAttribute("internal_name", batchName),
            new XAttribute("units", _configuration.Defaults.Units));

        var testsElement = new XElement("clashtests");

        foreach (var test in tests)
        {
            testsElement.Add(CreateClashTest(test));
        }

        batchTest.Add(testsElement);
        exchange.Add(batchTest);
        document.Add(exchange);
        return document;
    }

    private XElement CreateClashTest(ClashTestModel test)
    {
        var toleranceFeet = test.ToleranceMillimeters / 304.8d;
        var tolerance = toleranceFeet.ToString("0.0000000000", CultureInfo.InvariantCulture);
        var clashtest = new XElement("clashtest",
            new XAttribute("name", test.Name),
            new XAttribute("test_type", test.Type.ToLowerInvariant()),
            new XAttribute("status", _configuration.Defaults.Clash.Status),
            new XAttribute("tolerance", tolerance),
            new XAttribute("merge_composites", _configuration.Defaults.Clash.MergeComposites));

        clashtest.Add(new XElement("linkage",
            new XAttribute("mode", _configuration.Defaults.Clash.LinkageMode)));

        clashtest.Add(CreateSelection("left", test.LeftSet));
        clashtest.Add(CreateSelection("right", test.RightSet));

        clashtest.Add(new XElement("rules"));

        if (!string.IsNullOrWhiteSpace(test.Description))
        {
            clashtest.Add(new XElement("description", test.Description));
        }

        if (!string.IsNullOrWhiteSpace(test.Grouping))
        {
            clashtest.SetAttributeValue("group", test.Grouping);
        }

        if (!test.Active)
        {
            clashtest.SetAttributeValue("disabled", 1);
        }

        if (!string.IsNullOrWhiteSpace(test.Severity))
        {
            clashtest.SetAttributeValue("priority", test.Severity);
        }

        return clashtest;
    }

    private XElement CreateSelection(string elementName, string setName)
    {
        return new XElement(elementName,
            new XElement("clashselection",
                new XAttribute("selfintersect", _configuration.Defaults.Clash.SelfIntersect),
                new XAttribute("primtypes", _configuration.Defaults.Clash.PrimitiveTypes),
                new XElement("locator", $"lcop_selection_set_tree/{setName}")));
    }
}
