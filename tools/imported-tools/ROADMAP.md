# Pipeline Tools Roadmap

This document tracks the current state, known issues, and planned work for the Prototype → Tokens conversion pipeline.

## Current State

**Pipeline Runner**: ✅ Operational  
**Last Validated**: 2026-01-27

The pipeline infrastructure is in place and runs end-to-end. Each stage executes, produces output, and the orchestrator manages temp directories and artifact collection.

### Stage Status

| Stage | Tool | Status | Notes |
|-------|------|--------|-------|
| 1 | ReactComponentFlattener | ✅ Functional | TypeScript stripping improved |
| 2 | StructuralExtractor | ⚠️ Partial | Expects JSX input, receives HTML |
| 3 | CssComputer | ⚠️ Partial | Needs compiled CSS for Tailwind projects |
| 4 | TokenGenerator | 🔲 Placeholder | DLS → Tokens path not implemented |

---

## Resolved Issues

### Issue 1: TypeScript Parsing Limitations in Tool 1 ✅ RESOLVED

**Resolution Date**: 2026-01-27

**Original Problem**: Parser errors on files with advanced TypeScript syntax (type annotations, generics, type assertions, import aliases).

**Solution Applied**: Enhanced `TypeScriptHelper.StripTypeScriptTypes()` with:
1. Fixed object literal corruption by restricting parameter type regex to match only actual types
2. New `RemoveTypeAssertions()` method with smart context detection to preserve import aliases
3. Added rest parameter type stripping (`...args: Type[]`)
4. Support for indexed access types (`as Type['key']`)
5. Fixed multi-line type annotations and intersection types

**Result**: All 68 test files now parse successfully (was ~30 failures before fix).

---

## Known Issues

### Issue 2: Structure Extraction Receives HTML Instead of JSX

**Symptom**: Stage 2 finds 0 components and 0 pages  
**Root Cause**: Stage 1 outputs HTML (via `EmitHtml: true`), but Stage 2's `StructuralExtractorService` only parses JSX/TSX.

**Impact**: Application structure contract is empty, losing valuable metadata about pages, components, and navigation.

**Solution**: Enhance Stage 2 to support HTML input using data attributes from Stage 1.

Stage 1 already emits:
- `data-component="ComponentName"` on elements that were React components
- `data-source="filename"` on root elements
- `<meta name="component">` in document head

Stage 2 should:
1. Detect input type (HTML vs JSX/TSX)
2. For HTML: Parse and extract structure from `data-component` attributes
3. For JSX: Use current behavior (backward compatibility)

This preserves the benefit of normalization (cleaner, flattened structure) while retaining component boundary information.

---

### Issue 3: CSS Computation Requires Compiled CSS

**Symptom**: Minimal style extraction from Tailwind projects  
**Root Cause**: Tool 3 computes styles from actual CSS. Tailwind utility classes don't exist in source CSS - they're generated during build.

**Impact**: For Tailwind/utility-class projects, DLS output is limited to non-utility styles only.

**Workaround**: Run `npm run build` on the prototype first to generate compiled CSS. The tool checks standard build output locations:
- `.next/static/css/` (Next.js)
- `dist/assets/` (Vite)
- `build/static/css/` (CRA)

**Long-term**: Document this requirement clearly; optionally add a pre-build step to the pipeline.

---

### Issue 4: Token Generation Not Implemented

**Symptom**: Stage 4 creates a placeholder file instead of actual tokens  
**Root Cause**: The `FlagstoneUI.TokenGenerator` currently supports:
- `generate`: XAML → JSON (extract from existing themes)
- `validate`: Validate XAML or JSON tokens
- `xaml`: JSON → XAML (generate XAML from catalog)

It does **not** support: DLS → Tokens (creating tokens from extracted styles)

**Impact**: Pipeline cannot produce usable Flagstone UI theme artifacts.

**Required Work**: Implement DLS analysis and token projection in TokenGenerator.

---

## Backlog

### High Priority

#### P1-001: Implement DLS → Token Projection

**Goal**: Convert DLS output to Flagstone UI token definitions

**Tasks**:
- [ ] Analyze DLS styles to identify color patterns
- [ ] Extract spacing values and map to Space.* tokens
- [ ] Identify typography scales (font-size, line-height, font-weight)
- [ ] Detect border-radius patterns for Radius.* tokens
- [ ] Map semantic roles to appropriate token categories
- [ ] Generate `tokens-catalog.json` in FlagstoneUI schema
- [ ] Optionally generate `Tokens.xaml` directly

**Acceptance Criteria**:
- Given a DLS with color/spacing/typography styles
- When token projection runs
- Then valid tokens-catalog.json is produced
- And tokens map to FlagstoneUI token schema

---

