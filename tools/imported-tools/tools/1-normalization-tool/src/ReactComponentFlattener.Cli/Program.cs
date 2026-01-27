using ReactComponentFlattener.Core.Services;

// Parse command-line arguments
var cmdArgs = Environment.GetCommandLineArgs().Skip(1).ToArray();

if (cmdArgs.Length == 0 || cmdArgs.Contains("--help") || cmdArgs.Contains("-h"))
{
    ShowHelp();
    return 0;
}

string inputPath = cmdArgs[0];
string? outputPath = null;
string? reportPath = null;
bool dryRun = false;
bool preserveComments = true;
int? maxDepth = null;
bool copyCss = false;
bool copyAll = false;
bool editInPlace = false;
bool unusedComponentsSpecified = false;
bool emitHtml = false;
UnusedComponentAction unusedComponents = UnusedComponentAction.Remove; // Default for output mode

// Parse options
for (int i = 1; i < cmdArgs.Length; i++)
{
    switch (cmdArgs[i])
    {
        case "--out":
        case "--output":
            if (i + 1 < cmdArgs.Length)
                outputPath = cmdArgs[++i];
            break;
        case "--report":
            if (i + 1 < cmdArgs.Length)
                reportPath = cmdArgs[++i];
            break;
        case "--dry-run":
            dryRun = true;
            break;
        case "--edit-in-place":
            editInPlace = true;
            break;
        case "--emit-html":
            emitHtml = true;
            break;
        case "--unused-components":
            if (i + 1 < cmdArgs.Length)
            {
                unusedComponentsSpecified = true;
                var value = cmdArgs[++i].ToLowerInvariant();
                
                // Validate and provide helpful error message for invalid values
                if (value != "retain" && value != "remove" && value != "archive")
                {
                    Console.Error.WriteLine($"Invalid value for --unused-components: {value}. Valid values are: retain, remove, archive. Defaulting to 'retain'.");
                    unusedComponents = UnusedComponentAction.Retain;
                }
                else
                {
                    unusedComponents = value switch
                    {
                        "retain" => UnusedComponentAction.Retain,
                        "remove" => UnusedComponentAction.Remove,
                        "archive" => UnusedComponentAction.Archive,
                        _ => throw new InvalidOperationException($"Unexpected value '{value}' after validation")
                    };
                }
            }
            break;
        case "--preserve-comments":
            if (i + 1 < cmdArgs.Length)
            {
                if (!bool.TryParse(cmdArgs[++i], out preserveComments))
                {
                    Console.Error.WriteLine($"Invalid boolean value for --preserve-comments: {cmdArgs[i]}");
                    return 1;
                }
            }
            else
            {
                preserveComments = true;
            }
            break;
        case "--max-depth":
            if (i + 1 < cmdArgs.Length)
            {
                if (int.TryParse(cmdArgs[++i], out var depth))
                {
                    maxDepth = depth;
                }
                else
                {
                    Console.Error.WriteLine($"Invalid integer value for --max-depth: {cmdArgs[i]}");
                    return 1;
                }
            }
            break;
        case "--copy-css":
            copyCss = true;
            break;
        case "--copy-all":
            copyAll = true;
            break;
    }
}

// Validate input
if (!File.Exists(inputPath) && !Directory.Exists(inputPath))
{
    Console.Error.WriteLine($"Error: Input path '{inputPath}' does not exist");
    return 1;
}

// Validate mode and set defaults
if (editInPlace && !string.IsNullOrEmpty(outputPath))
{
    Console.Error.WriteLine("Error: Cannot use both --edit-in-place and --output");
    return 1;
}

// Validate that copyCss and copyAll are only used in output mode
if (editInPlace && (copyCss || copyAll))
{
    Console.Error.WriteLine("Error: --copy-css and --copy-all can only be used in output mode (without --edit-in-place)");
    return 1;
}

// Validate that emit-html is only used in output mode
if (editInPlace && emitHtml)
{
    Console.Error.WriteLine("Error: --emit-html can only be used in output mode (without --edit-in-place)");
    return 1;
}

if (!editInPlace && string.IsNullOrEmpty(outputPath))
{
    // Default to output mode with default path
    outputPath = "./normalised";
}

if (editInPlace)
{
    // For edit-in-place, default unused action is archive
    if (!unusedComponentsSpecified)
    {
        unusedComponents = UnusedComponentAction.Archive;
    }
    outputPath = inputPath; // Use input path for in-place editing
}

// Set default report path
if (string.IsNullOrEmpty(reportPath))
{
    reportPath = "./normalisation.json";
}

Console.WriteLine("React Component Flattener");
Console.WriteLine("========================");
Console.WriteLine($"Input: {inputPath}");
Console.WriteLine($"Mode: {(editInPlace ? "Edit in place" : $"Output to {outputPath}")}");
Console.WriteLine($"Report: {reportPath}");
Console.WriteLine($"Unused components: {unusedComponents}");
Console.WriteLine($"Emit HTML: {emitHtml}");
Console.WriteLine($"Dry Run: {dryRun}");
if (!editInPlace)
{
    Console.WriteLine($"Copy CSS: {copyCss}");
    Console.WriteLine($"Copy All: {copyAll}");
}
Console.WriteLine();

