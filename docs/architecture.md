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
- ✅ **Card**: Complete custom ContentView with elevation (shadow support), corner radius, and border properties. Elevation automatically applies Material Design-compliant shadows.
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

### Theme Configuration

**Approach**: Themes are configured via merged ResourceDictionaries in App.xaml (no builder pattern needed - YAGNI).

**Implementation**:
```xml
<!-- In App.xaml -->
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <material:Theme />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

**Note**: A minimal FlagstoneUIBuilder class exists in the codebase but is not required for theme configuration and may be removed in the future.

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

### 1. Cross-Assembly Resource Loading

**Status**: ✅ Fully resolved and working

Cross-assembly ResourceDictionary referencing is implemented using typed references in MergedDictionaries. See [ADR004: Cross-Assembly ResourceDictionary Loading](Decisions/adr004-cross-assembly-resource-loading.md) for detailed technical decisions and implementation.

**Quick Reference for Consumers**:
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

### 3. Builder Pattern Not Required
**Decision**: FlagstoneUIBuilder pattern is not needed (YAGNI principle)  
**Approach**: Theme configuration via merged dictionaries in App.xaml  
**Note**: Minimal builder class may be removed in future cleanup  
**See**: [Archived technical-plan.md](archive/technical-plan.md) for historical builder API plans

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

### Testing Approach
- **Unit Tests**: Control properties and behavior
- **Integration Tests**: Theme application and resource loading
- **Visual Tests**: Cross-platform rendering validation
- **Accessibility Tests**: Screen reader and contrast validation

## Dependencies

### Current Dependencies
- **.NET 10**: Minimum version requirement
- **MAUI Workload**: Required for all projects
- **CommunityToolkit.Maui**: Optional (currently disabled in Directory.Build.props - see issue #12)

### Future Dependencies
- **Font Assets**: Inter Variable for typography
- **Icon Libraries**: Material Design Icons or similar
- **Animation Libraries**: For motion system implementation

**Note**: For current implementation status and completion tracking, see [implementation-status.md](implementation-status.md).

*Last Updated: November 2025*