# FsButton Control

The `FsButton` control is a themeable button that extends the .NET MAUI `Button` with Flagstone UI styling and token integration. Unlike `FsEntry`, `FsButton` is a simple subclass rather than a wrapper control, as the MAUI Button already provides comprehensive styling capabilities.

## Features

- **Complete Native Support**: Leverages all .NET MAUI Button properties without abstraction
- **Border Styling**: Configurable border color, width, and corner radius
- **Typography Control**: Full font customization (size, family, attributes)
- **Token Integration**: Seamless theming through DynamicResource bindings
- **Command Pattern**: Built-in ICommand support for MVVM scenarios
- **Image Support**: Can display text, images, or both

## Properties

All standard .NET MAUI Button properties are available. Key styling properties include:

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Text` | `string` | `null` | Button text content |
| `TextColor` | `Color` | Platform default | Color of the button text |
| `BackgroundColor` | `Color` | Platform default | Background color of the button |
| `BorderColor` | `Color` | `Transparent` | Border color (requires BorderWidth > 0) |
| `BorderWidth` | `double` | `0` | Border width in device-independent units |
| `CornerRadius` | `int` | Platform default | Corner radius in device-independent units |
| `FontSize` | `double` | Platform default | Font size for button text |
| `FontFamily` | `string` | Platform default | Font family name |
| `FontAttributes` | `FontAttributes` | `None` | Text style (Bold, Italic, or both) |
| `Padding` | `Thickness` | Platform default | Inner padding |
| `Command` | `ICommand` | `null` | Command executed when button is clicked |
| `CommandParameter` | `object` | `null` | Parameter passed to Command |
| `ImageSource` | `ImageSource` | `null` | Image displayed on the button |

## Usage Examples

### Basic Button

```xaml
<fs:FsButton Text="Click Me" />
```

### Styled Button

```xaml
<fs:FsButton
    Text="Sign In"
    BackgroundColor="{DynamicResource Color.Primary}"
    TextColor="{DynamicResource Color.OnPrimary}"
    BorderColor="{DynamicResource Color.Primary}"
    BorderWidth="2"
    CornerRadius="{DynamicResource Radius.Button.Small}"
    Padding="16,12"
    FontSize="16"
    FontAttributes="Bold" />
```

> **Note**: Always use `Radius.Button.*` tokens for CornerRadius. The Button.CornerRadius property requires Int32 type.

### Button with Command (MVVM)

```xaml
<fs:FsButton
    Text="Submit"
    Command="{Binding SubmitCommand}"
    CommandParameter="{Binding CurrentItem}"
    BackgroundColor="{DynamicResource Color.Secondary}"
    TextColor="{DynamicResource Color.OnSecondary}" />
```

### Outlined Button

```xaml
<fs:FsButton
    Text="Cancel"
    BackgroundColor="Transparent"
    BorderColor="{DynamicResource Color.Outline}"
    BorderWidth="1"
    TextColor="{DynamicResource Color.OnSurface}"
    CornerRadius="{DynamicResource Radius.Button.ExtraSmall}"
    Padding="20,10" />
```

### Button with Image

```xaml
<fs:FsButton
    Text="Save"
    ImageSource="save_icon.png"
    BackgroundColor="{DynamicResource Color.Primary}"
    TextColor="{DynamicResource Color.OnPrimary}" />
```

### Text Button (No Background)

```xaml
<fs:FsButton
    Text="Learn More"
    BackgroundColor="Transparent"
    TextColor="{DynamicResource Color.Primary}"
    BorderWidth="0" />
```

## Theme Styling

Buttons can be styled globally through themes:

```xaml
<Style TargetType="fs:FsButton">
    <Setter Property="BackgroundColor" Value="{DynamicResource Color.Primary}" />
    <Setter Property="TextColor" Value="{DynamicResource Color.OnPrimary}" />
    <Setter Property="CornerRadius" Value="{DynamicResource Radius.Button.Small}" />
    <Setter Property="Padding" Value="16,12" />
    <Setter Property="FontSize" Value="16" />
    <Setter Property="BorderWidth" Value="0" />
</Style>
```

> **⚠️ Important**: Always use `Radius.Button.*` tokens (Int32 type) for button CornerRadius. Using standard `Radius.*` tokens (Double type) will cause silent binding failure. See [ADR003](../Decisions/adr003-button-corner-radius-type.md) for details.

### Material Design Button Variants

You can create different button styles for various contexts:

```xaml
<!-- Primary Button -->
<Style TargetType="fs:FsButton" x:Key="PrimaryButton">
    <Setter Property="BackgroundColor" Value="{DynamicResource Color.Primary}" />
    <Setter Property="TextColor" Value="{DynamicResource Color.OnPrimary}" />
    <Setter Property="CornerRadius" Value="{DynamicResource Radius.Button.ExtraSmall}" />
    <Setter Property="Padding" Value="24,12" />
