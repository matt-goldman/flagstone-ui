# FsShell Control

> _One-paragraph intro: FsShell is a drop-in `Shell` subclass that lets you bring your own bottom chrome (tab bar, FAB, side rail, whatever) without writing platform code. It preserves Shell's routing, navigation, and lifecycle semantics; you just hand it a `ContentView` and FsShell hosts it._

## Features

- TODO: drop-in `Shell` subclass — works wherever `Shell` works
- TODO: default `FsTabBar` with a `TabBarItemTemplate` for per-tab customisation
- TODO: full bar replacement via `TabBar` slot — any `ContentView`
- TODO: routing & current-tab binding done for you (when bar implements `IFsTabBar`)
- TODO: optional tab-change animations via `ITabTransitionAnimator`
- TODO: optional keyboard-avoidance (`HideTabBarOnKeyboard`)
- TODO: bottom-chrome height published as a `DynamicResource` for pages to consume
- TODO: per-page opt-in padding via `FsLayout.BottomChromePadding` attached property

## Architecture: Hosted Chrome, Not Replaced Chrome

- TODO: vanilla `Shell` owns its native chrome (`UITabBar` on iOS, `BottomNavigationView` on Android) and only lets you skin it
- TODO: FsShell hides the native chrome and hosts a MAUI `ContentView` you author in XAML/C#
- TODO: FsShell stays inside Shell — routing, flyouts, navigation stack, lifecycle all unchanged
- TODO: platform-specific renderers handle suppression + hosting; the bar itself is plain cross-platform MAUI
- TODO: chrome _layout_ inside the page is decoupled — pages opt in via the published `DynamicResource`, no platform safe-area juggling

### Why not just use `Shell`?

- TODO: explain limits of `Shell.SetTabBarIsVisible`, item templates, theming
- TODO: explain why we can't just inherit and re-skin
- TODO: explain the "we don't render the bar anymore; you do" framing

## Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `TabBar` | `ContentView?` | auto-instantiated `FsTabBar` | The bar to host at the bottom. Auto-set to a new `FsTabBar` if left null. |
| `TabBarItemTemplate` | `DataTemplate?` | `null` | Template applied to each item in the default `FsTabBar`. Ignored if `TabBar` is replaced with a non-`FsTabBar`. |
| `TabTransitionAnimator` | `ITabTransitionAnimator?` | `null` | Invoked on tab selection changes to drive a transition. |
| `HideTabBarOnKeyboard` | `bool` | `true` | When true, slides the bar off-screen while the soft keyboard is open. |
| `Tabs` | `IReadOnlyList<FsTabContext>` | live | Live projection of the active item's sections; bound to the bar's `ItemsSource`. |

### Extension points

- TODO: `protected virtual void RebuildTabs()` — override to project tabs differently
- TODO: subclassing `FsShell` for app-specific shell behaviour

## Companion Types

### `IFsTabBar`

- TODO: contract any bar must honour to participate in routing
- TODO: `ItemsSource`, `SelectedRoute`, `ItemSelected` event

### `FsTabBar` (default implementation)

