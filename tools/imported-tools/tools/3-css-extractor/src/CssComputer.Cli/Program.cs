using CssComputer.Core.Models;
using CssComputer.Core.Services;
using System.Globalization;
using System.Text.Json;

// Parse command-line arguments
var cmdArgs = Environment.GetCommandLineArgs().Skip(1).ToArray();

if (cmdArgs.Length == 0 || cmdArgs.Contains("--help") || cmdArgs.Contains("-h"))
{
    ShowHelp();
    return 0;
}

string inputPath = cmdArgs[0];
string? cssSourcePath = null;
string? outputPath = null;
string? reportPath = null;
bool emitCss = false;
double numericTolerance = 0.0;

// Parse options
for (int i = 1; i < cmdArgs.Length; i++)
{
    switch (cmdArgs[i])
    {
        case "--css-source":
        case "-c":
            if (i + 1 < cmdArgs.Length)
                cssSourcePath = cmdArgs[++i];
            break;
        case "--out":
        case "-o":
            if (i + 1 < cmdArgs.Length)
                outputPath = cmdArgs[++i];
            break;
        case "--report":
            if (i + 1 < cmdArgs.Length)
                reportPath = cmdArgs[++i];
            break;
        case "--emit-css":
            emitCss = true;
            break;
        case "--tolerance":
            if (i + 1 < cmdArgs.Length)
            {
                if (double.TryParse(cmdArgs[++i], NumberStyles.Float, CultureInfo.InvariantCulture, out var tolerance))
                {
                    numericTolerance = tolerance;
                }
                else
                {
                    Console.Error.WriteLine($"Invalid numeric value for --tolerance: {cmdArgs[i]}");
                    return 1;
                }
            }
            else
            {
                Console.Error.WriteLine("Missing value for --tolerance");
                return 1;
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
    outputPath = "./dls.json";
}

// Set default report path
if (string.IsNullOrEmpty(reportPath))
{
    reportPath = "./computation-report.json";
}

Console.WriteLine("CSS Computer - Design Language Specification Extractor");
Console.WriteLine("=======================================================");
Console.WriteLine($"Input: {inputPath}");
if (!string.IsNullOrEmpty(cssSourcePath))
    Console.WriteLine($"CSS Source: {cssSourcePath}");
Console.WriteLine($"Output: {outputPath}");
Console.WriteLine($"Report: {reportPath}");
Console.WriteLine($"Emit CSS: {emitCss}");
Console.WriteLine($"Numeric Tolerance: {numericTolerance}");
Console.WriteLine();

try
{
    var computerService = new CssComputerService();
    
    var options = new ComputationOptions
    {
        EmitCssProjection = emitCss,
        NumericTolerance = numericTolerance,
        IncludeSourceMetadata = true,
        CssSourcePath = cssSourcePath  // Separate CSS source directory
    };

    Console.WriteLine("Stage 1: Resolving styles...");
    Console.WriteLine("Stage 2: Normalizing values...");
    Console.WriteLine("Stage 3: Grouping styles...");
    Console.WriteLine("Stage 4: Detecting variants...");

    var (dls, report) = await computerService.ComputeAsync(inputPath, options);

    Console.WriteLine();
    Console.WriteLine($"Processed {report.TotalElements} elements");
    Console.WriteLine($"Generated {report.UniqueStyles} unique styles");
    Console.WriteLine($"Detected {report.TotalVariants} variants");
    Console.WriteLine();

    // Write DLS output
    var dlsJson = JsonSerializer.Serialize(dls, new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    });
    await File.WriteAllTextAsync(outputPath, dlsJson);
    Console.WriteLine($"✓ DLS written to: {outputPath}");

    // Write report
    var reportJson = JsonSerializer.Serialize(report, new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    });
    await File.WriteAllTextAsync(reportPath, reportJson);
    Console.WriteLine($"✓ Report written to: {reportPath}");

    // Emit CSS projection if requested
    if (emitCss)
    {
        var cssPath = Path.ChangeExtension(outputPath, ".css");
        var css = GenerateCssProjection(dls);
        await File.WriteAllTextAsync(cssPath, css);
        Console.WriteLine($"✓ CSS projection written to: {cssPath}");
    }

    if (report.Warnings.Count > 0)
    {
        Console.WriteLine();
        Console.WriteLine("Warnings:");
        foreach (var warning in report.Warnings)
        {
            Console.WriteLine($"  ⚠ {warning}");
        }
    }

    Console.WriteLine();
    Console.WriteLine("✓ Computation complete");
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    Console.Error.WriteLine(ex.StackTrace);
    return 1;
}

void ShowHelp()
{
    Console.WriteLine("CSS Computer - Design Language Specification Extractor");
    Console.WriteLine();
    Console.WriteLine("Usage: CssComputer <input-path> [options]");
    Console.WriteLine();
    Console.WriteLine("Arguments:");
    Console.WriteLine("  <input-path>               Path to normalized prototype source (file or directory)");
    Console.WriteLine();
    Console.WriteLine("Options:");
    Console.WriteLine("  -c, --css-source <path>    Separate CSS source directory (for Tailwind builds)");
    Console.WriteLine("  -o, --out <path>           Output path for DLS JSON (default: ./dls.json)");
    Console.WriteLine("  --report <path>            Output path for computation report (default: ./computation-report.json)");
    Console.WriteLine("  --emit-css                 Emit optional CSS projection for inspection");
    Console.WriteLine("  --tolerance <value>        Numeric tolerance for style grouping (default: 0.0)");
    Console.WriteLine("  -h, --help                 Show this help message");
    Console.WriteLine();
    Console.WriteLine("Examples:");
    Console.WriteLine("  CssComputer ./normalized-prototype --out ./dls.json --emit-css");
    Console.WriteLine("  CssComputer ./html-output --css-source ./original-project --out ./dls.json");
}

string GenerateCssProjection(DesignLanguageSpecification dls)
{
    var css = new System.Text.StringBuilder();
    css.AppendLine("/* CSS Projection - FOR INSPECTION ONLY */");
    css.AppendLine("/* This is a lossy projection from the canonical DLS */");
    css.AppendLine("/* Do not re-parse or treat as authoritative */");
    css.AppendLine();

    foreach (var style in dls.Styles)
    {
        css.AppendLine($".{style.Id} {{");
        foreach (var (key, value) in style.Properties)
        {
            css.AppendLine($"  {key}: {value};");
        }
        css.AppendLine("}");
        css.AppendLine();

        // Emit variants
        if (style.Variants != null)
        {
            foreach (var variant in style.Variants)
            {
                css.AppendLine($".{style.Id}.{variant.Name} {{");
                foreach (var (key, value) in variant.Properties)
                {
                    css.AppendLine($"  {key}: {value};");
                }
                css.AppendLine("}");
                css.AppendLine();
            }
        }
    }

    return css.ToString();
}
