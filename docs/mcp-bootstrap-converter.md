# Bootstrap Theme Converter MCP

## Overview

An MCP (Model Context Protocol) server that converts Bootstrap themes into Flagstone UI token definitions and theme files. This enables developers to use existing Bootstrap themes with .NET MAUI applications, bridging the gap between web and mobile styling.

## Goals

1. **Primary**: Convert Bootstrap theme variables to Flagstone UI tokens
2. **Secondary**: Generate complete Flagstone theme files from Bootstrap themes
3. **Tertiary**: Enable theme switching demonstrations
4. **Future**: Serve as reference for Tailwind converter

## Value Proposition

- **Reuse existing Bootstrap themes** in .NET MAUI apps
- **Consistent styling** across web and mobile applications
- **Faster theme development** by leveraging Bootstrap ecosystem
- **Demonstrates MCP tooling** as core differentiator for Flagstone UI

## Architecture

### MCP Server Structure

```
flagstone-bootstrap-mcp/
├── src/
│   ├── server.ts (or .js)          # MCP server implementation
│   ├── parser.ts                    # Bootstrap variable parser
│   ├── mapper.ts                    # Bootstrap → Flagstone mapping
│   └── generator.ts                 # XAML theme file generator
├── resources/
│   ├── mappings.json                # Bootstrap variable mappings
│   └── templates/
│       ├── Tokens.xaml.template     # Token file template
│       └── Theme.xaml.template      # Theme file template
├── tests/
│   └── fixtures/
│       ├── bootstrap-default.scss   # Test Bootstrap themes
│       └── bootstrap-custom.css
└── package.json
```

### MCP Tools Exposed

#### 1. `bootstrap_parse`

**Purpose**: Parse Bootstrap theme file and extract variables

**Input**:

```json
{
  "source": "url|file|content",
  "value": "https://bootswatch.com/5/darkly/bootstrap.css"
}
```

**Output**:

```json
{
  "variables": {
    "colors": {
      "primary": "#375a7f",
      "secondary": "#444",
      "success": "#00bc8c",
      "danger": "#e74c3c",
      "warning": "#f39c12",
      "info": "#3498db",
      "light": "#adb5bd",
      "dark": "#303030"
    },
    "typography": {
      "font-family-base": "-apple-system, BlinkMacSystemFont, 'Segoe UI'",
      "font-size-base": "1rem",
      "line-height-base": "1.5"
    },
    "spacing": {
      "spacer": "1rem"
    },
    "borders": {
      "border-radius": "0.25rem",
      "border-width": "1px"
    }
  }
}
```

#### 2. `bootstrap_to_tokens`

**Purpose**: Convert Bootstrap variables to Flagstone tokens XAML

**Input**:

```json
{
  "variables": { /* from bootstrap_parse */ },
  "options": {
    "namespace": "FlagstoneUI.Themes.Bootstrap",
    "includeComments": true,
    "darkMode": "auto|manual|none"
  }
}
```

**Output**:

```xml
<?xml version="1.0" encoding="utf-8"?>
<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
    
    <!-- Color Tokens (converted from Bootstrap variables) -->
    <Color x:Key="Color.Primary">#375a7f</Color>
    <Color x:Key="Color.Primary.Dark">#2a4460</Color>
    
    <Color x:Key="Color.Secondary">#444444</Color>
    <Color x:Key="Color.Secondary.Dark">#333333</Color>
    
    <!-- ... additional tokens ... -->
    
</ResourceDictionary>
```

#### 3. `bootstrap_to_theme`

**Purpose**: Generate complete Flagstone theme from Bootstrap

**Input**:

```json
{
  "variables": { /* from bootstrap_parse */ },
  "themeName": "Bootstrap Darkly",
  "baseTheme": "Material|Modern|None",
  "options": {
    "includeVisualStates": true,
    "includeControlStyles": ["FsEntry", "FsButton", "FsCard"]
  }
}
```

**Output**: Complete `Theme.xaml` file content with:

- Token imports
- Base styles for specified controls
- Visual state definitions
- Bootstrap-specific adjustments

#### 4. `bootstrap_list_themes`

**Purpose**: List popular Bootstrap themes from CDNs/libraries

**Input**:

```json
{
  "source": "bootswatch|themesberg|default"
}
```

**Output**:

```json
{
  "themes": [
    {
      "name": "Darkly",
      "url": "https://bootswatch.com/5/darkly/bootstrap.css",
      "preview": "https://bootswatch.com/5/darkly/",
      "colors": {
        "primary": "#375a7f",
        "accent": "#00bc8c"
      }
    },
    // ... more themes
  ]
}
```

## Bootstrap → Flagstone Mapping

### Color Mapping

| Bootstrap Variable | Flagstone Token | Notes |
|-------------------|-----------------|-------|
| `--bs-primary` | `Color.Primary` | Direct mapping |
| `--bs-secondary` | `Color.Secondary` | Direct mapping |
| `--bs-success` | `Color.Success` | Direct mapping |
| `--bs-danger` | `Color.Error` | Semantic mapping |
| `--bs-warning` | `Color.Warning` | Direct mapping |
| `--bs-info` | `Color.Info` | Direct mapping |
| `--bs-light` | `Color.Surface` | Semantic mapping |
| `--bs-dark` | `Color.SurfaceVariant.Dark` | Semantic mapping |
| `--bs-body-bg` | `Color.Background` | Semantic mapping |
| `--bs-body-color` | `Color.OnBackground` | Semantic mapping |
| `--bs-border-color` | `Color.Outline` | Semantic mapping |

### Dark Mode Strategy

Bootstrap doesn't have built-in dark mode variables, so we need to generate them:

1. **Auto-generate**: Use color manipulation to create dark variants
   - Darken light colors by 20%
   - Lighten dark colors by 20%
   - Invert surface/background relationships

2. **Manual specification**: Allow user to provide dark mode values

3. **None**: Single theme only (user handles dark mode elsewhere)

### Typography Mapping

| Bootstrap Variable | Flagstone Token | Conversion |
|-------------------|-----------------|------------|
| `--bs-font-family-sans-serif` | `FontFamily.Default` | CSS font stack → MAUI font names |
| `--bs-font-size-base` | `FontSize.Body` | rem → absolute pixels (16px base) |
| `--bs-font-weight-normal` | `FontWeight.Regular` | CSS weight → MAUI weight |
| `--bs-font-weight-bold` | `FontWeight.Bold` | CSS weight → MAUI weight |
| `--bs-line-height-base` | `LineHeight.Default` | Unitless → pixels |
| `--bs-headings-font-family` | `FontFamily.Heading` | CSS font stack → MAUI font names |

### Spacing Mapping

| Bootstrap Variable | Flagstone Token | Conversion |
|-------------------|-----------------|------------|
| `--bs-spacer` (1rem) | `Spacing.Medium` | Base unit |
| `--bs-spacer * 0.25` | `Spacing.ExtraSmall` | 0.25rem |
| `--bs-spacer * 0.5` | `Spacing.Small` | 0.5rem |
| `--bs-spacer * 1.5` | `Spacing.Large` | 1.5rem |
| `--bs-spacer * 3` | `Spacing.ExtraLarge` | 3rem |

### Border Mapping

| Bootstrap Variable | Flagstone Token | Conversion |
|-------------------|-----------------|------------|
| `--bs-border-radius` | `CornerRadius.Medium` | CSS px → MAUI CornerRadius |
| `--bs-border-radius-sm` | `CornerRadius.Small` | CSS px → MAUI CornerRadius |
| `--bs-border-radius-lg` | `CornerRadius.Large` | CSS px → MAUI CornerRadius |
| `--bs-border-width` | `BorderWidth.Default` | CSS px → double |

## Implementation Phases

### Phase 1: Core Parser (Days 1-2)

- Bootstrap CSS/SCSS variable parser
- Support CSS custom properties (`--bs-*`)
- Support SCSS variables (`$primary`)
- Extract color, typography, spacing, border values
- Unit conversion utilities (rem → px, etc.)

**Deliverable**: `bootstrap_parse` tool working with test fixtures

### Phase 2: Token Generator (Day 2-3)

- Bootstrap → Flagstone token mapping
- Dark mode generation strategies
- XAML token file generation
- Validation of generated tokens

**Deliverable**: `bootstrap_to_tokens` tool generating valid Tokens.xaml

