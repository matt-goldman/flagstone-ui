# Flagstone UI Design Tokens

This document provides a comprehensive reference of all design tokens available in Flagstone UI. These tokens form the foundation of the theming system and can be customized by themes to create consistent, cohesive user experiences.

## What are Design Tokens?

Design tokens are named entities that store visual design attributes. They provide a single source of truth for design decisions and make it easy to maintain consistency across an application. In Flagstone UI, tokens are defined as XAML resources that can be referenced throughout your application and overridden by themes.

## How to Use This Document

This document is intended for:
- **Designers** creating design language systems (DLS) or branding guidelines
- **Developers** implementing custom themes based on design specifications
- **Teams** ensuring design-development alignment

Each token includes:
- **Token name**: The XAML resource key used to reference the token
- **Default value**: The value defined in the base token system
- **Purpose**: What the token is used for
- **Type**: Color, size, spacing, etc.

## Color Tokens

Color tokens follow Material Design 3's color system, providing semantic roles for colors rather than fixed palette values. This allows themes to maintain consistent meaning while varying visual appearance.

### Primary Colors

Primary colors are used for key components and high-emphasis elements.

| Token Name | Default Value (Light) | Purpose |
|------------|----------------------|---------|
| `Color.Primary` | `#6750A4` (Purple) | Main brand color, used for primary actions and key UI elements |
| `Color.OnPrimary` | `#FFFFFF` (White) | Text and icons displayed on primary color |
| `Color.PrimaryContainer` | `#EADDFF` (Light Purple) | Containers and backgrounds for primary-related elements |
| `Color.OnPrimaryContainer` | `#21005D` (Dark Purple) | Text and icons on primary containers |

### Secondary Colors

Secondary colors are used for less prominent components and provide additional ways to accent and distinguish your product.

| Token Name | Default Value (Light) | Purpose |
|------------|----------------------|---------|
| `Color.Secondary` | `#625B71` (Muted Purple) | Secondary actions and less prominent UI elements |
| `Color.OnSecondary` | `#FFFFFF` (White) | Text and icons on secondary color |
| `Color.SecondaryContainer` | `#E8DEF8` (Light Lavender) | Containers for secondary-related elements |
| `Color.OnSecondaryContainer` | `#1D192B` (Dark Plum) | Text and icons on secondary containers |

### Tertiary Colors

Tertiary colors are used for contrasting accents that balance primary and secondary colors.

| Token Name | Default Value (Light) | Purpose |
|------------|----------------------|---------|
| `Color.Tertiary` | `#7D5260` (Mauve) | Accent color for additional visual hierarchy |
| `Color.OnTertiary` | `#FFFFFF` (White) | Text and icons on tertiary color |
| `Color.TertiaryContainer` | `#FFD8E4` (Light Pink) | Containers for tertiary-related elements |
| `Color.OnTertiaryContainer` | `#31111D` (Dark Burgundy) | Text and icons on tertiary containers |

### Error Colors

Error colors communicate errors and destructive actions.

| Token Name | Default Value (Light) | Purpose |
|------------|----------------------|---------|
| `Color.Error` | `#B3261E` (Red) | Error states, destructive actions, critical alerts |
| `Color.OnError` | `#FFFFFF` (White) | Text and icons on error color |
| `Color.ErrorContainer` | `#F9DEDC` (Light Red) | Containers for error-related messages |
| `Color.OnErrorContainer` | `#410E0B` (Dark Red) | Text and icons on error containers |

### Surface Colors

Surface colors affect surfaces of components, such as cards, sheets, and menus.

| Token Name | Default Value (Light) | Purpose |
|------------|----------------------|---------|
| `Color.Surface` | `#FFFBFE` (Off-White) | Main background surface color |
| `Color.OnSurface` | `#1C1B1F` (Near-Black) | Text and icons on surface |
| `Color.SurfaceVariant` | `#E7E0EC` (Light Gray-Purple) | Alternative surface color for emphasis |
| `Color.OnSurfaceVariant` | `#49454F` (Medium Gray) | Text and icons on surface variants |

### Background Colors

Background colors appear behind scrollable content.

| Token Name | Default Value (Light) | Purpose |
|------------|----------------------|---------|
| `Color.Background` | `#FFFBFE` (Off-White) | Main background color behind surfaces |
| `Color.OnBackground` | `#1C1B1F` (Near-Black) | Text and icons on background |

### Outline Colors

Outline colors are for borders and dividers.

| Token Name | Default Value (Light) | Purpose |
|------------|----------------------|---------|
| `Color.Outline` | `#79747E` (Medium Gray) | Important borders and dividers |
| `Color.OutlineVariant` | `#CAC4D0` (Light Gray) | Subtle borders and dividers |

### Inverse Colors

Inverse colors are used for elements that need to stand out against the standard color scheme.

