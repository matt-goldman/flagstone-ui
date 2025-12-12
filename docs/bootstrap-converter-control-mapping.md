# Bootstrap Converter Control Mapping Design

**Status:** Implemented (Baseline)  
**Date:** 2025-12-12  
**Related:** [Bootstrap Converter Enhancement Plan](bootstrap-converter-enhancement-plan.md)

## Problem Statement

The Bootstrap converter (`FlagstoneUI.BootstrapConverter`) previously generated minimal styles (only 3 button variants) despite extracting comprehensive Bootstrap component data. The baseline implementation now generates a complete `Styles.xaml` covering all currently implemented FlagstoneUI controls, with room to improve Bootstrap fidelity over time.

## Current State

### FlagstoneUI Controls
- **FsButton** - Simple Button subclass
- **FsEntry** - BorderlessEntry + Border wrapper
- **FsEditor** - BorderlessEditor + Border wrapper  
- **FsCard** - ContentView with Border wrapper

### Bootstrap Analyzer Output
The `BootstrapCssAnalyzer` already extracts:
- Button variants: `.btn-primary`, `.btn-secondary`, `.btn-success`, `.btn-danger`, `.btn-warning`, `.btn-info`, `.btn-light`, `.btn-dark`
- Outline variants: `.btn-outline-primary`, `.btn-outline-secondary`, etc.
- Size variants: `.btn-sm`, `.btn-lg`
- Form controls: `.form-control`
- Validation states: `.is-valid`, `.is-invalid`
- Card components: `.card`, `.card-header`, `.card-body`, `.card-footer`

### Current Generator Output
- ✅ Tokens.xaml (comprehensive)
- ✅ Styles.xaml (FsButton variants + FsEntry/FsEditor + FsCard)
- ✅ Validation state styles (`EntryValid`/`EntryInvalid`, `EditorValid`/`EditorInvalid`)

## Design Goals

1. **Complete Coverage:** Generate styles for all current Flagstone controls
2. **Bootstrap Fidelity:** Map all Bootstrap variants to equivalent Flagstone styles
3. **Extensibility:** Easy to add new controls as they're implemented
4. **Validation Support:** Include validation states for form controls (MCT integration)
5. **Naming Convention:** Clear, consistent style naming that follows MAUI patterns

## Proposed Architecture

### 1. Control-to-Bootstrap Mapping

Baseline mapping is implemented directly inside `XamlThemeGenerator` (no separate mapper type yet). The converter uses tokens for most values, and optionally accepts `BootstrapComponentStyles` (when CSS analysis is available) for better sizing/padding.

```csharp
public class ControlStyleMapping
{
    public string ControlType { get; set; } // "FsButton", "FsEntry", etc.
    public string[] BootstrapClasses { get; set; } // CSS classes to extract
    public Dictionary<string, string> PropertyMappings { get; set; } // Bootstrap→MAUI property map
    public string[] Variants { get; set; } // Color variants to generate
    public string[] States { get; set; } // Visual states (Normal, Focused, etc.)
}
```

### 2. Style Generation Strategy

**FsButton Styles (Implemented):**
- Default `TargetType="fs:FsButton"` (Primary)
- Compatibility keys preserved: `OutlinedButton`, `TextButton`
- Semantic variants: `ButtonSecondary`, `ButtonSuccess`, `ButtonDanger`, `ButtonWarning`, `ButtonInfo`, `ButtonLight`, `ButtonDark`
- Outline variants: `ButtonOutlinePrimary`, `ButtonOutlineSecondary`, etc.
- Size variants: `ButtonSmall`, `ButtonLarge`

**FsEntry Styles (Implemented):**
- Default `TargetType="fs:FsEntry"`
- Validation: `EntryValid`, `EntryInvalid`

**FsEditor Styles (Implemented):**
- Default `TargetType="fs:FsEditor"`
- Validation: `EditorValid`, `EditorInvalid`

**FsCard Styles (Implemented):**
- Default `TargetType="fs:FsCard"`

### 3. Property Mapping

Bootstrap CSS properties need to map to MAUI properties:

| Bootstrap Property | MAUI Property | Notes |
|-------------------|---------------|-------|
| `background-color` | `BackgroundColor` | Direct map |
| `color` | `TextColor` | For Button/Entry/Editor |
| `border-color` | `Stroke` (Border) | For wrapper controls |
| `border-width` | `StrokeThickness` (Border) | |
| `border-radius` | `StrokeShape` | Requires RoundRectangle |
| `padding` | `Padding` | May need conversion |
| `font-size` | `FontSize` | Requires px→pt conversion |
| `font-weight` | `FontAttributes` | Map to Bold/None |
| `font-family` | `FontFamily` | May need fallback |
| `box-shadow` | `Shadow` | Complex mapping |

### 4. Implementation Plan

#### Phase 1: Baseline Complete (Implemented)
1. Expand `XamlThemeGenerator` to emit styles for FsButton/FsEntry/FsEditor/FsCard
2. Include validation styles for FsEntry/FsEditor
3. Wire CLI + MCP to include/generated styles output
4. Add unit tests to assert expected styles exist

#### Phase 2: Higher Fidelity (Next)
1. Improve pseudo-class parity: `:hover`, `:active`, `:disabled` → VisualStates
2. Improve focus styling parity for form controls (where MAUI allows)
3. Extend cards to include header/body/footer patterns once Flagstone exposes parts or conventions

#### Phase 3: Composability (Future)
1. Consider a composable style strategy (e.g., size + intent styles that can be combined)
2. Introduce an explicit mapping layer (optional) if the generator logic becomes hard to maintain

#### Phase 4: Validation & Testing (Ongoing)
1. Add fixture outputs for a dark theme (e.g., Darkly) to ensure good coverage
2. Expand tests to validate a few key property mappings (not just presence)

## Implementation Details

### ControlStyleMapper Structure

```csharp
public static class ControlStyleMapper
{
    public static Dictionary<string, ControlStyleMapping> Mappings = new()
    {
        ["FsButton"] = new ControlStyleMapping
        {
            ControlType = "FsButton",
            BootstrapClasses = new[] { "btn", "btn-primary", "btn-secondary", /* ... */ },
            PropertyMappings = new Dictionary<string, string>
            {
                ["background-color"] = "BackgroundColor",
                ["color"] = "TextColor",
                ["border-color"] = "BorderColor",
                ["padding"] = "Padding",
                // ...
            },
            Variants = new[] { "Primary", "Secondary", "Success", "Danger", "Warning", "Info", "Light", "Dark" },
            States = new[] { "Normal", "Pressed", "Disabled", "PointerOver" }
        },
        
        ["FsEntry"] = new ControlStyleMapping
        {
            ControlType = "FsEntry",
            BootstrapClasses = new[] { "form-control", "is-valid", "is-invalid" },
            PropertyMappings = new Dictionary<string, string>
            {
                ["border-color"] = "Stroke", // On Border element
                ["background-color"] = "BackgroundColor", // On Entry
                ["color"] = "TextColor",
                // ...
            },
            Variants = new[] { "Default", "Valid", "Invalid" },
            States = new[] { "Normal", "Focused", "Disabled" }
        },
        
        // Similar for FsEditor, FsCard
    };
}
```

### Enhanced XamlThemeGenerator

