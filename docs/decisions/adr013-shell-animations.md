# ADR013: Shell Animations — Deferred to Consumer Implementations

**update: the tab-item disposition below was later revised — see [ADR013_1 — FsTabBar Built-in Selection Animation](./adr013_1-fstabbar-built-in-selection-animation-addendum.md). The reference `FsTabBar` now ships with built-in selection animation (without resurrecting the FsTransition primitive), and "bring your own bar" is now the abstract `FsTabBarBase`. The two structural decisions here — no general VSM animation primitive, and page transitions deferred — still stand.**

**Status:** Accepted
**Date:** 2026-06-12
**Deciders:** Matt Goldman
**Scope:** FlagstoneUI.Core — FsShell V1. Covers both tab-item visual transitions and page-to-page transitions; both are deferred for different reasons.

## Context

Two animation concerns surfaced during FsShell V1 work:

1. **Tab-item visual transitions.** When the user taps a tab, the bar items' visuals (background, scale, opacity, etc.) snap between Unselected and Selected states. Native apps animate this; MAUI's `VisualStateManager` applies `<Setter>`s instantly via `SetValue`. There is no MAUI equivalent of WPF/UWP's `<VisualTransition>` declarative animation element. Closing this gap with a XAML-friendly primitive looked like an obvious V1 polish item.
2. **Page-to-page transitions.** When the active tab changes, Shell swaps the content page instantly. `ITabTransitionAnimator` was introduced in ADR012 as the extension point for custom transitions; the wiring was completed (the animator now receives the outgoing and incoming `Page` references in `FsTabTransitionContext`) but no built-in animator ships.

Both problems are visible. Both proved harder than expected in different ways. This ADR captures what was attempted, why each path was abandoned, and the V1 disposition.

## Decision

FlagstoneUI.Core does **not** ship an animation primitive in V1.

- **Tab-item animation is a consumer concern**, fulfilled via the existing extension point: `FsShell.TabBar` accepts any `ContentView` that implements `IFsTabBar`, and a consumer who wants animated selection visuals authors their own bar (or subclasses `FsTabBar`). The "bring your own bar" promise from ADR012 is the official path; a sample app demo will showcase an animated `IFsTabBar` as the canonical example.
- **Page transitions are deferred.** The `ITabTransitionAnimator` interface and `FsTabTransitionContext` (including `OutgoingPage`/`IncomingPage`) stay in place as a forward-compatible hook for a future implementation, but no V1 animator ships.

## What we tried

### FsTransition — attached property augmenting VSM

The intent was a primitive that turned VSM `<Setter>` application into an animated interpolation: set `fs:FsTransition.Duration="200"` on any `VisualElement`, and every subsequent change to a VSM-targeted property animates instead of snapping. Designed in ADR (now removed); implemented twice.

**v1 — deferred initialisation to `Loaded`.** The constructor subscribed to `Loaded` because attached properties parse in the XAML attribute pass and `<VisualStateManager.VisualStateGroups>` parses in the child-element pass that follows — so discovering watched properties in the constructor would see an empty group list. The fix was to walk the groups at `Loaded`, by which point everything is realised.

This failed because **templated items in `FsTabBar` aren't parented through MAUI's logical tree**. The bar's platform view is added as a `UIView` subview (or Android sibling) of the shell's native container; the bar's MAUI element doesn't propagate `Parent`/`Window` down through MAUI's hierarchy in the way that gives items a `Loaded` signal. The tracker waited for an event that never fired and stayed dormant — VSM still applied setters, but the result snapped because nothing was watching.

**v2 — subscribe to `PropertyChanged` immediately, resolve lazily.** No `Loaded` dependency. Every `PropertyChanged` event triggered a live lookup against the element's current `VisualStateGroups`; first sighting of a state-targeted property was captured as the starting value with no animation, subsequent changes animated. This should have sidestepped the parenting issue entirely.

It also did not work in practice. The user's interactive test showed snap-only behaviour on every tab transition with no visible interpolation. Root cause was not isolated before the work was abandoned; suspects include `PropertyChanged` not firing for these properties on bar-item `Border`s in the hosting configuration, our self-set re-entry guard misfiring, or the animation `Commit` running against an element MAUI doesn't consider animatable in this context. Pursuing the diagnosis further was judged not worth the cost relative to the alternative below.

### Page-transition wiring (ITabTransitionAnimator)

The interface and `FsTabTransitionContext` shipped in ADR012; `OutgoingPage` and `IncomingPage` were wired through to the animator in this work. The wiring is correct — those references resolve to the right materialised `Page` instances.

What we discovered is that `RunTransitionAsync` is invoked from `UpdateSelectedFlags`, which runs via `OnShellNavigated` — Shell's `Navigated` event. That's the *post*-navigation moment. The new page is already on screen and the outgoing page has been hidden by Shell before the animator gets called. A meaningful cross-fade or slide would need to either:

- Hook `Navigating` (the pre-event) instead of `Navigated`, capture the outgoing page snapshot, then run the animation between the snapshot and the live incoming page — substantially more wiring, and competes with Shell's own page presentation; or
- Operate at a layer below MAUI Shell (intercept the platform-level transition), which crosses the "no consumer platform code" line and is a much bigger surgery.

A third option emerged in discussion: a base `ContentPage` subclass exposing `OnNavigatingFrom`/`OnNavigatingTo` overrides that pages opt into. This sidesteps the Shell-internal swap timing entirely by letting the page itself drive its own enter/exit animation. That feels right and is the most likely V2 path, but it's a separate decision and is not scoped here.

## Decision drivers

**"Bring your own bar" already exists as the extensibility surface.** ADR012's `FsShell.TabBar` slot was always intended to absorb consumers who outgrew the default `FsTabBar`'s capabilities. Animated selection visuals are exactly the kind of "outgrew it" case the slot exists for. Adding a parallel Core animation primitive on top of an existing extension point is duplication; failing to make that primitive work reliably while the extension point is sitting right there makes the duplication worse.

**Two implementation attempts, neither converged.** v1 had a clear root cause (templated items don't fire Loaded in our hosting). v2 should have worked on paper and didn't, with no obvious explanation. Shipping a primitive whose failure mode we don't understand is a maintenance bet we don't need to take when an alternative path exists.

**Page transitions are the wrong altitude.** The Shell-Navigated timing problem isn't a bug in our wiring; it's the wrong place in the lifecycle for what we want. Solving it properly means rethinking *where* the hook lives, not how the animator behaves. A base `ContentPage` with navigation hooks is a cleaner problem statement than continuing to push from FsShell.

**The MCT integration shape is wrong for both.** `CommunityToolkit.Maui.Animations.BaseAnimation<T>` is continuous, imperative, externally triggered — designed for things like the gradient rotation in `FsEditorBorderAnimation`. Both state-driven tab transitions and page-to-page transitions are discrete, declarative-friendly, and bounded. They sit on MAUI's `Animation`/`ViewExtensions.Animate` primitives directly and would not benefit from a unifying layer.

## Alternatives considered (and why not now)

**Implement `<VisualTransition>` declaratively in MAUI.** WPF/UWP ship this element inside `VisualStateGroup`; you write `<VisualTransition From="X" To="Y" Duration="..."/>` and the runtime interpolates. MAUI hasn't shipped it. Building it ourselves would require either forking MAUI's VSM internals or wrapping `VisualStateManager.GoToState` — both are large undertakings for a fix that helps only the case where someone is using stock VSM. If MAUI ships this upstream the question reopens; we shouldn't pre-empt them.

**Build animations into each FlagstoneUI control directly** (`FsTabBar.SelectionAnimation`, `FsButton.PressAnimation`, etc.). Rejected on the same grounds the FsTransition design rejected it: solves the same problem N times, locks the animation shape per control, and doesn't help consumer-authored VSM elsewhere. The right altitude is "every `VisualElement` that pumps VSM gets the benefit," which is what FsTransition was attempting; if we can't make a single primitive work, splitting into N control-specific implementations is worse.

**Behaviors on bar items.** Composes with VSM and works, but requires a `<VisualElement.Behaviors>` collection in the template — verbose, conflicts with consumer behaviors, and is documentation-heavy. A consumer who's willing to write a behavior is also willing to write a custom `IFsTabBar`, and the latter is the more direct path.

**An `FlagstoneUI.Animations` integration library**, parallel to `FlagstoneUI.Integrations.MCT`. Opt-in dependency, registered via a builder extension, not part of Core. Could ship FsTransition, page-transition base classes, custom interpolators — whatever stabilises. Promising; deferred to post-V1 along with a clean re-attempt at FsTransition once we understand what broke v2.

## Consequences

### Positive

- V1 ships without an animation primitive whose failure modes we don't understand. The "bring your own bar" story is unambiguous and aligns with how FsShell was already designed.
- `ITabTransitionAnimator` and `FsTabTransitionContext` remain part of the public API, so consumers who want to experiment with page transitions can do so today (with full awareness of the post-swap timing constraint).
- The Core animation surface is zero, which is the right starting point for a primitive we'll likely revisit. Adding the primitive later is easy; removing it once consumers depend on it is hard.

### Negative / accepted

- Out-of-the-box `FsTabBar` selection has no animated transition. Consumers who want one author their own bar. We accept this as a documented-but-real ergonomic gap for V1.
- `OutgoingPage`/`IncomingPage` on `FsTabTransitionContext` are wired but unused by anything that ships. Forward-compatible dead weight; the alternative is removing them and risking a breaking change when the hook is filled.
- Two abandoned implementation attempts on FsTransition cost real time. We retain the learnings here so the next attempt — if there is one — starts from a different angle.

### Operational

- The Core directory `src/FlagstoneUI.Core/Animations/` does not exist in V1. ADR013's earlier draft (FsTransition design) has been removed; the slot is reused for this decision.
- `FsTabTransitionContext.OutgoingPage`/`IncomingPage` are documented in `docs/controls/FsShell.md` (or will be) with the caveat that post-swap timing limits their usefulness today.
- Sample app to include an animated `IFsTabBar` example as the canonical demo of the "bring your own bar" story. Until that ships, the documentation references the default `FsTabBar` and notes that animated variants are a consumer responsibility.

## Learnings worth preserving

### MAUI VSM vs WPF/UWP VSM

MAUI's `VisualStateManager` applies `Setter`s directly via `SetValue` with no notion of transitions. WPF and UWP ship `<VisualTransition>` inside `<VisualStateGroup>` for declarative interpolation between states; MAUI does not. This is the structural gap behind the entire problem. Any FlagstoneUI primitive that tries to fill it has to either intercept VSM (sealed-ish) or react to the downstream `PropertyChanged` (timing-sensitive in non-trivial hosting setups).

### `Loaded`/`Unloaded` are unreliable for templated items in non-MAUI-parented containers

When `FsTabBar`'s items are templated into its inner Grid, and that Grid is hosted as a native `UIView`/`AView` subview (not added through MAUI's parent-child hierarchy), the templated items' `Loaded` event does not fire. This is the specific configuration FsShell uses, and it broke FsTransition v1. The general lesson: don't use `Loaded` as a "fully realised" signal for items that may end up in a hybrid MAUI/native hosting arrangement. Subscribing to `PropertyChanged` from construction and resolving state lazily is more robust, but as v2 showed, "more robust" is not "actually works."