| Token Name | Default Value (Light) | Purpose |
|------------|----------------------|---------|
| `Color.InverseSurface` | `#313033` (Dark Gray) | Inverse surface (e.g., dark tooltips in light mode) |
| `Color.InverseOnSurface` | `#F4EFF4` (Off-White) | Text and icons on inverse surface |
| `Color.InversePrimary` | `#D0BCFF` (Light Purple) | Primary color for inverse elements |

## Border Radius Tokens

Border radius tokens control the roundness of corners on UI elements.

| Token Name | Value | Purpose |
|------------|-------|---------|
| `Radius.None` | `0` | Sharp corners (no rounding) |
| `Radius.ExtraSmall` | `4` | Minimal rounding for compact elements |
| `Radius.Small` | `8` | Small rounding for buttons and inputs |
| `Radius.Medium` | `12` | Medium rounding for cards |
| `Radius.Large` | `16` | Large rounding for prominent containers |
| `Radius.ExtraLarge` | `28` | Extra large rounding for special emphasis |
| `Radius.Full` | `9999` | Fully rounded (circular/pill shape) |

## Spacing Tokens

Spacing tokens define consistent spacing throughout the application.

| Token Name | Value | Purpose |
|------------|-------|---------|
| `Space.0` | `0` | No spacing |
| `Space.2` | `2` | Minimal spacing between very tight elements |
| `Space.4` | `4` | Tight spacing |
| `Space.8` | `8` | Standard small spacing |
| `Space.12` | `12` | Medium-small spacing |
| `Space.16` | `16` | Standard spacing (most common) |
| `Space.20` | `20` | Medium-large spacing |
| `Space.24` | `24` | Large spacing |
| `Space.32` | `32` | Extra large spacing |
| `Space.40` | `40` | Section separation |
| `Space.48` | `48` | Major section separation |

## Typography Tokens

Typography tokens define font sizes following Material Design 3 type scale.

### Display Styles
Large, expressive type for hero text and marketing content.

| Token Name | Value | Purpose |
|------------|-------|---------|
| `FontSize.DisplayLarge` | `57` | Largest display text (hero headlines) |
| `FontSize.DisplayMedium` | `45` | Medium display text |
| `FontSize.DisplaySmall` | `36` | Small display text |

### Headline Styles
High-emphasis text for section headers.

| Token Name | Value | Purpose |
|------------|-------|---------|
| `FontSize.HeadlineLarge` | `32` | Large headline text |
| `FontSize.HeadlineMedium` | `28` | Medium headline text |
| `FontSize.HeadlineSmall` | `24` | Small headline text |

### Title Styles
Medium-emphasis text for section titles.

| Token Name | Value | Purpose |
|------------|-------|---------|
| `FontSize.TitleLarge` | `22` | Large title text (page titles) |
| `FontSize.TitleMedium` | `16` | Medium title text (card titles) |
| `FontSize.TitleSmall` | `14` | Small title text (list headers) |

### Body Styles
Default text styles for body content.

| Token Name | Value | Purpose |
|------------|-------|---------|
| `FontSize.BodyLarge` | `16` | Large body text (emphasized content) |
| `FontSize.BodyMedium` | `14` | Medium body text (standard content) |
| `FontSize.BodySmall` | `12` | Small body text (supporting text) |

### Label Styles
Text styles for UI labels and button text.

| Token Name | Value | Purpose |
|------------|-------|---------|
| `FontSize.LabelLarge` | `14` | Large label text (prominent buttons) |
| `FontSize.LabelMedium` | `12` | Medium label text (standard labels) |
| `FontSize.LabelSmall` | `11` | Small label text (compact UI) |

### Legacy Typography
Backward compatibility aliases.

| Token Name | Value | Purpose |
|------------|-------|---------|
| `FontSize.Body` | `14` | Alias for BodyMedium |
| `FontSize.Title` | `20` | Legacy title size |

## Border Width Tokens

Border width tokens control the thickness of borders and dividers.

| Token Name | Value | Purpose |
|------------|-------|---------|
| `BorderWidth.None` | `0` | No border |
| `BorderWidth.Thin` | `1` | Thin border (standard) |
| `BorderWidth.Thick` | `2` | Thick border (emphasis) |
| `BorderWidth.Heavy` | `4` | Heavy border (strong emphasis) |

## Elevation Tokens

Elevation tokens represent shadow depth levels, creating visual hierarchy through layering.

| Token Name | Value | Purpose |
|------------|-------|---------|
| `Elevation.Level0` | `0` | No elevation (flat on surface) |
| `Elevation.Level1` | `2` | Minimal elevation (cards) |
| `Elevation.Level2` | `4` | Low elevation (raised buttons) |
| `Elevation.Level3` | `8` | Medium elevation (floating action buttons) |
| `Elevation.Level4` | `12` | High elevation (dialogs, modal sheets) |

