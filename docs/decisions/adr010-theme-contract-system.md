# ADR010: Theme Contract System

**Status**: Deferred (part of Flagstrap)  
**Date**: 2026-01-26  
**Deciders**: @matt-goldman, AI Assistant
**Superseded by**: ADR011 (Token System Repositioning) - contract system deferred as part of Flagstrap  
**Relates to**: ADR009 (Agent Guidance Strategy)

## Context

During analysis of the token system architecture, we identified a conceptual confusion around what the token system represents and how themes should interact with it. Two problematic interpretations had emerged:

1. **Overly Authoritative**: Base tokens in Core are the complete, fixed catalog of all possible tokens
2. **Completely Arbitrary**: The token system has no contract—themes can do anything

Neither interpretation serves the goal of enabling a toolchain that can generate FlagstoneUI themes from external sources (e.g., web app style extraction).

### Gap Analysis

| Area | Finding |
|------|---------|
| **Tokens.xaml** | ✅ Good structure, semantic naming, proper types |
| **tokens-catalog.json** | ❌ Conflates contract (schema) with values; empty control definitions |
| **TokenGenerator** | ⚠️ Extracts tokens but not the styling surface; control analysis returns placeholders |
| **ThemeValidator** | ⚠️ Validates token existence but not style completeness |
| **Style Contract** | ❌ No formal definition of what a valid theme must provide |

### The Core Question

What does "a valid FlagstoneUI theme" mean?

We considered two options:
1. **Minimal**: A theme must provide implicit styles for all Fs* controls
2. **Extended**: A theme must also provide specific named variants (OutlinedButton, TextButton, etc.)

## Decision

We adopt a **layered contract system** that separates concerns:

### Layer 1: Styling Surface (FlagstoneUI.Core)

Defines what CAN be styled:
- Fs* controls and their bindable properties
- Base token schema (keys, types, default values)
- No requirements on themes—this is the "canvas"

### Layer 2: Theme (Minimum Viable Contract)

Defines what a valid **theme** MUST provide:
- Token values (can override base tokens, can add new tokens)
- Implicit style for every Fs* control

This is the `minimal` contract—the floor that all themes must meet.

### Layer 3: Design System (Optional Extension)

Defines what a valid **design system implementation** MUST provide:
- Extends a base contract (typically `minimal`)
- Requires specific named style variants (e.g., OutlinedButton, ElevatedCard)
- May require additional semantic tokens (e.g., Color.Danger, Color.Success)

Examples: `material`, `bootstrap`, or community-defined design systems.

### Contract Resolution

Contracts are resolved from multiple sources:

```bash
# Built-in contracts (ship with tooling)
flagstone validate --contract minimal

# Local file (development/custom)
flagstone validate --contract ./my-design-system.json

# NuGet package (published design systems)
flagstone validate --contract nuget:Acme.DesignSystem.Contract

# URL (hosted contracts)
flagstone validate --contract https://example.com/contract.json
```

### Contract Inheritance

Design system contracts can extend other contracts:

```json
{
  "name": "Acme.DesignSystem",
  "extends": "minimal",
  "requiredStyles": {
    "FsButton": ["GhostButton", "DangerButton"]
  },
  "requiredTokens": {
    "Color.Danger": { "type": "color" }
  }
}
```

### Theme Declaration

Themes declare what contracts they implement:

```json
// flagstone-implements.json in theme package
{
  "implements": ["minimal", "Acme.DesignSystem"]
}
```

## Terminology

| Term | Definition |
|------|------------|
| **Styling Surface** | The set of stylable properties exposed by FlagstoneUI controls |
| **Token** | A named design value (color, spacing, radius, etc.) |
| **Theme** | Token values + implicit styles for all Fs* controls |
| **Design System** | A contract extending Theme, requiring specific named style variants |
| **Contract** | A JSON document defining validation requirements |

## Consequences

### Positive

- **Clear validation semantics**: "Valid theme" has a precise, testable definition
- **Extensible**: Community can define design system contracts
- **Pipeline-friendly**: External toolchains can target `minimal` or stricter contracts
- **Decoupled concerns**: Token values, implicit styles, and named variants are separate layers
- **Self-documenting**: Contracts serve as both validation spec and documentation

### Negative

- **Additional tooling required**: TokenGenerator must be enhanced
- **Schema maintenance**: Contract schema must be versioned and maintained
- **Learning curve**: Users must understand the layered model

### Neutral

- **Current Material theme unchanged**: It already provides implicit styles + named variants
- **Backward compatible**: Existing themes work; contracts add validation, not requirements

## Implementation Plan

### Phase 1: Contract Schema
1. Define JSON Schema for design system contracts
2. Create `minimal.json` contract (the Layer 2 floor)
3. Create `material.json` contract (what Material theme provides)

### Phase 2: TokenGenerator Enhancement
1. Add reflection-based control surface extraction
2. Generate `minimal` contract from Fs* control analysis
3. Add contract validation mode

### Phase 3: Documentation
1. Update tokens.md with layered architecture explanation
2. Update token-catalog-system.md with contract system
3. Create design system authoring guide

### Phase 4: Integration
1. Enhance BootstrapConverter to output contract-compliant themes
2. Add contract validation to CI pipeline
3. Consider NuGet package convention for shipping contracts

## File Structure

```
docs/
├── schemas/
│   └── design-system-contract.schema.json   # JSON Schema
├── contracts/
│   ├── minimal.json                         # Layer 2 floor
│   ├── material.json                        # Material Design system
│   └── bootstrap.json                       # Bootstrap-style system
├── tokens-catalog.json                      # Default token VALUES (rename?)
└── tokens.md                                # Human documentation
```

## References

- [tokens.md](../reference/tokens.md) - Human-readable token documentation
- [token-catalog-system.md](../reference/token-catalog-system.md) - Machine-readable catalog docs
- [TokenGenerator](../../tools/FlagstoneUI.TokenGenerator/) - Tooling for catalog/contract generation
