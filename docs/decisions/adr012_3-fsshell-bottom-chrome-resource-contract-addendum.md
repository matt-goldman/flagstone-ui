# ADR Addendum: FsShell — Bottom Chrome Height Resource Contract

**note: this is an addendum to [ADR012 — FsShell: Stylable Shell Chrome via Subclass](./adr012-fsshell.md). It defines how pages reserve space for the hosted bar now that the renderers no longer do so. It is a co-decision with [ADR012_2 — FsShell Renderer Scope Narrowing](./adr012_2-fsshell-renderer-scope-narrowing-addendum.md), and the two should be read together. It does not change the subclass approach, the LSP guarantee, or the "no consumer-facing platform code" principle.**

**Status:** Accepted
**Date:** 2026-06-12
**Deciders:** Matt Goldman
**Scope:** FlagstoneUI.Core — V1, cross-platform; iOS renderer participates as a publisher

## Context

ADR012_2 narrowed the `FsShell` renderers to host-only. They suppress the native chrome and pin a `ContentView` to the bottom edge; they do not reserve space inside page content. Pages whose content extends to the bottom of the screen will, by default, render under the hosted bar.

Pages therefore need a mechanism to know how much room to leave. The mechanism has to satisfy a number of constraints simultaneously:

- **Reactive.** The bar's height is not constant. It changes with the item template (the default icon+label produces ~60 DIPs; a compact horizontal template produces ~36 DIPs; a centre-FAB design might produce 80+). It also changes with safe-area context (the home indicator on devices with one), with theming, with orientation, and with `Shell.SetTabBarIsVisible` toggling per page. A static "tell me once at startup" value doesn't work.
- **Cross-platform.** Whatever the consumer writes in their page should work the same on iOS, Android, MacCatalyst, and Windows without `OnPlatform` switches or `#if` guards.
- **XAML-friendly.** The library's stated principle is that consumers configure FlagstoneUI from XAML. A C#-only API (event subscription, manual `OnSizeAllocated` hooks) doesn't meet that bar for the common case.
- **Decoupled.** Pages should not need a back-reference to the FsShell instance, nor know what type the bar is. Multiple chrome shapes — a tab bar, plus a side rail, plus a top app bar — should compose without each one having to teach the others its dimensions.
- **General.** The pattern should apply to any chrome the consumer mounts, not just a bottom tab bar. A side rail should be able to publish its width; a top app bar should publish its height; a FAB cluster might publish nothing at all.

MAUI already provides a primitive that satisfies all five constraints: the `DynamicResource` markup extension. A resource written into `Application.Current.Resources` is observable by any XAML binding via `{DynamicResource Key}` and updates reactively when the resource value changes. There's no additional change-notification plumbing to design and no platform-specific code.

## Decision

`FsShell` publishes the currently hosted bar's effective height into `Application.Current.Resources` under a documented key, **`"FsBottomChromeHeight"`**, exposed as `FsShell.BottomChromeHeightResourceKey`. The value is a `double` in DIPs.

Pages reserve space for the bar by binding to that resource. The library provides a small, opt-in attached property as the recommended consumption path; direct consumption from XAML or code-behind is also supported and documented.

The same pattern is the contract for any chrome that wants to participate. Custom chrome (side rails, top app bars, FABs, radial menus) publishes its own dimensions to its own resource key; pages consume each via the same `{DynamicResource}` mechanism.

### What is published

The resource value is:

- The bar's measured cross-platform `Height` (DIPs), plus
- On iOS, the device's bottom safe-area inset (so the published value matches the bar's actual visual footprint, including the home-indicator area). The iOS renderer writes its own value into the same key, overriding the cross-platform publication where applicable. See ADR012_2 for why this lives in the renderer.

The value is **0** when:

- No bar is hosted (`FsShell.TabBar` is null).
- The bar's `IsVisible` is false (the active item has fewer than two sections, or the current page sets `Shell.SetTabBarIsVisible="False"`).

### When it is published

`FsShell` writes the resource:

- When a bar is attached (so the resource exists at a defined value as soon as the shell is constructed).
- When the bar's `SizeChanged` fires (after MAUI's layout cascade).
- When the bar's `IsVisible` `PropertyChanged` fires.
- When a bar is replaced or detached (back to 0).

The iOS renderer additionally writes after each `ViewDidLayoutSubviews` pass with the safe-area-inclusive value.

### Consumption

The recommended path is the `FsLayout.BottomChromePadding` attached property:

```xaml
<ContentPage
    xmlns:fs="clr-namespace:FlagstoneUI.Core.Controls;assembly=FlagstoneUI.Core"
    fs:FsLayout.BottomChromePadding="{DynamicResource FsBottomChromeHeight}">
    <!-- content -->
</ContentPage>
```

