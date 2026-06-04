# FsShell V1 Specification

**Status:** Draft
**Target:** FlagstoneUI.Core V1
**Related:** [ADR: FsShell — Stylable Shell Chrome via Subclass](./adr-fsshell.md)

## Goal

Provide a drop-in replacement for `.NET MAUI Shell` that allows the bottom tab bar to be fully styled in shared XAML/C#, without consumer-facing platform code, while preserving Shell's routing, navigation, and lifecycle semantics unchanged.

## Non-goals

- Replacing flyout chrome, navigation bar, top-tab strip, or search handler. Reserved for future versions.
- Providing default tab transition animations. The extension point is exposed; no implementations ship in V1.
- Shipping a navigation framework or new routing primitives. `FsShell` uses stock Shell routing.
- Editorialising about platform-appropriate use of Shell. FsShell renders the consumer's declared tab template consistently across all platforms Shell supports; whether bottom tabs are idiomatic on a given platform is a decision the consumer makes by writing them.

## Platform support

V1 supports every platform on which MAUI Shell itself runs: iOS, Android, MacCatalyst, and Windows. The Windows implementation goes through a different platform path (`NavigationView` from WinUI) than the others (`UITabBarController` / `BottomNavigationView`), and the renderer code reflects that, but the consumer-facing API and behaviour are identical across all four.

## Principles

The following principles take precedence over any specific design choice in this document. If implementation pressure forces a trade-off, these win:

1. **Drop-in compatibility.** `FsShell` substitutes for `Shell` per LSP. Any code that compiles and runs against `Shell` compiles and runs against `FsShell` with identical semantics, with the sole exception of the visual tab bar rendering.
2. **No consumer-facing platform code.** Consumers do not write `#if IOS` or `#if ANDROID`, do not subclass renderers, and do not register handlers. The word "renderer" does not appear in any consumer-facing docs or sample.
3. **XAML-discoverable surface.** Configuration happens through bindable properties on `FsShell` and standard MAUI templating idioms. IntelliSense and XAML compilation should make the API self-evident to anyone who already knows Shell.
4. **No required Fs-specific children.** All existing Shell child element types (`ShellItem`, `ShellSection`, `ShellContent`, `Tab`, `TabBar`, `FlyoutItem`) work unchanged inside `FsShell`. Per-tab metadata (title, icon) is read from the existing `Shell.Title` and `Shell.Icon` attached properties.

## Public API

### `FsShell : Shell`

Subclass of `Microsoft.Maui.Controls.Shell`. Adds the following bindable properties:

- **`TabBarItemTemplate`** (`DataTemplate`)
  Template applied to each tab in the default `FsTabBar`. Receives an `FsTabContext` as `BindingContext`. If null, the default item template ships with the library is used (sensible defaults: icon over title, selected state via colour).
- **`TabBar`** (`ContentView`)
  Optional. Replaces the entire bar with a consumer-supplied `ContentView`. When set, `TabBarItemTemplate` is ignored and a debug-level warning is logged. The supplied view is expected to honour the bar contract (see *Bar replacement contract* below).
- **`TabTransitionAnimator`** (`ITabTransitionAnimator`)
  Optional. If set, invoked on tab selection changes to drive a transition between the outgoing and incoming tab content. If null, content swaps instantly (current Shell behaviour).

All other Shell properties — `FlyoutBehavior`, `Shell.TabBarIsVisible` (attached), `Shell.TabBarBackgroundColor` (attached), routing properties, etc. — are inherited unchanged. The Shell appearance attached properties (`Shell.TabBarBackgroundColor`, `Shell.TabBarTitleColor`, etc.) continue to work in `FsShell` and are honoured by the default `FsTabBar` where applicable, so consumers migrating from `Shell` retain those declarations.

### `FsTabBar : ContentView`

The reference tab bar implementation. Public, documented, and subclassable. Used internally by `FsShell` as the default bar; can also be used directly in the bar-replacement slot.

Bindable properties:

- **`ItemsSource`** (`IReadOnlyList<FsTabContext>`)
  Auto-populated by `FsShell` when `FsTabBar` is hosted as the default bar. Settable directly when `FsTabBar` is used standalone or wrapped inside a custom layout.
- **`ItemTemplate`** (`DataTemplate`)
  Template applied to each tab. When `FsTabBar` is used as the default inside `FsShell`, this is forwarded from `FsShell.TabBarItemTemplate`.
- **`SelectedRoute`** (`string`, two-way)
  The route of the currently selected tab. Setting this navigates Shell to that route; observing this reflects external navigation back into the bar's selection state.

Internally, `FsTabBar` uses a `BindableLayout`-driven layout (likely `Grid` or `HorizontalStackLayout` depending on configuration) so that template instances participate in normal MAUI layout and can be styled with the standard layout, sizing, and visual-state idioms.

### `FsTabContext`

Public type. Represents a single tab's data, presented to the item template as `BindingContext`. Properties:

- **`Route`** (`string`) — the Shell route, derived from the corresponding `ShellContent.Route`.
- **`Title`** (`string`) — sourced from `Shell.Title` on the underlying `ShellContent`.
- **`Icon`** (`ImageSource`) — sourced from `Shell.Icon` on the underlying `ShellContent`.
- **`IsSelected`** (`bool`, observable) — updated by the bar as the active tab changes; consumers bind to this from inside the template to drive selected-state visuals.
- **`IsEnabled`** (`bool`, observable) — reserved for future "disabled tab" support; defaults to `true`.

`FsTabContext` is the binding context type, deliberately distinguished from any future `FsTabItem` *control* type to avoid the naming ambiguity. The name communicates "context for a tab," not "tab control."

### `ITabTransitionAnimator`

Interface for pluggable tab transitions.

```csharp
public interface ITabTransitionAnimator
{
    Task AnimateAsync(
        FsTabTransitionContext context,
        CancellationToken cancellationToken);
}
```

`FsTabTransitionContext` exposes the outgoing and incoming page views, the bar instance, the previous and new selected indices, and any other state needed by an animator implementation. No default implementation ships in V1; a null animator means instant switching, matching current Shell behaviour.

## Bar replacement contract

When a consumer supplies their own `ContentView` to `FsShell.TabBar`, the contract is intentionally minimal:

- The view receives an `IReadOnlyList<FsTabContext>` via a known mechanism (bindable property `ItemsSource` if the view exposes it; otherwise via `BindingContext` containing the items).
- The view raises selection changes by setting a known `SelectedRoute` bindable property, or by raising an `ItemSelected` event.

The simplest path is to subclass `FsTabBar` or use it composed inside a wrapping layout. For consumers who want to write their own `ContentView` from scratch, an `IFsTabBar` interface is exposed exposing the minimal contract. Implementing it is the contract; not implementing it falls back to convention-based binding lookup.

The intent: a consumer with a pre-existing custom `ContentView` should be able to make it work as an `FsShell` tab bar in fewer than ten lines of glue code.

## Behaviour

### Tab selection

Tap on a tab → bar updates `SelectedRoute` → `FsShell` translates the route to a Shell navigation (`Shell.Current.GoToAsync` or equivalent direct property assignment, depending on whether intermediate URI semantics matter). Routing parameters, query properties, and `OnNavigating` / `OnNavigated` lifecycle events fire identically to stock Shell.

External navigation (`GoToAsync` from code) → Shell raises navigation event → `FsShell` updates `FsTabBar.SelectedRoute` → bar re-renders selection state. The bar must not assume it is the only source of truth for the selected tab.

### Bar visibility

`Shell.SetTabBarIsVisible(page, false)` continues to hide the bar on a per-page basis, exactly as in stock Shell. Internally, this is bridged from the platform renderer's bar-visibility tracking to a `IsVisible` toggle on the FlagstoneUI bar instance.

### Safe area, keyboard, and modal interaction

These are the rough edges that distinguish a polished tab bar from one that "almost works." V1 must handle:

- **Safe area inset (iOS home indicator).** The bar's bottom edge respects the safe area inset. Consumers writing the item template do not need to think about insets; the bar handles it at the container level.
- **Keyboard avoidance.** When the soft keyboard is presented, the bar slides off-screen, matching the standard iOS and Android behaviour. Re-appears when the keyboard dismisses. This behaviour is on by default and toggleable via a `FsShell.HideTabBarOnKeyboard` (or similar) attached property.
- **Modal presentation.** When a modal page is presented over the Shell, the bar is hidden along with the underlying chrome, matching stock Shell behaviour. No consumer action required.

These behaviours are implemented inside the platform renderer code and are not consumer-facing.

### Animation

When `TabTransitionAnimator` is set, on each tab selection change:

1. `FsShell` resolves the outgoing and incoming page views.
2. Constructs an `FsTabTransitionContext`.
3. Invokes `AnimateAsync` and awaits its completion before considering the transition finished.
4. If the animator throws or the cancellation token is signalled, falls back to instant swap and logs the failure at warning level.

The animator is not invoked on the initial appearance of the Shell or on programmatic navigation that does not involve a tab change.

## Implementation notes

The following are implementation guidance, not API contract. They are recorded here so that contributors and future-self have a starting point and don't relitigate the choices unnecessarily.

### Platform structure

- `Platforms/iOS/FsShellRenderer.cs` — subclass of `Microsoft.Maui.Controls.Platform.Compatibility.ShellRenderer`. Overrides item/section renderer creation to suppress the native `UITabBar` and host an `FsTabBar` instance in its place.
- `Platforms/Android/FsShellRenderer.cs` — equivalent for Android, replacing or hiding `BottomNavigationView`.
- `Platforms/MacCatalyst/FsShellRenderer.cs` — equivalent for MacCatalyst, with whatever differences from iOS prove necessary.
- `Platforms/Windows/FsShellRenderer.cs` — equivalent for Windows, suppressing or replacing the WinUI `NavigationView`-based chrome that stock Shell produces.

These types are `internal`. They are registered via `MauiAppBuilder.UseFlagstoneUI()` so that consumers do not invoke or reference them directly.

### Renderer registration

The bootstrap extension method registers the FlagstoneUI renderer for `FsShell` only, not for stock `Shell`. Consumers using stock `Shell` are unaffected by installing FlagstoneUI; only consumers who explicitly opt in by writing `FsShell` in their XAML get the FlagstoneUI behaviour. This is a deliberate consequence of the subclass-based approach (see ADR for full rationale).

### Items collection

`FsShell` enumerates its `ShellContent` descendants on attachment and on visual-tree changes, projecting each into an `FsTabContext`. The projection observes `Shell.Title` and `Shell.Icon` attached properties for live updates.

### LSP test surface

A test project exercises every public method, property, and event on `Shell` against an `FsShell` instance, asserting equivalent behaviour. New tests added to the LSP suite when new Shell features are released. This is the same approach used for the existing FlagstoneUI controls and does not introduce a new pattern.

## Migration

Consumers migrating from `Shell` to `FsShell`:

1. Add `FlagstoneUI.Core` to the project (if not already present).
2. Call `.UseFlagstoneUI()` in `MauiProgram.cs` (if not already present for other FlagstoneUI controls).
3. Change `<Shell ...>` to `<flagstone:FsShell ...>` in each AppShell (or equivalent) XAML file.
4. Optionally, define `FsShell.TabBarItemTemplate` to customise the bar.

No other code changes are required. Existing `ShellContent` declarations, route registrations, `GoToAsync` calls, and lifecycle overrides continue to work unchanged.

## Open questions

- **Top tabs (multiple `ShellContent` inside a `Tab`).** Currently out of scope for V1 visual replacement, but the underlying routing must continue to work. Verify behaviour when an `FsShell` contains a `Tab` with multiple `ShellContent` — probably falls through to stock Shell rendering for the top strip while the bottom bar uses FsTabBar.
  - **Resolution:** Acceptable for V1; flag in docs.