#### P1-002: Add HTML Support to Structure Extractor

**Goal**: Enable Stage 2 to extract structure from normalized HTML output

**Approach**: Enhance `StructuralExtractorService` to detect and parse HTML files, using `data-component` attributes emitted by Stage 1.

**Tasks**:
- [ ] Add HTML file detection (check extension and/or content)
- [ ] Implement HTML parser using AngleSharp (already a dependency)
- [ ] Extract components from `data-component` attributes
- [ ] Extract page info from `<meta name="component">` and file paths
- [ ] Infer navigation from `<a href>` elements
- [ ] Preserve current JSX parsing as fallback
- [ ] Update pipeline to pass normalized output to Stage 2

**Acceptance Criteria**:
- Given normalized HTML with `data-component` attributes
- When structure extraction runs
- Then components are identified from data attributes
- And pages are identified from file structure
- And backward compatibility with JSX input is maintained

---

### Medium Priority

#### P2-001: Improve TypeScript Support in Tool 1

**Goal**: Reduce parser failures on TypeScript files

**Tasks**:
- [ ] Audit common failure patterns from test runs
- [ ] Evaluate TypeScript-capable parsers (ts-morph via edge, etc.)
- [ ] Consider pre-processing step to strip type annotations
- [ ] Implement solution that maintains current parsing speed

**Note**: Full TypeScript support may require significant work. Prioritize based on frequency of failures in real prototypes.

---

#### P2-002: Add Pre-Build Step for Tailwind Projects

**Goal**: Automatically compile CSS before running Tool 3

**Tasks**:
- [ ] Detect project type (Next.js, Vite, CRA, standalone Tailwind)
- [ ] Add optional `--build` flag to pipeline runner
- [ ] Execute appropriate build command (npm/pnpm/yarn)
- [ ] Verify compiled CSS is detected by Tool 3

**Risks**: Build may fail due to missing dependencies or node_modules.

---

#### P2-003: Document Pipeline Requirements

**Goal**: Clear documentation for pipeline users

**Tasks**:
- [ ] Document input requirements (what constitutes a valid prototype)
- [ ] Document Tailwind/utility-class project preparation
- [ ] Add examples for each test sample
- [ ] Create troubleshooting guide for common errors

---

### Low Priority

#### P3-001: Add Pipeline Stage Hooks

**Goal**: Allow custom processing between stages

**Tasks**:
- [ ] Define hook interface (before/after each stage)
- [ ] Add configuration for custom hooks
- [ ] Document hook usage

---

#### P3-002: Parallel Stage Execution

**Goal**: Run independent stages in parallel where possible

**Tasks**:
- [ ] Identify stage dependencies (1→3, 1→2, etc.)
- [ ] Implement parallel execution for independent stages
- [ ] Add progress reporting for parallel runs

---

#### P3-003: GitHub Actions Workflow

**Goal**: Run pipeline in CI/CD

**Tasks**:
- [ ] Create workflow for pipeline execution
- [ ] Add artifact upload for outputs
- [ ] Consider workflow dispatch with input parameters

---

## Test Samples

| Sample | Location | Notes |
|--------|----------|-------|
| ai-learning-platform | `tools/1-normalization-tool/Test Samples/` | Tailwind, Next.js, shadcn/ui |
| mobile-app-prototype | `tools/1-normalization-tool/Test Samples/` | TBD |
| socrates-website | `tools/1-normalization-tool/Test Samples/` | TBD |

---

## Success Criteria for MVP Pipeline

The pipeline is MVP-ready when:

1. ✅ All 4 stages execute without critical failures
2. 🔲 Structure extraction produces meaningful output (pages, components)
3. 🔲 CSS computation extracts styles from compiled CSS
4. 🔲 Token generation produces valid FlagstoneUI tokens
5. 🔲 Tokens can be loaded into a FlagstoneUI theme
6. 🔲 At least one test sample runs end-to-end successfully

---

## Future Considerations

- **MCP Server Integration**: Tools 1-3 could be exposed via MCP for agent orchestration
- **Theme Generator**: Extend TokenGenerator to produce complete Theme.xaml
- **Visual Diff Tool**: Compare generated theme against prototype visuals
- **Incremental Updates**: Support updating existing themes rather than full regeneration

---

## Changelog

### 2026-01-27

- ✅ Created `Pipeline.slnx` solution file
- ✅ Implemented `FlagstoneUI.PipelineRunner` orchestrator
- ✅ Added temp directory management (`~/.fs-pipeline/`)
- ✅ Integrated Tools 1, 2, 3 into pipeline
- ✅ Created placeholder for Stage 4 (TokenGenerator)
- ✅ Validated end-to-end execution with test sample
- ✅ Documented known issues and backlog
