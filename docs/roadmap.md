# FlagstoneUI Roadmap (Updated)

> **⚠️ Architecture Update (February 2026)**: The token system has been repositioned following ADR011. FlagstoneUI is now clearly established as the **unified styling plane for .NET MAUI** (Controls → Styles → Themes). The token system is an optional implementation detail, not a core requirement. References to "token-first design" in this roadmap reflect the pre-ADR011 understanding. See [ADR011: Token System Repositioning](Decisions/adr011-token-system-repositioning.md) for details.

This document provides an updated roadmap based on current implementation progress and lessons learned.

## Current Status (December 2025)

**Phase 1 (MVP) Progress**: ~98% Complete

### Recently Completed ✅

- [x] Solution structure and build system
- [x] Design token system (Tokens.xaml)
- [x] Card control implementation
- [x] Basic CI/CD pipeline
- [x] ThemeLoader utility
- [x] Material theme foundation
- [x] Test project structure
- [x] **FsEntry control** with visual state support
- [x] **FsButton control** (basic implementation)
- [x] **FsEditor control** with themable properties
- [x] **Visual State Pattern** - Theme-driven state styling
- [x] **BorderlessEntry handlers** - Platform-specific native styling removal
- [x] **Sample App** - Working demonstration with Controls showcase
- [x] **CSS-aligned Visual States** - Empty Normal state pattern for future conversion tooling
- [x] **Bootstrap Converter Library** - Complete SCSS/CSS parsing and token mapping
- [x] **Bootstrap Converter CLI** - Command-line tool with convert/info commands
- [x] **Bootstrap Converter UI App** - MAUI desktop app with visual conversion and live preview
- [x] **Font Handling** - Detection, download, and registration instructions
- [x] **Save/Export** - FileSaver integration with folder picker

### Current Focus 🎯

#### Post-Integration MCP Development

The project has successfully completed the Bootstrap Converter UI app as a quality-of-life tool for validating the converter. The next priority is to create a comprehensive **FlagstoneUI MCP Server** that provides agent-friendly functionality.

**Strategic Vision**:

1. **Agent-Assisted Development**: Enable AI agents to generate complete FlagstoneUI interfaces through natural language
2. **Theme Intelligence**: Provide tools for theme conversion, analysis, and generation
3. **System Understanding**: Make FlagstoneUI architecture and patterns accessible to agents
4. **Learning by Example**: Use Bootstrap conversions to teach agents how FlagstoneUI works
5. **Rapid Prototyping**: Accelerate development through AI-assisted code generation

**Current Tasks**:
- Merge UI app changes to main branch ✅
- Integrate changes from other branches 🚧
- Plan comprehensive FlagstoneUI.Mcp server architecture 🚧
- Remove temporary BootstrapConverter.Mcp test project 📋

### Completed Milestone: Bootstrap Theme Converter ✅

**Status**: COMPLETE (December 12, 2025)

- [x] **Bootstrap → Flagstone Converter Library**
  - ✅ Parse Bootstrap SCSS variables (`$primary`, `$font-size-base`, etc.)
  - ✅ Parse CSS classes (`.btn-primary`, `.card`) - limited by ExCSS for Bootstrap 5+
  - ✅ Multi-mode analysis: variables (recommended), css, hybrid
  - ✅ Generate `Tokens.xaml` with 20+ mapped tokens (colors, spacing, typography, borders)
  - ✅ Generate `Styles.xaml` with control variants (FsButton, FsEntry, FsEditor, FsCard)
  - ✅ Generate `Theme.xaml` with merged resource dictionaries
  - ✅ Multi-file SCSS parsing with variable resolution
  - ✅ Dark mode color variant generation
  - ✅ Font detection with download URLs and registration instructions
  - ✅ Output format options: XAML and C#
  - ✅ Validated with Bootswatch themes (Darkly, Flatly, Brite, and others)
  - ✅ **Per-edge border support**: BorderTopWidth/RightWidth/BottomWidth/LeftWidth extraction and generation
  - ✅ **Shadow support**: Box-shadow extraction from variables and CSS, MAUI Shadow resource generation
  - ✅ **AppThemeBinding for light/dark mode**: CSS custom property extraction, .Dark token generation, adaptive theme bindings

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

### Next Milestone: Agent Guidance & Developer Experience 🚀

**Target**: Q1 2026

**Purpose**: Enable AI-assisted and human developer productivity with FlagstoneUI

**Strategy Decision**: AGENTS.md documentation file instead of MCP server (see ADR009)

**Rationale**:
- ✅ Works with ANY AI tool (not just MCP-compatible clients)
- ✅ Lives in repo (discoverable, version-controlled)
- ✅ Can be included in .NET templates
- ✅ Zero infrastructure to maintain
- ✅ Easier to update as patterns evolve

**Planned Deliverables**:

- [ ] **AGENTS.md Documentation**
  - Architecture overview (token-first design, available controls)
  - Code generation patterns (semantic color usage, token bindings)
  - Real code examples of common patterns
  - Available CLI tools (Bootstrap converter, Tailwind palette converter)
  - DO/DON'T anti-patterns
  - Integration guidance (Crosswind, design tools)

- [ ] **Bootstrap Converter as .NET CLI Tool**
  - Already complete as library and CLI
  - Publish as global tool: `dotnet tool install -g flagstone-bootstrap`
  - Update documentation for global tool usage

- [ ] **.NET Project Templates**
  - FlagstoneUI app template with sample content
  - Include AGENTS.md
  - Opinionated patterns and best practices
  - Integration with broader full-stack solution

**Future Considerations**:

- [ ] **Figma → FlagstoneUI Converter** (Q2 2026)
  - Convert Figma design tokens to FlagstoneUI themes
  - Bridge designer → developer workflow
  - Figma is industry-standard design tool
  - More valuable than Bootstrap converter for real-world design systems
  - Implementation: Figma Plugin + .NET CLI tool, or Figma API client

- [ ] **Crosswind Integration**
  - Document integration patterns in AGENTS.md
  - Leverage Steven Thewissen's Tailwind-to-MAUI work
  - Complementary to FlagstoneUI (utility classes + design tokens)

**Benefits**:
- ✅ Universal AI tool compatibility
- ✅ Template-based rapid project setup
- ✅ Designer-developer workflow (Figma converter)
- ✅ Comprehensive guidance for humans and AI
- ✅ No infrastructure maintenance burden
3. Theme marketplace integration
4. VS Code extension (MCP-based)
