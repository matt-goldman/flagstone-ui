# Bootstrap Converter Refactoring

## Overview

The conversion logic has been refactored from the CLI layer into a shared service class (`BootstrapConverterService`) in the class library. This makes the core conversion workflow reusable by both the CLI and the upcoming UI app.

## Changes Made

### New Class: `BootstrapConverterService`

**Location**: `tools/FlagstoneUI.BootstrapConverter/BootstrapConverterService.cs`

This service orchestrates the complete conversion workflow:

1. **Input Reading**: Reads files from disk or URLs
2. **Analysis Strategy**: Supports CSS-only, Variables-only, or Hybrid analysis
3. **Token Extraction**: Uses `BootstrapParser`, `BootstrapCssAnalyzer`, and `BootstrapMapper`
4. **Token Merging**: Merges tokens from multiple analysis strategies (CSS takes precedence)
5. **Statistics**: Returns detailed statistics about the conversion

**Key Methods**:

- `ConvertAsync(ConversionRequest)` - Execute conversion and return tokens + statistics
- `ConvertAndGenerateFilesAsync(ConversionRequest, string)` - Convert and generate resource dictionary files (XAML or C#)

**Key Types**:

- `ConversionRequest` - Input configuration (files, format, strategy, options)
- `ConversionResult` - Output result (tokens, component styles, statistics)
- `ConversionStatistics` - Token counts, variables parsed, component styles extracted
- `AnalysisStrategy` - Enum for CssOnly, VariablesOnly, Hybrid

### New Class: `CSharpThemeGenerator`

**Location**: `tools/FlagstoneUI.BootstrapConverter/CSharpThemeGenerator.cs`

Generates C# ResourceDictionary classes as an alternative to XAML:

- `GenerateTokensCs()` - Generate Tokens.cs with all token definitions
- `GenerateThemeCs()` - Generate Theme.cs that merges tokens
- `GenerateStylesCs()` - Generate Styles.cs (placeholder for styles)
- `GenerateFilesAsync()` - Generate all three files

**Benefits of C# ResourceDictionaries**:
- ? Strongly-typed access to resources
- ? No XAML compilation overhead
- ? Easier to modify programmatically
- ? Better for generated code scenarios

### Updated: `ConversionOptions`

**Location**: `tools/FlagstoneUI.BootstrapConverter/Models/ConversionOptions.cs`

Added `OutputFormat` property with `ResourceDictionaryFormat` enum:

```csharp
public enum ResourceDictionaryFormat
{
    Xaml,    // Generate XAML resource dictionaries (default)
    CSharp   // Generate C# resource dictionaries
}
```

### Updated: `ConvertCommand`

**Location**: `tools/FlagstoneUI.BootstrapConverter.Cli/Commands/ConvertCommand.cs`

The CLI command now:

1. **Parses CLI options** (inputs, format, dark mode, analysis mode, **output format**, etc.)
2. **Creates ConversionRequest** from parsed options
3. **Calls BootstrapConverterService** to execute conversion
4. **Chooses generator** based on output format (XAML or C#)
5. **Displays progress and statistics** (CLI-specific concern)
6. **Generates files** using appropriate generator

**New Option**:
- `--output-format` - Choose between `xaml` (default) or `csharp` for generated resource dictionaries

**Responsibilities**:
- CLI option parsing
- Console output (progress, colors, formatting)
- Error handling with user-friendly messages

### Token Merge Logic

The `MergeTokens` method has been moved from `ConvertCommand` to `BootstrapConverterService` as a private static method. This ensures the merge logic is available to all consumers of the service.

## Benefits

### 1. **Reusability**

The UI app can now use the same conversion logic:

```csharp
var service = new BootstrapConverterService();
var request = new ConversionRequest
{
    Inputs = ["path/to/theme.scss"],
    Format = BootstrapFormat.Scss,
    Strategy = BootstrapConverterService.AnalysisStrategy.VariablesOnly,
    Options = new ConversionOptions
    {
        DarkModeStrategy = DarkModeStrategy.Auto,
        IncludeComments = true,
        Namespace = "MyApp.Themes",
        OutputFormat = ResourceDictionaryFormat.CSharp  // or Xaml
    }
};

var result = await service.ConvertAsync(request);
// Use result.Tokens, result.Statistics, etc.
```

### 2. **Output Format Flexibility**

Users can now choose between XAML and C# resource dictionaries:

```bash
# Generate XAML (default)
bootstrap-converter convert -i theme.scss -o ./output

# Generate C# resource dictionaries
bootstrap-converter convert -i theme.scss -o ./output --output-format csharp
```

**XAML Output**:
- `Tokens.xaml` + `Tokens.xaml.cs`
- `Theme.xaml` + `Theme.xaml.cs`
- `Styles.xaml` + `Styles.xaml.cs` (with full control styles)

**C# Output**:
- `Tokens.cs` - All token definitions in code
- `Theme.cs` - Theme that merges tokens
- `Styles.cs` - Placeholder for programmatic styles

### 3. **Separation of Concerns**

- **Class Library**: Core conversion logic, reusable across platforms
- **CLI**: User interaction, progress output, command-line parsing
- **UI (future)**: Visual interaction, file pickers, live preview

### 4. **Testability**

The service can be unit tested independently:

```csharp
[Fact]
public async Task ConvertAsync_WithVariablesStrategy_ExtractsTokens()
{
    var service = new BootstrapConverterService();
    var request = new ConversionRequest
    {
        Inputs = ["test-theme.scss"],
        Strategy = AnalysisStrategy.VariablesOnly
    };
    
    var result = await service.ConvertAsync(request);
    
    Assert.True(result.Statistics.ColorTokens > 0);
    Assert.True(result.Statistics.TypographyTokens > 0);
}
```

### 5. **Consistency**

Both CLI and UI will use the exact same conversion logic, ensuring consistent results regardless of the interface used.

### 6. **Statistics & Reporting**

The service returns structured statistics that can be displayed differently in CLI (console output) vs. UI (dialog, progress bar, etc.).

## Migration Guide for UI App

When implementing the UI app, follow this pattern:

1. **Construct ConversionRequest** from user input (file pickers, dropdowns, checkboxes)
2. **Call ConvertAsync()** to execute conversion
3. **Display Statistics** in a dialog or status panel
4. **Show Tokens** in a preview panel (optional)
5. **Generate Files** or allow user to save tokens directly

Example:

```csharp
// In UI ViewModel
public async Task ConvertThemeAsync()
{
    var request = new ConversionRequest
    {
        Inputs = SelectedFiles.ToArray(),
        Format = SelectedFormat,
        Strategy = SelectedStrategy,
        EnableDebugLogging = DebugMode,
        Options = new ConversionOptions
        {
            DarkModeStrategy = SelectedDarkMode,
            IncludeComments = IncludeComments,
            Namespace = ThemeNamespace,
            OutputFormat = SelectedOutputFormat  // XAML or C#
        }
    };

    var service = new BootstrapConverterService();
    var result = await service.ConvertAsync(request);

    // Update UI
    ColorTokenCount = result.Statistics.ColorTokens;
    TypographyTokenCount = result.Statistics.TypographyTokens;
    PreviewTokens = result.Tokens;
}
```

## CLI Usage Examples

### Generate XAML Resource Dictionaries (Default)

```bash
bootstrap-converter convert \
  --input path/to/theme.scss \
  --output ./MyTheme

# Output:
# - Tokens.xaml + Tokens.xaml.cs
# - Theme.xaml + Theme.xaml.cs
# - Styles.xaml + Styles.xaml.cs
```

### Generate C# Resource Dictionaries

```bash
bootstrap-converter convert \
  --input path/to/theme.scss \
  --output ./MyTheme \
  --output-format csharp

# Output:
# - Tokens.cs
# - Theme.cs
# - Styles.cs
```

### When to Use Each Format

**Use XAML when**:
- You want full control styles with visual states (included)
- You prefer declarative UI
- You're following traditional MAUI patterns
- You want hot reload support (XAML Hot Reload)

**Use C# when**:
- You prefer strongly-typed access to resources
- You're generating themes programmatically
- You want to avoid XAML compilation overhead
- You need to modify resources at runtime

## Backward Compatibility

No breaking changes to the CLI interface. All existing commands and options work exactly as before. The `--output-format` option is optional and defaults to XAML.

## Future Enhancements

Possible additions:

1. **Progress Reporting**: `IProgress<ConversionProgress>` parameter
2. **Cancellation**: `CancellationToken` parameter
3. **Validation**: Pre-conversion validation of inputs
4. **Caching**: Cache parsed variables/styles for repeated conversions
5. **Custom Mappings**: Allow users to override default Bootstrap ? Flagstone mappings
6. **Hybrid ResourceDictionaries**: Generate both XAML (for styles) and C# (for tokens)

## Related Files

- `tools/FlagstoneUI.BootstrapConverter/BootstrapConverterService.cs` - New service class
- `tools/FlagstoneUI.BootstrapConverter/CSharpThemeGenerator.cs` - New C# generator
- `tools/FlagstoneUI.BootstrapConverter/Models/ConversionOptions.cs` - Updated with OutputFormat
- `tools/FlagstoneUI.BootstrapConverter.Cli/Commands/ConvertCommand.cs` - Updated CLI command
- `tools/FlagstoneUI.BootstrapConverter/BootstrapParser.cs` - Variable parsing (unchanged)
- `tools/FlagstoneUI.BootstrapConverter/BootstrapCssAnalyzer.cs` - CSS analysis (unchanged)
- `tools/FlagstoneUI.BootstrapConverter/BootstrapMapper.cs` - Token mapping (unchanged)
- `tools/FlagstoneUI.BootstrapConverter/XamlThemeGenerator.cs` - XAML generation (unchanged)
