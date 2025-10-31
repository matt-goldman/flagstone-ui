# Technical plan

This file contains the technical plan for Flagstone UI implementation.

**Note**: This document reflects the original planning. See [implementation-status.md](implementation-status.md) and [architecture.md](architecture.md) for current state and implementation details.

## Technology

1. Based on .NET 9 _minimum_.
2. Include MAUI Community Toolkit

## Solution files

* FlagstoneUI.sln at repo root, groups src/, samples/, tests/.

## Core API sketch

`Builders/FlagstoneUIBuilder.cs`

```
namespace FlagstoneUI;

public sealed class FlagstoneUIOptions
{
    public string? ThemeResourcePath { get; set; } // "Themes/Material.xaml" or resource id
    public Density Density { get; set; } = Density.Default;
    public Motion Motion { get; set; } = Motion.Standard;
}

public static class MauiAppBuilderExtensions
{
    public static MauiAppBuilder UseFlagstoneUI(this MauiAppBuilder builder, Action<FlagstoneUIOptions>? configure = null)
    {
        var options = new FlagstoneUIOptions();
        configure?.Invoke(options);

        builder.ConfigureFonts(f => {
            f.AddFont("InterVariable.ttf", "Inter");
        });

        FlagstoneUI.Theming.ThemeLoader.Register(builder, options);
        FlagstoneUI.Handlers.Register(builder); // neutralise native paddings/ink where needed

        return builder;
    }
}
```

`ThemeLoader.cs`

```csharp
public static class ThemeLoader
{
    public static void Register(MauiAppBuilder builder, FlagstoneUIOptions options)
    {
        builder.Services.AddSingleton(options);
        builder.ConfigureMauiHandlers(_ => { /* no-op now */ });
        builder.ConfigureLifecycleEvents(_ => { /* optional */ });
    }

    public static void Apply(ResourceDictionary appResources, string themeXaml)
    {
        var dict = new ResourceDictionary { Source = new Uri(themeXaml, UriKind.Relative) };
        appResources.MergedDictionaries.Add(dict);
    }
}
```

`Tokens.xaml (skeleton)`

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui">
  <!-- Colour -->
  <Color x:Key="Color.Primary">#6750A4</Color>
  <Color x:Key="Color.OnPrimary">#FFFFFF</Color>
  <Color x:Key="Color.Surface">#FFFBFE</Color>
  <Color x:Key="Color.OnSurface">#1C1B1F</Color>
  <!-- Typography -->
  <OnIdiom x:Key="TextStyle.Body" x:TypeArguments="Font">
    <OnIdiom.Phone>14</OnIdiom.Phone>
    <OnIdiom.Tablet>16</OnIdiom.Tablet>
  </OnIdiom>
  <!-- Spacing -->
  <x:Int32 x:Key="Space.0">0</x:Int32>
  <x:Int32 x:Key="Space.1">4</x:Int32>
  <x:Int32 x:Key="Space.2">8</x:Int32>
  <x:Int32 x:Key="Space.3">12</x:Int32>
  <x:Int32 x:Key="Radius.M">8</x:Int32>
  <!-- Elevation, Opacity, Border, Motion tokens as needed -->
</ResourceDictionary>
```

## Sample app usage

`MauiProgram.cs`

```csharp
builder.UseFlagstoneUI(ui =>
{
    ui.ThemeResourcePath = "Themes/Material.xaml";
    ui.Density = Density.Default;
    ui.Motion = Motion.RespectSystem;
});
```

`App.xaml.cs`

```csharp
var opts = Services.GetRequiredService<FlagstoneUIOptions>();
ThemeLoader.Apply(Current!.Resources, opts.ThemeResourcePath ?? "Themes/Material.xaml");
```

### Windows Gotcha

You can't remove the underline and border from the Entry control on Windows in a handler. This is because the `PlatformView` on Windows is subclassed from a base view that is used for other things in WinUI too. So you have to disable the underline and background for the whole `TextControl`. Add this to `App.xaml` in the `Windows` platform folder:

```xml
<maui:MauiWinUIApplication
    x:Class="FlagstoneUI.SampleApp.WinUI.App"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:maui="using:Microsoft.Maui"
    xmlns:local="using:FlagstoneUI.SampleApp.WinUI">
    <!-- Add from here: -->
    <maui:MauiWinUIApplication.Resources>
        <Thickness x:Key="TextControlBorderThemeThickness">0</Thickness>
        <Thickness x:Key="TextControlBorderThemeThicknessFocused">0</Thickness>
    </maui:MauiWinUIApplication.Resources>
    <!-- to here -->
