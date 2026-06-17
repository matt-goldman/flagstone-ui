# FsTabBar Control

> _One-paragraph intro: FsTabBar is the reference bottom bar that ships with FsShell. It renders a horizontal row of tabs, drives selection via a two-way `SelectedRoute` binding, and is designed to be customised through `ItemTemplate` first and subclassing second. It implements `IFsTabBar` so it can be dropped into the `FsShell.TabBar` slot directly or wrapped inside a custom container._

## Features

- TODO: ships as the default bar inside `FsShell` — no extra setup
- TODO: per-tab `DataTemplate` via `ItemTemplate` (forwarded from `FsShell.TabBarItemTemplate`)
- TODO: default template (icon + label) for the zero-config case
- TODO: implements `IFsTabBar` — routing/selection wired up automatically
- TODO: visual-state pumping for `Selected`/`Unselected` and `Normal`/`Disabled`
- TODO: subclassable — override `OnTabTapped` to customise selection behaviour
- TODO: also usable standalone (outside `FsShell`) via the `IFsTabBar` API

## Architecture: Grid-Based Item Layout

- TODO: items are placed in a single-row `Grid` with one auto-sized column per item
- TODO: rationale — earlier `HorizontalStackLayout`-of-items hit a MAUI iOS layout bug where item children with nested `HorizontalStackLayout` roots collapsed to height 0 on layout passes after the first
- TODO: `Grid.CrossPlatformArrange` distributes children from measured row/column geometry rather than re-reading each child's `DesiredSize`, so children stay at their measured size across passes
- TODO: outer `VerticalOptions.Start` keeps the bar at its natural content height so the FsShell renderers can position the bar deterministically against the bottom edge

### Item wrapping

- TODO: every instantiated item is wrapped with a `TapGestureRecognizer` that calls `OnTabTapped`
- TODO: the wrapper template is cached per inner `ItemTemplate` instance to keep `BindableLayout`'s template-recycling logic happy

## Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ItemsSource` | `IReadOnlyList<FsTabContext>` | empty | The collection of tabs to render. Auto-populated by `FsShell`. |
| `ItemTemplate` | `DataTemplate?` | `null` | Template applied to each tab. When unset, the built-in icon+label template is used. |
| `SelectedRoute` | `string?` | `null` | Two-way: the current selection's route. Setting it requests navigation; FsShell mirrors external navigation back into it. |

### Inherited from `ContentView`

- TODO: usual `BackgroundColor`, `Padding`, `Margin`, `IsVisible`, etc. apply
- TODO: bar's `BackgroundColor` is the most common style customisation

### Extension points

- TODO: `protected virtual void OnTabTapped(FsTabContext context)` — override to customise what happens on a tap
- TODO: subclass `FsTabBar` to add bindable properties (e.g. badge counts) without re-implementing the routing wiring

## Companion Type: `FsTabContext`

- TODO: per-tab binding context handed to item templates
- TODO: `Route` — the Shell route this tab represents (immutable)
- TODO: `Title` — sourced from `Shell.Title` on the underlying `ShellContent`
- TODO: `Icon` — sourced from `Shell.Icon`
- TODO: `IsSelected` — true for the active tab; drives the VSM `Selected` state
- TODO: `IsEnabled` — reserved for future disabled-tab support; drives `Normal`/`Disabled` VSM
- TODO: `INotifyPropertyChanged` — bind freely with two-way semantics on the observable fields

## Default Item Template

- TODO: vertical stack of icon (24×24, centred) and label (12pt, centred), 8pt padding, 4pt spacing
- TODO: bindings: `Image.Source` → `Icon`, `Label.Text` → `Title`
- TODO: not styled with visual states — replace via `ItemTemplate` to add selected-state visuals

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

> Setting `TabBar` explicitly lets you reach properties on `FsTabBar` that have no XAML shorthand on `FsShell`. The default template is still used unless you also set `ItemTemplate`.

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

- TODO: prefer `ItemTemplate` over subclassing for visual changes — most polish lives there
- TODO: use VSM `Selected`/`Unselected` for selection visuals rather than triggers on `IsSelected` directly (the pump runs on `PropertyChanged`)
- TODO: keep templates cheap to instantiate — the bar instantiates one per tab on each `ItemsSource` change
- TODO: set `BackgroundColor` (or `Background`) on the bar — it shows behind item content and, on iOS, extends behind the home indicator
- TODO: don't put per-page state inside the bar; the bar is a single shared instance across `ShellItem` switches
- TODO: when subclassing, route selection through `base.OnTabTapped(context)` so `SelectedRoute` and `ItemSelected` stay in sync

## Platform Support

- TODO: Android, iOS, MacCatalyst, Windows
- TODO: pure cross-platform MAUI — no per-platform code in `FsTabBar` itself; `FsShell` renderers handle the host-and-pin work

## Technical Implementation

### Item lifecycle

- TODO: `BindableLayout.SetItemTemplateSelector` drives item instantiation
- TODO: `ChildAdded` adds a new `ColumnDefinition(GridLength.Star)` and assigns `Grid.Column` to the new item
- TODO: `ChildRemoved` trims a column off the end
- TODO: `OnItemsSourceChanged` hooks `INotifyCollectionChanged` and per-item `PropertyChanged` so VSM stays in sync

### Template wrapping

- TODO: every template is wrapped to attach a `TapGestureRecognizer`
- TODO: wrappers are cached by inner-template identity — on some platforms, handing back a freshly-constructed `DataTemplate` per `OnSelectTemplate` call breaks `BindableLayout`'s items collection

### VSM pumping

- TODO: `PumpAllVsmStates` runs on items-source change
- TODO: `PumpVsmState(ctx)` runs on individual `IsSelected`/`IsEnabled` changes
- TODO: states are pushed to the item's root `VisualElement` (its `BindingContext` is the `FsTabContext`)

## See Also

- [FsShell Control](FsShell.md) — the host
- [ADR012 — FsShell: Stylable Shell Chrome via Subclass](../decisions/adr012-fsshell.md)
- [ADR012_1 — Per-`ShellItem` Tab Bar Scoping](../decisions/adr012_1-fsshell-per-item-bar-scoping-addendum.md)
- [ADR012_2 — FsShell Renderer Scope Narrowing](../decisions/adr012_2-fsshell-renderer-scope-narrowing-addendum.md)
- [ADR012_3 — Bottom Chrome Height Resource Contract](../decisions/adr012_3-fsshell-bottom-chrome-resource-contract-addendum.md)
