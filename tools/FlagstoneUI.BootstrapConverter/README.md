# FlagstoneUI.BootstrapConverter

A .NET class library for converting Bootstrap themes to Flagstone UI tokens and theme files.

## Overview

This library provides the core functionality for parsing Bootstrap CSS/SCSS files, extracting computed styles from CSS classes, mapping to Flagstone UI tokens, and generating XAML theme files.

## Features

- **Parse Bootstrap themes** from CSS, SCSS, URLs, or files
- **Top-down CSS analysis** - extract computed styles from Bootstrap component classes
- **Bottom-up variable mapping** - map Bootstrap variables to Flagstone UI tokens
- **Hybrid mode** - combine both approaches for maximum coverage
- **Generate XAML** token and theme files with proper ResourceDictionary structure
- **Auto-generate dark mode** color variants with **AppThemeBinding support** for light/dark adaptive themes
- **Light/dark mode CSS custom property extraction** from `[data-bs-theme="light"]` and `[data-bs-theme="dark"]` blocks (Bootstrap 5+)
- **Shadow support** - extract and map `box-shadow` properties to MAUI Shadow resources
- **Per-edge border support** - extract multi-value `border-width` properties (e.g., `1px 0 0 0`) to BorderTopWidth/RightWidth/BottomWidth/LeftWidth
- **Font detection** with download URLs and registration instructions
- **Extensible** architecture for custom mappings

## Quick Start

**Recommended for Bootstrap 5+ Themes (Bootswatch):**

```bash
# Using the CLI (variables mode - production ready)
dotnet run --project tools/FlagstoneUI.BootstrapConverter.Cli -- convert \
  --input path/to/_variables.scss path/to/_bootswatch.scss \
  --output ./output \
  --analysis-mode variables \
  --verbose
```

**Why SCSS source files?**

- ✅ Bootstrap 5+ uses CSS custom properties (`--bs-*`) in compiled CSS
- ⚠️ ExCSS 4.2.3 doesn't parse CSS custom properties
- ✅ SCSS source files contain actual values and work perfectly with `variables` mode

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

- **`BootstrapCssAnalyzer`**: Analyzes Bootstrap CSS classes (top-down approach)
  - `AnalyzeComponents()` - Extract computed styles from CSS classes
  - `ExtractStyle()` - Aggregate properties for a selector using CSS cascade
  - `ExtractShadows()` - Extract box-shadow properties from component styles
  - `ExtractThemeCustomProperties()` - Extract CSS custom properties from `[data-bs-theme="light/dark"]` blocks (regex-based workaround for ExCSS limitation)
  - Supports 25+ Bootstrap component classes (buttons, forms, cards)

- **`BootstrapMapper`**: Maps Bootstrap variables/styles to Flagstone tokens
  - `MapToFlagstoneTokens()` - Convert SCSS variables to tokens (recommended for Bootstrap 5+)
  - `MapComponentStylesToTokens()` - Convert CSS component styles to tokens
  - `MapShadowVariables()` - Extract shadow tokens from Bootstrap shadow variables
  - `ParseBoxShadow()` - Parse CSS box-shadow values (handles multi-shadow, inset filtering, rgba extraction)
  - `MapPerEdgeBorders()` - Parse multi-value border-width properties to individual edge tokens
  - Handles color, typography, spacing, border, and shadow mapping
  - Auto-generates dark mode variants
  - AppThemeBinding support via `MapCustomPropertiesToColorTokens()` for light/dark adaptive themes

- **`XamlThemeGenerator`**: Generates XAML theme files
  - `GenerateTokensXaml()` - Generate Tokens.xaml with color, typography, spacing, border, and shadow tokens
  - `GenerateThemeXaml()` - Generate Theme.xaml
  - `GenerateStylesXaml()` - Generate Styles.xaml with FsButton, FsCard, FsEntry, FsEditor styles
  - `AddShadowTokens()` - Generate MAUI Shadow resources with Offset, Radius, Brush properties
  - `CreateColorResourceReference()` - Generate AppThemeBinding syntax when dark mode values exist
  - `GenerateFilesAsync()` - Generate all theme files
  - Supports both XAML and C# output formats

### Models

- **`BootstrapVariables`**: Parsed Bootstrap variables
- **`FlagstoneTokens`**: Mapped Flagstone tokens
  - Colors, Typography, Spacing, BorderRadius, BorderWidth dictionaries
  - **Shadows** dictionary (ShadowToken with OffsetX/Y, Radius, Color, Opacity)
  - **Per-edge borders**: BorderTopWidth, BorderRightWidth, BorderBottomWidth, BorderLeftWidth
- **`ComputedStyle`**: CSS class selector + computed property values
- **`BootstrapComponentStyles`**: Container for all component styles (buttons, forms, cards)
- **`ColorToken`**: Color token with optional dark variant (DarkValue for AppThemeBinding)
- **`ShadowToken`**: Shadow token with offset, radius, color, opacity (dark mode properties ready)
- **`TypographyToken`**: Typography token (fonts, sizes)
- **`NumericToken`**: Numeric token (spacing, borders)
- **`ConversionOptions`**: Configuration for conversion

