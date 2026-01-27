using System.CommandLine;
using FlagstoneUI.PipelineRunner;

// ===== Root Command =====
var rootCommand = new RootCommand("Flagstone UI Pipeline Runner - Process prototypes through the conversion pipeline")
{
    Description = "Runs a React/TSX prototype through the full pipeline: Normalize → Structure → CSS/DLS → Tokens"
};

// ===== Options =====
var inputOption = new Option<DirectoryInfo>(
    aliases: ["--input", "-i"],
    description: "Input directory containing the React/TSX prototype")
{ IsRequired = true };

var outputOption = new Option<DirectoryInfo>(
    aliases: ["--output", "-o"],
    description: "Output directory for pipeline artifacts",
    getDefaultValue: () => new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "pipeline-output")));

var preserveTempOption = new Option<bool>(
    aliases: ["--preserve-temp"],
    description: "Keep the temp directory after run completes (for debugging)",
    getDefaultValue: () => false);

var noCleanupOption = new Option<bool>(
    aliases: ["--no-cleanup"],
    description: "Don't clean up stale temp directories on startup",
    getDefaultValue: () => false);

var verboseOption = new Option<bool>(
    aliases: ["--verbose", "-v"],
    description: "Show detailed progress output",
    getDefaultValue: () => false);

var cssSourceOption = new Option<DirectoryInfo?>(
    aliases: ["--css-source", "-c"],
    description: "Explicit path to CSS source files (for Tailwind compiled output)");

rootCommand.AddOption(inputOption);
rootCommand.AddOption(outputOption);
rootCommand.AddOption(preserveTempOption);
rootCommand.AddOption(noCleanupOption);
rootCommand.AddOption(verboseOption);
rootCommand.AddOption(cssSourceOption);

// ===== Handler =====
rootCommand.SetHandler(async (input, output, preserveTemp, noCleanup, verbose, cssSource) =>
{
    // Banner
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine();
    Console.WriteLine("  ╔═══════════════════════════════════════════╗");
    Console.WriteLine("  ║     Flagstone UI Pipeline Runner          ║");
    Console.WriteLine("  ║     Prototype → Tokens Conversion         ║");
    Console.WriteLine("  ╚═══════════════════════════════════════════╝");
    Console.ResetColor();
    Console.WriteLine();

    // Validate input
    if (!input.Exists)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"❌ Input directory not found: {input.FullName}");
        Console.ResetColor();
        Environment.Exit(1);
        return;
    }

    // Clean up stale temp directories unless disabled
    if (!noCleanup)
    {
        var cleaned = TempDirectoryManager.CleanupStaleDirectories();
        if (cleaned > 0 && verbose)
        {
            Console.WriteLine($"🗑️  Cleaned up {cleaned} stale temp directory(ies)");
            Console.WriteLine();
        }
    }

    // Configure and run pipeline
    var config = new PipelineConfig
    {
        InputPath = input.FullName,
        OutputPath = output.FullName,
        PreserveTemp = preserveTemp,
        NoCleanup = noCleanup,
        Verbose = verbose,
        CssSourcePath = cssSource?.FullName
    };

    var orchestrator = new PipelineOrchestrator(verbose);
    var manifest = await orchestrator.RunAsync(config);

    // Summary
    Console.WriteLine();
    Console.ForegroundColor = manifest.Status switch
    {
        PipelineStatus.Completed => ConsoleColor.Green,
        PipelineStatus.CompletedWithWarnings => ConsoleColor.Yellow,
        _ => ConsoleColor.Red
    };

    var statusEmoji = manifest.Status switch
    {
        PipelineStatus.Completed => "✅",
        PipelineStatus.CompletedWithWarnings => "⚠️",
        _ => "❌"
    };

    Console.WriteLine($"{statusEmoji} Pipeline {manifest.Status}");
    Console.ResetColor();

    if (manifest.DurationMs.HasValue)
    {
        Console.WriteLine($"   Duration: {manifest.DurationMs}ms");
    }

    Console.WriteLine($"   Artifacts: {manifest.Artifacts.Count}");
    Console.WriteLine($"   Output: {manifest.OutputPath}");

    if (manifest.Errors.Any())
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Errors:");
        foreach (var error in manifest.Errors)
        {
            Console.WriteLine($"  • {error}");
        }
        Console.ResetColor();
    }

    // List warnings from all stages
    var allWarnings = manifest.Stages.SelectMany(s => s.Warnings).ToList();
    if (allWarnings.Any())
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Warnings:");
        foreach (var warning in allWarnings)
        {
            Console.WriteLine($"  • {warning}");
        }
        Console.ResetColor();
    }

    Console.WriteLine();

    // Exit code based on status
    Environment.Exit(manifest.Status == PipelineStatus.Failed ? 1 : 0);

}, inputOption, outputOption, preserveTempOption, noCleanupOption, verboseOption, cssSourceOption);

// ===== List Runs Command =====
var listCommand = new Command("list", "List existing pipeline run temp directories");

listCommand.SetHandler(() =>
{
    var runs = TempDirectoryManager.GetExistingRuns().ToList();

    if (!runs.Any())
    {
        Console.WriteLine("No existing pipeline runs found.");
        return;
    }

    Console.WriteLine($"Found {runs.Count} pipeline run(s):");
    Console.WriteLine();

    foreach (var (path, runId, createdAt) in runs.OrderByDescending(r => r.CreatedAt))
    {
        var age = DateTime.UtcNow - createdAt;
        var ageStr = age.TotalHours < 1 ? $"{age.Minutes}m ago" :
                     age.TotalDays < 1 ? $"{age.Hours}h ago" :
                     $"{age.Days}d ago";

        Console.WriteLine($"  {runId}  {createdAt:yyyy-MM-dd HH:mm}  ({ageStr})");
        Console.WriteLine($"    {path}");
    }
});

rootCommand.AddCommand(listCommand);

// ===== Clean Command =====
var cleanCommand = new Command("clean", "Clean up all pipeline temp directories");

var maxAgeOption = new Option<int>(
    aliases: ["--max-age"],
    description: "Maximum age in hours (0 = delete all)",
    getDefaultValue: () => 0);

cleanCommand.AddOption(maxAgeOption);

cleanCommand.SetHandler((maxAge) =>
{
    var age = maxAge == 0 ? TimeSpan.Zero : TimeSpan.FromHours(maxAge);
    var cleaned = TempDirectoryManager.CleanupStaleDirectories(age);

    if (cleaned == 0)
    {
        Console.WriteLine("No temp directories to clean up.");
    }
    else
    {
        Console.WriteLine($"🗑️  Cleaned up {cleaned} temp directory(ies).");
    }
}, maxAgeOption);

rootCommand.AddCommand(cleanCommand);

// ===== Run =====
return await rootCommand.InvokeAsync(args);
