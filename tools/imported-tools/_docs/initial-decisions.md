# Design Decisions

This document records key design decisions made during the development of the Prototype → Application Conversion Pipeline.

Its purpose is to preserve *why* certain choices were made, so future changes are intentional rather than accidental.

This is not a full ADR system. Entries are lightweight, scoped, and pragmatic.

---

## Closed-World Assumption

**Decision**  
All tools in this pipeline operate under a **closed-world assumption**.

Flattening, classification, and extraction decisions are made strictly within the input scope provided to the tool (file, folder, or tree).

**Rationale**
- Prototypes are self-contained by nature
- AI-generated code does not express reliable reuse intent
- Speculating about usage outside scope leads to over-preservation

**Implications**
- Incomplete input may produce aggressive flattening
- This is considered caller responsibility, not tool error
- No tool should attempt to infer external usage

---

## Prototype-Only Assumption

**Decision**  
Inputs are assumed to represent **application prototypes**, not curated or shared component libraries.

**Rationale**
- AI generators over-componentise for convenience during generation
- Exported or repeated components do not imply architectural intent
- Treating prototypes as libraries produces poor downstream results

**Guiding Principle**
> Assume components have been included for convenience, not necessity.

**Implications**
- Default posture is aggressive flattening
- Reuse signals outside the provided scope are ignored
- Library-like structures are treated as structural noise unless proven otherwise

---

## Visual-Only Structural Normalisation

**Decision**  
Structural normalisation applies **only to visual components that participate in the rendered UI hierarchy**.

Behavioural, contextual, or infrastructural components (e.g. providers, context wrappers, portals, side-effect-only components) are **out of scope**.

**Rationale**
- Visual structure must be stabilised before semantic or architectural reasoning
- Mixing visual and behavioural concerns leads to brittle heuristics
- Later pipeline stages are better suited to architectural interpretation

**Implications**
- Out-of-scope components pass through unchanged
- They must not influence flattening heuristics or outcomes
- No attempt is made to reinterpret non-visual components

---

## Structural Noise vs Semantic Independence

**Decision**  
Named visual subcomponents are treated as **structural noise** unless they demonstrate semantic independence.

A component is preserved during normalisation only if it:
- Owns behaviour (state, effects, context), or
- Is semantically independent of its parent, or
- Is used in different visual roles within the provided scope

**Rationale**
- AI-generated prototypes aggressively name layout subdivisions
- Most such components do not represent meaningful abstraction boundaries
- Preserving them pollutes downstream semantic analysis

**Implications**
- Default behaviour is flattening
- Preservation is opt-in, not opt-out
- “Component families” (e.g. Card*, Dialog*) collapse into a single conceptual unit

---

## Flattening vs Retention Separation

**Decision**  
Flattening decisions are strictly separated from retention decisions.

Flattening determines *structure*.  
Retention determines *what files remain* after flattening.

**Rationale**
- Reuse, export, and unused signals are orthogonal to structure
- Mixing these concerns leads to accidental preservation
- Separation keeps heuristics simple and explainable

**Implications**
- Export status, usage count, and unused status must not block flattening
- Unused components are handled in a post-flatten retention pass
- Retention behaviour depends on execution mode (edit-in-place vs output)

---

## CSS Computation over Translation

**Decision**  
CSS is **computed into a canonical style model**, not translated selector-by-selector.

**Rationale**
- Prototypes contain overlapping, contradictory selectors
- Visual intent matters more than authoring patterns
- Target platforms require consolidated, explicit styles

**Implications**
- Structural normalisation must occur before style extraction
- Style extraction operates on resolved visual elements
- Multiple selectors may intentionally collapse into one conceptual style
- Deterministic tooling is preferred over LLM inference

**Out of Scope**
- Perfect CSS fidelity
- Preservation of original selector structure

---

## Deterministic-First Tooling Philosophy

**Decision**  
Pipeline stages should prefer **deterministic, rule-based tooling** wherever feasible.

LLMs may assist with heuristic or judgment-heavy tasks, but must not be required for correctness.

**Rationale**
- Deterministic tools are explainable and testable
- Auditability is essential for trust
- LLMs are best used as accelerators, not foundations

**Implications**
- No pipeline stage fundamentally requires an LLM
- Heuristic tooling must produce inspectable output
- Agent orchestration is an execution concern, not a correctness requirement

---

## Structural Completeness over Visual Fidelity

**Decision**  
Generated UI code must be **structurally complete**, not visually perfect.

**Rationale**
- Eliminating heavy lifting is the primary goal
- Manual refinement is expected and acceptable
- Attempting perfect visual parity increases complexity and fragility

**Implications**
- Pages and views must be complete artefacts
- Approximate styling is acceptable
- Success is measured by completeness and clarity, not pixel parity

---

### Prototype Normalisation Tool — Implementation Decisions

These are **locked in** decisions unless explicitly revisited later:

*   **Prototype-only assumption**
    *   Input is an application prototype, not a library
    *   Components are assumed included for convenience, not necessity
*   **Closed-world assumption**
    *   Decisions are made strictly within the provided scope
    *   Anything outside the scope does not exist
*   **Visual-only scope**
    *   Only visual hierarchy participates
    *   Behavioural / contextual / infrastructural components are out of scope
    *   They must not influence heuristics or outcomes
*   **Flattening rules**
    *   Default = flatten
    *   Preserve only if:
        *   Behavioural ownership (hooks, state, effects)
        *   Semantic independence
        *   Cross-visual-role usage _within scope_
*   **Component families**
    *   Named subcomponents of a visual unit (Card\*, Dialog\*, etc.) are structural noise
    *   Flatten into a single conceptual component unless independently justified
*   **Flattening ≠ deletion**
    *   Retention (retain/remove/archive unused) is a _separate post-pass_
    *   Unused/exported status must not block flattening
*   **Execution modes**
    *   Edit-in-place vs emit-to-new-tree is explicit and mandatory
    *   Defaults differ depending on mode
*   **Deterministic, non-LLM**
    *   Heuristic, but rule-based
    *   Explainable and auditable output

This is one of the most thoroughly specified tools in the whole system already.

* * *

### CSS / Style Computation Tool — Implementation Decisions

These are decisions we **explicitly reasoned through**, not just hinted at:

*   **CSS is computed, not translated**
    *   Selector-by-selector conversion is explicitly rejected
    *   Visual intent > authoring fidelity
*   **Order matters**
    *   Structural normalisation must happen _before_ style extraction
    *   CSS computation operates on resolved visual elements
*   **Collapse is intentional**
    *   Multiple selectors may collapse into a single conceptual style
    *   Variants are identified deliberately
*   **Deterministic tooling**
    *   This is not primarily an LLM task
    *   Programmatic computation is preferred
*   **Output is a canonical style model**
    *   Not platform-specific
    *   Suitable for tokenisation and theming

These decisions absolutely need to be captured in a design-decisions artefact.

* * *

### UI Code Generation Tool — Implementation Decisions

Already agreed:

*   Output is **structurally complete**, not final
*   Visual match is approximate by design
*   Human refinement is expected
*   Heavy lifting must be eliminated
*   Pages and views must be complete artefacts

This directly constrains:

*   Success criteria
*   User expectations
*   How “good enough” is defined

