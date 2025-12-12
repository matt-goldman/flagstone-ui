# Bootstrap Converter Enhancement Plan

## Executive Summary

The Bootstrap converter now generates a **complete `Styles.xaml`** for the currently implemented FlagstoneUI controls (`FsButton`, `FsEntry`, `FsEditor`, `FsCard`), including Bootstrap-inspired **valid/invalid** form control styles. This document tracks what’s implemented and what remains.

**Status (2025-12-12):** Phase 1–4 baseline complete (variants + validation + tests); deeper Bootstrap fidelity (hover/active/focus parity, card subparts, etc.) remains.

## Current State Analysis

### What Works ✅
- **Token Generation**: Successfully extracts and converts Bootstrap variables to Flagstone tokens
- **CSS Analysis**: `BootstrapCssAnalyzer` already extracts all component variants from CSS
- **File Structure**: Generates proper Tokens.xaml, Theme.xaml, Styles.xaml with code-behind

### The Problem ❌
- (Resolved) `Styles.xaml` now includes full control coverage and common variants
- (Resolved) Validation styles are generated (`EntryValid`/`EntryInvalid`, `EditorValid`/`EditorInvalid`)
- (Partially) `BootstrapComponentStyles` is now accepted by the generator for better sizing/padding when CSS analysis is available

### Root Cause
The analyzer extracts complete data (`ButtonPrimary`, `ButtonSuccess`, `FormControl`, etc.) but the generator previously ignored it and created only a minimal hardcoded set.

## Proposed Architecture

### 1. Control-to-Bootstrap Mapping

Create explicit mappings between FlagstoneUI controls and Bootstrap components:

```
FsButton → .btn-primary, .btn-secondary, .btn-success, etc.
          → .btn-outline-primary, .btn-outline-secondary, etc.
          → .btn-lg, .btn-sm
FsEntry  → .form-control, .form-control:focus
          → .is-valid, .is-invalid
FsEditor → .form-control, .form-control:focus (textarea variant)
          → .is-valid, .is-invalid
FsCard   → .card, .card-body, .card-header, .card-footer
```

### 2. Style Generator Refactoring

Replace `AddButtonStyles()` with a control-aware generation system:

**Implemented:** control-aware style generation inside `XamlThemeGenerator`:
- `AddButtonStyles(...)` now emits semantic variants, outline variants, and size variants (while preserving compatibility keys)
- Added: `AddEntryStyles(...)`, `AddEditorStyles(...)`, `AddCardStyles(...)`

**Style Naming Convention:**
```xaml
<!-- FsButton Variants -->
<Style TargetType="fs:FsButton" /> <!-- Default = Primary -->
<Style x:Key="ButtonSecondary" TargetType="fs:FsButton" />
<Style x:Key="ButtonSuccess" TargetType="fs:FsButton" />
<Style x:Key="ButtonDanger" TargetType="fs:FsButton" />
<Style x:Key="ButtonWarning" TargetType="fs:FsButton" />
<Style x:Key="ButtonInfo" TargetType="fs:FsButton" />
<Style x:Key="ButtonLight" TargetType="fs:FsButton" />
<Style x:Key="ButtonDark" TargetType="fs:FsButton" />

<!-- Outline Variants -->
<Style x:Key="ButtonOutlinePrimary" TargetType="fs:FsButton" />
<Style x:Key="ButtonOutlineSecondary" TargetType="fs:FsButton" />
<!-- etc. -->

<!-- Size Variants -->
<Style x:Key="ButtonLarge" TargetType="fs:FsButton" />
<Style x:Key="ButtonSmall" TargetType="fs:FsButton" />

<!-- FsEntry Variants -->
<Style TargetType="fs:FsEntry" /> <!-- Default -->
<Style x:Key="EntryValid" TargetType="fs:FsEntry" />
<Style x:Key="EntryInvalid" TargetType="fs:FsEntry" />

<!-- FsEditor Variants -->
<Style TargetType="fs:FsEditor" /> <!-- Default -->
<Style x:Key="EditorValid" TargetType="fs:FsEditor" />
<Style x:Key="EditorInvalid" TargetType="fs:FsEditor" />

<!-- FsCard -->
<Style TargetType="fs:FsCard" /> <!-- Default -->
```

