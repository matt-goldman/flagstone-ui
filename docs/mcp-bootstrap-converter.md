# Bootstrap Theme Converter MCP

> **⚠️ DRAFT DOCUMENT - WORK IN PROGRESS**  
> This documentation is incomplete and reflects planned functionality.  
> See [implementation-status.md](implementation-status.md) for current tool status.

## Overview

A .NET-first tool that converts Bootstrap themes into Flagstone UI token definitions and theme files. Follows the proven architecture of the TailwindXamlThemeConverter - core functionality in a class library, wrapped in a console app for POC/testing, packaged as a .NET tool, and serving MCP via stdout.

## Goals

1. **Primary**: Build robust .NET class library with conversion logic
2. **Secondary**: Create console app wrapper for rapid testing/iteration
3. **Tertiary**: Package as .NET global tool
4. **Quaternary**: Serve MCP protocol via stdout (like .NET tool)
5. **Future**: Easy migration path to TypeScript/Python if needed for VS Code extension

## Value Proposition

- **Reuse existing Bootstrap themes** in .NET MAUI apps
- **Consistent styling** across web and mobile applications
- **Faster theme development** by leveraging Bootstrap ecosystem
- **Demonstrates MCP tooling** as core differentiator for Flagstone UI
- **Rapid iteration** with familiar .NET tooling and testing
- **Migration-friendly** - solve functionality first, port later if needed

## Architecture

### .NET Project Structure

```tree
tools/FlagstoneUI.BootstrapConverter/
├── FlagstoneUI.BootstrapConverter.csproj          # Class library (core logic)
├── BootstrapParser.cs                             # Parse Bootstrap CSS/SCSS
├── BootstrapMapper.cs                             # Map Bootstrap → Flagstone tokens
├── XamlThemeGenerator.cs                          # Generate Tokens.xaml & Theme.xaml
├── Models/
│   ├── BootstrapVariables.cs                      # Bootstrap variable models
│   ├── BootstrapTheme.cs                          # Complete Bootstrap theme
│   └── ConversionOptions.cs                       # Conversion configuration
├── Mappings/
│   ├── ColorMappings.cs                           # Bootstrap → Flagstone color mapping
│   ├── TypographyMappings.cs                      # Typography mapping rules
│   └── SpacingMappings.cs                         # Spacing/sizing mappings
└── Resources/
    ├── bootstrap-mappings.json                    # Reference mappings
    └── templates/
        ├── Tokens.xaml.template                   # Token template
        └── Theme.xaml.template                    # Theme template

tools/FlagstoneUI.BootstrapConverter.Cli/
├── FlagstoneUI.BootstrapConverter.Cli.csproj      # Console app / .NET tool
├── Program.cs                                     # CLI commands (System.CommandLine)
├── Commands/
│   ├── ConvertCommand.cs                          # bootstrap convert
│   ├── ParseCommand.cs                            # bootstrap parse
│   └── ListCommand.cs                             # bootstrap list-themes
└── McpServer.cs                                   # MCP protocol via stdout

tests/FlagstoneUI.BootstrapConverter.Tests/
├── FlagstoneUI.BootstrapConverter.Tests.csproj
├── ParserTests.cs                                 # Bootstrap parsing tests
├── MapperTests.cs                                 # Mapping logic tests
├── GeneratorTests.cs                              # XAML generation tests
└── Fixtures/
    ├── bootstrap-default.css                      # Test Bootstrap themes
    ├── bootstrap-darkly.css                       # Bootswatch Darkly
    └── bootstrap-custom.scss                      # Custom SCSS
```

### Inspired by TokenGenerator

The structure mirrors `tools/FlagstoneUI.TokenGenerator`:

- **Core logic in class library** for reusability and testability
- **Console app wrapper** with System.CommandLine for CLI interface
- **Separate concerns**: parsing, mapping, generation
- **JSON-first internal format** for MCP compatibility
- **XAML generation** utilities similar to `XamlGenerator.cs`

### CLI Commands (Console App)

The console app exposes functionality via System.CommandLine:

#### 1. `bootstrap convert`

**Purpose**: Convert Bootstrap theme to Flagstone tokens and/or theme

**Usage**:

