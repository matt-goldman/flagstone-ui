# Token Catalog System (Flagstrap Exploration)

> **⚠️ Important Note**: This document describes token catalog tooling that was developed as part of token system exploration. Following ADR011 (Token System Repositioning), this work is now understood to be related to **Flagstrap** (a future design language system), not FlagstoneUI core.
>
> **Flagstrap is deferred** while FlagstoneUI core is established. This document remains for historical context and as reference for future Flagstrap development.
>
> **FlagstoneUI Core** does not require tokens or catalog systems—themes are simply collections of styles.
>
> See [ADR011: Token System Repositioning](Decisions/adr011-token-system-repositioning.md) and [ADR010: Theme Contract System](Decisions/adr010-theme-contract-system.md) for architectural context.

This document describes the machine-readable token catalog system for token-based themes, designed to support AI agents, automated tooling, and theme generation.

## Overview

The token catalog system provides structured, machine-readable data about Flagstone UI's design tokens, controls, and theme contracts. This documentation is intended for **AI agents, automated tooling, and developers building tools** that integrate with Flagstone UI.

**For human-readable token documentation**, see [tokens.md](tokens.md) which provides complete descriptions, usage guidelines, design principles, and the layered architecture explanation.

This machine-readable system enables:

- **AI Agents**: Automated theme generation, style recommendations, and code generation
- **Developer Tooling**: IDE integration, documentation generation, and validation
- **Design Tools**: Import tokens into Figma, Sketch, or other design tools
- **Contract Validation**: Verify themes meet minimum requirements or design system standards

## Architecture: Three-Layer Model

The token catalog system implements a layered architecture for theme validation:

| Layer | Purpose | Contract | Required |
|-------|---------|----------|----------|
| **Styling Surface** | All styleable properties | N/A (extracted from source) | Informational |
| **Theme** | Minimum viable styling | `minimal.json` | Implicit styles for all controls |
| **Design System** | Complete style variants | e.g., `material.json` | Named variants (OutlinedButton, etc.) |

See [ADR010: Theme Contract System](Decisions/adr010-theme-contract-system.md) for full details.

## Files

### Core Catalog Files

- **`tokens-catalog.json`** - Machine-readable token catalog
  - All base design tokens (colors, spacing, typography, etc.)
  - Control-specific styling properties and recommendations
  - Common style patterns for each control
  - Theme variant definitions
  - Structured for AI/tooling consumption

- **`tokens-schema.json`** - JSON Schema for token catalog validation
  - Validates `tokens-catalog.json` structure
  - Enables IDE auto-completion
  - Documents expected data structure

### Contract Files (New)

- **`contracts/minimal.json`** - Minimum viable theme contract
  - Requires implicit styles for all Fs* controls
  - Defines token schema requirements
  - Base contract that all themes must satisfy

- **`contracts/material.json`** - Material Design 3 design system contract
  - Extends `minimal` with named style requirements
  - Specifies variants: `OutlinedButton`, `TextButton`, `ElevatedCard`, etc.
  - Example of a complete design system contract

- **`schemas/design-system-contract.schema.json`** - Contract schema
  - Validates contract JSON structure
  - Documents contract format
  - Supports contract inheritance (`extends`)

## CLI Tool: FlagstoneUI.TokenGenerator

The `FlagstoneUI.TokenGenerator` tool provides commands for catalog generation, validation, and contract management.

### Installation

```bash
# Run from repository root
dotnet run --project tools/FlagstoneUI.TokenGenerator -- <command>

# Or build and run
dotnet build tools/FlagstoneUI.TokenGenerator
./tools/FlagstoneUI.TokenGenerator/bin/Debug/net10.0/FlagstoneUI.TokenGenerator <command>
```

### Commands

#### generate - Generate token catalog (legacy)

```bash
flagstone-tokens generate --source ./src --output ./docs/tokens-catalog.json
```

#### validate - Validate theme tokens

```bash
flagstone-tokens validate --input Theme.xaml
flagstone-tokens validate --input tokens.json --format json --json
```

#### generate-xaml - Generate XAML from JSON catalog

```bash
flagstone-tokens generate-xaml --input tokens-catalog.json --output Tokens.xaml
```

#### extract-surface - Extract control styling surface

```bash
flagstone-tokens extract-surface --controls ./src/FlagstoneUI.Core/Controls
```

Output shows all BindableProperty declarations:
```
📦 FsButton
   Inherits: Microsoft.Maui.Controls.Button
   Architecture: subclass
   Styled Properties (10):
      • BackgroundColor (color) → Color.Primary
      • BorderColor (color) → Color.Outline
      • CornerRadius (int) → Radius.Button.Medium
      ...
```

