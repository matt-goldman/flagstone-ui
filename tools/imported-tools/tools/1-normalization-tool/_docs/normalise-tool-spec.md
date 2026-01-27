Tool 1: Prototype Normaliser
----------------------------

### Working name (internal)

`proto-normalise`  
Name doesn’t matter yet. The contract does.

* * *

Purpose
-------

> Transform AI-generated or over-componentised React code into a **flattened, human-readable, structurally honest React tree** that preserves visual intent and is suitable for _any_ downstream rewrite (Angular, MAUI, Blazor, something else).

This tool:

*   Does **not** target MAUI
*   Does **not** generate XAML
*   Does **not** apply styling decisions
*   Does **not** change behaviour

It exists to **remove noise**.

* * *

Input
-----

### Required

*   One or more React source files
    *   JSX / TSX
    *   May include:
        *   Inline components
        *   Colocated components
        *   Excessive abstraction
        *   Tailwind / CSS class usage
*   Optional CSS files (only for reference, not transformation)

### Assumptions (v1)

*   Functional components only
*   No class components
*   No hooks logic is modified
*   No state logic is modified

If hooks exist, they are preserved verbatim.

* * *

Output
------

### Primary output

*   A **normalised React source tree** where:
    *   Trivially-scoped components are inlined
    *   The remaining component boundaries are meaningful
    *   Layout hierarchy is explicit and readable

### Secondary output (metadata)

A machine-readable artefact (JSON is fine) containing:

For every transformation:

*   Original file
*   Original component name
*   Reason for flattening or preservation
*   New location
*   Line range mapping (best effort)

This is critical. Even if you don’t surface it yet.

* * *

Non-goals (explicit)
--------------------

The tool **must not**:

*   Rename semantic elements
*   Change props
*   Infer intent
*   Re-order visual hierarchy
*   Optimise performance
*   “Clean up” code style beyond structure

If it feels like refactoring, it’s probably out of scope.

* * *

Core behaviour
--------------

### 1. Parse & build a component graph

For each component:

*   Identify:
    *   Where it is declared
    *   Where it is used
    *   How many times it is used
    *   Whether it has side effects (hooks, context)

This is purely analytical.

* * *

### 2. Identify **inline-eligible components**

A component is _eligible_ for flattening if **all** are true:

*   Used in exactly one **semantic parent**
*   Hook detection is shallow and lexical in v1 (presence-based, not control-flow aware).
*   Contains no context providers or consumers
*   Does not export anything else
*   Does not accept children _or_ simply passes them through

This rule set is intentionally conservative.

* * *

### 3. Inline eligible components

For each eligible component:

*   Replace its usage with its JSX body
*   Hoist props as inline values
*   Preserve comments
*   Preserve formatting where reasonable

Original component code is removed **only if**:

*   It is not used elsewhere
*   It has no side effects

* * *

### 4. Preserve **structural components**

Components are preserved if **any** are true:

*   Used in multiple **semantic parents**
*   Represent a clear semantic boundary (heuristic)
*   Contain hooks or logic
*   Are likely to map to a page or reusable view later

This is where _restraint_ matters.

If unsure → preserve.

#### Definition: semantic parent

A component is considered to have multiple semantic parents **only if**:

*   It is used under **different conceptual components**, _and_
*   Those parents are not merely layout subdivisions of the same visual unit

In other words:

*   Same _visual component_, different internal locations → **not reusable**
*   Different _visual components_, different roles → **reusable**

See card flattening example for details.

A component should be preserved **only if at least one is true**:

1.  **Cross-component reuse**
    *   Used under different _top-level_ components or pages
    *   Example: `PrimaryButton` used in Card, Dialog, Toolbar
2.  **Behavioural ownership**
    *   Contains hooks, state, effects, or context
    *   Owns interaction logic
3.  **Semantic independence**
    *   Would still make sense if lifted out of its current parent
    *   Can be named without referencing its container

If none of those are true → flatten.

#### A useful heuristic (very practical)
-----------------------------------

When deciding whether to flatten, ask:

> “If I were rewriting this by hand, would I ever recreate this as a separate component?”

For:

*   `CardButton` → no
*   `UserAvatar` → maybe
*   `DatePickerField` → yes

* * *

### 5. Emit readable, boring code

Output code should optimise for:

*   Readability
*   Explicit structure
*   Minimal nesting indirection

This is not a formatter, but it should not make things worse.

* * *

Example (simplified)
--------------------

### Input

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

### Output

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

### Metadata

```json
{
  "flattened": [
    {
      "component": "ButtonWrapper",
      "reason": "single-use, presentational, no hooks",
      "originalFile": "Page.tsx"
    }
  ]
}
```

Example (conceptual)
--------------------

### Prototype structure (AI-style)

*   `Card`
    *   `CardHeader`
    *   `CardBody`
    *   `CardFooter`
        *   `CardActions`
            *   `CardButton`

`CardButton` is:

*   Used in multiple places
*   But only within the _Card family_
*   Always subordinate to Card semantics

Therefore:

*   ❌ Not a reusable component
*   ✅ Inline it
*   ✅ Replace conceptually later with `FsButton + styles`

Flattening result:

*   `Card`

* * *

CLI surface (minimal)
---------------------

You don’t need much.

```bash
proto-normalise ./src \
  --out ./normalised \
  --report ./normalisation.json
```

Options:

*   `--dry-run`
*   `--preserve-comments` (default true)
*   `--max-depth` (optional safety valve)

* * *

Test strategy (important)
-------------------------

You don’t need exhaustive tests. You need **confidence tests**.

Test categories:

*   Single-use component flattening
*   Multi-use component preservation
*   Hook preservation
*   Nested inline components
*   No-op cases (already flat code)

Golden-file tests are perfect here.

Implementation notes
--------------------

* The tool is implemented as a .NET console application with all core logic in a reusable class library.
* JSX/TSX parsing and rewriting is performed using a JavaScript AST engine (Babel), invoked as a subprocess.
* The C# layer is responsible for orchestration, rule evaluation, IO, and metadata generation.
* AST transforms must be deterministic when run in deterministic mode.
* The tool must support:
  - Single file input
  - Multiple explicit files
  - Recursive directory traversal with JSX/TSX filtering