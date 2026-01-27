# ReactComponentFlattener

A tool to normalise overly-componentised React code into more maintainable chunks.

## Overview

ReactComponentFlattener (proto-normalise) transforms AI-generated or over-componentised React code into flattened, human-readable, structurally honest React trees. It removes noise while preserving visual intent and behavioral logic.

This tool:
- ✅ Flattens single-use, presentational components
- ✅ Preserves components with hooks, state, or meaningful boundaries
- ✅ Generates detailed metadata reports
- ✅ Supports both JSX and TSX files
- ✅ Works with single files or entire directories
- ✅ Optionally copies CSS files for downstream processing (e.g., CSS Extraction tool)
- ✅ Optionally copies entire app structure to new location

## Installation & Setup

### Prerequisites

- .NET 10.0 SDK or later

### Build

```bash
# Build the .NET solution
dotnet build

# Or build in Release mode
dotnet build -c Release
```

## Usage

### Basic Usage

```bash
# Process a single file
dotnet run --project src/ReactComponentFlattener.Cli/ReactComponentFlattener.Cli.csproj -- input.tsx

# Process a directory
dotnet run --project src/ReactComponentFlattener.Cli/ReactComponentFlattener.Cli.csproj -- ./src

# Specify output and report paths
dotnet run --project src/ReactComponentFlattener.Cli/ReactComponentFlattener.Cli.csproj -- ./src \
  --out ./normalised \
  --report ./report.json
```

### Command-Line Options

```
proto-normalise <input> [options]

Arguments:
  <input>                       Input file or directory path

Output Mode Options (mutually exclusive):
  --output <path>               Output to new directory (default: ./normalised)
  --out <path>                  Alias for --output
  --edit-in-place               Modify files in place

Flattening Options:
  --unused-components <action>  Handle unused components after flattening
                                  retain  - Keep in output
                                  remove  - Don't include (default for --output)
                                  archive - Move to _archive folder (default for --edit-in-place)
  --report <path>               Report output path (default: ./normalisation.json)
  --dry-run                     Analyze without writing files

Output Mode Only Options:
  --copy-css                    Copy CSS files to output directory
  --copy-all                    Copy entire app structure (includes all non-React files)

Advanced Options:
  --preserve-comments           Preserve comments (default: true)
  --max-depth <n>               Maximum nesting depth
  --help, -h                    Show help
```

### Examples

```bash
# Dry run to see what would be flattened
dotnet run --project src/ReactComponentFlattener.Cli/ReactComponentFlattener.Cli.csproj -- ./src --dry-run

# Process with custom output location
dotnet run --project src/ReactComponentFlattener.Cli/ReactComponentFlattener.Cli.csproj -- ./components \
  --output ./flattened \
  --report ./analysis.json

# Edit files in place with archiving
dotnet run --project src/ReactComponentFlattener.Cli/ReactComponentFlattener.Cli.csproj -- ./src \
  --edit-in-place \
  --unused-components archive

# Edit files in place with removal
dotnet run --project src/ReactComponentFlattener.Cli/ReactComponentFlattener.Cli.csproj -- ./src \
  --edit-in-place \
  --unused-components remove

# Process and copy CSS files (needed for downstream tools like CSS Extraction)
dotnet run --project src/ReactComponentFlattener.Cli/ReactComponentFlattener.Cli.csproj -- ./app \
  --output ./normalised \
  --copy-css

# Process and copy entire app structure
dotnet run --project src/ReactComponentFlattener.Cli/ReactComponentFlattener.Cli.csproj -- ./app \
  --output ./normalised \
  --copy-all
```

## Output Modes

The tool supports two primary output modes:

### 1. Output to New Directory (default)

When using `--output` or `--out` (without `--edit-in-place`), the tool processes files and writes them to a new directory:

- **Components Only** (default): Only React/TypeScript files are processed and copied
- **Components + CSS** (`--copy-css`): Copies all `.css` files in addition to processed components. This is required for downstream tools like the CSS Extraction tool (Tool 3) which need matching CSS to resolve styles.
- **Complete App** (`--copy-all`): Copies the entire app structure including all non-processed files (package.json, config files, images, etc.). This is useful when you want to create a complete standalone copy of your app.

### 2. Edit In Place

When using `--edit-in-place`, the tool modifies files directly in their original location. This mode is useful when you want to normalize code in-place without creating a copy.

**Unused Component Handling:**
- `--unused-components retain` - Keep all files, even if all components were flattened
- `--unused-components remove` - Delete files that contain only flattened components
- `--unused-components archive` (default for edit-in-place) - Move files with only flattened components to an `_archive` folder in the same directory

**Note:** 
- CSS copying options (`--copy-css`, `--copy-all`) only apply to output mode, not edit-in-place
- When editing in place, CSS files remain in their original location
- The `--edit-in-place` and `--output` options are mutually exclusive

## How It Works

### Component Flattening Rules

A component is **flattened** (inlined) if **all** of these are true:
- Used in exactly one semantic parent
- Contains no React hooks (useState, useEffect, etc.)
- Contains no context providers or consumers
- Does not accept children (or only passes them through)
- Is not exported

A component is **preserved** if **any** of these are true:
- Used in multiple locations
- Contains hooks or state logic
- Uses context
- Is exported (likely reusable elsewhere)
- Accepts children as props

### Example

**Before:**
```tsx
function ButtonWrapper({ label }) {
  return (
    <div className="p-2">
      <button className="btn">{label}</button>
    </div>
  );
}

export default function Page() {
  return (
    <Layout>
      <ButtonWrapper label="Save" />
    </Layout>
  );
}
```

**After:**
```tsx
export default function Page() {
  return (
    <Layout>
      <div className="p-2">
        <button className="btn">Save</button>
      </div>
    </Layout>
  );
}
```

### Report Output

The tool generates a JSON report with details about each transformation:

```json
{
  "timestamp": "2024-01-22T08:00:00Z",
  "filesProcessed": 61,
  "reports": [
    {
      "Flattened": [
        {
          "Component": "ButtonWrapper",
          "Reason": "single-use, presentational, no hooks",
          "OriginalFile": "Page.tsx",
          "NewLocation": "Page",
          "LineRange": { "Start": 1, "End": 7 }
        }
      ],
      "Preserved": [
        {
          "Component": "Page",
          "Reason": "exported component, likely reusable",
          "File": "Page.tsx"
        }
      ]
    }
  ]
}
```

## Architecture

### Components

1. **ReactComponentFlattener.Core** - Core library containing:
   - `ComponentGraphBuilder` - Analyzes component relationships
   - `ComponentFlattener` - Orchestrates the flattening process
   - `AcornimaParserService` - JSX/TSX parser using Acornima

2. **ReactComponentFlattener.Cli** - Console application providing CLI interface

### Technology Stack

- **.NET 10.0** - Core orchestration and CLI
- **Acornima 1.2.0** - JSX/TSX parsing (pure .NET solution)
- **Acornima.Extras 1.2.0** - JSX support for Acornima
- **System.Text.Json** - JSON serialization

## Testing

Run the tool on the provided test samples:

```bash
# Test on socrates-website samples
dotnet run --project src/ReactComponentFlattener.Cli/ReactComponentFlattener.Cli.csproj -- \
  "Test Samples/socrates-website/components" \
  --out ./test-output \
  --report ./test-report.json

# Test on ai-learning-platform samples
dotnet run --project src/ReactComponentFlattener.Cli/ReactComponentFlattener.Cli.csproj -- \
  "Test Samples/ai-learning-platform/components" \
  --out ./test-output \
  --report ./test-report.json
```

## Limitations (v1)

- Functional components only (no class components)
- Conservative about flattening components with children
- No modification of hooks or state logic
- Does not change props or rename elements
- Does not optimize performance or clean up code style

## License

See LICENSE file for details.

