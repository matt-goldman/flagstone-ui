# Your First Shell App

`FsShell` is a drop-in replacement for .NET MAUI's `Shell` that lets you fully control the look of the tab bar — from light per-tab styling all the way to a completely custom navigation bar with a floating action button — **without any platform code**. It keeps all of Shell's routing, navigation, and lifecycle behaviour; only the presentation of the bar changes.

This guide builds a tabbed app in stages:

1. A standard bottom-tab app using the built-in bar.
2. Leaving room for the bar on your pages.
3. Styling the tab items.
4. Replacing the bar entirely with a floating, custom bar and a centre action button.

The two example apps — [instagrim](../examples.md) and [Beer Driven Devs](../examples.md) — are built exactly this way; the snippets below mirror their real markup.

> Prefer the full API surface? See the [FsShell reference](../controls/FsShell.md) and the [FsTabBar reference](../controls/FsTabBar.md).

## Prerequisites

- A .NET MAUI project on .NET 10 (`dotnet new maui`).
- The FlagstoneUI package installed — see the [Quickstart](quickstart.md):

  ```bash
  dotnet add package FlagstoneUI.Core
  ```

## Step 1 — Register FlagstoneUI

Most FlagstoneUI controls self-register, but `FsShell` needs its platform renderers registered at startup because Shell still uses MAUI's legacy renderer model. Add `UseFlagstoneUI()` in `MauiProgram.cs`:

```csharp
using FlagstoneUI.Core.Builders;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseFlagstoneUI();   // 👈 registers the FsShell renderers

        return builder.Build();
    }
}
```

That's the only platform-related code you'll write.

## Step 2 — Make your AppShell an FsShell

Take a normal `AppShell.xaml` and change the root element from `<Shell>` to `<fs:FsShell>`. Everything inside — `TabBar`, `ShellContent`, routes — stays the same:

```xml
<?xml version="1.0" encoding="UTF-8" ?>
<fs:FsShell
    x:Class="MyApp.AppShell"
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:fs="clr-namespace:FlagstoneUI.Core.Controls;assembly=FlagstoneUI.Core"
    xmlns:pages="clr-namespace:MyApp.Pages"
    Title="MyApp">

    <TabBar>
        <ShellContent Title="Home"
                      Icon="home.png"
                      ContentTemplate="{DataTemplate pages:HomePage}"
                      Route="HomePage" />

        <ShellContent Title="Community"
                      Icon="community.png"
                      ContentTemplate="{DataTemplate pages:CommunityPage}"
                      Route="CommunityPage" />

        <ShellContent Title="Settings"
                      Icon="settings.png"
                      ContentTemplate="{DataTemplate pages:SettingsPage}"
                      Route="SettingsPage" />
    </TabBar>
</fs:FsShell>
```

```csharp
// AppShell.xaml.cs
public partial class AppShell : FlagstoneUI.Core.Controls.FsShell
{
    public AppShell() => InitializeComponent();
}
```

Run the app. You get a bottom tab bar — the built-in `FsTabBar` — that's fully cross-platform and identical on every device. By default `TabBarIsDocked` is `true`, so the bar is pinned to the bottom edge with safe-area handling taken care of for you.

## Step 3 — Leave room for the bar

Unlike the native tab bar, the `FsShell` bar is a normal view hosted over your content, so it does **not** automatically shrink your pages. `FsShell` publishes the bar's measured height as a `DynamicResource` named `FsBottomChromeHeight`, and pages opt in to padding with the `FsLayout.BottomChromePadding` attached property.

The easiest approach is to set it once on your `Page` style so every page picks it up:

```xml
<!-- In your App.xaml resources -->
<Style TargetType="Page" ApplyToDerivedTypes="True">
    <Setter Property="fs:FsLayout.BottomChromePadding"
            Value="{DynamicResource FsBottomChromeHeight}" />
</Style>
```

