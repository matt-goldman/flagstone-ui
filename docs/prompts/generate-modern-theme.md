# Generate Modern Theme for Flagstone UI

## Objective

Generate a complete "Modern" theme palette for Flagstone UI that follows Material Design 3 semantic token structure. The Modern theme should be clean, minimal, and iOS-inspired with a professional feel suitable for business and productivity applications.

## Design Direction

**Visual Style:**
- Clean and minimal aesthetic inspired by iOS/macOS design language
- Soft, sophisticated color palette based on blues and grays
- Subtle depth through elevation rather than heavy shadows
- Smooth, rounded corners (larger border radius than Material)
- High readability with excellent contrast
- Professional and trustworthy feel

**Color Palette Characteristics:**
- **Primary**: Modern blue (#007AFF-inspired, iOS blue family)
- **Secondary**: Sophisticated gray-blue for supporting elements
- **Tertiary**: Warm accent color (subtle orange/amber for highlights)
- **Surface**: Clean white/near-white in light mode, deep charcoal in dark mode
- **Background**: Very subtle off-white to reduce eye strain
- **Error**: Professional red (less aggressive than Material's red)

**Dark Mode:**
- True dark background (not pure black, but deep charcoal #1C1C1E)
- Reduced color saturation for comfort
- Increased contrast for readability
- Soft glows instead of harsh shadows

## Required Output Format

Generate a JSON file following this exact structure (based on Flagstone UI's token catalog schema):

```json
{
  "$schema": "./tokens-schema.json",
  "version": "0.1.0",
  "lastUpdated": "2025-11-01",
  "baseTokens": {
    "colors": {
      "Color.Primary": {
        "type": "color",
        "defaultValue": "#007AFF",
        "purpose": "Main brand color for primary actions and key UI elements",
        "category": "primary",
        "darkValue": "#0A84FF"
      },
      ... (all 27 color tokens with semantic names)
    },
    "spacing": { ... (copy from Material theme) },
    "typography": { ... (copy from Material theme) },
    "borderRadius": {
      "BorderRadius.None": { "type": "numeric", "value": 0, "unit": "dp", "purpose": "No rounding" },
      "BorderRadius.Small": { "type": "numeric", "value": 8, "unit": "dp", "purpose": "Subtle rounding for small elements" },
      "BorderRadius.Medium": { "type": "numeric", "value": 12, "unit": "dp", "purpose": "Standard rounding for cards and buttons" },
      "BorderRadius.Large": { "type": "numeric", "value": 16, "unit": "dp", "purpose": "Prominent rounding for large surfaces" },
      "BorderRadius.ExtraLarge": { "type": "numeric", "value": 28, "unit": "dp", "purpose": "Maximum rounding for pill-shaped elements" },
      "BorderRadius.Full": { "type": "numeric", "value": 9999, "unit": "dp", "purpose": "Full circular rounding" }
    },
    "borderWidth": { ... (copy from Material theme) },
    "elevation": { ... (copy from Material theme) },
    "padding": { ... (copy from Material theme) },
    "opacity": { ... (copy from Material theme) }
  },
  "controls": {},
  "themeVariants": {
    "light": { "name": "Modern Light", "description": "Clean, professional light theme" },
    "dark": { "name": "Modern Dark", "description": "Comfortable dark theme for extended use" }
  }
}
```

## Color Token Requirements

Generate ALL 27 semantic color tokens following Material Design 3 structure:

### Primary Colors (6 tokens)
- `Color.Primary` - Main brand color
- `Color.OnPrimary` - Text/icons on primary (ensure WCAG AA contrast)
- `Color.PrimaryContainer` - Lighter container variant
- `Color.OnPrimaryContainer` - Text on primary container
- `Color.InversePrimary` - Primary color for inverse surfaces
- *(Include darkValue for each)*

### Secondary Colors (4 tokens)
- `Color.Secondary` - Supporting brand color
- `Color.OnSecondary`
- `Color.SecondaryContainer`
- `Color.OnSecondaryContainer`

### Tertiary Colors (4 tokens)
- `Color.Tertiary` - Accent/highlight color
- `Color.OnTertiary`
- `Color.TertiaryContainer`
- `Color.OnTertiaryContainer`

### Error Colors (4 tokens)
- `Color.Error` - Error/destructive actions
- `Color.OnError`
- `Color.ErrorContainer`
- `Color.OnErrorContainer`

### Surface & Background (9 tokens)
- `Color.Surface`
- `Color.OnSurface`
- `Color.SurfaceVariant`
- `Color.OnSurfaceVariant`
- `Color.Background`
- `Color.OnBackground`
- `Color.InverseSurface`
- `Color.InverseOnSurface`
- `Color.Outline`
- `Color.OutlineVariant`

## Validation Criteria

Your generated theme must:

1. **Pass Schema Validation**
   - All required properties present
   - Correct value types (color strings, numbers)
   - Valid hex color format (#RRGGBB or #AARRGGBB)

2. **Meet Accessibility Standards**
   - WCAG AA contrast ratio (4.5:1) for all text colors
   - WCAG AA contrast ratio (3:1) for large text and UI components
   - Test with online contrast checkers

3. **Provide Complete Dark Mode**
   - Every color token must have a `darkValue`
   - Dark mode should invert surface brightness appropriately
   - Maintain readability in both modes

4. **Follow Semantic Consistency**
   - Primary family: Same hue family, varying lightness
   - Secondary: Harmonious with primary (analogous or complementary)
   - Tertiary: Distinct accent that doesn't clash
   - Error: Universally recognizable as "danger"

## Reference Files

Attach these files from the Flagstone UI repository for context:

1. **`docs/tokens-catalog.json`** - Example Material theme structure
2. **`docs/tokens-schema.json`** - JSON Schema for validation
3. **`docs/tokens.md`** - Token documentation

## Suggested Color Palettes

Choose ONE of these as your starting point, then generate all semantic variants:

### Option A: iOS-Inspired (Recommended)
- Primary: #007AFF (iOS Blue)
- Secondary: #5856D6 (iOS Purple)
- Tertiary: #FF9500 (iOS Orange)
- Surface: #FFFFFF / #1C1C1E
- Background: #F2F2F7 / #000000

### Option B: Professional Gray-Blue
- Primary: #0066CC (Deep Blue)
- Secondary: #6B7280 (Cool Gray)
- Tertiary: #F59E0B (Amber)
- Surface: #FFFFFF / #1F2937
- Background: #F9FAFB / #111827

### Option C: Soft & Warm
- Primary: #3B82F6 (Sky Blue)
- Secondary: #8B5CF6 (Purple)
- Tertiary: #F97316 (Orange)
- Surface: #FFFFFF / #27272A
- Background: #FAFAF9 / #18181B

## Testing Your Output

After generating the JSON:

1. Save as `modern-theme-catalog.json`
2. Validate structure:
   ```bash
   dotnet run --project tools/FlagstoneUI.TokenGenerator -- validate --input modern-theme-catalog.json --json
   ```
3. Generate XAML preview:
   ```bash
   dotnet run --project tools/FlagstoneUI.TokenGenerator -- generate-xaml --input modern-theme-catalog.json --output ModernTheme.xaml
   ```
4. Check contrast ratios at https://webaim.org/resources/contrastchecker/

## Success Criteria

✅ JSON passes validation with no errors
✅ All 27 color tokens defined with defaultValue and darkValue
✅ All text-on-background combinations meet WCAG AA (4.5:1)
✅ Border radius values are larger than Material (more rounded)
✅ Theme feels distinctly "Modern" and different from Material Design
✅ Dark mode is comfortable for extended viewing

## Additional Notes

- **Don't just copy Material colors** - Create a genuinely different palette
- **Test your colors together** - They should feel harmonious as a system
- **Consider color psychology** - Modern theme should feel professional and trustworthy
- **Numeric tokens** - You can copy spacing, typography, elevation, padding, opacity from Material theme (they're universal)
- **Focus on colors and border radius** - These are what make the theme distinctive

## Example Output Snippet

Here's what the first few tokens should look like:

```json
{
  "$schema": "./tokens-schema.json",
  "version": "0.1.0",
  "lastUpdated": "2025-11-01",
  "baseTokens": {
    "colors": {
      "Color.Primary": {
        "type": "color",
        "defaultValue": "#007AFF",
        "purpose": "Main brand color for primary actions and key UI elements. iOS-inspired blue.",
        "category": "primary",
        "darkValue": "#0A84FF"
      },
      "Color.OnPrimary": {
        "type": "color",
        "defaultValue": "#FFFFFF",
        "purpose": "Text and icons displayed on primary color surfaces",
        "category": "primary",
        "darkValue": "#FFFFFF"
      },
      ...
    }
  }
}
```

---

**Pro Tip**: If you're using v0 or GitHub Spark, you can ask it to generate the entire JSON in one go. If using ChatGPT/Claude, you might want to generate colors first, validate them for contrast, then complete the full structure.
