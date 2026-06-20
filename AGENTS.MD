# FlagstoneUI Agent Guide

> **For AI coding assistants and developers**: This guide helps you generate quality FlagstoneUI code by explaining the architecture, patterns, and best practices.

## Overview

**FlagstoneUI** is a cross-platform UI kit and framework for .NET MAUI that provides a unified styling plane for .NET MAUI—controls with full visual control from shared code, no platform handlers needed. It solves the problem of inconsistent visual styling across platforms by offering enhanced controls with comprehensive styling properties exposed through standard .NET MAUI patterns.

### Philosophy

- **Full Visual Control**: Controls expose all styling properties via BindableProperties—border, corner radius, padding, colors, etc.
- **Standard .NET MAUI Styling**: Use inline values, `StaticResource`, `DynamicResource`, explicit styles, or implicit styles—all approaches are valid
- **No Platform Code**: Style from shared code without handlers or renderers
- **Platform Neutrality**: Controls are designed to look and behave consistently across iOS, Android, Windows, and macOS
- **Flexibility**: Use direct values for simple cases, app resources for reusability, or design tokens for consistency—choose what fits your project

### When to Use FlagstoneUI

✅ **DO use FlagstoneUI when:**
- Building cross-platform apps that need consistent, custom visual styling
- Implementing a design system or brand identity across .NET MAUI
- You need controls with full visual customization without platform-specific code
- You want full control over borders, corner radius, padding, colors from shared code
- You need theming support with standard .NET MAUI patterns

❌ **DON'T use FlagstoneUI when:**
- You specifically want platform-native look and feel
- Your app has minimal UI customization needs
- You're targeting a single platform only

## Current Status

**Proof of Concept (POC)** - ~60% complete, evolving toward MVP.

**Available Controls:**
- `FsButton` - Fully styleable button with corner radius, borders, colors
- `FsCard` - Container with elevation (shadows), borders, corner radius
- `FsEntry` - Single-line text input with full visual control
- `FsEditor` - Multi-line text input with full visual control
- `FsShell` - Drop-in replacement for Shell that allows full customisation

**Available Themes:**
- Material Design 3 (`FlagstoneUI.Themes.Material`)
- Example themes in sample app

**Optional Integrations:**
- MAUI Community Toolkit integration (`FlagstoneUI.Integrations.MCT`)

## Architecture

### Package Structure

```
FlagstoneUI.Core
├── Controls/          # FsButton, FsCard, FsEntry, FsEditor
├── Styles/            # Tokens.xaml (optional design tokens)
└── Builders/          # FlagstoneUIBuilder (minimal, may be removed)

FlagstoneUI.Themes.Material
└── Theme.xaml         # Material theme example

FlagstoneUI.Integrations.MCT (optional)
├── Behaviors/         # ValidationBehaviorAdapter for FsEntry
└── Animations/        # FsEditorBorderAnimation for animated borders
```

### Core Architecture

FlagstoneUI's architecture is simple and focused:

```
Controls → Styles → Themes
```

That's it. This is the styling plane for .NET MAUI.

**What FlagstoneUI Provides:**
- Enhanced controls (`FsButton`, `FsCard`, `FsEntry`, `FsEditor`) with full visual control
- All styling properties exposed as BindableProperties
- Standard .NET MAUI styling patterns (inline, explicit styles, implicit styles)
- Themes as collections of styles

### Optional: Design Tokens

Some themes (like Material) use design tokens as an implementation detail to organize style values:

**Token Categories (Material theme example):**
- **Colors**: `Color.Primary`, `Color.OnPrimary`, `Color.Surface`, `Color.Error`, etc.
- **Spacing**: `Space.0`, `Space.8`, `Space.16`, `Space.24`, etc.
- **Border Radius**: `Radius.Small`, `Radius.Medium`, `Radius.Large` (Double), `Radius.Button.Small`, `Radius.Button.Medium` (Int32 for Button.CornerRadius)
- **Typography**: `FontSize.BodyLarge`, `FontSize.TitleMedium`, `FontSize.HeadlineLarge`, etc.
- **Border Width**: `BorderWidth.Thin`, `BorderWidth.Thick`, etc.
- **Elevation**: `Elevation.Level0`, `Elevation.Level1`, `Elevation.Level2`, etc.

**Tokens are optional** — themes can use direct values, app resources, or any standard .NET MAUI pattern.

### Theme Configuration

**Important:** FlagstoneUI uses ResourceDictionary merging in `App.xaml` for theme configuration. There is **no builder pattern** (YAGNI principle).

**Correct theme setup in App.xaml:**

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
                
                <!-- Add FlagstoneUI theme -->
                <material:Theme />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

