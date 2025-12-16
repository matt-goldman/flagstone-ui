# FsBorder Control

## Overview

FsBorder is a control that provides per-edge border primitives, allowing developers to specify different thickness and brush values for each border edge (top, right, bottom, left). This enables styling scenarios such as asymmetric borders, inset/outset effects, dividers, and accessibility-focused themes.

## Key Features

- **Per-edge thickness control**: Set individual thickness for top, right, bottom, and left borders
- **Per-edge brush control**: Use different brushes (solid colors, gradients) for each border edge
- **Shorthand syntax**: Convenient string-based border definition following .NET MAUI conventions
- **Line-based rendering**: Borders are rendered using Line elements for deterministic cross-platform behavior
- **Lazy materialization**: Border lines are only created when their thickness > 0
- **Stroke cap control**: Optional BorderStrokeCap property for line ending customization

## Properties

### Border Shorthand Property

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Border` | `string` | null | Shorthand syntax for defining borders. Accepts 1, 2, or 4 comma-separated "thickness color" values. |

The `Border` property provides a convenient shorthand syntax for defining per-edge borders, following .NET MAUI conventions (similar to `Thickness`, `Padding`, `ColumnDefinitions`).

**Syntax Rules:**
- **1 value**: Applies to all edges (e.g., `"2 Blue"`)
- **2 values**: First for vertical (top/bottom), second for horizontal (left/right) (e.g., `"1 Black, 2 Grey"`)
- **4 values**: Top, Right, Bottom, Left in TRBL order (e.g., `"1 White, 3 Black, 3 Black, 1 White"`)

Each value consists of:
- A thickness (double)
- A color (parsed as solid color brush)

**Constraints:**
- Only solid colors are supported in shorthand (for gradients, use explicit per-edge properties)
- Three-value syntax is not supported
- Color must be a valid .NET MAUI color name or hex value

### Border Thickness Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `BorderTopThickness` | `double` | 0.0 | Thickness of the top border |
| `BorderRightThickness` | `double` | 0.0 | Thickness of the right border |
| `BorderBottomThickness` | `double` | 0.0 | Thickness of the bottom border |
| `BorderLeftThickness` | `double` | 0.0 | Thickness of the left border |

### Border Brush Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `BorderTopBrush` | `Brush` | Transparent | Brush for the top border |
| `BorderRightBrush` | `Brush` | Transparent | Brush for the right border |
| `BorderBottomBrush` | `Brush` | Transparent | Brush for the bottom border |
| `BorderLeftBrush` | `Brush` | Transparent | Brush for the left border |

### Other Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `BorderStrokeCap` | `PenLineCap` | Flat | Line cap style for border edges |
| `Background` | `Brush` | Transparent | Background brush for the border area |
| `Padding` | `Thickness` | 0 | Padding inside the border |
| `StrokeShape` | `IShape` | null | Shape for clipping (e.g., RoundRectangle for rounded corners) |
| `BorderContent` | `View` | null | Content displayed within the border |

## Usage Examples

### Shorthand Syntax Examples

#### Uniform Border (Single Value)

```xaml
<fs:FsBorder Border="2 Blue" Padding="10">
    <fs:FsBorder.BorderContent>
        <Label Text="Uniform border using shorthand" />
    </fs:FsBorder.BorderContent>
</fs:FsBorder>
```

#### Vertical/Horizontal Borders (Two Values)

```xaml
<fs:FsBorder Border="1 Black, 2 Grey" Padding="10">
    <fs:FsBorder.BorderContent>
        <Label Text="1px black top/bottom, 2px grey left/right" />
    </fs:FsBorder.BorderContent>
</fs:FsBorder>
```

#### Full TRBL Control (Four Values)

```xaml
<fs:FsBorder Border="1 White, 3 Black, 3 Black, 1 White" Padding="10">
    <fs:FsBorder.BorderContent>
        <Label Text="Inset effect with shorthand" />
    </fs:FsBorder.BorderContent>
</fs:FsBorder>
```

#### With Hex Colors

```xaml
<fs:FsBorder Border="2 #FF0000, 2 #00FF00, 2 #0000FF, 2 #FFFF00" Padding="10">
    <fs:FsBorder.BorderContent>
        <Label Text="Rainbow borders!" />
    </fs:FsBorder.BorderContent>
</fs:FsBorder>
```

### Explicit Property Examples

#### Basic Uniform Border

```xaml
<fs:FsBorder BorderTopThickness="2"
             BorderRightThickness="2"
             BorderBottomThickness="2"
             BorderLeftThickness="2"
             BorderTopBrush="Blue"
             BorderRightBrush="Blue"
             BorderBottomBrush="Blue"
             BorderLeftBrush="Blue"
             Padding="10">
    <fs:FsBorder.BorderContent>
        <Label Text="Hello World" />
    </fs:FsBorder.BorderContent>
