# FsCard Control

The `FsCard` control is a customizable card view that provides a surface for displaying content with visual styling options including elevation, rounded corners, and borders.

## Features

- **Elevation**: Automatic shadow effects based on Material Design 3 specifications
- **Corner Radius**: Customizable rounded corners
- **Border Styling**: Configurable border color and width
- **Background Color**: Themeable background color
- **Content**: Can contain any .NET MAUI content

## Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Elevation` | `int` | `0` | Controls shadow depth. Values 0-5 recommended. 0 = no shadow. |
| `CornerRadius` | `double` | `0.0` | Corner radius in device-independent units |
| `BorderColor` | `Color` | `null` | Border color |
| `BorderWidth` | `double` | `1.0` | Border width in device-independent units |
| `BackgroundColor` | `Color` | `Transparent` | Background color of the card |
| `Padding` | `Thickness` | `0` | Inner padding |

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
