# Bootstrap Converter - Phase 1 Summary

**Date**: December 12, 2025  
**Status**: Phase 1 Complete  
**Next Phase**: UI App Development (Q1 2026)

## Executive Summary

Successfully implemented a production-ready Bootstrap to Flagstone UI theme converter. The tool converts Bootstrap SCSS themes (particularly Bootswatch themes) into Flagstone UI XAML resource dictionaries with excellent token coverage and automated button style generation.

**Key Achievement**: Variables mode extracts 20+ tokens from Bootswatch themes and generates complete, ready-to-use Flagstone UI theme files.

## Architecture Evolution

### Initial Approach (Abandoned)

- **Vision**: Bottom-up variable mapping (SCSS variables → Flagstone tokens)
- **Problem**: Bootstrap themes have inconsistent variable definitions
- **Lesson**: Variables are implementation details, not Bootstrap's public API

### Pivot: Top-Down Analysis

- **Insight**: Bootstrap's public API is CSS classes (`.btn-primary`), not variables
- **Implementation**: Built `BootstrapCssAnalyzer` to extract computed styles from CSS classes
- **Discovery**: ExCSS 4.2.3 doesn't parse CSS custom properties (`--bs-*`)
- **Impact**: CSS analysis mode doesn't work for Bootstrap 5+ compiled CSS

### Production Solution: Variables Mode

- **Approach**: Parse SCSS source files with variable resolution
- **Result**: Excellent token coverage (11 colors, 3 typography, 5 spacing, 3 radius, 1 width)
- **Recommendation**: Use SCSS source files (Bootswatch provides `_variables.scss`, `_bootswatch.scss`)

## Implementation Details

### Three Analysis Modes

#### 1. Variables Mode (Recommended)

```bash
dotnet run -- convert \
  --input _variables.scss _bootswatch.scss \
  --output ./theme \
  --analysis-mode variables \
  --verbose
```

**Capabilities**:

- ✅ Parses SCSS variables (`$primary: #375a7f;`)
- ✅ Resolves variable references (`$success: $green` → `#56cc9d`)
- ✅ Multi-file parsing for complete theme coverage
- ✅ Production-ready with validated Bootswatch themes

#### 2. CSS Mode (Limited)

```bash
dotnet run -- convert \
  --input bootstrap.css \
  --output ./theme \
  --analysis-mode css
```

**Capabilities**:

- ⚠️ Extracts styles from CSS classes (`.btn-primary`, `.card`)
- ❌ ExCSS 4.2.3 limitation: CSS custom properties return 0 declarations
- ❌ Bootstrap 5+ uses `--bs-*` custom properties exclusively
- ✅ Could work for Bootstrap 4 with explicit property values

#### 3. Hybrid Mode (Fallback)

```bash
dotnet run -- convert \
  --input bootstrap.css _variables.scss \
  --output ./theme \
  --analysis-mode hybrid
```

**Capabilities**:

- Runs both CSS and variables analysis
- Merges results (CSS takes precedence)
- Maximum coverage when both sources available

### Code Architecture

**Core Classes**:

```tree
FlagstoneUI.BootstrapConverter/
├── BootstrapParser.cs           # SCSS variable parsing (production-ready)
├── BootstrapCssAnalyzer.cs      # CSS class analysis (limited by ExCSS)
├── BootstrapMapper.cs           # Token mapping (both modes)
├── XamlThemeGenerator.cs        # XAML generation
└── Models/
    ├── BootstrapVariables.cs    # Variables mode model
    ├── ComputedStyle.cs         # CSS mode model
    └── FlagstoneTokens.cs       # Output model
```

**Key Methods**:

- `BootstrapParser.ParseMultipleFilesAsync()` - Multi-file SCSS with variable resolution
- `BootstrapCssAnalyzer.AnalyzeComponents()` - Extract 25+ component styles
- `BootstrapMapper.MapToFlagstoneTokens()` - Variables → tokens (recommended)
- `BootstrapMapper.MapComponentStylesToTokens()` - CSS styles → tokens (limited)
- `XamlThemeGenerator.GenerateFilesAsync()` - Create Tokens.xaml, Styles.xaml, Theme.xaml

### CLI Implementation

**Commands**:

- `convert` - Convert theme to Flagstone XAML files
- `info` - Display theme information without conversion

**Key Options**:

- `--input` / `-i` - Input file(s) or URL(s), supports multiple files
- `--output` / `-o` - Output directory
- `--analysis-mode` / `-a` - Analysis strategy: variables (default), css, hybrid
- `--dark-mode` / `-d` - Dark mode generation: auto, manual, none
- `--verbose` / `-v` - Detailed logging
- `--debug` - Debug-level logging