**Key Points:**
- Theme is added via `<material:Theme />` in MergedDictionaries
- Requires XAML namespace declaration: `xmlns:material="clr-namespace:FlagstoneUI.Themes.Material;assembly=FlagstoneUI.Themes.Material"`
- Requires project reference to the theme package
- Themes automatically merge core tokens

**Alternative: Define Theme Directly in Your App:**

You can also define a theme ResourceDictionary directly in your app alongside existing resource dictionaries:

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="Resources/Styles/Colors.xaml" />
            <ResourceDictionary Source="Resources/Styles/Styles.xaml" />
            <ResourceDictionary Source="Resources/Styles/YourTheme.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

This is a common pattern for custom themes that are specific to your application.

## Code Generation Patterns

### Styling Approaches

FlagstoneUI supports multiple valid styling approaches. Choose the one that fits your project:

#### Approach 1: Direct Styling (Simple Projects)

```xml
<!-- ✅ VALID: Direct values for simple cases -->
<fs:FsButton 
    Text="Submit"
    BackgroundColor="#6750A4"
    TextColor="White"
    CornerRadius="12"
    Padding="16,8" />

<fs:FsCard 
    BackgroundColor="#FAFAFA"
    CornerRadius="12"
    Padding="16">
    <Label Text="Card Content" />
</fs:FsCard>
```

**When to use:** Prototypes, simple apps, one-off styling needs.

#### Approach 2: Theme-Based with Implicit Styles (Recommended for Most Apps)

```xml
<!-- ✅ RECOMMENDED: Define implicit styles in your theme -->
<!-- In Theme.xaml or App.xaml -->
<Style TargetType="fs:FsButton">
    <Setter Property="BackgroundColor" Value="#6750A4" />
    <Setter Property="TextColor" Value="White" />
    <Setter Property="CornerRadius" Value="12" />
</Style>

<!-- Usage - styles applied automatically -->
<fs:FsButton Text="Submit" />
```

**When to use:** Most applications, consistent branding, reusable styles.

#### Approach 3: Explicit/Named Styles (Control Variants)

```xml
<!-- ✅ COMMON IN THEMES: Named variants for different appearances -->
<Style TargetType="fs:FsButton">
    <Setter Property="BackgroundColor" Value="#6750A4" />
    <Setter Property="TextColor" Value="White" />
</Style>

<Style x:Key="OutlinedButton" TargetType="fs:FsButton">
    <Setter Property="BackgroundColor" Value="Transparent" />
    <Setter Property="BorderColor" Value="#6750A4" />
    <Setter Property="BorderWidth" Value="1" />
</Style>

<Style x:Key="DeleteButton" TargetType="fs:FsButton">
    <Setter Property="BackgroundColor" Value="#DC3545" />
    <Setter Property="TextColor" Value="White" />
</Style>

<!-- Usage -->
<fs:FsButton Text="Submit" />  <!-- Uses implicit style -->
<fs:FsButton Text="Cancel" Style="{StaticResource OutlinedButton}" />
<fs:FsButton Text="Delete" Style="{StaticResource DeleteButton}" />
```

**When to use:** Most themes. Provides multiple visual or semantic variants while maintaining consistency.

**Common patterns for explicit styles:**
- Visual variants: `OutlinedButton`, `RoundedEntry`, `ElevatedCard`
- Semantic variants: `PurchaseButton`, `DeleteButton`, `WarningCard`
- Size variants: `SmallButton`, `LargeCard`

#### Approach 4: App Resources (Reusable Values)

```xml
<!-- Define reusable colors/values in App.xaml -->
<Application.Resources>
    <Color x:Key="PrimaryColor">#6750A4</Color>
    <Color x:Key="SurfaceColor">#FAFAFA</Color>
    <x:Double x:Key="StandardCornerRadius">12</x:Double>
</Application.Resources>

<!-- Use with StaticResource -->
<fs:FsButton 
    Text="Submit"
    BackgroundColor="{StaticResource PrimaryColor}"
    CornerRadius="{StaticResource StandardCornerRadius}" />
```

**When to use:** Reusable values across the app, simple theming.

#### Approach 5: Design Tokens (Design System Consistency)

```xml
<!-- ✅ ALSO VALID: Using design tokens when available -->
<fs:FsButton 
    Text="Submit"
    BackgroundColor="{DynamicResource Color.Primary}"
    TextColor="{DynamicResource Color.OnPrimary}"
    CornerRadius="{DynamicResource Radius.Button.Medium}"
    Padding="{DynamicResource Space.16}" />

<fs:FsCard 
    BackgroundColor="{DynamicResource Color.Surface}"
    CornerRadius="{DynamicResource Radius.Medium}"
    Padding="{DynamicResource Space.16}">
    <Label Text="Card Content" />
</fs:FsCard>
```

