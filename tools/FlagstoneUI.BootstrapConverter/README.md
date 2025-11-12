# FlagstoneUI.BootstrapConverter

A .NET class library for converting Bootstrap themes to Flagstone UI tokens and theme files.

## Overview

This library provides the core functionality for parsing Bootstrap CSS/SCSS files, mapping variables to Flagstone UI tokens, and generating XAML theme files.

## Features

- **Parse Bootstrap themes** from CSS, SCSS, URLs, or files
- **Map Bootstrap variables** to Flagstone UI token system
- **Generate XAML** token and theme files
- **Auto-generate dark mode** color variants
- **Extensible** architecture for custom mappings

## Installation

```bash
# From repository root
dotnet build tools/FlagstoneUI.BootstrapConverter
```

## Usage

### Basic Conversion

```csharp
using FlagstoneUI.BootstrapConverter;
using FlagstoneUI.BootstrapConverter.Models;

// Parse Bootstrap theme
var parser = new BootstrapParser();
var variables = await parser.ParseFromUrlAsync(
    "https://bootswatch.com/5/darkly/bootstrap.css"
);

// Map to Flagstone tokens
var mapper = new BootstrapMapper();
var tokens = mapper.MapToFlagstoneTokens(variables, new ConversionOptions 
{
    DarkModeStrategy = DarkModeStrategy.Auto,
    IncludeComments = true
});

// Generate XAML files
var generator = new XamlThemeGenerator();
await generator.GenerateFilesAsync(
    tokens, 
    "Bootstrap Darkly", 
    "./output"
);
```

### Parse from File

```csharp
var parser = new BootstrapParser();
var variables = await parser.ParseFromFileAsync(
    "./custom-bootstrap.scss",
    BootstrapFormat.Scss
);
```

### Parse Multiple Files (Bootswatch Themes)

For best results with Bootswatch themes, use multiple source files to get complete variable resolution:

```csharp
var parser = new BootstrapParser();
var variables = await parser.ParseMultipleFilesAsync(
    new[] { 
        "./bootswatch/_variables.scss",  // Theme-specific variables
        "./bootswatch/_bootswatch.scss"  // Theme overrides and fonts
    },
    BootstrapFormat.Scss
);
```

**Why multiple files?**

- `_variables.scss` contains the actual color values and theme definitions
- `_bootswatch.scss` contains font imports, custom styles, and variable overrides
- Multi-file parsing resolves variable references (e.g., `$success: $green` → `#56cc9d`)
- Better extraction of typography tokens (font families, font imports)

**Variable Resolution:**
The parser automatically resolves variable references across files:

- SCSS variables: `$primary: $green` → resolves `$green` to actual color
- CSS custom properties: `var(--bs-primary)` → resolves to value
- Recursive resolution: `$a: $b; $b: $c; $c: #fff` → all resolve to `#fff`

### Custom Conversion Options

```csharp
var options = new ConversionOptions
{
    DarkModeStrategy = DarkModeStrategy.Manual, // Don't auto-generate dark colors
    IncludeComments = false,                     // Skip comments in XAML
    Namespace = "MyApp.Themes.Bootstrap"         // Custom namespace
};

var tokens = mapper.MapToFlagstoneTokens(variables, options);
```

### Generate Tokens Only

```csharp
var generator = new XamlThemeGenerator();
var tokensXaml = generator.GenerateTokensXaml(tokens);
await File.WriteAllTextAsync("Tokens.xaml", tokensXaml);
```

## Architecture

### Core Classes

- **`BootstrapParser`**: Parses Bootstrap CSS/SCSS files
  - `ParseCss()` - Parse CSS custom properties
  - `ParseScss()` - Parse SCSS variables
  - `ParseFromUrlAsync()` - Fetch and parse from URL
  - `ParseFromFileAsync()` - Parse from local file
  - `ParseMultipleFilesAsync()` - Parse and merge multiple files with variable resolution

- **`BootstrapMapper`**: Maps Bootstrap variables to Flagstone tokens
  - `MapToFlagstoneTokens()` - Convert variables to tokens
  - Handles color, typography, spacing, and border mapping
  - Auto-generates dark mode variants

- **`XamlThemeGenerator`**: Generates XAML theme files
  - `GenerateTokensXaml()` - Generate Tokens.xaml
  - `GenerateThemeXaml()` - Generate Theme.xaml
  - `GenerateFilesAsync()` - Generate both files

### Models

- **`BootstrapVariables`**: Parsed Bootstrap variables
- **`FlagstoneTokens`**: Mapped Flagstone tokens
- **`ColorToken`**: Color token with optional dark variant
- **`TypographyToken`**: Typography token (fonts, sizes)
- **`NumericToken`**: Numeric token (spacing, borders)
- **`ConversionOptions`**: Configuration for conversion

## Bootstrap → Flagstone Mappings

### Colors

| Bootstrap | Flagstone | Notes |
|-----------|-----------|-------|
| `--bs-primary` | `Color.Primary` | Primary brand color |
| `--bs-secondary` | `Color.Secondary` | Secondary color |
| `--bs-success` | `Color.Success` | Success state |
| `--bs-danger` | `Color.Error` | Error state |
| `--bs-warning` | `Color.Warning` | Warning state |
| `--bs-info` | `Color.Info` | Info state |
| `--bs-light` | `Color.Surface` | Light surface |
| `--bs-dark` | `Color.SurfaceVariant.Dark` | Dark surface |

### Typography

| Bootstrap | Flagstone | Notes |
|-----------|-----------|-------|
| `--bs-font-family-base` | `FontFamily.Default` | Primary font |
| `--bs-headings-font-family` | `FontFamily.Default` | Fallback for Bootswatch themes |
| `--bs-font-size-base` | `FontSize.Body` | |
| `--bs-line-height-base` | `LineHeight.Default` | |

### Spacing

| Bootstrap | Flagstone |
|-----------|-----------|
| `--bs-spacer * 0.25` | `Spacing.ExtraSmall` |
| `--bs-spacer * 0.5` | `Spacing.Small` |
| `--bs-spacer * 1` | `Spacing.Medium` |
| `--bs-spacer * 1.5` | `Spacing.Large` |
| `--bs-spacer * 3` | `Spacing.ExtraLarge` |

### Borders

| Bootstrap | Flagstone | Notes |
|-----------|-----------|-------|
| `--bs-btn-border-radius-sm` | `Radius.Small` | Preferred for buttons |
| `--bs-border-radius-sm` | `Radius.Small` | Fallback |
| `--bs-btn-border-radius` | `Radius.Medium` | Preferred for buttons |
| `--bs-border-radius` | `Radius.Medium` | Fallback |
| `--bs-btn-border-radius-lg` | `Radius.Large` | Preferred for buttons |
| `--bs-border-radius-lg` | `Radius.Large` | Fallback |
| `--bs-border-width` | `BorderWidth.Default` | |

> **Note**: Button-specific radius values (`btn-border-radius-*`) are preferred over generic values to ensure buttons match the theme's intended appearance. For example, the Litera theme uses fully-rounded pill-shaped buttons.

## Dependencies

- **ExCSS** (4.2.3) - CSS parsing
- **System.Text.Json** (10.0.0) - JSON serialization

## Testing

See `tests/FlagstoneUI.BootstrapConverter.Tests/` for unit tests.

## License

MIT License - See LICENSE file in repository root.
