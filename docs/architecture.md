# Flagstone UI Architecture

This document describes the current architecture and implementation approach for Flagstone UI.

## Overview

Flagstone UI is a .NET MAUI UI kit that provides neutral, themeable controls through a design token system and custom control implementations.

## Current Architecture

### Package Structure

```
FlagstoneUI.Core/
├── Builders/
│   └── FlagstoneUIBuilder.cs          # Configuration builder pattern
├── Controls/
│   └── Card.cs                        # Custom controls (Card implemented)
├── Styles/
│   └── Tokens.xaml                    # Design tokens (colors, spacing, typography)
└── Themes/
    └── ThemeLoader.cs                 # Theme registration utilities

FlagstoneUI.Themes.Material/
└── Theme.xaml                         # Material Design theme implementation

FlagstoneUI.Themes.Modern/
└── Theme.xaml                         # Modern theme implementation (placeholder)

FlagstoneUI.Blocks/
└── (Future reusable page templates)
```

### Design Token System

**Core Concept**: Central design tokens defined in `FlagstoneUI.Core/Styles/Tokens.xaml` are consumed by theme files through `DynamicResource` references.

**Current Implementation**:
- ✅ Tokens defined for colors, spacing, radii, typography
- ❌ Cross-component resource loading blocked (see Known Issues)
- 🚧 Temporary workaround: Local token definitions in theme files

**Token Categories**:
```xml
<!-- Colors -->
<Color x:Key="Color.Primary">#6750A4</Color>
<Color x:Key="Color.OnPrimary">#FFFFFF</Color>
<Color x:Key="Color.Surface">#FFFBFE</Color>

<!-- Spacing -->
<x:Double x:Key="Space.8">8</x:Double>
<x:Double x:Key="Space.16">16</x:Double>

<!-- Radii -->
<x:Double x:Key="Radius.Medium">8</x:Double>

<!-- Typography -->
<x:Double x:Key="FontSize.Body">14</x:Double>
```

### Theme System

**Implementation Strategy**:
1. Themes merge core tokens via `ResourceDictionary.MergedDictionaries`
2. Themes define implicit styles for standard controls
3. Themes define styles for custom Flagstone controls

**Current Status**:
- ✅ Basic Material theme structure
- ❌ Resource merging not functional
- 🚧 Using local token definitions as workaround

**Example Theme Structure**:
```xml
<ResourceDictionary xmlns="...">
  <ResourceDictionary.MergedDictionaries>
    <!-- TODO: Fix cross-component resource references -->
    <!-- <ResourceDictionary Source="/FlagstoneUI.Core;component/Styles/Tokens.xaml" /> -->
  </ResourceDictionary.MergedDictionaries>
  
  <!-- Implicit styles for standard controls -->
  <Style TargetType="Button">
    <Setter Property="BackgroundColor" Value="{DynamicResource Color.Primary}" />
  </Style>
  
  <!-- Styles for custom controls -->
  <Style TargetType="fs:Card">
    <Setter Property="Padding" Value="{DynamicResource Space.16}" />
  </Style>
</ResourceDictionary>
```

### Control Implementation

**Philosophy**: Create neutral controls that strip platform-specific styling and apply theme-based styling consistently.

**Current Implementation**:
- ✅ **Card**: Complete custom ContentView with elevation, radius, border properties
- ❌ **FsButton**: Not implemented (planned: subclass Button, add platform handlers)
- ❌ **FsEntry**: Not implemented (planned: subclass Entry, add validation states)
- ❌ **Snackbar**: Not implemented (planned: overlay service pattern)

**Card Example**:
```csharp
public partial class Card : ContentView
{
    public static readonly BindableProperty ElevationProperty = ...;
    public static readonly BindableProperty CornerRadiusProperty = ...;
    public static readonly BindableProperty BorderColorProperty = ...;
    
    public int Elevation { get; set; }
    public double CornerRadius { get; set; }
    public Color BorderColor { get; set; }
}
```

