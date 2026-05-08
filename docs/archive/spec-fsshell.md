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

- **Top tabs (multiple `ShellContent` inside a `Tab`).** Currently out of scope for V1 visual replacement, but the underlying routing must continue to work. Verify behaviour when an `FsShell` contains a `Tab` with multiple `ShellContent` — probably falls through to stock Shell rendering for the top strip while the bottom bar uses FsTabBar. Acceptable for V1; flag in docs.
- **Selection model on the bar.** `SelectedRoute` (string) vs. `SelectedItem` (`FsTabContext`) vs. both. Leaning toward `SelectedRoute` as the primary, with `SelectedItem` as a derived convenience. Resolve before API freeze.
- **`IsSelected` propagation for templates.** `FsTabContext.IsSelected` is the canonical source. Whether to also expose visual states (`Selected`, `Unselected`, `Disabled`) via the standard MAUI `VisualStateManager` should be decided alongside the default item template — consumers familiar with VSM-driven styling will expect it.
