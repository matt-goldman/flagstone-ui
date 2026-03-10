**note: this is currently an addendum to ADR005, however it's listed in a separate file as this will form the bases of the published user documentation later.**

## Border Rendering Model in FlagstoneUI

FlagstoneUI supports two distinct border rendering models: uniform borders and per-edge borders. These models are mutually exclusive and are selected implicitly based on which properties are set.

### Uniform Border Model

The uniform border model represents a single, closed outline rendered around a control.

This model is active when:

* No per-edge border properties are set, and
* The `Border` shorthand is not used

In this mode:

* `BorderBrush` defines the border’s brush
* A single border thickness is applied uniformly
* `CornerRadius` applies and is respected
* Borders are rendered as a closed shape

This corresponds to the traditional .NET MAUI Border abstraction and is suitable for the majority of simple styling scenarios.

### Per-Edge Border Model

The per-edge border model represents borders as independent edge primitives, rendered as lines on each side of a control.

This model is activated when any of the following are set:

* Any per-edge border property (e.g. `BorderTopThickness`, `BorderLeftBrush`)
* The `Border` shorthand property

In this mode:

* Borders are rendered as independent edges (lines)
* `BorderBrush` is ignored
* Uniform border thickness is ignored
* `CornerRadius` does not apply
* No attempt is made to form or approximate corner joins

Per-edge borders are intentionally modeled as lines. As such, they do not form corners and do not support rounded geometry.

## Border Shorthand

The `Border` property is a shorthand syntax for defining per-edge borders concisely.

Key characteristics:

* The shorthand always expands into per-edge border properties
* It never activates the uniform border model
* It always activates the per-edge border model

For example:

```xml
Border="1 Black"
```

is equivalent to setting all four edge borders explicitly, and therefore disables `BorderBrush` and `CornerRadius`.

The shorthand exists purely as a developer convenience; the explicit per-edge properties remain the authoritative API.

## Precedence Rules

The following precedence rules apply:

1. If any per-edge border property is set, per-edge border mode is active.
2. If the `Border` shorthand is set, per-edge border mode is active.
3. When per-edge border mode is active:
 - `BorderBrush` is ignored
 - `CornerRadius`does not apply
4. If neither per-edge borders nor the shorthand are used, the uniform border model applies.

These rules are intentional and designed to avoid ambiguous or blended rendering behaviour.

## Design Notes

* Per-edge borders and rounded corners are deliberately not combined.
* Advanced corner join geometry (e.g. bevels or angled joins) is out of scope.
* Complex brush scenarios (gradients, image brushes, arbitrary geomteries) are supported via explicit properties, not shorthand.
* Any semantic meaning (e.g. raised, sunken, outline) is the responsibility of themes and styles, not the border primitives themselves.