using System.CommandLine;
using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using NavGen.Core.Configuration;
using NavGen.Core.Models;
using NavGen.Core.Services;
using NavGen.Core.Validation;
using NavGen.Core.Xml;

var baseDirectory = AppContext.BaseDirectory;
var configurationPath = Path.Combine(baseDirectory, "appsettings.json");
var configuration = ConfigurationLoader.Load(configurationPath);
var statePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData,
    Environment.SpecialFolderOption.Create), "NavGen", "state.json");
var state = UserStateStore.Load(statePath);
var searchParser = new SearchSetCsvParser(configuration);
var clashParser = new ClashTestCsvParser();
var searchBuilder = new SearchSetXmlBuilder(configuration);
var clashBuilder = new ClashTestXmlBuilder(configuration);
var propagationService = new ClashPropagationService(configuration);

var searchOption = new Option<FileInfo?>("--search", "Generate search set XML from CSV");
var clashOption = new Option<FileInfo?>("--clash", "Generate clash tests XML from CSV");
var menuOption = new Option<bool>("--menu", () => true, "Display interactive menu");

var rootCommand = new RootCommand("Navisworks XML generator")
{
    searchOption,
    clashOption,
    menuOption
};

rootCommand.SetHandler((FileInfo? search, FileInfo? clash, bool menu) =>
{
    if (search != null)
    {
        GenerateSearchSets(search.FullName);
    }

    if (clash != null)
    {
        GenerateClashTests(clash.FullName);
    }

    if (menu || (search is null && clash is null))
    {
        RunMenu();
    }
}, searchOption, clashOption, menuOption);

return await rootCommand.InvokeAsync(args);

void RunMenu()
{
    while (true)
    {
        Console.Clear();
        WriteBanner();
        Console.WriteLine("1. Generate Search Set XML from search_sets.csv");
        Console.WriteLine("2. Generate Clash Set XML from clash_sets.csv");
        Console.WriteLine("3. Propagate clashes across all Search Sets");
        Console.WriteLine("4. Validate CSV files");
        Console.WriteLine("5. Show configuration (output folder, defaults)");
        Console.WriteLine("0. Exit");
        Console.Write("Select an option: ");
        var input = Console.ReadLine();

        switch (input)
        {
            case "1":
                var searchPath = PromptForPath("Search set CSV path", state.LastSearchSetPath);
                if (!string.IsNullOrEmpty(searchPath))
                {
                    GenerateSearchSets(searchPath);
                    state.LastSearchSetPath = searchPath;
                    PersistState();
                }
                Pause();
                break;
            case "2":
                var clashPath = PromptForPath("Clash set CSV path", state.LastClashSetPath);
                if (!string.IsNullOrEmpty(clashPath))
                {
                    GenerateClashTests(clashPath);
                    state.LastClashSetPath = clashPath;
                    PersistState();
                }
                Pause();
                break;
            case "3":
                PropagateMenu();
                Pause();
                break;
            case "4":
                ValidateMenu();
                Pause();
                break;
            case "5":
                ShowConfiguration();
                Pause();
                break;
            case "0":
                return;
            default:
                Console.WriteLine("Unknown option. Please try again.");
                Pause();
                break;
        }
    }
}