- TODO: reference bar with a `Grid`-driven `BindableLayout`
- TODO: per-item template via `ItemTemplate`
- TODO: VSM states (`Selected`/`Unselected`, `Normal`/`Disabled`) pumped automatically
- TODO: see [FsTabBar.md](FsTabBar.md) for details (or move it inline here if you'd rather have one doc)

### `FsTabContext`

- TODO: per-tab data: `Route`, `Title`, `Icon`, `IsSelected`, `IsEnabled`
- TODO: `INotifyPropertyChanged` — bind freely from item templates

### `ITabTransitionAnimator` & `FsTabTransitionContext`

- TODO: hook for custom transitions between tab content
- TODO: receives outgoing/incoming index + cancellation token

### `FsLayout` (attached properties)

- TODO: `FsLayout.BottomChromePadding` — reflects a `double` into `Page.Padding.Bottom`
- TODO: designed to be bound to `{DynamicResource FsBottomChromeHeight}`

### `FsShell.BottomChromeHeightResourceKey`

- TODO: `const string` resource key under which FsShell publishes the current chrome height
- TODO: value is a `double` (DIPs); updates whenever the bar's size or visibility changes
- TODO: drops to 0 when no bar is hosted or the active page suppresses it via `Shell.SetTabBarIsVisible`

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
    <!-- bar drops out; FsBottomChromeHeight resource goes to 0 -->
</ContentPage>
```

## The Bottom Chrome Pattern

- TODO: explain the contract — FsShell writes `FsBottomChromeHeight` (a `double`) into `Application.Resources` whenever the bar size or visibility changes
- TODO: pages opt in by reading `{DynamicResource FsBottomChromeHeight}` — most simply through `FsLayout.BottomChromePadding`
- TODO: same pattern generalises to any chrome you mount (side rail → `FsRightChromeWidth`, top app bar → `FsTopChromeHeight`, etc.)
- TODO: explain why this beats `AdditionalSafeAreaInsets` / per-platform safe-area juggling

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

### Custom chrome publishing its own dimensions

- TODO: pattern for a side rail / top bar / FAB to publish a different key
- TODO: example sketch:

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

## Events

| Event | Signature | Description |
|-------|-----------|-------------|
| `IFsTabBar.ItemSelected` | `EventHandler<FsTabBarSelectionChangedEventArgs>` | Raised by the bar when the user picks a tab; FsShell routes accordingly. |
| `Shell.Navigated` / `Shell.Navigating` | inherited from `Shell` | Standard Shell navigation events fire as normal. |

- TODO: spell out what subscribers typically do with each
- TODO: mention `FsTabContext.PropertyChanged` for per-tab state observation

## Best Practices

- TODO: use `TabBarItemTemplate` for simple per-tab styling — replace `TabBar` only when you need a different _shape_ (FAB, side rail, etc.)
- TODO: keep custom bars cheap to measure — they are part of the page layout pass on every nav
- TODO: opt into `FsLayout.BottomChromePadding` on every page that scrolls, not just the first one
- TODO: don't depend on the bar's pixel height — read it from the `DynamicResource`
- TODO: avoid putting per-page state inside the bar (it's a single hosted instance shared across tabs)
- TODO: prefer `Shell.SetTabBarIsVisible` per-page over conditional bar rebuilds

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

- TODO: Android — bar hosted in the same `LinearLayout` as the navigation area, native `BottomNavigationView` suppressed
- TODO: iOS — bar hosted as a subview of the `UITabBarController`'s view, native `UITabBar` hidden, frame pinned to bottom safe-area
- TODO: macOS — TODO
- TODO: Windows — TODO

## Technical Implementation

### Renderer responsibilities

- TODO: suppress the native chrome
- TODO: host the user's `ContentView` from the `TabBar` slot
- TODO: keep it pinned to the bottom edge (including safe-area cap on iOS)
- TODO: slide it on keyboard if `HideTabBarOnKeyboard` is set

### What the renderer no longer does

- TODO: it does _not_ try to reserve page-content space — that's the page's job via the `DynamicResource`
- TODO: it does _not_ touch `AdditionalSafeAreaInsets` on child VCs / fragments
- TODO: it does _not_ size or arrange the bar's content — that's MAUI's normal cross-platform layout

### Bar lifecycle

- TODO: a single bar instance is shared across `ShellItem` switches
- TODO: re-parented from one item's host to the next as the active item changes
- TODO: released only when no FsShellItemRenderer references it anymore

## See Also

- [FsTabBar Control](FsTabBar.md) — default bar implementation
- [ADR012 — FsShell: Stylable Shell Chrome via Subclass](../decisions/adr012-fsshell.md)
- [ADR012_1 — Per-`ShellItem` Tab Bar Scoping](../decisions/adr012_1-fsshell-per-item-bar-scoping-addendum.md)
- [ADR012_2 — FsShell Renderer Scope Narrowing](../decisions/adr012_2-fsshell-renderer-scope-narrowing-addendum.md)
- [ADR012_3 — Bottom Chrome Height Resource Contract](../decisions/adr012_3-fsshell-bottom-chrome-resource-contract-addendum.md)
- [Control Implementation Guide](../guides/control-implementation-guide.md) — for contributors