### Generated Output

**Three XAML Files**:

1. **Tokens.xaml** - Design tokens with x:Class and code-behind
   - Color tokens (Primary, Secondary, Success, Error, Warning, Info, Background, Surface, etc.)
   - Typography tokens (FontFamily.Default, FontSize.Body, LineHeight.Default)
   - Spacing tokens (ExtraSmall → ExtraLarge)
   - Border tokens (Radius.Small/Medium/Large, BorderWidth.Default)

2. **Styles.xaml** - Control styles with DynamicResource bindings
   - FsButton default style (filled primary)
   - OutlinedButton style (bordered, transparent background)
   - TextButton style (minimal, no background/border)
   - VisualStateGroups for Normal/Disabled states

3. **Theme.xaml** - Merged resource dictionary
   - Merges Tokens.xaml and Styles.xaml
   - Single entry point for theme consumption

## Validation Results

### Successful Conversion: Bootswatch Darkly

**Input**: `tests/FlagstoneUI.BootstrapConverter.Tests/Fixtures/bootswatch-darkly.scss`

**Command**:

```bash
dotnet run -- convert \
  --input tests/.../bootswatch-darkly.scss \
  --output ./test-output-darkly \
  --analysis-mode variables \
  --verbose
```

**Results**:

- ✅ Parsed: 11 colors, 3 typography, 1 spacing, 4 borders
- ✅ Mapped: 11 color tokens, 3 typography, 5 spacing, 3 radius, 1 width
- ✅ Generated: Complete XAML theme (Tokens.xaml, Styles.xaml, Theme.xaml)
- ✅ FsButton styles: Default (filled), OutlinedButton (bordered), TextButton (minimal)
- ✅ Proper DynamicResource bindings
- ✅ VisualStateGroups configured

**Token Breakdown**:

Colors:

- Background: #222222
- Error: #E74C3C
- Info: #3498DB
- OnBackground: #ADB5BD
- Outline: #444444
- Primary: #375A7F
- Secondary: #444444
- Success: #00BC8C
- Surface: #ADB5BD
- SurfaceVariant.Dark: #303030
- Warning: #F39C12

Typography:

- FontFamily.Default: System
- FontSize.Body: 15
- LineHeight.Default: 1.5

Spacing:

- ExtraSmall: 4
- Small: 8
- Medium: 16
- Large: 24
- ExtraLarge: 48

Border Radius:

- Small: 3.2
- Medium: 4
- Large: 4.8

Border Width:

- Default: 1

### Failed Conversion: Bootstrap 5 CDN

**Input**: `https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css`

**Issue**: ExCSS 4.2.3 doesn't parse CSS custom properties

**Example**:

```css
.btn-primary {
  color: var(--bs-btn-color);
  background-color: var(--bs-btn-bg);
  border-color: var(--bs-btn-border-color);
}
```

**ExCSS Result**: `rule.Style.Length == 0` (declarations ignored)

**Workaround**: Use SCSS source files with variables mode

## Documentation

### Created/Updated Files

**Documentation**:

- `docs/Decisions/adr005-bootstrap-converter-analysis-modes.md` - Architecture decision record
- `docs/mcp-bootstrap-converter.md` - Updated with phase 1 completion status
- `docs/implementation-status.md` - Added Bootstrap converter completion
- `docs/roadmap.md` - Added completed milestone and UI app plan
- `tools/FlagstoneUI.BootstrapConverter/README.md` - Library documentation
- `tools/FlagstoneUI.BootstrapConverter.Cli/README.md` - CLI usage guide

**Code**:

- `tools/FlagstoneUI.BootstrapConverter/` - Core library (8 classes, 3 models)
- `tools/FlagstoneUI.BootstrapConverter.Cli/` - CLI tool (2 commands)
- `tests/FlagstoneUI.BootstrapConverter.Tests/` - Unit tests with fixtures

## Key Decisions (ADR005)

**Decision**: Multi-mode architecture with Variables Mode as recommended default

**Rationale**:

1. Variables mode provides excellent production-ready results
2. CSS mode infrastructure exists for future use (if ExCSS upgraded)
3. Hybrid mode provides fallback strategy

**Consequences**:

- ✅ Clear user guidance: use SCSS source files
- ✅ Production-ready path validated
- ⚠️ CSS mode limited until ExCSS upgrade
- ✅ Architecture supports future improvements

**Future Options**:

- Upgrade to ExCSS 5.x (if CSS custom property support added)
- Implement custom CSS variable parser
- Use browser automation (Playwright) to resolve CSS variables
- Use Sass.js to compile SCSS → CSS with resolved values

