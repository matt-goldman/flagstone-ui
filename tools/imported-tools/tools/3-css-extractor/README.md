# Tool 3 - CSS Computer

## Purpose

Computes a canonical **Design Language Specification (DLS)** from HTML and CSS by resolving all styling via proper CSS cascade into explicit, platform-agnostic visual design semantics.

## Key Features

- ✅ **Proper CSS Cascade Resolution** - Implements CSS specificity and cascade rules
- ✅ **Vanilla HTML/CSS Support** - Works with traditional websites out of the box
- ✅ **React/Next.js Support** - Handles JSX/TSX files with `className` attributes
- ✅ **External CSS Support** - Parses `.css` files, `<style>` tags, and inline styles
- ✅ **Compiled CSS Detection** - Automatically finds build output from Next.js, Vite, etc.
- ✅ **Standards-Compliant** - Uses CSS selectors, specificity calculation, proper cascade
- ✅ **Deterministic** - Same input always produces same output

## Important: Tailwind/Utility-Class Projects

**This tool computes styles, it does not translate them.**

For projects using Tailwind CSS or other utility-class frameworks, you **must run the CSS build step first** before using this tool. The tool looks for compiled CSS in standard build output locations:

- `.next/static/css/` or `.next/static/chunks/` (Next.js / Turbopack)
- `dist/assets/` (Vite)
- `build/static/css/` (Create React App)
- `dist/` or `build/` (generic)

### Example workflow for Tailwind projects:

```bash
# 1. Build your CSS first
npm run build
# or for standalone Tailwind:
npx tailwindcss -i ./src/input.css -o ./dist/output.css

# 2. Then run this tool
dotnet run --project src/CssComputer.Cli -- ./my-project --out ./dls.json
```

The tool will automatically detect and use compiled CSS files from these locations. If no compiled CSS is found, it falls back to source CSS files (which works for traditional CSS projects but not for utility-class frameworks).

## Architecture

- **CssComputer.Core**: Class library containing all computation logic
- **CssComputer.Cli**: CLI wrapper for command-line usage

## Processing Pipeline

1. **Stage 1 - Style Resolution**: Computes styles via CSS cascade
   - Parses HTML to identify elements
   - Parses external CSS files and inline `<style>` tags
   - Matches selectors to elements
   - Applies cascade (specificity, source order)
   - Applies inline styles (highest priority)
2. **Stage 2 - Normalization**: Canonicalizes values, removes defaults
3. **Stage 3 - Grouping**: Groups similar styles conservatively
4. **Stage 4 - Variant Detection**: Detects style variants as deltas

## Usage

```bash
dotnet run --project src/CssComputer.Cli -- <input-path> [options]
```

### Options

- `--out <path>`: Output path for DLS JSON (default: ./dls.json)
- `--report <path>`: Output path for computation report (default: ./computation-report.json)
- `--emit-css`: Emit optional CSS projection for inspection
- `--tolerance <value>`: Numeric tolerance for style grouping (default: 0.0)

### Examples

```bash
# Process HTML file with external CSS
dotnet run --project src/CssComputer.Cli -- ./index.html --out ./dls.json

# Process directory with all HTML/CSS files
dotnet run --project src/CssComputer.Cli -- ./src --out ./dls.json --emit-css

# Process a Next.js project (after running `npm run build`)
dotnet run --project src/CssComputer.Cli -- ./my-nextjs-app --out ./dls.json

# With custom options
dotnet run --project src/CssComputer.Cli -- ./input \
  --out ./output/dls.json \
  --report ./output/report.json \
  --emit-css \
  --tolerance 0.1
```

## Input

Supports any HTML/CSS combination:

**Markup Files:**
- `*.html` - Standard HTML
- `*.htm` - Standard HTML  
- `*.jsx` - React JSX (className converted to class for parsing)
- `*.tsx` - React TSX (className converted to class for parsing)

**CSS Sources (in priority order):**
1. Compiled CSS from build outputs (`.next/`, `dist/`, `build/`)
2. External `.css` files (via `<link rel="stylesheet">` or `import` statements)
3. Inline `<style>` tags
4. Inline `style` attributes

## Output

1. **DLS (JSON)**: Canonical design language specification
2. **Report (JSON)**: Metadata about computation process
3. **CSS Projection (optional)**: Lossy CSS output for inspection only

## Building

```bash
dotnet build
```

## Design Principles

- **CSS Cascade Computation**: Proper CSS specificity and cascade resolution
- **Computed, Not Translated**: Styles are resolved to final computed values
- **Deterministic**: Same input produces same output
- **Conservative**: Preserve distinct styles when uncertain
- **Auditable**: All decisions recorded in metadata
- **Standards-Compliant**: Follows W3C CSS specifications

## Dependencies

- **AngleSharp** - HTML parsing and CSS selector matching
- **ExCSS** - CSS stylesheet parsing

## Limitations

- **Requires compiled CSS for utility frameworks**: Tailwind, Bootstrap utility classes, etc. must be compiled before processing
- **No pseudo-class states**: :hover, :focus, :active states are not processed
- **No media queries**: Responsive breakpoints are not evaluated
- **No CSS preprocessing**: Sass, Less must be compiled to CSS first

See `IMPLEMENTATION.md` for detailed architecture and `_docs/css-computerv1_1.md` for full specification.
