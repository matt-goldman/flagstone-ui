# ADR011: Token System Repositioning

**Status**: Proposed (Not Yet Implemented)
**Date**: 2026-02-08
**Deciders**: Matt Goldman
**Supersedes**: ADR010 (Theme Contract System) - conceptually updated
**Relates to**: ADR009 (Agent Guidance Strategy)

## Context

The current FlagstoneUI architecture positions the token system as a **core feature** and foundational principle. This manifests in:

1. **Documentation** framing FlagstoneUI as "token-first design"
2. **Material theme** being positioned as a baseline rather than an example
3. **Tooling** (TokenGenerator, BootstrapConverter) centered around token workflows
4. **Agent guidance** (AGENTS.md, copilot-instructions.md) emphasizing tokens as primary

This positioning has created confusion and suboptimal outcomes:

- AI agents (and developers) focus on token compliance rather than control styling
- The "styling surface" (what controls expose) is obscured by token abstraction
- Simple use cases (custom themes without tokens) feel unsupported
- Material is treated as "the standard" rather than "an example"

### The Root Problem

The token system was designed to solve a specific problem: **rapid bootstrapping of consistent apps using a shared design language**. However, by making it the foundational principle, we've created an impression that:

1. Tokens are required to use FlagstoneUI
2. Themes must be token-based
3. Material represents the expected baseline

This is backwards. The **actual value proposition** of FlagstoneUI is:

> **A unified styling plane for .NET MAUI - controls with full visual control from shared code**

### The Web Analogy

Consider how web development works:

- **HTML elements** can be styled inline, via stylesheets, or via design language systems (DLS)
- **CSS** is the styling plane - it enables all approaches
- **Bootstrap** is a DLS built on top of CSS - it provides well-known style names (`btn btn-primary`) and uses CSS variables/tokens as an implementation detail
- Developers can use CSS directly, or use Bootstrap, or mix approaches

**FlagstoneUI should be analogous to CSS** - the unified styling plane that enables all approaches. The token system (Flagstrap) is more like Bootstrap - a separate design language system built on top of FlagstoneUI.

### Current Conflation

The token system has been conflated with FlagstoneUI core because:

1. The Bootstrap converter tooling has been very successful
2. The Material theme uses tokens extensively
3. Sample themes demonstrate token-based theming
4. This success created conceptual "gravity" that accreted more than intended

The tokens are not a layer in FlagstoneUI's architecture - they're an implementation detail of a specific approach (Flagstrap) that should be considered separately.

## Decision

**Clarify the separation between FlagstoneUI (the styling plane) and Flagstrap (a design language system). Defer Flagstrap development to focus on core FlagstoneUI.**

### FlagstoneUI Core: The Styling Plane

FlagstoneUI provides **enhanced controls** (`FsButton`, `FsCard`, `FsEntry`, `FsEditor`) with **all visual properties exposed** for styling. This is analogous to CSS for the web.

**FlagstoneUI Architecture**:
```
Controls → Styles → Themes
```

That's it. This is what FlagstoneUI IS:
- Controls with full visual control from shared code
- Standard .NET MAUI styling patterns (inline, explicit styles, implicit styles)
- Themes as collections of styles
- No token requirement

### Flagstrap: A Design Language System Built on FlagstoneUI (Deferred)

Flagstrap is a **higher-level abstraction** built on FlagstoneUI, analogous to how Bootstrap is built on CSS, or DaisyUI is built on Tailwind.

**Flagstrap adds a layer**:
```
FlagstoneUI:  Controls → Styles → Themes
                 ↑
Flagstrap:    Tokens → Well-known Style Names
```

Key characteristics:
- **Built on FlagstoneUI** - uses FlagstoneUI controls and styling capabilities
- Provides **well-known style names** as the contract (e.g., `Style="Primary"`)
- Uses **tokens as an implementation detail** to make theming easier
- Themes commit to providing the contracted style names
- Tokens enable rapid theme creation by overriding values

**Important clarification**: Flagstrap IS built on FlagstoneUI (like Bootstrap on CSS). However, it's a **separate project**, not part of FlagstoneUI's core architecture. Developers choose whether to:
1. Style directly using FlagstoneUI's styling surface, OR
2. Use Flagstrap's pre-built abstractions

Both are valid approaches. Using Flagstrap is not required to use FlagstoneUI.

### What This Means Practically

FlagstoneUI enables multiple valid approaches:

**Approach 1: Direct Styling (inline)**
```xaml
<FsButton Text="Submit" BackgroundColor="#6750A4" CornerRadius="12" />
```

**Approach 2: Theme-based Styling (implicit styles)**
```xaml
<!-- Implicit style - applies to all FsButton by default -->
<Style TargetType="fs:FsButton">
    <Setter Property="BackgroundColor" Value="#6750A4" />
</Style>

<!-- Usage -->
<FsButton Text="Submit" />
```

**Approach 3: Named/Explicit Styles (standard .NET MAUI)**
```xaml
<!-- Named styles for variants - this is standard FlagstoneUI, NOT Flagstrap -->
<Style x:Key="OutlinedButton" TargetType="fs:FsButton">
    <Setter Property="BackgroundColor" Value="Transparent" />
    <Setter Property="BorderColor" Value="#6750A4" />
    <Setter Property="BorderWidth" Value="1" />
</Style>

<!-- Usage -->
<FsButton Text="Cancel" Style="{StaticResource OutlinedButton}" />
```

