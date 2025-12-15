# Font Handling Enhancement

## Overview

Added comprehensive font extraction and reporting capabilities to the Bootstrap converter. This allows users to discover which fonts are used in a Bootstrap theme and get detailed instructions for registering them in their .NET MAUI applications.

## Motivation

Bootstrap themes often use custom web fonts (like Google Fonts) or specific font stacks. .NET MAUI requires fonts to be:
1. Downloaded and included in the app package
2. Registered in `MauiProgram.cs` with an alias
3. Referenced by the alias in XAML/code

The converter needed a way to extract font information and provide users with actionable instructions for font setup.

## Implementation

### 1. **New Models**

**`FontInformation`** (`Models/FontInformation.cs`):
- Container for all font information
- Lists of font families and download URLs

**`FontFamily`**:
- Font name, source, weights, italic variant
- Suggested alias for MAUI registration
- Auto-sanitization (e.g., "Segoe UI" ? "SegoeUI")

**`FontSource` enum**:
- `GoogleFonts` - Can be downloaded from URL
- `LocalFile` - Referenced in @font-face
- `System` - System font (no download needed)
- `Unknown` - Source unclear from variables alone

### 2. **Font Parser** (`FontParser.cs`)

Extracts font information from Bootstrap CSS/SCSS:

**Three parsing strategies**:

1. **Google Fonts @import detection**:
   ```scss
   @import url('https://fonts.googleapis.com/css2?family=Roboto:wght@400;700&display=swap');
   ```
   Extracts: family name, weights, italic variants, download URL

2. **@font-face rules**:
   ```css
   @font-face {
     font-family: 'CustomFont';
     src: url('fonts/CustomFont-Regular.ttf');
     font-weight: 400;
   }
   ```
   Extracts: family name, source URL, weights

3. **Font family variables**:
   ```scss
   $font-family-base: "Roboto", "Helvetica", sans-serif;
   ```
   Extracts: First explicitly-named font (skips generic families like `sans-serif`)

**Smart handling**:
- Maps system UI aliases (`-apple-system`, `BlinkMacSystemFont`) to `"System"`
- Deduplicates fonts discovered from multiple sources
- Aggregates weights and variants per font family

### 3. **Updated `ConversionOptions`**

Added `IncludeFonts` property (default: `false`):
```csharp
public bool IncludeFonts { get; set; } = false;
```

### 4. **Updated `ConversionResult`**

Added `Fonts` property:
```csharp
public FontInformation? Fonts { get; init; }
```

Populated when `IncludeFonts` is `true`.

### 5. **Service Integration**

`BootstrapConverterService.ConvertAsync()`:
- Calls `FontParser` when `IncludeFonts` is enabled
- Passes font information in result

### 6. **CLI Integration**

**New flag**: `--include-fonts`

```bash
bootstrap-converter convert \
  --input theme.scss \
  --output ./MyTheme \
  --include-fonts
```

**Output includes**:
- Font families discovered
- Source type (Google Fonts, Local, System, Unknown)
- Weights and italic variants
- Download URLs (for Google Fonts)
- Suggested aliases for MAUI registration
- Complete registration instructions with code examples
- ?? License warning

### Example Output

```
Conversion complete!
  Tokens.xaml: ./MyTheme/Tokens.xaml
  Theme.xaml:  ./MyTheme/Theme.xaml
  Styles.xaml: ./MyTheme/Styles.xaml

? Font Setup Required

Font: Roboto
  Source: GoogleFonts
  Weights: 400, 700
  Italic: Yes
  Suggested Alias: "Roboto"

Font: Segoe UI
  Source: System
  Suggested Alias: "SegoeUI"

Download fonts from:
  https://fonts.googleapis.com/css2?family=Roboto:wght@400;700&display=swap

Registration Instructions:
1. Download font files (.ttf or .otf format)
2. Add fonts to your project (e.g., Resources/Fonts/)
3. Register in MauiProgram.cs:

   builder.ConfigureFonts(fonts =>
   {
       fonts.AddFont("Roboto-Regular.ttf", "Roboto");
   });

? Always verify font licenses before using downloaded fonts in your application.
```

