# ADR008: Bootstrap Converter Advanced Features - Shadows and Per-Edge Borders

**Status**: Accepted  
**Date**: December 16, 2025  
**Deciders**: Matt Goldman  
**Context**: Bootstrap theme conversion enhancements for FlagstoneUI

## Context and Problem Statement

The initial Bootstrap converter implementation successfully extracted core design tokens (colors, spacing, typography, borders) but lacked support for advanced visual features that are common in Bootstrap themes:

1. **Shadows**: Bootstrap themes extensively use `box-shadow` for elevation and depth (buttons, cards, dropdowns, modals)
2. **Per-Edge Borders**: Bootstrap supports asymmetric borders with multi-value `border-width` properties (e.g., `border-width: 1px 0 0 0` for top-only borders)

Without these features, converted themes looked flat and lacked the visual polish of the original Bootstrap designs.

## Decision Drivers

1. **Visual Fidelity**: Converted themes should match Bootstrap's visual appearance as closely as possible
2. **Platform Capabilities**: MAUI Shadow and Border properties have different models than CSS
3. **Theme Coverage**: Support the most common Bootstrap shadow and border patterns
4. **Dark Mode Support**: Shadows and borders should respect light/dark theme modes
5. **Platform Limitations**: Windows MAUI has known shadow rendering issues (must document)

## Considered Options

### For Shadows

#### Option 1: Ignore Shadows
**Pros**: Simplest approach  
**Cons**: Significant visual fidelity loss, missing Bootstrap's depth hierarchy

#### Option 2: Map to MAUI Elevation (Android Material)
**Pros**: Native Android material design elevation  
**Cons**: Platform-specific, doesn't work on iOS/Windows, limited control

#### Option 3: Map to MAUI Shadow (Selected)
**Pros**: Cross-platform, full control over offset/radius/color, matches Bootstrap box-shadow model  
**Cons**: Windows rendering limitations (platform issue, not converter issue)

### For Per-Edge Borders

#### Option 1: Use Only Uniform Borders
**Pros**: Simple, works on all platforms  
**Cons**: Can't represent Bootstrap's asymmetric border patterns

#### Option 2: Implement Per-Edge Border Properties (Selected)
**Pros**: Full fidelity for Bootstrap border patterns, matches .NET MAUI's BorderTopWidth/etc. properties  
**Cons**: More complex token generation, requires FlagstoneUI controls to support per-edge borders

## Decision Outcome

**Chosen option**: Implement both **Shadow support** and **Per-Edge Border support** with comprehensive extraction and mapping.

### Shadow Implementation

#### Data Model

```csharp
public class ShadowToken
{
    public string Key { get; set; }
    public double OffsetX { get; set; }
    public double OffsetY { get; set; }
    public double Radius { get; set; }
    public string Color { get; set; }
    public double Opacity { get; set; } = 1.0;
    public string? Purpose { get; set; }
    
    // Dark mode support (ready, not yet extracted from CSS)
    public double? DarkOffsetX { get; set; }
    public double? DarkOffsetY { get; set; }
    public double? DarkRadius { get; set; }
    public string? DarkColor { get; set; }
    public double? DarkOpacity { get; set; }
}
```

#### Extraction Strategy

**From SCSS Variables** (bottom-up):
```scss
$box-shadow: 0 .5rem 1rem rgba(0, 0, 0, .15);
$btn-box-shadow: 0 2px 4px 0 rgba(0, 0, 0, .2);
$box-shadow-sm: 0 .125rem .25rem rgba(0, 0, 0, .075);
```

**From CSS** (top-down):
```css
.btn {
  box-shadow: 0 2px 4px 0 rgba(0, 0, 0, 0.2);
}
```

**Mapping Logic**:
- Variable name → Token key (e.g., `$btn-box-shadow` → `Shadow.Button`)
- Parse shadow value: extract offset-x, offset-y, blur-radius, color
- Handle rgba() with proper RGB extraction
- Filter out `inset` shadows (not supported in MAUI)
- Skip shadows with CSS variables (e.g., `rgba(var(--bs-primary-rgb), 0.5)`)

#### XAML Generation

```xaml
<Shadow x:Key="Shadow.Button">
    <Shadow.Offset>2,4</Shadow.Offset>
    <Shadow.Radius>4</Shadow.Radius>
    <Shadow.Brush>
        <SolidColorBrush Color="#000000" Opacity="0.2" />
    </Shadow.Brush>
</Shadow>
```

**Applied to Styles**:
```xaml
<Style TargetType="fs:FsButton">
    <Setter Property="Shadow" Value="{DynamicResource Shadow.Button}" />
</Style>

<Style TargetType="fs:FsCard">
    <Setter Property="Shadow" Value="{DynamicResource Shadow.Small}" />
</Style>
```

### Per-Edge Border Implementation

#### Data Model

Added dictionaries to `FlagstoneTokens`:
```csharp
public Dictionary<string, string> BorderTopWidth { get; set; } = new();
public Dictionary<string, string> BorderRightWidth { get; set; } = new();
public Dictionary<string, string> BorderBottomWidth { get; set; } = new();
public Dictionary<string, string> BorderLeftWidth { get; set; } = new();
```

#### Extraction Strategy

Parse multi-value `border-width` CSS properties:
```css
/* Bootstrap CSS */
border-width: 1px 0 0 0;  /* top only */
border-width: 1px 2px 3px 4px;  /* all different */
border-width: 2px 0;  /* top/bottom: 2px, left/right: 0 */
```