**When to use:** Design system driven apps, token-based themes (like Material), dynamic theme switching.

### ✅ DO: Choose the Appropriate Approach

**Decision Guide:**
```
├─ Simple app or prototype? → Direct values (Approach 1)
├─ Theme-based app? → Implicit styles + explicit variants (Approaches 2 & 3)
├─ Reusable values? → App resources (Approach 4)
└─ Design system or existing token-based theme? → Design tokens (Approach 5)
```

All approaches are valid. **Most themes combine implicit styles (for defaults) with explicit styles (for variants)**.

### When Using Design Tokens: Semantic Color Usage

If you're using a token-based theme (like Material), choose tokens based on semantic meaning:

```xml
<!-- ✅ CORRECT: Semantic token usage (if using tokens) -->
<fs:FsButton Text="Delete" 
             BackgroundColor="{DynamicResource Color.Error}"
             TextColor="{DynamicResource Color.OnError}" />

<Label Text="Success!" 
       TextColor="{DynamicResource Color.Primary}" />

<fs:FsCard BackgroundColor="{DynamicResource Color.Surface}">
    <Label Text="Card content" 
           TextColor="{DynamicResource Color.OnSurface}" />
</fs:FsCard>
```

**Color Role Guidelines (Material theme):**
- **Primary**: Main brand actions, key UI elements
- **Secondary**: Supporting actions, less prominent elements
- **Tertiary**: Accent color for visual hierarchy
- **Error**: Error states, destructive actions
- **Surface**: Card backgrounds, elevated surfaces
- **Background**: Main page/screen background
- **On[Color]**: Text/icons displayed on the named color (e.g., `OnPrimary` on `Primary`)

### ✅ DO: Use Theme Style Variants

Themes provide named style variants. Use them instead of custom inline styling:

```xml
<!-- ✅ CORRECT: Using theme style variants -->
<fs:FsButton Text="Primary Action" />  <!-- Default filled button -->
<fs:FsButton Text="Secondary Action" Style="{StaticResource OutlinedButton}" />
<fs:FsButton Text="Tertiary Action" Style="{StaticResource TextButton}" />

<fs:FsCard>  <!-- Default filled card -->
    <Label Text="Default card" />
</fs:FsCard>
<fs:FsCard Style="{StaticResource OutlinedCard}">
    <Label Text="Outlined card" />
</fs:FsCard>
<fs:FsCard Style="{StaticResource ElevatedCard}">
    <Label Text="Elevated card with shadow" />
</fs:FsCard>

<fs:FsEntry Placeholder="Email" />  <!-- Default filled entry -->
<fs:FsEntry Placeholder="Name" Style="{StaticResource OutlinedEntry}" />
```

**Material Theme Style Variants:**
- **Buttons**: Default (filled), `OutlinedButton`, `TextButton`, `TonalButton`, `SecondaryButton`, `TertiaryButton`
- **Cards**: Default (filled), `OutlinedCard`, `ElevatedCard`
- **Entry/Editor**: Default (filled), `OutlinedEntry`, `OutlinedEditor`

### ✅ DO: Use Namespace Prefix for Controls

Always use the `fs:` prefix for FlagstoneUI controls:

```xml
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:fs="clr-namespace:FlagstoneUI.Core.Controls;assembly=FlagstoneUI.Core"
             x:Class="YourApp.MainPage">
    
    <fs:FsButton Text="Click Me" />
    <fs:FsCard>
        <fs:FsEntry Placeholder="Enter text" />
    </fs:FsCard>
</ContentPage>
```

### ❌ DON'T: Mix Standard MAUI Controls with FlagstoneUI Controls

For consistency, prefer FlagstoneUI controls throughout your UI:

```xml
<!-- ❌ AVOID: Mixing standard and Fs controls -->
<VerticalStackLayout>
    <Button Text="Standard Button" />
    <fs:FsButton Text="Fs Button" />
    <Entry Placeholder="Standard Entry" />
    <fs:FsEntry Placeholder="Fs Entry" />
</VerticalStackLayout>

<!-- ✅ BETTER: Consistent control usage -->
<VerticalStackLayout>
    <fs:FsButton Text="Primary Action" />
    <fs:FsButton Text="Secondary Action" Style="{StaticResource OutlinedButton}" />
    <fs:FsEntry Placeholder="Email" />
    <fs:FsEntry Placeholder="Password" IsPassword="True" />
</VerticalStackLayout>
```

### ✅ DO: Use Per-Edge Borders When Needed

FlagstoneUI controls support per-edge borders for effects like underlines or 3D styling:

