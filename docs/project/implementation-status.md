# Implementation Status

This document tracks the current implementation status of FlagstoneUI components.

*Last Updated: March 2026*

## Phase 1: Proof of Concept (POC) — Status

### Completed ✅

| Component | Status | Notes |
|---|---|---|
| **Solution Structure** | ✅ Complete | Projects, directories, and build files properly structured |
| **Tokens.xaml** | ✅ Complete | Optional design tokens with colours, spacing, radii, typography |
| **FsButton Control** | ✅ Complete | Subclass of Button with themable properties |
| **FsCard Control** | ✅ Complete | Full implementation with elevation (shadow support), corner radius, and all styling properties |
| **FsEntry Control** | ✅ Complete | Wrapper implementation with visual state support, behaviour passthrough, and platform styling removal |
| **FsEditor Control** | ✅ Complete | Wrapper implementation with themable properties and border animation support |
| **Material Theme** | ✅ Complete (Example) | Example theme using tokens as an implementation detail; not a required baseline |
| **Sample App** | ✅ Complete | Working demonstration with controls showcase and MCT integration |
| **Bootstrap Converter Library** | ✅ Complete | Class library with SCSS parsing, CSS analysis, token mapping, XAML/C# generation, font handling |
| **Bootstrap Converter CLI** | ✅ Complete | Command-line tool with convert/info commands, multi-mode analysis, comprehensive options |
| **Bootstrap Converter UI App** | ✅ Complete | .NET MAUI desktop app with visual conversion, live preview, save/export functionality |
| **Bootstrap Converter Tests** | ✅ Complete | Unit tests with Bootswatch theme fixtures, validated end-to-end pipeline |
| **Font Handling System** | ✅ Complete | Detection, download URLs, registration instructions for theme fonts |
| **Save/Export Functionality** | ✅ Complete | Folder picker integration with CommunityToolkit.Maui.Storage.FileSaver |
| **CI/CD Pipeline** | ✅ Partial | Basic workflow exists; builds and tests pass (see known issues) |
| **Resource References** | ✅ Complete | Achieved through XAML resource dictionaries |
| **Quickstart Docs** | ✅ Complete | Documentation reviewed and published |
| **Theming Guide** | ✅ Complete | Validated with external developers and designers |
| **AGENTS.md Documentation** | ✅ Complete | Comprehensive AI agent guidance (architecture, patterns, examples, tools) |
| **ThemeLoader** | ❌ Removed | Completed but not needed; removed (YAGNI) |
| **FlagstoneUIBuilder** | ❌ Removed | No longer necessary; removed (YAGNI) |

### In Progress 🚧

| Component | Status | Next Steps |
|---|---|---|
| **Documentation reorganisation** | 🚧 In Progress | Restructuring docs into subdirectories, updating internal links |

### Planned 📋

| Component | Required for MVP | Priority | Notes |
|---|---|---|---|
| **FsSwitch** | No | Medium | Enhanced switch with improved theming |
| **Bootstrap CLI Global Tool** | No | High | Publish Bootstrap converter as global .NET CLI tool (`dotnet tool install -g flagstone-bootstrap`) |
| **.NET Project Templates** | No | High | FlagstoneUI app templates with AGENTS.md and opinionated patterns |
| **Figma → FlagstoneUI Converter** | No | Medium | Convert Figma design tokens to FlagstoneUI themes (designer-developer workflow) |
| **Crosswind Integration Docs** | No | Low | Document integration patterns with Steven Thewissen's Tailwind-to-MAUI work |
| **FlagstoneUI MCP Server** | No | Deferred | Deferred in favour of AGENTS.md (see [ADR009](../decisions/adr009-agent-guidance-strategy.md)); may reconsider if Figma converter proves valuable |

---

## Architecture Review

### Current Implementation vs. Planned

| Aspect | Planned | Current | Gap |
|---|---|---|---|
| **Control Strategy** | Neutral controls; platform styling removed | FsButton, FsCard, FsEntry, FsEditor implemented | Need post-MVP controls (FsSwitch, etc.) |
| **Theme System** | Cross-component styling with multiple approaches | Working — direct styling, implicit styles, explicit styles, tokens all supported | None |
| **Builder API** | Removed (YAGNI) | Removed | None |
| **Package Structure** | Separate theme packages | Basic structure exists | Ready for expansion |

---

## Milestone Progress

### Current Milestone: Developer Experience & Ecosystem 🎯

- **Target**: Q2–Q3 2026
- **Priority**: High

**Planned Deliverables**:
- [ ] Publish Bootstrap converter as global .NET CLI tool
- [ ] .NET project templates with opinionated patterns
- [ ] Additional controls (FsSwitch)

### Completed Milestones

#### Bootstrap Converter UI App ✅
- **Completed**: December 2025
- .NET MAUI desktop application for Windows/macOS
- Visual theme conversion with live preview
- Per-edge border support, shadow support, AppThemeBinding for light/dark mode

#### Agent Guidance & Developer Experience ✅
- **Completed**: Q1 2026
- AGENTS.md comprehensive documentation
- Architecture overview, patterns, examples, anti-patterns, CLI tools

---

## Known Issues

- Some UI component tests are filtered from CI due to headless environment limitations (see [ADR007](../decisions/adr007-ci-ui-test-strategy.md))
- ExCSS 4.2.3 does not parse CSS custom properties; Bootstrap 5+ CSS mode limited (use SCSS variables mode)
