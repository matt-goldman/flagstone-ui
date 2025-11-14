# Flagstone UI Documentation

Welcome to the Flagstone UI documentation! This directory contains comprehensive guides, references, and technical documentation for building apps with Flagstone UI.

> 👈 **New here?** Check the [main README](../README.md) for a quick overview, code examples, and how to run the samples.

## Getting Started

### For Developers

- **[Quickstart Guide](quickstart.md)** ⭐ **START HERE**
  - Quick setup and basic usage
  - Installation instructions
  - Using controls and themes
  - Complete examples

### For Designers

- **[Theming Guide](theming-guide.md)** 🎨 **FOR DESIGNERS**
  - Creating custom themes
  - Understanding the token system
  - Control properties reference
  - Sample theme documents
  - Designer-to-developer workflow

## Reference Documentation

### Token System

- **[Design Tokens Reference](tokens.md)** - Complete token documentation
  - Color, spacing, typography, shape tokens
  - Usage guidelines
  - Semantic meaning of each token

- **`tokens-catalog.json`** - Machine-readable token catalog
  - All base design tokens (colors, spacing, typography, etc.)
  - Control-specific styling properties and recommendations
  - Common style patterns for each control
  - Structured for AI/tooling consumption

- **`tokens-schema.json`** - JSON Schema for validation
  - Validates `tokens-catalog.json` structure
  - Enables IDE auto-completion

### Control Documentation

- **[FsButton](Controls/FsButton.md)** - Button control
- **[FsCard](Controls/FsCard.md)** - Card container control
- **[FsEntry](Controls/FsEntry.md)** - Text entry control

## Architecture & Technical Documentation

- **[Architecture Overview](architecture.md)** - System architecture and design decisions
- **[Control Implementation Guide](control-implementation-guide.md)** - Guide for implementing new controls
- **[Roadmap](roadmap.md)** - Project roadmap and planned features

## Advanced Topics

### Token Catalog System

The token catalog system provides machine-readable data about Flagstone UI's tokens and controls for AI agents, automated tooling, and design tool integration.

**Key files:**
- `tokens-catalog.json` - Machine-readable token catalog
- `tokens-schema.json` - JSON Schema for validation

**For complete details**, see the [Token Catalog System documentation](token-catalog-system.md).

### Sample Applications

- **[Sample App](../samples/FlagstoneUI.SampleApp/)** - Comprehensive showcase of all controls
- **[Theme Playground](../samples/FlagstoneUI.ThemePlayground/)** - Test and experiment with themes

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

## Getting Help

- 📖 **Documentation Issues?** Check the [quickstart](quickstart.md) or [theming guide](theming-guide.md)
- 💬 **Questions?** Open a [GitHub Discussion](https://github.com/matt-goldman/flagstone-ui/discussions)
- 🐛 **Found a Bug?** Report it on [GitHub Issues](https://github.com/matt-goldman/flagstone-ui/issues)
- 🤝 **Want to Contribute?** See [CONTRIBUTING.md](../CONTRIBUTING.md)

## Documentation Index

### Quick Links
- [Quickstart Guide](quickstart.md) - Get started in minutes
- [Theming Guide](theming-guide.md) - Create custom themes
- [Design Tokens](tokens.md) - Complete token reference
- [Architecture](architecture.md) - Technical architecture

### By Role
- **Developers**: Start with [quickstart.md](quickstart.md)
- **Designers**: Start with [theming-guide.md](theming-guide.md)
- **Contributors**: See [control-implementation-guide.md](control-implementation-guide.md)

---

*Flagstone UI - A token-based, themeable UI framework for .NET MAUI*