- **Selection model on the bar.** `SelectedRoute` (string) vs. `SelectedItem` (`FsTabContext`) vs. both. Leaning toward `SelectedRoute` as the primary, with `SelectedItem` as a derived convenience. Resolve before API freeze.
  - **Resolution:** Goal is both; either is acceptable on the concrete `FsTabBar`. Internal storage is expected to be a dictionary keyed by route with `FsTabContext` as value (or similar), so exposing both is essentially free and the implementation can decide which is "primary" as it shakes out.
  - **However**, the public bar-replacement contract (`IFsTabBar`) is genuinely a freeze question — changing it post-V1 breaks every consumer-authored bar in the wild. The interface therefore commits to `SelectedRoute` (string) only, as the minimum a custom bar must implement. `SelectedItem` (and any equivalent context-typed accessor) lives on the concrete `FsTabBar` only and can be promoted to the interface non-breakingly later if it proves useful. Implementation strategy stays free; the contract is locked.
- **`IsSelected` propagation for templates.** `FsTabContext.IsSelected` is the canonical source. Whether to also expose visual states (`Selected`, `Unselected`, `Disabled`) via the standard MAUI `VisualStateManager` should be decided alongside the default item template — consumers familiar with VSM-driven styling will expect it.
  - **Resolution:** Required and critical. Visual states and VSM are expected as standard .NET MAUI behaviour and we will support them. Two specifics:
    1. **`CommonStates` group** — driven by `FsTabContext.IsEnabled`. Standard `Normal` / `Disabled` (and `Focused` where the platform surfaces it). Matches what consumers expect on every other MAUI control.
    2. **`SelectionStates` group** — driven by `FsTabContext.IsSelected`. `Selected` / `Unselected`. Kept as a separate group from `CommonStates` so that selected-and-disabled is expressible (the two concerns compose). Pending a quick check of what stock Shell uses on its tab items; if Microsoft has settled on a different group name, match that for least-surprise.
    Both groups are pumped by `FsTabBar` on the template-instance root; subclassers and custom bars are expected to do the same. The default item template will demonstrate both groups in use.

## Delivery plan

The bootstrap (commit landing alongside this spec) covers the cross-platform API skeleton: `FsShell`, `FsTabBar`, `FsTabContext`, `IFsTabBar`, `ITabTransitionAnimator`, `FsTabTransitionContext`, the `UseFlagstoneUI()` registration, and stub `FsShellRenderer` partials on every supported platform. Everything below is what's required to take that skeleton to a shippable V1.

Tasks are ordered roughly by dependency, but several streams (per-platform renderers, animator, sample app) can run in parallel once the cross-platform API is frozen.

### A. Cross-platform API — finish & freeze

- [x] **Resolve open question: bar selection model.** Decide between `SelectedRoute` (string) only, `SelectedItem` (`FsTabContext`) only, or both with one as canonical and the other derived. Update `IFsTabBar` and `FsTabBar` accordingly. Document the choice in the ADR.
- [x] **Resolve open question: VSM on the default template.** Decide whether `FsTabContext.IsSelected` is the only selected-state hook or whether `FsTabBar` also pumps `VisualStateManager` `Selected` / `Unselected` / `Disabled` states on each template instance. If yes, wire the state transitions in the bar's tap/selection handler.
- [x] **Resolve open question: top tabs.** Verify what happens when a `Tab` contains multiple `ShellContent`. Either (a) confirm stock Shell top-strip rendering still occurs and document it, or (b) decide on a fallback. No code change expected; this is a verification + docs task.
- [ ] **Tighten the `FsTabContext` projection.** Bootstrap reads `Shell.Title` / `Shell.Icon` from the underlying `ShellContent` once at projection time. Replace with live observation: subscribe to `PropertyChanged` on each `ShellContent` so title/icon edits at runtime flow through to the bar.
- [ ] **Refine the items rebuild.** `FsShell.RebuildTabs` currently re-clears on every child mutation. Diff instead so that existing `FsTabContext` instances are preserved across rebuilds (template instances stay bound to the same context, no flicker).
- [ ] **`FlyoutItem` handling.** Decide whether `FlyoutItem`s contribute to the tab bar in V1 (probably no, since flyout chrome is out of scope) and document/enforce the decision in `RebuildTabs`.
- [ ] **`Shell.SetTabBarIsVisible` bridging at the cross-platform layer.** Surface the per-page bar visibility decision back to `FsTabBar.IsVisible`, in addition to the per-platform plumbing.
- [ ] **`HideTabBarOnKeyboard` attached property.** Defined in the spec but not yet on `FsShell`. Add the attached property + default-true wiring; the per-platform code reads it.
- [ ] **Strongly typed `FsTabTransitionContext`.** Bootstrap passes `null` for `OutgoingView` / `IncomingView`. The renderer needs to populate these from the platform page swap; coordinate the contract once the first renderer is in place.
- [ ] **Public API review.** Once the above settle, take a final pass: nullability, `sealed` vs not on `FsTabBar`, virtual hooks for subclassers, internal vs public on event args.

