# Quickstart Guide

Welcome to Flagstone UI! This guide will help you quickly get started building beautiful, consistent .NET MAUI applications with Flagstone UI's themeable controls and token-based styling system.

## What is Flagstone UI?

Flagstone UI is a cross-platform, open-source **themeable framework** for .NET MAUI that enables developers to easily style controls using a token-based API, similar to how web developers use CSS.

Unlike traditional control libraries, Flagstone UI's primary purpose is to **surface all styling properties via a common token-based API**, allowing you to:

- **Style at the API layer**: Use .NET MAUI's API layer for styling instead of dropping down to platform-specific Handler configuration
- **Token-based theming**: Apply consistent styling across your app using design tokens
- **Community-driven themes**: Share and use themes created by the community (like Bootstrap for .NET MAUI)
- **Framework controls**: Built-in controls (FsButton, FsCard, FsEntry) that work seamlessly with the token system
- **Dark mode support**: Automatic dark mode through token bindings

**Key Value Proposition**: Flagstone UI gives you the flexibility and control that web developers have with CSS, with a token system that ensures consistency and makes theming straightforward.

## Architecture Overview

Flagstone UI uses a layered architecture:

```
┌─────────────────────────────────────┐
│         Your Application            │
│    (Pages, ViewModels, Logic)       │
└─────────────────────────────────────┘
              ↓ uses
┌─────────────────────────────────────┐
│       Flagstone Controls            │
│  (FsButton, FsCard, FsEntry, etc.)  │
└─────────────────────────────────────┘
              ↓ styled by
┌─────────────────────────────────────┐
│          Theme Layer                │
│   (Material, Custom Themes, etc.)   │
└─────────────────────────────────────┘
              ↓ consumes
┌─────────────────────────────────────┐
│        Design Tokens                │
│ (Colors, Spacing, Typography, etc.) │
└─────────────────────────────────────┘
```

**Key Concepts:**

1. **Design Tokens**: Named values (like `Color.Primary`, `Space.16`) defined in the base token system
2. **Themes**: Collections of styles that reference tokens using `DynamicResource` bindings
3. **Controls**: Flagstone controls (prefixed with `Fs*`) that are designed to work seamlessly with themes
4. **Your App**: Uses Flagstone controls and can override theme values as needed

## Prerequisites

Before you begin, ensure you have:

- **.NET 10 SDK** or later ([download here](https://dotnet.microsoft.com/download/dotnet/10.0))
- **MAUI Workload** installed: `dotnet workload install maui`
- A .NET MAUI project (new or existing)

## Installation

> **Note**: Flagstone UI is currently in development. NuGet packages will be available in future releases.

For now, clone the repository and reference the projects directly:

```bash
git clone https://github.com/matt-goldman/flagstone-ui.git
```

Then add project references to your `.csproj` file:

```xml
<ItemGroup>
  <ProjectReference Include="..\flagstone-ui\src\FlagstoneUI.Core\FlagstoneUI.Core.csproj" />
  <ProjectReference Include="..\flagstone-ui\src\FlagstoneUI.Themes.Material\FlagstoneUI.Themes.Material.csproj" />
</ItemGroup>
```

## Basic Setup

### Step 1: Add Theme to Your Application

In your `App.xaml` file, add your theme to your application resources. In this example, we'll use the Material theme (included as a demonstration), but you would typically use your own custom theme or one from the community:

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
                
                <!-- Add your Flagstone UI theme (Material theme shown as example) -->
                <material:Theme />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

> **Note**: The Material theme is provided to showcase Flagstone UI's capabilities. The themes demonstrate the concept and are value-add components. The core value of Flagstone UI is the token-based theming framework itself, which allows you to create or use any theme that fits your needs.

### Step 2: Add Namespace to Your Pages

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

### Step 3: Use Flagstone Controls

Now you can use Flagstone controls in your XAML:

```xml
<VerticalStackLayout Padding="16" Spacing="16">
    
    <!-- Filled Button (default style) -->
    <fs:FsButton Text="Primary Action" 
                 Clicked="OnButtonClicked" />
    
    <!-- Card with content -->
    <fs:FsCard>
        <VerticalStackLayout Spacing="8">
            <Label Text="Welcome to Flagstone UI" 
                   FontSize="20" 
                   FontAttributes="Bold" />
            <Label Text="Build beautiful apps with themeable controls" />
        </VerticalStackLayout>
    </fs:FsCard>
    
    <!-- Text Entry -->
    <fs:FsEntry Placeholder="Enter your name" />
    
</VerticalStackLayout>
```

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

For a complete list of available tokens, see the [Token Reference Documentation](tokens.md).

## Bonus: Runtime Theme Switching

Here's something cool - you can even switch themes at runtime if your app needs it! While the primary use case for Flagstone UI is theming an app according to your desired look and feel, the token-based architecture enables dynamic theme switching:

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

Here's a complete example showing various Flagstone UI controls working together:

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

1. **Explore Controls**: Check out the [sample app](../samples/FlagstoneUI.SampleApp/) to see all available controls and styles
2. **Learn About Tokens**: Read the [Token Reference](tokens.md) to understand the complete token system
3. **Create Custom Themes**: See the [Theming Guide](theming-guide.md) for designers and theme creators
4. **View Control Documentation**: 
   - [FsButton](Controls/FsButton.md)
   - [FsCard](Controls/FsCard.md)
   - [FsEntry](Controls/FsEntry.md)
5. **Architecture Details**: Read the [Architecture Documentation](architecture.md) for deeper technical insights

## Getting Help

- **Issues**: Report bugs or request features on [GitHub Issues](https://github.com/matt-goldman/flagstone-ui/issues)
- **Discussions**: Join conversations on [GitHub Discussions](https://github.com/matt-goldman/flagstone-ui/discussions)
- **Documentation**: Browse the [full documentation](README.md)

## Minimal Example from Scratch

If you're starting a new project:

```bash
# Create new MAUI app
dotnet new maui -n MyFlagstoneApp
cd MyFlagstoneApp

# Add project references (adjust paths as needed)
dotnet add reference ../flagstone-ui/src/FlagstoneUI.Core/FlagstoneUI.Core.csproj
dotnet add reference ../flagstone-ui/src/FlagstoneUI.Themes.Material/FlagstoneUI.Themes.Material.csproj

# Build and run
dotnet build
dotnet run
```

Then modify `App.xaml` and your pages as shown above to start using Flagstone UI controls.

---

Happy coding with Flagstone UI! 🚀