The attached property reflects its `double` value into the target page's `Padding.Bottom`, leaving the other edges untouched. It is opt-in per page and composes with any other Padding the page sets declaratively.

Direct consumption is also supported for cases where `Padding.Bottom` isn't the right reservation — for example, a row in a `Grid` whose `HeightRequest` should match the chrome:

```xaml
<Grid RowDefinitions="*, Auto">
    <ScrollView Grid.Row="0">...</ScrollView>
    <BoxView Grid.Row="1"
             HeightRequest="{DynamicResource FsBottomChromeHeight}"
             Color="Transparent" />
</Grid>
```

Code-behind consumption:

```csharp
if (Application.Current?.Resources.TryGetValue(
        FsShell.BottomChromeHeightResourceKey,
        out var raw) is true && raw is double height)
{
    // use height
}
```

### Generalisation

The same contract is the pattern for any chrome shape. A side rail publishes a width:

```csharp
public class MySideRail : ContentView, IFsTabBar
{
    public const string WidthResourceKey = "FsRightChromeWidth";

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        if (Application.Current is { } app)
        {
            app.Resources[WidthResourceKey] = IsVisible ? width : 0;
        }
    }
}
```

A page reserves room for it via the same idiom (with an analogous attached property the consumer can add, or via direct `Padding.Right` binding).

## Decision drivers

**`{DynamicResource}` is already the reactive primitive we need.** MAUI's markup extension tracks resource changes and re-evaluates bindings automatically. Building a parallel change-notification mechanism (events, observable properties, `INotifyPropertyChanged` on FsShell) would duplicate functionality that XAML already understands.

**It decouples publishers from consumers.** Pages don't reference FsShell. The bar doesn't reference pages. Resources are the rendezvous point. A page can be moved between shells, instantiated standalone in a test harness, or hosted by a future Shell-alike, and its chrome-padding declaration still works as long as the resource exists.

**It generalises trivially to multiple chrome shapes.** A consumer who wants a bottom tab bar and a top app bar publishes to two keys. A side rail adds a third. Pages bind to whichever they need. The pattern doesn't grow more complex as more chrome is added; it just has more keys.

**The opt-in attached property keeps the simple case one attribute.** `fs:FsLayout.BottomChromePadding="{DynamicResource FsBottomChromeHeight}"` is the shortest possible expression of "leave room for the bottom bar." It is a single XAML attribute, requires no base class, doesn't conflict with other Padding the page sets, and composes with arbitrary other layout. Consumers who want different behaviour bypass the attached property and bind the resource somewhere else.

**It maps onto the consumer's existing mental model.** XAML developers already use `{DynamicResource}` for colours, fonts, dimensions, styles. Adding "chrome heights" to that list extends a pattern they know rather than introducing a new one.

**The iOS renderer override fits cleanly.** ADR012_2 establishes that the renderer no longer manages page insets, but the bar's safe-area footprint on iOS is genuinely part of its effective height — it's a fact about the device, not a layout decision. Letting the renderer write the safe-area-inclusive value to the same key keeps the contract a single number that means "this is what your page should reserve," rather than a content-only number that consumers have to combine with platform-specific safe-area math.

## Alternatives considered

**A bindable property on `FsShell` itself (e.g. `BottomChromeHeight`) that pages bind to.** Considered. Rejected because it forces the page to know about FsShell — a binding source either has to be `RelativeSource AncestorType={x:Type fs:FsShell}` (which is verbose and only works inside a Shell-hosted page) or has to be reached through an x:Reference, both of which are clunkier than `{DynamicResource ...}` and don't generalise to multiple chrome shapes the way resource keys do.

**A static event on `FsShell` that consumers subscribe to in code-behind.** Rejected. It's a code-behind-only mechanism, which violates the "XAML-first" principle. It also doesn't compose with bindings (the consumer has to set Padding manually), and the subscription lifetime needs to be managed across page navigations.

**Auto-padding all pages via an `FsContentPage` base class or a default implicit ControlTemplate.** Considered as a convenience layer; rejected as the sole mechanism. A base class forces inheritance, and an implicit `ContentPage` style with a built-in `ControlTemplate` would silently override consumer Padding everywhere. Both options can be added later as opt-in conveniences on top of the resource contract, but neither is the contract itself.

**Have the renderer mutate `Page.Padding` on the active page directly.** Rejected. The renderer mutating cross-platform consumer-authored state is exactly what ADR012_2 says it shouldn't do, and it ties chrome consumption to FsShell rather than to the resource. It also fights consumer-declared Padding on the page.

**A managed `IBottomChrome` interface that the consumer implements to declare height.** Rejected as overengineered. The bar already publishes its size via standard MAUI `SizeChanged`; adding an interface just to expose a number that's already available is friction without benefit.

