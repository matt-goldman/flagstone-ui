# Flagstone UI

A cross-platform, open-source, community-driven, customisable UI kit and framework for .NET MAUI.

## Developer Setup

### Prerequisites

1. **.NET 9 SDK**: Install from [Microsoft's .NET download page](https://dotnet.microsoft.com/download/dotnet/9.0)
2. **MAUI Workload**: After installing .NET 9, run:
   ```bash
   dotnet workload install maui
   ```

### Building the Solution

1. Clone the repository
2. Navigate to the repository root
3. Restore dependencies:
   ```bash
   dotnet restore Flagstone.UI.sln
   ```
4. Build the solution:
   ```bash
   dotnet build Flagstone.UI.sln
   ```
5. Run tests:
   ```bash
   dotnet test Flagstone.UI.sln
   ```

### Validation Scripts

To quickly validate your development environment setup, you can run one of the provided validation scripts:

**Linux/macOS:**
```bash
./scripts/validate-setup.sh
```

**Windows (PowerShell):**
```powershell
.\scripts\validate-setup.ps1
```

These scripts will check for:
- .NET 9 SDK installation
- MAUI workload availability
- Solution restore capability
- Solution build capability

### Important Notes

- All projects in this solution target MAUI platform frameworks (`net10.0-ios`, `net10.0-android`, `net10.0-windows10.0.19041.0`)
- The MAUI workload is required even for test projects to ensure TFM compatibility
- Common dependencies and test packages are managed centrally in `Directory.Build.props`

### GitHub Copilot Setup

For AI coding agents and GitHub Copilot setup, see [COPILOT_SETUP.md](COPILOT_SETUP.md) for specific environment configuration and platform targeting guidance.

## Project Structure

This is a WIP in the planning phase. Suggested layout for the repository is as follows.

```tree
flagstoneui/
├─ README.md
├─ LICENSE
├─ CONTRIBUTING.md
├─ CODE_OF_CONDUCT.md
├─ SECURITY.md
├─ .editorconfig
├─ Directory.Build.props
├─ Directory.Build.targets
├─ global.json
├─ src/
│  ├─ FlagstoneUI.Core/
│  │  ├─ FlagstoneUI.Core.csproj
│  │  ├─ Builders/
│  │  │  └─ FlagstoneUIBuilder.cs
│  │  ├─ Controls/        # Button, Entry, Card, Snackbar, Switch, FormField
│  │  ├─ Handlers/
│  │  ├─ Styles/          # token dictionaries wired to DynamicResource
│  │  │  └─ Tokens.xaml
│  │  └─ Themes/
│  │     └─ ThemeLoader.cs
│  ├─ FlagstoneUI.Themes.Material/
│  │  ├─ FlagstoneUI.Themes.Material.csproj
│  │  └─ Theme.xaml       # light/dark dictionaries, uses Tokens keys
│  ├─ FlagstoneUI.Themes.Modern/
│  │  ├─ FlagstoneUI.Themes.Modern.csproj
│  │  └─ Theme.xaml
│  └─ FlagstoneUI.Blocks/
│     ├─ FlagstoneUI.Blocks.csproj
│     └─ Blocks/          # CRUD screens, auth, settings, etc.
├─ samples/
│  ├─ FlagstoneUI.SampleApp/
│  │  ├─ FlagstoneUI.SampleApp.csproj
│  │  ├─ App.xaml
│  │  ├─ App.xaml.cs
│  │  ├─ MauiProgram.cs
│  │  └─ Pages/
│  │     ├─ ControlsGalleryPage.xaml
│  │     └─ BlocksGalleryPage.xaml
│  └─ FlagstoneUI.ThemePlayground/
│     ├─ ThemePlayground.csproj
│     └─ Themes/          # user-editable copies
├─ tests/
│  ├─ FlagstoneUI.Core.Tests/
│  │  ├─ FlagstoneUI.Core.Tests.csproj
│  │  └─ …
│  ├─ FlagstoneUI.Themes.Material.Tests/
│  └─ FlagstoneUI.Blocks.Tests/
└─ .github/
   ├─ ISSUE_TEMPLATE/
   │  ├─ bug_report.md
   │  └─ feature_request.md
   ├─ PULL_REQUEST_TEMPLATE.md
   ├─ workflows/
   │  ├─ ci.yml
   │  └─ release.yml
   └─ CODEOWNERS
```