Now your content stops just above the bar instead of scrolling underneath it. (If you *want* content to flow behind a translucent bar, just don't add the padding.)

## Step 4 — Style the tab items

You can template each tab without replacing the whole bar. Set `TabBarItemTemplate` on the shell; each item is bound to an `FsTabContext`, which exposes `Title`, `Icon`, `IsSelected`, and `IsEnabled`:

```xml
<fs:FsShell ...>
    <fs:FsShell.TabBarItemTemplate>
        <DataTemplate x:DataType="fs:FsTabContext">
            <VerticalStackLayout Padding="0,8" Spacing="2" HorizontalOptions="Center">
                <Image Source="{Binding Icon}" HeightRequest="22" WidthRequest="22" />
                <Label Text="{Binding Title}" FontSize="11" HorizontalOptions="Center">
                    <Label.Triggers>
                        <DataTrigger TargetType="Label"
                                     Binding="{Binding IsSelected}" Value="True">
                            <Setter Property="FontAttributes" Value="Bold" />
                        </DataTrigger>
                    </Label.Triggers>
                </Label>
            </VerticalStackLayout>
        </DataTemplate>
    </fs:FsShell.TabBarItemTemplate>

    <TabBar>
        <!-- ShellContent items as before -->
    </TabBar>
</fs:FsShell>
```

The built-in bar pumps `Selected`/`Unselected` and `Normal`/`Disabled` visual states onto each item automatically, so you can also drive styling from a `VisualStateManager` group inside the template.

## Step 5 — Replace the bar with a custom floating bar

When you need a different *shape* of navigation — a floating bar, a side rail, or the classic "centre action button" — replace the bar entirely. The simplest way to build one is to subclass **`FsTabBarBase`**, which gives you item materialisation, tap routing, and visual-state pumping for free. You supply the layout and the look.

This is exactly how `instagrim` builds its undocked nav bar with a big centre camera button.

### 5a — Switch to an undocked bar and plug in your control

Set `TabBarIsDocked="False"` (so the bar floats over content and *you* control its placement), hide the Shell nav bar, and provide your control in the `FsShell.TabBar` slot. Note the **reserved `ShellContent`** with no content — it holds the centre slot open for the action button:

```xml
<fs:FsShell
    x:Class="MyApp.AppShell"
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:fs="clr-namespace:FlagstoneUI.Core.Controls;assembly=FlagstoneUI.Core"
    xmlns:controls="clr-namespace:MyApp.Controls"
    xmlns:pages="clr-namespace:MyApp.Pages"
    TabBarIsDocked="False"
    NavBarIsVisible="False"
    Title="MyApp">

    <fs:FsShell.TabBar>
        <controls:NavBar />
    </fs:FsShell.TabBar>

    <TabBar>
        <ShellContent Title="Feed"      ContentTemplate="{DataTemplate pages:FeedPage}"      Route="feed" />
        <ShellContent Title="Discover"  ContentTemplate="{DataTemplate pages:DiscoverPage}"  Route="discover" />

        <!-- no content: reserves the centre slot for the action button -->
        <ShellContent Title="Action" Route="action" />

        <ShellContent Title="Hauntings" ContentTemplate="{DataTemplate pages:HauntingsPage}" Route="hauntings" />
        <ShellContent Title="Profile"   ContentTemplate="{DataTemplate pages:ProfilePage}"   Route="profile" />
    </TabBar>
</fs:FsShell>
```

### 5b — Build the custom bar

The bar's XAML root is `FsTabBarBase`. Give it an `ItemTemplate` for the tabs and a named layout to host them (here a `FlexLayout` called `Tabs`), then overlay your floating action button:

```xml
<?xml version="1.0" encoding="utf-8"?>
<fs:FsTabBarBase
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:fs="clr-namespace:FlagstoneUI.Core.Controls;assembly=FlagstoneUI.Core"
    x:Class="MyApp.Controls.NavBar">

    <fs:FsTabBarBase.ItemTemplate>
        <DataTemplate x:DataType="fs:FsTabContext">
            <Grid MinimumWidthRequest="50">
                <VisualStateManager.VisualStateGroups>
                    <VisualStateGroup x:Name="CommonStates">
                        <VisualState x:Name="Unselected" />
                        <VisualState x:Name="Selected" />
                    </VisualStateGroup>
                </VisualStateManager.VisualStateGroups>
                <Image Source="{Binding Icon}" HeightRequest="28" WidthRequest="28"
                       HorizontalOptions="Center" VerticalOptions="Center" />
            </Grid>
        </DataTemplate>
    </fs:FsTabBarBase.ItemTemplate>

    <Grid VerticalOptions="End" HorizontalOptions="Fill">
        <!-- the tab container the base class fills for you -->
        <FlexLayout x:Name="Tabs"
                    Direction="Row"
                    JustifyContent="SpaceAround"
                    AlignItems="Center"
                    HeightRequest="70" />

        <!-- the floating centre action button -->
        <ImageButton x:Name="ActionButton"
                     WidthRequest="80" HeightRequest="80" CornerRadius="40"
                     TranslationY="-40"
                     HorizontalOptions="Center" VerticalOptions="Center"
                     Source="camera.png"
                     Clicked="ActionButton_OnClicked" />
    </Grid>
</fs:FsTabBarBase>
```

In code-behind, tell the base class which layout hosts the tabs (`TabContainer`), call `InitializeTabContainer()`, and override `OnTabTapped` to ignore taps on the reserved slot. The action button does its own thing:

```csharp
using FlagstoneUI.Core.Controls;

public partial class NavBar : FsTabBarBase
{
    public NavBar()
    {
        InitializeComponent();
        InitializeTabContainer();   // wires tap routing into the Tabs layout
    }

    protected override Layout TabContainer => Tabs;

    protected override void OnTabTapped(FsTabContext context)
    {
        // The reserved "Action" slot isn't a real destination — ignore it.
        if (context.Title == "Action")
            return;

        base.OnTabTapped(context);  // base raises ItemSelected; FsShell routes the rest
    }

    private void ActionButton_OnClicked(object? sender, EventArgs e)
    {
        // open a camera page, a create-post sheet, whatever you like
    }
}
```

That's the whole pattern: `FsTabBarBase` materialises one templated view per `ShellContent`, wraps each in a tap gesture, and routes the selection back through `FsShell`. You only wrote the layout and the look.

> Because the bar is undocked, `FsBottomChromeHeight` is `0` — you control the bar's size and position with normal MAUI layout properties. If your floating bar needs pages to leave space for it, publish your own height resource from the bar (see [Undocked bar with custom chrome dimensions](../controls/FsShell.md#undocked-bar-with-custom-chrome-dimensions)).

## Where to go next

- **[FsShell reference](../controls/FsShell.md)** — every property, the chrome pattern, per-page bar visibility, tab transitions, and platform details.
- **[FsTabBar reference](../controls/FsTabBar.md)** — the built-in bar's sliding-pill animation and templating.
- **[Example apps](../examples.md)** — `instagrim` and `Beer Driven Devs` are full apps built with these exact patterns.
