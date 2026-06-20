# FsTabBar Control

> _One-paragraph intro: `FsTabBar` is the reference bottom bar that ships with `FsShell`. It renders a horizontal row of tabs, drives selection via a two-way `SelectedRoute` binding, and ships with built-in selection animation — a rounded pill that slides behind the active tab and a scale bump on the selected tab. It is split into an abstract `FsTabBarBase` (the reusable bar contract, item plumbing, and visual-state pumping) and the concrete `FsTabBar` (the pill/scale visual layer). It implements `IFsTabBar` so it can be dropped into the `FsShell.TabBar` slot directly or wrapped inside a custom container, and it is designed to be customised through `ItemTemplate` first, subclassing `FsTabBar` second, and subclassing `FsTabBarBase` for an entirely different bar third._

## Features

- Ships as the default bar inside `FsShell` — no extra setup
- Per-tab `DataTemplate` via `ItemTemplate` (forwarded from `FsShell.TabBarItemTemplate`)
- Default template (icon + label) for the zero-config case
- Implements `IFsTabBar` — routing/selection wired up automatically
- Visual-state pumping for `Selected`/`Unselected` and `Normal`/`Disabled`
- Built-in selection animation: a sliding pill (`ShowPill`) and a scaling selected tab (`ScaleSelectedTab`), toggleable independently
- Single consumer switch — `AnimateTransitions` — to enable/disable selection transition animation
- Subclassable at two levels — override hooks on `FsTabBar` to tweak the pill/scale, or subclass `FsTabBarBase` to build a different-looking bar without re-implementing the routing/plumbing
- Also usable standalone (outside `FsShell`) via the `IFsTabBar` API

## Architecture: Base/Derived Split

The control is two types:

- **`FsTabBarBase` (abstract, no XAML)** owns the reusable, look-agnostic concerns: the `IFsTabBar` contract (`ItemsSource`, `SelectedRoute`, `ItemSelected`), item materialisation and `INotifyCollectionChanged`/per-item `PropertyChanged` subscription bookkeeping, tap routing through `OnTabTapped`, visual-state pumping, the default item template, and the `AnimateTransitions` consumer switch.
- **`FsTabBar` (concrete, XAML)** is the reference visual layer: it supplies the layout, the sliding pill, the scaling selected tab, and the bindable properties that style them.

The base reaches the hosted item views only through an abstract `TabContainer` property that the derived bar supplies from its own XAML. This is what lets a subclass present a completely different layout without touching the items/selection/VSM machinery.

### `FsTabBar` layout

`FsTabBar.xaml` layers two children in a `Grid` (`BarBackground`):

- a `Border` pill (`TabPill`) painted with `PillBackground` and shaped by `PillShape` (any `IShape`), sized and translated in code to track the selected tab;
- a `FlexLayout` (`TabLayout`, `Direction="Row"`, `JustifyContent="SpaceEvenly"`) that hosts the instantiated tab views and is returned as the base's `TabContainer`.

The outer `VerticalOptions="Start"` keeps the bar at its natural content height so the `FsShell` renderers can position it deterministically when docked.

### Item wrapping

- Every instantiated item is wrapped (by `FsTabBarBase.TabItemTemplateSelector`) with a `TapGestureRecognizer` that calls `OnTabTapped`
- The wrapper template is cached per inner `ItemTemplate` instance to keep `BindableLayout`'s template-recycling logic happy

## Properties

### Declared on `FsTabBarBase` (inherited by every bar)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ItemsSource` | `IReadOnlyList<FsTabContext>` | empty | The collection of tabs to render. Auto-populated by `FsShell`. |
| `ItemTemplate` | `DataTemplate?` | `null` | Template applied to each tab. When unset, the built-in icon+label template is used. |
| `SelectedRoute` | `string?` | `null` | Two-way: the current selection's route. Setting it requests navigation; FsShell mirrors external navigation back into it. |
| `AnimateTransitions` | `bool` | `true` | Single consumer switch for selection transition animation. Flows to subclasses as the `animated` argument of `OnSelectionChanged`; a subclass may honour or ignore it. |

### Declared on `FsTabBar` (the reference visual layer)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `PillBackground` | `Brush` | `SolidColorBrush(DarkOrchid @ 0.65α)` | Brush filling the pill (its `Border.Background`). Opacity travels through the brush's colour — no separate opacity knob. |
| `PillShape` | `IShape` | `RoundRectangle` (corner 20) | Pill geometry, applied as the pill `Border`'s `StrokeShape`. Use any `IShape` — `RoundRectangle`, `Ellipse`, or a custom `Path`/`Geometry`. |
| `ShowPill` | `bool` | `true` | Whether the sliding pill is shown. When `false` the pill is hidden and its sizing/translation work is skipped entirely. |
| `ScaleSelectedTab` | `bool` | `true` | Whether the selected tab is scaled up to emphasise it. When `false` no scale work runs and tabs stay at natural size. |