### Different animation problems have different shapes

The discussion that ruled out unifying FsTransition with `FlagstoneUI.Integrations.MCT.Animations.FsEditorBorderAnimation` clarified that "animate a property over time" decomposes into at least two distinct problem shapes:

- **Discrete state-driven transitions.** Start at a previous value, end at a target value, duration is bounded, animation completes and stays at the end. Tab-item visuals and page transitions both fit here.
- **Continuous imperative animations.** No defined start or end value; loops while running; cancellation is the only stop signal. Brush gradient rotation, indeterminate progress visuals, attention-getting effects.

These do not benefit from a shared FlagstoneUI layer above MAUI's `Animation`/`ViewExtensions.Animate` infrastructure. Future animation work should not try to unify them.

### Page-transition timing requires a different hook than `ITabTransitionAnimator` currently has

`UpdateSelectedFlags` runs via `OnShellNavigated`, which is the post-navigation event. By then, Shell has already swapped pages. An animator called there sees the new page on screen and cannot run a meaningful between-pages transition without overlay snapshots or pre-swap interception. Future implementers should consider:

- Hooking `Navigating` (pre-event) instead of, or in addition to, `Navigated`.
- A base `ContentPage` with `OnNavigatingFrom`/`OnNavigatingTo` overrides that pages opt into and that drive enter/exit animations from the page itself — likely the cleaner shape.

### Diagnostic tooling debt

Both FsTransition attempts ran longer than they should have because we don't have a great way to inspect what `PropertyChanged` is firing on a templated item in a hosted bar. Future MAUI/Shell-adjacent work would benefit from a small diagnostic mode (gated by `Debug.WriteLine` or similar) that logs the relevant event timing without requiring per-attempt instrumentation.

## Open questions / triggers for re-opening

- **MAUI ships `<VisualTransition>` upstream.** The primary motivation for FsTransition disappears; the question becomes whether we still want a higher-level wrapper. Probably not.
- **`FlagstoneUI.Animations` integration library viability.** If a consumer base materialises and asks for animation primitives consistently, an opt-in integration library is the right vehicle — same shape as the MCT integration.
- **Base `ContentPage` with navigation hooks.** Independent enough from FsShell to deserve its own ADR when picked up. Most likely path for page transitions.
- **Root cause of FsTransition v2's failure.** If someone has the appetite to instrument and isolate it, the learnings would inform whether the v2 approach is reattemptable or fundamentally blocked by something we haven't named.

## References

- [ADR012 — FsShell: Stylable Shell Chrome via Subclass](./adr012-fsshell.md) — the `IFsTabBar` extension surface this ADR relies on.
- [ADR012_1 — Per-`ShellItem` Tab Bar Scoping](./adr012_1-fsshell-per-item-bar-scoping-addendum.md)
- [ADR012_2 — FsShell Renderer Scope Narrowing](./adr012_2-fsshell-renderer-scope-narrowing-addendum.md)
- [ADR012_3 — Bottom Chrome Height Resource Contract](./adr012_3-fsshell-bottom-chrome-resource-contract-addendum.md)
- [`FsTabTransitionContext`](../../src/FlagstoneUI.Core/Controls/Shell/ITabTransitionAnimator.cs) — wiring retained for future implementations.
- [`FlagstoneUI.Integrations.MCT.Animations.FsEditorBorderAnimation`](../../src/FlagstoneUI.Integrations.MCT/Animations/FsEditorBorderAnimation.cs) — the cousin pattern for continuous imperative animations, kept separate from the state-driven case for the reasons documented above.