</maui:MauiWinUIApplication>
```

## Build settings

`Directory.Build.props`

```xml
<Project>
  <PropertyGroup>
    <TargetFrameworks>net10.0-android;net10.0-ios;net10.0-maccatalyst;net10.0-windows10.0.19041.0</TargetFrameworks>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <AnalysisLevel>latest</AnalysisLevel>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <RepositoryUrl>https://github.com/your-org/flagstoneui</RepositoryUrl>
    <PackageReadmeFile>README.md</PackageReadmeFile>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="CommunityToolkit.Maui" Version="*"/>
    <PackageReference Include="Microsoft.CodeAnalysis.NetAnalyzers" PrivateAssets="all" Version="*"/>
    <PackageReference Include="coverlet.collector" Version="*" PrivateAssets="all" />
  </ItemGroup>
</Project>
```

## GitHub Actions

`.github/workflows/ci.yml`

```yml
name: CI
on:
  pull_request:
  push:
    branches: [ main ]
jobs:
  build-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'
      - name: Restore
        run: dotnet restore FlagstoneUI.sln
      - name: Build
        run: dotnet build FlagstoneUI.sln -c Release --no-restore
      - name: Test
        run: dotnet test tests/FlagstoneUI.Core.Tests/FlagstoneUI.Core.Tests.csproj -c Release --no-build --collect:"XPlat Code Coverage"
      - name: Pack
        run: dotnet pack src/FlagstoneUI.Core/FlagstoneUI.Core.csproj -c Release -o ./artifacts
```

`.github/workflows/release.yml`

```yml
name: Release
on:
  push:
    tags: [ 'v*.*.*' ]
jobs:
  publish:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'
      - name: Build solution
        run: dotnet build FlagstoneUI.sln -c Release
      - name: Pack all
        run: |
          dotnet pack src/FlagstoneUI.Core -c Release -o ./artifacts
          dotnet pack src/FlagstoneUI.Themes.Material -c Release -o ./artifacts
          dotnet pack src/FlagstoneUI.Themes.Modern -c Release -o ./artifacts
          dotnet pack src/FlagstoneUI.Blocks -c Release -o ./artifacts
      - name: Publish to NuGet
        run: dotnet nuget push "./artifacts/*.nupkg" --api-key ${{ secrets.NUGET_API_KEY }} --source https://api.nuget.org/v3/index.json
```

Optional: add release-please or GitVersion later for automated versioning. For now, tag v0.1.0 to publish.

## Issue templates

`.github/ISSUE_TEMPLATE/bug_report.md`

```yml
---
name: Bug report
about: Something not working as expected
---

**Describe the bug**
**Repro steps**
**Expected**
**Screenshots**
**Env** (MAUI version, platform, device)
```

`.github/ISSUE_TEMPLATE/feature_request.md`


```yml
---
name: Feature request
about: Suggest an idea
---

**Problem**
**Proposal**
**Alternatives**
**Additional context / mockups**
```

`PULL_REQUEST_TEMPLATE.md`

```yml

## Summary
What this changes.

## Screenshots
Before/after if visual.

## Checklist
- [ ] Tests added/updated
- [ ] Docs updated
- [ ] No breaking changes (or documented)
```

`CODEOWNERS`

```yml
* @your-handle
/src/FlagstoneUI.Core/ @your-handle @trusted-collab
```