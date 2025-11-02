# Visual State Pattern in Flagstone UI

## Overview

Flagstone UI controls use a **theme-driven visual state pattern** where controls trigger state changes programmatically, but themes define the visual appearance for each state. This separation of concerns allows maximum flexibility for theme authors while keeping control logic simple and focused.

## Architecture

### Control Responsibilities

Controls are responsible for:

- Detecting user interaction (focus, hover, click, etc.)
- Calling `VisualStateManager.GoToState(this, stateName)` to signal state changes
- **NOT** defining how visual states look

### Theme Responsibilities

Themes are responsible for:

- Defining `VisualStateManager.VisualStateGroups` in control styles
- Specifying visual appearance for each state using `Setter` elements
- Customizing transitions, animations, and visual effects

## Benefits

1. **Clean Separation** - Control logic is independent of visual design
2. **Theme Flexibility** - Each theme can style states completely differently
3. **No Coupling** - No hardcoded colors, sizes, or visual properties in controls
4. **Composition-Friendly** - States target the wrapper where all BindableProperties are exposed
5. **Easy Testing** - Visual behavior can be tested by triggering state changes

## Implementation Example

### Control Code (FsEntry.xaml.cs)

```csharp
private void BorderlessEntry_Focused(object sender, FocusEventArgs e)
{
    // Signal the state change - theme handles the visuals
    VisualStateManager.GoToState(this, VisualStateManager.CommonStates.Focused);
    Focused?.Invoke(this, EventArgs.Empty);
}

private void BorderlessEntry_Unfocused(object sender, FocusEventArgs e)
{
    VisualStateManager.GoToState(this, VisualStateManager.CommonStates.Normal);
    Unfocused?.Invoke(this, EventArgs.Empty);
}
```

**Key points:**

- State change triggered on the wrapper (`this` = ContentView)
- All BindableProperties are on the wrapper, making them targetable by visual states
- No visual styling in control code
- Uses official .NET MAUI `VisualStateManager.CommonStates` constants

### Theme Style (Material Theme)

```xaml
<Style TargetType="fs:FsEntry">
    <!-- Base appearance -->
    <Setter Property="Background">
        <Setter.Value>
            <SolidColorBrush Color="{AppThemeBinding 
                Light={DynamicResource Color.SurfaceVariant}, 
                Dark={DynamicResource Color.SurfaceVariant.Dark}}" />
        </Setter.Value>
    </Setter>
    <Setter Property="BorderBrush">
        <Setter.Value>
            <SolidColorBrush Color="Transparent" />
        </Setter.Value>
    </Setter>
    
    <!-- Visual state definitions -->
    <Setter Property="VisualStateManager.VisualStateGroups">
        <VisualStateGroupList>
            <VisualStateGroup x:Name="FocusStates">
                <VisualState x:Name="Focused">
                    <VisualState.Setters>
                        <Setter Property="BorderBrush">
                            <Setter.Value>
                                <SolidColorBrush Color="{AppThemeBinding 
                                    Light={DynamicResource Color.Primary}, 
                                    Dark={DynamicResource Color.Primary.Dark}}" />
                            </Setter.Value>
                        </Setter>
                        <Setter Property="BorderWidth" Value="2" />
                    </VisualState.Setters>
                </VisualState>
                <!-- Normal state is empty - base style properties apply -->
                <VisualState x:Name="Normal" />
            </VisualStateGroup>
        </VisualStateGroupList>
    </Setter>
</Style>
```

**Key points:**

- Visual states target properties on the control wrapper
- `AppThemeBinding` + `DynamicResource` provides dark mode support
- **Empty Normal state** - base style defines the normal appearance, visual states only define deviations
- CSS-aligned pattern: base styles = defaults, states = pseudo-classes like `:focus`
- Theme can completely customize the visual treatment
- Different themes can have radically different focus indicators

## Standard Visual State Names

Flagstone UI uses the official .NET MAUI `VisualStateManager.CommonStates` constants for consistency with the platform:

### Common States (from .NET MAUI)

- **`VisualStateManager.CommonStates.Normal`** - Default state, no special interaction
- **`VisualStateManager.CommonStates.Disabled`** - Control is disabled/inactive
- **`VisualStateManager.CommonStates.Focused`** - Control has input focus
- **`VisualStateManager.CommonStates.PointerOver`** - Mouse/pointer is over the control (hover, desktop-only)
- **`VisualStateManager.CommonStates.Selected`** - Item is selected (for selectable controls)

**Note:** Flagstone UI controls transition to `Normal` state when unfocused, rather than defining a separate "Unfocused" state.

### Reference

