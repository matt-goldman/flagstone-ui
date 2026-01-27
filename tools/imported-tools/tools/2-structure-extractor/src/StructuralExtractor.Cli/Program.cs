using StructuralExtractor.Core.Services;

// Parse command-line arguments
var cmdArgs = Environment.GetCommandLineArgs().Skip(1).ToArray();

if (cmdArgs.Length == 0 || cmdArgs.Contains("--help") || cmdArgs.Contains("-h"))
{
    ShowHelp();
    return 0;
}

string inputPath = cmdArgs[0];
string? outputPath = null;
OutputFormat format = OutputFormat.Yaml;

// Parse options
for (int i = 1; i < cmdArgs.Length; i++)
{
    switch (cmdArgs[i])
    {
        case "--out":
        case "-o":
            if (i + 1 < cmdArgs.Length)
                outputPath = cmdArgs[++i];
            break;
        case "--format":
        case "-f":
            if (i + 1 < cmdArgs.Length)
            {
                var formatStr = cmdArgs[++i].ToLowerInvariant();
                format = formatStr == "json" ? OutputFormat.Json : OutputFormat.Yaml;
            }
            break;
    }
}

// Validate input
if (!File.Exists(inputPath) && !Directory.Exists(inputPath))
{
    Console.Error.WriteLine($"Error: Input path '{inputPath}' does not exist");
    return 1;
}

// Set default output path
if (string.IsNullOrEmpty(outputPath))
{
    var extension = format == OutputFormat.Yaml ? "yaml" : "json";
    outputPath = $"./structure.{extension}";
}

Console.WriteLine("Structural Extractor");
Console.WriteLine("===================");
Console.WriteLine($"Input: {inputPath}");
Console.WriteLine($"Output: {outputPath}");
Console.WriteLine($"Format: {format}");
Console.WriteLine();

try
{
    var extractor = new StructuralExtractorService();
    var outputService = new OutputService();

    Console.WriteLine("Analyzing files...");
    var structure = await extractor.ExtractStructureAsync(inputPath);

    Console.WriteLine($"Found {structure.Components.Count} components");
    Console.WriteLine($"Found {structure.Pages.Count} pages");
    Console.WriteLine();

    Console.WriteLine("Writing output...");
    await outputService.WriteStructureAsync(structure, outputPath, format);

    Console.WriteLine($"✓ Structure written to: {outputPath}");
    Console.WriteLine();
    Console.WriteLine("Done!");

    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    if (ex.StackTrace != null)
    {
        Console.Error.WriteLine(ex.StackTrace);
    }
    return 1;
}

static void ShowHelp()
{
    Console.WriteLine(@"
Structural Extractor - Extract application structure from React prototypes

Usage:
  structural-extractor <input> [options]

Arguments:
  <input>              Input file or directory path

Options:
  --out, -o <path>     Output file path (default: ./structure.yaml)
  --format, -f <fmt>   Output format: yaml or json (default: yaml)
  --help, -h           Show this help message

Examples:
  # Extract structure from a directory
  structural-extractor ./app --out structure.yaml

  # Extract as JSON
  structural-extractor ./src --format json --out structure.json

  # Single file
  structural-extractor page.tsx
");
}