```xml
<!-- Bottom border only (underline effect) -->
<fs:FsEntry 
    Placeholder="Email"
    BorderBottomThickness="2"
    BorderBottomBrush="{DynamicResource Color.Primary}" />

<!-- 3D raised effect with different edge colors -->
<fs:FsCard 
    Padding="16"
    CornerRadius="8"
    BorderTopThickness="2"
    BorderLeftThickness="2"
    BorderRightThickness="2"
    BorderBottomThickness="2"
    BorderTopBrush="White"
    BorderLeftBrush="White"
    BorderRightBrush="Gray"
    BorderBottomBrush="Gray">
    <Label Text="3D effect card" />
</fs:FsCard>

<!-- Border shorthand syntax (uniform or TRBL) -->
<fs:FsCard Border="2 Blue" Padding="16">
    <Label Text="Uniform border using shorthand" />
</fs:FsCard>

<fs:FsCard Border="2 White, 2 Gray, 2 Gray, 2 White" Padding="16">
    <Label Text="TRBL borders using shorthand" />
</fs:FsCard>
```

**Border Properties:**
- **Uniform**: `BorderColor`/`BorderBrush` + `BorderWidth` (sets all four edges)
- **Per-Edge**: `BorderTopThickness`, `BorderRightThickness`, `BorderBottomThickness`, `BorderLeftThickness` + corresponding `Brush` properties
- **Shorthand**: `Border` property accepts "thickness color" syntax with 1, 2, or 4 values
- **Note**: FsCard uses `BorderColor`, FsEntry/FsEditor use `BorderBrush`

### ✅ DO: Apply Visual States Properly

Use Visual State Manager for interactive states:

```xml
<fs:FsButton Text="Interactive Button">
    <VisualStateManager.VisualStateGroups>
        <VisualStateGroup x:Name="CommonStates">
            <VisualState x:Name="Normal" />
            <VisualState x:Name="Disabled">
                <VisualState.Setters>
                    <Setter Property="Opacity" Value="{DynamicResource Opacity.Disabled}" />
                </VisualState.Setters>
            </VisualState>
        </VisualStateGroup>
    </VisualStateManager.VisualStateGroups>
</fs:FsButton>
```

## Form Validation Patterns

### Using MAUI Community Toolkit Validation

FlagstoneUI provides optional integration with MCT validators via the `FlagstoneUI.Integrations.MCT` package:

```xml
<ContentPage xmlns:fs="clr-namespace:FlagstoneUI.Core.Controls;assembly=FlagstoneUI.Core"
             xmlns:toolkit="http://schemas.microsoft.com/dotnet/2022/maui/toolkit">
    
    <ContentPage.Resources>
        <Style x:Key="ValidStyle" TargetType="fs:FsEntry">
            <Setter Property="BorderBrush" Value="{DynamicResource Color.Primary}" />
            <Setter Property="BorderWidth" Value="2" />
        </Style>
        
        <Style x:Key="InvalidStyle" TargetType="fs:FsEntry">
            <Setter Property="BorderBrush" Value="{DynamicResource Color.Error}" />
            <Setter Property="BorderWidth" Value="2" />
        </Style>
    </ContentPage.Resources>
    
    <!-- Email validation -->
    <fs:FsEntry Placeholder="Enter your email" Keyboard="Email">
        <fs:FsEntry.Behaviors>
            <toolkit:EmailValidationBehavior
                InvalidStyle="{StaticResource InvalidStyle}"
                ValidStyle="{StaticResource ValidStyle}"
                Flags="ValidateOnValueChanged" />
        </fs:FsEntry.Behaviors>
    </fs:FsEntry>
    
    <!-- Password validation (minimum length) -->
    <fs:FsEntry Placeholder="Password (min 8 chars)" IsPassword="True">
        <fs:FsEntry.Behaviors>
            <toolkit:CharactersValidationBehavior
                CharacterType="Any"
                MinimumCharacterCount="8"
                InvalidStyle="{StaticResource InvalidStyle}"
                ValidStyle="{StaticResource ValidStyle}"
                Flags="ValidateOnValueChanged" />
        </fs:FsEntry.Behaviors>
    </fs:FsEntry>
</ContentPage>
```

**Note:** MCT integration requires the `FlagstoneUI.Integrations.MCT` NuGet package. It's not a core dependency of FlagstoneUI.

**Available MCT Validators:**
- `EmailValidationBehavior` - Email validation
- `NumericValidationBehavior` - Numeric range validation
- `RequiredStringValidationBehavior` - Non-empty validation
- `TextValidationBehavior` - Regex-based validation
- `UriValidationBehavior` - URI/URL validation
- `CharactersValidationBehavior` - Character count validation
- `MultiValidationBehavior` - Combine multiple validators

## Common Patterns

### Sign-In Form

