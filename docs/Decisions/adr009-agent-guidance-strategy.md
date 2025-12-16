# ADR009: Agent Guidance Strategy - AGENTS.md vs MCP Server

**Status**: Accepted  
**Date**: December 16, 2025  
**Deciders**: Matt Goldman  
**Context**: AI agent integration strategy for FlagstoneUI

## Context and Problem Statement

With AI-assisted development becoming more prevalent, FlagstoneUI needs a strategy for helping AI agents (and human developers using AI tools) generate quality FlagstoneUI code. The question is: what's the most effective way to provide this guidance?

## Decision Drivers

1. **Broad Compatibility**: Works with any AI tool (Claude, GitHub Copilot, ChatGPT, etc.)
2. **Maintainability**: Easy to update as FlagstoneUI evolves
3. **Discoverability**: Developers and AI tools can easily find the guidance
4. **Development Velocity**: Don't create infrastructure that slows down core feature work
5. **Real Value**: Focus on what developers actually need, not theoretical use cases
6. **Version Control**: Guidance should be versioned with the codebase

## Considered Options

### Option 1: Comprehensive MCP Server

**Description**: Build a Model Context Protocol server with tools for:
- Bootstrap theme conversion
- Documentation access
- Theme analysis
- Example provision
- UI generation from natural language

**Pros**:
- ✅ Rich, interactive tooling
- ✅ Structured protocol
- ✅ Could enable "vibe-coding" complete UIs

**Cons**:
- ❌ Only works with MCP-compatible clients (Claude Desktop, limited VS Code support)
- ❌ Significant infrastructure to build and maintain
- ❌ Unclear value proposition for UI generation
- ❌ Bootstrap converter already works as CLI tool
- ❌ Documentation already exists in repo
- ❌ Most developers won't be converting Bootstrap themes (niche use case)

**Core Issue**: Struggled to identify tools that would be genuinely useful beyond what already exists (CLI tools, docs).

### Option 2: AGENTS.md Documentation File (Selected)

**Description**: Create a comprehensive `AGENTS.md` file in the repository root with:
- Architecture overview (token-first design, available controls)
- Code generation patterns (semantic color usage, token bindings)
- Real code examples of common patterns
- Available CLI tools (Bootstrap converter, Tailwind palette converter)
- DO/DON'T anti-patterns
- Integration guidance (Crosswind, design tools)

**Pros**:
- ✅ Works with ANY AI tool (no protocol dependency)
- ✅ Lives in repo (discoverable via file search, version controlled)
- ✅ Can be included in .NET project templates
- ✅ Zero infrastructure to maintain
- ✅ Easy to update alongside code changes
- ✅ Familiar pattern (like CONTRIBUTING.md, CODE_OF_CONDUCT.md)
- ✅ Humans can read it too (onboarding documentation)

**Cons**:
- ⚠️ Less interactive than MCP tools
- ⚠️ Static content (but this is also a pro for stability)

### Option 3: Figma Converter (Future Priority)

**Description**: Tool to convert Figma design tokens and components to FlagstoneUI themes.

**Pros**:
- ✅ Bridges real designer → developer workflow
- ✅ Figma is industry-standard design tool
- ✅ Design systems in Figma → FlagstoneUI themes (legitimate use case)
- ✅ Complements Crosswind (design tokens vs utility classes)
- ✅ Competitive differentiator

**Implementation Options**:
- Figma Plugin (TypeScript) - exports design tokens as JSON
- .NET CLI tool - reads Figma API, generates XAML/C#
- Hybrid approach

**Priority**: Medium-term (after core controls complete)

## Decision Outcome

**Chosen option**: **AGENTS.md documentation file** with future consideration for Figma converter.

### Rationale

1. **Real Developer Needs**: Developers need to know how to structure FlagstoneUI apps, use tokens properly, and follow best practices. Static documentation serves this better than interactive tools with unclear value.

2. **Bootstrap Converter Reality**: It's a nice showcase and useful for demonstrating FlagstoneUI capabilities, but most developers won't be converting Bootstrap themes. It's already available as a CLI tool - that's sufficient.

3. **Broad Compatibility**: AGENTS.md works with every AI tool and IDE, not just MCP-compatible clients.

