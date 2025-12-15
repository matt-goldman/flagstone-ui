# SRP Refactoring: Separation of Conversion and File I/O

## Overview

Refactored the Bootstrap converter to properly separate concerns according to the Single Responsibility Principle (SRP). The converter service now focuses solely on conversion logic, while file I/O is handled by consumers (CLI, UI, MCP).

## Problem

The original design violated SRP by mixing:
1. **Domain logic** (conversion) - Core responsibility ?
2. **File generation** (string building) - Acceptable as utility ??
3. **File I/O** (writing to disk) - Consumer responsibility ?

This made the code:
- ? Hard to test (file system mocking required)
- ? Less reusable (UI couldn't preview without writing files)
- ? More coupled (consumers forced to use file I/O even when not needed)

## Solution

### 1. **Service Responsibility** (`BootstrapConverterService`)

**Single Responsibility**: Convert Bootstrap themes to Flagstone tokens

```csharp
public async Task<ConversionResult> ConvertAsync(ConversionRequest request)
{
    // Parse Bootstrap
    // Analyze CSS/SCSS
    // Map to Flagstone tokens
    // Return in-memory result
}
```

**Returns**: `ConversionResult` containing:
- `Tokens` (in-memory)
- `ComponentStyles` (in-memory)
- `ThemeName`
- `Statistics`
- `Fonts` (if requested)

**Does NOT**: Write files, manage file paths, create directories

### 2. **Generator Responsibility** (`XamlThemeGenerator`, `CSharpThemeGenerator`)

**Single Responsibility**: Generate file content as strings

```csharp
// XAML Generator
string GenerateTokensXaml(tokens, options)
string GenerateThemeXaml(tokens, themeName, options)
string GenerateStylesXaml(tokens, themeName, componentStyles, options)
string GenerateCodeBehind(className, themeName)

// C# Generator  
string GenerateTokensCs(tokens, options)
string GenerateThemeCs(tokens, themeName, options)
string GenerateStylesCs(tokens, themeName, options)
```

**Returns**: Strings (XAML or C# code)

**Does NOT**: Write files, manage file paths

**Obsolete Methods**: `GenerateFilesAsync()` marked as obsolete (kept for backward compatibility)

### 3. **Consumer Responsibility** (CLI, UI, MCP)

**Single Responsibility**: Orchestrate conversion and handle output

```csharp
// CLI Example
var service = new BootstrapConverterService();
var result = await service.ConvertAsync(request);

var generator = new XamlThemeGenerator();
var tokensXaml = generator.GenerateTokensXaml(result.Tokens, options);
var themeXaml = generator.GenerateThemeXaml(result.Tokens, result.ThemeName, options);

await File.WriteAllTextAsync(Path.Combine(output, "Tokens.xaml"), tokensXaml);
await File.WriteAllTextAsync(Path.Combine(output, "Theme.xaml"), themeXaml);
```

**Responsibility**: 
- Call service
- Choose generator (XAML or C#)
- Decide what to do with strings (save, preview, transform, return)

## Benefits

### 1. **Separation of Concerns**

| Component | Responsibility |
|-----------|---------------|
| `BootstrapConverterService` | Bootstrap ? Flagstone conversion |
| `XamlThemeGenerator` | Tokens ? XAML strings |
| `CSharpThemeGenerator` | Tokens ? C# strings |
| CLI | Orchestration + file writing |
| UI | Orchestration + preview/save |
| MCP | Orchestration + JSON response |

### 2. **CLI Usage** (Unchanged)

```sh
bootstrap-converter convert -i theme.scss -o ./MyTheme
```

Internally:
1. Parse options
2. Call `service.ConvertAsync()`
3. Call `generator.Generate*()` methods
4. Write files to disk

**User experience**: No change! Same commands, same output.

### 3. **UI Usage** (Now Possible)

```csharp
// Preview without saving
var result = await service.ConvertAsync(request);
var generator = new XamlThemeGenerator();
var tokensXaml = generator.GenerateTokensXaml(result.Tokens, options);

// Load into ResourceDictionary for live preview
var dict = new ResourceDictionary();
dict.LoadFromXaml(tokensXaml);
Application.Current.Resources.MergedDictionaries.Add(dict);

// User clicks "Save"
if (userClickedSave)
{
    await File.WriteAllTextAsync(savePath, tokensXaml);
}
```

### 4. **MCP Usage** (Now Possible)

```csharp
// MCP server endpoint
public async Task<JsonResponse> ConvertTheme(McpRequest request)
{
    var service = new BootstrapConverterService();
    var result = await service.ConvertAsync(conversionRequest);
    
    var generator = new XamlThemeGenerator();
    var tokensXaml = generator.GenerateTokensXaml(result.Tokens, options);
    
    // Return as JSON (no file I/O)
    return new JsonResponse
    {
        Tokens = tokensXaml,
        Theme = generator.GenerateThemeXaml(...),
        Statistics = result.Statistics
    };
}
```

### 5. **Testing** (Much Easier)

**Before** (with file I/O):
```csharp
[Fact]
public async Task TestConversion()
{
    var tempDir = CreateTempDirectory();
    try
    {
        await service.ConvertAndGenerateFilesAsync(request, tempDir);
        var files = Directory.GetFiles(tempDir);
        // Assert on file system state
    }
    finally
    {
        Directory.Delete(tempDir, recursive: true);
    }
}
```

**After** (in-memory):
```csharp
[Fact]
public async Task TestConversion()
{
    var result = await service.ConvertAsync(request);
    
    result.Tokens.Colors.Count.ShouldBeGreaterThan(0);
    result.Statistics.ColorTokens.ShouldBe(expected);
    // No file system, no cleanup
}
```

## Migration Path

### For Current Users (None!)

? **No breaking changes to CLI**  
? **Same commands work exactly as before**  
? **Same output structure**

### For Library Consumers

**Old way** (still works, but obsolete):
```csharp
var generator = new XamlThemeGenerator();
await generator.GenerateFilesAsync(tokens, themeName, outputDir, options);
```

**New way** (recommended):
```csharp
var generator = new XamlThemeGenerator();
var tokensXaml = generator.GenerateTokensXaml(tokens, options);
var themeXaml = generator.GenerateThemeXaml(tokens, themeName, options);

await File.WriteAllTextAsync(Path.Combine(outputDir, "Tokens.xaml"), tokensXaml);
await File.WriteAllTextAsync(Path.Combine(outputDir, "Theme.xaml"), themeXaml);
```

**Why?** 
- More flexible (can preview, transform, or save)
- Testable without file system
- Follows SRP

## Implementation Details

### Changes Made

1. **Removed** `ConvertAndGenerateFilesAsync` from `BootstrapConverterService`
2. **Moved** nested types (`ConversionRequest`, `ConversionResult`, `AnalysisStrategy`, `ConversionStatistics`) to namespace level
3. **Marked** `GenerateFilesAsync` in generators as `[Obsolete]` (not removed - backward compatibility)
4. **Updated** CLI to handle file I/O directly
5. **Added** `SanitizeThemeName` helper to CLI (needed for file naming)

### Files Modified

**Service**:
- `tools/FlagstoneUI.BootstrapConverter/BootstrapConverterService.cs`

**CLI**:
- `tools/FlagstoneUI.BootstrapConverter.Cli/Commands/ConvertCommand.cs`

**Generators** (marked methods obsolete):
- `tools/FlagstoneUI.BootstrapConverter/XamlThemeGenerator.cs`
- `tools/FlagstoneUI.BootstrapConverter/CSharpThemeGenerator.cs`

### API Surface

**Public API** (what consumers use):

```csharp
// Service
public class BootstrapConverterService
{
    public async Task<ConversionResult> ConvertAsync(ConversionRequest request);
}

// Request/Result
public record ConversionRequest { ... }
public record ConversionResult { ... }
public record ConversionStatistics { ... }
public enum AnalysisStrategy { CssOnly, VariablesOnly, Hybrid }

// XAML Generator
public class XamlThemeGenerator
{
    public string GenerateTokensXaml(FlagstoneTokens, ConversionOptions?);
    public string GenerateThemeXaml(FlagstoneTokens, string themeName, ConversionOptions?);
    public string GenerateStylesXaml(FlagstoneTokens, string themeName, BootstrapComponentStyles?, ConversionOptions?);
    public string GenerateCodeBehind(string className, string themeName);
    
    [Obsolete] public async Task GenerateFilesAsync(...); // Backward compat
}

// C# Generator
public class CSharpThemeGenerator
{
    public string GenerateTokensCs(FlagstoneTokens, ConversionOptions?);
    public string GenerateThemeCs(FlagstoneTokens, string themeName, ConversionOptions?);
    public string GenerateStylesCs(FlagstoneTokens, string themeName, ConversionOptions?);
    
    [Obsolete] public async Task GenerateFilesAsync(...); // Backward compat
}
```

## Future Enhancements

Now that conversion and I/O are separated, we can easily add:

1. **Streaming**: Stream large themes without loading entirely in memory
2. **Compression**: Zip generated files before returning
3. **Transformation**: Convert to other formats (JSON, TOML, etc.)
4. **Validation**: Validate generated strings before saving
5. **Caching**: Cache conversion results without file system

## Summary

? **SRP Compliance**: Each class has one reason to change  
? **Testability**: No file system dependencies in core logic  
? **Flexibility**: Consumers choose what to do with results  
? **Reusability**: Service works for CLI, UI, MCP, and future consumers  
? **Backward Compatibility**: Obsolete methods preserved  
? **No Breaking Changes**: CLI works exactly as before  

The refactoring properly separates **what** (conversion) from **how** (storage), following SOLID principles and making the codebase more maintainable and extensible.
