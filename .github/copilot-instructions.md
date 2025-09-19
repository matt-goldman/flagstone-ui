# Copilot Instructions for Flagstone UI

This guide helps AI coding agents work productively in the Flagstone UI codebase. It summarizes architecture, workflows, and conventions unique to this project.

## Project Overview

- **Flagstone UI** is a cross-platform, open-source UI kit and framework for .NET MAUI.
- The repo is organized into modular projects for core controls, themes, blocks (app screens), samples, and tests.
- Status: early WIP; structure will evolve.

## Key Directories & Components

- `src/FlagstoneUI.Core/`: Core UI library. Key areas:
	- `Builders/FlagstoneUIBuilder.cs`: entry for configuring Flagstone features.
	- `Controls/`: custom controls (to be added, e.g., `Card`, `Snackbar`).
	- `Styles/Tokens.xaml`: token dictionary (colors, spacing, typography) used via `DynamicResource`.
	- `Themes/ThemeLoader.cs`: merges core resource dictionaries into app resources.
- `src/FlagstoneUI.Themes.*/`: Theme libraries (Material, Modern) exposing `Theme.xaml` which merge core tokens and define styles.
- `src/FlagstoneUI.Blocks/`: Reusable app screens (CRUD, auth, settings) under `Blocks/`.
- `samples/`: Sample apps for manual testing (`FlagstoneUI.SampleApp`, `FlagstoneUI.ThemePlayground`).
- `tests/`: xUnit test projects per library.

## Developer Workflows

- **Prereqs**: Install .NET 9 SDK as per `global.json` and the MAUI workload.
- **Restore/build** (root):
	- `dotnet workload install maui`
	- `dotnet restore`
	- `dotnet build Microsoft.Maui.sln`
- **Tests**:
	- `dotnet test .\tests\FlagstoneUI.Core.Tests\FlagstoneUI.Core.Tests.csproj`
	- `dotnet test .\tests\FlagstoneUI.Blocks.Tests\FlagstoneUI.Blocks.Tests.csproj`
	- `dotnet test .\tests\FlagstoneUI.Themes.Material.Tests\FlagstoneUI.Themes.Material.Tests.csproj`
- **CI/CD**: See `.github/workflows/ci.yml` (installs MAUI workload, builds solution, runs tests). `release.yml` is a scaffold for future packaging.

## Developer Environment Setup

- Install .NET 9 SDK and MAUI workload.
- _Never_ downgrade .NET SDK versions as this may cause compatibility issues. _Always_ update development environment to match minimum required SDK version specified in `global.json` (or if it is missing, the latest stable version).

**Always search Microsoft documentation (MS Learn) when working with .NET, Windows, or Microsoft features, or APIs.** Use the `microsoft_docs_search` tool to find the most current information about capabilities, best practices, and implementation patterns before making changes.

### Code Formatting

Run formatting before commits to keep style consistent:

```powershell
dotnet format Microsoft.Maui.sln --no-restore --exclude-diagnostics CA1822
```

- Uses `.editorconfig` settings; CA1822 is suppressed project-wide.
- Avoid formatting-only commits; include functional changes.
**IMPORTANT**: DO NOT commit code with formatting only changes. Ensure that all formatting changes are part of a meaningful commit that includes functional code changes.

## Project-Specific Conventions

- **Token-first styling**: Add/modify tokens in `src/FlagstoneUI.Core/Styles/Tokens.xaml`. Themes consume tokens via `DynamicResource` in their `Theme.xaml`.
- **Naming**: Projects/files start with `FlagstoneUI.*` for discoverability.
- **Controls**: Live under `src/FlagstoneUI.Core/Controls`. Keep public API minimal and theme-agnostic.
- **Themes**: Implement platform-agnostic styles in `Theme.xaml` and merge core tokens.
- **Blocks**: High-level screens go in `src/FlagstoneUI.Blocks/Blocks` and depend on Core controls.

## Integration Points

- **.NET MAUI**: Libraries set `<UseMaui>true</UseMaui>` and target Android/iOS/Windows. No explicit MAUI package reference is needed.
- **Resource dictionaries**: Merge tokens in apps or via `ThemeLoader.Register(app.Resources)` (see `src/FlagstoneUI.Core/Themes/ThemeLoader.cs`).
- **DynamicResource**: Use token keys (e.g., `Color.Primary`) in theme styles: `<Setter Property="BackgroundColor" Value="{DynamicResource Color.Primary}" />`.

## Important Notes

### Update Instructions
🔄 **Remember to update this `copilot-instructions.md` file with new instructions as the project evolves.** This includes:
- New architectural decisions
- Additional coding patterns
- New domain types or services
- Updated workflows or processes
- New external integrations

### Issues and Epics

- Review context before starting: confirm whether the issue is part of an epic, has sub-issues, or is itself a sub-issue. Use linked issues, milestones, and project views to understand scope and sequence.
- Scope intentionally: implement only the slice described by the current issue’s acceptance criteria. Avoid expanding scope unless explicitly agreed.
- Identify dependencies: call out upstream/downstream work. If a behavior is fulfilled by future issues, it’s acceptable to use placeholders and reference the relevant issue IDs.
- Placeholders: keep them minimal and deterministic (e.g., TODO comments with issue links, stub types/methods, or feature flags). They must not leak external DTOs into the domain, change public contracts unexpectedly, or break tests.
- PR hygiene: link issues precisely. Use “Fixes #<id>” only when closing the specific issue. Prefer “Relates to #<id>” or “Part of #<id>” for epics to avoid closing parents unintentionally. Cross-link dependent issues.
- Verification: when placeholders exist, add focused tests or guards to keep builds stable without masking unrelated failures. Document known follow-ups in the PR description.
- Communication: summarize the context (epic/sub-issues), what was completed, what remains (placeholders), and why the chosen approach aligns with the roadmap.**

## References
- `README.md`: repository layout and component descriptions.
- `Microsoft.Maui.sln`: open solution that includes all projects.
- `.github/workflows/ci.yml`: CI pipeline steps and SDK setup.
- `src/FlagstoneUI.Themes.Material/Theme.xaml`: example of a theme consuming tokens.

---

**Feedback:** If any section is unclear or missing, please specify which workflows, conventions, or architectural details need further documentation.