void PropagateMenu()
{
    var searchPath = PromptForPath("Search set CSV path", state.LastSearchSetPath);
    if (string.IsNullOrEmpty(searchPath))
    {
        return;
    }

    var parseResult = searchParser.Parse(searchPath);
    if (ReportErrors(parseResult.Errors))
    {
        return;
    }

    state.LastSearchSetPath = searchPath;
    PersistState();

    var sets = parseResult.Items;
    var pairCount = sets.Count * (sets.Count - 1) / 2;
    Console.WriteLine($"Propagating {pairCount} clash tests (excluding identical pairs).");
    Console.Write("Continue? (y/N): ");
    var confirmation = Console.ReadLine();
    if (!string.Equals(confirmation, "y", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("Propagation cancelled.");
        return;
    }

    var includeSelf = PromptYesNo("Include self comparisons? (y/N): ");
    var excludeSameFolder = PromptYesNo("Exclude intra-folder pairs? (y/N): ");
    Console.Write("Exclude same prefix pairs? Enter delimiter or leave blank: ");
    var delimiter = Console.ReadLine() ?? string.Empty;
    Console.Write("Default tolerance in millimeters (leave blank for 0): ");
    var toleranceInput = Console.ReadLine();
    var tolerance = double.TryParse(toleranceInput, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedTolerance)
        ? parsedTolerance
        : 0d;

    var options = new PropagationOptions
    {
        IncludeSelfComparisons = includeSelf,
        IncludeSameFolder = !excludeSameFolder,
        ExcludePrefixDelimiter = delimiter,
        DefaultToleranceMillimeters = tolerance,
        TestType = "Hard"
    };

    var propagated = propagationService.Propagate(sets, options);
    var document = clashBuilder.Build(propagated, "PropagatedClashes");
    var outputFolder = PromptForOutputFolder(searchPath);
    var outputPath = WriteDocument(document, "ClashTests", outputFolder);
    Console.WriteLine($"Generated {propagated.Count} clash tests at {outputPath}");
}

void ValidateMenu()
{
    var searchPath = PromptForPath("Search set CSV path", state.LastSearchSetPath, allowEmpty: true);
    if (!string.IsNullOrEmpty(searchPath))
    {
        var result = searchParser.Parse(searchPath);
        ReportErrors(result.Errors, "Search set validation");
    }

    var clashPath = PromptForPath("Clash set CSV path", state.LastClashSetPath, allowEmpty: true);
    if (!string.IsNullOrEmpty(clashPath))
    {
        var result = clashParser.Parse(clashPath);
        ReportErrors(result.Errors, "Clash set validation");
    }
}

void ShowConfiguration()
{
    Console.WriteLine("Current configuration:");
    Console.WriteLine($"  Output folder: {ResolveOutputFolder()}");
    Console.WriteLine($"  Units: {configuration.Defaults.Units}");
    Console.WriteLine($"  Search category: {configuration.Defaults.Search.CategoryInternal} -> {configuration.Defaults.Search.CategoryDisplay}");
    Console.WriteLine($"  Property: {configuration.Defaults.Search.PropertyInternal} -> {configuration.Defaults.Search.PropertyDisplay}");
    Console.WriteLine($"  Clash status: {configuration.Defaults.Clash.Status}");
    Console.WriteLine($"  Default tolerance (mm): configurable per CSV");
}

void GenerateSearchSets(string path)
{
    var result = searchParser.Parse(path);
    if (ReportErrors(result.Errors))
    {
        return;
    }

    var document = searchBuilder.Build(result.Items);
    var outputFolder = PromptForOutputFolder(path);
    var outputPath = WriteDocument(document, "SearchSets", outputFolder);
    Console.WriteLine($"Generated search set XML at {outputPath}");
}

void GenerateClashTests(string path)
{
    var result = clashParser.Parse(path);
    if (ReportErrors(result.Errors))
    {
        return;
    }

    var document = clashBuilder.Build(result.Items, "ClashSets");
    var outputFolder = PromptForOutputFolder(path);
    var outputPath = WriteDocument(document, "ClashTests", outputFolder);
    Console.WriteLine($"Generated clash test XML at {outputPath}");
}

string WriteDocument(XDocument document, string prefix, string outputFolder)
{
    var folder = Path.GetFullPath(outputFolder);
    Directory.CreateDirectory(folder);
    var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
    var filePath = Path.Combine(folder, $"{prefix}_{timestamp}.xml");
    var settings = new XmlWriterSettings
    {
        Encoding = new UTF8Encoding(false),
        Indent = true,
        NewLineChars = "\n",
        NewLineHandling = NewLineHandling.Replace
    };

    using var stream = File.Create(filePath);
    using var writer = XmlWriter.Create(stream, settings);
    document.Save(writer);
    writer.Flush();
    return filePath;
}

string ResolveOutputFolder() => Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, configuration.Output.Folder));

string PromptForOutputFolder(string csvPath)
{
    var defaultFolder = Path.GetDirectoryName(csvPath);
    if (string.IsNullOrEmpty(defaultFolder))
    {
        defaultFolder = Environment.CurrentDirectory;
    }

    if (Console.IsInputRedirected)
    {
        return defaultFolder;
    }

    Console.Write($"Output folder [{defaultFolder}]: ");
    var input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input))
    {
        return defaultFolder;
    }

    return Path.GetFullPath(input.Trim());
}

string? PromptForPath(string label, string? last, bool allowEmpty = false)
{
    Console.Write($"{label}{(string.IsNullOrEmpty(last) ? string.Empty : $" [{last}]")}: ");
    var input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input))
    {
        if (allowEmpty)
        {
            return last ?? string.Empty;
        }

        return last;
    }

    return input.Trim();
}

bool PromptYesNo(string message)
{
    Console.Write(message);
    var input = Console.ReadLine();
    return string.Equals(input, "y", StringComparison.OrdinalIgnoreCase);
}

bool ReportErrors(IReadOnlyList<ValidationError> errors, string? title = null)
{
    if (errors.Count == 0)
    {
        Console.WriteLine(title is null ? "Validation succeeded." : $"{title}: OK");
        return false;
    }

    Console.WriteLine(title ?? "Validation errors:");
    foreach (var error in errors)
    {
        Console.WriteLine($"  - {error}");
    }

    return true;
}

void PersistState()
{
    try
    {
        UserStateStore.Save(statePath, state);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to persist state: {ex.Message}");
    }
}

void Pause()
{
    Console.WriteLine();
    Console.Write("Press Enter to continue...");
    Console.ReadLine();
}

void WriteBanner()
{
    Console.WriteLine("============================================");
    Console.WriteLine(" NavGen :: Navisworks XML Generator ");
    Console.WriteLine("============================================");
}
