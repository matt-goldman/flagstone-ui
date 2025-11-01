# Generate Theme Palette Variations

## Quick Theme Generation

Use this prompt when you want to quickly generate color palette variations without the full token structure. Generate just the core colors, then map them to the full token schema manually or with AI assistance.

## Prompt Template

Copy and customize this prompt for your AI tool:

---

I need a color palette for a .NET MAUI theme with the following characteristics:

**Theme Name**: [Your Theme Name]

**Design Direction**: [Describe the mood, industry, inspiration - e.g., "Medical app with calming blues and greens, trustworthy and professional"]

**Target Audience**: [Who will use this? e.g., "Financial professionals", "Creative designers", "Healthcare workers"]

**Brand Personality**: [3-5 adjectives describing the feel - e.g., "Modern, minimal, trustworthy, calm, professional"]

**Color Preferences**: [Any specific colors you want to include or avoid]

Generate a complete color palette following Material Design 3 semantic structure with these categories:

1. **Primary Color Family** (4-5 shades from light to dark)
   - Used for main actions, key UI elements, branding

2. **Secondary Color Family** (4-5 shades)
   - Supporting color for less prominent elements

3. **Tertiary/Accent Color** (3-4 shades)
   - Highlights, special actions, visual interest

4. **Error/Warning Colors** (3-4 shades)
   - Destructive actions, alerts, validation errors

5. **Surface & Background**
   - Light mode: White, off-white, subtle grays
   - Dark mode: Deep charcoals, true blacks, dark grays

6. **Text Colors**
   - High emphasis, medium emphasis, disabled
   - For both light and dark backgrounds

**Requirements:**
- Provide hex color codes (#RRGGBB format)
- Include both light mode and dark mode variants
- Ensure WCAG AA contrast ratios (4.5:1 for text, 3:1 for UI components)
- Colors should work harmoniously together
- Dark mode should be comfortable for extended viewing

**Output Format:**
Please provide the palette as a structured list with color names, hex values, and usage descriptions.

---

## Example Themes to Generate

### Healthcare Theme
- **Direction**: Calming, trustworthy, clinical
- **Colors**: Soft blues, medical greens, clean whites
- **Mood**: Professional, reassuring, clean

### Financial Theme
- **Direction**: Sophisticated, stable, premium
- **Colors**: Navy blues, gold accents, deep charcoals
- **Mood**: Trustworthy, professional, luxury

### Creative Theme
- **Direction**: Bold, expressive, energetic
- **Colors**: Vibrant purples, electric blues, warm oranges
- **Mood**: Innovative, dynamic, inspiring

### Nature Theme
- **Direction**: Organic, sustainable, earthy
- **Colors**: Forest greens, earth browns, sky blues
- **Mood**: Natural, grounded, peaceful

### Neon Dark Theme
- **Direction**: High-tech, cyberpunk, futuristic
- **Colors**: Electric cyan, hot magenta, deep blacks
- **Mood**: Modern, edgy, tech-forward

## Mapping Generated Colors to Token Structure

Once you have your palette, use this mapping guide:

```
Generated Color          →  Flagstone Token
──────────────────────────────────────────────
Primary (Main)           →  Color.Primary
Primary (Light)          →  Color.PrimaryContainer
Primary (Dark)           →  Color.Primary (darkValue)
White/Black on Primary   →  Color.OnPrimary

Secondary (Main)         →  Color.Secondary
Secondary (Light)        →  Color.SecondaryContainer
Secondary (Dark)         →  Color.Secondary (darkValue)

Accent/Tertiary          →  Color.Tertiary
Accent Light             →  Color.TertiaryContainer

Error Red                →  Color.Error
Error Light              →  Color.ErrorContainer

Surface White            →  Color.Surface
Surface Dark             →  Color.Surface (darkValue)
Background Off-White     →  Color.Background
Background Dark          →  Color.Background (darkValue)

Text High Emphasis       →  Color.OnSurface
Text Medium Emphasis     →  Color.OnSurfaceVariant
Border/Outline           →  Color.Outline
```

## Quick Validation Checklist

After generating your palette:

- [ ] Primary, Secondary, Tertiary are visually distinct
- [ ] Error color is clearly recognizable as "danger"
- [ ] Dark mode colors are not just inverted (they're adjusted for comfort)
- [ ] Text on Primary background has 4.5:1 contrast ratio
- [ ] Text on Surface has 4.5:1 contrast ratio
- [ ] UI elements have 3:1 contrast ratio minimum
- [ ] Palette feels cohesive and harmonious
- [ ] Colors match the intended brand personality

## Contrast Testing Tools

Use these free tools to verify your color combinations:

- **WebAIM Contrast Checker**: https://webaim.org/resources/contrastchecker/
- **Coolors**: https://coolors.co/contrast-checker
- **Accessible Colors**: https://accessible-colors.com/

## Next Steps

1. Generate palette with AI tool
2. Verify contrast ratios
3. Map to Flagstone token structure using the mapping guide above
4. Create full `tokens-catalog.json` file
5. Validate with Flagstone tools:
   ```bash
   dotnet run --project tools/FlagstoneUI.TokenGenerator -- validate --input your-theme.json
   ```

## Pro Tips

- **Start simple**: Get 5-10 core colors right first
- **Test in context**: View colors next to each other, not in isolation
- **Consider color blindness**: Use tools like Coblis to test accessibility
- **Iterate**: Generate multiple options, mix and match the best colors
- **Use AI iteratively**: "Make the primary color warmer", "Increase contrast for dark mode"
