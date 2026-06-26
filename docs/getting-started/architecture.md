# How FlagstoneUI Works

This page explains the model behind FlagstoneUI so you know which knobs to reach for. If you just want to start building, the [Quickstart](quickstart.md) gets you there faster.

## The styling plane

FlagstoneUI is a .NET MAUI UI kit that provides a **unified styling plane** — enhanced controls whose visual properties are all exposed as `BindableProperty`s, so you can style them entirely from shared code using standard .NET MAUI patterns. No renderers, no handlers, no platform quirks.

The model is deliberately small:

```
Controls → Styles → Themes
```

- **Controls** (`FsButton`, `FsEntry`, `FsCard`, `FsEditor`, `FsBorder`, `FsShell`) expose full visual control.
- **Styles** apply values to those controls using any standard MAUI mechanism — inline, `StaticResource`, `DynamicResource`, implicit, or explicit.
- **Themes** are just collections of styles (a `ResourceDictionary`) bundled together.

That's it. Everything below is detail on those three layers.

## The styling surface

The reason FlagstoneUI exists: on stock MAUI controls, many visual properties simply aren't reachable from shared code, so a custom border or corner radius on an `Entry` drops you into per-platform handlers. FlagstoneUI's controls expose those properties directly.

```csharp
public partial class FsButton : Button
{
    public static readonly BindableProperty CornerRadiusProperty = ...;
    public static readonly BindableProperty BorderColorProperty = ...;
    public static readonly BindableProperty BorderWidthProperty = ...;
    // ... full visual surface, styleable from XAML or C#
}
```

Most controls reuse MAUI's standard handlers and self-register, so no setup is required to use them. The one exception is **`FsShell`**: because Shell still uses MAUI's legacy renderer model, its renderers must be registered at startup with `UseFlagstoneUI()` in `MauiProgram.cs`. See the [FsShell guide](your-first-shell-app.md).

## Styling approaches

FlagstoneUI supports the full range of MAUI styling. Pick whatever fits the job — they're all valid, and you can mix them.

**Direct values** — great for prototypes and one-offs:
```xml
<fs:FsButton Text="Submit" BackgroundColor="#6750A4" CornerRadius="12" />
```

**App resources** — share values without a full theme:
```xml
<Color x:Key="PrimaryColor">#6750A4</Color>
<fs:FsButton Text="Submit" BackgroundColor="{StaticResource PrimaryColor}" />
```

**Implicit styles** — a default look applied automatically to every instance:
```xml
<Style TargetType="fs:FsButton">
    <Setter Property="BackgroundColor" Value="#6750A4" />
    <Setter Property="CornerRadius" Value="12" />
</Style>

<fs:FsButton Text="Submit" />   <!-- styled automatically -->
```

**Explicit (named) styles** — variants you opt into:
```xml
<Style x:Key="OutlinedButton" TargetType="fs:FsButton">
    <Setter Property="BackgroundColor" Value="Transparent" />
    <Setter Property="BorderColor" Value="#6750A4" />
    <Setter Property="BorderWidth" Value="1" />
</Style>

<fs:FsButton Text="Cancel" Style="{StaticResource OutlinedButton}" />
```

**Design tokens** — `DynamicResource` references to named values for consistency and runtime switching:
```xml
<fs:FsButton
    BackgroundColor="{DynamicResource Color.Primary}"
    CornerRadius="{DynamicResource Radius.Button.Medium}" />
```

See the [Design Tokens reference](../reference/tokens.md) for the token catalogue.

## Themes

A **theme** is just a `ResourceDictionary` of styles for FlagstoneUI controls — typically implicit styles for the default look, plus named explicit styles for variants:

```xml
<ResourceDictionary xmlns="..." xmlns:fs="...">
    <!-- default look for every FsButton -->
    <Style TargetType="fs:FsButton">
        <Setter Property="BackgroundColor" Value="#6750A4" />
        <Setter Property="TextColor" Value="White" />
        <Setter Property="CornerRadius" Value="12" />
    </Style>

    <!-- a named variant -->
    <Style x:Key="OutlinedButton" TargetType="fs:FsButton">
        <Setter Property="BackgroundColor" Value="Transparent" />
        <Setter Property="BorderColor" Value="#6750A4" />
        <Setter Property="BorderWidth" Value="1" />
    </Style>
</ResourceDictionary>
```

A theme's setters can use direct values, app resources, or design tokens — that's an authoring choice, not a requirement. You apply a theme by merging it into your `App.xaml`:

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <local:MyTheme />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

Because tokens are `DynamicResource` references, swapping the merged dictionary at runtime re-themes a running app. For authoring your own theme, see the [Theming Guide](../guides/theming-guide.md).

## Project layout

```
FlagstoneUI.Core/
├── Builders/        # UseFlagstoneUI() registration (FsShell renderers)
├── Controls/        # FsButton, FsEntry, FsCard, FsEditor, FsBorder
│   └── Shell/       # FsShell, FsTabBar, FsTabBarBase, FsTabContext, FsLayout
├── Platforms/       # FsShell renderers per platform
├── Styles/          # built-in resources
└── Themes/          # theme loading utilities

FlagstoneUI.Integrations.MCT/   # optional Community Toolkit integrations
```

## Dependencies

- **.NET 10** and the **MAUI workload**.
- **CommunityToolkit.Maui** — optional, only needed if you use the `FlagstoneUI.Integrations.MCT` package. See [MCT Integration](../integrations/mct-integrations.md).
