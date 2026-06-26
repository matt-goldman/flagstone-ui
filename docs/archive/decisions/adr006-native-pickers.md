# ADR006: Native Pickers Remain Native in FlagstoneUI Core

## Status
Accepted

## Context

FlagstoneUI exists to address a long-standing gap in the .NET MAUI ecosystem: while .NET MAUI provides an abstraction over native UI controls, the **customisation surface exposed by those controls has not kept pace with modern expectations**, particularly when compared to the web.

FlagstoneUI’s role is to expose **token-friendly, cross-platform primitives** that make it easy to style applications consistently, without requiring deep platform-specific knowledge or handler-level customisation.

However, not all controls are equal in this regard.

Controls such as `Picker`, `DatePicker`, and `TimePicker` are fundamentally different from core layout and input primitives (e.g. `Entry`, `Button`, `Card`):

- They are **OS-mediated experiences**, not purely visual elements
- They invoke platform-provided modal or transient UI
- They are tightly coupled to accessibility, localisation, and user expectations
- They deliberately defer visual control to the operating system

This distinction also exists on the web. While many elements are highly stylable via CSS, native inputs such as `<select>`, `<input type="date">`, and `<input type="time">` typically invoke browser- and OS-provided UI. Fully custom pickers on the web are implemented as **entirely separate controls**, not as stylistic extensions of the native input.

## Decision

FlagstoneUI Core will **not** attempt to replace or deeply customise OS-native picker experiences, including but not limited to:

- Picker
- DatePicker
- TimePicker

Instead:

- FlagstoneUI Core may style the **invocation surface** of these controls (e.g. typography, borders, spacing, focus states), where this can be done in a consistent and cross-platform manner.
- The modal or transient picker UI itself remains the responsibility of the operating system.
- Fully custom picker implementations are considered **distinct controls** and do not belong in FlagstoneUI Core.

This decision intentionally preserves native behaviour, accessibility, and user expectations while keeping FlagstoneUI focused on its primary responsibility: surfacing a richer, consistent styling API for MAUI applications.

## Consequences

### Positive
- Preserves platform-native picker behaviour and accessibility guarantees.
- Avoids leaky abstractions and platform-specific hacks in core.
- Keeps FlagstoneUI focused on styling primitives rather than full control reimplementation.
- Aligns with established patterns on the web and in native UI frameworks.
- Establishes a clear layering boundary for future extensions.

### Trade-offs / Limitations
- Deep visual customisation of picker UI is not possible within FlagstoneUI Core.
- Developers who require fully custom picker experiences must opt into custom control implementations.
- Some visual inconsistency between platforms is accepted by design.

These trade-offs are considered acceptable and intentional.

## Non-Goals

- Providing fully custom date, time, or selection picker controls in FlagstoneUI Core.
- Abstracting or reimplementing OS picker behaviour.
- Guaranteeing visual parity of picker UI across platforms.

## Future Considerations

This decision does **not** preclude:

- A higher-level control or component library built on top of FlagstoneUI.
- Optional, fully custom picker controls implemented as distinct components.
- Re-evaluating this boundary in the future if platform capabilities or project goalsDFwF goals change.

If this decision is revisited, it should be superseded by a new ADR rather than modifying this one.

## Alternatives Considered

- Fully custom picker controls in core: rejected due to scope creep, accessibility risk, and violation of FlagstoneUI’s primitives-first philosophy.
- Deep styling of native picker UI via handlers: rejected due to platform inconsistency and maintenance burden.
- Avoiding pickers entirely: rejected as impractical.

## Notes

This ADR captures a point-in-time architectural decision about scope and responsibility. Its purpose is to preserve the reasoning behind the decision, even if it is revisited or reversed in the future.
