# Token Catalog System

This document describes the machine-readable token catalog system for Flagstone UI, designed to support AI agents, automated tooling, and theme generation.

## Overview

The token catalog system provides structured, machine-readable data about Flagstone UI's design tokens and controls. This documentation is intended for **AI agents, automated tooling, and developers building tools** that integrate with Flagstone UI.

**For human-readable token documentation**, see [tokens.md](tokens.md) which provides complete descriptions, usage guidelines, and design principles for designers and developers.

This machine-readable system enables:

- **AI Agents**: Automated theme generation, style recommendations, and code generation
- **Developer Tooling**: IDE integration, documentation generation, and validation
- **Design Tools**: Import tokens into Figma, Sketch, or other design tools

## Files

### Core Catalog Files

- **`tokens-catalog.json`** - Machine-readable token catalog
  - All base design tokens (colors, spacing, typography, etc.)
  - Control-specific styling properties and recommendations
  - Common style patterns for each control
  - Theme variant definitions
  - Structured for AI/tooling consumption

- **`tokens-schema.json`** - JSON Schema for validation
  - Validates `tokens-catalog.json` structure
  - Enables IDE auto-completion
  - Documents expected data structure
  - Can be used by validators and linters

## Using the Token Catalog

### For AI Agents

The `tokens-catalog.json` file provides structured data for:

- **Theme Generation**: AI can generate complete themes by overriding base tokens
- **Style Recommendations**: Common styles show recommended token combinations
- **Property Validation**: Check if token types match property requirements
- **Code Generation**: Generate XAML styles programmatically

Example AI workflow:

1. Parse `tokens-catalog.json` to understand available tokens
2. Read control `styledProperties` to see what can be customized
3. Review `commonStyles` for proven patterns
4. Generate theme override with custom token values
5. Validate against `tokens-schema.json`

### For Developers

```bash
# Validate the catalog against schema (requires JSON schema validator)
jsonschema -i tokens-catalog.json tokens-schema.json

# Use in tooling
cat tokens-catalog.json | jq '.controls.FsButton.styledProperties'
```

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

**Version**: 0.1.0 (MVP)  
**Last Updated**: 2025-11-01  
**Maintenance**: Manual (for now)

The catalog is currently maintained manually to establish the structure and validate the approach.

### Future Automation

A .NET tool is planned for automated catalog generation:

```bash
# Planned future usage
dotnet tool install -g FlagstoneUI.TokenGenerator
flagstone-tokens generate --source ./src --output ./docs/tokens-catalog.json
```

**Benefits of automation:**

- ✅ Single source of truth (XAML files)
- ✅ No manual sync needed
- ✅ Can run in CI/CD pipeline
- ✅ Validates XAML against schema
- ✅ Can be integrated into theme generator tool

### When to Update

Update the catalog when:

- ✏️ New controls are added
- ✏️ Control properties change
- ✏️ New base tokens are added
- ✏️ Common styles are introduced
- ✏️ New theme variants are created

## Integration Points

### MCP Server (Future)

The token catalog will serve as the data source for a Model Context Protocol (MCP) server:

```json
{
  "name": "flagstone-tokens",
  "version": "0.1.0",
  "resources": {
    "tokens": "file://docs/tokens-catalog.json",
    "schema": "file://docs/tokens-schema.json"
  }
}
```

AI agents will be able to:

- Query available tokens
- Validate theme definitions
- Generate code from tokens
- Suggest improvements
- Convert between formats

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