### 3. Property Mapping

Map Bootstrap CSS properties to MAUI control properties:

#### FsButton Mapping
```
Bootstrap CSS           → MAUI Property
-------------------------------------------
background-color        → BackgroundColor
color                   → TextColor
border-color            → BorderColor
border-width            → BorderWidth
border-radius           → CornerRadius (convert to int)
padding                 → Padding (parse to Thickness)
font-size               → FontSize
font-weight             → FontAttributes (≥700 = Bold)
```

#### FsEntry/FsEditor Mapping
```
Bootstrap CSS           → MAUI Property
-------------------------------------------
background-color        → Background (Brush)
color                   → TextColor
border-color            → BorderBrush (Brush)
border-width            → BorderWidth
border-radius           → CornerRadius
padding                 → Padding
font-size               → FontSize
```

#### FsCard Mapping
```
Bootstrap CSS           → MAUI Property
-------------------------------------------
background-color        → BackgroundColor
border-color            → BorderColor
border-width            → BorderWidth
border-radius           → CornerRadius
padding                 → Padding
```

### 4. Enhanced BootstrapComponentStyles

Add validation-related styles to the model:

```csharp
public class BootstrapComponentStyles
{
    // ... existing properties ...
    
    // Validation states
    public ComputedStyle? FormControlValid { get; set; }
    public ComputedStyle? FormControlInvalid { get; set; }
    public ComputedStyle? FormControlValidFocus { get; set; }
    public ComputedStyle? FormControlInvalidFocus { get; set; }
}
```

Update `BootstrapCssAnalyzer` to extract these:

```csharp
// In AnalyzeComponents():
FormControlValid = ExtractStyle(stylesheet, ".form-control.is-valid"),
FormControlInvalid = ExtractStyle(stylesheet, ".form-control.is-invalid"),
FormControlValidFocus = ExtractStyle(stylesheet, ".form-control.is-valid:focus"),
FormControlInvalidFocus = ExtractStyle(stylesheet, ".form-control.is-invalid:focus"),
```

**Implemented:**
- Model updated with the validation properties
- Analyzer updated to extract `::placeholder` plus valid/invalid selectors (and focus variants)

### 5. Implementation Plan

#### Phase 1: Extend BootstrapComponentStyles
- [x] Add validation state properties
- [x] Update `BootstrapCssAnalyzer.AnalyzeComponents()` to extract validation styles
- [x] Verify extraction with Flatly inputs (SCSS variables mode generates styles; CSS analysis path carries component sizing)

#### Phase 2: Create Style Generators
- [x] Implement full button variants (semantic + outline + sizes)
- [x] Implement FsEntry base + validation styles
- [x] Implement FsEditor base + validation styles
- [x] Implement FsCard base style

#### Phase 3: Refactor XamlThemeGenerator
- [x] Expand `AddButtonStyles()` and preserve existing keys (`OutlinedButton`, `TextButton`)
- [x] Update `GenerateStylesXaml()` to include FsEntry/FsEditor/FsCard
- [x] Add helper methods for basic CSS→MAUI conversions (padding/length parsing) where component styles are available

#### Phase 4: Testing & Validation
- [x] Add unit tests asserting required style keys and sections exist
- [x] Regenerate `test-output/` fixtures to include expanded `Styles.xaml`
- [ ] Add a Darkly validation run (optional fixture update)
- [ ] Improve state fidelity for focus/hover/pressed/disabled (see “Future Enhancements”)

## Expected Output

### Complete Styles.xaml Structure

