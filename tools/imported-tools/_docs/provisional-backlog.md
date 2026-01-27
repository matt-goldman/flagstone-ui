### Tool 1 — Prototype Normalisation Tool (React)

**Status:** In progress / just implemented  
**Pipeline stage:** Stage 1 — Prototype Normalisation

**Purpose**

*   Flatten AI-generated React prototypes
*   Remove structural noise
*   Produce a structurally honest visual hierarchy

**Key constraints (already locked)**

*   Prototype-only assumption
*   Closed-world scope
*   Visual-only hierarchy
*   Deterministic, rule-based
*   Flatten ≠ delete
*   Explicit edit-in-place vs output modes

This tool is already well specified and effectively “done” pending tweaks.

* * *

### Tool 2 — Structural Classification Tool

**Status:** Backlog (explicitly discussed, not built)  
**Pipeline stage:** Stage 2 — Structural Classification

**Purpose**

*   Classify normalised structure into:
    *   Pages / routes
    *   Views
    *   Layout containers
    *   Visual controls

**Important**

*   This tool does **not** apply platform knowledge
*   It answers _what this is_, not _how it maps_

**Decisions already made**

*   Operates on output of Tool 1
*   May use heuristics
*   Must be auditable
*   Does not require an LLM, but could optionally use one later

* * *

### Tool 3 — CSS / Style Computation & Extraction Tool

**Status:** Explicit backlog item  
**Pipeline stage:** Stage 3 — Style & Visual Model Extraction

**Purpose**

*   Compute resolved visual styles
*   Collapse selector chaos into a canonical style model

**Decisions already made**

*   CSS is computed, not translated
*   Structural normalisation happens first
*   Multiple selectors may intentionally collapse
*   Deterministic tooling preferred
*   Output is platform-agnostic

This is one of the most important tools and was heavily reasoned about.

* * *

### Tool 4 — Design Token & Theme Projection Tool

**Status:** Partially exists (Bootstrap, Tailwind palette), needs generalisation  
**Pipeline stage:** Stage 4 — Token & Theme Projection

**Purpose**

*   Convert canonical style model into:
    *   Design tokens
    *   Theme definitions
    *   Flagstone UI themes

**Decisions already made**

*   Input is Tool 3 output
*   Output is design-system artefacts, not UI code
*   Evolves existing tooling, not greenfield

* * *

### Tool 5 — UI Code Generation Tool (Target Platform)

**Status:** Explicit backlog item  
**Pipeline stage:** Stage 5 — UI Code Generation

**Purpose**

*   Generate complete UI artefacts (pages, views)

**Decisions already made**

*   Output is structurally complete, not final
*   Visual match is approximate
*   Heavy lifting is eliminated
*   Human refinement is expected
*   Same tool supports:
    *   Full scaffold
    *   Incremental additions

* * *

### Tool 6 — Integration & Reconciliation Tool

**Status:** Explicit backlog item  
**Pipeline stage:** Stage 6 — Integration & Reconciliation

**Purpose**

*   Safely merge generated UI into existing apps

**Decisions already made**

*   Defensive by default
*   Conservative changes
*   Diff/reporting required
*   Same pipeline, smaller scope

* * *

### Tool 7 — Validation & Reporting Tool

**Status:** Implicit but agreed  
**Pipeline stage:** Stage 7 — Validation & Reporting

**Purpose**

*   Produce inspectable reports:
    *   What changed
    *   What was assumed
    *   What needs manual refinement

**Decisions already made**

*   Deterministic
*   Essential for trust
*   Not optional “nice to have”