```bash
# Convert from URL
flagstone-bootstrap convert --source https://bootswatch.com/5/darkly/bootstrap.css --output ./themes/darkly

# Convert from file
flagstone-bootstrap convert --source ./bootstrap.scss --output ./themes/custom --format scss

# Generate tokens only
flagstone-bootstrap convert --source ./bootstrap.css --output ./tokens --tokens-only

# Generate complete theme with base
flagstone-bootstrap convert --source ./bootstrap.css --output ./theme --theme-name "Bootstrap Darkly" --base-theme Material
```

**Options**:

- `--source`, `-s`: URL or file path to Bootstrap theme (required)
- `--output`, `-o`: Output directory for generated files (required)
- `--format`, `-f`: Input format: `css`, `scss`, `json` (auto-detect if omitted)
- `--tokens-only`: Generate only Tokens.xaml (no Theme.xaml)
- `--theme-name`: Name for the theme (default: "Bootstrap Theme")
- `--base-theme`: Base theme to extend: `Material`, `Modern`, `None` (default: None)
- `--dark-mode`: Dark mode strategy: `auto`, `manual`, `none` (default: auto)
- `--json`: Output intermediate JSON for inspection

**Output**:

```text
📦 Bootstrap Theme Converter
   Source:  https://bootswatch.com/5/darkly/bootstrap.css
   Output:  ./themes/darkly
   Format:  css (auto-detected)

✅ Parsing Bootstrap theme...
   ✓ Found 47 variables (8 colors, 12 typography, 6 spacing, 4 borders)

✅ Mapping to Flagstone tokens...
   ✓ Mapped 8 color tokens
   ✓ Mapped 12 typography tokens
   ✓ Mapped 6 spacing tokens
   ✓ Mapped 4 border tokens
   ✓ Generated 8 dark mode variants (auto)

✅ Generating XAML files...
   ✓ Tokens.xaml (2.4 KB)
   ✓ Theme.xaml (5.1 KB)

✅ Conversion complete!
   Files saved to: ./themes/darkly
```

#### 2. `bootstrap parse`

**Purpose**: Parse Bootstrap theme and output variables as JSON

**Usage**:

```bash
# Parse and display
flagstone-bootstrap parse --source ./bootstrap.css

# Parse and save JSON
flagstone-bootstrap parse --source ./bootstrap.css --output ./variables.json
```

**Output**:

```json
{
  "colors": {
    "primary": "#375a7f",
    "secondary": "#444",
    "success": "#00bc8c",
    "danger": "#e74c3c",
    "warning": "#f39c12",
    "info": "#3498db",
    "light": "#adb5bd",
    "dark": "#303030"
  },
  "typography": {
    "fontFamilyBase": "-apple-system, BlinkMacSystemFont, 'Segoe UI'",
    "fontSizeBase": "1rem",
    "lineHeightBase": "1.5"
  },
  "spacing": {
    "spacer": "1rem"
  },
  "borders": {
    "borderRadius": "0.25rem",
    "borderWidth": "1px"
  }
}
```

#### 3. `bootstrap list-themes`

**Purpose**: List popular Bootstrap themes from Bootswatch

**Usage**:

```bash
# List all themes
flagstone-bootstrap list-themes

# List with JSON output
flagstone-bootstrap list-themes --json
```

**Output**:

```text
🎨 Available Bootstrap Themes (Bootswatch)

1. Darkly
   Primary: #375a7f | Accent: #00bc8c
   URL: https://bootswatch.com/5/darkly/bootstrap.css
   Preview: https://bootswatch.com/5/darkly/

2. Lux
   Primary: #1a1a1a | Accent: #d9230f
   URL: https://bootswatch.com/5/lux/bootstrap.css
   Preview: https://bootswatch.com/5/lux/

... (more themes)
```

### MCP Server Mode

The CLI tool can also run as an MCP server, communicating via stdin/stdout:

```bash
# Start MCP server mode
flagstone-bootstrap mcp

# Or via .NET tool
dotnet tool run flagstone-bootstrap mcp
```

**MCP Tools Exposed** (same as CLI commands):

- `bootstrap_convert` - Convert Bootstrap theme to Flagstone
- `bootstrap_parse` - Parse Bootstrap variables
- `bootstrap_list_themes` - List available themes

The MCP server wraps the same class library functionality, just with JSON-RPC protocol over stdio.

## Bootstrap → Flagstone Mapping

### Color Mapping