## Benefits

### 1. **Discovery**
- Users know exactly which fonts their theme needs
- No guessing about font requirements

### 2. **Actionable Instructions**
- Direct download URLs for Google Fonts
- Complete code examples for registration
- Suggested aliases that match font names

### 3. **Web to MAUI Translation**
- Handles web font stacks intelligently
- Maps system UI aliases to MAUI conventions
- Extracts first meaningful font from stack

### 4. **Safety**
- License warning reminds users to verify usage rights
- Clear distinction between system/downloadable fonts

### 5. **Opt-in**
- Default `false` keeps output minimal
- Only those who need font info see it

## Design Decisions

### Why Default to `false`?

Font information is optional because:
1. **Not always needed**: Many apps use default system fonts
2. **Keeps output clean**: Most users just want tokens
3. **Opt-in philosophy**: Users request what they need

### Why Extract Only First Font?

From font stacks like `"Roboto", "Helvetica", sans-serif`:
- **Web uses fallbacks**: If Roboto fails, use Helvetica, then system sans-serif
- **MAUI is different**: Fonts must be explicitly registered
- **First is intentional**: Theme author's preferred font
- **Users can adjust**: If they prefer a fallback, they can change the alias

### Why Suggested Aliases?

.NET MAUI requires font aliases for registration:
```csharp
fonts.AddFont("Roboto-Regular.ttf", "Roboto");  // "Roboto" is the alias
```

Suggested aliases:
- Match font names when possible
- Remove spaces (e.g., "Segoe UI" ? "SegoeUI")
- Provide consistent naming convention

## Future Enhancements

Possible additions:
1. **Auto-download**: Optionally download Google Fonts to output directory
2. **Font file detection**: If theme references local `.ttf` files, copy them
3. **Font mapping**: Map web fonts to platform-specific alternatives
4. **Font packages**: Suggest NuGet packages for common fonts
5. **License information**: Include license info from Google Fonts API

## Usage Examples

### Basic Conversion (No Fonts)

```bash
bootstrap-converter convert -i theme.scss -o ./MyTheme
```

No font information displayed (default behavior).

### With Font Information

```bash
bootstrap-converter convert -i theme.scss -o ./MyTheme --include-fonts
```

Complete font information and registration instructions shown.

### Multiple Files

```bash
bootstrap-converter convert \
  -i _variables.scss _bootswatch.scss \
  -o ./MyTheme \
  --include-fonts
```

Fonts discovered from both files.

## Testing

To test font extraction:

```bash
# Download a Bootswatch theme with Google Fonts
curl -o flatly_variables.scss https://raw.githubusercontent.com/thomaspark/bootswatch/v5/dist/flatly/_variables.scss
curl -o flatly_bootswatch.scss https://raw.githubusercontent.com/thomaspark/bootswatch/v5/dist/flatly/_bootswatch.scss

# Convert with font info
dotnet run --project tools/FlagstoneUI.BootstrapConverter.Cli -- convert \
  -i flatly_variables.scss flatly_bootswatch.scss \
  -o ./Flatly \
  --include-fonts \
  --verbose
```

Expected output:
- "Lato" font from Google Fonts
- Download URL
- Registration instructions

## Related Files

**New Files**:
- `tools/FlagstoneUI.BootstrapConverter/FontParser.cs`
- `tools/FlagstoneUI.BootstrapConverter/Models/FontInformation.cs`

**Modified Files**:
- `tools/FlagstoneUI.BootstrapConverter/Models/ConversionOptions.cs`
- `tools/FlagstoneUI.BootstrapConverter/BootstrapConverterService.cs`
- `tools/FlagstoneUI.BootstrapConverter.Cli/Commands/ConvertCommand.cs`

## Summary

The font handling enhancement provides users with:
? **Discovery** of theme fonts  
? **Download URLs** for web fonts  
? **Registration instructions** for MAUI  
? **Suggested aliases** for consistency  
? **License warnings** for safety  

All while maintaining a clean, opt-in experience that doesn't clutter the output for users who don't need font information.
