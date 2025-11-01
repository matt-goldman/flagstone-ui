using System.CommandLine;
using FlagstoneUI.TokenGenerator;

var rootCommand = new RootCommand("Flagstone UI Theme Tools")
{
    Description = "Tools for managing Flagstone UI themes and tokens"
};

// ===== Generate Command =====
var generateCommand = new Command("generate", "Generate token catalog from XAML files");

var genSourceOption = new Option<DirectoryInfo>(
    aliases: ["--source", "-s"],
    description: "Source directory containing XAML files",
    getDefaultValue: () => new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "src")));

var genOutputOption = new Option<FileInfo>(
    aliases: ["--output", "-o"],
    description: "Output path for the JSON catalog",
    getDefaultValue: () => new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), "docs", "tokens-catalog.json")));

generateCommand.AddOption(genSourceOption);
generateCommand.AddOption(genOutputOption);

generateCommand.SetHandler(async (sourceDir, outputFile) =>
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("🔍 Flagstone UI Token Catalog Generator");
    Console.ResetColor();
    Console.WriteLine($"   Source: {sourceDir.FullName}");
    Console.WriteLine($"   Output: {outputFile.FullName}");
    Console.WriteLine();

    try
    {
        if (!sourceDir.Exists)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Source directory not found: {sourceDir.FullName}");
            Console.ResetColor();
            Environment.Exit(1);
            return;
        }

        var generator = new TokenCatalogGenerator();
        var catalog = await generator.GenerateAsync(sourceDir.FullName);

        outputFile.Directory?.Create();
        await File.WriteAllTextAsync(outputFile.FullName, catalog);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("✅ Token catalog generated successfully!");
        Console.ResetColor();
        Console.WriteLine($"   File: {outputFile.FullName}");
        Console.WriteLine($"   Size: {new FileInfo(outputFile.FullName).Length:N0} bytes");
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"❌ Error: {ex.Message}");
        Console.ResetColor();
        Environment.Exit(1);
    }
}, genSourceOption, genOutputOption);

// ===== Validate Command =====
var validateCommand = new Command("validate", "Validate theme tokens (XAML or JSON)");

var valInputOption = new Option<FileInfo>(
    aliases: ["--input", "-i"],
    description: "Path to XAML or JSON file to validate")
{ IsRequired = true };

var valFormatOption = new Option<string>(
    aliases: ["--format", "-f"],
    description: "Input format: xaml or json (auto-detected if not specified)",
    getDefaultValue: () => "auto");

var valJsonOutputOption = new Option<bool>(
    aliases: ["--json", "-j"],
    description: "Output results as JSON",
    getDefaultValue: () => false);

validateCommand.AddOption(valInputOption);
validateCommand.AddOption(valFormatOption);
validateCommand.AddOption(valJsonOutputOption);

validateCommand.SetHandler((inputFile, format, jsonOutput) =>
{
    if (!jsonOutput)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("🔍 Flagstone UI Theme Validator");
        Console.ResetColor();
        Console.WriteLine($"   Input: {inputFile.FullName}");
        Console.WriteLine();
    }

    try
    {
        if (!inputFile.Exists)
        {
            throw new FileNotFoundException($"Input file not found: {inputFile.FullName}");
        }

        var validator = new ThemeValidator();
        
        // Auto-detect format if needed
        if (format == "auto")
        {
            format = inputFile.Extension.ToLowerInvariant() == ".json" ? "json" : "xaml";
        }

        var result = format.ToLowerInvariant() == "json" 
            ? validator.ValidateJson(inputFile.FullName) 
            : validator.ValidateXaml(inputFile.FullName);

        if (jsonOutput)
        {
            Console.WriteLine(result.ToJsonString());
        }
        else
        {
            if (result.IsValid)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✅ Validation passed!");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Validation failed with {result.Errors.Count} error(s)");
                Console.ResetColor();
                Console.WriteLine();
                
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"   • {error.Token}: {error.Message}");
                }
            }

            if (result.Warnings.Count > 0)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"⚠️  {result.Warnings.Count} warning(s):");
                Console.ResetColor();
                
                foreach (var warning in result.Warnings)
                {
                    Console.WriteLine($"   • {warning.Token}: {warning.Message}");
                }
            }
        }

        if (!result.IsValid)
        {
            Environment.Exit(1);
        }
    }
    catch (Exception ex)
    {
        if (jsonOutput)
        {
            Console.WriteLine($"{{\"valid\": false, \"error\": \"{ex.Message}\"}}");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Error: {ex.Message}");
            Console.ResetColor();
        }
        Environment.Exit(1);
    }
}, valInputOption, valFormatOption, valJsonOutputOption);

// ===== Generate XAML Command =====
var generateXamlCommand = new Command("generate-xaml", "Generate XAML from JSON catalog");

var xamlInputOption = new Option<FileInfo>(
    aliases: ["--input", "-i"],
    description: "Path to JSON catalog file",
    getDefaultValue: () => new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), "docs", "tokens-catalog.json")));

var xamlOutputOption = new Option<FileInfo>(
    aliases: ["--output", "-o"],
    description: "Output path for XAML file",
    getDefaultValue: () => new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), "src", "FlagstoneUI.Core", "Styles", "Tokens.xaml")));

generateXamlCommand.AddOption(xamlInputOption);
generateXamlCommand.AddOption(xamlOutputOption);

generateXamlCommand.SetHandler((inputFile, outputFile) =>
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("🔍 Flagstone UI XAML Generator");
    Console.ResetColor();
    Console.WriteLine($"   Input:  {inputFile.FullName}");
    Console.WriteLine($"   Output: {outputFile.FullName}");
    Console.WriteLine();

    try
    {
        if (!inputFile.Exists)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Input file not found: {inputFile.FullName}");
            Console.ResetColor();
            Environment.Exit(1);
            return;
        }

        var generator = new XamlGenerator();
        generator.GenerateXamlFile(inputFile.FullName, outputFile.FullName);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("✅ XAML generated successfully!");
        Console.ResetColor();
        Console.WriteLine($"   File: {outputFile.FullName}");
        Console.WriteLine($"   Size: {new FileInfo(outputFile.FullName).Length:N0} bytes");
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"❌ Error: {ex.Message}");
        Console.ResetColor();
        Environment.Exit(1);
    }
}, xamlInputOption, xamlOutputOption);

// Add commands to root
rootCommand.AddCommand(generateCommand);
rootCommand.AddCommand(validateCommand);
rootCommand.AddCommand(generateXamlCommand);

return await rootCommand.InvokeAsync(args);