**Note on Explicit Styles**: Themes can (and often should) provide both implicit styles for the default look AND named explicit styles for variants (e.g., `OutlinedButton`, `UnderlineEntry`, semantic names like `DeleteButton`). This is standard .NET MAUI styling - not exclusive to Flagstrap.

**Approach 4: Using Flagstrap (built on FlagstoneUI - Future/Deferred)**
```xaml
<!-- Flagstrap defines a CONTRACT for required style names -->
<!-- Theme authors commit to providing these contracted names -->
<FsButton Text="Submit" Style="{StaticResource Primary}" />
```

**What makes Flagstrap different** from regular explicit styles: Flagstrap would define a **contract** that theme authors must follow - specific style names that MUST exist in any Flagstrap-compliant theme. This is deferred.

Approaches 1-3 use FlagstoneUI directly. Approach 4 uses Flagstrap (when developed).

### Material Theme Repositioning

The Material theme is currently an example that uses tokens internally and provides named style variants. It should be repositioned as:

- An **example theme** (one approach among many)
- Demonstrates use of tokens as an internal implementation detail
- NOT a baseline or requirement
- Sample themes with different approaches should be equally promoted

### Tooling Clarification

FlagstoneUI tooling (BootstrapConverter, future v0/React/Next.js converters) **outputs FlagstoneUI-compatible themes** - NOT Flagstrap themes. The tools:

- Convert external theme definitions to FlagstoneUI styles
- May use tokens internally as an implementation convenience
- Output is standard FlagstoneUI (Controls → Styles → Themes)
- Are NOT dependent on or producing Flagstrap artifacts

The TokenGenerator is a utility for working with token-based themes, but tokens themselves are just an implementation detail - one way to organize style values. Using tokens doesn't mean using Flagstrap.

### Deferring Flagstrap

Given that:
1. The core FlagstoneUI styling plane needs to be complete and robust first
2. Token/Flagstrap work has created conceptual confusion
3. The Bootstrap converter success has added unintended conceptual weight

**We will defer Flagstrap development** and focus on:
1. Clarifying FlagstoneUI as the styling plane
2. Ensuring controls and theming work without tokens
3. Repositioning existing token work as "Flagstrap exploration"
4. Adding non-token theme examples

## Consequences

### Positive

1. **Clearer value proposition**: "Unified styling plane for .NET MAUI"
2. **Lower barrier to entry**: No need to understand tokens to use FlagstoneUI
3. **Better AI guidance**: Focus on control properties, not token compliance
4. **Flexibility**: Multiple valid approaches clearly documented
5. **Honest positioning**: Material as example Flagstrap theme, not core requirement
6. **Focus**: Core styling plane gets proper attention before layering complexity

### Negative

1. **Documentation overhaul**: Significant docs need rewriting
2. **Existing expectations**: Users familiar with token-first messaging may be confused
3. **Tooling positioning**: Bootstrap converter positioned as Flagstrap tooling
4. **Deferred work**: Flagstrap design system concept not fully developed yet

### Neutral

1. **No code changes required**: This is primarily a repositioning
2. **Token system still exists**: Reframed as Flagstrap exploration/example
3. **Existing themes still work**: No breaking changes
4. **Bootstrap converter keeps working**: Just repositioned conceptually

## Implementation Overview

See [rethinking-tokens.md](../rethinking-tokens.md) for detailed implementation plan.

### High-Level Phases

1. **Phase 1**: Update core messaging (README, agent guidance)
2. **Phase 2**: Revise documentation (remove token-first framing)
3. **Phase 3**: Add non-token theme examples (demonstrate flexibility)
4. **Phase 4**: Reposition tooling as Flagstrap-related

### What We're NOT Doing

- NOT removing token system or tooling
- NOT deprecating Material theme
- NOT changing any APIs or code
- NOT fully developing Flagstrap now (deferred)

## Alternatives Considered

### 1. Status Quo

Keep token-first positioning but improve documentation clarity.

**Rejected because**: The problem is structural, not just documentation. Token-first framing inherently obscures the control surface and creates the wrong mental model.

### 2. Remove Token System Entirely

Eliminate tokens and make all themes use direct values.

**Rejected because**: Tokens have legitimate value for design system consistency. The issue is positioning, not existence. Token work becomes Flagstrap exploration.

### 3. Make Flagstrap a Separate Repository

Move all token-related code to a separate repository.

**Rejected for now because**: The core styling plane needs to be proven first. Premature separation would add complexity. May reconsider later when Flagstrap is properly designed.

### 4. Continue Developing Flagstrap in Parallel

Fully develop both FlagstoneUI core and Flagstrap simultaneously.

**Rejected because**: This is what caused the current confusion. Focus is needed on getting FlagstoneUI core right first.

## References

- [rethinking-tokens.md](../rethinking-tokens.md) - Detailed implementation plan
- [ADR009: Agent Guidance Strategy](adr009-agent-guidance-strategy.md) - Related guidance decisions
- [ADR010: Theme Contract System](adr010-theme-contract-system.md) - Contract system (becomes Flagstrap component)
- Original GitHub issue describing the problem

## Decision Record

| Date | Decision | Rationale |
|------|----------|-----------|
| 2026-02-08 | Proposed | Initial proposal based on user feedback and clearer mental model |