> `ShowPill`/`ScaleSelectedTab` toggle whether each effect exists at all; `AnimateTransitions` toggles whether the (still-present) effects transition smoothly or snap into place.

### Inherited from `ContentView`

- Usual `BackgroundColor`, `Padding`, `Margin`, `IsVisible`, etc. apply
- The bar's `BackgroundColor` is the most common style customisation

### Extension points

On `FsTabBarBase` (override these to build a custom bar):

- `protected abstract Layout TabContainer { get; }` — return the layout that hosts the item views; the base binds items and pumps VSM against it
- `protected void InitializeTabContainer()` — call once from the subclass constructor *after* the container exists (e.g. after `InitializeComponent`) to install the tap-wrapping template selector
- `protected virtual void OnSelectionChanged(FsTabContext context, bool animated)` — called on a user tap; override to animate/restyle selection
- `protected virtual void OnSelectionInitialized()` — called after items (re)materialise; override to apply visuals for the already-selected tab before any tap
- `protected virtual DataTemplate BuildDefaultItemTemplate()` — override to change the zero-config default tab content
- `protected virtual void OnTabTapped(FsTabContext context)` — override to customise routing on a tap (call `base` to keep `SelectedRoute`/`ItemSelected`/`OnSelectionChanged` in sync)
- `protected VisualElement? FindTab(Func<FsTabContext,bool>)` / `protected int SelectedIndex()` — selection helpers for use inside overrides

On `FsTabBar` specifically:

- Subclass `FsTabBar` to tweak the pill/scale behaviour or add bindable properties (e.g. badge counts) without re-implementing the routing wiring

## Companion Type: `FsTabContext`

Per-tab binding context handed to item templates. Properties are derived automatically from the `ShellContent` items in your `TabBar`.

| Property | Type | Notes |
|---|---|---|
| `Route` | `string` | The Shell route this tab represents (read-only, set at construction) |
| `Title` | `string?` | Sourced from `ShellSection.Title`, falling back to `ShellContent.Title` |
| `Icon` | `ImageSource?` | Sourced from `ShellSection.Icon`, falling back to `ShellContent.Icon` |
| `IsSelected` | `bool` | `true` for the active tab; drives the VSM `Selected` state |
| `IsEnabled` | `bool` | Drives the `Normal`/`Disabled` VSM states |

`INotifyPropertyChanged` is implemented for all properties except `Route` (immutable), so you can bind freely in item templates.

## Default Item Template

When no `ItemTemplate` is set, `FsTabBarBase.BuildDefaultItemTemplate` provides a simple icon + label layout:

- `VerticalStackLayout` with 8pt padding, 4pt spacing, `HorizontalOptions="Fill"`
- `Image` bound to `Icon` (24×24, centred)
- `Label` bound to `Title` (12pt, centred)

The default template does not include visual state setters — replace it via `ItemTemplate` to add selected-state visuals (colours, bold, scale, etc.).

## Visual States

The bar pumps the following VSM states on each item:

| State group | States | Trigger |
|-------------|--------|---------|
| Selection | `Selected`, `Unselected` | `FsTabContext.IsSelected` |
| Enablement | `Normal`, `Disabled` | `FsTabContext.IsEnabled` |

```xaml
<DataTemplate>
    <Border Padding="8" Stroke="Transparent">
        <VisualStateManager.VisualStateGroups>
            <VisualStateGroup x:Name="CommonStates">
                <VisualState x:Name="Unselected">
                    <VisualState.Setters>
                        <Setter Property="BackgroundColor" Value="Transparent" />
                    </VisualState.Setters>
                </VisualState>
                <VisualState x:Name="Selected">
                    <VisualState.Setters>
                        <Setter Property="BackgroundColor" Value="{DynamicResource Color.SurfaceVariant}" />
                    </VisualState.Setters>
                </VisualState>
            </VisualStateGroup>
        </VisualStateManager.VisualStateGroups>

        <VerticalStackLayout Spacing="2" HorizontalOptions="Center">
            <Image Source="{Binding Icon}" HeightRequest="22" WidthRequest="22" />
            <Label Text="{Binding Title}" FontSize="11" HorizontalOptions="Center" />
        </VerticalStackLayout>
    </Border>
</DataTemplate>
```

## Events

| Event | Signature | Description |
|-------|-----------|-------------|
| `ItemSelected` | `EventHandler<FsTabBarSelectionChangedEventArgs>` | Raised when the user taps a tab. `FsShell` subscribes automatically and routes accordingly. |

## Usage Examples

### As the default `FsShell` bar (zero config)