| Bootstrap Variable | Flagstone Token | Notes |
|-------------------|-----------------|-------|
| `--bs-primary` | `Color.Primary` | Direct mapping |
| `--bs-secondary` | `Color.Secondary` | Direct mapping |
| `--bs-success` | `Color.Success` | Direct mapping |
| `--bs-danger` | `Color.Error` | Semantic mapping |
| `--bs-warning` | `Color.Warning` | Direct mapping |
| `--bs-info` | `Color.Info` | Direct mapping |
| `--bs-light` | `Color.Surface` | Semantic mapping |
| `--bs-dark` | `Color.SurfaceVariant.Dark` | Semantic mapping |
| `--bs-body-bg` | `Color.Background` | Semantic mapping |
| `--bs-body-color` | `Color.OnBackground` | Semantic mapping |
| `--bs-border-color` | `Color.Outline` | Semantic mapping |

### Dark Mode Strategy

Bootstrap doesn't have built-in dark mode variables, so we need to generate them:

1. **Auto-generate**: Use color manipulation to create dark variants
   - Darken light colors by 20%
   - Lighten dark colors by 20%
   - Invert surface/background relationships

2. **Manual specification**: Allow user to provide dark mode values

3. **None**: Single theme only (user handles dark mode elsewhere)

### Typography Mapping

| Bootstrap Variable | Flagstone Token | Conversion |
|-------------------|-----------------|------------|
| `--bs-font-family-sans-serif` | `FontFamily.Default` | CSS font stack → MAUI font names |
| `--bs-font-size-base` | `FontSize.Body` | rem → absolute pixels (16px base) |
| `--bs-font-weight-normal` | `FontWeight.Regular` | CSS weight → MAUI weight |
| `--bs-font-weight-bold` | `FontWeight.Bold` | CSS weight → MAUI weight |
| `--bs-line-height-base` | `LineHeight.Default` | Unitless → pixels |
| `--bs-headings-font-family` | `FontFamily.Heading` | CSS font stack → MAUI font names |

### Spacing Mapping

| Bootstrap Variable | Flagstone Token | Conversion |
|-------------------|-----------------|------------|
| `--bs-spacer` (1rem) | `Spacing.Medium` | Base unit |
| `--bs-spacer * 0.25` | `Spacing.ExtraSmall` | 0.25rem |
| `--bs-spacer * 0.5` | `Spacing.Small` | 0.5rem |
| `--bs-spacer * 1.5` | `Spacing.Large` | 1.5rem |
| `--bs-spacer * 3` | `Spacing.ExtraLarge` | 3rem |

### Border Mapping

| Bootstrap Variable | Flagstone Token | Conversion |
|-------------------|-----------------|------------|
| `--bs-border-radius` | `CornerRadius.Medium` | CSS px → MAUI CornerRadius |
| `--bs-border-radius-sm` | `CornerRadius.Small` | CSS px → MAUI CornerRadius |
| `--bs-border-radius-lg` | `CornerRadius.Large` | CSS px → MAUI CornerRadius |
| `--bs-border-width` | `BorderWidth.Default` | CSS px → double |

## Implementation Phases

### Phase 1: Core Class Library (Days 1-2)

**Goal**: Build `FlagstoneUI.BootstrapConverter` class library with core functionality

**Tasks**:

- Bootstrap CSS/SCSS variable parser (`BootstrapParser.cs`)
  - Support CSS custom properties (`--bs-*`)
  - Support SCSS variables (`$primary`)
  - Extract color, typography, spacing, border values
  - Unit conversion utilities (rem → px, etc.)
- Bootstrap → Flagstone mapping (`BootstrapMapper.cs`)
  - Color mapping (Bootstrap semantic colors → Flagstone tokens)
  - Typography mapping (font stacks, sizes, weights)
  - Spacing/border mapping
- XAML generation (`XamlThemeGenerator.cs`)
  - Similar to `tools/FlagstoneUI.TokenGenerator/XamlGenerator.cs`
  - Generate `Tokens.xaml` from mapped values
  - Generate `Theme.xaml` with control styles (optional)
- Dark mode generation strategies
  - Auto-generate dark variants (darken/lighten by 20%)
  - Manual specification support
  - None (single theme only)

**Tests**:

- Unit tests for parsing Bootstrap fixtures
- Mapping validation tests
- XAML generation tests with expected outputs

**Deliverable**: Working class library with comprehensive tests

### Phase 2: Console App Wrapper (Day 2-3)

**Goal**: Create CLI tool for rapid testing and iteration

**Tasks**:

- Set up `FlagstoneUI.BootstrapConverter.Cli` project
- Implement System.CommandLine commands:
  - `convert` - Full conversion pipeline
  - `parse` - Parse and output JSON
  - `list-themes` - List Bootswatch themes
- Add progress reporting and colored output
- Configure as packable .NET tool (.csproj settings)

**Tests**:

- Integration tests running CLI commands
- Fixture-based validation (convert known themes)

**Deliverable**: Working CLI tool, manually testable with `dotnet run`

### Phase 3: MCP Server Mode (Day 3-4)

**Goal**: Enable MCP protocol communication via stdio

**Tasks**:

- Implement `McpServer.cs` for JSON-RPC over stdio
- Map MCP tool calls to class library methods
- Expose tools: `bootstrap_convert`, `bootstrap_parse`, `bootstrap_list_themes`
- Add `mcp` command to CLI
- Test with MCP inspector or VS Code

**Tests**:

- MCP protocol tests (JSON-RPC request/response)
- Tool invocation tests

**Deliverable**: Working MCP server mode

### Phase 4: Integration & Packaging (Day 4-5)

**Goal**: Package as .NET tool and integrate with ThemePlayground

**Tasks**:

- Configure NuGet packaging (.nuspec or .csproj PackageReference)
- Publish to NuGet (or local feed for testing)
- Test installation: `dotnet tool install -g FlagstoneUI.BootstrapConverter`
- Integrate with ThemePlayground sample app
- Theme switching mechanism demonstration
- Documentation (README, API docs, examples)

**Tests**:

- End-to-end: install tool → convert theme → use in MAUI app
- Visual verification in ThemePlayground

**Deliverable**: Packaged tool, working demo, complete documentation

## Migration Path

If we later need to migrate to TypeScript/Python (e.g., for easier VS Code extension):

1. **Core logic is proven** in .NET - we know it works
2. **Test fixtures serve as migration validation** - same inputs/outputs
3. **Focus migration on translation**, not functionality
4. **Keep .NET version** as reference implementation
5. **Potentially maintain both** - .NET tool for CLI, TS/Python for VS Code extension

This approach dramatically reduces risk compared to building in an unfamiliar platform from scratch.

## File Format Support

### Input Formats

1. **Bootstrap CSS** (CSS custom properties)

   ```css
   :root {
     --bs-primary: #375a7f;
     --bs-secondary: #444;
   }
   ```

2. **Bootstrap SCSS** (SCSS variables)

   ```scss
   $primary: #375a7f;
   $secondary: #444;
   ```

3. **Bootstrap JSON** (custom format for easy parsing)

   ```json
   {
     "primary": "#375a7f",
     "secondary": "#444"
   }
   ```

### Output Formats

1. **Tokens.xaml** - Token definitions only
2. **Theme.xaml** - Complete theme with styles
3. **Combined** - Both in ResourceDictionary structure

## Usage Examples

### Example 1: Convert Bootswatch Theme (CLI)

```bash
# Install tool (once packaged)
dotnet tool install -g FlagstoneUI.BootstrapConverter

# Convert Bootswatch Darkly theme
flagstone-bootstrap convert \
  --source https://bootswatch.com/5/darkly/bootstrap.css \
  --output ./FlagstoneUI.Themes.Bootstrap.Darkly \
  --theme-name "Bootstrap Darkly" \
  --base-theme Material

# Result:
# ./FlagstoneUI.Themes.Bootstrap.Darkly/Tokens.xaml
# ./FlagstoneUI.Themes.Bootstrap.Darkly/Theme.xaml
```

### Example 2: Custom Bootstrap Theme

```bash
# User has custom Bootstrap SCSS
flagstone-bootstrap convert \
  --source ./custom-bootstrap.scss \
  --output ./FlagstoneUI.Themes.CustomBrand \
  --theme-name "Custom Brand" \
  --format scss

# Parse first to inspect variables
flagstone-bootstrap parse --source ./custom-bootstrap.scss --output ./variables.json

# Review variables.json, then convert
flagstone-bootstrap convert --source ./custom-bootstrap.scss --output ./theme
```

### Example 3: Programmatic Usage (C#)

