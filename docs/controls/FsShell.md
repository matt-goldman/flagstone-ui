# FsShell Control

FsShell is a `Shell` replacement that gives you full control over the visual appearance and behaviour of the tab bar. It's a `Shell` subclass and works as a drop-in replacement out of the box, meaning it fully preserves `Shell`'s routing, navigation, and lifecycle.

The _functionality_ is separated from the _presentation_, meaning you can provide a `ContentView` and completely replace (rather than just customise) the navigation experience altogether - a new kind of tab bar, FAB with radial menu, side rail, whatever you like.

FsShell includes a default tab bar which allows you to provide a `DataTemplate` for tab bar items (just like you can with menu items), which gives you styling and control over individual tab items (with full `VisualStateManager`) so you can customise tabs out of the box, without having to supply your own.

## Features

- **Shell sub-class:** drop-in `Shell` subclass — works wherever `Shell` works, works with existing `ShellItem` and `ShellContent`
- **Built-in customisable tab bar:** default `FsTabBar` with a `TabBarItemTemplate` for per-tab customisation
- **Replaceable TabBar:** full bar replacement via `TabBar` slot — use any `ContentView` for full control of the visual navigation experience
- **Routing and current-tab binding:** the `IFsTabBar` interface exposes `Shell` state, providing access to the underlying routing logic
- **Tab transitions:** optional tab-change animations via `ITabTransitionAnimator`
- **Keyboard control:** optional `HideTabBarOnKeyboard` lets you hide the navigation chrome when the keyboard is displayed
- **Docked or undocked bar:** `TabBarIsDocked` (default `true`) pins the bar to the bottom edge with renderer-managed safe-area/keyboard handling; set to `false` for a full-bounds overlay where the consumer controls placement via MAUI layout properties (e.g. a floating FAB)
- **Control tab bar page margin:** chrome dimension published as a `DynamicResource` for pages to consume (or ignore), with opt-in padding via the `FsLayout.BottomChromePadding` attached property

## Architecture: Hosted Chrome, Not Customised Chrome

As with all .NET MAUI controls, Shell is an abstraction over native controls (`UITabBar` on iOS, `BottomNavigationView` on Android, `NavigationView` on Windows); these controls can be customised, but require platform code. On iOS, Android, and Mac Catalyst, `Shell` uses the legacy renderer architecture, while on Windows it uses the handler architecture — either way, customisation requires a different paradigm to normal MAUI controls. This is the established approach for `Shell` tab bar customisation.

> **Note:** iOS, Android, Windows, and Mac Catalyst are all fully supported.

