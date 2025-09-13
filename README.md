# Flagstone UI

A cross-platform, open-source, community-driven, customisable UI kit and framework for .NET MAUI.

## Suggested layout

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