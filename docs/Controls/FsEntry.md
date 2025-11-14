# FsEntry Control

The `FsEntry` control is a customizable text input field that wraps the .NET MAUI `Entry` control to provide consistent, theme-driven styling across all platforms. Unlike `FsButton`, `FsEntry` uses a wrapper control approach to remove platform-specific native styling that cannot be controlled through standard XAML properties.

## Features

- **Borderless Native Entry**: Platform handlers remove native decorations (underlines, focus indicators)
- **Custom Border Styling**: Full control over border color, width, and corner radius via `Border` wrapper
- **Token Integration**: Seamless theming through DynamicResource bindings
- **Flexible Layout**: Configurable padding, alignment, and sizing
- **Password Support**: Built-in password masking
- **Keyboard Types**: Support for specialized keyboards (numeric, email, etc.)
- **Event Support**: Text changed and completion events

## Architecture: Why a Wrapper?

`FsEntry` uses a `ContentView` wrapper containing a `BorderlessEntry` (which extends `Entry`) inside a `Border` control. This architecture is necessary because:

### Platform-Specific Styling Issues

**Android:**

- Native underline that can't be removed via XAML
- Background tint that persists despite BackgroundColor settings
- Native ripple effects on focus

**iOS:**

- Default border that can't be disabled via properties alone
- Native background styling
- Platform-specific focus indicators

**Windows:**

- Native border thickness that requires handler modification
- Focus visual margin that adds unwanted spacing
- Background styling that overrides XAML settings

### Solution: BorderlessEntry + Platform Handlers

The `BorderlessEntry` control registers platform-specific handlers that directly manipulate native views:

```csharp
// Android
handler.PlatformView.Background = null;
handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent);
handler.PlatformView.BackgroundTintList = ColorStateList.ValueOf(Android.Graphics.Color.Transparent);

// iOS
handler.PlatformView.BackgroundColor = UIKit.UIColor.Clear;
handler.PlatformView.Layer.BorderWidth = 0;
handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;

// Windows
handler.PlatformView.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
handler.PlatformView.Background = null;
handler.PlatformView.FocusVisualMargin = new Microsoft.UI.Xaml.Thickness(0);
```

This gives Flagstone UI complete control over the Entry's appearance, allowing the outer `Border` to handle all visual styling consistently.

## Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Text` | `string` | `string.Empty` | Current text content |
| `Placeholder` | `string` | `string.Empty` | Placeholder text when empty |
| `TextColor` | `Color` | `Colors.Black` | Color of the input text |
| `BackgroundColor` | `Color` | `Colors.Transparent` | Background color of the entry |
| `BorderColor` | `Color` | `Colors.Transparent` | Border color |
| `BorderWidth` | `double` | `0` | Border width in device-independent units |
| `CornerRadius` | `double` | `0` | Corner radius in device-independent units |
| `Padding` | `Thickness` | `5` | Inner padding |
| `FontSize` | `double` | `14.0` | Font size for text |
| `HorizontalTextAlignment` | `TextAlignment` | `Start` | Horizontal text alignment |
| `VerticalTextAlignment` | `TextAlignment` | `Center` | Vertical text alignment |
| `IsPassword` | `bool` | `false` | Whether to mask text as password |
| `Keyboard` | `Keyboard` | `Keyboard.Default` | Keyboard type to display |

## Usage Examples

### Basic Entry

```xaml
<fs:FsEntry Placeholder="Enter your name" />
```

### Styled Entry

```xaml
<fs:FsEntry 
    Placeholder="Email Address"
    BorderColor="{DynamicResource Color.Outline}"
    BorderWidth="1"
    CornerRadius="8"
    Padding="12"
    BackgroundColor="{DynamicResource Color.SurfaceVariant}"
    TextColor="{DynamicResource Color.OnSurface}"
    FontSize="16" />
```

### Password Entry

