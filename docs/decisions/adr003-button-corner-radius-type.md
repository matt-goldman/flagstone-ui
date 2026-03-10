# ADR003: Button Corner Radius Type Handling

## Status
Accepted

## Context

During theme development and testing (November 2024), we discovered a critical issue with button corner radius styling: all styled buttons rendered with 90-degree corners (radius=1) at runtime, while a test button with hardcoded `CornerRadius="50"` worked correctly.

### The Problem

.NET MAUI's `Button.CornerRadius` property has type `int` (Int32), while all other dimension-related bindable properties typically accept `double` values. This creates a type mismatch when using `DynamicResource` bindings with our token system.

**Initial token definition (incorrect):**
```xml
<x:Double x:Key="Radius.Large">16</x:Double>
```

**Button style binding (silently fails):**
```xml
<Setter Property="CornerRadius" Value="{DynamicResource Radius.Large}" />
```

The `DynamicResource` binding **silently failed** at runtime because:
1. Button.CornerRadius expects `int` (Int32)
2. Radius.Large resource type is `double` (Double)
3. MAUI's DynamicResource doesn't coerce or warn on type mismatch
4. Property defaults to 1 (near-zero), resulting in square corners

### Investigation Findings

- `FsButton` is a simple `Button` subclass with no platform handlers interfering
- The property itself works correctly (hardcoded values render properly)
- Issue is isolated to `DynamicResource` type binding failure
- This affects all button styles across all themes
- Other controls (`FsCard`, `FsEntry`) use Double-typed dimension properties successfully

## Decision

### Dual Token System

Implement separate token sets for Int32 and Double dimension values:

1. **Original tokens (Double)** - For controls accepting double values:
   ```xml
   <!-- For FsCard, FsEntry, and most dimension properties -->
   <x:Double x:Key="Radius.None">0</x:Double>
   <x:Double x:Key="Radius.ExtraSmall">4</x:Double>
   <x:Double x:Key="Radius.Small">8</x:Double>
   <x:Double x:Key="Radius.Medium">12</x:Double>
   <x:Double x:Key="Radius.Large">16</x:Double>
   <x:Double x:Key="Radius.ExtraLarge">28</x:Double>
   <x:Double x:Key="Radius.Full">9999</x:Double>
   ```

2. **Button-specific tokens (Int32)** - For Button.CornerRadius property:
   ```xml
   <!-- For Button.CornerRadius only -->
   <x:Int32 x:Key="Radius.Button.None">0</x:Int32>
   <x:Int32 x:Key="Radius.Button.ExtraSmall">4</x:Int32>
   <x:Int32 x:Key="Radius.Button.Small">8</x:Int32>
   <x:Int32 x:Key="Radius.Button.Medium">12</x:Int32>
   <x:Int32 x:Key="Radius.Button.Large">16</x:Int32>
   <x:Int32 x:Key="Radius.Button.ExtraLarge">28</x:Int32>
   <x:Int32 x:Key="Radius.Button.Full">9999</x:Int32>
   ```

### Implementation

**Updated button styles across all themes:**
```xml
<!-- Material Theme - Square corners -->
<Style TargetType="fs:FsButton">
    <Setter Property="CornerRadius" Value="{DynamicResource Radius.Button.None}" />
    <!-- ... -->
</Style>

<!-- NovaPop Theme - Rounded corners -->
<Style TargetType="fs:FsButton">
    <Setter Property="CornerRadius" Value="{DynamicResource Radius.Button.Large}" />
    <!-- ... -->
</Style>
```

**Affected themes:**
- ✅ Material (7 button styles) - updated to `Radius.Button.None` for square corners
- ✅ NovaPop (3 button styles) - updated to `Radius.Button.Large`
- ✅ SlatePro (3 button styles) - updated to `Radius.Button.Small`
- ✅ Litera (5 button styles) - updated to `Radius.Button.Large`
- ✅ Brite (3 button styles) - updated to `Radius.Button.Large`
- ✅ Minty (3 button styles) - updated to `Radius.Button.Large`

## Rationale

### Why Not Use Converters?
- Adds complexity and runtime overhead
- Requires developers to remember converter usage
- Doesn't align with token-based API simplicity

### Why Not Modify MAUI?
- Button.CornerRadius type is platform constraint
- Can't change without MAUI framework contribution
- Need solution that works with current MAUI versions

### Why Dual Token System?
- ✅ Clean, declarative approach
- ✅ Type-safe at compile time
- ✅ Clear naming convention (`Radius.Button.*`)
- ✅ No runtime performance impact
- ✅ Follows token-first philosophy
- ✅ Self-documenting for theme authors

## Consequences

### Positive

1. **Corner radius works correctly** - Buttons now render with proper rounded or square corners
2. **Type-safe bindings** - Compile-time type checking prevents silent failures
3. **Clear naming** - `Radius.Button.*` explicitly indicates usage context
4. **Maintains token philosophy** - Theme authors still use semantic tokens, not hardcoded values
5. **Consistent values** - Int32 and Double token values stay synchronized
6. **Theme diversity** - Material uses square corners, other themes use rounded (demonstrates flexibility)

### Negative

1. **Token duplication** - Must maintain two token sets with identical values
2. **Developer awareness** - Theme authors must know to use `Radius.Button.*` for buttons
3. **Documentation burden** - Must explain dual system in theming guide

### Neutral

1. **Precedent set** - If other controls have Int32 dimension properties, same pattern applies
2. **Breaking change for themes** - Existing custom themes need updating (affects pre-release users only)

## Guidance for Theme Authors

**When styling buttons:**
```xml
<!-- ✅ CORRECT: Use Radius.Button.* tokens -->
<Style TargetType="fs:FsButton">
    <Setter Property="CornerRadius" Value="{DynamicResource Radius.Button.Large}" />
</Style>

<!-- ❌ WRONG: Using Double-typed radius token will fail silently -->
<Style TargetType="fs:FsButton">
    <Setter Property="CornerRadius" Value="{DynamicResource Radius.Large}" />
</Style>
```

**When styling cards/entries:**
```xml
<!-- ✅ CORRECT: Use standard Radius.* tokens (Double) -->
<Style TargetType="fs:FsCard">
    <Setter Property="CornerRadius" Value="{DynamicResource Radius.Medium}" />
</Style>
```

## Related Issues

- This issue emerged during theme refinement work
- Discovered through systematic testing with exaggerated corner radius values
- Resolved as part of theme switching epic completion

## References

- `.NET MAUI Button.CornerRadius`: [Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/api/microsoft.maui.controls.button.cornerradius)
- `src/FlagstoneUI.Core/Styles/Tokens.xaml`: Dual token definitions
- `src/FlagstoneUI.Themes.Material/Theme.xaml`: Reference implementation
- Sample theme files: NovaPop, SlatePro, Litera, Brite, Minty

## History

- **2024-11-12**: ADR created after discovering and resolving Button.CornerRadius type mismatch
- **Decision**: Implement dual token system (Radius.* and Radius.Button.*)
- **Status**: Accepted and implemented across all themes
