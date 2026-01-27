## 1. Purpose & Intent
--------------------

The purpose of Tool 2 is to extract a **canonical, human-readable application structure contract** from a normalised prototype.

This contract captures:

*   Pages
*   Routes
*   Navigation
*   Visual components
*   Structural composition

It explicitly does **not** capture:

*   Styling
*   Behaviour
*   Framework or platform details

The output is designed to be:

*   Easily understood by humans
*   Easily consumed by LLMs
*   Stable as an interchange artefact between tools

* * *

## 2. Position in the Pipeline
----------------------------

Tool 2 sits between:

*   **Tool 1 — Prototype Normalisation**
*   **Tool 3 — Style Computation & DLS Extraction**

It is the final **fully framework-agnostic** tool in the pipeline.

* * *

## 3. Scope & Assumptions
-----------------------

### Prototype-first assumption

*   Inputs represent application prototypes, not curated component libraries
*   Components are assumed to exist for convenience, not necessity

### Visual-only scope

*   Only **visual structure** is considered
*   No data flow, state management, or behavioural semantics are inferred

### Closed-world assumption

*   All decisions are made strictly within the provided input
*   Usage outside the provided scope is ignored

* * *

## 4. Inputs
----------

### Required inputs

*   Output from Tool 1 (normalised prototype)
*   One or more files that can be rendered into a **DOM-like visual tree**

### File inclusion heuristic

Files are included if they can reasonably be interpreted as producing visual structure, for example:

*   `.html`
*   `.jsx`, `.tsx`
*   `.razor`
*   `.cshtml`
*   `.php`

File extensions are not authoritative; **renderability into a visual tree is the deciding factor**.

* * *

## 5. Outputs
-----------

### Primary Output — Application Structure Contract

The output is a **single declarative document** (YAML recommended, JSON acceptable) describing:

*   Visual components
*   Pages
*   Routes
*   Layout composition
*   Navigation entry points

This output:

*   Is not a DSL requiring a runtime
*   Does not require a formal schema to be interpreted
*   Is intentionally self-describing and human-readable

* * *

## 6. Output Shape (Conceptual)
-----------------------------

The output follows an **OpenAPI-inspired shape**, without OpenAPI semantics.

At a minimum, it must support:

### Components

*   Named visual primitives
*   Classified by role (e.g. container, control, layout)
*   Declared once, referenced many times

### Pages

*   Named screens or views
*   Associated with routes (where applicable)
*   Contain a root visual structure

### Composition

*   Explicit parent/child relationships
*   Component reuse via references
*   No implicit hierarchy

### Navigation

*   Entry points
*   Page-to-page relationships (if present in the prototype)

Example (illustrative only):

```yaml
components:
  Card:
    type: container
    slots:
      body: component

  Button:
    type: control
    props:
      label: string

pages:
  HomePage:
    route: /
    layout:
      type: column
      children:
        - $ref: '#/components/Card'
        - $ref: '#/components/Button'

navigation:
  initial: HomePage
```

* * *

## 7. Core Responsibilities
-------------------------

Tool 2 **must**:

*   Identify distinct visual components
*   Identify pages/screens and their boundaries
*   Extract explicit structural hierarchy
*   Preserve intentional reuse
*   Remove incidental or redundant structure introduced by the prototype

* * *

## 8. Flattening & Retention Rules
--------------------------------

### Components should be flattened when:

*   They are purely structural noise
*   They exist only to mirror prototype abstraction patterns
*   They are never used independently of their parent

### Components should be retained when:

*   They are used independently across multiple pages
*   They represent a meaningful visual primitive
*   Flattening would obscure intent or reuse

Export semantics, file boundaries, and naming conventions are **not** authoritative signals.

* * *

## 9. Explicit Non-Goals
----------------------

Tool 2 does **not**:

*   Compute or infer styles
*   Detect variants or states
*   Infer interaction behaviour
*   Resolve responsiveness or breakpoints
*   Perform platform or framework mapping
*   Produce MAUI, FlagstoneUI, or any implementation code

These concerns are explicitly deferred to later pipeline stages or human refinement.

* * *

## 10. Determinism & Explainability
---------------------------------

Given the same input, Tool 2 must:

*   Produce deterministic output
*   Apply consistent flattening and retention rules
*   Be explainable at a structural level

Heuristics are acceptable, but silent or implicit decisions are not.

* * *

## 11. Success Criteria
---------------------

Tool 2 is successful if:

*   A human can understand the app’s structure by reading the output alone
*   An LLM can implement the app structure without additional context
*   Downstream tools no longer need to reason about prototype-specific noise
*   The artefact cleanly bridges prototype thinking and implementation thinking

Perfect abstraction is **not** required. Clarity is.

* * *

## 12. Relationship to Other Tools
--------------------------------

*   **Consumes:** Tool 1 output (normalised prototype)
*   **Feeds:** Tool 3 (style computation), human-driven implementation, or both
*   **Does not depend on:** FlagstoneUI, MAUI, or any design system

* * *

## 13. Versioning Notes
---------------------

This document represents **v1.0**, the first stable, non-draft specification.

Future revisions should preserve the human-readable, framework-agnostic nature of the output, even if additional metadata is introduced.


## Appendix: What we actually mean by “OpenAPI-like”
---------------------------------------

Let’s be very explicit about the _one thing_ we are borrowing from OpenAPI:

> **The method of constructing a contract by defining primitives first, then composing them into higher-level structures.**

We are **not** borrowing:

*   transport semantics
*   protocol concepts
*   verbs
*   operations
*   behaviour
*   validation rigor
*   tooling expectations

We are borrowing a **structural composition pattern**, not a specification format.