#### generate-contract - Generate design system contract

```bash
# Generate minimal contract from source
flagstone-tokens generate-contract \
  --name minimal \
  --source ./src/FlagstoneUI.Core/Controls \
  --output ./docs/contracts/minimal.json

# Generate design system contract from theme
flagstone-tokens generate-contract \
  --name material \
  --theme ./src/FlagstoneUI.Themes.Material/Theme.xaml \
  --extends minimal \
  --output ./docs/contracts/material.json
```

#### validate-contract - Validate theme against contract

```bash
# Validate theme complies with contract
flagstone-tokens validate-contract \
  --theme ./src/FlagstoneUI.Themes.Material/Theme.xaml \
  --contract ./docs/contracts/material.json

# JSON output for CI/CD
flagstone-tokens validate-contract \
  --theme Theme.xaml \
  --contract minimal.json \
  --json
```

Output:
```
🔍 Flagstone UI Contract Validator
   Theme:    Theme.xaml
   Contract: material.json

Contract: material (design-system layer)
Status: ✅ Valid

Coverage:
  • Implicit styles: 4
  • Named styles: 9
  • Tokens: 0

✅ Theme complies with contract!
```

## Using the Token Catalog

### For AI Agents

The token catalog and contract system enable sophisticated AI workflows:

**Theme Generation Workflow:**
1. Parse `tokens-catalog.json` to understand available tokens
2. Extract styling surface with `extract-surface` command
3. Generate theme XAML with custom token values
4. Validate against `minimal.json` contract for completeness
5. Optionally validate against design system contract

**Contract-Aware Generation:**
```bash
# 1. Extract what can be styled
flagstone-tokens extract-surface --controls ./src/FlagstoneUI.Core/Controls

# 2. Generate theme (AI or manual)

# 3. Validate theme meets requirements
flagstone-tokens validate-contract --theme MyTheme.xaml --contract minimal.json
```

**Token Catalog Data:**
- Control `styledProperties` show what can be customized
- `recommendedToken` hints at semantic token usage
- `commonStyles` provide proven patterns

### For Developers

```bash
# Validate the catalog against schema
jsonschema -i tokens-catalog.json tokens-schema.json

# Query control properties
cat tokens-catalog.json | jq '.controls.FsButton.styledProperties'

# Validate your theme meets minimum requirements
flagstone-tokens validate-contract --theme MyTheme.xaml --contract minimal.json

# Check against full design system contract
flagstone-tokens validate-contract --theme MyTheme.xaml --contract material.json
```

### For Theme Authors

When creating a new theme:

1. **Start with the styling surface** - understand what can be styled
2. **Define core tokens** - colors, spacing, typography, etc.
3. **Create implicit styles** - default styles for all Fs* controls
4. **Validate against minimal contract** - ensure basic completeness
5. **Optionally add named variants** - OutlinedButton, ElevatedCard, etc.
6. **Validate against design system contract** - ensure variant completeness

### For Designers

The catalog can be:

- Imported into design tools (Figma, Sketch)
- Used to generate design system documentation
- Validated against design specifications
- Exported to other design token formats

## Catalog Structure

### Base Tokens

```json
{
  "baseTokens": {
    "colors": { "Color.Primary": { "type": "color", "defaultValue": "#6750A4", ... }},
    "spacing": { "Space.16": { "type": "spacing", "value": 16, ... }},
    "typography": { "FontSize.BodyMedium": { "type": "fontSize", "value": 14, ... }},
    // ... other token categories
  }
}
```

### Control Definitions

```json
{
  "controls": {
    "FsButton": {
      "inheritsFrom": "Microsoft.Maui.Controls.Button",
      "architecture": "subclass",
      "styledProperties": [
        {
          "name": "BackgroundColor",
          "type": "color",
          "recommendedToken": "Color.Primary",
          "bindable": true
        }
      ],
      "commonStyles": [
        {
          "name": "PrimaryButton",
          "tokens": { "BackgroundColor": "Color.Primary", ... }
        }
      ]
    }
  }
}
```

## Maintenance

### Current Status

**Version**: 0.2.0 (POC)  
**Last Updated**: January 2026  
**Maintenance**: Semi-automated via CLI tooling

The `FlagstoneUI.TokenGenerator` tool provides comprehensive functionality for catalog and contract management.

### Tooling Status

