# FlagstoneUI Roadmap

This document provides the current roadmap based on implementation progress and architectural decisions.

## Current Status (March 2026)

**Phase 1 (POC) Progress**: Complete ✅

FlagstoneUI is established as the **unified styling plane for .NET MAUI** — controls with full visual control from shared code. The token system is an optional implementation detail; see [ADR011: Token System Repositioning](../decisions/adr011-token-system-repositioning.md) for context.

### Completed ✅

- [x] Solution structure and build system
- [x] Design token system (Tokens.xaml, optional)
- [x] **FsButton control** — subclass with themable properties
- [x] **FsCard control** — full implementation with elevation and border support
- [x] **FsEntry control** — wrapper with visual state support and platform styling removal
- [x] **FsEditor control** — wrapper with themable properties and border animations
- [x] **Visual State Pattern** — theme-driven state styling
- [x] **BorderlessEntry/BorderlessEditor handlers** — platform-specific native styling removal
- [x] **Sample App** — working demonstration with controls showcase
- [x] **CSS-aligned Visual States** — empty Normal state pattern for future conversion tooling
- [x] **Bootstrap Converter Library** — complete SCSS/CSS parsing and token mapping
- [x] **Bootstrap Converter CLI** — command-line tool with convert/info commands
- [x] **Bootstrap Converter UI App** — MAUI desktop app with visual conversion and live preview
- [x] **Font Handling** — detection, download, and registration instructions
- [x] **Save/Export** — FileSaver integration with folder picker
- [x] **AGENTS.md** — comprehensive AI agent guidance (architecture, patterns, examples, tools)
- [x] **Basic CI/CD pipeline** — builds and tests on push and pull request
- [x] **Material theme** — example theme using tokens as an implementation detail

---

## Completed Milestone: Bootstrap Theme Converter ✅

**Status**: COMPLETE (December 2025)

- [x] **Bootstrap → FlagstoneUI Converter Library**
  - ✅ Parse Bootstrap SCSS variables (`$primary`, `$font-size-base`, etc.)
  - ✅ Parse CSS classes (`.btn-primary`, `.card`) — limited by ExCSS for Bootstrap 5+
  - ✅ Multi-mode analysis: variables (recommended), css, hybrid
  - ✅ Generate `Tokens.xaml` with 20+ mapped tokens (colours, spacing, typography, borders)
  - ✅ Generate `Styles.xaml` with control variants (FsButton, FsEntry, FsEditor, FsCard)
  - ✅ Generate `Theme.xaml` with merged resource dictionaries
  - ✅ Multi-file SCSS parsing with variable resolution
  - ✅ Dark mode colour variant generation
  - ✅ Font detection with download URLs and registration instructions
  - ✅ Output format options: XAML and C#
  - ✅ Validated with Bootswatch themes (Darkly, Flatly, Brite, and others)
  - ✅ Per-edge border support: BorderTopWidth/RightWidth/BottomWidth/LeftWidth extraction and generation
  - ✅ Shadow support: box-shadow extraction from variables and CSS, MAUI Shadow resource generation
  - ✅ AppThemeBinding for light/dark mode: CSS custom property extraction, .Dark token generation, adaptive theme bindings

- [x] **Bootstrap Converter CLI**
  - ✅ Convert command with comprehensive options
  - ✅ Info command for theme analysis
  - ✅ Multiple output formats (XAML, C#)
  - ✅ Verbose and debug logging modes
  - ✅ Analysis strategy selection (variables/css/hybrid)
  - ✅ Dark mode strategy configuration (auto/manual/none)

- [x] **Bootstrap Converter UI App** (MAUI Desktop)
  - ✅ Visual theme conversion interface
  - ✅ File picker and URL input modes
  - ✅ Live theme preview with FlagstoneUI controls showcase
  - ✅ Configuration panel (analysis modes, dark mode, namespace, output format)
  - ✅ Font detection and optional download functionality
  - ✅ Save/export with folder picker (CommunityToolkit.Maui.Storage integration)
  - ✅ Real-time conversion results and font registration instructions
  - ✅ Dynamic UI switching between file and URL modes

- [x] **Converter Documentation**
  - ✅ Library README with architecture, API, examples
  - ✅ CLI README with usage, analysis modes, integration guide
  - ✅ ADR005: Analysis modes architecture decision
  - ✅ Known limitations documented (ExCSS CSS custom properties)
  - ✅ Font handling documentation
  - ✅ Token mapping reference

---

## Completed Milestone: Agent Guidance & Developer Experience ✅

**Status**: COMPLETE (Q1 2026)

**Strategy**: AGENTS.md documentation file instead of MCP server (see [ADR009](../decisions/adr009-agent-guidance-strategy.md))

- [x] **AGENTS.md Documentation**
  - ✅ Architecture overview (styling surface, available controls)
  - ✅ Code generation patterns (semantic colour usage, token bindings)
  - ✅ Real code examples of common patterns
  - ✅ Available CLI tools (Bootstrap converter)
  - ✅ DO/DON'T anti-patterns
  - ✅ Integration guidance (MCT, design tools)

---

## Next Milestone: Developer Experience & Ecosystem 🚀

**Target**: Q2–Q3 2026

### Planned Deliverables

- [ ] **Bootstrap Converter as .NET Global CLI Tool**
  - Already complete as library and local CLI
  - Publish as global tool: `dotnet tool install -g flagstone-bootstrap`
  - Update documentation for global tool usage

- [ ] **.NET Project Templates**
  - FlagstoneUI app template with sample content and AGENTS.md
  - Opinionated patterns and best practices
  - Integration with broader full-stack solution

- [ ] **Additional Controls**
  - FsSwitch — enhanced switch with improved theming
  - Further controls based on community feedback

---

## Future Considerations

- [ ] **Figma → FlagstoneUI Converter** (Q3–Q4 2026)
  - Convert Figma design tokens to FlagstoneUI themes
  - Bridge designer → developer workflow
  - Figma is industry-standard design tool; more impactful than Bootstrap converter for real-world design systems
  - Implementation options: Figma Plugin + .NET CLI tool, or Figma API client

- [ ] **Crosswind Integration**
  - Document integration patterns with Steven Thewissen's Tailwind-to-MAUI work
  - Complementary to FlagstoneUI (utility classes + styling surface)

- [ ] **FlagstoneUI.Blocks** — pre-built screens (auth, CRUD, settings)

---

*Last Updated: March 2026*
