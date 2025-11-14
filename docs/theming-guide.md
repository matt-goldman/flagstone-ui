# Theming Guide for Designers

This guide is for designers who want to create custom themes for Flagstone UI applications. Whether you're implementing a brand identity, design language system (DLS), or custom visual style, this guide will help you understand what's themable and how to communicate your design to developers.

## Table of Contents

- [Understanding Flagstone UI Themes](#understanding-flagstone-ui-themes)
- [The Token System](#the-token-system)
- [Creating a Custom Theme](#creating-a-custom-theme)
- [Sample Theme Document](#sample-theme-document)
- [Control Properties Reference](#control-properties-reference)
- [Designer-to-Developer Workflow](#designer-to-developer-workflow)
- [Best Practices](#best-practices)

## Understanding Flagstone UI Themes

### What is a Theme?

A theme in Flagstone UI is a coordinated set of design tokens that defines the visual appearance of an application. Themes control:

- **Colors**: Primary, secondary, surface, background, error states, and more
- **Typography**: Font sizes, weights, and line heights
- **Spacing**: Padding, margins, and gaps between elements
- **Shape**: Corner radii and border widths
- **Elevation**: Shadow depths for layered UI elements

### Theme Architecture

```
┌────────────────────────────────────────┐
│         Application UI                 │
│    (Buttons, Cards, Text Fields)       │
└────────────────────────────────────────┘
                 ↓
┌────────────────────────────────────────┐
│       Theme (Your Custom Theme)        │
│     Overrides token values to          │
│     match your brand/DLS               │
└────────────────────────────────────────┘
                 ↓
┌────────────────────────────────────────┐
│      Base Design Token System          │
│   (Default values + token names)       │
└────────────────────────────────────────┘
```

**Key Principle**: Instead of styling individual controls directly, you define token values. Controls automatically use these tokens, ensuring consistency across your application.

### What Makes a Good Theme?

A well-designed Flagstone UI theme:

1. ✅ Maintains **sufficient color contrast** for accessibility (WCAG 2.1 AA minimum)
2. ✅ Provides **consistent spacing** using the spacing scale
3. ✅ Uses **semantic color roles** correctly (e.g., error states use error colors)
4. ✅ Supports **both light and dark modes** (if required)
5. ✅ Reflects your **brand identity** while maintaining usability

## The Token System

### What are Design Tokens?

Design tokens are named values that store visual design decisions. They act as a "single source of truth" for your design system.

**Example:**
- Instead of hardcoding `#6750A4` everywhere, you use `Color.Primary`
- If you want to change your primary color brand-wide, you update one token value

### Token Categories

Flagstone UI organizes tokens into these categories:

#### 1. Color Tokens

Based on Material Design 3's semantic color system:

**Primary Colors** (Main brand color)
- `Color.Primary` - Main brand color, used for key UI elements
- `Color.OnPrimary` - Text/icons displayed on primary color
- `Color.PrimaryContainer` - Container backgrounds for primary elements
- `Color.OnPrimaryContainer` - Text/icons on primary containers

**Secondary Colors** (Supporting brand color)
- `Color.Secondary` - Secondary actions and less prominent elements
- `Color.OnSecondary` - Text/icons on secondary color
- `Color.SecondaryContainer` - Container backgrounds for secondary elements
- `Color.OnSecondaryContainer` - Text/icons on secondary containers

**Tertiary Colors** (Accent color)
- `Color.Tertiary` - Accent color for visual hierarchy
- `Color.OnTertiary` - Text/icons on tertiary color
- `Color.TertiaryContainer` - Container backgrounds for tertiary elements
- `Color.OnTertiaryContainer` - Text/icons on tertiary containers

**Error Colors** (Error and destructive states)
- `Color.Error` - Error states and destructive actions
- `Color.OnError` - Text/icons on error color
- `Color.ErrorContainer` - Error message backgrounds
- `Color.OnErrorContainer` - Text/icons on error containers

**Surface Colors** (Backgrounds and containers)
- `Color.Surface` - Main background surface
- `Color.OnSurface` - Primary text and icons
- `Color.SurfaceVariant` - Alternative surface for emphasis
- `Color.OnSurfaceVariant` - Secondary text and icons
- `Color.Background` - Page background
- `Color.OnBackground` - Text/icons on background

**Outline Colors** (Borders and dividers)
- `Color.Outline` - Borders, dividers, and focus indicators
- `Color.OutlineVariant` - Subtle borders and dividers

**Inverse Colors** (For inverted UI areas)
- `Color.InverseSurface` - Background for inverted areas
- `Color.InverseOnSurface` - Text/icons in inverted areas
- `Color.InversePrimary` - Primary color in inverted areas

#### 2. Spacing Tokens

Consistent spacing scale (in device-independent pixels):

- `Space.0` = 0px (no spacing)
- `Space.2` = 2px (very tight)
- `Space.4` = 4px (tight)
- `Space.8` = 8px (small)
- `Space.12` = 12px (small-medium)
- `Space.16` = 16px (medium) ← Most common
- `Space.20` = 20px (medium-large)
- `Space.24` = 24px (large)
- `Space.32` = 32px (extra-large)
- `Space.40` = 40px (extra-extra-large)
- `Space.48` = 48px (huge)

**Usage Guidelines:**
- Use `Space.16` or `Space.24` for card padding
- Use `Space.8` or `Space.12` for internal component spacing
- Use `Space.32` or `Space.48` for section separation

#### 3. Typography Tokens

Font size scale based on Material Design 3:

**Display** (Large headlines)
- `FontSize.DisplayLarge` = 57px
- `FontSize.DisplayMedium` = 45px
- `FontSize.DisplaySmall` = 36px

**Headline** (Section headers)
- `FontSize.HeadlineLarge` = 32px
- `FontSize.HeadlineMedium` = 28px
- `FontSize.HeadlineSmall` = 24px

**Title** (Card/component titles)
- `FontSize.TitleLarge` = 22px
- `FontSize.TitleMedium` = 16px
- `FontSize.TitleSmall` = 14px

**Body** (Main content text)
- `FontSize.BodyLarge` = 16px
- `FontSize.BodyMedium` = 14px ← Default body text
- `FontSize.BodySmall` = 12px

**Label** (Button labels, captions)
- `FontSize.LabelLarge` = 14px
- `FontSize.LabelMedium` = 12px
- `FontSize.LabelSmall` = 11px

#### 4. Shape Tokens (Corner Radii)

Corner radius values for different component sizes:

**Standard Radius Tokens (Double type - for Cards, Entries, etc.):**
- `Radius.None` = 0px (square corners)
- `Radius.ExtraSmall` = 4px
- `Radius.Small` = 8px
- `Radius.Medium` = 12px ← Cards
- `Radius.Large` = 16px
- `Radius.ExtraLarge` = 28px
- `Radius.Full` = 9999px (fully rounded/circular)

**Button-Specific Radius Tokens (Int32 type - for Button.CornerRadius only):**
- `Radius.Button.None` = 0 ← Square button corners
- `Radius.Button.ExtraSmall` = 4
- `Radius.Button.Small` = 8
- `Radius.Button.Medium` = 12
- `Radius.Button.Large` = 16 ← Rounded button corners
- `Radius.Button.ExtraLarge` = 28
- `Radius.Button.Full` = 9999 ← Pill-shaped buttons

> **⚠️ Important**: Use `Radius.Button.*` tokens when styling `FsButton` or standard `Button` controls. The Button.CornerRadius property requires Int32 type, while other controls use Double. Using the wrong token type will result in silent binding failure and square corners. See [ADR003](Decisions/adr003-button-corner-radius-type.md) for technical details.

#### 5. Other Tokens

**Border Widths**
- `BorderWidth.None` = 0px
- `BorderWidth.Thin` = 1px ← Standard border
- `BorderWidth.Thick` = 2px
- `BorderWidth.Heavy` = 4px

**Elevation Levels** (Shadow depth)
- `Elevation.Level0` = 0px (no shadow)
- `Elevation.Level1` = 2px (subtle)
- `Elevation.Level2` = 4px (cards)
- `Elevation.Level3` = 8px (raised elements)
- `Elevation.Level4` = 12px (dialogs, modals)

**Opacity Values**
- `Opacity.Disabled` = 0.38 (38%)
- `Opacity.Medium` = 0.74 (74%)
- `Opacity.High` = 0.87 (87%)
- `Opacity.Full` = 1.0 (100%)

## Creating a Custom Theme

### Step 1: Define Your Color Palette

Start by defining colors for your brand. You'll need to provide values for each semantic color role.

**Example: "Ocean" Brand Theme**

```
Primary Colors:
  - Primary: #006A6A (Teal)
  - OnPrimary: #FFFFFF (White)
  - PrimaryContainer: #6FF7F7 (Light Teal)
  - OnPrimaryContainer: #002020 (Dark Teal)

Secondary Colors:
  - Secondary: #4A6363 (Gray-Teal)
  - OnSecondary: #FFFFFF (White)
  - SecondaryContainer: #CCE8E8 (Very Light Teal)
  - OnSecondaryContainer: #051F1F (Very Dark Teal)

Tertiary Colors:
  - Tertiary: #4B607C (Blue-Gray)
  - OnTertiary: #FFFFFF (White)
  - TertiaryContainer: #D3E4FF (Light Blue)
  - OnTertiaryContainer: #041C35 (Dark Blue)

Error Colors:
  - Error: #BA1A1A (Red)
  - OnError: #FFFFFF (White)
  - ErrorContainer: #FFDAD6 (Light Red)
  - OnErrorContainer: #410002 (Dark Red)

Surface & Background:
  - Surface: #FAFDFC (Off-White with teal tint)
  - OnSurface: #191C1C (Near-Black)
  - SurfaceVariant: #DAE5E4 (Light Gray-Teal)
  - OnSurfaceVariant: #3F4948 (Medium Gray)
  - Background: #FAFDFC (Same as Surface)
  - OnBackground: #191C1C (Same as OnSurface)

Outline:
  - Outline: #6F7978 (Medium Gray)
  - OutlineVariant: #BEC9C8 (Light Gray)
```

### Step 2: Choose Typography (Optional)

Most themes keep the default font size scale, but you can customize if needed:

```
Typography (using defaults):
  - Display Large: 57px
  - Headline Large: 32px
  - Title Large: 22px
  - Body Medium: 14px
  - Label Large: 14px
  (see full scale above)
```

### Step 3: Define Shape & Spacing (Optional)

For most themes, use the default spacing and shape scales. Customize only if your brand requires it:

```
Shape (using defaults):
  - Button radius: 8px (Radius.Small)
  - Card radius: 12px (Radius.Medium)
  - Entry radius: 8px (Radius.Small)

Spacing (using defaults):
  - Standard padding: 16px (Space.16)
  - Large padding: 24px (Space.24)
  - Component spacing: 8px (Space.8)
```

### Step 4: Document Your Theme

Create a document (Word, PDF, Figma file, or Markdown) that lists all your token values. See the [Sample Theme Document](#sample-theme-document) section below.

## Sample Theme Document

Below is a template you can use to document your custom theme and hand it to a developer for implementation.

---

### **Theme Name: [Your Theme Name]**

**Version:** 1.0
**Created:** [Date]
**Designer:** [Your Name]
**Brand/Product:** [Brand Name]

---

#### **Color Palette**

| Token Name | Light Mode Value | Dark Mode Value (Optional) | Usage |
|------------|------------------|----------------------------|-------|
| `Color.Primary` | `#006A6A` | `#4FD9D9` | Main brand color, primary buttons |
| `Color.OnPrimary` | `#FFFFFF` | `#003737` | Text on primary color |
| `Color.PrimaryContainer` | `#6FF7F7` | `#004F4F` | Primary button backgrounds (tonal) |
| `Color.OnPrimaryContainer` | `#002020` | `#6FF7F7` | Text on primary containers |
| `Color.Secondary` | `#4A6363` | `#B1CCCC` | Secondary actions |
| `Color.OnSecondary` | `#FFFFFF` | `#1C3535` | Text on secondary color |
| `Color.SecondaryContainer` | `#CCE8E8` | `#324B4B` | Secondary backgrounds |
| `Color.OnSecondaryContainer` | `#051F1F` | `#CCE8E8` | Text on secondary containers |
| `Color.Tertiary` | `#4B607C` | `#B8C8EA` | Accent/tertiary actions |
| `Color.OnTertiary` | `#FFFFFF` | `#213A5C` | Text on tertiary color |
| `Color.TertiaryContainer` | `#D3E4FF` | `#3A5374` | Tertiary backgrounds |
| `Color.OnTertiaryContainer` | `#041C35` | `#D3E4FF` | Text on tertiary containers |
| `Color.Error` | `#BA1A1A` | `#FFB4AB` | Error states, destructive actions |
| `Color.OnError` | `#FFFFFF` | `#690005` | Text on error color |
| `Color.ErrorContainer` | `#FFDAD6` | `#93000A` | Error backgrounds |
| `Color.OnErrorContainer` | `#410002` | `#FFDAD6` | Text on error containers |
| `Color.Surface` | `#FAFDFC` | `#191C1C` | Card backgrounds, surfaces |
| `Color.OnSurface` | `#191C1C` | `#E1E3E2` | Primary text |
| `Color.SurfaceVariant` | `#DAE5E4` | `#3F4948` | Alternative surfaces |
| `Color.OnSurfaceVariant` | `#3F4948` | `#BEC9C8` | Secondary text |
| `Color.Background` | `#FAFDFC` | `#191C1C` | Page backgrounds |
| `Color.OnBackground` | `#191C1C` | `#E1E3E2` | Text on backgrounds |
| `Color.Outline` | `#6F7978` | `#899392` | Borders, dividers |
| `Color.OutlineVariant` | `#BEC9C8` | `#3F4948` | Subtle borders |
| `Color.InverseSurface` | `#2E3131` | `#E1E3E2` | Inverted surfaces (tooltips) |
| `Color.InverseOnSurface` | `#EFF1F0` | `#191C1C` | Text on inverted surfaces |
| `Color.InversePrimary` | `#4FD9D9` | `#006A6A` | Primary in inverted areas |

---

#### **Typography Scale**

| Token Name | Value | Usage |
|------------|-------|-------|
| `FontSize.DisplayLarge` | 57px | Hero text |
| `FontSize.DisplayMedium` | 45px | Large headers |
| `FontSize.DisplaySmall` | 36px | Headers |
| `FontSize.HeadlineLarge` | 32px | Page titles |
| `FontSize.HeadlineMedium` | 28px | Section headers |
| `FontSize.HeadlineSmall` | 24px | Card headers |
| `FontSize.TitleLarge` | 22px | Prominent titles |
| `FontSize.TitleMedium` | 16px | Component titles |
| `FontSize.TitleSmall` | 14px | Small titles |
| `FontSize.BodyLarge` | 16px | Large body text |
| `FontSize.BodyMedium` | 14px | Standard body text |
| `FontSize.BodySmall` | 12px | Small body text |
| `FontSize.LabelLarge` | 14px | Button labels |
| `FontSize.LabelMedium` | 12px | Input labels |
| `FontSize.LabelSmall` | 11px | Captions |

---

#### **Spacing Scale**

| Token Name | Value | Usage |
|------------|-------|-------|
| `Space.8` | 8px | Tight spacing |
| `Space.12` | 12px | Small spacing |
| `Space.16` | 16px | Standard padding ⭐ |
| `Space.24` | 24px | Large padding |
| `Space.32` | 32px | Section separation |

---

#### **Shape (Corner Radii)**

| Token Name | Value | Usage |
|------------|-------|-------|
| `Radius.Small` | 8px | Buttons, text fields |
| `Radius.Medium` | 12px | Cards |
| `Radius.Large` | 16px | Large containers |
| `Radius.Full` | 9999px | Circular elements |

---

#### **Elevation (Shadows)**

| Token Name | Value | Usage |
|------------|-------|-------|
| `Elevation.Level0` | 0px | Flat surfaces |
| `Elevation.Level1` | 2px | Subtle elevation |
| `Elevation.Level2` | 4px | Cards |
| `Elevation.Level3` | 8px | Raised elements |
| `Elevation.Level4` | 12px | Modals, dialogs |

---

#### **Notes & Special Instructions**

- **Accessibility**: All color combinations meet WCAG 2.1 AA contrast requirements (4.5:1 for normal text, 3:1 for large text)
- **Dark Mode**: Dark mode colors provided for seamless theme switching
- **Brand Alignment**: Primary color (#006A6A) matches our brand's ocean/teal identity
- **Testing**: Please test with actual content to ensure readability

---

### **Visual Examples**

[Include mockups, screenshots, or Figma links showing the theme applied to key screens]

---

## Control Properties Reference

This section shows what properties are themable for each Flagstone UI control.

### FsButton

| Property | Recommended Token | Description |
|----------|------------------|-------------|
| `BackgroundColor` | `Color.Primary` (filled)<br/>`Transparent` (outlined/text) | Button background |
| `TextColor` | `Color.OnPrimary` (filled)<br/>`Color.Primary` (outlined/text) | Button text color |
| `BorderColor` | `Color.Primary` (outlined)<br/>`Transparent` (filled/text) | Border color |
| `BorderWidth` | `BorderWidth.Thin` (outlined)<br/>`BorderWidth.None` (filled/text) | Border thickness |
| `CornerRadius` | **`Radius.Button.Small`** or **`Radius.Button.Large`** | **⚠️ Use Button-specific tokens only!** |
| `Padding` | `Space.16` (horizontal), `Space.12` (vertical) | Internal padding |
| `FontSize` | `FontSize.LabelLarge` | Button text size |

> **⚠️ Critical**: Always use `Radius.Button.*` tokens for button corner radius, **never** `Radius.*` tokens. Button.CornerRadius requires Int32 type. Using Double-typed tokens will result in silent failure and square corners.

**Variant Styles:**
- **Filled** (default): Solid background, primary color
- **Outlined**: Transparent background with border
- **Text**: No background, no border, just colored text
- **Tonal**: Subtle background using container colors

### FsCard

| Property | Recommended Token | Description |
|----------|------------------|-------------|
| `BackgroundColor` | `Color.Surface` (filled)<br/>`Color.SurfaceVariant` (tinted) | Card background |
| `BorderColor` | `Color.Outline` (outlined)<br/>`Transparent` (filled) | Border color |
| `BorderWidth` | `BorderWidth.Thin` (outlined)<br/>`BorderWidth.None` (filled) | Border thickness |
| `CornerRadius` | `Radius.Medium` | Card corner radius |
| `Padding` | `Space.16` to `Space.24` | Internal padding |
| `Elevation` | `Elevation.Level1` to `Elevation.Level2` | Shadow depth |

**Variant Styles:**
- **Filled** (default): Subtle elevation, solid background
- **Outlined**: Border instead of shadow
- **Elevated**: Increased shadow for emphasis

### FsEntry

| Property | Recommended Token | Description |
|----------|------------------|-------------|
| `BackgroundColor` | `Color.SurfaceVariant` (filled)<br/>`Transparent` (outlined) | Entry background |
| `TextColor` | `Color.OnSurface` | Input text color |
| `PlaceholderColor` | `Color.OnSurfaceVariant` | Placeholder text |
| `BorderColor` | `Color.Outline` (outlined) | Border color |
| `BorderWidth` | `BorderWidth.Thin` (outlined) | Border thickness |
| `CornerRadius` | `Radius.Small` | Corner radius |
| `Padding` | `Space.16` | Internal padding |
| `FontSize` | `FontSize.BodyMedium` | Input text size |

**Variant Styles:**
- **Filled** (default): Solid subtle background, no border
- **Outlined**: Transparent background with border

### Common Patterns

**Primary Action Button:**
```
BackgroundColor: Color.Primary
TextColor: Color.OnPrimary
CornerRadius: Radius.Small
Padding: Space.16
```

**Card with Standard Elevation:**
```
BackgroundColor: Color.Surface
CornerRadius: Radius.Medium
Padding: Space.16
Elevation: Elevation.Level1
```

**Text Input (Filled):**
```
BackgroundColor: Color.SurfaceVariant
TextColor: Color.OnSurface
CornerRadius: Radius.Small
Padding: Space.16
```

## Designer-to-Developer Workflow

Here's the recommended process for handing off a theme design to developers:

### 1. **Design Phase** (Designer)

- Create mockups in your design tool (Figma, Sketch, Adobe XD)
- Define your color palette with semantic roles (primary, secondary, etc.)
- Specify typography, spacing, and shape values
- Ensure accessibility (contrast ratios, touch target sizes)
- Test in both light and dark modes (if applicable)

### 2. **Documentation Phase** (Designer)

- Use the [Sample Theme Document](#sample-theme-document) template
- List all token values with hex codes
- Include visual examples and mockups
- Add any special notes or constraints
- Share as PDF, Markdown, or Figma file

### 3. **Implementation Phase** (Developer)

- Developer creates a new XAML ResourceDictionary file
- Maps your token values to XAML resources
- Tests the theme with all controls
- Validates accessibility and contrast
- Creates a pull request for review

### 4. **Review Phase** (Designer + Developer)

- Designer reviews the implementation in the running app
- Checks that colors, spacing, and typography match designs
- Identifies any adjustments needed
- Approves final implementation

### Example Handoff Document

**Format:** PDF, Markdown, Figma link, or design system documentation

**Contents:**
1. Theme name and version
2. Complete color palette table (like the sample above)
3. Typography scale (or note if using defaults)
4. Spacing scale (or note if using defaults)
5. Shape/radius values
6. Visual mockups showing key screens
7. Accessibility notes (contrast ratios verified)
8. Dark mode colors (if required)
9. Any special instructions or brand guidelines

**Example Message:**

> Hi [Developer Name],
>
> I've completed the design for our new "Ocean" theme. Please find the attached theme specification document that includes:
> - Complete color palette for light mode (dark mode optional)
> - Typography scale (using defaults with one exception)
> - Visual mockups showing the theme applied to buttons, cards, and forms
>
> All color combinations have been tested for WCAG AA accessibility.
> Let me know if you need any clarifications!
>
> Best,
> [Designer Name]

## Best Practices

### For Designers

1. **✅ Use Semantic Colors Correctly**
   - Primary = Main brand color (actions, emphasis)
   - Secondary = Supporting color (less prominent actions)
   - Tertiary = Accent color (visual variety)
   - Error = Errors and destructive actions only
   - Surface/Background = Content containers and page backgrounds

2. **✅ Maintain Sufficient Contrast**
   - Normal text: 4.5:1 minimum (WCAG AA)
   - Large text (18px+): 3.1 minimum (WCAG AA)
   - Use online contrast checkers to verify

3. **✅ Provide "On" Colors**
   - For every color, define its "On" color (text/icons that sit on top of it)
   - Example: If `Primary = #006A6A`, then `OnPrimary = #FFFFFF`

4. **✅ Test with Real Content**
   - Don't just design with Lorem Ipsum
   - Use actual content to ensure readability

5. **✅ Consider Dark Mode**
   - Even if not required now, plan for it
   - Dark mode often requires inverted relationships (lighter colors become darker)

6. **✅ Stick to the Scale**
   - Use spacing values from the spacing scale (`Space.8`, `Space.16`, etc.)
   - Use font sizes from the typography scale
   - Consistency is key to a polished design

### For Developers

1. **✅ Don't Hardcode Values**
   - Always use `DynamicResource` bindings to tokens
   - This enables runtime theme switching

2. **✅ Respect Semantic Roles**
   - Don't use error colors for success states
   - Don't use primary colors for destructive actions

3. **✅ Test Theme Switching**
   - Ensure the theme can be changed at runtime
   - Verify all controls update correctly

4. **✅ Validate Accessibility**
   - Use accessibility testing tools
   - Test with screen readers

## Advanced: Multi-Brand Themes

If you're designing for multiple brands (e.g., white-label products), you can create multiple theme documents:

**Theme: Brand A (Red)**
- Primary: #D32F2F (Red)
- Secondary: #7B1FA2 (Purple)
- ...

**Theme: Brand B (Blue)**
- Primary: #1976D2 (Blue)
- Secondary: #0288D1 (Light Blue)
- ...

Each theme follows the same structure but with different color values. This allows the same app to support multiple brand identities.

## Tools & Resources

### Recommended Design Tools

- **Figma**: Create design systems with token plugins
- **Adobe XD**: Use design tokens and shared libraries
- **Sketch**: Use symbol overrides for token-like behavior

### Color Tools

- **Material Theme Builder**: [material.io/resources/theme-builder](https://material.io/resources/theme-builder) - Generate Material Design 3 color schemes
- **Coolors**: [coolors.co](https://coolors.co) - Color palette generator
- **Adobe Color**: [color.adobe.com](https://color.adobe.com) - Color wheel and harmony rules

### Accessibility Tools

- **WebAIM Contrast Checker**: [webaim.org/resources/contrastchecker](https://webaim.org/resources/contrastchecker/)
- **Accessible Colors**: [accessible-colors.com](https://accessible-colors.com)
- **Color Oracle**: [colororacle.org](https://colororacle.org) - Simulate color blindness

### Typography Tools

- **Modular Scale**: [modularscale.com](https://modularscale.com) - Calculate typography scales
- **Type Scale**: [type-scale.com](https://type-scale.com) - Visual typography scale generator

## Examples from the Wild

### Example 1: Material Theme (Built-in)

The default Material theme demonstrates best practices:

- Clear semantic color roles
- Consistent spacing scale
- Material Design 3 compliant
- Supports both light and dark modes

### Example 2: Custom Ocean Theme

See the sample document above for a complete custom theme example based on a teal/ocean color scheme.

## Getting Help

- **Questions?** Open a discussion on [GitHub Discussions](https://github.com/matt-goldman/flagstone-ui/discussions)
- **Found an Issue?** Report it on [GitHub Issues](https://github.com/matt-goldman/flagstone-ui/issues)
- **Want to Contribute?** Check the [Contributing Guide](../CONTRIBUTING.md)

## Summary

Creating a Flagstone UI theme is straightforward:

1. ✅ Define your color palette (primary, secondary, surfaces, etc.)
2. ✅ Choose typography and spacing (or use defaults)
3. ✅ Document all token values in a handoff document
4. ✅ Share with your developer for implementation
5. ✅ Review the implementation in the running app

By following this guide, you can create beautiful, consistent, accessible themes that bring your brand to life in .NET MAUI applications.

---

**Next Steps:**

- [View the Token Reference](tokens.md) for complete token documentation
- [Token Catalog System](token-catalog-system.md) - For AI-assisted theme generation
- [See Control Documentation](Controls/) for detailed control properties
- [Check the Quickstart Guide](quickstart.md) to understand the developer perspective
- [Explore Sample Apps](../samples/) to see themes in action

Happy theming! 🎨