See [.NET MAUI VisualStateManager.CommonStates API](https://learn.microsoft.com/en-us/dotnet/api/microsoft.maui.controls.visualstatemanager.commonstates) for the official constants.

## Platform Considerations

### Hover/PointerOver States

Hover states are **desktop-only** (Windows, macOS, Mac Catalyst). Mobile platforms (iOS, Android) don't have hover interaction.

**Implementation pattern:**

```csharp
#if WINDOWS || MACCATALYST
private void OnPointerEntered(object sender, PointerEventArgs e)
{
    VisualStateManager.GoToState(this, VisualStateManager.CommonStates.PointerOver);
}

private void OnPointerExited(object sender, PointerEventArgs e)
{
    VisualStateManager.GoToState(this, VisualStateManager.CommonStates.Normal);
}

private void OnPointerExited(object sender, PointerEventArgs e)
{
    VisualStateManager.GoToState(this, VisualStateManager.CommonStates.Normal);
}
#endif
```

## Using Brush Properties

Controls should use **`Brush`** properties rather than **`Color`** properties for maximum flexibility:

### ✅ Recommended

```csharp
public Brush BorderBrush { get; set; }
public Brush Background { get; set; }
```

### ❌ Avoid

```csharp
public Color BorderColor { get; set; }
public Color BackgroundColor { get; set; }
```

### Why Brush is Better

- **Gradients** - `LinearGradientBrush`, `RadialGradientBrush`
- **Images** - `ImageBrush` for textured backgrounds
- **Patterns** - Complex visual effects
- **CSS Translation** - Easier to map from CSS gradients
- **Future-proof** - Supports advanced visual effects

## CSS/Tailwind Integration

This pattern enables **CSS-to-XAML translation** tools to generate visual states from CSS pseudo-classes. The empty Normal state pattern directly mirrors CSS behavior where base styles define defaults and pseudo-classes define deviations.

### CSS Input

```css
.input {
  /* Base styles = Normal state in XAML */
  border: 1px solid var(--color-slate-300);
  background: var(--color-white);
}

.input:focus {
  /* Pseudo-class = Visual State in XAML */
  border: 2px solid var(--color-blue-500);
  box-shadow: 0 0 0 3px var(--color-blue-100);
}

/* No .input:normal needed - same as XAML! */
```

### Generated XAML Visual State

```xaml
<!-- Base style properties = Normal state -->
<Setter Property="BorderBrush" Value="{DynamicResource ColorSlate300}" />
<Setter Property="BorderWidth" Value="1" />
<Setter Property="Background" Value="{DynamicResource ColorWhite}" />

<!-- Only define deviations -->
<Setter Property="VisualStateManager.VisualStateGroups">
    <VisualStateGroupList>
        <VisualStateGroup x:Name="FocusStates">
            <VisualState x:Name="Focused">
                <VisualState.Setters>
                    <Setter Property="BorderBrush" Value="{DynamicResource ColorBlue500}" />
                    <Setter Property="BorderWidth" Value="2" />
                    <Setter Property="Shadow">
                        <Setter.Value>
                            <Shadow Brush="{DynamicResource ColorBlue100}" 
                                    Radius="3" Opacity="1" />
                        </Setter.Value>
                    </Setter>
                </VisualState.Setters>
            </VisualState>
            <!-- Empty Normal state - reverts to base style -->
            <VisualState x:Name="Normal" />
        </VisualStateGroup>
    </VisualStateGroupList>
</Setter>
```

**Conversion Strategy:**

- CSS base styles → XAML base style `<Setter>` elements
- CSS pseudo-classes (`:focus`, `:hover`, `:active`) → XAML `<VisualState>` elements
- No explicit Normal state setters needed (empty state)
- Brush properties support CSS gradients (`linear-gradient` → `LinearGradientBrush`)
- CSS variables → XAML `DynamicResource` tokens

See [TailwindXamlThemeConverter](https://github.com/matt-goldman/TailwindXamlThemeConverter) for tooling that enables this workflow.

## Testing Visual States

Visual states can be tested by programmatically triggering state changes:

```csharp
[Fact]
public void Entry_ShouldHaveCorrectBorderWhenFocused()
{
    // Arrange
    var entry = new FsEntry();
    
    // Act
    VisualStateManager.GoToState(entry, VisualStateManager.CommonStates.Focused);
    
    // Assert
    // Visual state system will apply the Focused state setters
    // Theme-specific assertions can verify expected appearance
}
```

## Guidelines for Control Authors

1. **Signal, Don't Style** - Call `GoToState()` but don't define visual appearance
2. **Use .NET MAUI Constants** - Reference `VisualStateManager.CommonStates.*` for consistency with the platform
3. **Document States** - List supported states in control documentation
4. **Test State Transitions** - Verify state changes work across platforms
5. **Consider Platform Limits** - Use conditional compilation for desktop-only features

## Guidelines for Theme Authors

1. **Define All States** - Provide visual treatment for every state the control supports
2. **Use Tokens** - Reference design tokens via `DynamicResource` for consistency
3. **Support Dark Mode** - Use `AppThemeBinding` for light/dark variants
4. **Test Transitions** - Verify state changes look smooth and correct
5. **Be Creative** - Visual states are your canvas - customize freely!

## Related Documentation

- [Control Implementation Guide](control-implementation-guide.md) - General control authoring patterns
- [Token System](tokens.md) - Design token architecture
- [Material Theme](../src/FlagstoneUI.Themes.Material/Theme.xaml) - Reference implementation
- [TailwindXamlThemeConverter](https://github.com/matt-goldman/TailwindXamlThemeConverter) - CSS conversion tooling
