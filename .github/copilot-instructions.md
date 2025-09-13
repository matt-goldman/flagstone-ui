# Copilot Instructions for Flagstone UI

This guide helps AI coding agents work productively in the Flagstone UI codebase. It summarizes architecture, workflows, and conventions unique to this project.

## Project Overview

- **Flagstone UI** is a cross-platform, open-source UI kit and framework for .NET MAUI.
- The repository is organized into modular subprojects for core controls, themes, blocks (app screens), samples, and tests.
- The codebase is in early planning/WIP phase; structure may evolve.

## Key Directories & Components

- `src/FlagstoneUI.Core/`: Core UI controls, builders, handlers, styles, and themes. Controls (e.g., Button, Card) are in `Controls/`. Styles use token dictionaries (`Styles/Tokens.xaml`) and are wired via `DynamicResource`.
- `src/FlagstoneUI.Themes.*`: Theme packages (e.g., Material, Modern) with theme dictionaries (`Theme.xaml`) referencing core style tokens.
- `src/FlagstoneUI.Blocks/`: Reusable app screens (CRUD, auth, settings) in `Blocks/`.
- `samples/`: Example apps for manual testing and showcasing controls/themes.
- `tests/`: Unit and integration tests for each major component.

## Developer Workflows

- **Build**: Use standard .NET build commands (`dotnet build`, `dotnet restore`).
- **Test**: Run tests with `dotnet test` in the `tests/` subdirectories.
- **Sample Apps**: Launch sample apps in `samples/` for manual UI validation.
- **CI/CD**: GitHub Actions workflows in `.github/workflows/ci.yml` and `release.yml` automate builds and releases.

## Developer Environment Setup

- Install .NET 9 SDK and MAUI workload.
- _Never_ downgrade .NET SDK versions as this may cause compatibility issues. _Always_ update development environment to match minimum required SDK version specified in `global.json` (or if it is missing, the latest stable version).

### Code Formatting

Before committing any changes, format the codebase using the following command to ensure consistent code style:

```bash
dotnet format Microsoft.Maui.sln --no-restore --exclude Templates/src --exclude-diagnostics CA1822
```

This command:
- Formats all code files according to the repository's `.editorconfig` settings
- Excludes the Templates/src directory from formatting
- Excludes the CA1822 diagnostic (member can be marked as static)
- Uses `--no-restore` for faster execution when dependencies are already restored

**IMPORTANT**: DO NOT commit code with formatting only changes. Ensure that all formatting changes are part of a meaningful commit that includes functional code changes.

## Project-Specific Conventions

- **Component Structure**: Controls, themes, and blocks are separated for clarity and reusability. Theme resources use token-based styling for consistency.
- **Naming**: Subprojects and files use `FlagstoneUI.*` prefixes for discoverability.
- **Extensibility**: New controls/themes should follow the established directory and naming patterns.
- **Resource Dictionaries**: Style tokens and theme dictionaries are central to UI customization; reference `Tokens.xaml` and `Theme.xaml` for examples.

## Integration Points

- **.NET MAUI**: All UI components are designed for .NET MAUI compatibility.
- **DynamicResource**: Styles/themes leverage MAUI's dynamic resource system for runtime theme switching.
- **Sample Apps**: Integrate new controls/themes into sample apps for demonstration and manual testing.

## References
- See `README.md` for planned repository layout and component descriptions.
- Review `.github/workflows/ci.yml` for CI/CD details.
- Use sample apps in `samples/` to validate UI changes.

---

**Feedback:** If any section is unclear or missing, please specify which workflows, conventions, or architectural details need further documentation.
