# ADR Addendum: FsTabBar — Built-in Selection Animation and the `FsTabBarBase` Extraction

**note: this is an addendum to [ADR013 — Shell Animations: Deferred to Consumer Implementations](./adr013-shell-animations.md). It revises ADR013's tab-item disposition only. The two structural decisions ADR013 made — that Core ships no general VSM animation primitive, and that page-to-page transitions are deferred — stand unchanged.**

**Status:** Accepted
**Date:** 2026-06-19
**Deciders:** Matt Goldman
**Scope:** FlagstoneUI.Core — FsShell V1, reference `FsTabBar`

## Context

ADR013 deferred all shell animation to consumers. For tab-item visuals specifically it concluded:

> *"Out-of-the-box `FsTabBar` selection has no animated transition. Consumers who want one author their own bar."*

…and pointed at a future sample app as the canonical animated `IFsTabBar`. That conclusion was reached after two failed attempts at **FsTransition**, a primitive that tried to animate MAUI `VisualStateManager` `<Setter>` application. The failures were specific to *intercepting/augmenting VSM on templated bar items in FsShell's hybrid native hosting* — `Loaded` never fired (v1), and reacting to `PropertyChanged` produced snap-only behaviour for reasons never isolated (v2).

Since ADR013, the reference `FsTabBar` was given a built-in animated selection treatment — a pill that slides behind the active tab and a scale bump on the selected tab — and the control was split into an abstract `FsTabBarBase` plus the concrete `FsTabBar`. This addendum records that disposition change and, importantly, *why it does not reopen the problem ADR013 abandoned*.

## Decision

1. **The reference `FsTabBar` ships with built-in selection animation.** A sliding pill (`ShowPill`) and a scaling selected tab (`ScaleSelectedTab`), each independently toggleable; the pill's fill and geometry are configurable via `PillBackground` (a `Brush`) and `PillShape` (any `IShape`, the pill being a `Border`).

2. **The animation deliberately does not touch VSM transitions.** Rather than animating VSM-targeted properties on templated items — the thing FsTransition could not make work — the bar animates **elements it owns directly**: a separate pill `Border` and each item's `Scale`, driven imperatively from code via MAUI's own `ViewExtensions` async animations (`TranslateToAsync`/`ScaleToAsync`) in response to a selection-changed hook. VSM pumping (`Selected`/`Unselected`, `Normal`/`Disabled`) is unchanged and still snaps, exactly as before. ADR013's VSM-transition problem is **sidestepped, not solved**; every learning in ADR013 still holds.

3. **"Bring your own bar" is formalised into a reusable base.** `FsTabBarBase` (abstract, no XAML) owns the `IFsTabBar` contract, item materialisation, subscription bookkeeping, tap routing, and VSM pumping. A custom bar supplies a `TabContainer` and overrides hooks — `OnSelectionChanged(context, animated)`, `OnSelectionInitialized()`, optionally `BuildDefaultItemTemplate()` and `OnTabTapped()` — with `FindTab`/`SelectedIndex` helpers provided. The reference `FsTabBar` is itself just one subclass.

4. **`AnimateTransitions` is the single consumer switch** for selection transition animation. Defined on `FsTabBarBase`, it flows to subclasses as the `animated` argument of `OnSelectionChanged`; a subclass may honour it (as `FsTabBar` does — smooth vs. instant) or ignore it. This is the "enable/disable at the consumer layer, implementers choose whether to animate" API.

## Why this is consistent with ADR013

ADR013 punted for two reasons, **neither of which applies here**:

- **The primitive kept failing.** That was about *augmenting VSM* — intercepting setter application or reacting to downstream `PropertyChanged` on templated items in hybrid hosting. The reference bar animates its own pill and item scale from code; it never tries to animate a VSM setter, so the failure mode is structurally absent.
- **Page transitions were the wrong altitude.** Unrelated — this addendum is purely tab-item, in-bar, and does not go near Shell's page-swap timing or `ITabTransitionAnimator`.

ADR013 also classified tab-item visuals as a *discrete, bounded, state-driven* animation that "sits on MAUI's `Animation`/`ViewExtensions.Animate` primitives directly." That is exactly what this implementation does. The decision driver "bring your own bar already exists as the extensibility surface" is *strengthened*: it is now a first-class base class, not a copy-paste of the reference bar.

## Relationship to ADR013

- **Corrects** the disposition "out-of-the-box `FsTabBar` selection has no animated transition." It now does, by default, configurably. The future sample is no longer the *only* animated example — the shipping default is one.
- **Preserves unchanged**: Core ships **no general VSM animation primitive** (FsTransition stays unbuilt and its `src/FlagstoneUI.Core/Animations/` slot stays empty); **page transitions stay deferred** (`ITabTransitionAnimator`/`FsTabTransitionContext` untouched); the MCT continuous-vs-discrete animation separation; and the "no consumer-facing platform code" principle (the animation is pure cross-platform MAUI).

## Consequences

### Positive

- The default bar looks finished out of the box — the documented-but-real ergonomic gap ADR013 accepted is closed for the common case.
- "Bring your own bar" is now a genuine base class (`FsTabBarBase`) with hooks, not a re-implementation exercise; building a differently-shaped bar no longer means re-deriving the items/selection/VSM plumbing.
- The pill accepts any `Brush` and any `IShape`, so consumers get gradients and arbitrary geometry without subclassing.

### Negative / accepted

- `FsTabBar` now carries animation code and four bindable properties (`PillBackground`, `PillShape`, `ShowPill`, `ScaleSelectedTab`) plus the inherited `AnimateTransitions`. Accepted as the cost of a polished reference control.
- The pill is a content-less `Border` sized by explicit `WidthRequest`/`HeightRequest`. This relies on MAUI honouring explicit size requests on an empty `Border` across all four platforms; **pending runtime visual verification**. If a platform refuses to size it, the fallback is a transparent sizing child inside the `Border`.
- XAML-backed subclasses of `FsTabBarBase` must name the base type as their XAML root element (a `clr-namespace` reference), a minor authoring wrinkle documented in `docs/controls/FsTabBar.md`.

## Verification

- Builds clean across all four target frameworks (`net10.0`, `-android`, `-ios`, `-maccatalyst`).
- Runtime visual confirmation of the pill slide, scale bump, initial-selection placement, and the empty-`Border` sizing is still outstanding and should be done on at least one mobile target before this is considered closed.

## References

- [ADR013 — Shell Animations: Deferred to Consumer Implementations](./adr013-shell-animations.md) — the decision this addendum revises.
- [ADR012 — FsShell: Stylable Shell Chrome via Subclass](./adr012-fsshell.md) — the `IFsTabBar` extension surface.
- [FsTabBar Control](../controls/FsTabBar.md) — full property/extension-point documentation.
- [`FsTabBarBase`](../../src/FlagstoneUI.Core/Controls/Shell/FsTabBarBase.cs) / [`FsTabBar`](../../src/FlagstoneUI.Core/Controls/Shell/FsTabBar.xaml.cs) — the base/derived split.