</Style>

<!-- Outlined Button -->
<Style TargetType="fs:FsButton" x:Key="OutlinedButton">
    <Setter Property="BackgroundColor" Value="Transparent" />
    <Setter Property="TextColor" Value="{DynamicResource Color.Primary}" />
    <Setter Property="BorderColor" Value="{DynamicResource Color.Outline}" />
    <Setter Property="BorderWidth" Value="1" />
    <Setter Property="CornerRadius" Value="{DynamicResource Radius.Button.ExtraSmall}" />
    <Setter Property="Padding" Value="24,12" />
</Style>

<!-- Text Button -->
<Style TargetType="fs:FsButton" x:Key="TextButton">
    <Setter Property="BackgroundColor" Value="Transparent" />
    <Setter Property="TextColor" Value="{DynamicResource Color.Primary}" />
    <Setter Property="BorderWidth" Value="0" />
    <Setter Property="CornerRadius" Value="{DynamicResource Radius.Button.ExtraSmall}" />
    <Setter Property="Padding" Value="12,8" />
</Style>
```

## Events

`FsButton` supports all standard Button events:

| Event | Description |
|-------|-------------|
| `Clicked` | Raised when the button is clicked |
| `Pressed` | Raised when the button is pressed down |
| `Released` | Raised when the button is released |

### Event Example

```xaml
<fs:FsButton
    Text="Click Me"
    Clicked="OnButtonClicked" />
```

```csharp
private void OnButtonClicked(object sender, EventArgs e)
{
    // Handle click
}
```

## Best Practices

1. **Use Commands Over Events**: Prefer `Command` binding for MVVM patterns
2. **Accessible Text**: Ensure button text clearly indicates the action
3. **Consistent Sizing**: Use theme tokens for padding and font sizes
4. **Visual Hierarchy**: Reserve primary styling for main actions
5. **Touch Targets**: Maintain minimum 44x44 device-independent units for touch
6. **Color Contrast**: Ensure text is readable against background colors
7. **Loading States**: Disable button and show feedback during async operations

## Why FsButton is a Simple Subclass

Unlike `FsEntry`, which requires a wrapper control to remove platform-specific styling (underlines, native borders, focus indicators), the `Button` control in .NET MAUI already provides clean, controllable styling through its properties.

**No platform handlers needed because:**

- Button's native appearance is fully controllable via XAML properties
- BorderColor, BorderWidth, and CornerRadius work consistently across platforms
- No intrusive platform decorations that need to be removed
- BackgroundColor overrides native styling appropriately

This makes `FsButton` lightweight and performant while maintaining the Flagstone UI type identity for theme targeting.

## Example: Sign-In Form

The Button control is used in the SignInForm block:

```xaml
<fs:FsCard Elevation="2" Padding="24">
    <VerticalStackLayout Spacing="16">
        <fs:FsEntry
            Placeholder="Username"
            BackgroundColor="{DynamicResource Color.SurfaceVariant}" />
        <fs:FsEntry
            Placeholder="Password"
            IsPassword="True"
            BackgroundColor="{DynamicResource Color.SurfaceVariant}" />
        <fs:FsButton
            Text="Sign In"
            BackgroundColor="{DynamicResource Color.Primary}"
            TextColor="{DynamicResource Color.OnPrimary}"
            Command="{Binding SignInCommand}" />
        <fs:FsButton
            Text="Forgot Password?"
            BackgroundColor="Transparent"
            TextColor="{DynamicResource Color.Primary}"
            Style="{StaticResource TextButton}" />
    </VerticalStackLayout>
</fs:FsCard>
```

## Platform Support

The Button control is fully supported on:

- Android
- iOS
- Windows
- macOS

All styling properties work consistently across platforms with minor rendering variations inherent to each platform's native controls.

## See Also

- [FsEntry Control](FsEntry.md) - For contrast, see why Entry requires a wrapper approach
- [FsCard Control](FsCard.md) - Container control for grouping UI elements
- [Theme Tokens](../tokens.md) - Available design tokens for styling
- [Control Implementation Guide](../control-implementation-guide.md) - For contributors: how to implement new controls
