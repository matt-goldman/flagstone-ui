# ADR005: Bootstrap Converter Analysis Modes

**Status**: Accepted  
**Date**: December 12, 2025  
**Deciders**: Matt Goldman  
**Context**: Bootstrap theme conversion architecture for Flagstone UI

## Context and Problem Statement

When converting Bootstrap themes to Flagstone UI, we need to extract design tokens (colors, spacing, typography, borders) from Bootstrap's theme system. Bootstrap's architecture has evolved significantly between versions 4 and 5, with different approaches to theming:

- **Bootstrap 4**: Used SCSS variables (`$primary`, `$font-size-base`) and compiled to CSS with explicit property values
- **Bootstrap 5+**: Uses CSS custom properties (`--bs-primary`, `--bs-btn-bg`) for runtime theming
- **Bootswatch Themes**: Provide SCSS source files (`_variables.scss`, `_bootswatch.scss`) with actual color/spacing values

The challenge is determining the most reliable way to extract these tokens across different Bootstrap versions and distribution formats (compiled CSS vs SCSS source).

## Decision Drivers

1. **Accuracy**: Extract the correct token values that match the theme's visual appearance
2. **Reliability**: Work consistently across Bootstrap versions and Bootswatch themes
3. **Coverage**: Extract comprehensive tokens (colors, typography, spacing, borders)
4. **Library Limitations**: ExCSS 4.2.3 (current CSS parser) doesn't parse CSS custom properties (`--bs-*`)
5. **User Experience**: Provide clear guidance on which approach to use for best results

## Considered Options

### Option 1: Bottom-Up Variable Mapping (Variables Mode)

**Approach**: Parse SCSS variables from source files and map to Flagstone tokens.

**Pros**:
- ✅ Direct access to actual color/spacing values
- ✅ Works perfectly with Bootswatch SCSS themes
- ✅ No library limitations (SCSS parsing is straightforward)
- ✅ Excellent token coverage (tested: 11 colors, 3 typography, 5 spacing, 3 radius, 1 width)
- ✅ Variable resolution across multiple files (`$success: $green` → actual color)

**Cons**:
- ⚠️ Requires SCSS source files (not available for all themes)
- ⚠️ Variables are implementation details, not Bootstrap's public API

**Validation Results** (Bootswatch Darkly):
```
Parsed: 11 colors, 3 typography, 1 spacing, 4 borders
Mapped: 11 color tokens, 3 typography, 5 spacing, 3 border radius, 1 border width
Generated: Complete XAML theme with FsButton styles
```

### Option 2: Top-Down CSS Class Analysis (CSS Mode)

**Approach**: Extract computed styles from Bootstrap CSS classes (`.btn-primary`, `.card`) and map to tokens.

**Pros**:
- ✅ Analyzes Bootstrap's public API (CSS classes are the contract)
- ✅ More aligned with how Bootstrap actually works
- ✅ Could work with any Bootstrap-compatible CSS

**Cons**:
- ❌ ExCSS 4.2.3 doesn't parse CSS custom properties
- ❌ Bootstrap 5+ uses `--bs-*` custom properties exclusively
- ❌ Compiled CSS has minimal explicit values (relies on CSS variables)
- ❌ Testing showed 0 declarations extracted from `.btn-primary` in Bootstrap 5

**Technical Issue**:
```csharp
// Bootstrap 5 CSS contains:
.btn-primary {
  color: var(--bs-btn-color);
  background-color: var(--bs-btn-bg);
  border-color: var(--bs-btn-border-color);
}

// ExCSS 4.2.3 result:
rule.Style.Length == 0  // Custom properties ignored
```

### Option 3: Hybrid Mode (Fallback Strategy)

**Approach**: Combine CSS analysis with variable parsing, CSS tokens take precedence.

**Pros**:
- ✅ Maximum coverage when both sources available
- ✅ CSS analysis as primary, variables as fallback
- ✅ Validation/comparison between approaches

**Cons**:
- ⚠️ Added complexity
- ⚠️ CSS mode still limited by ExCSS for Bootstrap 5+
- ⚠️ Minimal benefit over variables-only for current use cases