```xaml
<fs:FsEntry 
    Placeholder="Password"
    IsPassword="True"
    BorderColor="{DynamicResource Color.Outline}"
    BorderWidth="1"
    CornerRadius="4"
    Padding="12" />
```

### Entry with Data Binding (MVVM)

```xaml
<fs:FsEntry 
    Text="{Binding Username}"
    Placeholder="Username"
    BorderColor="{DynamicResource Color.Primary}"
    BorderWidth="2"
    CornerRadius="8" />
```

### Numeric Entry

```xaml
<fs:FsEntry 
    Placeholder="Enter amount"
    Keyboard="Numeric"
    HorizontalTextAlignment="End"
    BorderColor="{DynamicResource Color.Outline}"
    BorderWidth="1" />
```

### Email Entry

```xaml
<fs:FsEntry 
    Placeholder="email@example.com"
    Keyboard="Email"
    BorderColor="{DynamicResource Color.Outline}"
    BorderWidth="1"
    CornerRadius="4" />
```

## Theme Styling

Entries can be styled globally through themes:

```xaml
<Style TargetType="fs:FsEntry">
    <Setter Property="BorderColor" Value="{DynamicResource Color.Outline}" />
    <Setter Property="BorderWidth" Value="1" />
    <Setter Property="CornerRadius" Value="8" />
    <Setter Property="Padding" Value="12" />
    <Setter Property="BackgroundColor" Value="{DynamicResource Color.SurfaceVariant}" />
    <Setter Property="TextColor" Value="{DynamicResource Color.OnSurface}" />
    <Setter Property="FontSize" Value="16" />
</Style>
```

### Material Design Entry Variants

```xaml
<!-- Filled Entry -->
<Style TargetType="fs:FsEntry" x:Key="FilledEntry">
    <Setter Property="BackgroundColor" Value="{DynamicResource Color.SurfaceVariant}" />
    <Setter Property="BorderColor" Value="Transparent" />
    <Setter Property="BorderWidth" Value="0" />
    <Setter Property="CornerRadius" Value="4" />
    <Setter Property="Padding" Value="16,12" />
</Style>

<!-- Outlined Entry -->
<Style TargetType="fs:FsEntry" x:Key="OutlinedEntry">
    <Setter Property="BackgroundColor" Value="Transparent" />
    <Setter Property="BorderColor" Value="{DynamicResource Color.Outline}" />
    <Setter Property="BorderWidth" Value="1" />
    <Setter Property="CornerRadius" Value="4" />
    <Setter Property="Padding" Value="16,12" />
</Style>
```

## Events

| Event | Parameters | Description |
|-------|------------|-------------|
| `TextChanged` | `TextChangedEventArgs` | Raised when text content changes |
| `Completed` | `EventArgs` | Raised when user presses return/enter key |

### Event Examples

```xaml
<fs:FsEntry 
    Text="{Binding SearchText}"
    TextChanged="OnSearchTextChanged"
    Completed="OnSearchCompleted" />
```

```csharp
private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
{
    // Handle text changes (e.g., live search)
    var newText = e.NewTextValue;
}

private void OnSearchCompleted(object sender, EventArgs e)
{
    // Handle completion (e.g., submit search)
}
```

## Validation and Behaviors

As documented in [ADR001](../Decisions/adr001-fsentry-behavior.md), `FsEntry` does not include built-in validation logic. Instead, it supports attaching behaviors directly:

### Using CommunityToolkit.Maui Validation (Optional)

```xaml
<fs:FsEntry Placeholder="Email">
    <fs:FsEntry.Behaviors>
        <toolkit:EmailValidationBehavior 
            InvalidStyle="{StaticResource InvalidEntryStyle}"
            ValidStyle="{StaticResource ValidEntryStyle}"
            Flags="ValidateOnValueChanged" />
    </fs:FsEntry.Behaviors>
</fs:FsEntry>
```