## Known Limitations

1. **ExCSS 4.2.3**: Doesn't parse CSS custom properties (`--bs-*`)
   - **Impact**: CSS mode doesn't work for Bootstrap 5+ compiled CSS
   - **Workaround**: Use variables mode with SCSS source files

2. **Component Coverage**: Currently focused on buttons
   - FsButton styles generated (Default, OutlinedButton, TextButton)
   - Forms, cards, navigation planned for future

3. **Bootstrap 4**: Not actively tested
   - CSS mode might work (uses explicit property values)
   - Variables mode should work with SCSS

## Integration with Flagstone UI

### Project Structure

```tree
src/FlagstoneUI.Themes.Bootstrap/
├── Tokens.xaml (generated)
├── Tokens.xaml.cs (generated)
├── Styles.xaml (generated)
├── Styles.xaml.cs (generated)
├── Theme.xaml (generated)
└── Theme.xaml.cs (generated)
```

### App.xaml Integration

```xaml
<Application xmlns:bootstrap="clr-namespace:FlagstoneUI.Themes.Bootstrap;assembly=FlagstoneUI.Themes.Bootstrap">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <bootstrap:Theme />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

## Lessons Learned

### Technical Insights

1. **Public API vs Implementation**: Bootstrap's public API is CSS classes, not variables
   - Variables are implementation details that vary between themes
   - CSS classes provide consistent interface but depend on CSS custom properties

2. **Library Limitations Matter**: ExCSS 4.2.3 CSS custom property gap is significant
   - Bootstrap 5+ relies heavily on CSS custom properties
   - SCSS source files provide reliable alternative

3. **Multi-File Parsing Critical**: Bootswatch themes split across multiple files
   - `_variables.scss` - Base color values
   - `_bootswatch.scss` - Overrides and fonts
   - Variable resolution across files essential for complete coverage

### Process Insights

1. **Architecture Before Scale**: Implementing both approaches validated the decision
   - Top-down CSS analysis built but limited by library
   - Bottom-up variables parsing became production path
   - Having both options documented supports future decisions

2. **Test with Real Data**: Bootswatch themes provided excellent validation
   - Real-world complexity exposed variable resolution needs
   - Multi-file parsing requirement discovered
   - Token coverage validated (20+ tokens consistently)

3. **Documentation is Code**: ADR captures context for future development
   - Why decisions were made
   - What was tried and didn't work
   - Clear recommendations for users

## Next Phase: UI App

### Vision

.NET MAUI desktop app for visual Bootstrap theme conversion with real-time preview.

### Key Features

1. **Visual Theme Converter**
   - File picker for local SCSS/CSS files
   - URL input for remote themes
   - Analysis mode selection dropdown
   - Dark mode strategy selector
   - Export to XAML files

2. **Live Preview**
   - Apply converted theme to sample Flagstone UI controls in-app
   - Toggle between original Bootstrap and converted theme
   - Side-by-side comparison
   - Control showcase (FsButton, FsEntry, FsCard)

3. **Bootswatch Integration**
   - Browse 26+ Bootswatch theme catalog
   - One-click theme download and conversion
   - Preview before conversion
   - Save favorite themes

### Benefits

- No command-line knowledge required
- Visual feedback during conversion
- Real-time theme validation
- Demonstrates .NET MAUI desktop capabilities
- Showcases Flagstone UI's theming system
- Lower barrier to entry for designers/non-developers

### Timeline

**Target**: Q1 2026 (January-March)

**Milestones**:

1. Basic UI with file picker and conversion (2 weeks)
2. Live preview with sample controls (2 weeks)
3. Bootswatch catalog integration (1 week)
4. Polish and testing (1 week)

## Conclusion

Phase 1 of the Bootstrap Converter is complete and production-ready for SCSS-based themes. The tool provides excellent token extraction (20+ tokens), automated style generation (3 FsButton variants), and complete XAML theme files ready for integration with Flagstone UI projects.

The next phase will build on this foundation with a visual UI app that makes theme conversion accessible to designers and developers alike, while demonstrating .NET MAUI's desktop capabilities and Flagstone UI's powerful theming system.

**Key Takeaway**: Sometimes the "obvious" approach (CSS class analysis) isn't viable due to library limitations, but having the architecture in place supports future improvements while the alternative approach (SCSS variable parsing) provides excellent production results today.

---

**References**:

- ADR005: Bootstrap Converter Analysis Modes
- Library README: tools/FlagstoneUI.BootstrapConverter/README.md
- CLI README: tools/FlagstoneUI.BootstrapConverter.Cli/README.md
- Validated Themes: Bootswatch Darkly, Bootswatch Flatly