```xml
<ScrollView>
    <VerticalStackLayout Padding="24" Spacing="16">
        
        <Label Text="Welcome Back"
               FontSize="{DynamicResource FontSize.HeadlineLarge}"
               TextColor="{DynamicResource Color.OnBackground}"
               HorizontalOptions="Center" />
        
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
                
            </VerticalStackLayout>
        </fs:FsCard>
        
    </VerticalStackLayout>
</ScrollView>
```

### Card List

```xml
<CollectionView ItemsSource="{Binding Items}">
    <CollectionView.ItemTemplate>
        <DataTemplate>
            <fs:FsCard Margin="16,8">
                <Grid ColumnDefinitions="*,Auto" ColumnSpacing="12">
                    <VerticalStackLayout Grid.Column="0" Spacing="4">
                        <Label Text="{Binding Title}"
                               FontSize="{DynamicResource FontSize.TitleMedium}"
                               FontAttributes="Bold" />
                        <Label Text="{Binding Description}"
                               FontSize="{DynamicResource FontSize.BodyMedium}"
                               TextColor="{DynamicResource Color.OnSurfaceVariant}" />
                    </VerticalStackLayout>
                    
                    <fs:FsButton Grid.Column="1"
                                 Text="View"
                                 Style="{StaticResource OutlinedButton}"
                                 Command="{Binding ViewCommand}" />
                </Grid>
            </fs:FsCard>
        </DataTemplate>
    </CollectionView.ItemTemplate>
</CollectionView>
```

### Settings Screen

```xml
<ScrollView>
    <VerticalStackLayout Padding="16" Spacing="12">
        
        <Label Text="Settings"
               FontSize="{DynamicResource FontSize.HeadlineMedium}"
               Margin="0,0,0,8" />
        
        <!-- Account section -->
        <Label Text="Account"
               FontSize="{DynamicResource FontSize.TitleMedium}"
               TextColor="{DynamicResource Color.Primary}" />
        
        <fs:FsCard>
            <VerticalStackLayout Spacing="12">
                <fs:FsEntry Placeholder="Display Name"
                            Text="{Binding DisplayName}" />
                <fs:FsEntry Placeholder="Email"
                            Text="{Binding Email}"
                            Keyboard="Email" />
            </VerticalStackLayout>
        </fs:FsCard>
        
        <!-- Preferences section -->
        <Label Text="Preferences"
               FontSize="{DynamicResource FontSize.TitleMedium}"
               TextColor="{DynamicResource Color.Primary}"
               Margin="0,16,0,0" />
        
        <fs:FsCard>
            <VerticalStackLayout Spacing="12">
                <Grid ColumnDefinitions="*,Auto">
                    <Label Text="Notifications" VerticalOptions="Center" />
                    <Switch Grid.Column="1" IsToggled="{Binding NotificationsEnabled}" />
                </Grid>
                <Grid ColumnDefinitions="*,Auto">
                    <Label Text="Dark Mode" VerticalOptions="Center" />
                    <Switch Grid.Column="1" IsToggled="{Binding DarkModeEnabled}" />
                </Grid>
            </VerticalStackLayout>
        </fs:FsCard>
        
        <fs:FsButton Text="Save Changes"
                     Margin="0,16,0,0"
                     Command="{Binding SaveCommand}" />
        
    </VerticalStackLayout>
</ScrollView>
```

## Anti-Patterns (DO NOT)

### ❌ DON'T: Hardcode Colors or Spacing

```xml
<!-- ❌ WRONG -->
<fs:FsButton BackgroundColor="#6750A4" Padding="24,10" />

<!-- ✅ CORRECT -->
<fs:FsButton 
    BackgroundColor="{DynamicResource Color.Primary}"
    Padding="{DynamicResource Space.24}" />
```

### ❌ DON'T: Override Token Values in Component Styles

```xml
<!-- ❌ WRONG: Hardcoded values in application styles break theming -->
<Style TargetType="fs:FsButton">
    <Setter Property="BackgroundColor" Value="#FF0000" />
</Style>

<!-- ✅ CORRECT: Use theme style variants or semantic tokens -->
<Style x:Key="DangerButton" TargetType="fs:FsButton">
    <Setter Property="BackgroundColor" Value="{DynamicResource Color.Error}" />
    <Setter Property="TextColor" Value="{DynamicResource Color.OnError}" />
</Style>
```

**Note**: Base themes (like Material theme) set token values and define base styles. This anti-pattern applies to application-level styles that consumers create.

### ❌ DON'T: Use Magic Numbers for Sizing

```xml
<!-- ❌ WRONG -->
<fs:FsCard Padding="14" CornerRadius="7" />

<!-- ✅ CORRECT -->
<fs:FsCard 
    Padding="{DynamicResource Space.16}"
    CornerRadius="{DynamicResource Radius.Small}" />
```