### Phase 3: Theme Generator (Day 3-4)

- Complete theme file generation
- Control style templates
- Visual state definitions based on Bootstrap colors
- Integration with existing Material/Modern themes

**Deliverable**: `bootstrap_to_theme` tool generating working themes

### Phase 4: Integration & Demo (Day 4-5)

- Theme switching mechanism in ThemePlayground
- Bootstrap theme gallery (Bootswatch integration)
- Documentation and examples
- .NET 10 launch blog post

**Deliverable**: Working demo switching between Material/Modern/Bootstrap themes

## File Format Support

### Input Formats

1. **Bootstrap CSS** (CSS custom properties)

   ```css
   :root {
     --bs-primary: #375a7f;
     --bs-secondary: #444;
   }
   ```

2. **Bootstrap SCSS** (SCSS variables)

   ```scss
   $primary: #375a7f;
   $secondary: #444;
   ```

3. **Bootstrap JSON** (custom format for easy parsing)

   ```json
   {
     "primary": "#375a7f",
     "secondary": "#444"
   }
   ```

### Output Formats

1. **Tokens.xaml** - Token definitions only
2. **Theme.xaml** - Complete theme with styles
3. **Combined** - Both in ResourceDictionary structure

## Usage Examples

### Example 1: Convert Bootswatch Theme

```typescript
// 1. Parse Bootswatch Darkly theme
const variables = await mcp.call('bootstrap_parse', {
  source: 'url',
  value: 'https://bootswatch.com/5/darkly/bootstrap.css'
});

// 2. Generate tokens
const tokens = await mcp.call('bootstrap_to_tokens', {
  variables: variables,
  options: {
    darkMode: 'auto',
    includeComments: true
  }
});

// 3. Generate complete theme
const theme = await mcp.call('bootstrap_to_theme', {
  variables: variables,
  themeName: 'Bootstrap Darkly',
  baseTheme: 'Material',
  options: {
    includeControlStyles: ['FsEntry', 'FsButton', 'FsCard']
  }
});

// 4. Save files
fs.writeFileSync('Tokens.Bootstrap.Darkly.xaml', tokens);
fs.writeFileSync('Theme.Bootstrap.Darkly.xaml', theme);
```

### Example 2: Custom Bootstrap Theme

```typescript
// User provides custom Bootstrap SCSS
const customScss = `
$primary: #ff6b6b;
$secondary: #4ecdc4;
$font-family-base: 'Inter', sans-serif;
`;

const variables = await mcp.call('bootstrap_parse', {
  source: 'content',
  value: customScss
});

const theme = await mcp.call('bootstrap_to_theme', {
  variables: variables,
  themeName: 'Custom Brand',
  baseTheme: 'None'
});
```

### Example 3: Theme Gallery

```typescript
// List available Bootstrap themes
const themes = await mcp.call('bootstrap_list_themes', {
  source: 'bootswatch'
});

// Let user pick one
const selected = themes.themes[0]; // "Darkly"

// Convert to Flagstone theme
const variables = await mcp.call('bootstrap_parse', {
  source: 'url',
  value: selected.url
});

const theme = await mcp.call('bootstrap_to_theme', {
  variables: variables,
  themeName: `Bootstrap ${selected.name}`
});
```

## Testing Strategy

### Unit Tests

- Bootstrap variable parsing (CSS, SCSS, JSON)
- Color mapping and conversion
- Dark mode generation algorithms
- Typography font stack conversion
- Unit conversion (rem, em, px)
- XAML generation and validation

### Integration Tests

- Full pipeline: Bootstrap CSS → Flagstone theme
- Theme switching in sample app
- Visual regression tests (screenshots)
- Performance (parsing large Bootstrap files)

### Test Fixtures

- Bootstrap 5 default theme
- Bootswatch themes (5-10 popular ones)
- Custom Bootstrap themes with edge cases
- Minimal theme (only required variables)
- Maximal theme (all possible variables)

## MCP Server Configuration

### Server Metadata

```json
{
  "name": "flagstone-bootstrap-mcp",
  "version": "0.1.0",
  "description": "Convert Bootstrap themes to Flagstone UI themes",
  "author": "Matt Goldman",
  "license": "MIT",
  "repository": "https://github.com/matt-goldman/flagstone-bootstrap-mcp"
}
```

