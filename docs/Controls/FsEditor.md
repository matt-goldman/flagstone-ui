# FsEditor Control

The `FsEditor` control is an enhanced multi-line text input field that extends the .NET MAUI `Editor` with full visual styling control from shared code. Like `FsEntry`, `FsEditor` uses a wrapper control approach to remove platform-specific native styling that cannot be controlled through standard XAML properties.

## Features

- **Borderless Native Editor**: Platform handlers remove native decorations (underlines, focus indicators)
- **Custom Border Styling**: Full control over border color, width, and corner radius via `Border` wrapper
- **Complete Styling Control**: All visual properties exposed as BindableProperties
- **Standard .NET MAUI Patterns**: Use inline values, StaticResource, DynamicResource, or styles
- **Auto-sizing**: Optional automatic height adjustment based on content
- **Event Support**: Text changed, completion, focus, and unfocus events
- **Keyboard Types**: Support for specialized keyboards (text, chat, etc.)
- **Character Limits**: Optional maximum length enforcement
- **Read-only Mode**: Can be set to read-only for display purposes

## Architecture: Why a Wrapper?

`FsEditor` uses a `ContentView` wrapper containing a `BorderlessEditor` (which extends `Editor`) inside a `Border` control. This architecture is necessary for the same reasons as `FsEntry`:

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

### Solution: BorderlessEditor + Platform Handlers

The `BorderlessEditor` control registers platform-specific handlers that directly manipulate native views to remove all native styling, giving Flagstone UI complete control over the Editor's appearance through the outer `Border` wrapper.

## Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Text` | `string` | `string.Empty` | Current text content |
| `Placeholder` | `string` | `string.Empty` | Placeholder text when empty |
| `PlaceholderColor` | `Color` | `Colors.Gray` | Color of the placeholder text |
| `TextColor` | `Color` | `Colors.Black` | Color of the input text |
| `Background` | `Brush` | `Transparent` | Background brush of the editor |
| `BorderBrush` | `Brush` | `Transparent` | Border brush |
| `BorderWidth` | `double` | `0` | Border width in device-independent units |
| `CornerRadius` | `double` | `0` | Corner radius in device-independent units |
| `Padding` | `Thickness` | `5` | Inner padding |
| `FontSize` | `double` | `14.0` | Font size for text |
| `HorizontalTextAlignment` | `TextAlignment` | `Start` | Horizontal text alignment |
| `VerticalTextAlignment` | `TextAlignment` | `Start` | Vertical text alignment |
| `Keyboard` | `Keyboard` | `Keyboard.Default` | Keyboard type to display |
| `AutoSize` | `EditorAutoSizeOption` | `Disabled` | Whether editor auto-adjusts height |
| `MaxLength` | `int` | `int.MaxValue` | Maximum number of characters allowed |
| `IsReadOnly` | `bool` | `false` | Whether the editor is read-only |

## Usage Examples

### Direct Styling (Simple)

```xaml
<fs:FsEditor Placeholder="Enter your comments here" />

<!-- With custom styling -->
<fs:FsEditor 
    Placeholder="Description"
    BorderColor="#CCCCCC"
    BorderWidth="1"
    CornerRadius="8"
    Padding="12"
    Background="#F5F5F5"
    TextColor="#333333"
    FontSize="16"
    MinimumHeightRequest="120" />
```

### Using App Resources

```xaml
<!-- Define once in App.xaml -->
<Color x:Key="InputBorder">#CCCCCC</Color>
<Color x:Key="InputBackground">#F5F5F5</Color>

<!-- Use with StaticResource -->
<fs:FsEditor 
    Placeholder="Notes"
    BorderColor="{StaticResource InputBorder}"
    BorderWidth="1"
    Background="{StaticResource InputBackground}"
    CornerRadius="8"
    Padding="12"
    MinimumHeightRequest="120" />
```

### Using Theme Styles

```xaml
<!-- Define implicit style in theme -->
<Style TargetType="fs:FsEditor">
    <Setter Property="BorderColor" Value="#CCCCCC" />
    <Setter Property="BorderWidth" Value="1" />
    <Setter Property="CornerRadius" Value="8" />
    <Setter Property="Padding" Value="12" />
    <Setter Property="Background" Value="#F5F5F5" />
    <Setter Property="FontSize" Value="16" />
    <Setter Property="MinimumHeightRequest" Value="88" />
</Style>

<!-- Usage - styles applied automatically -->
<fs:FsEditor Placeholder="Description" />
```

### Optional: Using Design Tokens

If your theme uses design tokens (like Material), you can reference them:

```xaml
<fs:FsEditor 
    Placeholder="Description"
    BorderBrush="{DynamicResource Color.Outline}"
    BorderWidth="1"
    CornerRadius="8"
    Padding="12"
    Background="{DynamicResource Color.SurfaceVariant}"
    TextColor="{DynamicResource Color.OnSurface}"
    FontSize="16"
    MinimumHeightRequest="120" />
```

> **Note**: Design tokens are optional. You can style FsEditor using direct values, app resources, or theme styles without tokens.

### Auto-Sizing Editor

