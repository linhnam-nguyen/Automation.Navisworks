using System.Globalization;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using NavGen.Core.Configuration;
using NavGen.Core.Csv;
using NavGen.Core.Models;
using NavGen.Core.Utilities;
using NavGen.Core.Validation;

namespace NavGen.Core.Services;

public sealed class SearchSetCsvParser
{
    private readonly AppConfiguration _configuration;
    private readonly DeterministicGuidFactory _guidFactory;

    public SearchSetCsvParser(AppConfiguration configuration)
    {
        _configuration = configuration;
        _guidFactory = new DeterministicGuidFactory("searchset");
    }

    public CsvParseResult<SearchSetModel> Parse(string path)
    {
        var records = new List<SearchSetCsvRecord>();
        var errors = new List<ValidationError>();

        if (!File.Exists(path))
        {
            errors.Add(new ValidationError(0, path, "File not found"));
            return new CsvParseResult<SearchSetModel>(Array.Empty<SearchSetModel>(), errors);
        }

        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            PrepareHeaderForMatch = args => args.Header?.Trim() ?? string.Empty,
            MissingFieldFound = null,
            TrimOptions = TrimOptions.Trim
        });

        try
        {
            records.AddRange(csv.GetRecords<SearchSetCsvRecord>());
        }
        catch (HeaderValidationException ex)
        {
            errors.Add(new ValidationError(0, string.Join(",", ex.InvalidHeaders?.Select(h => h.Names.FirstOrDefault()) ?? Array.Empty<string>()), ex.Message));
            return new CsvParseResult<SearchSetModel>(Array.Empty<SearchSetModel>(), errors);
        }
        catch (Exception ex)
        {
            errors.Add(new ValidationError(0, string.Empty, ex.Message));
            return new CsvParseResult<SearchSetModel>(Array.Empty<SearchSetModel>(), errors);
        }

        var grouped = records
            .Select((record, index) => (record, index: index + 2))
            .GroupBy(tuple => tuple.record.SetName);

        var results = new List<SearchSetModel>();

        foreach (var group in grouped)
        {
            var first = group.First();
            if (string.IsNullOrWhiteSpace(first.record.SetName))
            {
                errors.Add(new ValidationError(first.index, nameof(first.record.SetName), "SetName is required"));
                continue;
            }

            if (!Enum.TryParse<MatchLogic>(first.record.MatchLogic, true, out var matchLogic))
            {
                errors.Add(new ValidationError(first.index, nameof(first.record.MatchLogic), "MatchLogic must be 'All' or 'Any'"));
                continue;
            }

            var criteria = new List<SearchCriterionModel>();
            foreach (var item in group)
            {
                if (string.IsNullOrWhiteSpace(item.record.Category))
                {
                    errors.Add(new ValidationError(item.index, nameof(item.record.Category), "Category is required"));
                    continue;
                }

                if (string.IsNullOrWhiteSpace(item.record.PropertyName))
                {
                    errors.Add(new ValidationError(item.index, nameof(item.record.PropertyName), "PropertyName is required"));
                    continue;
                }

                if (string.IsNullOrWhiteSpace(item.record.Operator))
                {
                    errors.Add(new ValidationError(item.index, nameof(item.record.Operator), "Operator is required"));
                    continue;
                }

                criteria.Add(new SearchCriterionModel(
                    item.record.Category,
                    item.record.PropertyName,
                    item.record.Operator,
                    item.record.Value,
                    criteria.Count));
            }

            if (criteria.Count == 0)
            {
                continue;
            }

            var id = _guidFactory.Create(first.record.SetName);
            results.Add(new SearchSetModel
            {
                Id = id,
                Name = first.record.SetName,
                Folder = first.record.Folder ?? string.Empty,
                Description = first.record.Description ?? string.Empty,
                ColorHex = string.IsNullOrWhiteSpace(first.record.ColorHex)
                    ? _configuration.Colors.Fallback
                    : NormalizeColor(first.record.ColorHex),
                MatchLogic = matchLogic,
                Criteria = criteria,
                Order = first.index
            });
        }

        return new CsvParseResult<SearchSetModel>(results, errors);
    }

    private static string NormalizeColor(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return value.StartsWith('#') ? value.ToUpperInvariant() : $"#{value.ToUpperInvariant()}";
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