### B. Platform renderers — replace bar chrome

Each platform follows roughly the same shape: subclass `ShellRenderer`, suppress the native bar, host `FsTabBar` (or the consumer-supplied bar) as a sibling at the bottom of the platform shell view, and honour the bar-visibility / safe-area / keyboard / modal contracts.

- [ ] **iOS — `FsShellRenderer`.**
  - Override `CreateShellSectionRenderer` (or `CreateShellItemRenderer`) to suppress the native `UITabBar`.
  - Host the FlagstoneUI bar as a `UIView` pinned to the bottom of the shell's container view.
  - Project `FsTabContext` items into the bar (already done at the cross-platform layer; renderer just hosts the view).
  - Bridge `Shell.SetTabBarIsVisible(page, false)` to the bar's `IsVisible`.
  - Respect the bottom safe-area inset (home indicator). The bar's bottom padding adapts to `additionalSafeAreaInsets`.
  - Slide the bar off-screen on `UIKeyboardWillShow`; restore on `UIKeyboardWillHide`. Gated by `HideTabBarOnKeyboard`.
  - Hide the bar when a modal page is presented over the shell; restore on dismissal.
  - Populate `FsTabTransitionContext.OutgoingView` / `IncomingView` on tab change and await `ITabTransitionAnimator.AnimateAsync` before swapping.

- [ ] **MacCatalyst — `FsShellRenderer`.** Mostly identical to iOS, with whatever differences emerge during implementation (safe-area is generally a no-op; modal handling and keyboard avoidance still apply).

