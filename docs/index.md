# FlagstoneUI Documentation

Welcome to the FlagstoneUI documentation. This directory contains guides, references, and technical documentation for building apps with FlagstoneUI.

> 👈 **New here?** Start with the [main README](../README.md) for a project overview, code examples, and quick start instructions.

## Getting Started

- **[Quickstart Guide](getting-started/quickstart.md)** ⭐ — Quick setup, installation, controls, and complete examples
- **[Architecture Overview](getting-started/architecture.md)** — System design, control architecture, and how themes work

## Guides

- **[Theming Guide](guides/theming-guide.md)** 🎨 — Creating custom themes (for designers and theme authors)
- **[Control Implementation Guide](guides/control-implementation-guide.md)** — Standards for implementing new controls
- **[Visual State Pattern](guides/visual-state-pattern.md)** — Theme-driven visual states for controls
- **[Unit Testing Guide](guides/unit-testing-guide.md)** — Testing .NET MAUI UI components
- **[Test Timeout Configuration](guides/test-timeout-configuration.md)** — Configuring test timeouts for CI

## Controls

- **[FsButton](controls/FsButton.md)** — Button control
- **[FsCard](controls/FsCard.md)** — Card container control
- **[FsEntry](controls/FsEntry.md)** — Single-line text entry control
- **[FsEditor](controls/FsEditor.md)** — Multi-line text editor control
- **[FsBorder](controls/FsBorder.md)** — Per-edge border support

## Reference

- **[Design Tokens](reference/tokens.md)** — Human-readable token documentation (colours, spacing, typography, shape)
- **[Token Catalog System](reference/token-catalog-system.md)** — Machine-readable token architecture for AI agents and tooling

## Integrations

- **[MAUI Community Toolkit Integration](integrations/mct-integrations.md)** — Optional MCT package (ValidationBehaviorAdapter, FsEditorBorderAnimation)
- **[Bootstrap Converter: Control Mapping](integrations/bootstrap-converter-control-mapping.md)** — Bootstrap-to-FlagstoneUI control reference
- **[Bootstrap Converter: Enhancement Plan](integrations/bootstrap-converter-enhancement-plan.md)** — Converter roadmap and planned improvements

## Project Status

- **[Roadmap](project/roadmap.md)** — Current milestone and planned features
- **[Blocks Roadmap](project/blocks-roadmap.md)** — Prioritised list of planned FlagstoneUI.Blocks components
- **[Implementation Status](project/implementation-status.md)** — Component completion tracking

## Architecture Decisions

The `decisions/` directory contains Architecture Decision Records (ADRs):

- [ADR001: FsEntry Behaviour & MCT Dependency](decisions/adr001-fsentry-behavior.md)
- [ADR002: Project Templates](decisions/adr002-project-templates.md)
- [ADR003: Button CornerRadius Type](decisions/adr003-button-corner-radius-type.md)
- [ADR004: Cross-Assembly Resource Loading](decisions/adr004-cross-assembly-resource-loading.md)
- [ADR005: Bootstrap Converter Analysis Modes](decisions/adr005-bootstrap-converter-analysis-modes.md)
- [ADR005a: Per-Edge Borders](decisions/adr005-per-edge-borders.md)
- [ADR005a (addendum): Per-Edge Borders Border Rendering Model](decisions/adr005_1-per-edge-borders-addendum.md)
- [ADR006: Native Pickers](decisions/adr006-native-pickers.md)
- [ADR007: CI/UI Test Strategy](decisions/adr007-ci-ui-test-strategy.md)
- [ADR008: Converter Advanced Features](decisions/adr008-converter-advanced-features.md)
- [ADR009: Agent Guidance Strategy](decisions/adr009-agent-guidance-strategy.md)
- [ADR010: Theme Contract System](decisions/adr010-theme-contract-system.md)
- [ADR011: Token System Repositioning](decisions/adr011-token-system-repositioning.md)
- [ADR012: FsShell — Stylable Shell Chrome via Subclass](decisions/adr012-fsshell.md)
- [ADR012 (addendum): Per-ShellItem Tab Bar Scoping](decisions/adr012_1-fsshell-per-item-bar-scoping-addendum.md)
- [ADR012 (addendum): FsShell Renderer Scope Narrowing](decisions/adr012_2-fsshell-renderer-scope-narrowing-addendum.md)
- [ADR012 (addendum): Bottom Chrome Height Resource Contract](decisions/adr012_3-fsshell-bottom-chrome-resource-contract-addendum.md)
- [ADR013: Shell Animations — Deferred to Consumer Implementations](decisions/adr013-shell-animations.md)
- [ADR013 (addendum): FsTabBar Built-in Selection Animation](decisions/adr013_1-fstabbar-built-in-selection-animation-addendum.md)

## Theme Prompts

The `prompts/` directory contains AI prompts for theme generation:

- [Prompts README](prompts/README.md)
- [Generate Modern Theme](prompts/generate-modern-theme.md)
- [Generate Theme Variations](prompts/generate-theme-variations.md)

## Sample Applications

- **[Sample App](../samples/FlagstoneUI.SampleApp/)** — Comprehensive showcase of all controls and themes
- **[Theme Playground](../samples/FlagstoneUI.ThemePlayground/)** — Test and experiment with custom themes

## Contributing

When adding new controls or tokens:

1. Review the [Control Implementation Guide](guides/control-implementation-guide.md)
2. Update the XAML implementation
3. Add control documentation in `controls/`
4. Update token documentation if adding new tokens
5. Add tests following the [Unit Testing Guide](guides/unit-testing-guide.md)

## Getting Help

- 📖 **Documentation issues?** Check the [quickstart](getting-started/quickstart.md) or [theming guide](guides/theming-guide.md)
- 💬 **Questions?** Open a [GitHub Discussion](https://github.com/matt-goldman/flagstone-ui/discussions)
- 🐛 **Found a bug?** Report it on [GitHub Issues](https://github.com/matt-goldman/flagstone-ui/issues)
- 🤝 **Want to contribute?** See [CONTRIBUTING.md](../CONTRIBUTING.md)

---

*FlagstoneUI — A unified styling plane for .NET MAUI*
