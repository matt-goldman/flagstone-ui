//DRAFT//

**Note:** These tools were originally developed in another repo because they are not technically part of FlagstoneUI. However, the complete pipeline needs to be tested, including integration with the TokenGenerator. The code has been brought in initially for evaluation. It may or may not remain.

## Quick Start - Pipeline Runner

The `FlagstoneUI.PipelineRunner` orchestrates all tools in sequence:

```bash
# Build the pipeline solution
dotnet build Pipeline.slnx

# Run the pipeline on a prototype
dotnet run --project tools/FlagstoneUI.PipelineRunner -- \
  --input "path/to/prototype" \
  --output "./pipeline-output" \
  --verbose

# List existing temp directories
dotnet run --project tools/FlagstoneUI.PipelineRunner -- list

# Clean up all temp directories
dotnet run --project tools/FlagstoneUI.PipelineRunner -- clean
```

### Pipeline Stages

1. **Normalize** - Flatten over-componentized React code (Tool 1)
2. **Extract Structure** - Identify pages, components, layouts (Tool 2)
3. **CSS Compute** - Extract Design Language Specification (Tool 3)
4. **Token Generation** - Convert DLS to Flagstone UI tokens (placeholder)

### Output Artifacts

The pipeline produces:
- `normalized/` - Normalized prototype source (HTML output)
- `structure.json` - Application structure contract
- `dls.json` - Design Language Specification
- `tokens/` - Token generation artifacts
- `reports/` - Stage reports with metadata
- `pipeline-manifest.json` - Run summary and artifact listing

---

# Prototype → Application Conversion Pipeline

This document describes the high-level pipeline for converting AI-generated application prototypes (e.g. V0, Spark) into structured application code suitable for downstream targets such as .NET MAUI, Blazor, or other UI frameworks.

This pipeline is **implementation-agnostic**. Individual stages may be executed by humans, deterministic tools, LLMs, agents, or a combination thereof.

The intent is to define *what happens*, not *how it is orchestrated*.

## Scope

The pipeline is for the visual hierarchy of the app, not behaviour or functionality.

---

## Pipeline Principles

- The input is assumed to be an **application prototype**, not a curated component library.
- Structural noise is expected and should be removed early.
- Behavioural and semantic reasoning is deferred until structure is stable.
- Each stage should be independently executable and testable.
- Later stages must not compensate for ambiguity that earlier stages could have resolved.

---

## Stage 0 — Scope Definition

**Goal:** Establish the conversion boundary.

### Description
Define the scope of input that constitutes the prototype to be converted. This may be:
- A single file
- A folder
- A full prototype source tree

All subsequent decisions operate under a **closed world assumption** within this scope.

### Output
- Explicit input scope
- Conversion intent (e.g. scaffold vs incremental update)

---

## Stage 1 — Prototype Normalisation

**Goal:** Remove structural noise and over-componentisation.

### Description
Transform the prototype source into a flattened, structurally honest representation by:
- Collapsing purely structural visual subcomponents
- Preserving only behaviourally or semantically independent components
- Ignoring reuse intent outside the provided scope

This stage operates **only on visual hierarchy**.  
Behavioural, contextual, and infrastructural components are out of scope and passed through unchanged.

### Output
- Normalised prototype source
- Transformation report (what was flattened and why)

---

## Stage 2 — Structural Classification

**Goal:** Identify conceptual roles within the normalised structure.

### Description
Analyse the normalised prototype to classify elements into high-level structural roles, such as:
- Pages / routes
- Reusable views
- Layout containers
- Visual controls

This stage does not apply platform knowledge.  
It answers *what this is*, not *how it should be implemented*.

### Output
- Annotated structural tree
- Classification metadata

---

## Stage 3 — Style & Visual Model Extraction

**Goal:** Derive a coherent visual model from prototype styling.

### Description
Extract and consolidate styling information by:
- Resolving computed styles
- Grouping visual variants
- Identifying implicit design tokens (spacing, colour, typography)

At this stage, styling chaos is intentionally collapsed into a minimal, explicit model.

### Output
- Canonical style model
- Variant groupings
- Visual token candidates

---

## Stage 4 — Design Token & Theme Projection

**Goal:** Map the visual model into a target-agnostic design system representation.

### Description
Project the extracted style model into:
- Design tokens
- Theme definitions
- Platform-neutral abstractions

This stage bridges prototype styling and application theming, without generating UI code.

### Output
- Token definitions
- Theme artefacts
- Mapping metadata

---

## Stage 5 — UI Code Generation

**Goal:** Generate structured UI code for the target platform.

### Description
Generate UI artefacts such as:
- Pages
- Views
- Layout structures
- Control usage

Generation focuses on:
- Structural correctness
- Completeness
- Readability

It does **not** attempt to perfectly match prototype visuals.

### Output
- Generated UI code
- File structure aligned to target platform conventions

---

## Stage 6 — Integration & Reconciliation

**Goal:** Merge generated output into an existing application where applicable.

### Description
Depending on intent:
- Add new pages/views
- Extend existing structures
- Avoid destructive overwrites

All changes should be conservative, explicit, and auditable.

### Output
- Integrated application code
- Diff or reconciliation report

---

## Stage 7 — Validation & Reporting

**Goal:** Make the outcome inspectable and trustworthy.

### Description
Produce a report that:
- Summarises pipeline actions
- Highlights areas requiring manual refinement
- Identifies gaps or assumptions

This stage is essential for user confidence.

### Output
- Conversion report
- Known limitations and next steps

---

## Notes

- The pipeline is intentionally **front-loaded** with structural normalisation.
- Semantic and architectural judgement is deferred until noise is removed.
- The same pipeline supports:
  - Full prototype scaffolding
  - Incremental updates to an existing application

Orchestration decisions (human vs agent vs MCP) are explicitly out of scope for this document.


## Automation Principles

- Prefer deterministic tooling where rules are explicit and stable
- Use LLMs for heuristic, judgement-heavy, or pattern-recognition tasks
- Keep humans in the loop where intent or risk is high
- Favour conservative automation over speculative correctness

---

## Stage-by-Stage Assessment

| Stage | Primary Mode | Notes |
|-----|-------------|------|
| 0 — Scope Definition | Human / Agent | Requires intent; tooling can assist but not decide |
| 1 — Prototype Normalisation | Deterministic Tool | Strong automation candidate; conservative rules |
| 2 — Structural Classification | Tool + Heuristics | Automatable with auditability |
| 3 — Style Extraction | Deterministic Tool | Ideal for automation |
| 4 — Token Projection | Deterministic Tool | Pure transformation |
| 5 — UI Generation | Deterministic Tool | Codegen with constraints |
| 6 — Integration | Tool (Defensive) | Must avoid destructive changes |
| 7 — Validation | Deterministic Tool | Reporting and diffing |