## Analysis Modes

The converter supports three analysis strategies (configured via CLI `--analysis-mode`):

### Variables Mode (Recommended for Bootstrap 5+)

**Best for:** Bootswatch themes, Bootstrap 5+ SCSS source files

```csharp
var parser = new BootstrapParser();
var variables = await parser.ParseMultipleFilesAsync(
    new[] { "_variables.scss", "_bootswatch.scss" },
    BootstrapFormat.Scss
);
var tokens = mapper.MapToFlagstoneTokens(variables, options);
```

**Advantages:**
- ✅ Works perfectly with Bootstrap 5+ (CSS custom properties irrelevant)
- ✅ Excellent token coverage (11+ colors, 3+ typography, 5+ spacing)
- ✅ Production-ready and well-tested
- ✅ Resolves variable references across multiple files

**Example Output (Bootswatch Darkly):**
- 11 color tokens (Primary, Secondary, Success, Error, Warning, Info, Background, Surface, etc.)
- 3 typography tokens (FontFamily, FontSize, LineHeight)
- 5 spacing tokens (ExtraSmall to ExtraLarge)
- 3 border radius tokens (Small, Medium, Large)
- 1 border width token
- Shadow tokens (Shadow.Button, Shadow.Small, Shadow.Default, etc.)
- Per-edge border tokens when multi-value border-width properties exist
- AppThemeBinding support when `[data-bs-theme="light/dark"]` blocks detected (Bootstrap 5+ themes)

### CSS Mode (Limited - Bootstrap 4 Only)

**Best for:** Bootstrap 4 themes with explicit CSS property values

```csharp
var analyzer = new BootstrapCssAnalyzer();
var styles = analyzer.AnalyzeComponents(cssContent);
var tokens = mapper.MapComponentStylesToTokens(styles, options);
```

**Limitations:**
- ⚠️ ExCSS 4.2.3 doesn't parse CSS custom properties (`--bs-*`)
- ⚠️ Bootstrap 5+ uses CSS custom properties exclusively
- ⚠️ Minified CSS has minimal explicit values
- ✅ Works for Bootstrap 4 with hardcoded property values

### Hybrid Mode (Fallback Strategy)

**Best for:** Maximum coverage when both CSS and SCSS are available

```csharp
// CSS analysis first
var cssTokens = mapper.MapComponentStylesToTokens(styles, options);

// Variables analysis second
var varTokens = mapper.MapToFlagstoneTokens(variables, options);

// Merge (CSS takes precedence)
var tokens = MergeTokens(cssTokens, varTokens);
```

**When to use:**
- When you have both compiled CSS and SCSS source files
- As a fallback strategy for incomplete SCSS themes
- For validation/comparison between approaches

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

### Per-Edge Borders

| Bootstrap | Flagstone | Notes |
|-----------|-----------|-------|
| `border-width: 1px 0 0 0` | `BorderTopWidth.Default: 1`<br>`BorderRightWidth.Default: 0`<br>`BorderBottomWidth.Default: 0`<br>`BorderLeftWidth.Default: 0` | Top-only border |
| `border-width: 1px 2px 3px 4px` | Individual edge tokens | All edges different |

Multi-value `border-width` properties are parsed according to CSS standard:
- 1 value: all edges
- 2 values: [top/bottom] [left/right]
- 3 values: [top] [left/right] [bottom]
- 4 values: [top] [right] [bottom] [left]

### Shadows

| Bootstrap Variable | Flagstone | Example Values |
|-------------------|-----------|----------------|
| `$box-shadow` | `Shadow.Default` | offset: 0,8 / radius: 16 |
| `$box-shadow-sm` | `Shadow.Small` | offset: 0,2 / radius: 4 |
| `$box-shadow-lg` | `Shadow.Large` | offset: 0,16 / radius: 48 |
| `$btn-box-shadow` | `Shadow.Button` | offset: 0,2 / radius: 4 |
| `$toast-box-shadow` | `Shadow.Toast.Default` | offset: 3,3 / radius: 0 |

**Shadow Properties**:
- OffsetX, OffsetY (double)
- Radius (blur radius)
- Color (hex or rgb/rgba)
- Opacity (0.0 to 1.0)
- Dark mode variants (DarkOffsetX, DarkOffsetY, etc.) - data model ready, extraction TODO

**Platform Notes**:
- ✅ Android: Full support with correct offset and blur
- ⚠️ iOS/macOS: Always applies some blur even when radius=0
- ❌ Windows: Broken rendering - ignores offset, uniform blur only (MAUI platform limitation)

## Dependencies

- **ExCSS** (4.2.3) - CSS parsing
- **System.Text.Json** (10.0.0) - JSON serialization

## Testing

See `tests/FlagstoneUI.BootstrapConverter.Tests/` for unit tests.

## License

MIT License - See LICENSE file in repository root.