```xaml
<fs:FsShell ...>
    <TabBar>
        <ShellContent Title="Home"   Icon="home.png"   ContentTemplate="{DataTemplate pages:HomePage}"   Route="Home" />
        <ShellContent Title="Search" Icon="search.png" ContentTemplate="{DataTemplate pages:SearchPage}" Route="Search" />
    </TabBar>
</fs:FsShell>
```

The default icon+label template is used; `FsShell` instantiates an `FsTabBar` automatically.

### With a custom item template

```xaml
<fs:FsShell ...>
    <fs:FsShell.TabBarItemTemplate>
        <DataTemplate>
            <Border Padding="8,6" StrokeThickness="0" StrokeShape="RoundRectangle 12">
                <VisualStateManager.VisualStateGroups>
                    <VisualStateGroup x:Name="CommonStates">
                        <VisualState x:Name="Unselected" />
                        <VisualState x:Name="Selected">
                            <VisualState.Setters>
                                <Setter Property="BackgroundColor" Value="{DynamicResource Color.SecondaryContainer}" />
                            </VisualState.Setters>
                        </VisualState>
                    </VisualStateGroup>
                </VisualStateManager.VisualStateGroups>
                <VerticalStackLayout Spacing="2" HorizontalOptions="Center">
                    <Image Source="{Binding Icon}" HeightRequest="22" WidthRequest="22" />
                    <Label Text="{Binding Title}" FontSize="11" HorizontalOptions="Center" />
                </VerticalStackLayout>
            </Border>
        </DataTemplate>
    </fs:FsShell.TabBarItemTemplate>
    ...
</fs:FsShell>
```

### Styling the bar itself

```xaml
<fs:FsShell ...>
    <fs:FsShell.TabBar>
        <fs:FsTabBar
            BackgroundColor="{DynamicResource Color.Surface}"
            Padding="0,8" />
    </fs:FsShell.TabBar>
    ...
</fs:FsShell>
```

> Setting `TabBar` explicitly lets you reach properties on `FsTabBar` that have no XAML shorthand on `FsShell` — including `PillBackground`, `ShowPill`, `ScaleSelectedTab`, and `AnimateTransitions`. The default template is still used unless you also set `ItemTemplate`.

### Tuning the built-in animation

```xaml
<fs:FsShell.TabBar>
    <fs:FsTabBar
        PillBackground="{DynamicResource Brush.SecondaryContainer}"
        PillShape="RoundRectangle 24"
        ShowPill="True"
        ScaleSelectedTab="False"
        AnimateTransitions="True" />
</fs:FsShell.TabBar>
```

Set `AnimateTransitions="False"` to make selection changes snap instead of slide while keeping the pill and/or scale visible. Set `ShowPill="False"`/`ScaleSelectedTab="False"` to drop an effect entirely (and skip its work).

### Subclassing `FsTabBar`

```csharp
public class BadgeAwareTabBar : FsTabBar
{
    public static readonly BindableProperty BadgesProperty = BindableProperty.Create(
        nameof(Badges), typeof(IDictionary<string, int>), typeof(BadgeAwareTabBar));

    public IDictionary<string, int>? Badges
    {
        get => (IDictionary<string, int>?)GetValue(BadgesProperty);
        set => SetValue(BadgesProperty, value);
    }

    protected override void OnTabTapped(FsTabContext context)
    {
        // TODO: clear badge for the tapped tab before routing
        base.OnTabTapped(context);
    }
}
```

```xaml
<fs:FsShell>
    <fs:FsShell.TabBar>
        <local:BadgeAwareTabBar Badges="{Binding TabBadges}" />
    </fs:FsShell.TabBar>
    ...
</fs:FsShell>
```

### Building a different bar on `FsTabBarBase`

When the pill/scale look doesn't fit, subclass `FsTabBarBase` instead of `FsTabBar`: supply your own XAML and layout, return it as `TabContainer`, and implement the selection hooks. All the items/selection/VSM plumbing is inherited.

```csharp
public partial class UnderlineTabBar : FsTabBarBase
{
    public UnderlineTabBar()
    {
        InitializeComponent();
        InitializeTabContainer();   // after the container element exists
    }

    // The named FlexLayout/Grid from UnderlineTabBar.xaml that hosts the items.
    protected override Layout TabContainer => Items;

    protected override void OnSelectionChanged(FsTabContext context, bool animated)
    {
        // Honour the consumer's AnimateTransitions choice via `animated`.
        MoveUnderlineTo(SelectedIndex(), animated);
    }

    protected override void OnSelectionInitialized() => MoveUnderlineTo(SelectedIndex(), animated: false);
}
```

The XAML root must name the base type (because XAML's root tag is the partial's base class):

