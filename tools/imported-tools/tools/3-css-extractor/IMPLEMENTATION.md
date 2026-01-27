# CSS Computer - Implementation Notes

## Overview

This implementation follows the Tool 3 specification (v1.1) for computing a canonical Design Language Specification from normalized prototype source.

**Key Update**: The implementation now uses proper CSS cascade resolution instead of framework-specific utility class mapping, making it framework-agnostic and standards-compliant.

## Architecture

```
CssComputer/
├── Core/                          # Class library
│   ├── Models/                    # Data models
│   │   ├── DesignLanguageSpecification.cs
│   │   ├── Style.cs
│   │   ├── StyleVariant.cs
│   │   ├── ResolvedElement.cs
│   │   ├── ComputationOptions.cs
│   │   └── ComputationReport.cs
│   └── Services/                  # Processing pipeline
│       ├── CssComputerService.cs             (Orchestrator)
│       ├── StyleResolutionService.cs         (Stage 1 - CSS CASCADE)
│       ├── StyleNormalizationService.cs      (Stage 2)
│       ├── StyleGroupingService.cs           (Stage 3)
│       └── VariantDetectionService.cs        (Stage 4)
└── Cli/                           # CLI wrapper
    └── Program.cs
```

## Processing Stages

### Stage 1: Style Resolution (CSS Cascade Implementation)

**Framework-agnostic CSS computation using proper cascade resolution:**

1. **Parse HTML** - Uses AngleSharp to parse HTML/JSX/TSX files
2. **Collect CSS Rules** - Parses external `.css` files and inline `<style>` tags
3. **Match Selectors** - Uses CSS selector matching to find applicable rules
4. **Apply Cascade** - Sorts rules by specificity and applies in correct order
5. **Apply Inline Styles** - Inline `style` attributes have highest priority
6. **Output**: List of ResolvedElement with computed property sets

**Libraries Used:**
- **AngleSharp** - HTML parsing and selector matching
- **ExCSS** - CSS parsing and stylesheet handling

**Key Features:**
- ✓ Proper CSS specificity calculation (IDs > classes > elements)
- ✓ CSS cascade resolution (later rules override earlier)
- ✓ External CSS file support
- ✓ Inline style tag support
- ✓ Inline style attribute support (highest priority)
- ✓ Framework-agnostic (works with any HTML/CSS)

### Stage 2: Normalization
- Removes default/insignificant values (inherit, initial, auto)
- Canonicalizes colors (named → hex, rgb → hex, #abc → #aabbcc)
- Normalizes numeric values and units
- **Output**: Minimal, canonical property sets

### Stage 3: Grouping
- Groups elements with identical property sets
- Optional: Merges similar styles within tolerance
- Conservative approach - preserves distinction when uncertain
- **Output**: List of canonical Style objects

### Stage 4: Variant Detection
- Detects systematic differences between similar styles
- Represents variants as deltas from base style
- Heuristic: ≥70% property match with 1-3 differences
- **Output**: Styles with detected variants

## Key Design Decisions

### CSS Cascade, Not Translation
The implementation computes styles via proper CSS cascade resolution:
- Parses CSS selectors and declarations
- Calculates selector specificity
- Applies rules in correct cascade order
- No framework-specific heuristics

### Framework-Agnostic
Works with any HTML and CSS combination:
- No assumptions about React, Vue, Angular, etc.
- No assumptions about Tailwind, Bootstrap, etc.
- Pure HTML/CSS processing

### Element-First Processing
1. Identify visual elements in HTML
2. Find matching CSS rules via selector matching
3. Compute final styles via cascade
4. No bottom-up inference from utility class names

## Input Support

### HTML/Markup Files
- `.html` - Standard HTML
- `.htm` - Standard HTML
- `.jsx` - React JSX (parsed as HTML)
- `.tsx` - React TSX (parsed as HTML)

### CSS Files
- External `.css` files referenced via `<link rel="stylesheet">`
- Inline `<style>` tags
- Inline `style` attributes

## Out of Scope for This Implementation

These features are explicitly deferred:

1. **Framework-specific utility support** - No special Tailwind/Bootstrap handling
2. **Pseudo-classes & states** - No :hover, :focus, :active, etc.
3. **Responsive design** - No media query resolution
4. **CSS preprocessing** - No Sass/Less support
5. **CSS-in-JS** - No styled-components, emotion, etc.

## Testing

Validated with proper HTML/CSS examples:
- ✓ External CSS file parsing
- ✓ Inline `<style>` tag parsing
- ✓ CSS cascade resolution (specificity, source order)
- ✓ Inline style attributes (highest priority)
- ✓ ID, class, and element selector matching
- ✓ Style property normalization
- ✓ Deterministic output

## Usage Example

```bash
# Process HTML file with external CSS
dotnet run --project src/CssComputer.Cli -- ./input.html --out ./dls.json

# Process directory with all HTML/CSS files
dotnet run --project src/CssComputer.Cli -- ./src --out ./dls.json

# With all options
dotnet run --project src/CssComputer.Cli -- ./input \
  --out ./dls.json \
  --report ./report.json \
  --emit-css \
  --tolerance 0.1
```

## Relationship to Tool 1

Tool 1 (React Component Flattener) normalizes component structure. Tool 3 operates on any HTML output (from Tool 1 or elsewhere) and computes canonical styles via CSS cascade.

**Pipeline**: `Prototype → Tool 1 (optional) → Tool 3 → Tool 4 (future)`

Tool 3 is now standalone and framework-agnostic.