### Tool Definitions

Each tool will be registered with:

- Name and description
- Input schema (JSON Schema)
- Output schema
- Examples
- Error handling

## Error Handling

### Parser Errors

- **Invalid Bootstrap format**: Clear message about supported formats
- **Missing required variables**: Provide defaults or fail gracefully
- **Malformed CSS/SCSS**: Line number and error context

### Conversion Errors

- **Unsupported color format**: Fallback to nearest supported format
- **Invalid font family**: Use system default
- **Missing dark mode values**: Generate or warn

### Generation Errors

- **Invalid XAML**: Syntax validation before returning
- **Duplicate token names**: Rename with suffix
- **Circular dependencies**: Detect and report

## Future Enhancements

### Version 2

- Support for Bootstrap 4 themes
- Custom mapping configuration (user-defined mappings)
- Theme variants (light/dark/high-contrast from single source)
- Real-time preview in VS Code extension
- Bootstrap component → Flagstone control mapping

### Version 3

- Tailwind CSS support (separate MCP server)
- Material Design theme import
- Figma token integration
- Design system synchronization

## Dependencies

### Required

- TypeScript/Node.js (MCP server)
- CSS parser library (postcss or similar)
- SCSS parser (sass.js)
- Color manipulation library (chroma.js or tinycolor)
- XML generation (fast-xml-parser)

### Optional

- Bootstrap CDN client (for theme gallery)
- VS Code extension (future GUI)

## Documentation Deliverables

1. **README.md** - Quick start and examples
2. **API.md** - Complete MCP tool documentation
3. **MAPPINGS.md** - Detailed variable mapping reference
4. **CONTRIBUTING.md** - How to add new mappings
5. **Blog Post** - .NET 10 launch announcement

## Success Metrics

### MVP (Week 1)

- ✅ Parse Bootstrap 5 default theme
- ✅ Generate valid Tokens.xaml
- ✅ Generate working Theme.xaml
- ✅ Demo theme switching with 3+ Bootstrap themes
- ✅ Documentation complete

### Post-MVP (Month 1)

- 📊 10+ Bootstrap themes converted successfully
- 📊 Community contributed custom themes
- 📊 GitHub stars/engagement
- 📊 Blog post views/shares

## Open Questions

1. **Font family handling**: How to map web fonts to MAUI fonts?
   - Option A: Document required font installation
   - Option B: Fallback to system fonts
   - Option C: Bundle fonts with theme

2. **Component-specific styling**: Bootstrap has component styles (buttons, cards), should we map these?
   - Option A: Generate full control styles
   - Option B: Tokens only, let themes handle styles
   - **Recommendation**: Option B (MVP), Option A (V2)

3. **CSS variable fallbacks**: Bootstrap uses `var(--bs-primary, fallback)`, how to handle?
   - Option A: Resolve at parse time
   - Option B: Convert to XAML DynamicResource
   - **Recommendation**: Option A (simpler)

4. **Responsive spacing**: Bootstrap has breakpoint-based spacing, relevant for MAUI?
   - Option A: Ignore (mobile doesn't have breakpoints like web)
   - Option B: Map to idiom-specific resources
   - **Recommendation**: Option A (MVP)

5. **Shadows**: Bootstrap uses box-shadow, MAUI uses Shadow element, how to convert?
   - Option A: Parse shadow syntax, generate Shadow properties
   - Option B: Approximate with Border/opacity
   - **Recommendation**: Option A (more accurate)

## Next Steps

1. **Review this spec** - Validate approach and scope
2. **Set up MCP server project** - Bootstrap TypeScript project
3. **Implement Phase 1** - Parser with test fixtures
4. **Create mapping reference** - Document all Bootstrap → Flagstone mappings
5. **Build Phase 2** - Token generator
6. **Integrate with ThemePlayground** - Theme switching demo
7. **Documentation** - Complete API docs and examples
8. **Blog post** - Write .NET 10 launch announcement

## Timeline

- **Day 1-2**: Parser + token generator
- **Day 3-4**: Theme generator + integration
- **Day 5**: Documentation + demo polish
- **Target**: Ready for .NET 10 launch (November 11-12, 2025)
