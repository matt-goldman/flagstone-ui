# FlagstoneUI Documentation

Welcome to the FlagstoneUI documentation! This directory contains comprehensive guides, references, and technical documentation for building apps with FlagstoneUI.

> 👈 **New here?** Start with the [main README](../README.md) for a project overview, code examples, and quick start instructions.

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

- **[Design Tokens Reference](tokens.md)** - Complete human-readable token documentation
  - Color, spacing, typography, shape tokens
  - Usage guidelines and design principles
  - Semantic meaning of each token

- **[Token Catalog System](token-catalog-system.md)** - Machine-readable token architecture
  - For AI agents and automated tooling
  - JSON Schema and validation
  - Automation and integration plans

### Control Documentation

- **[FsButton](Controls/FsButton.md)** - Button control
- **[FsCard](Controls/FsCard.md)** - Card container control
- **[FsEntry](Controls/FsEntry.md)** - Single-line text entry control
- **[FsEditor](Controls/FsEditor.md)** - Multi-line text editor control

## Architecture & Technical Documentation

- **[Architecture Overview](architecture.md)** - System architecture and design decisions
- **[Control Implementation Guide](control-implementation-guide.md)** - Guide for implementing new controls
- **[Roadmap](roadmap.md)** - Project roadmap and planned features

## Integrations

- **[MAUI Community Toolkit Integration](mct-integrations.md)** - Optional MCT integration package
  - ValidationBehaviorAdapter for FsEntry
  - FsEditorBorderAnimation for animated borders
  - Examples and best practices

## Advanced Topics

- **[Visual State Pattern](visual-state-pattern.md)** - Theme-driven visual states for controls
- **[Unit Testing Guide](unit-testing-guide.md)** - Testing MAUI UI components

### Sample Applications

- **[Sample App](../samples/FlagstoneUI.SampleApp/)** - Comprehensive showcase of all controls
- **[Theme Playground](../samples/FlagstoneUI.ThemePlayground/)** - Test and experiment with themes

## Project Status

- **[Implementation Status](implementation-status.md)** - Current completion tracking
- **[Roadmap](roadmap.md)** - Planned features and phases
- **Decisions/** - Architecture Decision Records (ADRs)

## Contributing

When adding new controls or tokens:

1. Review the [Control Implementation Guide](control-implementation-guide.md)
2. Update XAML implementation first
3. Add/update control documentation in `Controls/`
4. Update token documentation if adding new tokens
5. Add tests following the [Unit Testing Guide](unit-testing-guide.md)

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

*FlagstoneUI - A token-based, themeable UI framework for .NET MAUI*
