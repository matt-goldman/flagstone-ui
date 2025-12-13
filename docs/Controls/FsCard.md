# FsCard Control

The `FsCard` control is a customizable card view that provides a surface for displaying content with visual styling options including elevation, rounded corners, and per-edge borders.

## Features

- **Elevation**: Automatic shadow effects based on Material Design 3 specifications
- **Corner Radius**: Customizable rounded corners
- **Per-Edge Borders**: Individual control over each border edge (top, right, bottom, left)
- **Border Styling**: Configurable border color and width (uniform or per-edge)
- **Background Color**: Themeable background color
- **Content**: Can contain any .NET MAUI content

## Properties

### Core Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Elevation` | `int` | `0` | Controls shadow depth. Values 0-5 recommended. 0 = no shadow. |
| `CornerRadius` | `double` | `0.0` | Corner radius in device-independent units |
| `BackgroundColor` | `Color` | `Transparent` | Background color of the card (backward compatibility) |
| `BackgroundBrush` | `Brush` | `Transparent` | Background brush for advanced styling |
| `Padding` | `Thickness` | `0` | Inner padding |

### Uniform Border Properties (Backward Compatibility)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `BorderColor` | `Color` | `null` | Border color (sets all edges) |
| `BorderWidth` | `double` | `1.0` | Border width in device-independent units (sets all edges) |

### Per-Edge Border Thickness Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `BorderTopThickness` | `double` | `0.0` | Thickness of the top border |
| `BorderRightThickness` | `double` | `0.0` | Thickness of the right border |
| `BorderBottomThickness` | `double` | `0.0` | Thickness of the bottom border |
| `BorderLeftThickness` | `double` | `0.0` | Thickness of the left border |

### Per-Edge Border Brush Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `BorderTopBrush` | `Brush` | `Transparent` | Brush for the top border |
| `BorderRightBrush` | `Brush` | `Transparent` | Brush for the right border |
| `BorderBottomBrush` | `Brush` | `Transparent` | Brush for the bottom border |
| `BorderLeftBrush` | `Brush` | `Transparent` | Brush for the left border |

### Other Border Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `BorderStrokeCap` | `PenLineCap` | `Flat` | Line cap style for border edges |

## Usage Examples

### Basic Card

```xaml
<fs:FsCard>
    <Label Text="Hello, Card!" />
</fs:FsCard>
```

### Card with Elevation

```xaml
<fs:FsCard Elevation="2">
    <VerticalStackLayout Spacing="8">
        <Label Text="Elevated Card" FontSize="18" FontAttributes="Bold" />
        <Label Text="This card has a shadow for depth." />
    </VerticalStackLayout>
</fs:FsCard>
```

### Styled Card

```xaml
<fs:FsCard 
    Elevation="3"
    CornerRadius="12"
    BorderColor="Gray"
    BorderWidth="1"
    BackgroundColor="White"
    Padding="16">
    <Label Text="Fully styled card" />
</fs:FsCard>
```

### Card with Per-Edge Border (Divider Style)

```xaml
<fs:FsCard 
    BorderBottomThickness="2"
    BorderBottomBrush="Gray"
    Padding="16">
    <Label Text="Card with bottom border only" />
</fs:FsCard>
```

### Card with Inset/Outset Effect

```xaml
<fs:FsCard 
    Elevation="0"
    CornerRadius="8"
    BorderTopThickness="2"
    BorderLeftThickness="2"
    BorderRightThickness="2"
    BorderBottomThickness="2"
    BorderTopBrush="White"
    BorderLeftBrush="White"
    BorderRightBrush="DarkGray"
    BorderBottomBrush="DarkGray"
    Padding="16">
    <Label Text="3D raised effect card" />
</fs:FsCard>
```

## Backward Compatibility

The `BorderColor` and `BorderWidth` properties are maintained for backward compatibility. Setting these properties will automatically update all four edge-specific properties:

```xaml
<!-- This approach still works -->
<fs:FsCard BorderColor="Blue" BorderWidth="2">
    <Label Text="Uniform border" />
</fs:FsCard>

<!-- Equivalent to -->
<fs:FsCard 
    BorderTopBrush="Blue"
    BorderRightBrush="Blue"
    BorderBottomBrush="Blue"
    BorderLeftBrush="Blue"
    BorderTopThickness="2"
    BorderRightThickness="2"
    BorderBottomThickness="2"
    BorderLeftThickness="2">
    <Label Text="Uniform border" />
</fs:FsCard>
```

## Border Shorthand Syntax

FsCard supports the `Border` property for convenient border definition using string shorthand:

```xaml
<!-- Uniform border -->
<fs:FsCard Border="2 Blue" Padding="16">
    <Label Text="Simple border" />
</fs:FsCard>

<!-- Vertical/horizontal -->
<fs:FsCard Border="1 Black, 2 Grey" Padding="16">
    <Label Text="Thin top/bottom, thick left/right" />
</fs:FsCard>

<!-- Full TRBL control for 3D effects -->
<fs:FsCard Border="2 White, 2 Gray, 2 Gray, 2 White" Padding="16">
    <Label Text="Inset effect" />
</fs:FsCard>
```

See [FsBorder documentation](FsBorder.md) for complete shorthand syntax details.

### Card with Custom Content

```xaml
<fs:FsCard Elevation="2" Padding="16">
    <Grid RowDefinitions="Auto,*,Auto" RowSpacing="12">
        <Label Text="Card Title" FontSize="20" FontAttributes="Bold" />
        <Image Source="image.png" Grid.Row="1" Aspect="AspectFill" HeightRequest="200" />
        <Button Text="Action" Grid.Row="2" />
    </Grid>
</fs:FsCard>
```

## Elevation Behavior

The `Elevation` property automatically creates shadows following Material Design 3 specifications:

- **Elevation 0**: No shadow
- **Elevation 1**: Subtle shadow (2px radius, 1px offset)
- **Elevation 2**: Small shadow (4px radius, 2px offset)
- **Elevation 3**: Medium shadow (6px radius, 3px offset)
- **Elevation 4**: Large shadow (8px radius, 4px offset)
- **Elevation 5+**: Extra large shadow (10px+ radius, 5px+ offset)

Shadow opacity increases with elevation (0.2 to 0.4) to maintain visual hierarchy.

## Theme Styling

Cards can be styled globally through themes:

```xaml
<Style TargetType="fs:FsCard">
    <Setter Property="Padding" Value="16" />
    <Setter Property="BackgroundColor" Value="{DynamicResource Color.Surface}" />
    <Setter Property="BorderColor" Value="{DynamicResource Color.Outline}" />
    <Setter Property="CornerRadius" Value="8" />
    <Setter Property="Elevation" Value="1" />
</Style>
```

## Best Practices

1. **Use Elevation Sparingly**: Reserve higher elevations (3+) for important UI elements
2. **Maintain Hierarchy**: Use different elevation levels to show content relationships
3. **Consider Performance**: Shadows have rendering costs; avoid excessive elevation on lists
4. **Theme Integration**: Leverage theme colors for borders and backgrounds
5. **Accessibility**: Ensure sufficient contrast between card background and text

## Example: Sign-In Form

The Card control is used in the SignInForm block:

```xaml
<fs:FsCard>
    <FlexLayout Direction="Column" AlignItems="Center" JustifyContent="SpaceAround">
        <fs:FsEntry Placeholder="Username" WidthRequest="250" />
        <fs:FsEntry Placeholder="Password" IsPassword="True" />
        <fs:FsButton Text="Sign In" WidthRequest="250" />
    </FlexLayout>
</fs:FsCard>
```

## Platform Support

The Card control is fully supported on:
- Android
- iOS
- Windows
- macOS

Shadow rendering may vary slightly by platform but maintains consistent visual hierarchy.

## See Also

- [FsBorder Control](FsBorder.md) - Per-edge border control documentation
- [FsButton Control](FsButton.md) - Button control
- [FsEntry Control](FsEntry.md) - Text input control
- [Theme Tokens](../tokens.md) - Available design tokens for styling
- [Control Implementation Guide](../control-implementation-guide.md) - For contributors: how to implement new controls
