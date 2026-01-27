# Structural Extractor

A tool to extract canonical, human-readable application structure contracts from normalized React prototypes.

## Overview

The Structural Extractor (Tool 2) sits between Tool 1 (React Component Flattener) and Tool 3 (CSS Extraction) in the Flagstone Pipeline. It analyzes normalized React/JSX/TSX code and produces a framework-agnostic structural contract that captures:

- **Pages** - Named screens with routes
- **Components** - Reusable visual primitives
- **Structural Composition** - Parent/child relationships
- **Navigation** - Entry points and page links

The output is designed to be:
- ✅ Easily understood by humans
- ✅ Easily consumed by LLMs
- ✅ Stable as an interchange artifact between tools
- ✅ Framework-agnostic

## Installation & Setup

### Prerequisites

- .NET 10.0 SDK or later

### Build

```bash
dotnet build

# Or build in Release mode
dotnet build -c Release
```

## Usage

### Basic Usage

```bash
# Process a directory
dotnet run --project src/StructuralExtractor.Cli/StructuralExtractor.Cli.csproj -- ./app

# Process a single file
dotnet run --project src/StructuralExtractor.Cli/StructuralExtractor.Cli.csproj -- page.tsx

# Specify output path
dotnet run --project src/StructuralExtractor.Cli/StructuralExtractor.Cli.csproj -- ./app --out structure.yaml

# Output as JSON instead of YAML
dotnet run --project src/StructuralExtractor.Cli/StructuralExtractor.Cli.csproj -- ./app --format json --out structure.json
```

### Command-Line Options

```
structural-extractor <input> [options]

Arguments:
  <input>              Input file or directory path

Options:
  --out, -o <path>     Output file path (default: ./structure.yaml)
  --format, -f <fmt>   Output format: yaml or json (default: yaml)
  --help, -h           Show help message
```

### Examples

```bash
# Extract structure from a Next.js app directory
dotnet run --project src/StructuralExtractor.Cli -- ./app --out app-structure.yaml

# Extract structure from components
dotnet run --project src/StructuralExtractor.Cli -- ./components --out components.yaml

# Generate JSON output
dotnet run --project src/StructuralExtractor.Cli -- ./src --format json --out structure.json
```

## Output Format

The tool generates an OpenAPI-inspired structure contract in YAML (or JSON):

```yaml
components:
  Card:
    type: container
    children:
      - type: div
        props:
          className: card-body
  
  Button:
    type: control
    props:
      label: string

pages:
  HomePage:
    route: /
    layout:
      type: div
      props:
        className: min-h-screen
      children:
        - type: Navigation
          ref: '#/components/Navigation'
        - type: main
          children:
            - type: section
              children:
                - type: h1
                  text: Welcome
    sourceFile: /path/to/app/page.tsx

  AboutPage:
    route: /about
    layout:
      type: div
      children:
        - type: Navigation
          ref: '#/components/Navigation'
    sourceFile: /path/to/app/about/page.tsx

navigation:
  initial: HomePage
```

## How It Works

### Page Detection

The tool identifies pages using common React/Next.js conventions:

- Next.js App Router: `page.tsx`, `page.jsx` files
- Pages directory: Files in `/pages/` directory
- Route inference from file paths

### Route Extraction

Routes are inferred from file system structure:

- `app/page.tsx` → `/`
- `app/about/page.tsx` → `/about`
- `app/users/[id]/page.tsx` → `/users/:id`
- `pages/index.tsx` → `/`
- `pages/blog/[slug].tsx` → `/blog/:slug`

### Component Classification

Components are classified by their visual role:

- **container** - Layout/structural elements (div, section, etc.)
- **control** - Interactive elements (button, input, etc.)
- **component** - Custom components

### Structural Extraction

The tool extracts:

1. **Component hierarchy** - Parent/child relationships in JSX
2. **Component references** - References to other components
3. **Props/attributes** - Properties passed to elements
4. **Text content** - Plain text nodes (excluding JSX expressions)

## Architecture

### Components

1. **StructuralExtractor.Core** - Core library containing:
   - `ApplicationStructure` - Data models for the output contract
   - `FileAnalyzer` - Identifies pages and components from files
   - `JsxParser` - Parses JSX/TSX to extract structure
   - `StructuralExtractorService` - Orchestrates the extraction
   - `OutputService` - Serializes to YAML/JSON

2. **StructuralExtractor.Cli** - Console application providing CLI interface

### Technology Stack

- **.NET 10.0** - Core platform
- **YamlDotNet** - YAML serialization
- **System.Text.Json** - JSON serialization
- **Regex-based parsing** - Simplified JSX/TSX parsing for MVP

## Design Principles

Following the specification, this tool:

- **Framework-agnostic** - No React, Next.js, or framework-specific assumptions
- **Visual-only scope** - Only captures visual structure, not behavior or state
- **Prototype-first** - Assumes input is a prototype, not a production codebase
- **Deterministic** - Same input produces same output
- **Human-readable** - Output is self-describing and clear

## Limitations (v1)

- Simplified JSX parsing using regex (not a full AST parser)
- Does not capture:
  - Styling information (deferred to Tool 3)
  - Behavioral logic or state
  - Complex JSX expressions
  - Dynamic component composition
- Conservative about complex structures
- Navigation link extraction is basic

## Integration with Pipeline

### Input

Consumes output from Tool 1 (React Component Flattener):
- Normalized JSX/TSX files
- Flattened component structure

### Output

Produces a YAML/JSON contract consumed by:
- Tool 3 (CSS Extraction) - For style computation
- Human developers - For understanding application structure
- LLMs - For code generation or analysis

## License

See LICENSE file for details.
