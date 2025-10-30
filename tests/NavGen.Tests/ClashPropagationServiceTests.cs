using System;
using System.Collections.Generic;
using NavGen.Core.Configuration;
using NavGen.Core.Models;
using NavGen.Core.Services;
using Xunit;

namespace NavGen.Tests;

public class ClashPropagationServiceTests
{
    [Fact]
    public void Propagate_ShouldCreateSymmetricPairs()
    {
        var configuration = new AppConfiguration();
        var service = new ClashPropagationService(configuration);
        var sets = new List<SearchSetModel>
        {
            new() { Name = "A", Folder = "F1", Criteria = Array.Empty<SearchCriterionModel>(), Order = 1, Id = Guid.NewGuid() },
            new() { Name = "B", Folder = "F2", Criteria = Array.Empty<SearchCriterionModel>(), Order = 2, Id = Guid.NewGuid() },
            new() { Name = "C", Folder = "F2", Criteria = Array.Empty<SearchCriterionModel>(), Order = 3, Id = Guid.NewGuid() }
        };

        var options = new PropagationOptions
        {
            IncludeSelfComparisons = false,
            IncludeSameFolder = false,
            DefaultToleranceMillimeters = 5,
            TestType = "Hard"
        };

        var results = service.Propagate(sets, options);

        Assert.Equal(3, results.Count);
        Assert.Contains(results, test => test.Name == "A vs B");
        Assert.Contains(results, test => test.Name == "A vs C");
        Assert.Contains(results, test => test.Name == "B vs C");
    }
}
