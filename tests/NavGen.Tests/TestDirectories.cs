using System;
using System.IO;

namespace NavGen.Tests;

internal static class TestDirectories
{
    public static string SolutionRoot { get; } = LocateSolutionRoot();

    public static string Samples => Path.Combine(SolutionRoot, "samples");

    public static string XmlSamples => Path.Combine(SolutionRoot, "SamplesXML");

    private static string LocateSolutionRoot()
    {
        var directory = AppContext.BaseDirectory;
        while (!string.IsNullOrEmpty(directory))
        {
            if (File.Exists(Path.Combine(directory, "NavGen.sln")))
            {
                return directory;
            }

            var parent = Directory.GetParent(directory);
            if (parent is null || string.Equals(parent.FullName, directory, StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            directory = parent.FullName;
        }

        throw new InvalidOperationException("Unable to locate solution root for tests.");
    }
}
