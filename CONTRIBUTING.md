# Contributing to Flagstone UI

Thank you for considering contributing to Flagstone UI! This document provides guidelines for contributing to the project.

## Development Setup

### Prerequisites

- .NET 9 SDK (minimum version 9.0.100 as specified in `global.json`)
- MAUI workload: `dotnet workload install maui`

### Getting Started

1. Fork and clone the repository
2. Validate your development environment:
   ```bash
   # Linux/macOS
   ./scripts/validate-setup.sh
   
   # Windows
   .\scripts\validate-setup.ps1
   ```
3. Restore dependencies: `dotnet restore Flagstone.UI.sln`
4. Build the solution: `dotnet build Flagstone.UI.sln`
5. Run tests: `dotnet test`

## Coding Standards

### Code Style

- Use the provided `.editorconfig` settings
- Run formatting before commits: `dotnet format Flagstone.UI.sln --no-restore --exclude-diagnostics CA1822`
- File-scoped namespaces preferred
- Nullable reference types enabled
- Treat warnings as errors

### Commit Convention

Use [Conventional Commits](https://www.conventionalcommits.org/) format:

- `feat:` - New features
- `fix:` - Bug fixes  
- `docs:` - Documentation changes
- `refactor:` - Code refactoring
- `test:` - Adding tests
- `chore:` - Build/tooling changes

Examples:
- `feat(core): add FsButton control with neutral styling`
- `fix(themes): resolve token reference in Material theme`
- `docs: add quickstart guide for theming`

### Code Requirements

- **Tests required** for controls and theme resources where possible
- **Visual diffs**: Include screenshots or short clips for UI changes
- **Accessibility**: Ensure focus visuals, semantics, and contrast tokens
- **Theme additions** must use tokens only - no hard-coded colors
- **Breaking changes** must be documented

## Project Structure

### Component Areas

Use these labels when creating issues/PRs:

- `area:core` - Core library and controls (`src/FlagstoneUI.Core/`)
- `area:themes` - Themes and styling (`src/FlagstoneUI.Themes.*/`)  
- `area:blocks` - Reusable page templates (`src/FlagstoneUI.Blocks/`)
- `area:docs` - Documentation and guides

### Development Guidelines

- **Token-first styling**: Add/modify tokens in `src/FlagstoneUI.Core/Styles/Tokens.xaml`
- **Theme consumption**: Themes consume tokens via `DynamicResource` in their `Theme.xaml`
- **Control design**: Keep public API minimal and theme-agnostic
- **Cross-platform**: Ensure controls work on all MAUI target platforms

## Pull Request Process

1. **Create feature branch** from `main`
2. **Implement changes** following coding standards
3. **Add/update tests** as appropriate
4. **Update documentation** if needed
5. **Test across platforms** if applicable
6. **Submit PR** with:
   - Clear description of changes
   - Screenshots for visual changes
   - Reference to related issues
   - Checklist completion

### PR Template Checklist

- [ ] Tests added/updated
- [ ] Docs updated
- [ ] No breaking changes (or documented)
- [ ] Visual changes include screenshots
- [ ] Accessibility considerations addressed

## Architecture Principles

### Design Tokens First

All styling must use the token system:

```xml
<!-- Good: Uses tokens -->
<Setter Property="BackgroundColor" Value="{DynamicResource Color.Primary}" />

<!-- Bad: Hard-coded values -->
<Setter Property="BackgroundColor" Value="#6750A4" />
```

### Platform Neutrality

- Strip native styling using handlers where necessary
- Use `ControlTemplate` and `VisualStateManager` for theming
- Support theme switching via merged dictionaries

## Getting Help

- **Issues**: Check existing issues or create new ones
- **Discussions**: Use GitHub Discussions for questions
- **Documentation**: Review docs in `/docs` folder

## Code of Conduct

This project follows the [Contributor Covenant](CODE_OF_CONDUCT.md). Please read it before contributing.

## License

By contributing, you agree that your contributions will be licensed under the MIT License.