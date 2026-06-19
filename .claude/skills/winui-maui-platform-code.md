---
description: Reference patterns for writing Windows platform code in a MAUI library. Use when writing or modifying files under Platforms/Windows/ that interact with WinUI controls.
---

# WinUI Platform Code Patterns for MAUI

## Type aliasing

MAUI's implicit usings (`<UseMaui>true</UseMaui>` + `<ImplicitUsings>enable</ImplicitUsings>`) import `Microsoft.Maui.Controls` globally. Several WinUI types collide. Always alias these:

```csharp
using WGrid = Microsoft.UI.Xaml.Controls.Grid;
using WRowDefinition = Microsoft.UI.Xaml.Controls.RowDefinition;
using WColumnDefinition = Microsoft.UI.Xaml.Controls.ColumnDefinition;
using WGridLength = Microsoft.UI.Xaml.GridLength;
using WVisibility = Microsoft.UI.Xaml.Visibility;
using ContentView = Microsoft.Maui.Controls.ContentView;  // if needed
```

Types that do NOT conflict and can be used directly from `Microsoft.UI.Xaml.Controls`:
`NavigationView`, `StackPanel`, `Panel`, `ContentControl`, `SplitView`, `Button`, `Frame`

## Finding template children by name

WinUI controls use XAML templates. After a control loads, find named template parts via `VisualTreeHelper`:

```csharp
private static T? FindByName<T>(DependencyObject root, string name) where T : FrameworkElement
{
    var count = VisualTreeHelper.GetChildrenCount(root);
    for (int i = 0; i < count; i++)
    {
        var child = VisualTreeHelper.GetChild(root, i);
        if (child is T element && element.Name == name)
            return element;

        var result = FindByName<T>(child, name);
        if (result is not null)
            return result;
    }
    return null;
}
```

Only works **after the control is loaded** (template applied). Guard with `IsLoaded` check or subscribe to `Loaded` event.

### Key MauiNavigationView template children

These are retrieved internally by `MauiNavigationView.OnApplyTemplate()`:

| Name | Type | Purpose |
|---|---|---|
| `TopNavArea` | `StackPanel` | Top navigation strip (tabs in PaneDisplayMode.Top) |
| `TopNavMenuItemsHost` | `ItemsRepeater` | The actual tab item repeater |
| `ContentGrid` | `Grid` | Content area (page content + padding) |
| `ContentPaneTopPadding` | `Grid` | Padding above content for top nav |
| `RootSplitView` | `SplitView` | Flyout split view |
| `PaneContentGrid` | `Grid` | Flyout pane content |
| `PaneToggleButtonGrid` | `Grid` | Hamburger button area |
| `NavigationViewBackButton` | `Button` | Back navigation button |
| `TogglePaneButton` | `Button` | Flyout toggle (hamburger) |

## Keeping modifications stable

MAUI handlers and WinUI controls actively manage their state. When you modify the visual tree (e.g., collapsing a template child), the framework may reset it. Use `RegisterPropertyChangedCallback` to re-apply:

```csharp
// Suppress native tabs — re-collapse when ShellItemHandler resets PaneDisplayMode
_topNavArea.Visibility = WVisibility.Collapsed;
_callbackToken = navigationView.RegisterPropertyChangedCallback(
    NavigationView.PaneDisplayModeProperty, (sender, dp) =>
    {
        _topNavArea.Visibility = WVisibility.Collapsed;
    });
```

Always pair with cleanup:
```csharp
navigationView.UnregisterPropertyChangedCallback(
    NavigationView.PaneDisplayModeProperty, _callbackToken);
```

## Hosting MAUI views in WinUI layouts

Convert a MAUI view to its WinUI platform element:

```csharp
var platformView = mauiView.ToPlatform(mauiContext);
```

When re-hosting a shared view instance (e.g., a tab bar that moves between containers):

```csharp
// Detach from previous parent — a WinUI element can only have one parent
if (platformView.Parent is Panel oldParent)
    oldParent.Children.Remove(platformView);
```

To add to a Grid with proper row/column placement:

```csharp
grid.RowDefinitions.Add(new WRowDefinition { Height = WGridLength.Auto });
WGrid.SetRow(platformView, grid.RowDefinitions.Count - 1);

// Span all columns if the grid has multiple
if (grid.ColumnDefinitions.Count > 1)
    WGrid.SetColumnSpan(platformView, grid.ColumnDefinitions.Count);

grid.Children.Add(platformView);
```

## Content change detection on NavigationView

`NavigationView` extends `ContentControl`. Watch for content changes (e.g., when Shell switches items):

```csharp
var token = navigationView.RegisterPropertyChangedCallback(
    ContentControl.ContentProperty, (sender, dp) =>
    {
        var newContent = ((NavigationView)sender).Content as FrameworkElement;
        // React to content change
    });
```

## Handler vs Renderer extension points

| Pattern | Renderer (iOS/Android) | Handler (Windows) |
|---|---|---|
| Lifecycle hook | `ViewDidLoad`, `OnCreateView` | `ConnectHandler` / `DisconnectHandler` |
| Customize sub-views | `CreateShellItemRenderer()` override | `RegisterPropertyChangedCallback` on Content |
| React to property changes | Override `OnElementPropertyChanged` | Property mapper or DependencyProperty callbacks |
| Access platform view | `this` (renderer IS the view) | `PlatformView` property |
| Access MAUI element | `Element` property | `VirtualView` property |