## Padding Tokens

Padding tokens define internal spacing within components.

| Token Name | Value | Purpose |
|------------|-------|---------|
| `Padding.None` | `0` | No internal padding |
| `Padding.Small` | `4` | Minimal internal padding |
| `Padding.Medium` | `8` | Standard internal padding |
| `Padding.Large` | `16` | Large internal padding |
| `Padding.ExtraLarge` | `24` | Extra large internal padding |

## State Layer Opacity Tokens

State layer opacity tokens define overlay opacity for interactive states (hover, focus, press).

| Token Name | Value | Purpose |
|------------|-------|---------|
| `StateLayer.Hover` | `0.08` | Opacity for hover state overlays |
| `StateLayer.Focus` | `0.12` | Opacity for focus state overlays |
| `StateLayer.Pressed` | `0.12` | Opacity for pressed state overlays |
| `StateLayer.Dragged` | `0.16` | Opacity for drag state overlays |

## Opacity Tokens

General opacity tokens for various UI states and elements.

| Token Name | Value | Purpose |
|------------|-------|---------|
| `Opacity.Disabled` | `0.38` | Opacity for disabled elements |
| `Opacity.Medium` | `0.74` | Medium emphasis opacity |
| `Opacity.High` | `0.87` | High emphasis opacity |
| `Opacity.Full` | `1.0` | Full opacity (no transparency) |

## Dark Mode Support

Flagstone UI themes can override token values to support dark mode. The Material theme, for example, provides dark mode variants for all color tokens using the `.Dark` suffix pattern (e.g., `Color.Primary.Dark`).

When implementing dark mode in your theme:
1. Define dark mode color variants with `.Dark` suffix
2. Use `AppThemeBinding` in styles to switch between light and dark values
3. Ensure sufficient contrast ratios for accessibility

Example:
```xml
<Color x:Key="Color.Primary.Dark">#D0BCFF</Color>

<Style TargetType="Button">
    <Setter Property="BackgroundColor" 
            Value="{AppThemeBinding Light={DynamicResource Color.Primary}, 
                                   Dark={DynamicResource Color.Primary.Dark}}" />
</Style>
```

## Using Tokens in Custom Themes

To create a custom theme:

1. **Reference base tokens**: Merge `Tokens.xaml` in your theme's resource dictionary
2. **Override color tokens**: Define theme-specific color values
3. **Define dark mode colors**: Add `.Dark` suffixed versions for dark mode support
4. **Create component styles**: Use tokens via `DynamicResource` in component styles
5. **Test in both modes**: Verify appearance in light and dark modes

Example theme structure:
```xml
<ResourceDictionary>
    <ResourceDictionary.MergedDictionaries>
        <tokens:Tokens />
    </ResourceDictionary.MergedDictionaries>

    <!-- Override light mode colors -->
    <Color x:Key="Color.Primary">#YourBrandColor</Color>
    
    <!-- Define dark mode colors -->
    <Color x:Key="Color.Primary.Dark">#YourDarkBrandColor</Color>
    
    <!-- Create styles using tokens -->
    <Style TargetType="Button">
        <Setter Property="BackgroundColor" 
                Value="{AppThemeBinding Light={DynamicResource Color.Primary}, 
                                       Dark={DynamicResource Color.Primary.Dark}}" />
    </Style>
</ResourceDictionary>
```

## Design Guidelines

When working with Flagstone UI tokens:

1. **Semantic over literal**: Use tokens by their semantic meaning (e.g., `Color.Primary`) rather than their color value (e.g., "purple")
2. **Consistent spacing**: Stick to the spacing scale for predictable layouts
3. **Type hierarchy**: Follow the typography scale to establish clear content hierarchy
4. **Color roles**: Use color roles consistently (e.g., always use `Color.Error` for errors)
5. **Accessibility**: Ensure sufficient contrast between foreground and background colors
6. **Dark mode**: Always test your theme in both light and dark modes

## Token Naming Conventions

Flagstone UI follows these naming conventions:

- **Category prefix**: All tokens start with their category (e.g., `Color.`, `FontSize.`, `Space.`)
- **Semantic naming**: Token names describe purpose, not appearance
- **Size scales**: Follow Small → Medium → Large → ExtraLarge patterns
- **Dark mode suffix**: Dark mode variants use `.Dark` suffix

## Future Enhancements

This token system is designed to grow with Flagstone UI. Planned additions include:

- Animation/transition tokens
- Shadow tokens (beyond elevation levels)
- Font family tokens
- Line height tokens
- Letter spacing tokens
- Icon size tokens

## Questions or Feedback?

For questions about tokens or to suggest new tokens, please open an issue on the [Flagstone UI GitHub repository](https://github.com/matt-goldman/flagstone-ui).

---

*Last Updated: 2025-11-01*  
*Version: 0.1.0 (MVP)*