```csharp
using FlagstoneUI.BootstrapConverter;

// Parse Bootstrap theme
var parser = new BootstrapParser();
var variables = await parser.ParseAsync("https://bootswatch.com/5/darkly/bootstrap.css", BootstrapFormat.Css);

// Map to Flagstone tokens
var mapper = new BootstrapMapper();
var tokens = mapper.MapToFlagstoneTokens(variables, new ConversionOptions 
{
    DarkModeStrategy = DarkModeStrategy.Auto,
    IncludeComments = true
});

// Generate XAML
var generator = new XamlThemeGenerator();
var tokensXaml = generator.GenerateTokensXaml(tokens);
var themeXaml = generator.GenerateThemeXaml(tokens, new ThemeOptions 
{
    ThemeName = "Bootstrap Darkly",
    BaseTheme = BaseTheme.Material,
    IncludeControlStyles = new[] { "FsEntry", "FsButton", "FsCard" }
});

// Save files
await File.WriteAllTextAsync("./Tokens.xaml", tokensXaml);
await File.WriteAllTextAsync("./Theme.xaml", themeXaml);
```

### Example 4: MCP Usage (from AI assistant)

```json
// AI assistant calls MCP tool
{
  "method": "tools/call",
  "params": {
    "name": "bootstrap_convert",
    "arguments": {
      "source": "https://bootswatch.com/5/darkly/bootstrap.css",
      "themeName": "Bootstrap Darkly",
      "baseTheme": "Material",
      "outputPath": "./themes/darkly"
    }
  }
}

// MCP server responds with file contents
{
  "result": {
    "tokensXaml": "<?xml version=\"1.0\" encoding=\"utf-8\"?>...",
    "themeXaml": "<?xml version=\"1.0\" encoding=\"utf-8\"?>...",
    "outputPath": "./themes/darkly"
  }
}
```

## Testing Strategy

### Unit Tests (Class Library)

**`FlagstoneUI.BootstrapConverter.Tests`**:

- **Parser Tests** (`ParserTests.cs`)
  - Bootstrap CSS parsing (CSS custom properties)
  - Bootstrap SCSS parsing (SCSS variables)
  - Bootstrap JSON parsing (custom format)
  - Unit conversion (rem, em, px)
  - Edge cases (missing variables, invalid syntax)
- **Mapper Tests** (`MapperTests.cs`)
  - Color mapping validation
  - Dark mode generation (auto strategy)
  - Typography font stack conversion
  - Spacing/border mapping
- **Generator Tests** (`GeneratorTests.cs`)
  - XAML token generation
  - XAML theme generation
  - XML validation
  - Output matches expected fixtures

### Integration Tests (CLI)

**`FlagstoneUI.BootstrapConverter.Cli.Tests`**:

- Command execution tests (via `CommandLineBuilder`)
- Full conversion pipeline with fixtures
- CLI output validation
- JSON output mode

### End-to-End Tests

- Install tool via NuGet local feed
- Convert real Bootstrap themes (Bootswatch)
- Load converted themes in ThemePlayground
- Visual verification (screenshots/manual testing)

### Test Fixtures

**`tests/FlagstoneUI.BootstrapConverter.Tests/Fixtures/`**:

- `bootstrap-5-default.css` - Bootstrap 5 default theme
- `bootswatch-darkly.css` - Bootswatch Darkly theme
- `bootswatch-lux.css` - Bootswatch Lux theme  
- `custom-minimal.scss` - Minimal custom theme (only required variables)
- `custom-maximal.scss` - Maximal theme (all possible variables)
- `expected-outputs/` - Expected Tokens.xaml and Theme.xaml for each fixture

### CI/CD Integration

Same as existing Flagstone UI CI:

- Run on `.github/workflows/ci.yml`
- Execute all tests on push/PR
- Validate against multiple Bootstrap versions
- Performance benchmarks (parsing large Bootstrap files)

## Packaging & Distribution

### .NET Tool Configuration

**`FlagstoneUI.BootstrapConverter.Cli.csproj`**:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    
    <!-- .NET Tool packaging -->
    <PackAsTool>true</PackAsTool>
    <ToolCommandName>flagstone-bootstrap</ToolCommandName>
    <PackageId>FlagstoneUI.BootstrapConverter</PackageId>
    <Version>0.1.0</Version>
    <Authors>Matt Goldman</Authors>
    <Description>Convert Bootstrap themes to Flagstone UI themes and tokens</Description>
    <PackageTags>maui;xaml;bootstrap;theme;converter;mcp</PackageTags>
    <PackageProjectUrl>https://github.com/matt-goldman/flagstone-ui</PackageProjectUrl>
    <RepositoryUrl>https://github.com/matt-goldman/flagstone-ui</RepositoryUrl>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\FlagstoneUI.BootstrapConverter\FlagstoneUI.BootstrapConverter.csproj" />
  </ItemGroup>