```xaml
<fs:FsEditor 
    Placeholder="Type a message..."
    AutoSize="TextChanges"
    BorderColor="#CCCCCC"
    BorderWidth="1"
    CornerRadius="8"
    Padding="12" />
```

### Editor with Character Limit

```xaml
<fs:FsEditor 
    Placeholder="Enter your bio (max 500 characters)"
    MaxLength="500"
    BorderColor="#6750A4"
    BorderWidth="2"
    CornerRadius="8"
    Padding="12" />
```

### Editor with Data Binding (MVVM)

```xaml
<fs:FsEditor 
    Text="{Binding Notes}"
    Placeholder="Notes"
    BorderColor="#CCCCCC"
    BorderWidth="1"
    CornerRadius="8"
    Padding="12"
    MinimumHeightRequest="100" />
```

### Read-Only Editor

```xaml
<fs:FsEditor 
    Text="{Binding DisplayText}"
    IsReadOnly="True"
    BorderColor="#E0E0E0"
    BorderWidth="1"
    CornerRadius="4"
    Background="#FAFAFA"
    Padding="12" />
```

## Theme Styling

Editors can be styled globally through themes:

```xaml
<Style TargetType="fs:FsEditor">
    <Setter Property="BorderBrush" Value="#CCCCCC" />
    <Setter Property="BorderWidth" Value="1" />
    <Setter Property="CornerRadius" Value="8" />
    <Setter Property="Padding" Value="12" />
    <Setter Property="Background" Value="#F5F5F5" />
    <Setter Property="TextColor" Value="#333333" />
    <Setter Property="PlaceholderColor" Value="#999999" />
    <Setter Property="FontSize" Value="16" />
    <Setter Property="MinimumHeightRequest" Value="88" />
</Style>
```

Themes using design tokens can reference them in styles:

```xaml
<Style TargetType="fs:FsEditor">
    <Setter Property="BorderBrush" Value="{DynamicResource Color.Outline}" />
    <Setter Property="BorderWidth" Value="1" />
    <Setter Property="CornerRadius" Value="8" />
    <Setter Property="Padding" Value="12" />
    <Setter Property="Background" Value="{DynamicResource Color.SurfaceVariant}" />
    <Setter Property="TextColor" Value="{DynamicResource Color.OnSurface}" />
    <Setter Property="PlaceholderColor" Value="{DynamicResource Color.OnSurfaceVariant}" />
    <Setter Property="FontSize" Value="16" />
    <Setter Property="MinimumHeightRequest" Value="88" />
</Style>
```

### Material Design Editor Variants

```xaml
<!-- Outlined Editor -->
<Style TargetType="fs:FsEditor" x:Key="OutlinedEditor">
    <Setter Property="Background" Value="Transparent" />
    <Setter Property="BorderBrush" Value="#CCCCCC" />
    <Setter Property="BorderWidth" Value="1" />
    <Setter Property="CornerRadius" Value="4" />
    <Setter Property="Padding" Value="16,12" />
    <Setter Property="MinimumHeightRequest" Value="88" />
</Style>

<!-- Filled Editor -->
<Style TargetType="fs:FsEditor" x:Key="FilledEditor">
    <Setter Property="Background" Value="#F5F5F5" />
    <Setter Property="BorderBrush" Value="Transparent" />
    <Setter Property="BorderWidth" Value="0" />
    <Setter Property="CornerRadius" Value="4" />
    <Setter Property="Padding" Value="16,12" />
    <Setter Property="MinimumHeightRequest" Value="88" />
</Style>
```

## Events

| Event | Parameters | Description |
|-------|------------|-------------|
| `TextChanged` | `TextChangedEventArgs` | Raised when text content changes |
| `Completed` | `EventArgs` | Raised when user completes input (platform-specific) |
| `Focused` | `EventArgs` | Raised when editor receives focus |
| `Unfocused` | `EventArgs` | Raised when editor loses focus |

### Event Examples

```xaml
<fs:FsEditor 
    Text="{Binding Message}"
    TextChanged="OnMessageTextChanged"
    Focused="OnEditorFocused"
    Unfocused="OnEditorUnfocused" />
```

```csharp
private void OnMessageTextChanged(object sender, TextChangedEventArgs e)
{
    // Handle text changes (e.g., auto-save, character count)
    var newText = e.NewTextValue;
    RemainingChars = MaxLength - newText.Length;
}

private void OnEditorFocused(object sender, EventArgs e)
{
    // Handle focus (e.g., show toolbar, apply focus styling)
}

private void OnEditorUnfocused(object sender, EventArgs e)
{
    // Handle unfocus (e.g., validate, save draft)
}
```

## Integration with MAUI Community Toolkit

`FsEditor` works seamlessly with MCT animations and behaviors through the optional `FlagstoneUI.Integrations.MCT` package. See the [MCT Integrations documentation](../mct-integrations.md) for details on using `FsEditorBorderAnimation` to create animated gradient borders and other integration features.

## Best Practices