### ❌ DON'T: Use Platform-Specific Code for Styling

```xml
<!-- ❌ WRONG: Platform-specific styling breaks FlagstoneUI's purpose -->
<fs:FsEntry>
    <OnPlatform x:TypeArguments="Color" x:Key="EntryBackground">
        <On Platform="iOS" Value="#F0F0F0" />
        <On Platform="Android" Value="#E0E0E0" />
    </OnPlatform>
</fs:FsEntry>

<!-- ✅ CORRECT: Use tokens for consistent cross-platform styling -->
<fs:FsEntry Background="{DynamicResource Color.SurfaceVariant}" />
```

### ❌ DON'T: Mix BorderColor/BorderWidth with Per-Edge Properties

```xml
<!-- ❌ WRONG: Conflicts between uniform and per-edge properties -->
<fs:FsCard 
    BorderColor="Blue"
    BorderWidth="2"
    BorderBottomThickness="4"
    BorderBottomBrush="Red">
    <!-- Behavior is undefined -->
</fs:FsCard>

<!-- ✅ CORRECT: Choose one approach -->
<!-- Option 1: Uniform border -->
<fs:FsCard BorderColor="Blue" BorderWidth="2">
    <Label Text="Uniform border" />
</fs:FsCard>

<!-- Option 2: Per-edge borders -->
<fs:FsCard 
    BorderTopThickness="2"
    BorderBottomThickness="4"
    BorderTopBrush="Blue"
    BorderBottomBrush="Red">
    <Label Text="Different edges" />
</fs:FsCard>
```

### ❌ DON'T: Use Hardcoded Values in Custom Containers

```xml
<!-- ❌ WRONG: Hardcoded styling can't be themed -->
<ContentView BackgroundColor="#FFFFFF" Padding="16">
    <Label Text="Content" TextColor="#000000" />
</ContentView>

<!-- ✅ CORRECT: Use tokens for any custom containers -->
<ContentView 
    BackgroundColor="{DynamicResource Color.Surface}"
    Padding="{DynamicResource Space.16}">
    <Label Text="Content" 
           TextColor="{DynamicResource Color.OnSurface}" />
</ContentView>

<!-- ✅ ALSO CORRECT: Use FsCard for card-style containers -->
<fs:FsCard>
    <Label Text="Content" 
           TextColor="{DynamicResource Color.OnSurface}" />
</fs:FsCard>
```

**Note**: You can create custom controls and containers using standard MAUI controls. The key is to use design tokens instead of hardcoded values so your custom controls respect the theme.

## Advanced Patterns

### Runtime Theme Switching

While the primary use case is static theming, FlagstoneUI's token architecture enables runtime theme switching:

```csharp
// In App.xaml.cs
public static void SwitchTheme(string themeName)
{
    // Clear existing themes
    Current!.Resources.MergedDictionaries.Clear();
    
    // Add base styles
    Current.Resources.MergedDictionaries.Add(
        new YourApp.Resources.Styles.Colors());
    Current.Resources.MergedDictionaries.Add(
        new YourApp.Resources.Styles.Styles());
    
    // Add selected theme
    switch (themeName)
    {
        case "Material":
            Current.Resources.MergedDictionaries.Add(
                new FlagstoneUI.Themes.Material.Theme());
            break;
        case "CustomTheme":
            Current.Resources.MergedDictionaries.Add(
                new YourApp.Themes.CustomTheme());
            break;
    }
}
```

### Animated Borders with MCT Integration

The optional MCT integration provides animated gradient borders:

```xml
xmlns:fs="clr-namespace:FlagstoneUI.Core.Controls;assembly=FlagstoneUI.Core"
xmlns:anim="clr-namespace:FlagstoneUI.Integrations.MCT.Animations;assembly=FlagstoneUI.Integrations.MCT"

<ContentPage.Resources>
    <anim:FsEditorBorderAnimation x:Key="AiBorderAnimation" Length="3000">
        <anim:FsEditorBorderAnimation.Gradient>
            <LinearGradientBrush StartPoint="0,0" EndPoint="1,1">
                <GradientStop Color="Gold" Offset="0.0" />
                <GradientStop Color="Orange" Offset="0.25" />
                <GradientStop Color="Goldenrod" Offset="0.5" />
                <GradientStop Color="DarkGoldenrod" Offset="0.75" />
                <GradientStop Color="Gold" Offset="1.0" />
            </LinearGradientBrush>
        </anim:FsEditorBorderAnimation.Gradient>
    </anim:FsEditorBorderAnimation>
</ContentPage.Resources>

<fs:FsEditor 
    x:Name="AiEditor"
    Placeholder="🤖 Ask me anything..."
    Focused="OnAiEditorFocused"
    Unfocused="OnAiEditorUnfocused" />
```