**Letting the cross-platform layer compute the iOS safe-area cap, so the renderer doesn't have to publish anything.** Considered. Rejected because MAUI doesn't expose the platform safe area cross-platform in a way that's reliably equivalent to `view.safeAreaInsets.bottom` at the FsShell layer. The renderer already knows the value; having it write the corrected number to the same key is more correct than trying to reconstruct it elsewhere.

## Consequences

### Positive

- A single XAML attribute (`fs:FsLayout.BottomChromePadding="{DynamicResource FsBottomChromeHeight}"`) gives a page correct bottom inset on every platform, with no consumer platform code.
- The pattern generalises: side rails, top app bars, and arbitrary chrome shapes participate by publishing to their own resource keys.
- Pages remain decoupled from FsShell. A page using the resource works in any shell that publishes it (or none — the resource simply defaults to 0).
- The reactive update path is MAUI-native — `{DynamicResource}` already works, no new mechanism to maintain.
- Custom-bar authors have a clear contract: publish your effective dimension to a resource on size changes and visibility changes.

### Negative / accepted

- Pages opt in explicitly. Forgetting the attribute means content scrolls under the bar. This is a real ergonomic step down from "magic just works" and we accept it for the reasons in ADR012_2 (the magic only ever worked on Android, and the explicit opt-in is one attribute).
- Two paths exist for writing the resource: cross-platform `FsShell` and iOS renderer override. The duality is necessary because MAUI's cross-platform `bar.Height` doesn't include the iOS safe-area cap. The iOS write happens after the cross-platform one in normal layout order, so the inclusive value wins; this is documented and exercised in the sample app.
- Tests that depend on the resource value need to account for which platform they run on (iOS will report a larger value than Android by the safe-area inset). This matches what consumers actually see and is preferable to publishing different "what MAUI thinks" and "what the screen actually shows" numbers.

### Operational

- `FsShell.BottomChromeHeightResourceKey` is a documented `public const string`. The literal `"FsBottomChromeHeight"` is what `{DynamicResource}` consumers reference; the constant exists for code-behind use and as the canonical source of the spelling.
- `FsLayout.BottomChromePaddingProperty` is the recommended consumption point. Documented in `docs/controls/FsShell.md`.
- The iOS renderer's resource write lives in `LayoutBar`, immediately after the bar's frame is set. It is the only platform-conditional touchpoint pages indirectly depend on, and it is one line.
- Custom chrome authors are encouraged to define their own `public const string` resource keys following the `FsXxxChromeYyy` convention (e.g. `FsRightChromeWidth`, `FsTopChromeHeight`) and document them alongside the chrome.

## Verification

Validated on iOS Simulator with the sample app's `ControlsShowcasePage` consuming the resource via `fs:FsLayout.BottomChromePadding="{DynamicResource FsBottomChromeHeight}"`. Page content stops at the bar's top edge (~805 DIPs on a 874-DIP iPhone 17 Pro view, corresponding to a 69.33-DIP chrome including the 34-DIP home-indicator inset). Toggling `Shell.SetTabBarIsVisible="False"` on a navigated-to page drops the resource to 0 and the page reclaims the full content area; navigating back restores the inset. Android behaviour is unchanged; the cross-platform `FsShell` publication of `bar.Height` is the only value written there and it already reflects the platform-padded bar size.

## Relationship to other ADRs

- **Refines** ADR012 by defining how pages reserve space for the hosted chrome.
- **Co-decision with** [ADR012_2 — FsShell Renderer Scope Narrowing](./adr012_2-fsshell-renderer-scope-narrowing-addendum.md), which establishes that the renderer doesn't do it.
- **Preserves unchanged** the subclass approach, the LSP/drop-in guarantee, the no-consumer-platform-code principle, per-`ShellItem` bar scoping (ADR012_1), and the XAML-discoverable `TabBarItemTemplate` / `TabBar` surface.

## References

- [ADR012 — FsShell: Stylable Shell Chrome via Subclass](./adr012-fsshell.md)
- [ADR012_1 — Per-`ShellItem` Tab Bar Scoping](./adr012_1-fsshell-per-item-bar-scoping-addendum.md)
- [ADR012_2 — FsShell Renderer Scope Narrowing](./adr012_2-fsshell-renderer-scope-narrowing-addendum.md)
- [`FsShell.cs`](../../src/FlagstoneUI.Core/Controls/Shell/FsShell.cs) — `BottomChromeHeightResourceKey`, `PublishBottomChromeHeight`
- [`FsLayout.cs`](../../src/FlagstoneUI.Core/Controls/Shell/FsLayout.cs) — `BottomChromePadding` attached property
- [`FsShellRenderer.cs` (iOS)](../../src/FlagstoneUI.Core/Platforms/iOS/Controls/FsShellRenderer.cs) — safe-area-inclusive override
- [`docs/controls/FsShell.md`](../controls/FsShell.md) — consumer documentation