```csharp
public class XamlThemeGenerator
{
    private readonly BootstrapComponentStyles _componentStyles;
    
    public void GenerateStyles(string outputPath)
    {
        var styles = new List<string>();
        
        // Generate styles for each control
        foreach (var (controlName, mapping) in ControlStyleMapper.Mappings)
        {
            styles.AddRange(GenerateControlStyles(controlName, mapping));
        }
        
        // Write Styles.xaml with all generated styles
        WriteStylesXaml(outputPath, styles);
    }
    
    private IEnumerable<string> GenerateControlStyles(string controlName, ControlStyleMapping mapping)
    {
        var styles = new List<string>();
        
        switch (controlName)
        {
            case "FsButton":
                styles.AddRange(GenerateButtonStyles(mapping));
                break;
            case "FsEntry":
                styles.AddRange(GenerateEntryStyles(mapping));
                break;
            case "FsEditor":
                styles.AddRange(GenerateEditorStyles(mapping));
                break;
            case "FsCard":
                styles.AddRange(GenerateCardStyles(mapping));
                break;
        }
        
        return styles;
    }
    
    private IEnumerable<string> GenerateButtonStyles(ControlStyleMapping mapping)
    {
        var styles = new List<string>();
        
        // Base button style
        styles.Add(GenerateBaseButtonStyle());
        
        // Color variant styles
        foreach (var variant in mapping.Variants)
        {
            var bootstrapClass = $"btn-{variant.ToLower()}";
            if (_componentStyles.ButtonStyles.ContainsKey(bootstrapClass))
            {
                styles.Add(GenerateButtonVariantStyle(variant, bootstrapClass));
            }
        }
        
        // Outline variant styles
        foreach (var variant in mapping.Variants)
        {
            var bootstrapClass = $"btn-outline-{variant.ToLower()}";
            if (_componentStyles.ButtonStyles.ContainsKey(bootstrapClass))
            {
                styles.Add(GenerateButtonOutlineStyle(variant, bootstrapClass));
            }
        }
        
        // Size variant styles
        if (_componentStyles.ButtonStyles.ContainsKey("btn-sm"))
        {
            styles.Add(GenerateButtonSizeStyle("Small", "btn-sm"));
        }
        if (_componentStyles.ButtonStyles.ContainsKey("btn-lg"))
        {
            styles.Add(GenerateButtonSizeStyle("Large", "btn-lg"));
        }
        
        return styles;
    }
    
    private string GenerateButtonVariantStyle(string variant, string bootstrapClass)
    {
        var style = _componentStyles.ButtonStyles[bootstrapClass];
        
        return $@"
    <Style x:Key=""FsButton{variant}Style"" TargetType=""controls:FsButton"">
        <Setter Property=""BackgroundColor"" Value=""{MapColor(style.BackgroundColor)}"" />
        <Setter Property=""TextColor"" Value=""{MapColor(style.Color)}"" />
        <Setter Property=""BorderColor"" Value=""{MapColor(style.BorderColor)}"" />
        <Setter Property=""Padding"" Value=""{MapThickness(style.Padding)}"" />
        <Setter Property=""CornerRadius"" Value=""{MapCornerRadius(style.BorderRadius)}"" />
        {GenerateVisualStates(bootstrapClass)}
    </Style>";
    }
    
    // Similar methods for Entry, Editor, Card...
}
```

### Visual State Generation

For button hover/active states:

```csharp
private string GenerateVisualStates(string bootstrapClass)
{
    var hoverClass = $"{bootstrapClass}:hover";
    var activeClass = $"{bootstrapClass}:active";
    var disabledClass = $"{bootstrapClass}:disabled";
    
    if (!_componentStyles.ButtonStyles.ContainsKey(hoverClass)) 
        return string.Empty;
        
    var hover = _componentStyles.ButtonStyles[hoverClass];
    var active = _componentStyles.ButtonStyles.ContainsKey(activeClass) 
        ? _componentStyles.ButtonStyles[activeClass] 
        : hover;
    
    return $@"
        <Setter Property=""VisualStateManager.VisualStateGroups"">
            <VisualStateGroupList>
                <VisualStateGroup x:Name=""CommonStates"">
                    <VisualState x:Name=""Normal"" />
                    <VisualState x:Name=""PointerOver"">
                        <VisualState.Setters>
                            <Setter Property=""BackgroundColor"" Value=""{MapColor(hover.BackgroundColor)}"" />
                            <Setter Property=""BorderColor"" Value=""{MapColor(hover.BorderColor)}"" />
                        </VisualState.Setters>
                    </VisualState>
                    <VisualState x:Name=""Pressed"">
                        <VisualState.Setters>
                            <Setter Property=""BackgroundColor"" Value=""{MapColor(active.BackgroundColor)}"" />
                            <Setter Property=""BorderColor"" Value=""{MapColor(active.BorderColor)}"" />
                        </VisualState.Setters>
                    </VisualState>
                </VisualStateGroup>
            </VisualStateGroupList>
        </Setter>";
}
```

## Expected Output

