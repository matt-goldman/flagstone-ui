using System.CommandLine;
using FlagstoneUI.TokenGenerator;

var rootCommand = new RootCommand("FlagstoneUI Theme Tools")
{
    Description = "Tools for managing FlagstoneUI themes, tokens, and design system contracts"
};

// ===== Generate Catalog Command =====
var generateCommand = new Command("generate", "Generate token catalog from XAML files (legacy)");

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
    Console.WriteLine("🔍 FlagstoneUI Token Catalog Generator");
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
    catch (UnauthorizedAccessException ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"❌ Access denied: {ex.Message}");
        Console.ResetColor();
        Environment.Exit(1);
    }
    catch (IOException ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"❌ File operation failed: {ex.Message}");
        Console.ResetColor();
        Environment.Exit(1);
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
        Console.WriteLine("🔍 FlagstoneUI Theme Validator");
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
    catch (FileNotFoundException ex)
    {
        if (jsonOutput)
        {
            Console.WriteLine($"{{\"valid\": false, \"error\": \"File not found: {ex.Message}\"}}");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ File not found: {ex.Message}");
            Console.ResetColor();
        }
        Environment.Exit(1);
    }
    catch (IOException ex)
    {
        if (jsonOutput)
        {
            Console.WriteLine($"{{\"valid\": false, \"error\": \"File operation failed: {ex.Message}\"}}");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ File operation failed: {ex.Message}");
            Console.ResetColor();
        }
        Environment.Exit(1);
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
    Console.WriteLine("🔍 FlagstoneUI XAML Generator");
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
    catch (FileNotFoundException ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"❌ File not found: {ex.Message}");
        Console.ResetColor();
        Environment.Exit(1);
    }
    catch (UnauthorizedAccessException ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"❌ Access denied: {ex.Message}");
        Console.ResetColor();
        Environment.Exit(1);
    }
    catch (IOException ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"❌ File operation failed: {ex.Message}");
        Console.ResetColor();
        Environment.Exit(1);
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"❌ Error: {ex.Message}");
        Console.ResetColor();
        Environment.Exit(1);
    }
}, xamlInputOption, xamlOutputOption);

// ===== Generate Contract Command =====
var generateContractCommand = new Command("generate-contract", "Generate design system contract from source files");

var contractSourceOption = new Option<DirectoryInfo>(
    aliases: ["--source", "-s"],
    description: "Source directory containing FlagstoneUI.Core",
    getDefaultValue: () => new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "src")));

var contractOutputOption = new Option<FileInfo>(
    aliases: ["--output", "-o"],
    description: "Output path for the contract JSON",
    getDefaultValue: () => new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), "docs", "contracts", "minimal.json")));

var contractNameOption = new Option<string>(
    aliases: ["--name", "-n"],
    description: "Contract name",
    getDefaultValue: () => "minimal");

var contractThemeOption = new Option<FileInfo?>(
    aliases: ["--theme", "-t"],
    description: "Optional theme XAML file to extract named styles from (for design system contracts)");

var contractExtendsOption = new Option<string?>(
    aliases: ["--extends", "-e"],
    description: "Base contract to extend (for design system contracts)",
    getDefaultValue: () => "minimal");

generateContractCommand.AddOption(contractSourceOption);
generateContractCommand.AddOption(contractOutputOption);
generateContractCommand.AddOption(contractNameOption);
generateContractCommand.AddOption(contractThemeOption);
generateContractCommand.AddOption(contractExtendsOption);

generateContractCommand.SetHandler(async (sourceDir, outputFile, name, themeFile, extends) =>
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("📜 FlagstoneUI Contract Generator");
    Console.ResetColor();
    Console.WriteLine($"   Source:   {sourceDir.FullName}");
    Console.WriteLine($"   Output:   {outputFile.FullName}");
    Console.WriteLine($"   Contract: {name}");
    if (themeFile != null)
        Console.WriteLine($"   Theme:    {themeFile.FullName}");
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

        var generator = new ContractGenerator();
        string contract;

        if (themeFile != null && themeFile.Exists)
        {
            // Generate design system contract from theme
            contract = await generator.GenerateDesignSystemContractAsync(
                themeFile.FullName,
                name,
                extends);
        }
        else
        {
            // Generate minimal contract from source
            contract = await generator.GenerateMinimalContractAsync(sourceDir.FullName);
        }

        outputFile.Directory?.Create();
        await File.WriteAllTextAsync(outputFile.FullName, contract);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("✅ Contract generated successfully!");
        Console.ResetColor();
        Console.WriteLine($"   File: {outputFile.FullName}");
        Console.WriteLine($"   Size: {new FileInfo(outputFile.FullName).Length:N0} bytes");
    }
    catch (FileNotFoundException ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"❌ File not found: {ex.Message}");
        Console.ResetColor();
        Environment.Exit(1);
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"❌ Error: {ex.Message}");
        Console.ResetColor();
        Environment.Exit(1);
    }
}, contractSourceOption, contractOutputOption, contractNameOption, contractThemeOption, contractExtendsOption);

// ===== Extract Surface Command =====
var extractSurfaceCommand = new Command("extract-surface", "Extract control styling surface (for debugging)");

var surfaceControlsPathOption = new Option<DirectoryInfo>(
    aliases: ["--controls", "-c"],
    description: "Path to Controls directory",
    getDefaultValue: () => new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "src", "FlagstoneUI.Core", "Controls")));

