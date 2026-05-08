# ADR: FsShell — Stylable Shell Chrome via Subclass

**Status:** Accepted
**Date:** 2026-05-07
**Deciders:** Matt Goldman
**Scope:** FlagstoneUI.Core — V1 inclusion

## Context

`Shell` is the default navigation paradigm in the .NET MAUI templates and the recommended starting point in most MAUI documentation. It bundles two concerns into one API:

1. A routing and navigation graph (`ShellItem` → `ShellSection` → `ShellContent`, URI routing, `GoToAsync`, lifecycle).
2. A renderer-driven chrome (tab bar, flyout, navigation bar, search handler).

The first concern is genuinely valuable. The second is the source of recurring product-polish complaints: the tab bar and other chrome are rendered by `ShellRenderer` and its platform subordinates (`ShellItemRenderer`, `ShellSectionRenderer`, `ShellTabBarAppearanceTracker` and equivalents on Android), with no clean cross-platform extension point. Visual properties that a designer would reasonably specify — custom shapes, indented selected backgrounds, action buttons not bound to a route, transition animations — are unreachable without per-platform renderer subclasses.

This is the same category of gap FlagstoneUI exists to close for `Entry`, `Editor`, and similar leaf controls. With `FsButton`, `FsEntry`, `FsEditor`, and `FsCard` shipped, Shell is the remaining sharp edge that materially limits what consumers can build in shared code. Closing it is the threshold for a credible V1.

Notably, `Shell` is one of the few areas where MAUI did not migrate from the Xamarin.Forms renderer model to the handler model; the platform implementation lives in `Microsoft.Maui.Controls.Platform.Compatibility`. References to "renderers" in this document are deliberate and refer to the actual `ShellRenderer` types, not handler terminology used loosely.

## Decision

Ship `FsShell` as a subclass of `Shell` in V1, with two layers of customisation accessed through one extension point:

1. **Default tab bar with a custom item template.** The reference `FsTabBar` ships with the core, accepts a `DataTemplate`, and renders one instance per `ShellContent`. This is the path for the 90% case.
2. **Replace the tab bar entirely.** A consumer can supply their own `ContentView` (including a subclassed or composed `FsTabBar`) as the bar, with a documented contract for receiving items and reporting selection.

The platform-level work required to suppress Shell's native chrome and host the FlagstoneUI bar lives inside the library, behind the `FsShell` type. Consumers never write platform-conditional code, never register renderers, and never see the word "renderer" in any documented path.

## Decision drivers

**Consistency with the rest of FlagstoneUI.** `FsButton` is a `Button` subclass. The borderless entry/editor primitives that back `FsEntry` and `FsEditor` are subclasses of their stock counterparts. Subclassing is the established pattern for raising visual control to the cross-platform layer when LSP can be preserved, and is the pattern consumers already understand from the existing controls.

**LSP guarantee preserved.** `FsShell : Shell` keeps `Shell.Current` casts working, keeps `GoToAsync` working, keeps virtual hooks like `OnNavigating` and `OnNavigated` overridable, and keeps the existing visual-tree element types (`ShellItem`, `ShellSection`, `ShellContent`, `Tab`, `TabBar`, `FlyoutItem`) usable as children unchanged. A consumer migrating from `Shell` to `FsShell` performs a single find-and-replace per file with no further code changes required.

**No consumer-facing platform code.** FlagstoneUI's stated principle is that developers do not have to know about or worry about handlers, renderers, or platform concerns. The internal implementation will register and use custom `ShellRenderer` subclasses on iOS, Android, and MacCatalyst, but this happens inside the library bootstrap and is invisible to consumers. Consumers write XAML; the library handles platforms.

**XAML-discoverable API surface.** Defining the tab template as a property of `FsShell` (`FsShell.TabBarItemTemplate`) means it appears in IntelliSense, lives next to the rest of the consumer's Shell declaration, and follows the same XAML idioms as `FlyoutItemTemplate`, `MenuItemTemplate`, and other Shell-native templates. A registration-based or resource-key-based alternative would work but feels less native to MAUI and is harder to discover.

**Composability of the two layers.** Because the reference `FsTabBar` is itself a `ContentView`, a consumer who has outgrown the template approach can still drop `FsTabBar` into the bar-replacement slot and wrap or extend it. The two layers are not separate APIs; they are one extension point exercised at different depths.

**Animation hook as a deliberate first-class concept.** Tab transitions are a visible polish gap relative to native apps. An `ITabTransitionAnimator` (or equivalent, see Spec) extension point is exposed in V1 even without a default animation implementation, on the basis that it is much cheaper to design in now than retrofit later, and the same hook generalises to bar-show/bar-hide behaviour for keyboard and modal interactions in future versions.

## Alternatives considered

**Custom `ShellRenderer` registration only, no `Shell` subclass.** Considered because it would allow FlagstoneUI's bar to apply to any standard `Shell` without source changes — the "drop the package in and existing apps get the bar" pitch. Rejected because:

- The consumer needs *somewhere* to declare what the tab template should look like. Without a subclass to host the bindable property, the template has to live in a resource dictionary keyed by convention, an attached property, or a service registration. All three are clunkier than a XAML-discoverable property and inconsistent with how the rest of FlagstoneUI is configured.
- The renderer registration itself would still need to be wired up via `MauiProgram.cs`, which crosses the FlagstoneUI principle of keeping platform concerns out of consumer code. The existing `.UseFlagstoneUI()` extension hides the registration call, but only the subclass approach gives a natural XAML home for the template.
- Consumers who have adopted `FsButton`, `FsEntry` etc. have already accepted the find-and-replace migration model. Extending it to `FsShell` is consistent rather than additional friction.

**Hide chrome via `Shell.TabBarIsVisible="False"` and overlay a MAUI bar inside the page area.** Considered as a v1 simplification. Rejected because:

- The overlay approach has known rough edges with safe areas, keyboard avoidance, modal presentation, and `NavigationPage` push animations. These are exactly the polish-level details that consumers turning to FlagstoneUI care most about; shipping a deliberately less-polished implementation would undermine the library's positioning.


**Exposing additional Fs-specific child element types (e.g. `FsShellContent`).** Rejected unconditionally. `FsShell` accepts the existing Shell visual-tree elements unchanged. Title and icon metadata are read from the existing `Shell.Title` and `Shell.Icon` attached properties, not from a parallel hierarchy. This preserves drop-in compatibility and keeps the migration story to a single `Shell` → `FsShell` change.

**Coupling `FsShell` to a navigation framework or service.** Rejected. FlagstoneUI is a styling library that happens to need to address Shell's chrome, not a navigation library. `FsShell` depends only on stock MAUI Shell. Consumers using third-party navigation libraries on top of Shell are unaffected.

## Consequences

### Positive

- Closes the last major customisation gap in MAUI for shared-code styling, completing the V1 thesis.
- Consistent mental model with the rest of FlagstoneUI: `Fs[ControlName]` is a drop-in for `[ControlName]` with full visual control from XAML.
- Consumers gain access to a category of polish (custom tab shapes, action buttons not bound to routes, indented backgrounds, transition animations) currently unreachable without per-platform renderer code.
- Establishes a pattern (subclass + internal renderer work) that can be reused if any other Shell-region chrome customisation is added later (flyout, nav bar).

### Negative / accepted

- `FsShell` carries more platform-specific implementation code than any other FlagstoneUI control. This is contained behind the type boundary and does not leak into shared abstractions, but it changes the project's overall ratio of shared to platform code. Acceptable: this is exactly the kind of work FlagstoneUI exists to absorb on consumers' behalf.
- Consumers using stock `Shell` do not get the FlagstoneUI bar without source changes. Accepted as consistent with the rest of the library; the find-and-replace migration is a single-character difference per file.
- Some Shell scenarios are out of scope for V1 and will need follow-up work: flyout chrome customisation, top-tab visual customisation, navigation-bar customisation. Tracked separately; their absence does not block FsShell V1 delivery.

### Operational

- The internal renderer code must be carefully isolated. Specifically: it lives strictly inside the `FsShell` implementation under `Platforms/iOS/`, `Platforms/Android/`, and `Platforms/MacCatalyst/`. It is not consumed by any other FlagstoneUI control, exposed in any public API, or referenced in any documented consumer path.
- Consumer-facing docs describe `FsShell` purely in cross-platform terms. The word "renderer" does not appear in any quickstart, sample, or template path. Advanced docs may acknowledge the internal mechanism for contributors and curious users, but the recommended path is XAML only.
- A test surface that exercises `Shell`'s public API against `FsShell` is required to enforce the LSP guarantee. This extends the existing per-control LSP testing approach rather than introducing a new pattern.

## Platform scope

V1 supports every platform on which MAUI Shell itself runs: iOS, Android, MacCatalyst, and Windows. FlagstoneUI's role is to serve the MAUI surface as it exists, not to editorialise about which platform configurations are advisable. A consumer who chooses to use Shell on Windows is making a decision FlagstoneUI does not second-guess; FsShell renders the consumer's tab template as a bottom bar on Windows in the same way it does on the mobile platforms. Whether bottom tabs are idiomatic for Windows is a structural decision the consumer made by writing them; FsShell honours that intent.

The Windows implementation requires its own renderer work (Shell on Windows uses a different platform path from iOS/Android/MacCatalyst — `NavigationView` from WinUI rather than `UITabBarController` or `BottomNavigationView`). This is recognised as additional implementation cost but does not change the V1 scope.

## Out of scope for V1

- Flyout chrome replacement.
- Navigation bar (top toolbar) replacement.
- Top-tab strip replacement (where multiple `ShellContent` are nested in a `Tab`).
- Search handler chrome replacement.
- Default animation implementations beyond the extension point itself.

These are explicitly deferred, not rejected. The V1 scope is the bottom tab bar across all platforms Shell supports, with an animation extension point reserved for future use.

## References

- FlagstoneUI principle: raise platform-level customisations up to the cross-platform layer, allowing developers to fully visually style their apps across all platforms using a single unified codebase.
- Existing FlagstoneUI control patterns: `FsButton` (subclass), `BorderlessEntry` / `FsEntry` (subclass + wrapper), `BorderlessEditor` / `FsEditor` (subclass + wrapper), `FsCard` (standalone).