- [ ] **Android — `FsShellRenderer`.**
  - Suppress / hide `BottomNavigationView` in the shell fragment hierarchy.
  - Host the FlagstoneUI bar as an Android `View` (likely via `ContentViewGroup` or hosting `FsTabBar`'s handler) in the shell's coordinator layout.
  - Bridge `Shell.SetTabBarIsVisible`.
  - Respect Android system bars / gesture navigation insets (`WindowInsetsCompat`).
  - Hide the bar on soft keyboard show; restore on hide. Gated by `HideTabBarOnKeyboard`.
  - Hide on modal page presentation; restore on dismissal.
  - Populate `FsTabTransitionContext` and await the animator on tab change.

- [ ] **Windows — `FsShellRenderer`.**
  - Suppress / replace the WinUI `NavigationView` chrome that stock Shell produces.
  - Host the FlagstoneUI bar at the bottom of the shell's root layout.
  - Bridge `Shell.SetTabBarIsVisible`.
  - Keyboard avoidance is generally a no-op on desktop Windows; verify and document.
  - Modal handling: hide on `ContentDialog` / modal page show; restore on dismissal. Confirm exact stock Shell behaviour and match it.
  - Populate `FsTabTransitionContext` and await the animator on tab change.

### C. Tab transition animator

- [ ] **First-party reference animator.** Even though the spec ships no default, having one usable example (e.g. cross-fade) in the sample app drives out the `FsTabTransitionContext` shape end-to-end.
- [ ] **Cancellation contract test.** Verify that rapidly tapping between tabs cancels the in-flight animation cleanly on every platform.
- [ ] **Failure / fallback behaviour.** Confirm the spec's "if the animator throws or the cancellation token is signalled, falls back to instant swap and logs the failure at warning level" actually happens at the renderer-driven swap point, not just inside `FsShell.RunTransitionAsync`.

### D. Test surface

- [ ] **LSP test suite.** Add `FlagstoneUI.Core.Tests` coverage that exercises every public method, property, and event on `Shell` against an `FsShell` instance, asserting equivalent behaviour. Mirrors the existing per-control LSP testing pattern.
- [ ] **Cross-platform unit tests** for `FsTabContext` projection, items diff, `SelectedRoute` ↔ `Navigated` round-trip, custom-bar attach/detach, animator invocation/cancellation.
- [ ] **Bar contract tests.** Verify both implementation paths for the bar replacement contract: implementing `IFsTabBar` directly, and the convention-based fallback (bindable property `ItemsSource` + `SelectedRoute` or `ItemSelected` event).
- [ ] **Per-platform smoke tests.** Manual or automated UI test that exercises tab selection, bar visibility toggling, keyboard show/hide, modal presentation, and safe-area on each platform. Defer automation if necessary; manual checklist is acceptable for V1 if recorded.

### E. Sample app — definition of done

V1 is not shippable until the sample app demonstrates `FsShell` end-to-end. The sample app is the proof that consumers can do what the spec promises without writing platform code.

- [ ] **Migrate `samples/FlagstoneUI.SampleApp` from `Shell` to `FsShell`.** Single find-and-replace per shell file; nothing else changes. Confirms drop-in compatibility.
- [ ] **Default tab bar demo.** A page that uses `FsShell` with no `TabBarItemTemplate` set, showing the library default looks reasonable on every platform.
- [ ] **Custom `TabBarItemTemplate` demo.** A page (or a separate sample shell) demonstrating a meaningfully different tab look — e.g. pill-shaped selected background, action button not bound to a route, custom selected colour driven by `FsTabContext.IsSelected`.
- [ ] **Custom bar replacement demo.** A page demonstrating `FsShell.TabBar` set to a consumer-authored `ContentView` that implements `IFsTabBar`. Show that this is < 50 lines including XAML.
- [ ] **Animator demo.** A page using the reference cross-fade (or equivalent) `ITabTransitionAnimator`.
- [ ] **Visibility demos.** Pages that exercise `Shell.SetTabBarIsVisible(page, false)`, keyboard avoidance, and modal presentation, so the polish behaviours are visible to anyone running the sample.

### F. Documentation

- [ ] **`docs/controls/FsShell.md`.** Public-facing control doc following the pattern of `docs/controls/FsButton.md` etc. Describes the API, the two extension layers, and the migration story.
- [ ] **Update `docs/getting-started/quickstart.md`** to mention `FsShell` and the `UseFlagstoneUI()` requirement.
- [ ] **Update `AGENTS.MD` and `.github/copilot-instructions.md`** with `FsShell` as an available control.
- [ ] **Move this spec out of `docs/archive/`.** Currently lives in archive; promote to `docs/specs/` or fold the relevant parts into the control doc once V1 ships. Keep the ADR ([adr012-fsshell.md](../decisions/adr012-fsshell.md)) where it is.

### G. Loose ends / nice-to-haves (defer if needed)

- [ ] **Diagnostics.** Replace `System.Diagnostics.Debug.WriteLine` calls in `FsShell` with proper MAUI logger usage.
- [ ] **`FlagstoneUIBuilder` cleanup.** The `UseDefaultTheme()` no-op method is a leftover from the original builder pattern. Either remove it or make it do something. Out of scope for this spec but adjacent.
- [ ] **NuGet packaging.** Confirm `FsShell` types are exported from the `FlagstoneUI.Core` NuGet package and that the `UseFlagstoneUI()` extension is discoverable.

