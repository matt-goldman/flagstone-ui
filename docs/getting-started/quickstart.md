# Quickstart Guide

Welcome to FlagstoneUI! This guide will help you quickly get started building beautiful, consistent .NET MAUI applications with FlagstoneUI's enhanced controls and flexible styling options.

## What is FlagstoneUI?

FlagstoneUI is a cross-platform, open-source **UI kit** for .NET MAUI that provides a unified styling plane—controls with full visual control from shared code.

FlagstoneUI solves the problem of .NET MAUI's limited styling surface by providing enhanced controls where **all visual properties are exposed as BindableProperties**. This allows you to:

- **Style from shared code**: Use standard .NET MAUI styling patterns (inline values, styles, resources) without platform handlers
- **Full visual control**: Access borders, corner radius, padding, colors—everything you need
- **Flexible approaches**: Direct values, app resources, implicit styles, or design tokens—choose what fits your project
- **Consistent cross-platform**: Same API and behavior on iOS, Android, Windows, and macOS

**Key Value Proposition**: FlagstoneUI closes the gap in .NET MAUI's styling surface, giving you the control that web developers have with CSS, but in .NET MAUI.

## Architecture Overview

FlagstoneUI uses a simple, focused architecture:

```
┌─────────────────────────────────────┐
│         Your Application            │
│    (Pages, ViewModels, Logic)       │
└─────────────────────────────────────┘
               ↓ uses
┌─────────────────────────────────────┐
│    FlagstoneUI Controls             │
│  (FsButton, FsCard, FsEntry, etc.)  │
│   with full styling properties      │
└─────────────────────────────────────┘
               ↓ styled by
┌─────────────────────────────────────┐
│   Styles (inline, explicit,         │
│   implicit, or resource-based)      │
└─────────────────────────────────────┘
               ↓ organized in
┌─────────────────────────────────────┐
│          Themes                     │
│   (Collections of styles)           │
└─────────────────────────────────────┘
```

**Key Concepts:**

1. **Controls**: FlagstoneUI controls (prefixed with `Fs*`) expose all visual properties
2. **Styles**: Use standard .NET MAUI styling—inline, StaticResource, DynamicResource, implicit/explicit styles
3. **Themes**: Collections of styles (can use direct values, app resources, or design tokens)
4. **Flexibility**: Choose the styling approach that fits your project

## Prerequisites

Before you begin, ensure you have:

- **.NET 10 SDK** or later ([download here](https://dotnet.microsoft.com/download/dotnet/10.0))
- **MAUI Workload** installed: `dotnet workload install maui`
- A .NET MAUI project (new or existing)

## Installation

FlagstoneUI is available on NuGet. Add it to your MAUI project:

```bash
# Required — the core controls and styling surface
dotnet add package FlagstoneUI.Core

# Optional — Community Toolkit integrations (validation adapter, animated editor border)
dotnet add package FlagstoneUI.Integrations.MCT
```

Or add the package reference directly to your `.csproj`:

```xml
<ItemGroup>
  <PackageReference Include="FlagstoneUI.Core" Version="2.0.4" />
</ItemGroup>
```

That's everything you need to start using the controls — themes are optional (covered below).

## Basic Setup

### Step 1: Add FlagstoneUI Controls Namespace

In any page where you want to use Flagstone controls, add the namespace:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:fs="clr-namespace:FlagstoneUI.Core.Controls;assembly=FlagstoneUI.Core"
             x:Class="YourApp.MainPage">

    <!-- Your content here -->

</ContentPage>
```

### Step 2: Use Flagstone Controls

Now you can use Flagstone controls with direct styling:

```xml
<VerticalStackLayout Padding="16" Spacing="16">

    <!-- Direct styling - this is valid FlagstoneUI -->
    <fs:FsButton
        Text="Submit"
        BackgroundColor="#6750A4"
        TextColor="White"
        CornerRadius="12"
        Padding="16,8"
        Clicked="OnButtonClicked" />

    <!-- Card with direct styling -->
    <fs:FsCard
        BackgroundColor="#FAFAFA"
        CornerRadius="12"
        Padding="16">
        <VerticalStackLayout Spacing="8">
            <Label Text="Welcome to FlagstoneUI"
                   FontSize="20"
                   FontAttributes="Bold" />
            <Label Text="Build beautiful apps with full visual control" />
        </VerticalStackLayout>
    </fs:FsCard>

    <!-- Text Entry with styling -->
    <fs:FsEntry
        Placeholder="Enter your name"
        BorderBrush="#CCCCCC"
        BorderWidth="1"
        CornerRadius="8"
        Padding="12,8" />

</VerticalStackLayout>
```

**This is completely valid FlagstoneUI.** No themes required. No tokens required. Just styled controls.

## Styling Approaches

FlagstoneUI supports multiple valid styling approaches. Choose what fits your project:

### Approach 1: Direct Styling (Simple Projects)

Perfect for prototypes and simple apps:

```xml
<fs:FsButton
    Text="Click Me"
    BackgroundColor="#6750A4"
    CornerRadius="12" />
```

### Approach 2: Theme-Based (Recommended for Most Apps)

Define implicit styles in a theme for consistent styling:

```xml
<!-- In Theme.xaml or App.xaml -->
<Style TargetType="fs:FsButton">
    <Setter Property="BackgroundColor" Value="#6750A4" />
    <Setter Property="TextColor" Value="White" />
    <Setter Property="CornerRadius" Value="12" />
</Style>

<!-- Usage - styles applied automatically -->
<fs:FsButton Text="Submit" />
```

### Optional: Using a Pre-Built Theme

You can also use pre-built themes like Material. In your `App.xaml`:

```xml
<?xml version="1.0" encoding="UTF-8" ?>
<Application xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:material="clr-namespace:FlagstoneUI.Themes.Material;assembly=FlagstoneUI.Themes.Material"
             x:Class="YourApp.App">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <!-- Your existing styles -->
                <ResourceDictionary Source="Resources/Styles/Colors.xaml" />
                <ResourceDictionary Source="Resources/Styles/Styles.xaml" />

                <!-- Add Material theme (optional) -->
                <material:Theme />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

> **Note**: The Material theme is an example that uses design tokens internally. Themes are optional—you can style FlagstoneUI controls directly or create your own theme.

> **Important Requirements**:
> - You must add a **project reference** to `FlagstoneUI.Themes.Material` in your `.csproj`
> - You must declare the **XAML namespace** (the `xmlns:material=...` line shown above)
> - The theme will take advantage of .NET 10's global XAML namespaces and implicit usings in future updates

> **Note**: The Material theme is provided to showcase FlagstoneUI's capabilities. The themes demonstrate the concept and are value-add components. The core value of FlagstoneUI is the token-based theming framework itself, which allows you to create or use any theme that fits your needs.

## Using Control Variants

Themes typically provide several style variants for each control. For example, the Material theme includes these button variants:

### Button Variants (from Material Theme)

```xml
<!-- Filled Button (default) -->
<fs:FsButton Text="Filled Button" />

<!-- Outlined Button -->
<fs:FsButton Text="Outlined Button"
             Style="{StaticResource OutlinedButton}" />

<!-- Text Button -->
<fs:FsButton Text="Text Button"
             Style="{StaticResource TextButton}" />

<!-- Tonal Button -->
<fs:FsButton Text="Tonal Button"
             Style="{StaticResource TonalButton}" />
```

### Card Variants (from Material Theme)

```xml
<!-- Filled Card (default) -->
<fs:FsCard>
    <Label Text="Default filled card" />
</fs:FsCard>

<!-- Outlined Card -->
<fs:FsCard Style="{StaticResource OutlinedCard}">
    <Label Text="Card with border" />
</fs:FsCard>

<!-- Elevated Card -->
<fs:FsCard Style="{StaticResource ElevatedCard}">
    <Label Text="Card with shadow" />
</fs:FsCard>
```

### Entry Variants (from Material Theme)

```xml
<!-- Filled Entry (default) -->
<fs:FsEntry Placeholder="Filled Entry" />

<!-- Outlined Entry -->
<fs:FsEntry Placeholder="Outlined Entry"
            Style="{StaticResource OutlinedEntry}" />
```

> **Note**: These variants are defined by the Material theme. Your custom theme can define its own variants with different names and styling.

## Using Design Tokens

You can reference design tokens directly in your custom styles using `DynamicResource`:

```xml
<Label Text="Custom Styled Text"
       TextColor="{DynamicResource Color.Primary}"
       FontSize="{DynamicResource FontSize.TitleLarge}" />

<BoxView BackgroundColor="{DynamicResource Color.PrimaryContainer}"
         HeightRequest="100"
         CornerRadius="{DynamicResource Radius.Medium}" />
```

### Common Token Categories

- **Colors**: `Color.Primary`, `Color.Secondary`, `Color.Surface`, `Color.Error`, etc.
- **Spacing**: `Space.8`, `Space.16`, `Space.24`, etc.
- **Typography**: `FontSize.BodyMedium`, `FontSize.TitleLarge`, etc.
- **Radii**: `Radius.Small`, `Radius.Medium`, `Radius.Large`, etc.
- **Elevation**: `Elevation.Level1`, `Elevation.Level2`, etc.

For a complete list of available tokens, see the [Token Reference Documentation](../reference/tokens.md).

## Bonus: Runtime Theme Switching

Here's something cool - you can even switch themes at runtime if your app needs it! While the primary use case for FlagstoneUI is theming an app according to your desired look and feel, the token-based architecture enables dynamic theme switching:

```csharp
// In your App.xaml.cs
public static void SwitchTheme(string themeName)
{
    // Clear existing themes
    Current!.Resources.MergedDictionaries.Clear();

    // Add your base styles
    Current.Resources.MergedDictionaries.Add(new YourApp.Resources.Styles.Colors());
    Current.Resources.MergedDictionaries.Add(new YourApp.Resources.Styles.Styles());

    // Add the new theme
    switch (themeName)
    {
        case "Material":
            Current.Resources.MergedDictionaries.Add(new Themes.Material.Theme());
            break;
        case "YourCustomTheme":
            Current.Resources.MergedDictionaries.Add(new YourCustomTheme());
            break;
    }
}
```

This can be useful for multi-tenant applications, A/B testing different designs, or providing theme options to users.

## Creating Custom Variants

You can create custom style variants in your theme that build on Flagstone tokens:

```xml
<Style x:Key="AccentButton" TargetType="fs:FsButton">
    <Setter Property="BackgroundColor" Value="{DynamicResource Color.Tertiary}" />
    <Setter Property="TextColor" Value="{DynamicResource Color.OnTertiary}" />
    <Setter Property="CornerRadius" Value="{DynamicResource Radius.Large}" />
    <Setter Property="Padding" Value="{DynamicResource Space.16}" />
</Style>
```

Use your custom style:

```xml
<fs:FsButton Text="Custom Styled Button"
             Style="{StaticResource AccentButton}" />
```

## Example: Complete Page

Here's a complete example showing various FlagstoneUI controls working together:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:fs="clr-namespace:FlagstoneUI.Core.Controls;assembly=FlagstoneUI.Core"
             x:Class="YourApp.SignInPage"
             BackgroundColor="{DynamicResource Color.Background}">

    <ScrollView>
        <VerticalStackLayout Padding="24" Spacing="24">

            <!-- Header -->
            <Label Text="Welcome Back"
                   FontSize="{DynamicResource FontSize.HeadlineLarge}"
                   TextColor="{DynamicResource Color.OnBackground}"
                   HorizontalOptions="Center"
                   Margin="0,32,0,0" />

            <Label Text="Sign in to continue"
                   FontSize="{DynamicResource FontSize.BodyMedium}"
                   TextColor="{DynamicResource Color.OnSurfaceVariant}"
                   HorizontalOptions="Center" />

            <!-- Sign In Card -->
            <fs:FsCard Style="{StaticResource ElevatedCard}">
                <VerticalStackLayout Spacing="16">

                    <fs:FsEntry Placeholder="Email"
                                Keyboard="Email" />

                    <fs:FsEntry Placeholder="Password"
                                IsPassword="True" />

                    <fs:FsButton Text="Sign In"
                                 Clicked="OnSignInClicked" />

                    <fs:FsButton Text="Create Account"
                                 Style="{StaticResource OutlinedButton}"
                                 Clicked="OnCreateAccountClicked" />

                    <fs:FsButton Text="Forgot Password?"
                                 Style="{StaticResource TextButton}"
                                 Clicked="OnForgotPasswordClicked"
                                 HorizontalOptions="Center" />

                </VerticalStackLayout>
            </fs:FsCard>

        </VerticalStackLayout>
    </ScrollView>

</ContentPage>
```

## Next Steps

Now that you have the basics:

1. **Explore Controls**: Check out the [sample app](../../samples/FlagstoneUI.SampleApp/) to see all available controls and styles
2. **Learn About Tokens**: Read the [Token Reference](../reference/tokens.md) to understand the complete token system
3. **Create Custom Themes**: See the [Theming Guide](../guides/theming-guide.md) for designers and theme creators
4. **View Control Documentation**:
   - [FsButton](../controls/FsButton.md)
   - [FsCard](../controls/FsCard.md)
   - [FsEntry](../controls/FsEntry.md)
5. **Architecture Details**: Read the [Architecture Documentation](architecture.md) for deeper technical insights

## Optional: MAUI Community Toolkit Integration

FlagstoneUI is designed to work well with the [MAUI Community Toolkit](https://github.com/CommunityToolkit/Maui), which provides additional behaviors, converters, and functionality.

**Note**: CommunityToolkit.Maui is **not** a required dependency of FlagstoneUI. Add it to your application project only if you need its features:

```bash
dotnet add package CommunityToolkit.Maui
```

Then initialize it in your `MauiProgram.cs`:

```csharp
builder.UseMauiApp<App>()
    .UseMauiCommunityToolkit();
```

**Common use cases with FlagstoneUI:**
- Email/URL validation on `FsEntry` using `EmailValidationBehavior`
- Converters for advanced binding scenarios
- Additional UI enhancements and animations

See [ADR001](../archive/decisions/adr001-fsentry-behavior.md) for more details on the FlagstoneUI + MCT integration approach.

## Troubleshooting

### Theme Not Applied

If your controls don't have the expected styling:

1. Verify the theme ResourceDictionary is in your `App.xaml` MergedDictionaries
2. Ensure you have the correct XAML namespace declaration (e.g., `xmlns:material=...`)
3. Check that you've added the project reference to the theme library
4. Clean and rebuild your solution

### Controls Not Found

If you get "Type not found" errors:

1. Verify the Flagstone.Core namespace is declared in your XAML:
   ```xml
   xmlns:fs="clr-namespace:FlagstoneUI.Core.Controls;assembly=FlagstoneUI.Core"
   ```
2. Ensure you have a project reference to FlagstoneUI.Core
3. Check that the control name is spelled correctly (e.g., `FsButton` not `FsButton`)

### Build Errors

If you encounter build errors:

1. Ensure you have .NET 10 SDK installed (minimum version from `global.json`)
2. Install the MAUI workload: `dotnet workload install maui`
3. Restore packages: `dotnet restore`
4. Clean and rebuild: `dotnet clean && dotnet build`

## Getting Help

- **Issues**: Report bugs or request features on [GitHub Issues](https://github.com/matt-goldman/flagstone-ui/issues)
- **Discussions**: Join conversations on [GitHub Discussions](https://github.com/matt-goldman/flagstone-ui/discussions)
- **Documentation**: Browse the [full documentation](../index.md)

## Minimal Example from Scratch

If you're starting a new project:

```bash
# Create new MAUI app
dotnet new maui -n MyFlagstoneApp
cd MyFlagstoneApp

# Add FlagstoneUI from NuGet
dotnet add package FlagstoneUI.Core

# Build and run
dotnet build
dotnet run
```

Then modify `App.xaml` and your pages as shown above to start using FlagstoneUI controls.

---

Happy coding with FlagstoneUI! 🚀
