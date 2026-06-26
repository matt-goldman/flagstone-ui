# ADR Addendum: FsShell — Renderer Scope Narrowing

**note: this is an addendum to [ADR012 — FsShell: Stylable Shell Chrome via Subclass](./adr012-fsshell.md). It refines what the per-platform renderers do (and, importantly, what they no longer do) without changing the subclass approach, the LSP guarantee, or the "no consumer-facing platform code" principle, all of which stand unchanged. It is a co-decision with [ADR012_3 — Bottom Chrome Height Resource Contract](./adr012_3-fsshell-bottom-chrome-resource-contract-addendum.md), and the two should be read together.**

**Status:** Accepted
**Date:** 2026-06-12
**Deciders:** Matt Goldman
**Scope:** FlagstoneUI.Core — V1, iOS renderer primarily; Android renderer confirmed unaffected; MacCatalyst/Windows follow the same scope

## Context

ADR012 charged the `FsShell` renderers with two responsibilities, expressed loosely: "suppress Shell's native chrome" and "host the FlagstoneUI bar in its place." Neither was bounded explicitly. As the iOS renderer matured, a third responsibility accreted without being articulated: **reserve page-content space so the page doesn't render beneath the bar.**

On Android the third responsibility is trivial and falls out of the platform layout model — the renderer adds the bar as a sibling of the navigation container inside an outer `LinearLayout`, the linear stack reserves the bar's slice naturally, and `WindowInsetsCompat` handles the gesture/navigation inset. There was nothing to invent.

On iOS there is no equivalent stacking primitive. `UITabBarController` makes its children fill the entire view; with the native `UITabBar` hidden, child VC views extend to the screen edge. The iOS renderer tried successively:

1. Setting `AdditionalSafeAreaInsets` on the `UITabBarController` itself.
2. Setting `AdditionalSafeAreaInsets` on every child `UINavigationController`.
3. Setting `AdditionalSafeAreaInsets` on every pushed page view controller as well.
4. Manually shrinking each child VC's `view.Frame` by the bar's height.
5. Combinations of the above.

In every attempt the MAUI iOS page handler — which renders the `ContentPage` inside Shell's compatibility renderer — failed to reflect the additional inset in its laid-out content. Pages remained at their stock size and content scrolled under the bar. Forcing the issue by mutating frames worked for one layout pass before UIKit reverted them on the next.

The accumulated logic was also unstable in other ways. Recomputing the bar's `IView.Measure` on every layout pass produced oscillating results (35.33 → 240 → 35.33 → ...) because MAUI's iOS handler treats prior `Arrange`d sizes as a growth hint. Caching the first measure collapsed children to height 0 on subsequent `Arrange` calls. Both behaviours were knock-on effects of the renderer trying to be more than a host.

The pattern became clear: the renderer was carrying responsibilities that belong to the consumer's page layout, and the platform pipeline kept punishing the attempt. The "we give you a surface, you build on it" framing that justifies the bar's existence applies equally to page-content insets. The renderer should not be solving that problem.

## Decision

The `FsShell` platform renderers have an explicitly bounded scope. They do **exactly four things** and no more:

1. **Suppress** the native bottom chrome (`UITabBar` on iOS, `BottomNavigationView` on Android, equivalents on MacCatalyst/Windows).
2. **Host** the user's `ContentView` from `FsShell.TabBar` as a subview/sibling of the platform shell-item view, with the bar's natural measured size.
3. **Pin** the hosted bar to the bottom edge of the view (including the safe-area cap on iOS so the bar's background extends behind the home indicator).
4. **Slide** the bar out of the way on soft-keyboard show, restoring it on hide, when `FsShell.HideTabBarOnKeyboard` is true.

The renderers do **not**:

- Reserve page-content space.
- Set or propagate `AdditionalSafeAreaInsets` on any view controller.
- Manipulate child view-controller frames.
- Apply `WindowInsetsCompat`-style padding to the navigation area (Android continues to apply the gesture-inset bottom padding to the bar itself, not to the navigation area).
- Wrap or restructure the Shell content hierarchy.

Page-content insets are the consumer's responsibility, addressed via the `DynamicResource` contract decided in ADR012_3. The renderer is a host, not a layout engine.

## Cross-platform invariant

Every platform renderer must keep its scope to the four responsibilities above. Any new platform implementation (Windows, future targets) inherits the same scope.

Specifically:

- **iOS** (`FsShellItemRenderer`): suppress, host as `UIView` subview with `TranslatesAutoresizingMaskIntoConstraints = true`, pin via `Frame = (0, view.bounds.height - chromeHeight, width, chromeHeight)` (where `chromeHeight = measuredContentHeight + view.safeAreaInsets.bottom`), bring to front on each layout pass, slide on keyboard.
- **Android** (`FsShellItemRenderer`): suppress, host as a sibling inside the outer `LinearLayout` with `MatchParent × WrapContent`, the linear stack handles pinning, apply `WindowInsetsCompat` bottom padding to the bar's platform view only.
- **MacCatalyst** / **Windows**: must implement the same four responsibilities using their respective platform primitives.

The renderer's `Dispose`/`OnDestroyView` releases the bar without removing it from a re-hosting renderer's view — the single-shared-bar lifecycle in ADR012_1 is unaffected.

## Decision drivers

**Composability over magic.** The bar is a user-authored `ContentView`. Page padding is a property on a user-authored `ContentPage`. Both belong to the consumer's layout. A renderer that silently reaches into the consumer's pages to inject insets violates the separation FlagstoneUI is supposed to provide.

**The MAUI iOS page handler does not reliably honour `AdditionalSafeAreaInsets` set programmatically through Shell's compatibility renderer chain.** Every variant tried failed in some realistic configuration. Continuing to attempt it produced complex platform code that didn't work; backing off produced ~140 lines of straightforward host code that does.

**The renderer's instability resolves once it stops measuring more than it needs to.** Measuring the bar once on first layout (with a bounded constraint that covers any realistic tab-bar height) and reusing the cached value avoids the measure-arrange-measure oscillation and the children-collapse-to-zero behaviour. Holding a single measurement is appropriate because the bar is a host-once, position-many concern; the cross-platform layer handles re-measuring when content changes.

**The pattern generalises to any chrome shape.** A renderer that hosts a `ContentView` and pins it to the bottom works the same whether that view is a tab bar, a centre-FAB cluster, a side rail mounted bottom-left, or a radial menu. Page-padding is then driven by whatever resource the chrome publishes (see ADR012_3). The renderer doesn't care about the chrome's shape, the consumer doesn't write platform code, and adding a new chrome pattern requires zero renderer changes.

**Symmetry with how Android already worked.** The Android renderer never grew the third responsibility because its platform layout makes the question trivial. Bringing iOS's scope down to match Android's is a simplification that aligns the two and makes the contract uniform — both renderers are pure hosts.

## Alternatives considered

**Keep trying to make `AdditionalSafeAreaInsets` propagate.** Rejected. Every layered variant (parent only, parent + nav controllers, parent + nav controllers + pages, with and without `SetNeedsLayout`) produced the same outcome: MAUI's page handler ignored the inset and rendered the page at its stock size. The next step would have been forking the Shell compatibility renderer or the page handler, neither of which is in scope for a styling library.

**Shrink the active child VC view's frame on each layout pass.** Rejected. UIKit reverts the frame on its subsequent layout pass, and racing it requires intercepting `viewWillLayoutSubviews` on the navigation controller — an invasive change that would need to be re-applied to whatever VC type Shell uses internally on each platform. Even when temporarily successful, the page layout inside the navigation controller didn't reliably reflect the smaller frame because the page measured against the navigation controller's `safeAreaLayoutGuide`, which doesn't track manual frame changes.

**Wrap the entire Shell view hierarchy in a custom container that does the layout.** Rejected. This is essentially forking `ShellRenderer`, which crosses the LSP guarantee in ADR012 (`Shell.Current` continuing to work, `GoToAsync` continuing to work, flyout drawer behaviour continuing to work all rest on staying inside Shell's renderer chain) and changes the project's complexity profile dramatically.

**Ship an `FsContentPage` base class with a built-in `ControlTemplate` that reserves bottom padding automatically.** Considered, but rejected as the sole mechanism. A base class is more invasive than necessary (forces inheritance, doesn't compose with other base-class needs the consumer may have) and prescribes a layout. The attached property decided in ADR012_3 is opt-in per page without requiring inheritance, and an `FsContentPage` (or a `ControlTemplate` resource) can be added later as a convenience layer on top of the attached property without conflicting with it.

**Have the renderer write a per-platform resource into a consumer-observable property on `FsShell`.** Considered. Rejected because it moves consumer XAML further from the natural MAUI idiom (`{DynamicResource ...}` is already reactive; an FsShell-specific property would need its own change-notification path and offers no benefit over the resource pattern).

## Consequences

### Positive

- The iOS renderer collapses to ~290 lines from the ~420-line accreted version. Cognitive overhead per platform drops sharply.
- Renderer responsibilities are uniform across iOS and Android: both are pure hosts. New platforms inherit a clear, narrow contract.
- The fragile measure/arrange logic disappears. The bar is measured once with a bounded constraint and reused; oscillation and child-collapse bugs are gone.
- The "we host the surface, you build it" framing now applies all the way down: bar shape is the consumer's choice, page padding is the consumer's choice, renderer makes neither.
- Custom chrome shapes (centre-FAB, side rail, radial menu) cost the renderer nothing additional — they're all just `ContentView` instances hosted in the same way.

### Negative / accepted

- Pages don't get automatic bottom padding. A consumer who does nothing will have their page content scroll under the bar. This is mitigated by the attached property in ADR012_3 and by documentation, but it is a genuine ergonomic step down from "magic just works." Accepted: the magic only ever worked on Android, and forcing it on iOS produced worse results than asking the consumer to opt in.
- The iOS renderer must also write the bar's effective chrome height to the `DynamicResource` directly, since MAUI's `bar.Height` reports only the content height (not the safe-area cap added at the platform level). This is a small targeted write at the end of `LayoutBar`, not a re-entry into the inset-management problem.

### Operational

- `FsShellItemRenderer.iOS` no longer needs the `_appliedInset`, `ApplyBarInset`, `ApplyInsetToChildren`, or `OnBarPropertyChanged` (`IsVisible`-tracking) members. They are deleted, not preserved as private no-ops.
- The keyboard handlers no longer touch `AdditionalSafeAreaInsets`; they only translate the bar's transform.
- Documentation in `docs/controls/FsShell.md` makes the renderer's bounded scope and the consumer's reserve-space responsibility explicit, with the `DynamicResource` pattern alongside.

## Verification

Validated on iOS Simulator (iPhone 17 Pro, iOS 26.5) with the sample app's `ControlsShowcasePage` opted into `FsLayout.BottomChromePadding="{DynamicResource FsBottomChromeHeight}"`. The bar renders with a custom item template, sits at the bottom with its background extending behind the home indicator, and the page content stops at the bar's top edge. Switching between tabs preserves padding; navigating with `Shell.SetTabBarIsVisible="False"` causes the resource to drop to 0 and the page reclaims the bottom space. Android behaviour is unchanged from before the narrowing.

## Relationship to other ADRs

- **Refines** ADR012 by bounding the renderer's responsibilities explicitly.
- **Preserves unchanged** the subclass approach, the LSP/drop-in guarantee, the no-consumer-platform-code principle, the per-`ShellItem` bar scoping (ADR012_1), the XAML-discoverable `TabBarItemTemplate` / `TabBar` surface, and the deferral of flyout-chrome and top-tab-strip replacement.
- **Co-decision with** [ADR012_3 — Bottom Chrome Height Resource Contract](./adr012_3-fsshell-bottom-chrome-resource-contract-addendum.md), which defines how pages reserve space now that the renderer doesn't.

## References

- [ADR012 — FsShell: Stylable Shell Chrome via Subclass](./adr012-fsshell.md)
- [ADR012_1 — Per-`ShellItem` Tab Bar Scoping](./adr012_1-fsshell-per-item-bar-scoping-addendum.md)
- [ADR012_3 — Bottom Chrome Height Resource Contract](./adr012_3-fsshell-bottom-chrome-resource-contract-addendum.md)
- [`FsShellRenderer.cs` (iOS)](../../src/FlagstoneUI.Core/Platforms/iOS/Controls/FsShellRenderer.cs)
- [`FsShellRenderer.cs` (Android)](../../src/FlagstoneUI.Core/Platforms/Android/Controls/FsShellRenderer.cs)