**Note:** The MAUI Community Toolkit is not a dependency of FlagstoneUI.Core. It's an optional integration that consumers can add to their own projects.

## Best Practices

1. **Use Placeholders Effectively**: Provide clear guidance on expected input format
2. **Appropriate Keyboard Types**: Set `Keyboard` property to match input type (Email, Numeric, etc.)
3. **Password Security**: Always use `IsPassword="True"` for sensitive data
4. **Text Alignment**: Consider `HorizontalTextAlignment="End"` for numeric inputs
5. **Consistent Border Styling**: Use theme tokens for border colors
6. **Sufficient Padding**: Ensure adequate touch targets (minimum 44x44 device-independent units)
7. **Error States**: Use border color changes or behaviors to indicate validation errors
8. **Accessibility**: Ensure sufficient color contrast between text and background

## Example: Sign-In Form

The Entry control is used in the SignInForm block:

```xaml
<fs:FsCard Elevation="2" Padding="24">
    <VerticalStackLayout Spacing="16">
        <Label Text="Sign In" FontSize="24" FontAttributes="Bold" />
        
        <fs:FsEntry 
            Placeholder="Username"
            BorderColor="{DynamicResource Color.Outline}"
            BorderWidth="1"
            CornerRadius="8"
            Padding="12"
            Text="{Binding Username}" />
        
        <fs:FsEntry 
            Placeholder="Password"
            IsPassword="True"
            BorderColor="{DynamicResource Color.Outline}"
            BorderWidth="1"
            CornerRadius="8"
            Padding="12"
            Text="{Binding Password}" />
        
        <fs:FsButton 
            Text="Sign In"
            Command="{Binding SignInCommand}" />
    </VerticalStackLayout>
</fs:FsCard>
```

## Platform Support

The Entry control is fully supported on:

- Android
- iOS
- Windows
- macOS

Platform handlers ensure consistent borderless rendering across all platforms, with the outer `Border` providing uniform styling.

## Technical Implementation

### XAML Structure

```xaml
<ContentView ...>
    <Border 
        BackgroundColor="{Binding BackgroundColor}"
        Stroke="{Binding BorderColor}"
        StrokeThickness="{Binding BorderWidth}"
        StrokeShape="{Binding BorderShape}">
        <Grid Padding="{Binding Padding}">
            <fs:BorderlessEntry 
                Text="{Binding Text}"
                Placeholder="{Binding Placeholder}"
                ... />
        </Grid>
    </Border>
</ContentView>
```

### Why This Architecture?

1. **Separation of Concerns**: Native entry handles input, Border handles visual styling
2. **Platform Independence**: Handlers strip native styling once, XAML applies theme consistently
3. **Full Control**: All styling properties work predictably across platforms
4. **Performance**: Minimal overhead compared to purely XAML-based solutions
5. **Maintainability**: Clear separation between platform-specific code and cross-platform UI

## Comparison with FsButton

| Aspect | FsEntry | FsButton |
|--------|---------|----------|
| **Architecture** | Wrapper (ContentView + BorderlessEntry + Border) | Simple subclass |
| **Reason** | Remove intrusive native styling | Native styling already controllable |
| **Platform Handlers** | Required (removes underlines, borders, focus) | Not needed |
| **Performance** | Slight overhead from wrapper | No overhead |
| **Complexity** | Higher (3-layer structure) | Lower (direct inheritance) |
| **Use Case** | When native control has uncontrollable styling | When native control is already clean |

## See Also

- [FsButton Control](FsButton.md) - Simpler subclass approach for comparison
- [FsCard Control](FsCard.md) - Container control for grouping UI elements
- [ADR001: FsEntry Behavior](../Decisions/adr001-fsentry-behavior.md) - Design decisions and validation strategy
- [Theme Tokens](../tokens.md) - Available design tokens for styling
- [Control Implementation Guide](../control-implementation-guide.md) - For contributors: how to implement new controls