| Command | Status | Notes |
|---------|--------|-------|
| `generate` | ✅ Functional | Legacy catalog generation |
| `validate` | ✅ Functional | Basic XAML/JSON validation |
| `generate-xaml` | ✅ Functional | Catalog → XAML conversion |
| `extract-surface` | ✅ Functional | Extract control styling properties |
| `generate-contract` | ✅ Functional | Generate minimal or design system contracts |
| `validate-contract` | ✅ Functional | Validate theme against contract |

**Not Yet Implemented:**
- ❌ CI/CD integration (see note below)
- ❌ Published as .NET global tool
- ❌ NuGet/URL contract resolution

### CI/CD Integration Note

Contract validation in CI/CD is more appropriate for **theme library repositories** than the core FlagstoneUI repo:

- **FlagstoneUI repo**: Maintains contracts and tooling
- **Theme repos**: Use contracts to validate their themes meet requirements
- **Design system repos**: Validate against design system contracts

Example CI workflow for a theme repository:
```yaml
- name: Validate Theme Contract
  run: |
    dotnet tool install -g FlagstoneUI.TokenGenerator
    flagstone-tokens validate-contract \
      --theme ./src/MyTheme/Theme.xaml \
      --contract minimal \
      --json
```

### Maintenance Responsibility

**Who**: Repository maintainers

**When to Update Contracts**:
- ✏️ New Fs* controls are added
- ✏️ Control styled properties change
- ✏️ Design system requirements evolve

**When to Regenerate Contracts**:
```bash
# After control changes
flagstone-tokens generate-contract --name minimal --source ./src/FlagstoneUI.Core/Controls --output ./docs/contracts/minimal-generated.json

# After theme style changes  
flagstone-tokens generate-contract --name material --theme ./src/FlagstoneUI.Themes.Material/Theme.xaml --extends minimal --output ./docs/contracts/material-generated.json
```

## Integration Points

### Bootstrap Converter Integration

The FlagstoneUI Bootstrap Converter can output themes that comply with contracts:

```bash
# Convert Bootstrap theme with contract compliance
flagstone-bootstrap convert \
  --input bootstrap-theme.css \
  --output ./MyTheme/Theme.xaml \
  --validate-contract minimal

# The converter can ensure:
# - All Fs* controls have implicit styles
# - Token mappings follow recommendations
# - Output is contract-compliant
```

See [bootstrap-converter-enhancement-plan.md](bootstrap-converter-enhancement-plan.md) for integration plans.

### MCP Server (Future)

The token catalog and contracts will serve as data sources for a Model Context Protocol (MCP) server:

```json
{
  "name": "flagstone-tokens",
  "version": "0.2.0",
  "resources": {
    "tokens": "file://docs/tokens-catalog.json",
    "contracts": "file://docs/contracts/",
    "schema": "file://docs/schemas/"
  }
}
```

AI agents will be able to:

- Query available tokens and styling surface
- Validate theme definitions against contracts
- Generate contract-compliant themes
- Suggest improvements based on contracts
- Convert between design formats

### CI/CD Integration (Future)

```yaml
# Example GitHub Actions workflow
- name: Generate Token Catalog
  run: dotnet flagstone-tokens generate
  
- name: Validate Token Catalog
  run: jsonschema -i docs/tokens-catalog.json docs/tokens-schema.json
  
- name: Commit if changed
  run: |
    git diff --exit-code docs/tokens-catalog.json || \
    (git add docs/tokens-catalog.json && git commit -m "Update token catalog")
```

## Validation

### Schema Validation

Ensure the catalog conforms to the schema:

```bash
# Using ajv-cli (npm install -g ajv-cli)
ajv validate -s tokens-schema.json -d tokens-catalog.json

# Using python jsonschema
python -m jsonschema -i tokens-catalog.json tokens-schema.json
```

### Manual Checks

When updating manually:

1. ✅ All token names match XAML resource keys
2. ✅ Color values are valid hex codes
3. ✅ Numeric values match XAML values
4. ✅ Recommended tokens reference existing tokens
5. ✅ Control properties match actual implementations
6. ✅ Version and lastUpdated are current

## Contributing

When adding new tokens or controls:

1. Update the XAML implementation first
2. Update `tokens-catalog.json` to match
3. Update `tokens.md` with human-readable documentation
4. Add/update control documentation in `Controls/`
5. Validate against schema
6. Update version and lastUpdated date

## Questions?

For questions about the token catalog system:

- 📖 See [`tokens.md`](tokens.md) for human-readable documentation
- 🔧 Check [`tokens-schema.json`](tokens-schema.json) for technical structure
- 💬 Open an issue on [GitHub](https://github.com/matt-goldman/flagstone-ui)

---

*This catalog system is designed to be AI-friendly while remaining human-readable. It bridges the gap between design intent and implementation reality.*