```xaml
<?xml version="1.0" encoding="utf-16" ?>
<ResourceDictionary
    x:Class="FlagstoneUI.Resources.FlatlyStyles"
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:fs="clr-namespace:FlagstoneUI.Core.Controls;assembly=FlagstoneUI.Core">
    
    <ResourceDictionary.MergedDictionaries>
        <ResourceDictionary Source="Tokens.xaml" />
    </ResourceDictionary.MergedDictionaries>

    <!-- ========== FsButton Styles ========== -->
    
    <!-- Default (Primary) -->
    <Style TargetType="fs:FsButton">
        <!-- Properties from .btn-primary -->
    </Style>
    
    <!-- Semantic Variants -->
    <Style x:Key="ButtonSecondary" TargetType="fs:FsButton">
        <!-- Properties from .btn-secondary -->
    </Style>
    
    <Style x:Key="ButtonSuccess" TargetType="fs:FsButton">
        <!-- Properties from .btn-success -->
    </Style>
    
    <!-- ... ButtonDanger, ButtonWarning, ButtonInfo, ButtonLight, ButtonDark ... -->
    
    <!-- Outline Variants -->
    <Style x:Key="ButtonOutlinePrimary" TargetType="fs:FsButton">
        <!-- Properties from .btn-outline-primary -->
    </Style>
    
    <!-- ... all outline variants ... -->
    
    <!-- Size Variants -->
    <Style x:Key="ButtonLarge" TargetType="fs:FsButton">
        <!-- Properties from .btn-lg -->
    </Style>
    
    <Style x:Key="ButtonSmall" TargetType="fs:FsButton">
        <!-- Properties from .btn-sm -->
    </Style>
    
    <!-- ========== FsEntry Styles ========== -->
    
    <Style TargetType="fs:FsEntry">
        <!-- Properties from .form-control -->
    </Style>
    
    <Style x:Key="EntryValid" TargetType="fs:FsEntry">
        <!-- Properties from .form-control.is-valid -->
    </Style>
    
    <Style x:Key="EntryInvalid" TargetType="fs:FsEntry">
        <!-- Properties from .form-control.is-invalid -->
    </Style>
    
    <!-- ========== FsEditor Styles ========== -->
    
    <Style TargetType="fs:FsEditor">
        <!-- Properties from .form-control (textarea) -->
    </Style>
    
    <Style x:Key="EditorValid" TargetType="fs:FsEditor">
        <!-- Properties from .form-control.is-valid -->
    </Style>
    
    <Style x:Key="EditorInvalid" TargetType="fs:FsEditor">
        <!-- Properties from .form-control.is-invalid -->
    </Style>
    
    <!-- ========== FsCard Styles ========== -->
    
    <Style TargetType="fs:FsCard">
        <!-- Properties from .card -->
    </Style>
    
</ResourceDictionary>
```

## Integration with MCT

The validation styles (`EntryValid`, `EntryInvalid`) will work seamlessly with the MCT integration package:

```xaml
<fs:FsEntry 
    Text="{Binding Email}"
    Style="{DynamicResource EntryValid}">
    <fs:FsEntry.Behaviors>
        <mct:ValidationBehaviorAdapter 
            IsValid="{Binding IsEmailValid}" />
    </fs:FsEntry.Behaviors>
</fs:FsEntry>
```

## Benefits

1. **Complete Themes**: Every Bootstrap theme generates complete styles for all controls
2. **Semantic Consistency**: Button colors match Bootstrap's semantic palette (success, danger, etc.)
3. **Validation Support**: Ready-to-use validation styles for form controls
4. **Maintainability**: Adding new controls just requires adding a new generator method
5. **Token Usage**: All styles reference tokens, making theme customization easy

## Migration Path

**Backward Compatibility**: The default (no x:Key) styles remain the same, so existing code won't break.

**New Features**: Users can now use:
```xaml
<fs:FsButton Style="{StaticResource ButtonSuccess}" Text="Save" />
<fs:FsEntry Style="{StaticResource EntryInvalid}" />
```

## Future Enhancements

- Badge styles (when FsBadge is implemented)
- Alert styles (when FsAlert is implemented)
- Navigation styles (when FsTabBar, FsNavigationBar are implemented)
- Size modifier styles that can be combined with semantic styles (composable style approach)
- Higher-fidelity interactive states (Pressed/PointerOver/Disabled parity with Bootstrap pseudo-classes)
- Form-control focus styling parity (e.g., focus ring/outline equivalents where feasible in MAUI)
- Card subparts (header/body/footer) once Flagstone has explicit parts or conventions