```csharp
private CancellationTokenSource? _animationCts;

private async void OnAiEditorFocused(object? sender, EventArgs e)
{
    _animationCts = new CancellationTokenSource();
    var animation = (FsEditorBorderAnimation)Resources["AiBorderAnimation"];
    
    try
    {
        await animation.Animate(AiEditor, _animationCts.Token);
    }
    catch (OperationCanceledException) { }
}

private void OnAiEditorUnfocused(object? sender, EventArgs e)
{
    _animationCts?.Cancel();
}
```

## Available Tools

### CLI Tools

- **Bootstrap Theme Converter**: Available in the codebase but not yet published as a .NET CLI tool. Will be available as `flagstone-bootstrap convert --help` once published.
- **Tailwind Palette Converter**: Tool for converting Tailwind color palettes to FlagstoneUI tokens (planned)

### Integration Tools

- **Crosswind**: Steven Thewissen's Tailwind-to-MAUI utility classes project (external, compatible with FlagstoneUI)
- **Figma Converter**: Design token export from Figma to FlagstoneUI themes (future consideration per ADR009)

## Project Requirements

### Prerequisites

- **.NET 10 SDK** (minimum version specified in `global.json`)
- **MAUI Workload**: `dotnet workload install maui`

### Installation

**Via NuGet (Preview):**

```bash
dotnet add package FlagstoneUI.Core --version 0.0.1-preview1
dotnet add package FlagstoneUI.Themes.Material --version 0.0.1-preview1

# Optional MCT integration
dotnet add package FlagstoneUI.Integrations.MCT --version 0.0.1-preview1
```

**Via Project Reference (Development):**

```xml
<ItemGroup>
  <ProjectReference Include="..\flagstone-ui\src\FlagstoneUI.Core\FlagstoneUI.Core.csproj" />
  <ProjectReference Include="..\flagstone-ui\src\FlagstoneUI.Themes.Material\FlagstoneUI.Themes.Material.csproj" />
</ItemGroup>
```

## Control Reference

### FsButton

Button control with full visual customization.

**Key Properties:**
- `Text`, `Command`, `CommandParameter` - Standard button properties
- `BackgroundColor`, `TextColor` - Colors (use tokens)
- `CornerRadius` - Corner radius (Int32, use `Radius.Button.*` tokens)
- `Padding` - Internal spacing
- `BorderColor`, `BorderWidth` - Uniform border
- `FontSize`, `FontFamily`, `FontAttributes` - Typography

**Example:**

```xml
<fs:FsButton Text="Primary Action" />
<fs:FsButton Text="Secondary" Style="{StaticResource OutlinedButton}" />
<fs:FsButton Text="Tertiary" Style="{StaticResource TextButton}" />
```

### FsCard

Container control with elevation, borders, and corner radius.

**Key Properties:**
- `Content` - ContentView content property
- `BackgroundColor` - Background color (use tokens)
- `CornerRadius` - Corner radius (Double, use `Radius.*` tokens)
- `Padding` - Internal spacing
- `Elevation` - Shadow depth (0-4, follows Material Design elevation levels)
- `BorderColor`, `BorderWidth` - Uniform border
- Per-edge borders: `BorderTopThickness`, `BorderRightThickness`, `BorderBottomThickness`, `BorderLeftThickness` with corresponding `Brush` properties
- `Border` - Shorthand for border (e.g., `"2 Blue"` or `"2 White, 2 Gray, 2 Gray, 2 White"`)

**Example:**

```xml
<fs:FsCard>
    <Label Text="Default filled card" />
</fs:FsCard>

<fs:FsCard Style="{StaticResource ElevatedCard}">
    <Label Text="Card with shadow" />
</fs:FsCard>
```

### FsEntry

Single-line text input with full visual control.

**Key Properties:**
- `Text`, `Placeholder` - Text content
- `TextColor`, `PlaceholderColor` - Colors (use tokens)
- `FontSize`, `FontFamily` - Typography
- `Keyboard` - Keyboard type (Email, Numeric, etc.)
- `IsPassword` - Password masking
- `Background` - Background brush (use tokens, can be gradient)
- `CornerRadius` - Corner radius (Double, use `Radius.*` tokens)
- `Padding` - Internal spacing
- `BorderBrush`, `BorderWidth` - Uniform border
- Per-edge borders: `BorderTopThickness`, `BorderRightThickness`, `BorderBottomThickness`, `BorderLeftThickness` with corresponding `Brush` properties
- `Border` - Shorthand for border
- `HeightRequest` - Control height
- Visual states: `Focused`, `Normal`

**Example:**