## Decision Outcome

**Chosen option**: **Multi-mode architecture with Variables Mode as recommended default**

### Implementation

Implemented three analysis modes via CLI `--analysis-mode` option:

1. **`variables`** (RECOMMENDED for Bootstrap 5+)
   - Uses `BootstrapParser` to parse SCSS variables
   - Maps via `MapToFlagstoneTokens()`
   - Production-ready, excellent coverage

2. **`css`** (Limited - Bootstrap 4 only)
   - Uses `BootstrapCssAnalyzer` to extract component styles
   - Maps via `MapComponentStylesToTokens()`
   - Limited by ExCSS CSS custom property support

3. **`hybrid`** (Fallback strategy)
   - Runs both analyses, merges results (CSS precedence)
   - Uses `MergeTokens()` helper
   - For maximum coverage scenarios

### Code Architecture

**`BootstrapParser`**:
```csharp
// SCSS variable parsing
public async Task<BootstrapVariables> ParseMultipleFilesAsync(
    string[] paths, 
    BootstrapFormat format)
{
    // Resolves $success: $green → #56cc9d
}
```

**`BootstrapCssAnalyzer`**:
```csharp
// CSS class analysis (limited by ExCSS)
public BootstrapComponentStyles AnalyzeComponents(string cssContent)
{
    // Extracts .btn-primary, .card styles
    // NOTE: CSS custom properties return 0 declarations in ExCSS 4.2.3
}
```

**`BootstrapMapper`**:
```csharp
// Variables → tokens (production ready)
public FlagstoneTokens MapToFlagstoneTokens(
    BootstrapVariables variables, 
    ConversionOptions options)

// CSS styles → tokens (limited)
public FlagstoneTokens MapComponentStylesToTokens(
    BootstrapComponentStyles styles, 
    ConversionOptions options)
```

### User Guidance

**README.md recommendations**:
- ✅ Use `--analysis-mode variables` for Bootstrap 5+ and Bootswatch themes
- ✅ Provide SCSS source files (`_variables.scss`, `_bootswatch.scss`)
- ⚠️ Document ExCSS limitation with CSS custom properties
- ⚠️ CSS mode only works for Bootstrap 4 with explicit property values

## Consequences

### Positive

1. **Production-Ready Path**: Variables mode works excellently with Bootswatch themes
2. **Clear Guidance**: Users know to use SCSS source files for best results
3. **Future-Proof**: Top-down architecture exists if/when we upgrade ExCSS
4. **Flexible**: Multiple modes allow experimentation and validation

### Negative

1. **ExCSS Limitation**: CSS mode not practical for Bootstrap 5+ without library upgrade
2. **Source File Dependency**: Best results require SCSS sources, not just compiled CSS
3. **Complexity**: Three modes add cognitive load (mitigated by clear documentation)

### Future Improvements

**Short-term**:
- Document CSS mode as "Bootstrap 4 only" in tooltips/help text
- Consider defaulting to `variables` mode instead of `hybrid`

**Long-term options**:
- Upgrade to ExCSS 5.x (if CSS custom property support added)
- Implement custom CSS variable parser
- Extract CSS custom property values via browser automation (Playwright/Selenium)
- Use CSS preprocessor APIs (Sass.js) to compile SCSS to CSS with resolved values

## Validation Evidence

**Successful conversion with Variables Mode** (Bootswatch Darkly SCSS):

```bash
dotnet run -- convert \
  --input tests/FlagstoneUI.BootstrapConverter.Tests/Fixtures/bootswatch-darkly.scss \
  --output ./test-output-darkly \
  --analysis-mode variables \
  --verbose
```

