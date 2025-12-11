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
- `-a, --analysis-mode`: Analysis mode: `css`, `variables`, or `hybrid` (default: hybrid)
- `-f, --format`: Input format: `css`, `scss`, or `auto` (default: auto)
- `-d, --dark-mode`: Dark mode generation: `auto`, `manual`, or `none` (default: auto)
- `-n, --namespace`: XAML namespace for generated resources (default: FlagstoneUI.Resources)
- `-c, --comments`: Include purpose comments in generated XAML (default: true)
- `-v, --verbose`: Enable verbose output
- `--debug`: Enable debug logging (shows all discovered variables)

#### Analysis Modes

**`variables` (Recommended for Bootstrap 5+)**
- Parses SCSS variables (`$primary`, `$font-size-base`, etc.)
- ✅ Works perfectly with Bootswatch themes
- ✅ Production-ready with excellent token coverage
- Example: `--analysis-mode variables`

**`css` (Limited - Bootstrap 4 Only)**
- Analyzes CSS classes (`.btn-primary`, `.card`, etc.)
- ⚠️ ExCSS 4.2.3 doesn't parse CSS custom properties (`--bs-*`)
- ⚠️ Bootstrap 5+ uses CSS custom properties exclusively
- Example: `--analysis-mode css`

**`hybrid` (Fallback Strategy)**
- Combines both CSS and variables analysis
- CSS tokens take precedence when conflicts occur
- Use when both compiled CSS and SCSS source available
- Example: `--analysis-mode hybrid`

#### Examples

**Convert Bootswatch theme (Recommended):**

```bash
bootstrap-converter convert \
  -i _variables.scss _bootswatch.scss \
  -o ./themes \
  --analysis-mode variables \
  --verbose
```

This is the **recommended approach** because:
- ✅ Bootstrap 5+ uses CSS custom properties in compiled CSS
- ✅ SCSS source files contain actual color/spacing values
- ✅ Variables mode produces excellent token coverage (11+ colors, 3+ typography, 5+ spacing)
- `_variables.scss` contains theme color values
- `_bootswatch.scss` contains font imports and custom overrides
- Variable references are automatically resolved (e.g., `$success: $green` → `#56cc9d`)

**Convert from local CSS file:**

```bash
bootstrap-converter convert -i ./bootstrap.css -o ./themes
```

**Convert from URL with verbose output:**

```bash
bootstrap-converter convert -i https://example.com/bootstrap.css -o ./themes --verbose
```

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

The converter generates three XAML files:

### Tokens.xaml

Contains all design tokens as resource dictionary entries:

```xaml
<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
                    x:Class="FlagstoneUI.Resources.Tokens">
    <!-- Color Tokens -->
    <Color x:Key="Color.Primary">#375A7F</Color>
    <Color x:Key="Color.Secondary">#444444</Color>
    <Color x:Key="Color.Success">#00BC8C</Color>
    <Color x:Key="Color.Error">#E74C3C</Color>
    
    <!-- Typography Tokens -->
    <x:String x:Key="FontFamily.Default">System</x:String>
    <x:Double x:Key="FontSize.Body">15</x:Double>
    <x:Double x:Key="LineHeight.Default">1.5</x:Double>
    
    <!-- Spacing Tokens -->
    <x:Double x:Key="Spacing.ExtraSmall">4</x:Double>
    <x:Double x:Key="Spacing.Small">8</x:Double>
    <x:Double x:Key="Spacing.Medium">16</x:Double>
    <x:Double x:Key="Spacing.Large">24</x:Double>
    <x:Double x:Key="Spacing.ExtraLarge">48</x:Double>
    
    <!-- Border Radius Tokens -->
    <x:Double x:Key="Radius.Small">3.2</x:Double>
    <x:Double x:Key="Radius.Medium">4</x:Double>
    <x:Double x:Key="Radius.Large">4.8</x:Double>
    
    <!-- Border Width Tokens -->
    <x:Double x:Key="BorderWidth.Default">1</x:Double>
</ResourceDictionary>
```

### Styles.xaml

Contains control styles that reference tokens using `DynamicResource`:

```xaml
<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
                    xmlns:fs="clr-namespace:FlagstoneUI.Core.Controls;assembly=FlagstoneUI.Core"
                    x:Class="FlagstoneUI.Resources.Styles">
    
    <!-- Default Button Style (Filled Primary) -->
    <Style TargetType="fs:FsButton">
        <Setter Property="BackgroundColor" Value="{DynamicResource Color.Primary}" />
        <Setter Property="TextColor" Value="#FFFFFF" />
        <Setter Property="CornerRadius" Value="{DynamicResource Radius.Medium}" />
        <Setter Property="Padding" Value="12,6" />
        <Setter Property="VisualStateManager.VisualStateGroups">
            <VisualStateGroupList>
                <VisualStateGroup x:Name="CommonStates">
                    <VisualState x:Name="Normal" />
                    <VisualState x:Name="Disabled">
                        <VisualState.Setters>
                            <Setter Property="Opacity" Value="0.5" />
                        </VisualState.Setters>
                    </VisualState>
                </VisualStateGroup>
            </VisualStateGroupList>
        </Setter>
    </Style>
    
    <!-- Outlined Button Style -->
    <Style x:Key="OutlinedButton" TargetType="fs:FsButton">
        <Setter Property="BackgroundColor" Value="Transparent" />
        <Setter Property="TextColor" Value="{DynamicResource Color.Primary}" />
        <Setter Property="BorderColor" Value="{DynamicResource Color.Primary}" />
        <Setter Property="BorderWidth" Value="{DynamicResource BorderWidth.Default}" />
        <!-- ... -->
    </Style>
    
    <!-- Text Button Style -->
    <Style x:Key="TextButton" TargetType="fs:FsButton">
        <Setter Property="BackgroundColor" Value="Transparent" />
        <Setter Property="TextColor" Value="{DynamicResource Color.Primary}" />
        <!-- ... -->
    </Style>
</ResourceDictionary>
```

### Theme.xaml

Merges all resource dictionaries together:

```xaml
<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
                    x:Class="FlagstoneUI.Resources.Theme">
    <ResourceDictionary.MergedDictionaries>
        <ResourceDictionary Source="Tokens.xaml" />
        <ResourceDictionary Source="Styles.xaml" />
    </ResourceDictionary.MergedDictionaries>
</ResourceDictionary>
```

## Integration with Flagstone UI

Add the generated files to your Flagstone UI project. The recommended location is `src/FlagstoneUI.Themes.YourTheme/`:

**Project structure:**
```
src/FlagstoneUI.Themes.YourTheme/
├── Tokens.xaml (generated)
├── Tokens.xaml.cs (generated code-behind)
├── Styles.xaml (generated)
├── Styles.xaml.cs (generated code-behind)
├── Theme.xaml (generated)
└── Theme.xaml.cs (generated code-behind)
```

**Use in MAUI App.xaml:**

```xaml
<Application xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:yourtheme="clr-namespace:FlagstoneUI.Themes.YourTheme;assembly=FlagstoneUI.Themes.YourTheme"
             x:Class="YourApp.App">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <yourtheme:Theme />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
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

### Completed ✅

- [x] Support for Bootstrap 5.x SCSS variables
- [x] Top-down CSS class analysis (BootstrapCssAnalyzer)
- [x] Bottom-up SCSS variable mapping (BootstrapParser)
- [x] Hybrid analysis mode
- [x] Control style generation (FsButton variants)
- [x] Multi-file SCSS parsing with variable resolution
- [x] Dark mode color generation

### In Progress 🚧

- [ ] Expand component coverage (forms, cards, navigation)
- [ ] Bootstrap 4.x support (CSS mode)

### Planned 📋

- [ ] Custom token name mappings via config file
- [ ] Bootswatch theme catalog integration
- [ ] Direct URL fetching from CDN
- [ ] Interactive mode for customization
- [ ] Theme preview generation
- [ ] Batch conversion support

### Known Limitations ⚠️

- **ExCSS 4.2.3**: Doesn't parse CSS custom properties (`--bs-*`)
  - **Impact**: CSS analysis mode doesn't work with Bootstrap 5+ compiled CSS
  - **Workaround**: Use `variables` mode with SCSS source files (production-ready)
  - **Future**: Consider upgrading ExCSS or implementing custom CSS variable parser