try
{
    var parserService = new AcornimaParserService();
    var flattener = new ComponentFlattener(parserService);
    
    var options = new FlatteningOptions
    {
        DryRun = dryRun,
        PreserveComments = preserveComments,
        MaxDepth = maxDepth,
        CopyCss = copyCss,
        CopyAll = copyAll,
        EditInPlace = editInPlace,
        UnusedComponents = unusedComponents,
        EmitHtml = emitHtml
    };

    if (File.Exists(inputPath))
    {
        // Process single file
        var (flattenedCode, report) = await flattener.FlattenFileAsync(inputPath, options);
        
        if (!dryRun)
        {
            if (editInPlace)
            {
                await File.WriteAllTextAsync(inputPath, flattenedCode);
                Console.WriteLine($"Updated: {inputPath}");
            }
            else
            {
                Directory.CreateDirectory(outputPath);
                var fileName = Path.GetFileName(inputPath);
                // When emitting HTML, change extension to .html to match multi-file behavior
                if (emitHtml && (fileName.EndsWith(".jsx", StringComparison.OrdinalIgnoreCase) || 
                                  fileName.EndsWith(".tsx", StringComparison.OrdinalIgnoreCase)))
                {
                    fileName = Path.ChangeExtension(fileName, ".html");
                }
                var outputFile = Path.Combine(outputPath, fileName);
                await File.WriteAllTextAsync(outputFile, flattenedCode);
                Console.WriteLine($"Written: {outputFile}");
            }
        }
        
        // Write report
        var reportJson = System.Text.Json.JsonSerializer.Serialize(report, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });
        await File.WriteAllTextAsync(reportPath, reportJson);
        
        Console.WriteLine($"\nFlattened: {report.Flattened.Count}");
        Console.WriteLine($"Preserved: {report.Preserved.Count}");
    }
    else
    {
        // Process directory
        await flattener.ProcessDirectoryAsync(inputPath, outputPath, reportPath, options);
    }
    
    Console.WriteLine("\nDone!");
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    Console.Error.WriteLine(ex.StackTrace);
    return 1;
}

static void ShowHelp()
{
    Console.WriteLine("React Component Flattener (proto-normalize)");
    Console.WriteLine();
    Console.WriteLine("Aggressively flattens AI-generated or over-componentised React code.");
    Console.WriteLine("Operates on closed-world assumption within provided scope.");
    Console.WriteLine();
    Console.WriteLine("Usage: proto-normalize <input> [options]");
    Console.WriteLine();
    Console.WriteLine("Arguments:");
    Console.WriteLine("  <input>                       Input file or directory path");
    Console.WriteLine();
    Console.WriteLine("Output Mode Options (mutually exclusive):");
    Console.WriteLine("  --output <path>               Output to new directory (default: ./normalised)");
    Console.WriteLine("  --out <path>                  Alias for --output");
    Console.WriteLine("  --edit-in-place               Modify files in place");
    Console.WriteLine();
    Console.WriteLine("Flattening Options:");
    Console.WriteLine("  --unused-components <action>  Handle unused components after flattening");
    Console.WriteLine("                                  retain  - Keep in output");
    Console.WriteLine("                                  remove  - Don't include (default for --output)");
    Console.WriteLine("                                  archive - Move to _archive folder (default for --edit-in-place)");
    Console.WriteLine("  --report <path>               Report output path (default: ./normalisation.json)");
    Console.WriteLine("  --dry-run                     Analyze without writing files");
    Console.WriteLine();
    Console.WriteLine("Output Mode Only Options:");
    Console.WriteLine("  --copy-css                    Copy CSS files to output directory");
    Console.WriteLine("  --copy-all                    Copy entire app structure (includes all non-React files)");
    Console.WriteLine("  --emit-html                   Emit HTML files instead of JSX/TSX (for pipeline use)");
    Console.WriteLine("                                HTML includes data-component attributes for downstream tools");
    Console.WriteLine();
    Console.WriteLine("Advanced Options:");
    Console.WriteLine("  --preserve-comments           Preserve comments (default: true)");
    Console.WriteLine("  --max-depth <n>               Maximum nesting depth");
    Console.WriteLine("  --help, -h                    Show this help");
    Console.WriteLine();
    Console.WriteLine("Flattening Rules:");
    Console.WriteLine("  Components are flattened unless they have:");
    Console.WriteLine("  - Behavioral ownership (hooks, state, context)");
    Console.WriteLine("  - Semantic independence (meaningful outside parent)");
    Console.WriteLine("  - Cross-visual-role usage (used in different contexts)");
    Console.WriteLine();
    Console.WriteLine("  Component families (Card*, Dialog*, etc.) are flattened together");
    Console.WriteLine("  unless individual members meet preservation criteria.");
    Console.WriteLine();
    Console.WriteLine("Examples:");
    Console.WriteLine("  # Output to new directory");
    Console.WriteLine("  proto-normalize ./src --output ./normalised");
    Console.WriteLine();
    Console.WriteLine("  # Edit in place with archiving");
    Console.WriteLine("  proto-normalize ./src --edit-in-place --unused-components archive");
    Console.WriteLine();
    Console.WriteLine("  # Output mode with CSS files");
    Console.WriteLine("  proto-normalize ./src --output ./normalised --copy-css");
    Console.WriteLine();
    Console.WriteLine("  # Dry run to preview changes");
    Console.WriteLine("  proto-normalize component.tsx --dry-run");
}