### Builder Pattern

**Purpose**: Provide fluent API for configuring Flagstone UI in MAUI applications.

**Current Implementation**:
```csharp
public class FlagstoneUIBuilder
{
    public FlagstoneUIBuilder UseDefaultTheme() => this;
    // TODO: Expand configuration options
}
```

**Planned API**:
```csharp
builder.UseFlagstoneUI(ui =>
{
    ui.Theme("Themes/Material.xaml");
    ui.Density = Density.Compact;
    ui.Motion = Motion.Standard;
});
```

### Theme Loading

**Current Implementation**:
```csharp
public static class ThemeLoader
{
    public static void Register(ResourceDictionary appResources)
    {
        appResources.MergedDictionaries.Add(new ResourceDictionary
        {
            Source = new Uri("/FlagstoneUI.Core;component/Styles/Tokens.xaml", UriKind.Relative)
        });
    }
}
```

**Usage Pattern**:
```csharp
// In App.xaml.cs or MauiProgram.cs
ThemeLoader.Register(app.Resources);
```

## Known Issues & Technical Debt

### 1. Resource Loading ✅ RESOLVED
**Previous Issue**: Cross-component XAML resource references not working  
**Previous Impact**: Themes cannot consume core tokens properly  
**Previous Workaround**: Temporary local token definitions  

**Resolution**: Implemented proper cross-assembly ResourceDictionary referencing using Microsoft's recommended approach:
- Added x:Class attributes and code-behind files to all ResourceDictionary files that need cross-assembly consumption
- Use `MergedDictionaries` collection with typed references (e.g., `<tokens:Tokens />`) instead of `Source` property
- Added proper XAML namespace definitions via GlobalXmlns.cs files
- All themes now properly inherit design tokens from FlagstoneUI.Core

**For Consumers**: When using Flagstone UI themes in your applications, reference them using the typed syntax:
```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <material:Theme />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```  

### 2. Missing Platform Handlers
**Issue**: No platform handlers for neutral styling  
**Impact**: Controls will show platform-specific styling  
**Solution**: Implement handlers for FsButton, FsEntry to strip native styling  

### 3. Incomplete Builder API
**Issue**: FlagstoneUIBuilder lacks configuration options  
**Impact**: Limited customization and theming options  
**Solution**: Expand builder with density, motion, theme selection options  

## Future Architecture Considerations

### Phase 2 Expansions
- **Form Controls**: CheckBox, RadioButton, Picker, Slider
- **Display Controls**: Badge, Avatar, ProgressBar, Divider
- **Navigation**: AppBar, TabBar, Drawer
- **Feedback**: Dialog, Toast

### Phase 3 Blocks
- **Auth Screens**: Sign in/up forms, onboarding
- **CRUD Patterns**: List/detail, create/edit forms
- **App Chrome**: Navigation, settings templates

### Community Themes
- **Theme Gallery**: Community-contributed themes
- **Theme Development Kit**: Tooling and guidelines
- **Theme Validation**: Automated accessibility and consistency checks

## Testing Strategy

### Current State
- ✅ Basic test projects exist
- ❌ Minimal test coverage
- ❌ No UI testing infrastructure

### Planned Testing
- **Unit Tests**: Control properties and behavior
- **Integration Tests**: Theme application and resource loading
- **Visual Tests**: Cross-platform rendering validation
- **Accessibility Tests**: Screen reader and contrast validation

## Dependencies

### Current Dependencies
- **.NET 10**: Minimum version requirement
- **MAUI Workload**: Required for all projects
- **CommunityToolkit.Maui**: Included via Directory.Build.props

### Future Dependencies
- **Font Assets**: Inter Variable for typography
- **Icon Libraries**: Material Design Icons or similar
- **Animation Libraries**: For motion system implementation

*Last Updated: [Current Date]*
*Status: Initial implementation phase*