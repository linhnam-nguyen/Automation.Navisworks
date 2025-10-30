using System;
using System.Linq;
using System.Xml.Linq;
using NavGen.Core.Configuration;
using NavGen.Core.Models;
using NavGen.Core.Utilities;

namespace NavGen.Core.Xml;

public sealed class SearchSetXmlBuilder
{
    private readonly AppConfiguration _configuration;
    private readonly DeterministicGuidFactory _folderGuidFactory = new("search-folder");

    public SearchSetXmlBuilder(AppConfiguration configuration)
    {
        _configuration = configuration;
    }

    public XDocument Build(IEnumerable<SearchSetModel> sets)
    {
        var xsi = XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance");
        var document = new XDocument(new XDeclaration("1.0", "UTF-8", null));

        var exchange = new XElement("exchange",
            new XAttribute(XNamespace.Xmlns + "xsi", xsi),
            new XAttribute(xsi + "noNamespaceSchemaLocation", "http://download.autodesk.com/us/navisworks/schemas/nw-exchange-12.0.xsd"),
            new XAttribute("units", _configuration.Defaults.Units),
            new XAttribute("filename", string.Empty),
            new XAttribute("filepath", string.Empty));

        var selectionSetsElement = new XElement("selectionsets");
        exchange.Add(selectionSetsElement);

        var folderGroups = sets
            .GroupBy(s => s.Folder ?? string.Empty)
            .Select(group => new
            {
                Folder = group.Key,
                Order = group.Min(s => s.Order),
                Items = group.OrderBy(s => s.Order)
            })
            .OrderBy(g => g.Order)
            .ThenBy(g => g.Folder, StringComparer.OrdinalIgnoreCase);

        foreach (var folderGroup in folderGroups)
        {
            var folderName = string.IsNullOrWhiteSpace(folderGroup.Folder) ? "Root" : folderGroup.Folder;
            var folderElement = new XElement("viewfolder",
                new XAttribute("name", folderName),
                new XAttribute("guid", _folderGuidFactory.Create(folderName)));

            foreach (var set in folderGroup.Items)
            {
                folderElement.Add(CreateSelectionSet(set));
            }

            selectionSetsElement.Add(folderElement);
        }

        document.Add(exchange);
        return document;
    }

    private XElement CreateSelectionSet(SearchSetModel set)
    {
        var findspec = new XElement("findspec",
            new XAttribute("mode", set.MatchLogic == MatchLogic.All ? "all" : "any"),
            new XAttribute("disjoint", _configuration.Defaults.Search.Disjoint));

        var conditions = new XElement("conditions");
        for (var index = 0; index < set.Criteria.Count; index++)
        {
            var criterion = set.Criteria[index];
            conditions.Add(CreateCondition(index, criterion));
        }

        findspec.Add(conditions);

        var selectionSetElement = new XElement("selectionset",
            new XAttribute("name", set.Name),
            new XAttribute("guid", set.Id));

        if (!string.IsNullOrWhiteSpace(set.Description))
        {
            selectionSetElement.Add(new XElement("comment", set.Description));
        }

        selectionSetElement.Add(findspec);
        return selectionSetElement;
    }

    private XElement CreateCondition(int index, SearchCriterionModel criterion)
    {
        var (categoryInternal, categoryDisplay) = ResolveQualifiedName(criterion.Category, _configuration.Defaults.Search.CategoryInternal, _configuration.Defaults.Search.CategoryDisplay);
        var (propertyInternal, propertyDisplay) = ResolveQualifiedName(criterion.PropertyName, _configuration.Defaults.Search.PropertyInternal, _configuration.Defaults.Search.PropertyDisplay);
        var (valueType, valueContent) = ResolveValue(criterion.Value);

        return new XElement("condition",
            new XAttribute("test", criterion.Operator.ToLowerInvariant()),
            new XAttribute("flags", index == 0 ? 0 : 64),
            new XElement("category",
                new XElement("name",
                    new XAttribute("internal", categoryInternal),
                    categoryDisplay)),
            new XElement("property",
                new XElement("name",
                    new XAttribute("internal", propertyInternal),
                    propertyDisplay)),
            new XElement("value",
                new XElement("data",
                    new XAttribute("type", valueType),
                    valueContent)));
    }

    private static (string internalName, string display) ResolveQualifiedName(string value, string defaultInternal, string defaultDisplay)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return (defaultInternal, defaultDisplay);
        }

        var parts = value.Split('|', 2);
        return parts.Length == 2
            ? (parts[0].Trim(), parts[1].Trim())
            : (defaultInternal, value.Trim());
    }

    private static (string type, string value) ResolveValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return ("wstring", string.Empty);
        }

        var parts = value.Split(':', 2);
        return parts.Length == 2
            ? (parts[0].Trim().ToLowerInvariant(), parts[1].Trim())
            : ("wstring", value.Trim());
    }

    private sealed class DeterministicGuidFactory
    {
        private readonly string _namespaceName;

        public DeterministicGuidFactory(string namespaceName)
        {
            _namespaceName = namespaceName;
        }

        public Guid Create(string value) => DeterministicGuid.Create(_namespaceName, value);
    }
}