FsShell takes a different approach. The platform specific renderers suppress the native implementation completely rather than customise it; instead the tab bar is now a fully cross-platform control. The approach is closer to [Sharpnado Tabs](https://github.com/roubachof/Sharpnado.Tabs), which gives you absolute control over the tabs, but also introduces its own navigation paradigm. FsShell offers a combination of both - it hides the native chrome, hosting a `ContentView` in its place, but FsShell remains inside `Shell`, so routing, flyouts, navigation stack, lifecycle are all unchanged.

Note that Sharpnado Tabs is a much more complete implementation; out of the box FsShell gives more control over tab appearance than you get with `Shell`, and you have the freedom to fully customise to the extent that Sharpnado does (or beyond), but you have to provide it.

With the native tab bar, page layout is automatically adjusted to allow space for it, but this is decoupled with FsShell. The chrome dimension is published to a `DynamicResource` (`FsBottomChromeHeight`), which you can consume from page XAML; the recommended path is to add the `FsLayout.BottomChromePadding` attached property to your apps' `Page` style (see section on `FsLayout` below); you can also bind the resource into a `ControlTemplate`, a `Padding` directly, or any other layout target.

You could also ignore it (for example, if you want a blur effect on your tab bar, removing it allows page content to scroll behind it and be blurred). This approach gives you the most flexibility without relying on per-platform safe areas.

### Why not just use `Shell`?

`Shell` is a powerful .NET MAUI feature but it conflates two concerns: navigation _behaviour_ and navigation _presentation_. It is heavily opinionated on both, and that constraint is helpful but also problematic for apps with a strong design identity and custom navigation aesthetic.

Providing full customisation for the tab bar in Shell is not feasible; while more control surface could be exposed, many apps have design requirements that can't be anticipated. A common one is a central button that provides functionality rather than navigation; but accommodating that is a slippery slope.

Flagstone UI in general acknowledges that .NET MAUI already gives you powerful tools for presenting UI to the user; FsShell lets you use them.

## Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `TabBar` | `ContentView?` | auto-instantiated `FsTabBar` | The bar to host. Auto-set to a new `FsTabBar` if left null. |
| `TabBarIsDocked` | `bool` | `true` | When `true`, the renderer pins the bar to the bottom edge and handles safe-area/keyboard. When `false`, the bar is hosted as a full-bounds overlay with no renderer positioning — the consumer controls placement via MAUI layout properties (`HorizontalOptions`, `VerticalOptions`, `Margin`, etc.). |
| `TabBarItemTemplate` | `DataTemplate?` | `null` | Template applied to each item in the default `FsTabBar`. Ignored if `TabBar` is replaced with a non-`FsTabBar`. |
| `TabTransitionAnimator` | `ITabTransitionAnimator?` | `null` | Invoked on tab selection changes to drive a transition. |
| `HideTabBarOnKeyboard` | `bool` | `true` | When true, slides the bar off-screen while the soft keyboard is open. |
| `Tabs` | `IReadOnlyList<FsTabContext>` | live | Live projection of the active item's sections; bound to the bar's `ItemsSource`. |

## Companion Types

### `IFsTabBar`

The `IFsTabBar` interface makes navigation and routing state and events available to your custom tab bar (or other navigation UI).

It provides the entry point to `Shell`'s navigation infrastructure by exposing the following items:
- `ItemsSource`: of type `IReadOnlyList<FsTabContext>`. Provides a live projection of the current navigation hierarchy and state.
- `SelectedRoute`: a `string`. FsShell sets it on the bar to mirror external navigation (e.g. `GoToAsync`), and the bar sets it when the user taps a tab (alongside raising `ItemSelected`, which is what FsShell actually listens for).
- `ItemSelected`: an event handler that exposes the selected `FsTabContext` when tab selection changes.

### `FsTabBar` (default implementation)

FsShell includes FsTabBar, intended as a reference. It implements the `IFsTabBar` interface, allowing it to participate in `Shell` navigation, but is otherwise a vanilla `ContentView`. It's a simple `Grid`-driven `BindableLayout`.

It is however usable in your apps out of the box; the following provide enhancements over stock `Shell` while also serving as useful references:

- per-item template via `ItemTemplate` (bound to `FsTabContext`, see next sub-heading)
- VSM states (`Selected`/`Unselected`, `Normal`/`Disabled`) pumped automatically on navigation state change
 
See [FsTabBar.md](FsTabBar.md) for details.

### `FsTabContext`

`FsTabContext` provides per-tab data:

- `Route`
- `Title`
- `Icon`
- `IsSelected`
- `IsEnabled`

Note that the collection of `FsTabContext` (and these properties) are derived automatically from the provided `ShellContent` items in your `TabBar`.

`INotifyPropertyChanged` is implemented for each of these properties except `Route` (which provides `get` only), so you can bind to them in tab bar item templates.

### `FsLayout` (attached properties)

`FsLayout` provides an attached property to adjust your page layout to accommodate the tab bar. As it is a `ContentView` in FsShell and not part of the OS UI chrome, page layout is not automatically adjusted.

| Attached property | Padding edge | Bind to |
|---|---|---|
| `FsLayout.BottomChromePadding` | Bottom | `{DynamicResource FsBottomChromeHeight}` |

The easiest way to use this is to add it to your app's styles for `Page`. For example:

```xml
<Style TargetType="Page" ApplyToDerivedTypes="True">
    <Setter Property="Padding" Value="0"/>
    <Setter Property="BackgroundColor" Value="{AppThemeBinding Light={StaticResource White}, Dark={StaticResource OffBlack}}" />

    <Setter Property="fs:FsLayout.BottomChromePadding" Value="{DynamicResource FsBottomChromeHeight}" />
</Style>
```

### Chrome resource keys

FsShell exposes a `const string` field for the resource key. You can reference it from C# or use the string value directly in XAML:

| Constant | Value | Published when |
|---|---|---|
| `BottomChromeHeightResourceKey` | `"FsBottomChromeHeight"` | `TabBarIsDocked = true` |

The resource is a `double`, updated whenever the bar's size or visibility changes. It is set to `0` when `TabBarIsDocked` is `false`, when no bar is hosted, or when the active page suppresses it via `Shell.SetTabBarIsVisible`.

## Usage Examples

### Drop-in replacement for `Shell`

```xaml
<fs:FsShell
    x:Class="MyApp.AppShell"
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:fs="clr-namespace:FlagstoneUI.Core.Controls;assembly=FlagstoneUI.Core"
    xmlns:pages="clr-namespace:MyApp.Pages">

    <TabBar>
        <ShellContent Title="Home"   ContentTemplate="{DataTemplate pages:HomePage}"   Route="Home" />
        <ShellContent Title="Search" ContentTemplate="{DataTemplate pages:SearchPage}" Route="Search" />
        <ShellContent Title="Me"     ContentTemplate="{DataTemplate pages:MePage}"     Route="Me" />
    </TabBar>
</fs:FsShell>
```

### Custom item template

```xaml
<fs:FsShell ...>
    <fs:FsShell.TabBarItemTemplate>
        <DataTemplate>
            <VerticalStackLayout Padding="8" Spacing="2" HorizontalOptions="Center">
                <Image Source="{Binding Icon}" HeightRequest="24" WidthRequest="24" />
                <Label Text="{Binding Title}" FontSize="11" HorizontalOptions="Center" />
            </VerticalStackLayout>
        </DataTemplate>
    </fs:FsShell.TabBarItemTemplate>

    <TabBar>
        <ShellContent Title="Home"   Icon="home.png"   ContentTemplate="{DataTemplate pages:HomePage}"   Route="Home" />
        <ShellContent Title="Search" Icon="search.png" ContentTemplate="{DataTemplate pages:SearchPage}" Route="Search" />
        <ShellContent Title="Me"     Icon="me.png"     ContentTemplate="{DataTemplate pages:MePage}"     Route="Me" />
    </TabBar>
</fs:FsShell>
```

### Replacing the bar entirely

```xaml
<fs:FsShell ...>
    <fs:FsShell.TabBar>
        <local:MyCenterFabBar />
    </fs:FsShell.TabBar>

    <TabBar>
        <!-- ShellContent items as above -->
    </TabBar>
</fs:FsShell>
```

### A custom bar implementing `IFsTabBar`

```csharp
public class MyCenterFabBar : ContentView, IFsTabBar
{
    public IReadOnlyList<FsTabContext> ItemsSource
    {
        get => (IReadOnlyList<FsTabContext>)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public string? SelectedRoute
    {
        get => (string?)GetValue(SelectedRouteProperty);
        set => SetValue(SelectedRouteProperty, value);
    }

    public event EventHandler<FsTabBarSelectionChangedEventArgs>? ItemSelected;

    // TODO: build whatever bar you like (radial menu, side rail, big FAB...).
    // Raise ItemSelected when the user picks a tab; FsShell routes the rest.

    // TODO: BindableProperty declarations for ItemsSource / SelectedRoute
}
```

### Tab transitions

```csharp
public class FadeAnimator : ITabTransitionAnimator
{
    public async Task AnimateAsync(FsTabTransitionContext context, CancellationToken token)
    {
        // TODO: drive a fade between context.OutgoingView and context.IncomingView
    }
}
```

```xaml
<fs:FsShell>
    <fs:FsShell.TabTransitionAnimator>
        <local:FadeAnimator />
    </fs:FsShell.TabTransitionAnimator>
    ...
</fs:FsShell>
```

### Reserving page space for the bar

```xaml
<ContentPage
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:fs="clr-namespace:FlagstoneUI.Core.Controls;assembly=FlagstoneUI.Core"
    fs:FsLayout.BottomChromePadding="{DynamicResource FsBottomChromeHeight}">

    <ScrollView>
        <!-- page content -->
    </ScrollView>
</ContentPage>
```

### Hiding the bar on a single page

```xaml
<ContentPage Shell.TabBarIsVisible="False">
    <!-- bar drops out; FsBottomChromeHeight goes to 0 -->
</ContentPage>
```

## The Chrome Pattern

FsShell publishes the bar's measured height into `Application.Resources` as `FsBottomChromeHeight` whenever the bar's size or visibility changes.

| `TabBarIsDocked` | Resource key | Value |
|---|---|---|
| `true` | `FsBottomChromeHeight` | `bar.Height` |
| `false` | `FsBottomChromeHeight` | `0` |

The resource drops to `0` when no bar is hosted or the active page suppresses it via `Shell.SetTabBarIsVisible`.

Pages opt in via the `FsLayout.BottomChromePadding` attached property, which owns the bottom edge of `Page.Padding`:

```xml
<Style TargetType="Page" ApplyToDerivedTypes="True">
    <Setter Property="fs:FsLayout.BottomChromePadding" Value="{DynamicResource FsBottomChromeHeight}" />
</Style>
```

This is how FsShell brings full UI control up to the cross-platform layer without depending on platform-layer integration (e.g. `AdditionalSafeAreaInsets` / per-platform safe-area juggling). FsShell suppresses the native chrome and hosts whatever `View` you want to provide instead; the offset is then also the responsibility of the cross-platform layer.

This is fundamentally the core philosophy of Flagstone UI.

### Consuming the resource directly

```xaml
<!-- For when you want padding into something other than Page.Padding.Bottom -->
<ContentPage>
    <Grid RowDefinitions="*, Auto">
        <ScrollView Grid.Row="0">...</ScrollView>
        <BoxView
            Grid.Row="1"
            HeightRequest="{DynamicResource FsBottomChromeHeight}"
            Color="Transparent" />
    </Grid>
</ContentPage>
```

### Reading from code-behind

```csharp
if (Application.Current?.Resources.TryGetValue(
        FsShell.BottomChromeHeightResourceKey,
        out var raw) is true && raw is double height)
{
    // TODO: do something with the live chrome height
}
```

### Undocked bar with custom chrome dimensions

When `TabBarIsDocked` is `false` (floating FAB, radial menu, side rail), the bar is hosted as a full-bounds overlay with no renderer-imposed positioning. `FsBottomChromeHeight` is set to `0`. The consumer controls where the bar appears via standard MAUI layout properties (`HorizontalOptions`, `VerticalOptions`, `Margin`, `WidthRequest`, etc.) and can publish its own resource for page layout:

```csharp
public class MyFloatingBar : ContentView, IFsTabBar
{
    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        if (Application.Current is { } app)
            app.Resources["MyCustomChromeHeight"] = IsVisible ? height : 0;
    }

    // ... IFsTabBar implementation
}
```

## Events

| Event | Signature | Description |
|-------|-----------|-------------|
| `IFsTabBar.ItemSelected` | `EventHandler<FsTabBarSelectionChangedEventArgs>` | Raised by the bar when the user picks a tab; FsShell routes accordingly. |
| `Shell.Navigated` / `Shell.Navigating` | inherited from `Shell` | Standard Shell navigation events fire as normal. |

The `FsTabBar` sample implementation demonstrates how to use these. When the `ItemSelected` event is raised by an `IFsTabBar` implementation, FsShell sets the current selected item.  `FsTabContext.PropertyChanged` will fire for changed items, i.e. if the state transitions to or from selected or unselected; `DataTemplate`s can bind to `FsTabContext` properties to reflect the state accordingly.

## Best Practices

- Use the default `FsTabBar` with `TabBarItemTemplate` for simple per-tab styling — replace `TabBar` only when you need a different _shape_ (FAB, side rail, etc.)
- Keep custom bars cheap to measure — they are part of the page layout pass on every nav
- Opt into `FsLayout.BottomChromePadding` on every page that scrolls, not just the first one
- Don't depend on the bar's pixel height — read it from the `DynamicResource`
- **DO NOT** put per-page state inside the bar (it's a single hosted instance shared across tabs)
- For per-page bar visibility, prefer `Shell.SetTabBarIsVisible` per-page over conditional bar rebuilds

## Example: A Three-Tab App with Custom Items and Padded Pages

```xaml
<fs:FsShell
    x:Class="MyApp.AppShell"
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:fs="clr-namespace:FlagstoneUI.Core.Controls;assembly=FlagstoneUI.Core"
    xmlns:pages="clr-namespace:MyApp.Pages">

    <fs:FsShell.TabBarItemTemplate>
        <DataTemplate>
            <VerticalStackLayout Padding="0,8" Spacing="2" HorizontalOptions="Center">
                <Image Source="{Binding Icon}" HeightRequest="22" WidthRequest="22" />
                <Label Text="{Binding Title}" FontSize="11" HorizontalOptions="Center">
                    <Label.Triggers>
                        <DataTrigger TargetType="Label" Binding="{Binding IsSelected}" Value="True">
                            <Setter Property="FontAttributes" Value="Bold" />
                        </DataTrigger>
                    </Label.Triggers>
                </Label>
            </VerticalStackLayout>
        </DataTemplate>
    </fs:FsShell.TabBarItemTemplate>

    <TabBar>
        <ShellContent Title="Home"     Icon="home.png"     ContentTemplate="{DataTemplate pages:HomePage}"     Route="Home" />
        <ShellContent Title="Search"   Icon="search.png"   ContentTemplate="{DataTemplate pages:SearchPage}"   Route="Search" />
        <ShellContent Title="Profile"  Icon="profile.png"  ContentTemplate="{DataTemplate pages:ProfilePage}"  Route="Profile" />
    </TabBar>
</fs:FsShell>
```

```xaml
<!-- Every page that should leave room for the bar -->
<ContentPage
    x:Class="MyApp.Pages.HomePage"
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:fs="clr-namespace:FlagstoneUI.Core.Controls;assembly=FlagstoneUI.Core"
    fs:FsLayout.BottomChromePadding="{DynamicResource FsBottomChromeHeight}">
    ...
</ContentPage>
```

## Platform Support

`TabBarIsDocked = true` and `TabBarIsDocked = false` are fully supported on all platforms (Android, iOS, Mac Catalyst, Windows).

### Platform architecture

- **Android** — bar hosted in the same `LinearLayout` as the navigation area, native `BottomNavigationView` suppressed
- **iOS** — bar hosted as a subview of the `UITabBarController`'s view, native `UITabBar` hidden, frame pinned to bottom safe-area
- **Mac Catalyst** — shares the iOS renderer
- **Windows** — bar overlaid on the `ShellView`'s root grid. Uses the handler architecture (`ShellHandler`), not the legacy renderer compatibility layer used by other platforms

## Technical Implementation

### Renderer/handler responsibilities

Each platform suppresses the native tab chrome and hosts the consumer's `ContentView` from the `TabBar` slot:

- **Suppress native chrome:** iOS hides `UITabBar`, Android hides `BottomNavigationView`, Windows collapses the `TopNavArea` in the `ShellItemHandler`'s `MauiNavigationView`. On iOS/Android this is re-applied on layout passes to counter stock Shell re-enabling its own visibility logic.
- **Host the bar:** the bar's platform view (via `ToPlatform`) is added to the platform shell hierarchy — as a subview (iOS), a `LinearLayout` child (Android), or a root-grid overlay (Windows).
- **Docked positioning:** when `TabBarIsDocked` is `true`, the renderer pins the bar to the bottom edge and handles safe-area insets. When `false`, the bar is hosted as a full-bounds overlay with no renderer positioning.
- **Keyboard avoidance:** on iOS, the bar slides off-screen on `UIKeyboard.WillShow` and restores on `UIKeyboard.WillHide`, gated by `HideTabBarOnKeyboard`. Keyboard avoidance on Android and Windows is not yet implemented.

### What the renderer does NOT do

- It does _not_ reserve page-content space — that's the page's job via the `DynamicResource` and `FsLayout.BottomChromePadding`.
- It does _not_ touch `AdditionalSafeAreaInsets` on child view controllers or fragments.
- It does _not_ size or arrange the bar's content — that's MAUI's normal cross-platform layout.

### Bar lifecycle

- A single bar instance is shared across `ShellItem` switches.
- On iOS/Android, it is re-parented from one item's host to the next as the active item changes. On Windows, the `ShellItemHandler` is reused so the bar stays in the outer `ShellView`'s root grid.
- On disconnect, the bar is detached from its current parent (guarded against a newer host having already re-parented it) so it survives to be re-hosted.

## See Also

- [FsTabBar Control](FsTabBar.md) — default bar implementation
- [ADR012 — FsShell: Stylable Shell Chrome via Subclass](../decisions/adr012-fsshell.md)
- [ADR012_1 — Per-`ShellItem` Tab Bar Scoping](../decisions/adr012_1-fsshell-per-item-bar-scoping-addendum.md)
- [ADR012_2 — FsShell Renderer Scope Narrowing](../decisions/adr012_2-fsshell-renderer-scope-narrowing-addendum.md)
- [ADR012_3 — Bottom Chrome Height Resource Contract](../decisions/adr012_3-fsshell-bottom-chrome-resource-contract-addendum.md)
- [Control Implementation Guide](../guides/control-implementation-guide.md) — for contributors