After implementation, running the converter on a Bootstrap theme should generate:

**Styles.xaml:**
```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
                    xmlns:controls="clr-namespace:FlagstoneUI.Core.Controls;assembly=FlagstoneUI.Core">

    <!-- FsButton Styles -->
    <Style x:Key="FsButtonStyle" TargetType="controls:FsButton">
        <!-- Base button properties -->
    </Style>
    
    <Style x:Key="FsButtonPrimaryStyle" TargetType="controls:FsButton" BasedOn="{StaticResource FsButtonStyle}">
        <!-- Primary variant with visual states -->
    </Style>
    
    <Style x:Key="FsButtonSecondaryStyle" TargetType="controls:FsButton" BasedOn="{StaticResource FsButtonStyle}">
        <!-- Secondary variant with visual states -->
    </Style>
    
    <!-- ... all other button variants ... -->
    
    <Style x:Key="FsButtonOutlinePrimaryStyle" TargetType="controls:FsButton" BasedOn="{StaticResource FsButtonStyle}">
        <!-- Outline primary variant -->
    </Style>
    
    <!-- ... all outline variants ... -->
    
    <!-- FsEntry Styles -->
    <Style x:Key="FsEntryStyle" TargetType="controls:FsEntry">
        <!-- Base entry with focus states -->
    </Style>
    
    <Style x:Key="FsEntryValidStyle" TargetType="controls:FsEntry" BasedOn="{StaticResource FsEntryStyle}">
        <!-- Valid state styling -->
    </Style>
    
    <Style x:Key="FsEntryInvalidStyle" TargetType="controls:FsEntry" BasedOn="{StaticResource FsEntryStyle}">
        <!-- Invalid state styling -->
    </Style>
    
    <!-- FsEditor Styles -->
    <Style x:Key="FsEditorStyle" TargetType="controls:FsEditor">
        <!-- Similar to FsEntry -->
    </Style>
    
    <!-- FsCard Styles -->
    <Style x:Key="FsCardStyle" TargetType="controls:FsCard">
        <!-- Card styling -->
    </Style>
    
</ResourceDictionary>
```

## Validation Strategy

1. **Compilation Test:** All generated XAML must compile without errors
2. **Visual Test:** Sample app showing all variants side-by-side
3. **Bootstrap Fidelity:** Visual comparison with actual Bootstrap components
4. **Token Usage:** Verify styles use tokens where appropriate (e.g., `{StaticResource Color.Primary}`)
5. **State Transitions:** Test hover, active, disabled states work correctly

## Future Extensibility

When new controls are added:

1. Add control to FlagstoneUI.Core
2. Define mapping in `ControlStyleMapper`
3. Implement generation method in `XamlThemeGenerator`
4. Re-run converter on existing themes
5. Themes automatically get new control styles

## Open Questions

1. **Default Styles:** Should we generate an implicit default style (no x:Key) for each control?
2. **Token References:** Should generated styles reference tokens or use literal values?
3. **Style Inheritance:** Should variants use `BasedOn` or be independent?
4. **MCT Dependencies:** How to handle validation styles when MCT package isn't referenced?
5. **Platform Differences:** Any Android/iOS/Windows-specific considerations?

## Success Criteria

- [ ] All 4 controls have complete style sets
- [ ] FsButton has all 8 color variants + outlines + sizes
- [ ] FsEntry/FsEditor have base + validation styles
- [ ] FsCard has complete component styles
- [ ] All styles compile and render correctly
- [ ] Sample app demonstrates all variants
- [ ] Documentation updated with style naming conventions
- [ ] Tests validate style generation

## References

- [Bootstrap Button Documentation](https://getbootstrap.com/docs/5.3/components/buttons/)
- [Bootstrap Forms Documentation](https://getbootstrap.com/docs/5.3/forms/overview/)
- [Bootstrap Cards Documentation](https://getbootstrap.com/docs/5.3/components/card/)
- [MAUI Styling Documentation](https://learn.microsoft.com/en-us/dotnet/maui/user-interface/styles/xaml)
- [MAUI Visual States](https://learn.microsoft.com/en-us/dotnet/maui/user-interface/visual-states)