**Results**:
- ✅ 11 color tokens (Primary, Secondary, Success, Error, Warning, Info, Background, Surface, etc.)
- ✅ 3 typography tokens (FontFamily.Default, FontSize.Body, LineHeight.Default)
- ✅ 5 spacing tokens (ExtraSmall → ExtraLarge)
- ✅ 3 border radius tokens (Small, Medium, Large)
- ✅ 1 border width token
- ✅ Per-edge border tokens (BorderTopWidth, BorderRightWidth, BorderBottomWidth, BorderLeftWidth)
- ✅ Shadow tokens (Shadow.Button, Shadow.Small, Shadow.Default with OffsetX/Y, Radius, Color, Opacity)
- ✅ AppThemeBinding support for light/dark mode (Color.Background + Color.Background.Dark)
- ✅ Generated complete XAML theme with FsButton styles (Default, OutlinedButton, TextButton)
- ✅ Proper `DynamicResource` bindings to tokens
- ✅ VisualStateGroups for Normal/Disabled states

**Failed conversion with CSS Mode** (Bootstrap 5 CDN):

```bash
# Fetched Bootstrap 5.3.x from CDN
# Result: 0 color tokens extracted
# Cause: .btn-primary has color: var(--bs-btn-color) - ExCSS returns 0 declarations
```

## Addendum: AppThemeBinding Implementation (December 2025)

### Challenge: Bootstrap 5+ Adaptive Themes

Bootstrap 5+ themes use CSS custom properties in `[data-bs-theme="light"]` and `[data-bs-theme="dark"]` blocks for adaptive theming:

```css
[data-bs-theme=light] {
  --bs-body-bg: #ffffff;
  --bs-body-color: #212529;
  --bs-border-color: #dee2e6;
}

[data-bs-theme=dark] {
  --bs-body-bg: #212529;
  --bs-body-color: #dee2e6;
  --bs-border-color: #000000;
}
```

**ExCSS 4.2.3 Limitation**: Parser returns 0 declarations for rules with CSS custom properties.

### Solution: Regex-Based CSS Custom Property Parser

Implemented manual parsing in `BootstrapCssAnalyzer.ExtractThemeCustomProperties()`:

```csharp
// Manual parsing since ExCSS doesn't support CSS custom properties
var lightPattern = @"(?:^|\n)\s*(?::root|(?:\:root,)?\[data-bs-theme\s*=\s*['""]?light['""]?\])[^{]*\{([^}]*)\}";
var darkPattern = @"(?:^|\n)\s*\[data-bs-theme\s*=\s*['""]?dark['""]?\][^{]*\{([^}]*)\}";

// Extract and parse --property-name: value; declarations
```

**Results** (Brite theme):
- ✅ Extracted 127 light mode CSS custom properties
- ✅ Extracted 67 dark mode CSS custom properties
- ✅ Mapped to ColorToken.DarkValue
- ✅ Generated `.Dark` suffix tokens (e.g., Color.Background.Dark)
- ✅ Styles use AppThemeBinding syntax: `{AppThemeBinding Light={DynamicResource Color.X}, Dark={DynamicResource Color.X.Dark}}`

### Implementation Details

1. **Data Model**: `ColorToken.DarkValue` property stores dark mode color value
2. **Extraction**: Regex-based parser in `BootstrapCssAnalyzer`
3. **Mapping**: `BootstrapConverterService.MapCustomPropertiesToColorTokens()` maps common Bootstrap variables
4. **Token Generation**: `XamlThemeGenerator.AddColorTokens()` generates both base and `.Dark` tokens
5. **Style Generation**: `CreateColorResourceReference()` helper generates AppThemeBinding when DarkValue exists

## Related Decisions

- ADR004: Cross-Assembly Resource Loading (establishes XAML generation patterns)
- Future: May need ADR for ExCSS upgrade or CSS variable parser implementation

## References

- Bootstrap 5 Theming: https://getbootstrap.com/docs/5.3/customize/css-variables/
- Bootswatch Themes: https://bootswatch.com/
- ExCSS Library: https://github.com/TylerBrinks/ExCSS
- Implementation: `tools/FlagstoneUI.BootstrapConverter/`
- CLI: `tools/FlagstoneUI.BootstrapConverter.Cli/`
- Tests: `tests/FlagstoneUI.BootstrapConverter.Tests/`