1. **Appropriate Height**: Set `MinimumHeightRequest` to provide adequate space for multi-line text
2. **Auto-sizing**: Use `AutoSize="TextChanges"` for chat-style inputs that grow with content
3. **Character Limits**: Set `MaxLength` for inputs with known constraints (e.g., bio fields)
4. **Placeholder Guidance**: Provide clear instructions in placeholder text
5. **Read-Only Display**: Use `IsReadOnly="True"` for displaying formatted text content
6. **Consistent Styling**: Use theme tokens for border and background colors
7. **Touch Targets**: Ensure adequate height for comfortable typing (minimum 88 device-independent units recommended)
8. **Focus Indicators**: Consider using focus events to provide visual feedback
9. **Accessibility**: Ensure sufficient color contrast between text and background
10. **Keyboard Types**: Set appropriate `Keyboard` property for the expected input type

## Example: Comment Form

```xaml
<fs:FsCard Padding="16">
    <VerticalStackLayout Spacing="12">
        <Label Text="Leave a Comment" FontSize="18" FontAttributes="Bold" />
        
        <fs:FsEditor 
            Text="{Binding CommentText}"
            Placeholder="What are your thoughts?"
            BorderBrush="{DynamicResource Color.Outline}"
            BorderWidth="1"
            CornerRadius="8"
            Padding="12"
            MinimumHeightRequest="120"
            MaxLength="1000"
            AutoSize="TextChanges" />
        
        <HorizontalStackLayout HorizontalOptions="End">
            <Label 
                Text="{Binding RemainingCharacters, StringFormat='{0} characters remaining'}"
                FontSize="12"
                TextColor="{DynamicResource Color.OnSurfaceVariant}" />
        </HorizontalStackLayout>
        
        <fs:FsButton 
            Text="Submit Comment"
            Command="{Binding SubmitCommentCommand}"
            HorizontalOptions="End" />
    </VerticalStackLayout>
</fs:FsCard>
```

## Example: Chat-Style Input

The Editor control is well-suited for chat or messaging interfaces:

```xaml
<fs:FsEditor 
    Placeholder="Type a message..."
    Text="{Binding MessageText}"
    BorderBrush="{DynamicResource Color.Outline}"
    BorderWidth="1"
    CornerRadius="20"
    Padding="12,8"
    MinimumHeightRequest="44"
    MaximumHeightRequest="120"
    AutoSize="TextChanges" />
```

For advanced chat UI examples with animated borders, see the [MCT Integrations documentation](../mct-integrations.md).

## Platform Support

The Editor control is fully supported on:

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
        Background="{Binding Background}"
        Stroke="{Binding BorderBrush}"
        StrokeThickness="{Binding BorderWidth}"
        StrokeShape="{Binding BorderShape}">
        <Grid 
            Padding="{Binding Padding}"
            MinimumHeightRequest="88">
            <fs:BorderlessEditor 
                Text="{Binding Text}"
                Placeholder="{Binding Placeholder}"
                PlaceholderColor="{Binding PlaceholderColor}"
                TextColor="{Binding TextColor}"
                FontSize="{Binding FontSize}"
                HorizontalTextAlignment="{Binding HorizontalTextAlignment}"
                VerticalTextAlignment="{Binding VerticalTextAlignment}"
                Keyboard="{Binding Keyboard}"
                AutoSize="{Binding AutoSize}"
                MaxLength="{Binding MaxLength}"
                IsReadOnly="{Binding IsReadOnly}"
                VerticalOptions="Fill"
                Focused="BorderlessEditor_Focused"
                Unfocused="BorderlessEditor_Unfocused" />
        </Grid>
    </Border>
</ContentView>
```

### Why This Architecture?

1. **Separation of Concerns**: Native editor handles input, Border handles visual styling
2. **Platform Independence**: Handlers strip native styling once, XAML applies theme consistently
3. **Full Control**: All styling properties work predictably across platforms
4. **Visual State Management**: Focus and unfocus events trigger visual state changes
5. **Maintainability**: Clear separation between platform-specific code and cross-platform UI

## Comparison with FsEntry

| Aspect | FsEditor | FsEntry |
|--------|----------|---------|
| **Base Control** | Editor (multi-line) | Entry (single-line) |
| **Architecture** | Wrapper (ContentView + BorderlessEditor + Border) | Wrapper (ContentView + BorderlessEntry + Border) |
| **Default Height** | 88 device-independent units | Single line height |
| **Auto-sizing** | Optional (TextChanges) | N/A (always single line) |
| **Use Case** | Comments, notes, long text | Usernames, emails, short inputs |
| **Character Limit** | Optional with MaxLength | Optional with MaxLength |
| **Completion Event** | Platform-specific | Return key press |

## See Also

- [FsEntry Control](FsEntry.md) - Single-line text input control
- [FsButton Control](FsButton.md) - Button control
- [FsCard Control](FsCard.md) - Container control for grouping UI elements
- [MCT Integrations](../mct-integrations.md) - MAUI Community Toolkit integration features
- [Theme Tokens](../tokens.md) - Available design tokens for styling
- [Control Implementation Guide](../control-implementation-guide.md) - For contributors: how to implement new controls