```xaml
<controls:FsTabBarBase xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                       xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
                       xmlns:controls="clr-namespace:FlagstoneUI.Core.Controls;assembly=FlagstoneUI.Core"
                       x:Class="MyApp.UnderlineTabBar">
    <!-- pill/underline decoration + the named item host returned by TabContainer -->
</controls:FsTabBarBase>
```

### Standalone use (outside `FsShell`)

`FsTabBar` only needs an `ItemsSource` and a way to react to selection — it doesn't depend on `Shell` at runtime.

```csharp
var bar = new FsTabBar
{
    ItemsSource = new List<FsTabContext>
    {
        new("home")    { Title = "Home",    Icon = "home.png" },
        new("search")  { Title = "Search",  Icon = "search.png" },
    },
};

bar.ItemSelected += (s, e) => Console.WriteLine($"Picked {e.Selected.Route}");
```

```xaml
<Grid RowDefinitions="*, Auto">
    <!-- your content -->
    <fs:FsTabBar Grid.Row="1" ItemsSource="{Binding Tabs}" />
</Grid>
```

## Best Practices

- Prefer `ItemTemplate` over subclassing for visual changes — most polish lives there
- Use VSM `Selected`/`Unselected` for selection visuals rather than triggers on `IsSelected` directly (the VSM pump runs on `PropertyChanged`)
- Keep templates cheap to instantiate — the bar instantiates one per tab on each `ItemsSource` change
- Set `BackgroundColor` (or `Background`) on the bar — it shows behind item content and, on iOS, extends behind the home indicator
- Don't put per-page state inside the bar; the bar is a single shared instance across `ShellItem` switches
- When subclassing, route selection through `base.OnTabTapped(context)` so `SelectedRoute` and `ItemSelected` stay in sync

## Platform Support

`FsTabBar` is a pure cross-platform MAUI `ContentView` — it contains no per-platform code. All platform-specific hosting and positioning is handled by the `FsShell` renderers/handler. Supported on Android, iOS, Mac Catalyst, and Windows.

Note: on iOS, content that scales beyond the bar's bounds (e.g. the pill animation) is not clipped. On Android and Windows, the bar's platform container clips overflow. Use `TabBarIsDocked = false` for designs that need to break out of the bar's bounds on those platforms.

## Technical Implementation

### Item lifecycle (`FsTabBarBase`)

- `BindableLayout.SetItemsSource` on `TabContainer` drives item instantiation; the `FlexLayout` distributes the resulting children evenly (`JustifyContent="SpaceEvenly"`)
- `OnItemsSourceChanged` hooks `INotifyCollectionChanged` and per-item `PropertyChanged` (and unhooks the old collection/items) so VSM stays in sync, then pumps all states and calls `OnSelectionInitialized`

### Template wrapping (`FsTabBarBase`)

- Every template is wrapped (by `TabItemTemplateSelector`) to attach a `TapGestureRecognizer` that routes through `OnTabTapped`
- Wrappers are cached by inner-template identity — on some platforms, handing back a freshly-constructed `DataTemplate` per `OnSelectTemplate` call breaks `BindableLayout`'s items collection

### VSM pumping (`FsTabBarBase`)

- `PumpAllVsmStates` runs on items-source change (and on collection changes)
- `PumpVsmState(ctx)` runs on individual `IsSelected`/`IsEnabled` changes
- States are pushed to the item's root `VisualElement` (its `BindingContext` is the `FsTabContext`)

### Selection animation (`FsTabBar`)

- On tap, the base calls `OnSelectionChanged(context, animated)` where `animated == AnimateTransitions`; `FsTabBar` runs the pill translate and scale bump (smoothly when `animated`, instantly otherwise)
- On `OnSelectionInitialized` (after items materialise) the already-selected tab is scaled and the pill parked under it instantly — so the default selection looks correct before any tap
- The pill's width and position depend on the bar's measured width, so placement also re-runs from `BarBackground.SizeChanged`
- All pill/scale work early-returns when `ShowPill`/`ScaleSelectedTab` are `false`, so disabled effects cost nothing

## See Also

- [FsShell Control](FsShell.md) — the host
- [ADR012 — FsShell: Stylable Shell Chrome via Subclass](../decisions/adr012-fsshell.md)
- [ADR013 — Shell Animations](../decisions/adr013-shell-animations.md) — context for the "bring your own bar" animation story this control now partly fulfils
- [ADR012_1 — Per-`ShellItem` Tab Bar Scoping](../decisions/adr012_1-fsshell-per-item-bar-scoping-addendum.md)
- [ADR012_2 — FsShell Renderer Scope Narrowing](../decisions/adr012_2-fsshell-renderer-scope-narrowing-addendum.md)
- [ADR012_3 — Bottom Chrome Height Resource Contract](../decisions/adr012_3-fsshell-bottom-chrome-resource-contract-addendum.md)
