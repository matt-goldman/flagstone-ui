# FlagstoneUI Architecture

This document describes the architecture and implementation approach for FlagstoneUI.

## Overview

FlagstoneUI is a .NET MAUI UI kit that provides a unified styling plane—enhanced controls with full visual control from shared code, no platform handlers needed for styling. It enables developers to style controls using standard .NET MAUI patterns: inline values, explicit styles, implicit styles, and resource references.

## Core Architecture

FlagstoneUI's architecture is simple and focused:

```
Controls → Styles → Themes
```

That's it. This is the styling plane for .NET MAUI.

**What FlagstoneUI Provides:**
- Enhanced controls with all visual properties exposed as BindableProperties
- Standard .NET MAUI styling patterns (inline, StaticResource, DynamicResource, styles)
- Themes as collections of styles
- No requirement for platform-specific code to achieve visual consistency

## Current Architecture

### Package Structure

```
FlagstoneUI.Core/
├── Builders/
│   └── FlagstoneUIBuilder.cs          # Minimal builder (may be removed)
├── Controls/
│   ├── FsButton.cs                    # Enhanced button with full styling control
│   ├── FsCard.cs                      # Card control with elevation, borders
│   ├── FsEntry.cs                     # Single-line text input with full styling
│   └── FsEditor.cs                    # Multi-line text input with full styling
├── Styles/
│   └── Tokens.xaml                    # Optional design tokens (used by some themes)
└── Themes/
    └── ThemeLoader.cs                 # Theme registration utilities

FlagstoneUI.Themes.Material/
└── Theme.xaml                         # Material theme example (uses tokens internally)

FlagstoneUI.Themes.Modern/
└── Theme.xaml                         # Modern theme (planned)

FlagstoneUI.Blocks/
└── (Future reusable page templates)
```

### Styling Approaches

FlagstoneUI supports multiple valid styling approaches. Developers choose what fits their project:

**Approach 1: Direct Styling**
```xml
<fs:FsButton
    Text="Submit"
    BackgroundColor="#6750A4"
    CornerRadius="12" />
```

**Approach 2: App Resources**
```xml
<!-- Define in App.xaml -->
<Color x:Key="PrimaryColor">#6750A4</Color>

<!-- Use with StaticResource -->
<fs:FsButton
    Text="Submit"
    BackgroundColor="{StaticResource PrimaryColor}" />
```

**Approach 3: Implicit Styles (Default Look)**
```xml
<!-- Define in Theme -->
<Style TargetType="fs:FsButton">
    <Setter Property="BackgroundColor" Value="#6750A4" />
    <Setter Property="CornerRadius" Value="12" />
</Style>

<!-- Usage - styles applied automatically -->
<fs:FsButton Text="Submit" />
```

**Approach 4: Explicit/Named Styles (Variants)**
```xml
<!-- Named styles for control variants -->
<Style x:Key="OutlinedButton" TargetType="fs:FsButton">
    <Setter Property="BackgroundColor" Value="Transparent" />
    <Setter Property="BorderColor" Value="#6750A4" />
    <Setter Property="BorderWidth" Value="1" />
</Style>

<!-- Usage -->
<fs:FsButton Text="Cancel" Style="{StaticResource OutlinedButton}" />
```

**Approach 5: Design Tokens (Optional)**
```xml
<!-- Some themes use tokens as an implementation detail -->
<fs:FsButton
    BackgroundColor="{DynamicResource Color.Primary}"
    CornerRadius="{DynamicResource Radius.Button.Medium}" />
```

All approaches are valid. Themes commonly combine implicit styles (for defaults) with explicit styles (for variants).

### Theme System

**What is a Theme?**: A collection of styles for FlagstoneUI controls.

**Implementation**:
Themes are ResourceDictionaries containing implicit and/or explicit styles:

```xml
<ResourceDictionary xmlns="...">
  <!-- Implicit style - default appearance for all FsButton -->
  <Style TargetType="fs:FsButton">
    <Setter Property="BackgroundColor" Value="#6750A4" />
    <Setter Property="TextColor" Value="White" />
    <Setter Property="CornerRadius" Value="12" />
  </Style>

  <!-- Explicit style variants - provide multiple options -->
  <Style x:Key="OutlinedButton" TargetType="fs:FsButton">
    <Setter Property="BackgroundColor" Value="Transparent" />
    <Setter Property="BorderColor" Value="#6750A4" />
    <Setter Property="BorderWidth" Value="1" />
  </Style>

  <Style x:Key="DeleteButton" TargetType="fs:FsButton">
    <Setter Property="BackgroundColor" Value="#DC3545" />
    <Setter Property="TextColor" Value="White" />
  </Style>
</ResourceDictionary>
```

**Themes typically provide**:
- **Implicit styles** - Default appearance applied automatically to all controls
- **Explicit styles** - Named variants for different visual styles or semantic purposes (e.g., `OutlinedButton`, `DeleteButton`, `RoundedEntry`)

**Themes can use**:
- Direct values (like `#6750A4`)
- App resources (like `{StaticResource PrimaryColor}`)
- Design tokens (like `{DynamicResource Color.Primary}`) - optional implementation detail

**Current Themes**:
- ✅ Material theme (example using tokens internally with multiple variants)
- 🚧 Additional example themes coming

### Control Implementation

**Philosophy**: Create enhanced controls that expose all visual properties via BindableProperties, enabling full visual control from shared code without platform handlers.

**The Styling Surface**: This is what makes FlagstoneUI valuable—all visual properties are exposed and styleable:

**Current Implementation**:
- ✅ **FsButton**: Button with corner radius, borders, colors, padding fully exposed
- ✅ **FsCard**: Container with elevation (shadow support), corner radius, border properties, and per-edge borders
- ✅ **FsEntry**: Single-line text input with full visual control, Community Toolkit validator integration
- ✅ **FsEditor**: Multi-line text input with full visual control, optional animated border effects

**Example - FsButton Properties**:
```csharp
public partial class FsButton : Button
{
    public static readonly BindableProperty CornerRadiusProperty = ...;
    public static readonly BindableProperty BorderColorProperty = ...;
    public static readonly BindableProperty BorderWidthProperty = ...;

    public int CornerRadius { get; set; }
    public Color BorderColor { get; set; }
    public double BorderWidth { get; set; }
    // ... more properties
}
```

**Example - FsCard Properties**:
```csharp
public partial class FsCard : ContentView
{
    public static readonly BindableProperty ElevationProperty = ...;
    public static readonly BindableProperty CornerRadiusProperty = ...;
    public static readonly BindableProperty BorderColorProperty = ...;

    public int Elevation { get; set; }
    public double CornerRadius { get; set; }
    public Color BorderColor { get; set; }
    // ... including per-edge border properties
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

### Optional: Design Tokens

Some themes (like Material) use design tokens as an implementation detail to organize style values. This is optional—themes can use direct values, app resources, or any standard .NET MAUI pattern.

**Token Categories (Material theme example)**:
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
```

Tokens enable:
- Consistent values across theme
- Easy theming by overriding token values
- Dynamic theme switching at runtime

**Note**: Using tokens is an implementation choice for theme authors, not a requirement for using FlagstoneUI.

## Known Issues & Technical Debt

### 1. Cross-Assembly Resource Loading

**Status**: ✅ Fully resolved and working

Cross-assembly ResourceDictionary referencing is implemented using typed references in MergedDictionaries. See [ADR004: Cross-Assembly ResourceDictionary Loading](../decisions/adr004-cross-assembly-resource-loading.md) for detailed technical decisions and implementation.

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
**See**: [Archived technical-plan.md](../archive/technical-plan.md) for historical builder API plans

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

**Note**: For current implementation status and completion tracking, see [implementation-status.md](../project/implementation-status.md).

*Last Updated: November 2025*