</fs:FsBorder>
```

### Asymmetric Border (Border-Bottom Only)

```xaml
<fs:FsBorder BorderBottomThickness="2"
             BorderBottomBrush="Gray"
             Padding="10">
    <fs:FsBorder.BorderContent>
        <Label Text="Divider Example" />
    </fs:FsBorder.BorderContent>
</fs:FsBorder>
```

### Inset/Outset Effect (3D Border)

```xaml
<fs:FsBorder BorderTopThickness="2"
             BorderLeftThickness="2"
             BorderRightThickness="2"
             BorderBottomThickness="2"
             BorderTopBrush="White"
             BorderLeftBrush="White"
             BorderRightBrush="DarkGray"
             BorderBottomBrush="DarkGray"
             Padding="10">
    <fs:FsBorder.BorderContent>
        <Label Text="Raised Button" />
    </fs:FsBorder.BorderContent>
</fs:FsBorder>
```

### With Rounded Corners

```xaml
<fs:FsBorder BorderTopThickness="2"
             BorderRightThickness="2"
             BorderBottomThickness="2"
             BorderLeftThickness="2"
             BorderTopBrush="Red"
             BorderRightBrush="Red"
             BorderBottomBrush="Red"
             BorderLeftBrush="Red"
             Padding="10">
    <fs:FsBorder.StrokeShape>
        <RoundRectangle CornerRadius="8" />
    </fs:FsBorder.StrokeShape>
    <fs:FsBorder.BorderContent>
        <Label Text="Rounded Border" />
    </fs:FsBorder.BorderContent>
</fs:FsBorder>
```

## Implementation Details

### Line-Based Rendering

FsBorder uses up to four Line elements to render borders:

- **Top line**: From (0, 0) to (Width, 0)
- **Right line**: From (Width, 0) to (Width, Height)
- **Bottom line**: From (0, Height) to (Width, Height)
- **Left line**: From (0, 0) to (0, Height)

Lines are positioned and sized in `OnSizeAllocated` and are only materialized when the corresponding edge thickness > 0.

**Layering**: Border Line elements have `ZIndex = -1` to ensure they render behind all other content in the Grid (which defaults to `ZIndex = 0`).

**Input Transparency**: All border Line elements have `InputTransparent = true` to ensure they do not intercept hit testing or block user interaction with the underlying content.

### Corner Radius Behavior

When using rounded corners (via `StrokeShape` with `RoundRectangle`), the border lines may clip or approximate corners. This is an acceptable limitation documented in the design.

**Note**: Advanced corner joins (angled bevels, custom joins) are explicitly out of scope.

## Integration with FsCard and FsEntry

Both FsCard and FsEntry use FsBorder internally and expose per-edge border properties. For backward compatibility, they also maintain the original uniform border properties:

- `BorderColor` / `BorderBrush` - Sets all edge brushes
- `BorderWidth` - Sets all edge thicknesses

When these properties are set, they automatically update all four edge-specific properties.

## Design Decisions

1. **Line elements over platform handlers**: Using Line elements provides deterministic behavior across all platforms without relying on platform-specific rendering.

2. **Lazy materialization**: Border lines are only created when needed (thickness > 0), reducing memory overhead.

3. **Brush over Color**: Using Brush type allows for gradients and other advanced brush types, not just solid colors.

4. **Separate thickness and brush properties**: Keeping these separate provides maximum flexibility, though this could be revisited based on usage patterns.

5. **ControlTemplate architecture**: FsBorder uses a ControlTemplate (like FsCard) to properly handle content through ContentPresenter. The `BorderContent` property syncs to the `Content` property for compatibility with existing usage.

6. **Z-index layering**: Border lines use `ZIndex = -1` to render behind all Grid content (which defaults to `ZIndex = 0`). This ensures borders never cover or obscure content.

7. **Input transparency**: Border lines are set to `InputTransparent = true` to ensure they render visually without blocking user interaction with the content inside the border.

## Accessibility Considerations

Per-edge borders enable high-contrast and accessibility-focused themes:

- Use high-contrast colors for borders in accessibility themes
- Support system high-contrast mode by adjusting border brushes
- Ensure sufficient contrast ratios between border and background

## Performance Considerations

- Lines are only created when thickness > 0
- Lines are reused when size changes (not recreated)
- Visibility is toggled rather than adding/removing from visual tree
- No platform-specific handlers required

## Future Enhancements

Potential future additions (currently out of scope):

- Shorthand properties: `BorderXThickness`, `BorderYThickness`
- Corner join styles: angled bevels, custom joins
- Border dash patterns
- Gradient support for individual edges (already supported via Brush type)

## See Also

- [FsCard Control](FsCard.md)
- [FsEntry Control](FsEntry.md)
- [Control Implementation Guide](../control-implementation-guide.md)
