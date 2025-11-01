# Flagstone UI Theme Tools

A .NET command-line tool for managing Flagstone UI themes and design tokens. Provides automated catalog generation, validation, and bidirectional conversion between XAML and JSON formats.

## Features

- **Generate Token Catalog**: Extract tokens from XAML files into machine-readable JSON
- **Validate Themes**: Validate both XAML and JSON theme files for correctness
- **Generate XAML**: Convert JSON catalog back to XAML format
- **MCP-Ready**: Structured JSON output for Model Context Protocol integration

## Installation

```bash
# From the repository root
dotnet build tools/FlagstoneUI.TokenGenerator/FlagstoneUI.TokenGenerator.csproj

# Future: Install as global tool
# dotnet tool install -g FlagstoneUI.TokenGenerator
```

## Usage

### Generate Token Catalog (XAML → JSON)

Extract tokens from XAML source files and generate a structured JSON catalog:

```bash
dotnet run --project tools/FlagstoneUI.TokenGenerator -- generate --source src --output docs/tokens-catalog.json
```

**Options:**
- `--source`, `-s`: Source directory containing XAML files (default: `src`)
- `--output`, `-o`: Output path for JSON catalog (default: `docs/tokens-catalog.json`)

**Example Output:**
```
🔍 Flagstone UI Token Catalog Generator
   Source: E:\source\repos\flagstone-ui\src
   Output: E:\source\repos\flagstone-ui\docs\tokens-catalog.json

✅ Token catalog generated successfully!
   File: E:\source\repos\flagstone-ui\docs\tokens-catalog.json
   Size: 16,821 bytes
```

### Validate Theme

Validate token definitions in XAML or JSON format:

```bash
# Validate JSON catalog
dotnet run --project tools/FlagstoneUI.TokenGenerator -- validate --input docs/tokens-catalog.json

# Validate XAML tokens
dotnet run --project tools/FlagstoneUI.TokenGenerator -- validate --input src/FlagstoneUI.Core/Styles/Tokens.xaml

# Get JSON output (for MCP integration)
dotnet run --project tools/FlagstoneUI.TokenGenerator -- validate --input docs/tokens-catalog.json --json
```

**Options:**
- `--input`, `-i`: Path to file to validate (required)
- `--format`, `-f`: Input format (`xaml` or `json`, default: auto-detect)
- `--json`, `-j`: Output results as JSON (default: false)

**Validation Checks:**
- Token structure and required properties
- Color value formats (hex, named colors)
- Numeric value types and ranges
- Dark mode completeness (warnings for missing darkValue)
- Opacity constraints (0-1 range)

**Example Output:**
```
🔍 Flagstone UI Theme Validator
   Input: E:\source\repos\flagstone-ui\docs\tokens-catalog.json

✅ Validation passed!

⚠️  27 warning(s):
   • Color.PrimaryContainer: Color token 'Color.PrimaryContainer' missing darkValue (recommended)
   • Color.Secondary: Color token 'Color.Secondary' missing darkValue (recommended)
```

**JSON Output** (for MCP):
```json
{
  "valid": true,
  "errors": [],
  "warnings": [
    {
      "token": "Color.Primary",
      "message": "Color token missing darkValue (recommended for theme support)"
    }
  ]
}
```

### Generate XAML (JSON → XAML)

Convert JSON catalog back to XAML ResourceDictionary format:

```bash
dotnet run --project tools/FlagstoneUI.TokenGenerator -- generate-xaml --input docs/tokens-catalog.json --output src/FlagstoneUI.Core/Styles/Tokens.xaml
```

**Options:**
- `--input`, `-i`: Path to JSON catalog (default: `docs/tokens-catalog.json`)
- `--output`, `-o`: Output path for XAML (default: `src/FlagstoneUI.Core/Styles/Tokens.xaml`)

**Features:**
- Generates properly formatted XAML with namespace declarations
- Includes purpose comments for tokens with meaningful descriptions
- Adds dark mode values as comments for reference
- Organizes tokens by category with section headers

**Example Output:**
```
🔍 Flagstone UI XAML Generator
   Input:  E:\source\repos\flagstone-ui\docs\tokens-catalog.json
   Output: E:\source\repos\flagstone-ui\src\FlagstoneUI.Core\Styles\Tokens.xaml

✅ XAML generated successfully!
   File: E:\source\repos\flagstone-ui\src\FlagstoneUI.Core\Styles\Tokens.xaml
   Size: 6,286 bytes
```

## MCP Integration

This tool is designed for easy integration with Model Context Protocol (MCP) servers. The JSON output format and structured validation make it ideal for AI-assisted theme development.

**Recommended MCP Tool Signatures:**

```typescript
// Generate catalog from source
flagstone_generate_catalog(source_path: string) 
  → { catalog: string, size: number }

// Validate theme
flagstone_validate_theme(input: string, format?: "xaml" | "json")
  → { valid: boolean, errors: ValidationError[], warnings: ValidationWarning[] }

// Generate XAML
flagstone_generate_xaml(catalog_path: string, output_path: string)
  → { xaml: string, size: number }
```

## Architecture

The tool is organized into focused components:

- **TokenCatalogGenerator**: XAML parsing and JSON generation
- **ThemeValidator**: Token validation for both XAML and JSON
- **XamlGenerator**: JSON to XAML conversion
- **Program**: CLI interface using System.CommandLine

## Development

Built with:
- .NET 10.0
- System.CommandLine 2.0.0-beta4
- System.Xml.Linq for XAML parsing

## Future Enhancements

- [ ] JSON Schema validation in validator
- [ ] Circular dependency detection
- [ ] Accessibility checks (WCAG contrast ratios)
- [ ] Control property analysis with Roslyn
- [ ] Package as dotnet global tool
- [ ] CI/CD integration for automatic catalog updates

## See Also

- [Token Catalog Documentation](../../docs/README.md)
- [Token Schema](../../docs/tokens-schema.json)
- [Design Tokens Guide](../../docs/tokens.md)