**Mapping Rules** (CSS standard):
- 1 value: all edges
- 2 values: [top/bottom] [left/right]
- 3 values: [top] [left/right] [bottom]
- 4 values: [top] [right] [bottom] [left]

#### XAML Generation

```xaml
<x:Double x:Key="BorderWidth.Top.Default">1</x:Double>
<x:Double x:Key="BorderWidth.Right.Default">0</x:Double>
<x:Double x:Key="BorderWidth.Bottom.Default">0</x:Double>
<x:Double x:Key="BorderWidth.Left.Default">0</x:Double>
```

## Consequences

### Positive

1. **Visual Fidelity**: Converted themes now match Bootstrap's visual depth and border patterns
2. **Component Hierarchy**: Shadows establish proper visual hierarchy (buttons vs cards vs modals)
3. **Bootstrap Parity**: Per-edge borders support Bootstrap's asymmetric border patterns
4. **Dark Mode Ready**: Shadow data model includes dark mode properties (extraction TODO)
5. **Comprehensive Coverage**: Tested with Brite theme - extracted 12 shadow tokens successfully

### Negative

1. **Windows Platform Limitation**: MAUI on Windows has broken shadow rendering (ignores offset, uniform blur)
   - **Mitigation**: Documented as known limitation, works correctly on Android
2. **CSS Variable Shadows**: Shadows using CSS variables (e.g., `rgba(var(--bs-primary-rgb), 0.5)`) cannot be parsed
   - **Mitigation**: Log and skip, recommend SCSS variable mode for best results
3. **Increased Complexity**: More token types to track, generate, and merge
   - **Mitigation**: Clear data models and helper methods, comprehensive logging

### Platform-Specific Behavior

| Platform | Shadow Support | Notes |
|----------|---------------|-------|
| **Android** | ✅ Full support | Renders offset, blur, color correctly |
| **iOS** | ⚠️ Limited | Always applies some blur even when radius=0 |
| **Windows** | ❌ Broken | Ignores offset, renders uniform blur only |
| **macOS** | ✅ Good | Similar to iOS behavior |

**Recommendation**: Test themes on Android for accurate shadow preview. Windows limitations are a MAUI platform issue, not a converter issue.

## Validation Evidence

### Successful Shadow Extraction (Brite Theme)

```bash
dotnet run -- convert \
  --input brite-theme/_variables.scss \
          brite-theme/_bootswatch.scss \
          brite-theme/bootstrap.css \
  --output test-output-brite \
  --analysis-mode hybrid
```

**Results**:
- ✅ Extracted 12 shadow tokens:
  - Shadow.Small (offset: 0,2 / radius: 4)
  - Shadow.Button (offset: 0,2 / radius: 4)
  - Shadow.Toast.Default (offset: 3,3 / radius: 0)
  - Shadow.Modal (offset: 0,8 / radius: 32)
  - And more...
- ✅ Proper rgba color parsing with opacity extraction
- ✅ Multi-shadow handling (takes first non-inset shadow)
- ✅ Applied to FsButton and FsCard styles with fallback logic

### Per-Edge Border Support

**Bootstrap Input**:
```scss
$card-border-width: 1px 0 0 0;
$alert-border-width: 0 0 0 4px;
```

**Generated Tokens**:
```xaml
<x:Double x:Key="BorderWidth.Top.Card">1</x:Double>
<x:Double x:Key="BorderWidth.Right.Card">0</x:Double>
<x:Double x:Key="BorderWidth.Bottom.Card">0</x:Double>
<x:Double x:Key="BorderWidth.Left.Card">0</x:Double>

<x:Double x:Key="BorderWidth.Top.Alert">0</x:Double>
<x:Double x:Key="BorderWidth.Right.Alert">0</x:Double>
<x:Double x:Key="BorderWidth.Bottom.Alert">0</x:Double>
<x:Double x:Key="BorderWidth.Left.Alert">4</x:Double>
```

## Future Enhancements

1. **Shadow Dark Mode Extraction**: Extract dark mode shadow values from `[data-bs-theme="dark"]` CSS custom properties
2. **Gradient Shadow Fallback**: For complex shadows, generate gradient approximations
3. **Shadow Composition**: Handle multi-shadow layering (currently uses first non-inset)
4. **Platform-Specific Shadow Profiles**: Generate different shadow values optimized for each platform's rendering capabilities

## Related Decisions

- ADR005: Bootstrap Converter Analysis Modes (establishes extraction architecture)
- ADR005 Addendum: AppThemeBinding Implementation (light/dark mode support)
- ADR004: Cross-Assembly Resource Loading (XAML generation patterns)

## References

- Bootstrap Box Shadow Documentation: https://getbootstrap.com/docs/5.3/utilities/shadows/
- MAUI Shadow Documentation: https://learn.microsoft.com/en-us/dotnet/maui/user-interface/shadow
- CSS box-shadow Specification: https://www.w3.org/TR/css-backgrounds-3/#box-shadow
- Implementation Files:
  - `tools/FlagstoneUI.BootstrapConverter/Models/FlagstoneTokens.cs`
  - `tools/FlagstoneUI.BootstrapConverter/BootstrapMapper.cs`
  - `tools/FlagstoneUI.BootstrapConverter/BootstrapCssAnalyzer.cs`
  - `tools/FlagstoneUI.BootstrapConverter/XamlThemeGenerator.cs`
  - `tools/FlagstoneUI.BootstrapConverter/BootstrapConverterService.cs`
