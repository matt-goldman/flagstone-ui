# ADR-001: Remove Node.js Dependency from Tool 1 (React Flattener)

**Date:** 2026-01-25  
**Status:** Accepted  
**Deciders:** Development Team  
**Context:** Packaging tools as .NET CLI tools for simplified testing and scripted scenarios

## Context and Problem Statement

Tool 1 (React Flattener - normalisation tool) currently depends on Node.js for parsing JSX/TSX files using Babel libraries. This dependency creates several challenges:

1. **CLI Tool Packaging**: External Node.js dependency complicates distribution as a .NET CLI tool via NuGet
2. **Deployment Complexity**: Requires Node.js installation on target systems
3. **Process Overhead**: Spawning Node.js processes and inter-process communication adds overhead
4. **Maintenance**: Dual technology stack (C# + Node.js) increases complexity

The goal is to find a pure .NET solution for JSX/TSX parsing that removes the Node.js dependency while maintaining the functionality needed for component analysis and flattening.

## Decision Drivers

* Must support JSX/TSX syntax parsing
* Must provide AST traversal capabilities for component analysis
* Lightweight for NuGet packaging and distribution
* Actively maintained with good .NET compatibility
* Performance should be acceptable for CLI tool use cases
* Should support code generation from AST (for future flattening implementation)

## Considered Options

### Option 1: Acornima + Acornima.Extras (Selected)

**Description:** .NET port of the Acorn JavaScript parser with JSX support via Extras package

**Pros:**
* ✅ Full JSX/TSX support via Acornima.Extras package
* ✅ Complete ECMAScript 2023 support
* ✅ Passes complete Test262 test suite
* ✅ Actively maintained (combines best of Acorn.js and Esprima.NET)
* ✅ Lightweight and performant
* ✅ Better performance than Esprima.NET
* ✅ More economical stack usage (~2x deeper structures)
* ✅ Strong AST visitor pattern support
* ✅ Used by production projects (Jint JavaScript interpreter)
* ✅ MIT License
* ✅ Well-documented API

**Cons:**
* Requires two packages (Acornima + Acornima.Extras)
* Code generation API is lower-level (JavaScriptTextWriter)

**Package Details:**
* Package: `Acornima` (1.2.0) + `Acornima.Extras` (1.2.0)
* Repository: https://github.com/adams85/acornima
* Size: Relatively small, appropriate for CLI distribution

### Option 2: Esprima (.NET port)

**Description:** .NET port of the Esprima JavaScript parser

**Pros:**
* Mature and stable library
* Single package
* Good ECMAScript support
* BSD License

**Cons:**
* ❌ **No native JSX support** (deal-breaker)
* Would require custom JSX parser extension
* Less actively developed than Acornima
* Heavier than Acornima

**Decision:** Rejected due to lack of JSX support

### Option 3: Jint

**Description:** Full JavaScript interpreter for .NET that includes a parser

**Pros:**
* Full JavaScript execution engine
* Uses Acornima internally for parsing
* Well-maintained

**Cons:**
* ❌ Too heavy - entire JS execution engine when only parser is needed
* Overkill for AST parsing requirements
* Larger package size inappropriate for CLI tool

**Decision:** Rejected as too heavy for requirements

### Option 4: Keep Node.js (Current Implementation)

**Description:** Continue using Babel parser via spawned Node.js processes

**Pros:**
* Already implemented and working
* Full JSX/TSX support
* Well-tested Babel parser

**Cons:**
* ❌ External Node.js dependency
* ❌ Process spawning overhead
* ❌ Complicates CLI tool packaging
* ❌ Requires Node.js on target systems
* ❌ Dual technology stack complexity

**Decision:** Rejected to meet CLI tool packaging requirements

## Decision Outcome

**Chosen option: Acornima + Acornima.Extras**

### Rationale

Acornima with Acornima.Extras is the best fit for our requirements because:

1. **Meets all technical requirements**: Full JSX/TSX parsing, AST traversal, lightweight, actively maintained
2. **Removes external dependency**: Pure .NET solution eliminating Node.js requirement
3. **Production-ready**: Used by Jint and other major .NET projects, passes Test262 suite
4. **Performance**: Better than alternatives, economical stack usage
5. **Appropriate size**: Lightweight enough for NuGet CLI tool distribution
6. **Modern and maintained**: Based on latest Acorn.js with ongoing development

### Trade-offs Accepted

* **Two packages instead of one**: Acceptable given both packages are small
* **API Learning curve**: Different from Babel API, but well-documented
* **Lower-level code generation**: JavaScriptTextWriter is more verbose than Babel generator, but provides necessary functionality

## Implementation Details

### What Was Implemented

1. **AcornimaParserService** (431 lines)
   * Replaces Node.js-based ParserService
   * Implements `AnalyzeFileAsync` for component/import extraction
   * Implements `FlattenComponentsAsync` (returns unchanged code with warning - future enhancement)

2. **AST Analysis** using visitor pattern:
   * Component detection (functions, arrow functions, declarations)
   * Hook usage detection (useState, useEffect, etc.)
   * Children prop detection  
   * Export tracking (named and default exports)
   * Component usage tracking
   * Import extraction with specifiers
   * Source location tracking

3. **Integration Updates**:
   * Updated `ComponentFlattener` to use `AcornimaParserService`
   * Updated CLI Program.cs to instantiate new service
   * Added Acornima packages to ReactComponentFlattener.Core.csproj

### Component Flattening Implementation

**Component Flattening via AST Transformation**: Initially deferred in the Node.js removal work, the `FlattenComponentsAsync` method has now been fully implemented with:
* AST transformation to inline components at usage sites
* Prop substitution logic for both string literals and expressions
* Removal of flattened component definitions
* Code generation from transformed AST using Acornima's ToJsx() method
* Support for both function declarations and arrow function components

## Testing

### Test Results

✅ **Build**: 0 errors, 2 warnings (both NuGet package pruning suggestions)  
✅ **Simple Component Test**: Correctly identified Button component as exported/preserved  
✅ **Complex Component Test**: Correctly analyzed:
* Label → marked for flattening (single-use, presentational)
* Counter → preserved (contains hooks)
* Card → preserved (exported, has children)

### Verified Functionality

* Component detection (various declaration styles)
* Hook detection (useState, useEffect, custom hooks)
* Children prop detection
* Export tracking (default and named)
* Component usage tracking
* Import extraction
* Flattening rule evaluation

## Consequences

### Positive

* ✅ Node.js dependency removed - simplifies CLI tool packaging
* ✅ Pure .NET solution - easier deployment and maintenance
* ✅ No process spawning overhead
* ✅ Single technology stack
* ✅ Better alignment with .NET ecosystem

### Negative

* ⚠️ Component flattening not yet implemented (deferred feature)
* ⚠️ Different API from Babel (one-time learning curve)

### Neutral

* Two packages required (Acornima + Acornima.Extras)
* Different code generation approach if/when flattening is implemented

## References

* [Acornima GitHub Repository](https://github.com/adams85/acornima)
* [Acornima NuGet Package](https://www.nuget.org/packages/Acornima)
* [Acornima.Extras NuGet Package](https://www.nuget.org/packages/Acornima.Extras)
* [Test262 ECMAScript Test Suite](https://github.com/tc39/test262)

## Notes

* Acornima version 1.2.0 was the latest stable version at the time of this decision
* The library is actively maintained with regular updates
* Community adoption is growing, particularly with Jint using it as the parser
* Code generation functionality exists but requires lower-level API usage
