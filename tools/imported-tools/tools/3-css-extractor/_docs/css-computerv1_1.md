# Tool 3 — Style Computation & Canonical Design Language Specification (DLS) Extraction

**Version:** v1.1  
**Status:** First stable, implementation-ready specification


## 1. Purpose & Intent

The purpose of this tool is to compute a **canonical Design Language Specification (DLS)** from an application prototype by resolving all styling inputs into explicit, platform-agnostic **visual design semantics**.

The intent is to extract _what the design is_, not _how it was authored_, so that the design can later be transferred to other design systems, platforms, or UI frameworks via downstream pipeline stages.

This tool deliberately separates **design semantics** from **implementation detail**.

* * *

## 2. Scope & Assumptions

### Prototype-only assumption

*   Inputs represent application prototypes, not curated or shared component libraries.
*   Styling is assumed to have been included for convenience, not necessity.

### Prerequisite

*   Input **must** be the output of the Prototype Normalisation Tool (Tool 1).
*   Structural noise must already be removed.

### Closed-world assumption

*   All decisions are made strictly within the provided scope.
*   External styles, libraries, or usage outside the input are ignored.

### Visual-only scope

*   Only visual styling is considered.
*   Behavioural, architectural, or semantic component concerns are explicitly out of scope.

* * *

## 3. Inputs

### Required inputs

*   Normalised prototype source (e.g. JSX / TSX / equivalent)
*   Styling sources within scope:
    *   CSS files
    *   Utility-class frameworks (e.g. Tailwind)
    *   Inline styles

### Optional inputs

*   Viewport or breakpoint configuration (defaults apply)
*   Configuration flags controlling tolerance thresholds and output options

* * *

## 4. Outputs

### 4.1 Authoritative Output — Canonical DLS

The primary output is a **canonical Design Language Specification (DLS)** that represents resolved visual design semantics.

The DLS:

*   Is the **source of truth**
*   Is platform-agnostic
*   Does not encode CSS-specific concepts (selectors, cascade, specificity)
*   Is suitable for downstream projection into tokens, themes, or platform styles

### 4.2 Optional Output — CSS Projection (Flag-controlled)

Optionally, the tool may emit a **CSS projection** derived from the canonical DLS.

*   This output is **lossy**
*   It exists solely for inspection, debugging, and validation
*   It must never be re-parsed or treated as authoritative

### 4.3 Reports & Metadata

The tool must emit structured metadata describing:

*   Which elements contributed to each style
*   Which styles were grouped or collapsed
*   Which ambiguities or conflicts were detected
*   Any values ignored or normalised

* * *

## 5. Core Design Principles

### CSS is computed, not translated

All styling inputs are resolved into **computed visual results** before any grouping or abstraction occurs.

Selector structure, utility names, and authoring intent are discarded.

### Conservatism over collapse

Style grouping must be conservative:

*   Collapse styles only when confidence is high
*   When ambiguous, preserve distinction (more styles, not fewer)

### Determinism & auditability

*   Given the same input and configuration, output must be deterministic
*   All grouping decisions must be explainable via metadata

* * *

## 6. Processing Stages

### Stage 1 — Style Resolution

For each visual element in the normalised structure:

*   Resolve final computed styles by applying:
    *   CSS cascade
    *   Utility class resolution
    *   Inline style overrides
*   Produce an explicit property set per element

Discarded at this stage:

*   Selectors
*   Utility class names
*   Authoring patterns

* * *

### Stage 2 — Normalisation

Normalise resolved styles by:

*   Removing default or insignificant values
*   Canonicalising equivalent values (units, colour formats, numeric representations)
*   Producing a minimal explicit property set per element

* * *

### Stage 3 — Grouping

Group elements into **conceptual styles** based on:

*   Identical or near-identical resolved properties
*   Explicit tolerances (implementation-defined)

Rules:

*   Grouping is opt-in, not opt-out
*   Ambiguity results in separate styles
*   Silent over-collapse is forbidden

* * *

### Stage 4 — Variant Detection

Detect variants where:

*   A base style exists
*   Differences are systematic and consistent across multiple elements

Variants:

*   Are represented as explicit deltas from a base style
*   Must not duplicate the full base property set

* * *

## 7. Canonical DLS Structure (Embedded Schema)

This schema defines **shape and invariants**, not exhaustive property coverage.

```json
{
  "type": "object",
  "required": ["styles"],
  "properties": {
    "styles": {
      "type": "array",
      "items": {
        "type": "object",
        "required": ["id", "properties"],
        "properties": {
          "id": { "type": "string" },
          "properties": {
            "type": "object",
            "additionalProperties": { "type": ["string", "number"] }
          },
          "variants": {
            "type": "array",
            "items": {
              "type": "object",
              "required": ["name", "properties"],
              "properties": {
                "name": { "type": "string" },
                "properties": {
                  "type": "object",
                  "additionalProperties": { "type": ["string", "number"] }
                }
              }
            }
          },
          "metadata": {
            "type": "object",
            "additionalProperties": true
          }
        }
      }
    }
  }
}
```

This schema is intentionally minimal and may be extracted into a standalone artifact if it grows beyond the scope of this specification.

**Note:** The schema is currently considered final; however minor adjustments are acceptable. Any changes must be documented and explained/justified as they will impact later pipeline tools and stages. Major changes are not acceptable as the schema currently aligns closely with the end state.

* * *

## 8. Error Handling & Limits

*   Conflicting or irreconcilable styles must be preserved separately
*   Ambiguities must be surfaced in metadata
*   No behaviour may silently discard or over-collapse styles

* * *

## 9. Relationship to Other Tools

*   **Consumes:** Normalised prototype output (Tool 1)
*   **Feeds:** Tokenisation and theming projection (Tool 4)
*   **Must not:** Apply platform or framework assumptions

* * *

## 10. Success Criteria

This tool is successful if:

*   Prototype styling chaos collapses into a small, explicit DLS
*   Downstream tools no longer need to reason about CSS complexity
*   Human refinement is possible without reverse-engineering intent

Perfect visual parity is **not** required.

* * *

## 11. Explicit Non-Goals

This tool does **not**:

*   Infer semantic meaning of components
*   Perform layout abstraction
*   Handle behavioural states (hover, focus, disabled)
*   Generate platform-specific styles or tokens
*   Optimise for any specific UI framework

* * *

## 12. Versioning Notes

This document represents **v1.1**, the first fully merged, implementation-ready specification.

Future changes should be intentional and documented, as this spec now represents a stability baseline for Tool 3.