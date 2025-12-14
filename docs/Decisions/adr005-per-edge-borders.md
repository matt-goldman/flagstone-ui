# ADR005: Per-Edge Border Primitives in FlagstoneUI

## Status
Accepted

## Context

FlagstoneUI aims to expose **low-level, token-friendly primitives** for styling .NET MAUI applications, without encoding semantic styles or visual meaning in the core framework.

.NET MAUI's built-in `Border` and `Shape` APIs apply stroke uniformly to a closed geometry and do not support per-edge borders (e.g. top-only, horizontal-only, or asymmetric borders). This limits a range of legitimate styling scenarios, including:

- Asymmetric borders (`border-top`, `border-x`, etc.)
- Inset / outset borders (light top/left, dark bottom/right)
- Dividers implemented as borders
- Retro, high-contrast, or accessibility-focused themes

These scenarios are commonplace on the web and in historical native UI systems. While they can be messy (particularly when combined with rounded corners), the goal is not to make them “perfect”, but to ensure FlagstoneUI does not block them and that any unavoidable complexity is encapsulated within the framework rather than repeated in every consuming application.

## Decision

FlagstoneUI will model borders as **per-edge primitives**, rather than as a single stroke applied to a closed shape.

Specifically:

- Controls may expose independent border properties for each edge (top, right, bottom, left), expressed in terms of thickness and colour/brush.
- The framework will not introduce semantic border styles (e.g. “Raised”, “Sunken”, “Outline”) at the primitive layer.
  - Any such meaning is the responsibility of themes and styles.
- Internally, borders are rendered using **edge primitives** (e.g. one line per edge), sized and positioned based on the control's allocated layout.
- Controls are wrapped where necessary (e.g. via `ContentView`) to fully encapsulate layout and rendering logic.

This decision intentionally avoids reliance on .NET MAUI's `Border` or `Shape` abstractions, which do not align with the required level of control.

## Shorthand Border Syntax (Developer Experience)

To improve developer experience, FlagstoneUI may provide a **string shorthand syntax** for defining per-edge borders, following existing .NET MAUI conventions (e.g. `Thickness`, `Padding`, `ColumnDefinitions`).

This shorthand is a convenience only; the explicit per-edge properties remain the primary and authoritative API.

### Syntax Rules

The shorthand accepts **1, 2, or 4 values**, expanded as follows:

- **1 value**  
  Applies to all edges.

```xml
Border="1 Black"
```

- **2 values (Vertical, Horizontal)**  
First value applies to Top and Bottom, second to Left and Right.

```xml
Border="1 Black, 2 Grey"
```

- **4 values (Top, Right, Bottom, Left)**  
Values are applied in TRBL order, matching .NET MAUI conventions.

```xml
Border="1 White, 3 Black, 3 Black, 1 White"
```

Each value represents:
- A thickness
- A colour (parsed into a `SolidColorBrush`)

Advanced scenarios (e.g. gradients or image brushes) are handled via the explicit per-edge properties and are intentionally out of scope for the shorthand.

### Constraints

- Three-value syntax is intentionally not supported.
- The shorthand supports solid colours only.
- Corner join geometry is not configurable via shorthand.

These constraints are intentional, keeping the shorthand predictable, debuggable, and aligned with .NET MAUI's existing mental model.

## Consequences

### Positive
- Enables a wider range of styling scenarios without introducing semantic opinion into the core framework.
- Keeps the API primitive, composable, and token-friendly.
- Aligns with .NET MAUI conventions and developer expectations.
- Moves unavoidable rendering complexity into FlagstoneUI rather than into each consuming app.
- Provides a stable foundation for theme-driven visual systems.

### Trade-offs / Limitations
- Rounded corners combined with per-edge borders are inherently messy; the initial implementation may clip or approximate corners.
- Advanced corner join geometry (e.g. angled bevel joins) is explicitly out of scope.
- Some controls (e.g. `FsButton`) must be wrapped rather than subclassed to maintain a consistent rendering model.

These limitations are considered acceptable and consistent with similar trade-offs on the web.

## Non-Goals

- Encoding visual semantics (e.g. raised/sunken) in FlagstoneUI primitives.
- Perfect mathematical handling of rounded corners with asymmetric borders.
- Supporting complex brush grammars or vector join styles via shorthand syntax.

## Alternatives Considered

- Using .NET MAUI `Border` with `StrokeThickness`: rejected due to lack of per-edge support.
- Using `Shape` or `Path` as the primary abstraction: rejected due to increased complexity and leakage of geometry concerns into theming.
- Introducing semantic border styles in core: rejected to preserve the primitives-only design philosophy.

## Notes

This decision aligns with FlagstoneUI's broader goal of making .NET MAUI applications as flexible to style as web applications, while remaining honest about trade-offs and avoiding leaky abstractions.