```xml
<fs:FsEntry Placeholder="Email" Keyboard="Email" />
<fs:FsEntry Placeholder="Password" IsPassword="True" />
<fs:FsEntry Placeholder="Outlined" Style="{StaticResource OutlinedEntry}" />
```

### FsEditor

Multi-line text input with full visual control.

**Key Properties:**
- `Text`, `Placeholder` - Text content
- `TextColor`, `PlaceholderColor` - Colors (use tokens)
- `FontSize`, `FontFamily` - Typography
- `Background` - Background brush (use tokens, can be gradient)
- `CornerRadius` - Corner radius (Double, use `Radius.*` tokens)
- `Padding` - Internal spacing
- `BorderBrush`, `BorderWidth` - Uniform border
- Per-edge borders: `BorderTopThickness`, `BorderRightThickness`, `BorderBottomThickness`, `BorderLeftThickness` with corresponding `Brush` properties
- `Border` - Shorthand for border
- `MinimumHeightRequest` - Minimum control height
- `AutoSize` - Auto-expand behavior
- Visual states: `Focused`, `Normal`

**Example:**

```xml
<fs:FsEditor Placeholder="Enter description..." 
             MinimumHeightRequest="120" />

<fs:FsEditor Placeholder="Outlined editor"
             Style="{StaticResource OutlinedEditor}" />
```

## Understanding the Sample App

The sample app (`samples/FlagstoneUI.SampleApp/`) demonstrates FlagstoneUI capabilities but is **atypical**:

- It showcases **multiple themes** and theme switching
- It demonstrates **per-edge borders** and advanced styling
- It shows **MCT integration** examples

**Typical consumer apps:**
- Use **one theme** (Material or custom)
- Apply theme in `App.xaml` and forget about it
- Use standard control patterns without advanced styling
- May not use MCT integration at all

**Key Takeaway:** The sample app shows what's *possible*, not what's *typical*. Most apps will have simpler, more straightforward usage.

## Best Practices Summary

1. **Always use tokens** for colors, spacing, typography, and other visual properties
2. **Reference tokens with DynamicResource** to support theming and dark mode
3. **Use semantic color tokens** based on meaning, not appearance
4. **Prefer theme style variants** over custom inline styling
5. **Use the `fs:` prefix** for all FlagstoneUI controls
6. **Be consistent** - don't mix standard MAUI and FlagstoneUI controls
7. **Choose uniform OR per-edge borders** - don't mix both approaches
8. **Validate against tokens** - if you're hardcoding values, you're doing it wrong

## Troubleshooting

### Theme Not Applied

- Verify `<material:Theme />` is in `App.xaml` MergedDictionaries
- Check XAML namespace declaration is correct
- Ensure project reference to theme package exists
- Clean and rebuild solution

### Controls Not Found

- Verify `xmlns:fs="clr-namespace:FlagstoneUI.Core.Controls;assembly=FlagstoneUI.Core"` is declared
- Check project reference to FlagstoneUI.Core exists
- Ensure control name is correct (e.g., `FsButton` not `FsButton`)

### Validation Not Working

- Install `FlagstoneUI.Integrations.MCT` package
- Add MCT namespace: `xmlns:toolkit="http://schemas.microsoft.com/dotnet/2022/maui/toolkit"`
- Set `Flags` property (e.g., `ValidateOnValueChanged`)
- Ensure styles target `fs:FsEntry` not `Entry`

### Build Errors

- Ensure .NET 10 SDK is installed (check `global.json`)
- Install MAUI workload: `dotnet workload install maui`
- Restore packages: `dotnet restore`
- Clean and rebuild: `dotnet clean && dotnet build`

## Documentation and Examples

- **Documentation**: `/docs/` directory in repository
- **Sample App**: `samples/FlagstoneUI.SampleApp/`
- **Theme Playground**: `samples/FlagstoneUI.ThemePlayground/`
- **Control Documentation**: `/docs/controls/` directory
- **Quickstart Guide**: `/docs/getting-started/quickstart.md`
- **Theming Guide**: `/docs/guides/theming-guide.md`
- **Architecture**: `/docs/getting-started/architecture.md`

## Contributing

FlagstoneUI is under active development. When contributing:

- Follow the token-first design philosophy
- Use existing tokens before creating new ones
- Keep controls platform-neutral
- Add documentation for new patterns
- Update this AGENTS.MD file when patterns change

## Future Considerations

Per ADR009, FlagstoneUI may add:

- **Figma Converter**: Import design tokens from Figma
- **Additional Themes**: Community-contributed themes
- **More Controls**: Pickers, lists, dialogs
- **FlagstoneUI.Blocks**: Pre-built screens (sign-in, CRUD, settings)
- **Project Templates**: .NET templates with FlagstoneUI pre-configured

---

**Last Updated**: June 2026  
**Version**: 2.2
**Status**: V1 complete
