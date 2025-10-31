# ADR: FsEntry Validation and MAUI Community Toolkit Dependency

## Context

`FsEntry` is a core FlagstoneUI control designed to wrap the native `Entry` control, removing platform styling and applying theme tokens through `DynamicResource`. The control aims to provide a consistent, theme-driven experience while maintaining compatibility with existing MAUI patterns.

The key design question was whether to include a **hard dependency on the MAUI Community Toolkit (MCT)** for validation and behavior support, and how to expose validation extensibility to consumers.

Originally, Flagstone used a `PassthroughBehavior` approach that forwarded behaviors to the internal `Entry`. With Flagstone's mission shifting toward being the **theming substrate for .NET MAUI**, this required reassessment.

---

## Decision

### 1. Community Toolkit Dependency

* **Decision:** Do not add a hard dependency on `CommunityToolkit.Maui` to `FlagstoneUI.Core`.
* **Rationale:**

  * Keeps `Core` clean, lightweight, and independent.
  * Aligns with Flagstone's role as a *neutral UI foundation*, not a control suite.
  * Avoids version coupling and unnecessary maintenance overhead.
* **Implementation:** Use MCT in **samples** and **documentation** to demonstrate optional integration.

### 2. Validation Strategy

* **Decision:** `FsEntry` will not include built-in validation logic.
* **Rationale:** Validation is an application concern; Flagstone provides the visual and state hooks, not the rules.
* **Outcome:** Consumers can attach any `Behavior<VisualElement>` (including MCT `ValidationBehavior`) directly to `FsEntry`.

### 3. Behavior Passthrough

* **Decision:** Test `ValidationBehavior` against `FsEntry`. If it attaches successfully (likely, since it targets `VisualElement`), no passthrough is needed.
* **Fallback:** If MCT validators require direct `Entry` access, introduce an `EntryBehaviors` collection property that forwards attached behaviors to the inner `Entry`.
* **Rationale:** Keeps the API minimal and idiomatic while preserving full extensibility.

### 4. Naming and API Surface

* **Decision:** Replace the legacy `PassthroughBehavior` pattern with a clean property name: `FsEntryBehaviors` (or singular `FsEntryBehavior`).
* **Rationale:** Reflects intentional extensibility rather than a workaround. Consistent with Flagstone naming conventions.

### 5. Long-Term Positioning

* **Decision:** Maintain philosophical alignment with the MAUI Community Toolkit without formal dependency.
* **Future Option:** Create an optional `FlagstoneUI.ToolkitIntegration` package for deeper MCT interoperability (e.g., pre-wired validators).

---

## Consequences

| Aspect               | Impact                                               |
| -------------------- | ---------------------------------------------------- |
| **Dependency graph** | Core remains pure (.NET MAUI only).                  |
| **Extensibility**    | Behaviors can be attached directly to `FsEntry`.     |
| **Documentation**    | Include MCT integration guidance and examples.       |
| **Future packages**  | Optional integration layer can extend compatibility. |
| **API stability**    | Minimal, idiomatic surface area to maintain.         |

---

## Outcome

`FsEntry` will be considered **MVP-complete** when:

* It provides token-based, neutral styling;
* Supports attaching external behaviors (directly or via `EntryBehaviors` if required);
* Demonstrates compatibility with MCT validators.

This reinforces FlagstoneUI's mission:

> **FlagstoneUI is the CSS of .NET MAUI — a community-driven, tokenized UI foundation.**