4. **Template Integration**: Can be included in future .NET project templates along with opinionated patterns and full-stack solution guidance.

5. **Focus on Core Value**: Time is better spent on core controls and features than building infrastructure for hypothetical use cases.

## Implementation Plan

### AGENTS.md Structure

```markdown
# FlagstoneUI Agent Guide

## Overview
- What FlagstoneUI is (token-first design system)
- Philosophy and approach
- When to use FlagstoneUI

## Architecture
- Token system (semantic naming)
- Theme structure
- Control library (FsButton, FsCard, FsEntry, FsEditor)
- Integration points (MCT)

## Code Generation Patterns

### Using Tokens Correctly
✅ DO: <fs:FsButton BackgroundColor="{DynamicResource Color.Primary}" />
❌ DON'T: <fs:FsButton BackgroundColor="#a2e436" />

### Semantic Color Selection
- Color.Primary - primary brand actions
- Color.Success - success states
- Color.Error - error states
[etc.]

### Spacing Guidelines
- Use token scale: Spacing.Small, Spacing.Medium, Spacing.Large
- Don't use magic numbers

### Form Validation Patterns
[Real examples with FsEntry + MCT ValidationBehavior]

### Button Style Selection
[When to use default vs OutlinedButton vs TextButton]

## Common Patterns
[Real code for: forms, cards, navigation, etc.]

## Available Tools
- Bootstrap theme converter: `flagstone-bootstrap convert --help`
- Tailwind palette converter: `[tool-name]`

## Integration
- Crosswind (when available)
- Community Toolkit MAUI
- Future: Figma design token import

## Anti-Patterns (DO NOT)
- Hardcode colors or spacing values
- Use .NET MAUI controls directly (use Fs* equivalents)
- Override token values in component styles
- [Other anti-patterns]

## Examples Repository
[Link to sample apps and common patterns]
```

### Future: Figma Converter

When FlagstoneUI control library is more mature:

1. **Research Phase**:
   - Analyze Figma design token export formats
   - Map Figma primitives to FlagstoneUI tokens
   - Define component mapping strategy

2. **Prototype**:
   - Figma plugin OR Figma API client
   - Token extraction and mapping
   - XAML generation

3. **Integration**:
   - Document in AGENTS.md
   - Add to project templates
   - Consider MCP tool wrapper (if Figma converter proves valuable)

## Consequences

### Positive

1. **Immediate Value**: AGENTS.md can be created now and provides instant benefit
2. **No Infrastructure Burden**: Zero maintenance overhead for protocol servers
3. **Universal Compatibility**: Works with all AI tools and IDEs
4. **Template Ready**: Can be included in .NET templates when ready
5. **Human Benefit**: Also serves as onboarding documentation for new developers
6. **Version Control**: Guidance evolves with codebase

### Negative

1. **Less Interactive**: No dynamic tool invocation (but unclear if this is actually needed)
2. **Manual Updates**: Requires updating documentation when patterns change (but this is good discipline)

### Neutral

1. **MCP Server Not Ruled Out**: Could revisit if compelling use cases emerge (e.g., Figma converter as MCP tool)
2. **Bootstrap Converter Exists**: Already available as CLI tool, just won't have MCP wrapper

## Future Considerations

### When to Reconsider MCP Server

Revisit MCP server if:
- Figma converter proves valuable and would benefit from MCP integration
- FlagstoneUI.Blocks (app screens) library creates opportunities for UI composition tools
- Clear use cases emerge for interactive agent tooling
- MCP client adoption becomes widespread

### AGENTS.md Evolution

Update AGENTS.md when:
- New controls added
- New patterns established
- Integration tools available (Crosswind, Figma)
- Anti-patterns identified
- Templates published

## Related Decisions

- ADR005: Bootstrap Converter Analysis Modes (CLI tool is sufficient)
- ADR008: Converter Advanced Features (focuses on conversion quality, not agent interaction)
- Future: ADR for Figma converter architecture (when implemented)

## References

- Bootstrap Converter CLI: `tools/FlagstoneUI.BootstrapConverter.Cli/`
- Tailwind Palette Converter: [external tool]
- Crosswind: Steven Thewissen's Tailwind-to-MAUI project
- Model Context Protocol: https://modelcontextprotocol.io/
- .NET Templates: Future FlagstoneUI project templates