</Project>
```

### Publishing

```bash
# Build and pack
dotnet pack tools/FlagstoneUI.BootstrapConverter.Cli -c Release -o ./nupkg

# Publish to NuGet.org
dotnet nuget push ./nupkg/FlagstoneUI.BootstrapConverter.0.1.0.nupkg --source https://api.nuget.org/v3/index.json --api-key <key>

# Or test locally
dotnet tool install --global --add-source ./nupkg FlagstoneUI.BootstrapConverter
```

### Installation & Updates

```bash
# Install globally
dotnet tool install -g FlagstoneUI.BootstrapConverter

# Update
dotnet tool update -g FlagstoneUI.BootstrapConverter

# Uninstall
dotnet tool uninstall -g FlagstoneUI.BootstrapConverter
```

## Error Handling

### Parser Errors

- **Invalid Bootstrap format**: Clear message about supported formats
- **Missing required variables**: Provide defaults or fail gracefully
- **Malformed CSS/SCSS**: Line number and error context

### Conversion Errors

- **Unsupported color format**: Fallback to nearest supported format
- **Invalid font family**: Use system default
- **Missing dark mode values**: Generate or warn

### Generation Errors

- **Invalid XAML**: Syntax validation before returning
- **Duplicate token names**: Rename with suffix
- **Circular dependencies**: Detect and report

## Future Enhancements

### Version 2

- Support for Bootstrap 4 themes
- Custom mapping configuration (user-defined mappings)
- Theme variants (light/dark/high-contrast from single source)
- Real-time preview in VS Code extension
- Bootstrap component → Flagstone control mapping

### Version 3

- Tailwind CSS support (separate MCP server)
- Material Design theme import
- Figma token integration
- Design system synchronization

## Dependencies

### Class Library (`FlagstoneUI.BootstrapConverter`)

- **Target Framework**: `net10.0`
- **NuGet Packages**:
  - `ExCSS` or `AngleSharp.Css` - CSS parser
  - `LibSassHost` or custom parser - SCSS parser (optional, phase 2)
  - `System.Text.Json` - JSON parsing/serialization (built-in)
  - `System.Drawing.Common` or `SixLabors.ImageSharp` - Color manipulation
- **Optional**:
  - `HtmlAgilityPack` - HTML parsing if fetching from URLs
  - `HttpClient` - Fetching Bootstrap themes from CDNs

### CLI Tool (`FlagstoneUI.BootstrapConverter.Cli`)

- **Target Framework**: `net10.0`
- **NuGet Packages**:
  - `System.CommandLine` - CLI framework
  - `Spectre.Console` - Colored terminal output (optional)
  - Reference to `FlagstoneUI.BootstrapConverter` class library

### Test Projects

- **Target Framework**: `net10.0`
- **NuGet Packages**:
  - `xUnit` - Test framework (matching Flagstone UI conventions)
  - `FluentAssertions` - Assertion library
  - `Microsoft.NET.Test.Sdk` - Test runner

### Rationale

- **ExCSS/AngleSharp**: Proven CSS parsers for .NET
- **LibSassHost**: If we need full SCSS support (can defer to phase 2)
- **System.CommandLine**: Microsoft's official CLI framework
- **Spectre.Console**: Beautiful terminal UI (like existing TokenGenerator)
- **xUnit**: Consistent with Flagstone UI test infrastructure

## Documentation Deliverables

1. **README.md** (in `tools/FlagstoneUI.BootstrapConverter/`)
   - Quick start guide
   - Installation instructions
   - CLI command reference
   - Programmatic API examples

2. **API.md** (in `tools/FlagstoneUI.BootstrapConverter/`)
   - Class library API documentation
   - Public types and methods
   - Extension points for custom mappings

3. **MAPPINGS.md** (in `tools/FlagstoneUI.BootstrapConverter/`)
   - Complete Bootstrap → Flagstone mapping reference
   - Color, typography, spacing, border mappings
   - Dark mode generation strategies

4. **MCP.md** (in `tools/FlagstoneUI.BootstrapConverter/`)
   - MCP server mode documentation
   - Tool schemas (JSON-RPC)
   - Integration examples (Claude Desktop, VS Code)

5. **Blog Post** (in `docs/` or external)
   - .NET 10 launch announcement
   - Bootstrap converter showcase
   - Theme migration tutorial
   - Before/after comparisons

### In-Code Documentation

- XML doc comments on all public APIs
- Usage examples in remarks sections
- Links to relevant mapping documentation

## Success Metrics

### MVP (Week 1)

- ✅ Parse Bootstrap 5 default theme
- ✅ Generate valid Tokens.xaml
- ✅ Generate working Theme.xaml
- ✅ Demo theme switching with 3+ Bootstrap themes
- ✅ Documentation complete

### Post-MVP (Month 1)

- 📊 10+ Bootstrap themes converted successfully
- 📊 Community contributed custom themes
- 📊 GitHub stars/engagement
- 📊 Blog post views/shares

## Open Questions

1. **Font family handling**: How to map web fonts to MAUI fonts?
   - Option A: Document required font installation
   - Option B: Fallback to system fonts
   - Option C: Bundle fonts with theme

2. **Component-specific styling**: Bootstrap has component styles (buttons, cards), should we map these?
   - Option A: Generate full control styles
   - Option B: Tokens only, let themes handle styles
   - **Recommendation**: Option B (MVP), Option A (V2)

3. **CSS variable fallbacks**: Bootstrap uses `var(--bs-primary, fallback)`, how to handle?
   - Option A: Resolve at parse time
   - Option B: Convert to XAML DynamicResource
   - **Recommendation**: Option A (simpler)

4. **Responsive spacing**: Bootstrap has breakpoint-based spacing, relevant for MAUI?
   - Option A: Ignore (mobile doesn't have breakpoints like web)
   - Option B: Map to idiom-specific resources
   - **Recommendation**: Option A (MVP)

5. **Shadows**: Bootstrap uses box-shadow, MAUI uses Shadow element, how to convert?
   - Option A: Parse shadow syntax, generate Shadow properties
   - Option B: Approximate with Border/opacity
   - **Recommendation**: Option A (more accurate)

## Next Steps

1. **Review this spec** ✅ - Validate .NET-first approach and architecture
2. **Set up projects** 🔲
   - Create `tools/FlagstoneUI.BootstrapConverter/` (class library)
   - Create `tools/FlagstoneUI.BootstrapConverter.Cli/` (console app)
   - Create `tests/FlagstoneUI.BootstrapConverter.Tests/` (xUnit tests)
   - Add to `Flagstone.UI.sln`
3. **Phase 1: Core Library** 🔲
   - Implement `BootstrapParser.cs` with CSS support
   - Implement `BootstrapMapper.cs` with color/typography mapping
   - Implement `XamlThemeGenerator.cs` (reuse patterns from `TokenGenerator`)
   - Write comprehensive unit tests
4. **Phase 2: CLI Wrapper** 🔲
   - Implement `convert`, `parse`, `list-themes` commands
   - Add progress reporting and colored output
   - Integration tests with fixtures
5. **Phase 3: MCP Mode** 🔲
   - Implement `McpServer.cs` for stdio JSON-RPC
   - Test with MCP inspector
6. **Phase 4: Package & Demo** 🔲
   - Configure NuGet packaging
   - Publish to local feed for testing
   - Integrate with ThemePlayground
   - Complete documentation
   - Write blog post

## Timeline (Revised)

- **Day 1**: Project setup + Phase 1 start (parser + basic mapping)
- **Day 2**: Phase 1 completion (full mapping + XAML generation + tests)
- **Day 3**: Phase 2 (CLI wrapper + integration tests)
- **Day 4**: Phase 3 (MCP mode) + Phase 4 start (packaging)
- **Day 5**: Phase 4 completion (demo + docs) + buffer for issues

**Target**: Ready for .NET 10 launch (November 11-12, 2025)

## Advantages of .NET-First Approach

1. **Familiar tooling** - C#, Visual Studio, xUnit, NuGet
2. **Rapid iteration** - No learning curve, faster development
3. **Strong typing** - Compile-time safety, better refactoring
4. **Proven patterns** - Follow `FlagstoneUI.TokenGenerator` structure
5. **Easy testing** - Robust unit testing with xUnit
6. **.NET tool ecosystem** - Natural distribution via NuGet
7. **MCP via stdout** - .NET tools can serve MCP protocol
8. **Migration-friendly** - Functionality proven before porting
9. **Maintenance** - Single language/platform for entire repo
10. **Integration** - Seamless with existing Flagstone UI codebase
