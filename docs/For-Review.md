# Documentation Items For Review

This document lists items that require clarification or decisions from the project maintainer during the documentation review process.

## 1. FlagstoneUIBuilder Status and Intent

**Location:** Multiple files reference `FlagstoneUIBuilder`

**Issue:** The implementation in `src/FlagstoneUI.Core/Builders/FlagstoneUIBuilder.cs` contains only a minimal `UseDefaultTheme()` method, while documentation suggests a richer fluent API.

**Documentation References:**
- `technical-plan.md` - Shows planned API with `UseFlagstoneUI(ui => { ... })`
- `architecture.md` - States "Builder API: Removed" then later "Builder API: Incomplete Builder API"
- `implementation-status.md` - Lists as "Removed - No longer necessary expansion"
- `roadmap.md` - Lists "Enhanced Builder API" as Phase 1.5 item

**Question:** 
- Is the builder pattern still planned for future implementation?
- Should references to it be removed/updated in quickstart.md and other docs?
- What is the recommended way for consumers to configure Flagstone UI?

## 2. Resource Loading Architecture Description

**Location:** `architecture.md`

**Issue:** Section "1. Resource Loading ✅ RESOLVED" contains extensive details about a previous problem and workaround. This is historical information that clutters the current architecture documentation.

**Question:**
- Should this historical context be moved to an ADR (Architecture Decision Record)?
- Or should it be simplified to just mention the current working solution?

## 3. Control Implementation Status Discrepancies

**Location:** Multiple status tracking files

**Issue:** Different documents report different completion percentages:
- `roadmap.md` - "Phase 1 (MVP) Progress: ~85% Complete"
- `implementation-status.md` - "Current Progress: ~85% complete"
- `roadmap.md` also states "Phase 1 (MVP) Progress: ~85% Complete" in "Current Status (November 2025)" section

**Actual Status from Code Review:**
- ✅ FsButton - Implemented (simple subclass)
- ✅ FsCard - Implemented (complete with elevation)
- ✅ FsEntry - Implemented (wrapper with BorderlessEntry)
- ❌ FsSwitch - Not implemented
- ❌ Snackbar - Not implemented

**Question:**
- What is the current accurate completion percentage?
- Should we update all status indicators to match?
- Is FsSwitch still planned for MVP or moved to Phase 1.5?

## 4. Theme System Description Clarity

**Location:** `quickstart.md`, `theming-guide.md`

**Issue:** Instructions for adding themes to App.xaml show using:
```xml
<material:Theme />
```

But it's not immediately clear that this requires:
1. XAML namespace declaration for Material theme
2. Project reference to FlagstoneUI.Themes.Material

**Question:**
- Should quickstart.md include the complete App.xaml example with namespace declarations?
- Should we add a troubleshooting section for common setup issues?

## 5. MCP Bootstrap Converter Documentation

**Location:** `docs/mcp-bootstrap-converter.md`

**Issue:** Document appears incomplete - cuts off mid-sentence at line 200. Last visible section is "Available Bootstrap Themes (Bootswatch)" heading with no content.

**Question:**
- Is this document still under development?
- Should it be marked as draft or moved to a different location?
- What completion percentage should be shown?

## 6. Dark Mode Token Naming Convention

**Location:** Multiple files describe dark mode tokens

**Issue:** Documentation describes using `.Dark` suffix for dark mode colors (e.g., `Color.Primary.Dark`), but it's unclear if these are:
- Separate resource keys that consumers must define
- Automatically generated
- Optional or required for themes

**Documentation shows:**
```xml
<Color x:Key="Color.Primary.Dark">#D0BCFF</Color>
```

But actual Tokens.xaml doesn't contain `.Dark` variants - these appear to be theme-specific.

**Question:**
- Should token documentation clarify that `.Dark` variants are theme-specific, not in base tokens?
- Should we show both where base tokens live vs. where themes override them?

## 7. Community Toolkit Integration Clarity

**Location:** `quickstart.md`, `FsEntry.md`, ADR001

**Issue:** Multiple places mention Community Toolkit integration:
- Some say "MAUI Community Toolkit included automatically via Directory.Build.props"
- ADR001 explicitly states "Do not add a hard dependency on CommunityToolkit.Maui to FlagstoneUI.Core"
- FsEntry.md notes "The MAUI Community Toolkit is not a dependency of FlagstoneUI.Core"

**Question:**
- Is CommunityToolkit.Maui included via Directory.Build.props or not?
- If yes, is this just for sample apps and tests, not for the Core library?
- Should documentation be more explicit about this distinction?

## 8. Copilot Instructions vs Repository Documentation

**Location:** `.github/copilot-instructions.md`

**Issue:** The copilot instructions file is extensive and describes the project architecture, but it may duplicate information in other docs. It also contains some statements that may be outdated.

**Question:**
- Should copilot-instructions.md be reviewed for accuracy separately?
- Or is it intentionally maintained separately as a stable reference for AI agents?

## 9. Historical Documents Purpose

**Location:** 
- `docs/technical-plan.md` - Labeled "This document reflects the original planning"
- `docs/draft-spec-project-pan.md` - Labeled "This document contains the original project specification"

**Issue:** Both documents have notes directing readers to current documentation, but they still exist in the main docs folder where they may confuse new contributors.

**Question:**
- Should these be moved to a `docs/historical/` or `docs/archive/` folder?
- Or should they be removed entirely if they're superseded by current docs?

## 10. Prompt Documentation Purpose

**Location:** `docs/prompts/` directory

**Issue:** Contains prompts for AI agents to generate themes. Purpose isn't immediately clear for human developers browsing documentation.

**Question:**
- Should prompts directory have a more prominent README explaining its purpose?
- Is this intended for end users, contributors, or internal tooling?

## 11. Token Catalog Maintenance

**Location:** `docs/token-catalog-system.md`, `docs/README.md`

**Issue:** Documents describe the token catalog as "currently maintained manually" with plans for automation via a .NET tool (FlagstoneUI.TokenGenerator).

The tool directory exists at `tools/FlagstoneUI.TokenGenerator/` but its status is unclear.

**Question:**
- Is the TokenGenerator tool functional?
- Should documentation be updated to reflect current automation status?
- Who is responsible for keeping tokens-catalog.json in sync?

## 12. Project Structure Description Accuracy

**Location:** Root `README.md` shows:
```
flagstone-ui/
├── src/
│   ├── FlagstoneUI.Core/
│   ├── FlagstoneUI.Themes.Material/
│   └── FlagstoneUI.Blocks/        # "Reusable app screens (future)"
```

**Issue:** The Blocks project exists but contains minimal content. Status is unclear.

**Question:**
- Is FlagstoneUI.Blocks still planned or on hold?
- Should README reflect current vs. planned structure more clearly?
