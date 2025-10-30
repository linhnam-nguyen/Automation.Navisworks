using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using NavGen.Core.Csv;
using NavGen.Core.Models;
using NavGen.Core.Utilities;
using NavGen.Core.Validation;

namespace NavGen.Core.Services;

public sealed class ClashTestCsvParser
{
    private readonly DeterministicGuidFactory _guidFactory = new("clashtest");

    public CsvParseResult<ClashTestModel> Parse(string path)
    {
        var records = new List<ClashTestCsvRecord>();
        var errors = new List<ValidationError>();

        if (!File.Exists(path))
        {
            errors.Add(new ValidationError(0, path, "File not found"));
            return new CsvParseResult<ClashTestModel>(Array.Empty<ClashTestModel>(), errors);
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
            records.AddRange(csv.GetRecords<ClashTestCsvRecord>());
        }
        catch (HeaderValidationException ex)
        {
            errors.Add(new ValidationError(0, string.Join(",", ex.InvalidHeaders?.Select(h => h.Names.FirstOrDefault()) ?? Array.Empty<string>()), ex.Message));
            return new CsvParseResult<ClashTestModel>(Array.Empty<ClashTestModel>(), errors);
        }
        catch (Exception ex)
        {
            errors.Add(new ValidationError(0, string.Empty, ex.Message));
            return new CsvParseResult<ClashTestModel>(Array.Empty<ClashTestModel>(), errors);
        }

        var results = new List<ClashTestModel>();
        var lineNumber = 2;
        foreach (var record in records)
        {
            if (string.IsNullOrWhiteSpace(record.TestName))
            {
                errors.Add(new ValidationError(lineNumber, nameof(record.TestName), "TestName is required"));
            }

            if (string.IsNullOrWhiteSpace(record.LeftSet))
            {
                errors.Add(new ValidationError(lineNumber, nameof(record.LeftSet), "LeftSet is required"));
            }

            if (string.IsNullOrWhiteSpace(record.RightSet))
            {
                errors.Add(new ValidationError(lineNumber, nameof(record.RightSet), "RightSet is required"));
            }

            if (record.ToleranceMillimeters < 0)
            {
                errors.Add(new ValidationError(lineNumber, nameof(record.ToleranceMillimeters), "Tolerance must be ≥ 0"));
            }

            if (!string.Equals(record.Type, "Hard", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(record.Type, "Clearance", StringComparison.OrdinalIgnoreCase))
            {
                errors.Add(new ValidationError(lineNumber, nameof(record.Type), "Type must be 'Hard' or 'Clearance'"));
            }

            var model = new ClashTestModel
            {
                Id = _guidFactory.Create(record.TestName),
                Name = record.TestName,
                LeftSet = record.LeftSet,
                RightSet = record.RightSet,
                ToleranceMillimeters = record.ToleranceMillimeters,
                Type = record.Type,
                Active = ParseBoolean(record.Active, defaultValue: true),
                Severity = record.Severity ?? string.Empty,
                Grouping = record.Grouping ?? string.Empty,
                Description = record.Description ?? string.Empty
            };

            results.Add(model);
            lineNumber++;
        }

        return new CsvParseResult<ClashTestModel>(results, errors);
    }

    private static bool ParseBoolean(string? value, bool defaultValue)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        if (bool.TryParse(value, out var parsed))
        {
            return parsed;
        }

        return defaultValue;
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