extractSurfaceCommand.AddOption(surfaceControlsPathOption);

extractSurfaceCommand.SetHandler((controlsDir) =>
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("🔍 FlagstoneUI Control Surface Extractor");
    Console.ResetColor();
    Console.WriteLine($"   Controls: {controlsDir.FullName}");
    Console.WriteLine();

    try
    {
        if (!controlsDir.Exists)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Controls directory not found: {controlsDir.FullName}");
            Console.ResetColor();
            Environment.Exit(1);
            return;
        }

        var extractor = new ControlSurfaceExtractor();
        var surfaces = extractor.ExtractFromDirectory(controlsDir.FullName);

        Console.WriteLine();
        foreach (var (name, surface) in surfaces)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"📦 {name}");
            Console.ResetColor();
            Console.WriteLine($"   Inherits: {surface.InheritsFrom}");
            Console.WriteLine($"   Architecture: {surface.Architecture}");
            Console.WriteLine($"   Styled Properties ({surface.StyledProperties.Count}):");

            foreach (var prop in surface.StyledProperties.OrderBy(p => p.Name))
            {
                var tokenInfo = prop.RecommendedToken != null
                    ? $" → {prop.RecommendedToken}"
                    : "";
                Console.WriteLine($"      • {prop.Name} ({prop.Type}){tokenInfo}");
            }
            Console.WriteLine();
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✅ Extracted {surfaces.Count} control surfaces");
        Console.ResetColor();
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"❌ Error: {ex.Message}");
        Console.ResetColor();
        Environment.Exit(1);
    }
}, surfaceControlsPathOption);

// ===== Validate Contract Command =====
var validateContractCommand = new Command("validate-contract", "Validate a theme against a design system contract");

var vcThemeOption = new Option<FileInfo>(
    aliases: ["--theme", "-t"],
    description: "Path to theme XAML file to validate")
{ IsRequired = true };

var vcContractOption = new Option<FileInfo>(
    aliases: ["--contract", "-c"],
    description: "Path to contract JSON file")
{ IsRequired = true };

var vcContractsDir = new Option<DirectoryInfo>(
    aliases: ["--contracts-dir", "-d"],
    description: "Directory containing contract files (for resolving 'extends')",
    getDefaultValue: () => new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "docs", "contracts")));

var vcJsonOutputOption = new Option<bool>(
    aliases: ["--json", "-j"],
    description: "Output results as JSON",
    getDefaultValue: () => false);

validateContractCommand.AddOption(vcThemeOption);
validateContractCommand.AddOption(vcContractOption);
validateContractCommand.AddOption(vcContractsDir);
validateContractCommand.AddOption(vcJsonOutputOption);

validateContractCommand.SetHandler(async (themeFile, contractFile, contractsDir, jsonOutput) =>
{
    if (!jsonOutput)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("🔍 FlagstoneUI Contract Validator");
        Console.ResetColor();
        Console.WriteLine($"   Theme:    {themeFile.FullName}");
        Console.WriteLine($"   Contract: {contractFile.FullName}");
        Console.WriteLine();
    }

    try
    {
        if (!themeFile.Exists)
        {
            throw new FileNotFoundException($"Theme file not found: {themeFile.FullName}");
        }

        if (!contractFile.Exists)
        {
            throw new FileNotFoundException($"Contract file not found: {contractFile.FullName}");
        }

        var validator = new ContractValidator(contractsDir.FullName);
        var result = await validator.ValidateThemeAsync(themeFile.FullName, contractFile.FullName);

        if (jsonOutput)
        {
            Console.WriteLine(result.ToJsonString());
        }
        else
        {
            Console.WriteLine(result.ToSummaryString());

            if (result.Errors.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ {result.Errors.Count} error(s):");
                Console.ResetColor();
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"   • [{error.Token}] {error.Message}");
                }
                Console.WriteLine();
            }

            if (result.Warnings.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"⚠️  {result.Warnings.Count} warning(s):");
                Console.ResetColor();
                foreach (var warning in result.Warnings)
                {
                    Console.WriteLine($"   • [{warning.Token}] {warning.Message}");
                }
                Console.WriteLine();
            }

            if (result.IsValid)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✅ Theme complies with contract!");
                Console.ResetColor();
            }
        }

        if (!result.IsValid)
        {
            Environment.Exit(1);
        }
    }
    catch (FileNotFoundException ex)
    {
        if (jsonOutput)
        {
            Console.WriteLine($"{{\"valid\": false, \"error\": \"File not found: {ex.Message}\"}}");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ File not found: {ex.Message}");
            Console.ResetColor();
        }
        Environment.Exit(1);
    }
    catch (Exception ex)
    {
        if (jsonOutput)
        {
            Console.WriteLine($"{{\"valid\": false, \"error\": \"{ex.Message.Replace("\"", "\\\"", StringComparison.Ordinal)}\"}}");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Error: {ex.Message}");
            Console.ResetColor();
        }
        Environment.Exit(1);
    }
}, vcThemeOption, vcContractOption, vcContractsDir, vcJsonOutputOption);

// Add commands to root
rootCommand.AddCommand(generateCommand);
rootCommand.AddCommand(validateCommand);
rootCommand.AddCommand(generateXamlCommand);
rootCommand.AddCommand(generateContractCommand);
rootCommand.AddCommand(extractSurfaceCommand);
rootCommand.AddCommand(validateContractCommand);

return await rootCommand.InvokeAsync(args);
