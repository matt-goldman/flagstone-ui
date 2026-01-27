---
context: This is a feedback comment given to Copilot following implementation of the tool according to the spec. Subsequent refactor delivered the expected outcome.
---

@copilot This is a strong first pass and the tool is behaving consistently with the current spec. The issue I’m seeing is that component preservation is still too aggressive for the intended use case (AI-generated prototypes, not component libraries).

I want to tighten the rules so that normalisation removes structural noise more aggressively, while keeping behaviourally or semantically independent components intact.

## Key clarification (scope)

This tool operates on a closed world assumption.
Flattening decisions must be made only within the provided context (files/folders passed to the tool). Anything outside that scope is explicitly ignored.

**Visual-only scope**

This tool operates exclusively on visual components that participate in the rendered UI hierarchy.

Components that are primarily behavioural, contextual, or infrastructural (e.g. providers, context wrappers, portals, side-effect-only components) are out of scope for normalisation.

Out-of-scope components must not influence flattening decisions or heuristics and should be passed through unchanged.

**Prototype-only assumption**

This tool assumes the input represents an application prototype, not a curated or shared component library.

Component reuse intent outside the provided scope is explicitly ignored.

Assume components have been included for convenience, not necessity.

## Requested changes

### 1. Flattening must be structural only

Flattening decisions should be based only on structural and semantic considerations.

The following signals must NOT influence flattening decisions:

* `export` status
* usage count
* whether a component is unused after flattening
* whether a component “might” be used elsewhere

These signals may be used later for _retention_ (remove / archive / retain), but not to block flattening.

### 2. Invert the default assumption

The default assumption should be:

> Components are _not_ independent unless proven otherwise.

A component should be preserved during flattening only if at least one of the following is true:

#### 1. Behavioural ownership

* Contains hooks, state, effects, or context
* Owns interaction logic that cannot be trivially hoisted

#### 2. Semantic independence

* Would still make sense if lifted out of its parent
* Can be meaningfully named without implying its parent’s visual role

A visual subcomponent that is only ever rendered as part of a single visual parent within the provided scope should be flattened into that parent.
If the same subcomponent is rendered independently of that parent within the provided scope, it should be preserved.

#### 3. Cross-visual-role usage (within scope only)

* Used under different conceptual parent components
* Reuse within the same visual component family does not count

If none of the above are true, the component should be flattened.

### 3. Treat component families explicitly

Components that:

* share a common prefix (e.g. `Card*`, `Dialog*`, `Accordion*`)
* are declared together or co-located
* are only used within the same visual unit

should be treated as a component family and flattened into a single conceptual component, unless an individual member independently meets the preservation criteria above.

This applies regardless of export status.

### 4. Separate flattening from retention

After flattening is complete:

* Identify unused components
* Apply `--unused-components [retain | remove | archive]`

Unused/exported status should only affect this post-pass and must not influence flattening decisions.

### 5. Explicit context boundary and output mode

The tool must operate under a closed-world assumption:

* Flattening decisions are made strictly within the provided input scope (files/folders passed to the tool)
* No assumptions are made about usage outside that scope
* If insufficient context is provided, aggressive flattening is expected and is the caller’s responsibility

The tool must support:

```bash
--edit-in-place        (modify files in the provided scope)
--output <directory>  (emit a normalised tree to a new location, already exists)
```

Retention behaviour for unused components applies after flattening and depends on the selected mode.

#### Sensible defaults

* Edit in place is **false** by default; if not specified, and an output directory is specified, emit to new location. If not explicitly set to true but no output location is required, fail.
* When emitting to a new location, default for unused components is remove (i.e. do not include in the output). Overridable with the CLI argument.
* When editing in place, default for unused components is _archive_ (i.e. create an archive folder and move unused components into there; an archive folder should be created at each level of the hierarchy where components are archived)

## Expected outcome

With these changes, prototype UI families (e.g. Card, Dialog, Accordion, Pagination, Alert, etc.) should collapse to a single conceptual component, while genuinely independent or behavioural components remain preserved.