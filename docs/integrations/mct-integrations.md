# MAUI Community Toolkit (MCT) Integrations

FlagstoneUI provides optional integrations with the [MAUI Community Toolkit](https://github.com/CommunityToolkit/Maui) through the `FlagstoneUI.Integrations.MCT` package. These integrations bridge FlagstoneUI controls with MCT's powerful behaviors, animations, and other features.

## Overview

The `FlagstoneUI.Integrations.MCT` package is an **optional dependency** that provides:

- **ValidationBehaviorAdapter**: Enables MCT validation behaviors to work with `FsEntry` controls
- **FsEditorBorderAnimation**: Animated gradient border effects for `FsEditor` controls

## Installation

```bash
# Install the MCT integration package
dotnet add package FlagstoneUI.Integrations.MCT

# This will automatically install CommunityToolkit.Maui as a dependency
```

### Package Dependencies

- **FlagstoneUI.Core**: Core FlagstoneUI controls
- **CommunityToolkit.Maui**: MAUI Community Toolkit library

## Features

### ValidationBehaviorAdapter

The `ValidationBehaviorAdapter` allows you to use any `ValidationBehavior` from the MAUI Community Toolkit with `FsEntry` controls, providing seamless validation integration.

#### How It Works

The adapter attaches to an `FsEntry` control and internally connects the MCT validation behavior to the inner `Entry` control. It monitors the `IsValid` property and applies styles or visual states based on validation results.

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Behavior` | `ValidationBehavior` | The MCT validation behavior to use |
| `ValidStyle` | `Style` | Style to apply when validation passes |
| `InvalidStyle` | `Style` | Style to apply when validation fails |
| `Flags` | `ValidationFlags` | When to validate (e.g., `ValidateOnValueChanged`) |

#### Usage Example

```xaml
xmlns:fs="clr-namespace:FlagstoneUI.Core.Controls;assembly=FlagstoneUI.Core"
xmlns:mct="http://schemas.microsoft.com/dotnet/2022/maui/toolkit"
xmlns:int="clr-namespace:FlagstoneUI.Integrations.MCT.Behaviors;assembly=FlagstoneUI.Integrations.MCT"

<!-- Define validation styles -->
<ContentPage.Resources>
    <Style x:Key="ValidStyle" TargetType="fs:FsEntry">
        <Setter Property="BorderBrush" Value="Green" />
        <Setter Property="BorderWidth" Value="2" />
    </Style>

    <Style x:Key="InvalidStyle" TargetType="fs:FsEntry">
        <Setter Property="BorderBrush" Value="Red" />
        <Setter Property="BorderWidth" Value="2" />
    </Style>
</ContentPage.Resources>

<!-- Email validation -->
<fs:FsEntry Placeholder="Enter your email" Keyboard="Email">
    <fs:FsEntry.Behaviors>
        <int:ValidationBehaviorAdapter
            ValidStyle="{StaticResource ValidStyle}"
            InvalidStyle="{StaticResource InvalidStyle}"
            Behavior="{mct:EmailValidationBehavior}"
            Flags="ValidateOnValueChanged" />
    </fs:FsEntry.Behaviors>
</fs:FsEntry>
```

#### Available MCT Validators

The MAUI Community Toolkit provides several built-in validators:

- **EmailValidationBehavior**: Validates email addresses
- **NumericValidationBehavior**: Validates numeric input (min/max values)
- **RequiredStringValidationBehavior**: Ensures non-empty input
- **TextValidationBehavior**: Custom regex-based validation
- **UriValidationBehavior**: Validates URIs/URLs
- **CharactersValidationBehavior**: Character count validation
- **CompareValidationBehavior**: Compares two values
- **MultiValidationBehavior**: Combines multiple validators

#### Advanced Example: Multiple Validators

```xaml
<fs:FsEntry Placeholder="Password (min 8 characters)" IsPassword="True">
    <fs:FsEntry.Behaviors>
        <int:ValidationBehaviorAdapter
            ValidStyle="{StaticResource ValidStyle}"
            InvalidStyle="{StaticResource InvalidStyle}"
            Flags="ValidateOnValueChanged">
            <int:ValidationBehaviorAdapter.Behavior>
                <mct:MultiValidationBehavior>
                    <mct:RequiredStringValidationBehavior />
                    <mct:CharactersValidationBehavior
                        CharacterType="Any"
                        MinimumCharacterCount="8" />
                </mct:MultiValidationBehavior>
            </int:ValidationBehaviorAdapter.Behavior>
        </int:ValidationBehaviorAdapter>
    </fs:FsEntry.Behaviors>
</fs:FsEntry>
```

#### Custom Validator Example

```xaml
<fs:FsEntry Placeholder="Enter a number between 1 and 100">
    <fs:FsEntry.Behaviors>
        <int:ValidationBehaviorAdapter
            ValidStyle="{StaticResource ValidStyle}"
            InvalidStyle="{StaticResource InvalidStyle}"
            Flags="ValidateOnValueChanged">
            <int:ValidationBehaviorAdapter.Behavior>
                <mct:NumericValidationBehavior
                    MinimumValue="1"
                    MaximumValue="100" />
            </int:ValidationBehaviorAdapter.Behavior>
        </int:ValidationBehaviorAdapter>
    </fs:FsEntry.Behaviors>
</fs:FsEntry>
```

### FsEditorBorderAnimation

The `FsEditorBorderAnimation` provides an animated gradient border effect for `FsEditor` controls. This is useful for creating attention-grabbing inputs like AI chat interfaces or premium features.

#### How It Works

The animation rotates gradient stops around a diagonal line, creating a flowing color effect. It's based on the MCT's `BaseAnimation<T>` class and can be triggered on focus events.

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Gradient` | `GradientBrush` | The gradient brush to animate |
| `Length` | `uint` | Animation duration in milliseconds (inherited from BaseAnimation) |

#### Usage Example

```xaml
xmlns:fs="clr-namespace:FlagstoneUI.Core.Controls;assembly=FlagstoneUI.Core"
xmlns:mct="http://schemas.microsoft.com/dotnet/2022/maui/toolkit"
xmlns:anim="clr-namespace:FlagstoneUI.Integrations.MCT.Animations;assembly=FlagstoneUI.Integrations.MCT"

<!-- Define the animated gradient -->
<ContentPage.Resources>
    <anim:FsEditorBorderAnimation x:Key="AiBorderAnimation" Length="3000">
        <anim:FsEditorBorderAnimation.Gradient>
            <LinearGradientBrush StartPoint="0,0" EndPoint="1,1">
                <GradientStop Color="Gold" Offset="0.0" />
                <GradientStop Color="Orange" Offset="0.25" />
                <GradientStop Color="Goldenrod" Offset="0.5" />
                <GradientStop Color="DarkGoldenrod" Offset="0.75" />
                <GradientStop Color="Gold" Offset="1.0" />
            </LinearGradientBrush>
        </anim:FsEditorBorderAnimation.Gradient>
    </anim:FsEditorBorderAnimation>
</ContentPage.Resources>

<fs:FsEditor
    x:Name="AiEditor"
    Placeholder="🤖 Ask me anything..."
    Focused="OnAiEditorFocused"
    Unfocused="OnAiEditorUnfocused" />
```

```csharp
using FlagstoneUI.Integrations.MCT.Animations;

private CancellationTokenSource? _animationCts;
private FsEditorBorderAnimation? _borderAnimation;

private async void OnAiEditorFocused(object? sender, EventArgs e)
{
    _animationCts = new CancellationTokenSource();
    _borderAnimation = (FsEditorBorderAnimation)Resources["AiBorderAnimation"];

    try
    {
        await _borderAnimation.Animate(AiEditor, _animationCts.Token);
    }
    catch (OperationCanceledException)
    {
        // Animation was cancelled (expected on unfocus)
    }
}

private void OnAiEditorUnfocused(object? sender, EventArgs e)
{
    _animationCts?.Cancel();
    _animationCts = null;
}
```

#### Best Practices for Border Animation

1. **Gradient Design**: Use 3-5 evenly-spaced gradient stops with the first color repeated at offset 1.0 for seamless looping
2. **Performance**: The animation runs at ~20 fps (50ms frame delay) to balance smoothness and performance
3. **Cancellation**: Always cancel animations on unfocus to prevent memory leaks
4. **Duration**: Typical values are 2000-5000ms for a smooth, noticeable effect
5. **Color Choice**: Use colors that complement your theme and provide good contrast

#### Complete AI Editor Example

See the sample app (`ControlShowcasePage.xaml`) for a complete implementation:

```xaml
<!-- Style definition -->
<Style TargetType="fs:FsEditor" x:Key="AiEditorStyle">
    <Setter Property="Background">
        <Setter.Value>
            <LinearGradientBrush StartPoint="0,0" EndPoint="1,1">
                <GradientStop Color="#1A1A2E" Offset="0.0" />
                <GradientStop Color="#16213E" Offset="1.0" />
            </LinearGradientBrush>
        </Setter.Value>
    </Setter>
    <Setter Property="BorderBrush" Value="Goldenrod" />
    <Setter Property="BorderWidth" Value="2" />
    <Setter Property="CornerRadius" Value="8" />
    <Setter Property="TextColor" Value="White" />
    <Setter Property="PlaceholderColor" Value="LightGray" />
    <Setter Property="Padding" Value="16,12" />
    <Setter Property="MinimumHeightRequest" Value="88" />
</Style>

<!-- Usage with overlaid button -->
<Grid>
    <fs:FsEditor
        Placeholder="🤖 Ask me anything..."
        x:Name="AiEditor"
        Style="{DynamicResource AiEditorStyle}"
        Focused="AiEditor_OnFocused"
        Unfocused="AiEditor_OnUnfocused" />

    <Button
        Background="Goldenrod"
        CornerRadius="5"
        HorizontalOptions="End"
        VerticalOptions="End"
        Margin="5,10">
        <Button.ImageSource>
            <FontImageSource
                FontFamily="FluentIcons"
                Glyph="{DynamicResource IconSparkle}"
                Size="48"
                Color="Gold" />
        </Button.ImageSource>
    </Button>
</Grid>
```

## Architecture

### ValidationBehaviorAdapter Implementation

The adapter works by:

1. Attaching to the `FsEntry` control
2. Finding the inner `Entry` control by name (`InnerEntry`)
3. Attaching the MCT validation behavior to the inner `Entry`
4. Listening for `PropertyChanged` events on the validation behavior
5. Updating the `FsEntry` visual state and style based on `IsValid`

This design maintains the separation between FlagstoneUI's custom controls and the MCT's validation system while providing seamless integration.

### FsEditorBorderAnimation Implementation

The animation works by:

1. Storing the original border brush
2. Creating a continuous loop that:
   - Rotates gradient stops by a small increment each frame
   - Reconstructs the gradient brush with new stop positions
   - Updates the editor's `BorderBrush` property
3. Respecting cancellation tokens for proper cleanup
4. Restoring the original brush when cancelled

The animation runs on the UI thread via `MainThread.InvokeOnMainThreadAsync` and checks for cancellation before each update.

## Design Decisions

### Why a Separate Package?

The MCT integration is provided as a separate package (`FlagstoneUI.Integrations.MCT`) rather than being included in `FlagstoneUI.Core` because:

1. **Optional Dependency**: Not all users need or want the MAUI Community Toolkit dependency
2. **Package Size**: Keeps the core package lightweight
3. **Clear Separation**: Makes it clear which features require MCT
4. **Versioning**: Allows independent versioning of integration code
5. **Modularity**: Users can mix and match integration packages as needed

### Adapter Pattern

The `ValidationBehaviorAdapter` uses the adapter pattern to bridge between:

- **FsEntry**: FlagstoneUI's wrapper control with custom styling
- **Entry**: The inner MAUI control that MCT behaviors expect
- **ValidationBehavior**: MCT's validation system

This allows users to leverage MCT's mature validation ecosystem while using FlagstoneUI's themed controls.

## Future Integrations

Potential future additions to the MCT integration package:

- **Popup Integration**: Adapters for MCT popups with FlagstoneUI styling
- **Additional Animations**: More animation types for other Flagstone controls
- **Converter Helpers**: Utilities for MCT converters with token values
- **Behavior Extensions**: Additional behavior adapters for other Flagstone controls

## Troubleshooting

### Validation Not Working

If validation isn't working as expected:

1. Ensure `FlagstoneUI.Integrations.MCT` package is installed
2. Check that namespace declarations are correct
3. Verify `Flags` property is set (default is `ValidateOnValueChanged`)
4. Ensure styles target `fs:FsEntry` not `Entry`

### Animation Not Running

If border animation isn't running:

1. Verify the animation is defined in resources
2. Check that `Focused` and `Unfocused` event handlers are connected
3. Ensure gradient has at least 2 stops with closing color at offset 1.0
4. Verify cancellation token is being managed correctly

### Performance Issues

If experiencing performance problems:

1. Reduce animation frame rate by adjusting `frameDelayMs` (currently 50ms)
2. Simplify gradient (use fewer gradient stops)
3. Cancel animations when not needed (on unfocus, navigation away)
4. Consider using animations only for premium/focal UI elements

## See Also

- [FsEntry Control](../controls/FsEntry.md) - Single-line text input control
- [FsEditor Control](../controls/FsEditor.md) - Multi-line text input control
- [MAUI Community Toolkit Documentation](https://learn.microsoft.com/dotnet/communitytoolkit/maui/)
- [MCT Behaviors](https://learn.microsoft.com/dotnet/communitytoolkit/maui/behaviors/)
- [MCT Animations](https://learn.microsoft.com/dotnet/communitytoolkit/maui/animations/)

## Sample Code

For complete working examples, see:

- `samples/FlagstoneUI.SampleApp/Pages/ControlShowcasePage.xaml` - Validation and animation examples
- `src/FlagstoneUI.Integrations.MCT/` - Source code for integrations
