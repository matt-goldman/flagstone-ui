# Bootstrap to Flagstone UI Converter

A .NET command-line tool that converts Bootstrap CSS/SCSS themes to Flagstone UI XAML resource dictionaries.

## Installation

### As a .NET Tool (Recommended)

```bash
dotnet tool install --global FlagstoneUI.BootstrapConverter.Cli
```

### From Source

```bash
cd tools/FlagstoneUI.BootstrapConverter.Cli
dotnet build -c Release
```

## Usage

### Convert Command

Convert a Bootstrap theme to Flagstone UI XAML:

```bash
bootstrap-converter convert -i path/to/bootstrap.css -o ./output
```

#### Options

- `-i, --input` (REQUIRED): Path(s) to Bootstrap CSS/SCSS file(s) or URL(s). Multiple files will be merged.
- `-o, --output`: Output directory for generated XAML files (default: current directory)
- `-f, --format`: Input format: `css`, `scss`, or `auto` (default: auto)
- `-d, --dark-mode`: Dark mode generation: `auto`, `manual`, or `none` (default: auto)
- `-n, --namespace`: XAML namespace for generated resources (default: FlagstoneUI.Resources)
- `-c, --comments`: Include purpose comments in generated XAML (default: true)
- `-v, --verbose`: Enable verbose output
- `--debug`: Enable debug logging (shows all discovered variables)

#### Examples

**Convert from local CSS file:**

```bash
bootstrap-converter convert -i ./bootstrap.css -o ./themes
```

**Convert from URL with verbose output:**

```bash
bootstrap-converter convert -i https://example.com/bootstrap.css -o ./themes --verbose
```

**Convert multiple files (Bootswatch theme):**

```bash
bootstrap-converter convert -i _variables.scss -i _bootswatch.scss -o ./themes
```

This is the **recommended approach for Bootswatch themes** because:
- `_variables.scss` contains actual theme color values
- `_bootswatch.scss` contains font imports and custom overrides
- Variable references are automatically resolved (e.g., `$success: $green` → `#56cc9d`)
- Better typography token extraction (font families, etc.)

**Convert with debug logging:**

```bash
bootstrap-converter convert -i ./bootstrap.scss -o ./themes --debug
```

**Convert SCSS without dark mode:**

```bash
bootstrap-converter convert -i ./custom.scss -o ./themes --dark-mode none
```

**Convert with custom namespace:**

```bash
bootstrap-converter convert -i ./bootstrap.css -o ./themes --namespace MyApp.Themes
```

### Info Command

Display information about a Bootstrap theme without converting:

```bash
bootstrap-converter info -i path/to/bootstrap.css
```

#### Options

- `-i, --input` (REQUIRED): Path to Bootstrap CSS/SCSS file or URL
- `-f, --format`: Input format: `css`, `scss`, or `auto` (default: auto)

#### Example Output

```
Bootstrap Variables Summary
========================================

Colors (11):
  primary                        = #0d6efd
  secondary                      = #6c757d
  success                        = #198754
  ...

Typography (3):
  font-family-base               = -apple-system, BlinkMacSystemFont, "Segoe UI", ...
  font-size-base                 = 1rem
  line-height-base               = 1.5

Total variables: 19
```

## Output Files

The converter generates two XAML files:

### Tokens.xaml

Contains all design tokens as resource dictionary entries:

```xaml
<ResourceDictionary>
    <!-- Color Tokens -->
    <Color x:Key="Color.Primary">#0D6EFD</Color>
    
    <!-- Typography Tokens -->
    <x:Double x:Key="FontSize.Body">16</x:Double>
    
    <!-- Spacing Tokens -->
    <x:Double x:Key="Spacing.Medium">16</x:Double>
    
    <!-- Corner Radius Tokens -->
    <x:Double x:Key="CornerRadius.Medium">6</x:Double>
    
    <!-- Border Width Tokens -->
    <x:Double x:Key="BorderWidth.Default">1</x:Double>
</ResourceDictionary>
```

### Theme.xaml

Contains style definitions that reference the tokens:

```xaml
<ResourceDictionary>
    <ResourceDictionary.MergedDictionaries>
        <ResourceDictionary Source="Tokens.xaml" />
    </ResourceDictionary.MergedDictionaries>
    
    <!-- Control styles will be added here in future versions -->
</ResourceDictionary>
```

## Integration with .NET MAUI

Add the generated files to your MAUI project and merge them in `App.xaml`:

```xaml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="Resources/Themes/Tokens.xaml" />
            <ResourceDictionary Source="Resources/Themes/Theme.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

## Supported Bootstrap Variables

### Colors

- Brand colors: `primary`, `secondary`, `success`, `danger`, `warning`, `info`
- UI colors: `light`, `dark`
- Semantic colors: `body-bg`, `body-color`, `border-color`

### Typography

- Font family: `font-family-base`
- Font size: `font-size-base`
- Line height: `line-height-base`

### Spacing

- Base spacer: `spacer`
- Generated scale: Extra Small, Small, Medium, Large, Extra Large

### Borders

- Border radius: `border-radius`, `border-radius-sm`, `border-radius-lg`
- Border width: `border-width`

## Token Mapping

Bootstrap variables are automatically mapped to Flagstone UI semantic token names:

| Bootstrap | Flagstone UI |
|-----------|--------------|
| `--bs-primary` | `Color.Primary` |
| `--bs-secondary` | `Color.Secondary` |
| `--bs-success` | `Color.Success` |
| `--bs-danger` | `Color.Error` |
| `--bs-warning` | `Color.Warning` |
| `--bs-info` | `Color.Info` |
| `--bs-light` | `Color.Surface` |
| `--bs-dark` | `Color.SurfaceVariant.Dark` |
| `--bs-body-bg` | `Color.Background` |
| `--bs-body-color` | `Color.OnBackground` |
| `--bs-border-color` | `Color.Outline` |
| `--bs-font-family-base` | `FontFamily.Default` |
| `--bs-font-size-base` | `FontSize.Body` |
| `--bs-line-height-base` | `LineHeight.Default` |
| `--bs-spacer` | `Spacing.Medium` (+ scale) |
| `--bs-border-radius` | `CornerRadius.Medium` |
| `--bs-border-radius-sm` | `CornerRadius.Small` |
| `--bs-border-radius-lg` | `CornerRadius.Large` |
| `--bs-border-width` | `BorderWidth.Default` |

## Dark Mode

The converter can automatically generate dark mode color variants:

- **Auto** (default): Automatically darken light colors and lighten dark colors
- **Manual**: Use manually specified dark mode values (future feature)
- **None**: No dark mode support (single theme only)

Dark mode colors are included as comments in the generated XAML:

```xaml
<Color x:Key="Color.Primary">#0D6EFD</Color>
<!-- Dark mode: #108FFF -->
```

## Requirements

- .NET 10.0 or later
- Works with Bootstrap 5.x CSS/SCSS files

## Development

### Building from Source

```bash
git clone https://github.com/matt-goldman/flagstone-ui.git
cd flagstone-ui/tools/FlagstoneUI.BootstrapConverter.Cli
dotnet build
```

### Running Tests

```bash
cd tests/FlagstoneUI.BootstrapConverter.Tests
dotnet test
```

### Packaging

```bash
cd tools/FlagstoneUI.BootstrapConverter.Cli
dotnet pack -c Release
```

## Contributing

Contributions are welcome! Please see the main [Flagstone UI repository](https://github.com/matt-goldman/flagstone-ui) for contribution guidelines.

## License

MIT License - see the [LICENSE](../../LICENSE) file for details.

## Related Projects

- [Flagstone UI](https://github.com/matt-goldman/flagstone-ui) - Open-source UI kit for .NET MAUI
- [Bootstrap](https://getbootstrap.com/) - The world's most popular front-end framework

## Roadmap

- [ ] Support for Bootstrap 4.x
- [ ] Custom token name mappings via config file
- [ ] Bootswatch theme catalog integration
- [ ] Direct URL fetching from CDN
- [ ] Interactive mode for customization
- [ ] Control style generation (buttons, cards, etc.)
- [ ] Theme preview generation
- [ ] Batch conversion support
