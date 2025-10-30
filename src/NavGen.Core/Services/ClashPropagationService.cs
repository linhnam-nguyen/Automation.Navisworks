using System;
using System.Linq;
using NavGen.Core.Configuration;
using NavGen.Core.Models;
using NavGen.Core.Utilities;

namespace NavGen.Core.Services;

public sealed class ClashPropagationService
{
    private readonly DeterministicGuidFactory _guidFactory = new("propagated-clash");

    public ClashPropagationService(AppConfiguration configuration)
    {
        _ = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public IReadOnlyList<ClashTestModel> Propagate(IEnumerable<SearchSetModel> searchSets, PropagationOptions options)
    {
        var sets = searchSets.OrderBy(s => s.Folder).ThenBy(s => s.Name, StringComparer.OrdinalIgnoreCase).ToList();
        var results = new List<ClashTestModel>();

        for (var i = 0; i < sets.Count; i++)
        {
            for (var j = i; j < sets.Count; j++)
            {
                if (i == j && !options.IncludeSelfComparisons)
                {
                    continue;
                }

                var left = sets[i];
                var right = sets[j];

                if (!options.IncludeSelfComparisons && string.Equals(left.Name, right.Name, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!options.IncludeSameFolder && string.Equals(left.Folder, right.Folder, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(options.ExcludePrefixDelimiter))
                {
                    var leftPrefix = ExtractPrefix(left.Name, options.ExcludePrefixDelimiter);
                    var rightPrefix = ExtractPrefix(right.Name, options.ExcludePrefixDelimiter);
                    if (!string.IsNullOrEmpty(leftPrefix) && string.Equals(leftPrefix, rightPrefix, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                }

                var name = $"{left.Name} vs {right.Name}";
                var id = _guidFactory.Create(name);
                results.Add(new ClashTestModel
                {
                    Id = id,
                    Name = name,
                    LeftSet = left.Name,
                    RightSet = right.Name,
                    ToleranceMillimeters = options.DefaultToleranceMillimeters,
                    Type = options.TestType,
                    Active = true
                });
            }
        }

        return results;
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

    private static string ExtractPrefix(string value, string delimiter)
    {
        var index = value.IndexOf(delimiter, StringComparison.OrdinalIgnoreCase);
        return index <= 0 ? string.Empty : value[..index];
    }
}

public sealed class PropagationOptions
{
    public bool IncludeSelfComparisons { get; init; }

    public bool IncludeSameFolder { get; init; }

    public string ExcludePrefixDelimiter { get; init; } = string.Empty;

    public double DefaultToleranceMillimeters { get; init; }

    public string TestType { get; init; } = "Hard";
}
