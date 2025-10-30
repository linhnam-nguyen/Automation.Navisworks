# NavGen

NavGen is a cross-platform CLI tool that transforms tabular CSV descriptions of Autodesk Navisworks search sets and clash tests into exchange XML files. The repository also contains sample CSV files that reproduce the XML examples distributed with Navisworks, automated validation, and clash propagation utilities.

## Solution layout

```
NavGen.sln
├─ src/
│  ├─ NavGen.Core/   # Domain models, CSV parsing, validation, XML builders
│  └─ NavGen.Cli/    # Interactive CLI host and configuration
├─ tests/
│  └─ NavGen.Tests/  # xUnit unit and integration tests
├─ samples/          # CSV files reproducing SamplesXML content
└─ SamplesXML/       # Reference XML payloads (unmodified)
```

## Prerequisites

* .NET SDK 8.0 or newer (the project is compatible with future .NET 10 runtimes).
* PowerShell, Bash, or any shell capable of invoking `dotnet` commands.

## Building

```
dotnet restore
```

To publish a self-contained native AOT executable for the current OS/architecture:

```
dotnet publish src/NavGen.Cli/NavGen.Cli.csproj -c Release -r <RID> -p:PublishAot=true --self-contained true
```

Replace `<RID>` with a Runtime Identifier such as `win-x64`, `linux-x64`, or `osx-arm64`.

## Testing

Run all unit and integration tests with:

```
dotnet test
```

## CLI usage

Launch the CLI menu:

```
dotnet run --project src/NavGen.Cli/NavGen.Cli.csproj
```

You can also run individual operations directly:

```
# Generate SearchSets XML
dotnet run --project src/NavGen.Cli/NavGen.Cli.csproj -- --search path/to/search_sets.csv

# Generate ClashSets XML
dotnet run --project src/NavGen.Cli/NavGen.Cli.csproj -- --clash path/to/clash_sets.csv
```

### Menu options

1. **Generate Search Set XML** – prompts for a CSV file (defaults to the most recent path) and writes `SearchSets_{timestamp}.xml` to the configured output folder.
2. **Generate Clash Set XML** – same workflow for clash test CSV files, outputting `ClashTests_{timestamp}.xml`.
3. **Propagate clashes** – builds an N×(N−1)/2 matrix of clash tests from the loaded search sets. Users can optionally exclude intra-folder or same-prefix pairs and set a default tolerance.
4. **Validate CSV files** – runs header, required field, enum, and numeric validation for search and clash CSV files, reporting all issues with line numbers.
5. **Show configuration** – displays the effective output folder and default metadata.

The CLI stores the most recently used search and clash CSV paths in `%AppData%/NavGen/state.json` (or the equivalent directory on Unix-like systems).

## CSV format reference

### Search sets

| Column        | Required | Description                                                                 |
|---------------|----------|-----------------------------------------------------------------------------|
| `SetName`     | ✔        | Logical search set name. Rows with the same name are merged into one set.   |
| `Category`    | ✔        | Category reference. Either `Display` or `Internal|Display` form.            |
| `PropertyName`| ✔        | Property reference. Either `Display` or `Internal|Display` form.            |
| `Operator`    | ✔        | Navisworks comparison operator (e.g., `Equals`, `Contains`).               |
| `Value`       | ✔        | Value in `type:value` format (defaults to `wstring`).                       |
| `MatchLogic`  | ✔        | `All` or `Any`.                                                             |
| `ColorHex`    |          | Optional hex color (prefix `#` added when omitted).                         |
| `Description` |          | Optional comment exported as `<comment>`.                                  |
| `Folder`      |          | Logical folder for grouping in the XML tree.                               |

### Clash tests

| Column         | Required | Description                                                             |
|----------------|----------|-------------------------------------------------------------------------|
| `TestName`     | ✔        | Clash test name.                                                         |
| `LeftSet`      | ✔        | Left selection set locator (relative to `lcop_selection_set_tree/`).    |
| `RightSet`     | ✔        | Right selection set locator.                                            |
| `ToleranceMM`  | ✔        | Clash tolerance in millimetres (≥ 0).                                    |
| `Type`         | ✔        | `Hard` or `Clearance`.                                                   |
| `Active`       |          | Optional boolean flag (`true`/`false`). Inactive tests are marked `disabled`. |
| `Severity`     |          | Optional priority exported as the `priority` attribute.                  |
| `Grouping`     |          | Optional grouping string exported as the `group` attribute.              |
| `Description`  |          | Optional description node.                                               |

## Configuration

The CLI reads `appsettings.json` (copied into the output directory) for defaults:

```
{
  "output": {
    "folder": "out"
  },
  "defaults": {
    "units": "ft",
    "search": {
      "categoryInternal": "LcRevitData_Element",
      "categoryDisplay": "Element",
      "propertyInternal": "LcRevitPropertyElementCategory",
      "propertyDisplay": "Category",
      "disjoint": 0
    },
    "clash": {
      "status": "ok",
      "mergeComposites": 1,
      "selfIntersect": 0,
      "primitiveTypes": 1,
      "linkageMode": "none"
    }
  },
  "colors": {
    "fallback": "#FF9A00"
  }
}
```

Update these values to adjust units, default metadata, or the output directory. User selections are persisted separately in `state.json` as described earlier.

## Samples

The `samples/` directory contains `search_sets.csv` and `clash_sets.csv` files that reproduce the XML structures found under `SamplesXML/`. These are used by integration tests to ensure structural equivalence.

## License

This project is distributed under the MIT License. See [LICENSE](LICENSE) for details.
