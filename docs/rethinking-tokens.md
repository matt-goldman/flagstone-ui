# Rethinking Tokens: Complete Remediation Plan

This document outlines the complete plan for repositioning FlagstoneUI as the unified styling plane for .NET MAUI, and clarifying the separation from Flagstrap (the token-based design language system).

**Related ADR**: [ADR011: Token System Repositioning](Decisions/adr011-token-system-repositioning.md)

## Executive Summary

### The Problem

The token system has been positioned as foundational to FlagstoneUI, when FlagstoneUI should be the **styling plane** (like CSS) and tokens should be part of a **separate design language system** (like Bootstrap on CSS).

This conflation has caused:
1. AI agents focusing on token compliance instead of control styling
2. Simple use cases feeling unsupported
3. Material theme being treated as a baseline rather than an example
4. The actual value proposition (unified styling plane) being obscured

### The Mental Model

**FlagstoneUI** = The unified styling plane for .NET MAUI (like CSS)
```
Controls → Styles → Themes
```

**Flagstrap** = A design language system built ON FlagstoneUI (like Bootstrap on CSS, DaisyUI on Tailwind) - DEFERRED
```
FlagstoneUI:  Controls → Styles → Themes
                 ↑
Flagstrap:    Tokens → Well-known Style Names
```

Flagstrap is **built on** FlagstoneUI, but it's a **separate project** - not part of FlagstoneUI's core architecture. Developers choose whether to:
- Style directly using FlagstoneUI's styling surface, OR
- Use Flagstrap's pre-built abstractions

Both are valid approaches. Using Flagstrap is not required to use FlagstoneUI.

### The Fix

1. Reframe documentation to position FlagstoneUI as the styling plane
2. Remove token-first framing from core documentation
3. Defer Flagstrap development (tokens, contracts, tooling docs)
4. Add non-token theme examples demonstrating core FlagstoneUI
5. Reposition existing token work as "Flagstrap exploration"

---

## Impact Assessment

### Files Requiring Changes

#### High Impact (Core Messaging)

| File | Current State | Required Change | Priority |
|------|---------------|-----------------|----------|
| `README.md` | Token-first framing | Styling plane framing | P0 |
| `AGENTS.MD` | Token-first philosophy | Styling surface + flexibility | P0 |
| `.github/copilot-instructions.md` | Token-first styling | Multiple valid approaches | P0 |
| `docs/quickstart.md` | Token-based approach | Direct styling primary | P1 |

#### Medium Impact (Documentation)

| File | Current State | Required Change | Priority |
|------|---------------|-----------------|----------|
| `docs/architecture.md` | Token system as core | Controls → Styles → Themes | P1 |
| `docs/tokens.md` | Core documentation | Reframe as Flagstrap exploration | P2 |
| `docs/theming-guide.md` | Token-centric | Multiple approaches | P1 |
| `docs/token-catalog-system.md` | Core system | Token tooling (internal/optional) | P3 |
| `docs/implementation-status.md` | Status tracking | Update framing | P2 |
| `docs/roadmap.md` | Token-first references | Update terminology | P2 |

#### Control Documentation

| File | Current State | Required Change | Priority |
|------|---------------|-----------------|----------|
| `docs/Controls/FsButton.md` | Token integration emphasis | Styling surface primary | P1 |
| `docs/Controls/FsEntry.md` | Token integration emphasis | Styling surface primary | P1 |
| `docs/Controls/FsEditor.md` | Token integration emphasis | Styling surface primary | P1 |
| `docs/Controls/FsCard.md` | Token integration emphasis | Styling surface primary | P1 |

#### ADRs to Update

| File | Current State | Required Change | Priority |
|------|---------------|-----------------|----------|
| `adr009-agent-guidance-strategy.md` | Token-first references | Update to control-first | P2 |
| `adr010-theme-contract-system.md` | Core system | Mark as Flagstrap (deferred) | P2 |
| `adr003-button-corner-radius-type.md` | Token-first philosophy | Minor wording update | P3 |

#### Tooling Documentation

Tooling outputs FlagstoneUI themes - tokens are an internal implementation detail:

| File | Current State | Required Change | Priority |
|------|---------------|-----------------|----------|
| `tools/FlagstoneUI.TokenGenerator/README.md` | Core tool | Clarify tokens as optional implementation detail | P3 |
| `tools/FlagstoneUI.BootstrapConverter/README.md` | Token conversion | Clarify outputs FlagstoneUI themes | P3 |
| `docs/mcp-bootstrap-converter.md` | Token focus | Clarify outputs FlagstoneUI themes | P3 |

### Sample App Changes

| Component | Current State | Required Change | Priority |
|-----------|---------------|-----------------|----------|
| New sample theme | N/A | Add non-token theme example | P1 |
| Existing themes | Token-based | Add docs noting tokens are optional | P2 |

---

## Detailed Change Plans

### Phase 1: Core Messaging (P0)

#### 1.1 README.md Overhaul

**Current framing** (problematic):
```markdown
* Token‑based theming system (colour, spacing, shapes, typography)
```

**Updated framing** (styling plane):
```markdown
* Unified styling plane for .NET MAUI
* Use standard .NET MAUI styling: inline, styles, or themes
* Full visual control from shared code - no platform handlers
```

**Section changes**:

1. **"Why FlagstoneUI?"** - Emphasize the gap in .NET MAUI (styling properties not exposed) and how FlagstoneUI closes it (like CSS for the web)

2. **"What Does It Look Like?"** - Show direct styling first:
   ```xaml
   <!-- Direct styling - this is FlagstoneUI core -->
   <FsButton Text="Click Me" BackgroundColor="#6750A4" CornerRadius="12" />

   <!-- Theme-based styling -->
   <FsButton Text="Click Me" Style="{StaticResource MyButtonStyle}" />
   ```

3. **"How It Works"** - Reorder to emphasize:
   - Enhanced Controls (what FlagstoneUI provides)
   - Standard .NET MAUI styling patterns
   - Optional: design language systems like Flagstrap

4. **Remove/Defer** - Token system details (move to Flagstrap docs eventually)

#### 1.2 AGENTS.MD Restructuring

**Current framing** (problematic):
```markdown
### Philosophy
- **Token-First Design**: All visual properties reference semantic design tokens
```

**Updated framing** (styling plane):
```markdown
### Philosophy
- **Full Visual Control**: Controls expose all styling properties via BindableProperties
- **Standard .NET MAUI Styling**: Use inline values, StaticResource, DynamicResource, or styles
- **No Platform Code**: Style from shared code without handlers
- **Flexibility**: Use direct values, app resources, implicit styles, or design tokens - all valid
```

**Section changes**:

1. **Remove "Token System" as core concept** - Move to "Optional: Flagstrap" section
2. **"Best Practices"** - Remove "Always use tokens" - replace with "Choose the styling approach appropriate for your project"
3. **Anti-patterns** - Remove "hardcoded values are WRONG" - direct values are perfectly valid
4. **Add decision guidance**:

```
How to determine styling approach:
├─ Simple app or prototype? → Direct values work fine
├─ Theme-based app? → Use implicit styles
├─ Design system driven? → Consider Flagstrap approach
└─ Existing token-based themes? → Use DynamicResource with token keys
```

**Key behavioral changes for AI agents**:

| Current Guidance | New Guidance |
|-----------------|--------------|
| "Always use tokens for colors" | "Use approach appropriate for the project" |
| "Hardcoded values are WRONG" | "Direct values are valid for simple cases" |
| "Use DynamicResource for all properties" | "StaticResource, DynamicResource, or direct values all work" |
| "Reference Color.Primary, Space.16" | "Style using any standard .NET MAUI pattern" |

#### 1.3 copilot-instructions.md Updates

**Current framing**:
```markdown
- **Token-first styling**: Add/modify tokens in `Tokens.xaml`
```

**Updated framing**:
```markdown
- **Control styling**: Style controls using standard .NET MAUI patterns (inline, styles, themes)
- **Styling surface**: All visual properties exposed via BindableProperties
```

---

### Phase 2: Documentation Revision (P1)

#### 2.1 docs/quickstart.md

**Current approach**: Token-based theming as the approach

**Updated approach**: Show the simplest path first

1. **Quick Start (Direct Styling)**
   ```xaml
   <FsButton Text="Submit" BackgroundColor="Blue" TextColor="White" CornerRadius="8" />
   ```
   "This is valid FlagstoneUI. No themes, no tokens, just styled controls."

2. **Theme-based (Recommended for larger apps)**
   ```xaml
   <!-- Define styles in your theme -->
   <Style TargetType="fs:FsButton">
       <Setter Property="BackgroundColor" Value="Blue" />
       <Setter Property="TextColor" Value="White" />
       <Setter Property="CornerRadius" Value="8" />
   </Style>

   <!-- Usage -->
   <FsButton Text="Submit" />
   ```

3. **Advanced (Flagstrap approach - link to separate docs when ready)**
   "For design system consistency, see Flagstrap (coming soon)."

#### 2.2 docs/architecture.md

**Key changes**:

1. **Primary architecture**:
   ```
   FlagstoneUI Architecture:

   Controls → Styles → Themes

   That's it. This is the styling plane.
   ```

2. **Remove token system from core architecture** - Move to separate section:
   ```
   Flagstrap (Separate Project - Coming Later):

   Controls → Tokens → Styles → Themes

   A design language system built on FlagstoneUI.
   ```

3. **Add "Styling Surface"** section:
   - List all exposed properties per control
   - This is what makes FlagstoneUI valuable
   - This is what themes target

#### 2.3 docs/theming-guide.md

**Current**: Token-centric guide for designers

**Updated**: Styling-focused guide

**Structure**:
1. **"What is a FlagstoneUI Theme?"** - A collection of styles for FlagstoneUI controls
2. **"Creating a Theme"** - ResourceDictionary with implicit styles
3. **"Styling Approaches"**:
   - Direct values (simple)
   - App resource references (reusable)
   - See Flagstrap for token-based approach (link)
4. **Remove/Defer** - Token details, Material Design specifics

#### 2.4 docs/tokens.md

**Option**: Rename to `flagstrap-exploration.md` or add prominent header:

```markdown
# Flagstrap Token System (Exploration)

> **Note**: Flagstrap is a design language system concept built on FlagstoneUI.
> This is exploratory work and is NOT part of FlagstoneUI core.
> For core FlagstoneUI theming, see [theming-guide.md](theming-guide.md).
```

#### 2.5 Control Documentation Updates

For each control doc (`FsButton.md`, `FsEntry.md`, `FsEditor.md`, `FsCard.md`):

1. **Emphasize styling surface** - All exposed properties table
2. **Show direct styling first**:
   ```xaml
   <FsButton BackgroundColor="Blue" TextColor="White" CornerRadius="12" />
   ```
3. **Show theme-based styling**:
   ```xaml
   <Style TargetType="fs:FsButton">
       <Setter Property="BackgroundColor" Value="Blue" />
   </Style>
   ```
4. **Remove/minimize** token references in core examples

---

### Phase 3: Sample Additions (P1)

#### 3.1 New Non-Token Theme Example

Create a sample theme that demonstrates pure FlagstoneUI (no tokens):

```xaml
<!-- SimpleTheme.xaml - No tokens, just styles -->
<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
                    xmlns:fs="clr-namespace:FlagstoneUI.Core.Controls;assembly=FlagstoneUI.Core">

    <!-- Direct values in styles - completely valid FlagstoneUI -->
    <Style TargetType="fs:FsButton">
        <Setter Property="BackgroundColor" Value="#2196F3" />
        <Setter Property="TextColor" Value="White" />
        <Setter Property="CornerRadius" Value="8" />
        <Setter Property="Padding" Value="16,8" />
    </Style>

    <Style x:Key="SecondaryButton" TargetType="fs:FsButton">
        <Setter Property="BackgroundColor" Value="Transparent" />
        <Setter Property="TextColor" Value="#2196F3" />
        <Setter Property="BorderColor" Value="#2196F3" />
        <Setter Property="BorderWidth" Value="1" />
        <Setter Property="CornerRadius" Value="8" />
    </Style>

    <Style TargetType="fs:FsCard">
        <Setter Property="BackgroundColor" Value="#FAFAFA" />
        <Setter Property="CornerRadius" Value="12" />
        <Setter Property="Padding" Value="16" />
        <Setter Property="Elevation" Value="2" />
    </Style>

    <Style TargetType="fs:FsEntry">
        <Setter Property="Background" Value="White" />
        <Setter Property="BorderBrush" Value="#CCCCCC" />
        <Setter Property="BorderWidth" Value="1" />
        <Setter Property="CornerRadius" Value="4" />
        <Setter Property="Padding" Value="12,8" />
    </Style>

</ResourceDictionary>
```

**Placement**: `samples/FlagstoneUI.SampleApp/Themes/SimpleTheme.xaml`

**Message**: "This is a complete, valid FlagstoneUI theme. No tokens. No Flagstrap. Just styles."

#### 3.2 Update Sample App Documentation

Add documentation explaining the themes in the sample app:

- **SimpleTheme**: Pure FlagstoneUI - direct values in styles, no tokens
- **MaterialTheme**: Token-based approach - uses tokens as internal implementation
- **ConvertedThemes**: Bootstrap converter output - FlagstoneUI themes generated from Bootstrap

---

### Phase 4: Tooling Clarification (P3)

Tooling outputs FlagstoneUI themes. Tokens are an internal implementation detail.

#### 4.1 TokenGenerator

Add context to README:
```markdown
> **Note**: This tool helps work with token-based themes.
> Tokens are an implementation detail - one way to organize style values.
> FlagstoneUI core does not require tokens.
```

#### 4.2 BootstrapConverter

The Bootstrap converter should be positioned as:
- Converts Bootstrap themes to **FlagstoneUI-compatible themes**
- May use tokens internally as an implementation convenience
- The converted themes are standard FlagstoneUI themes
- NOT producing Flagstrap artifacts

#### 4.3 Contract System (ADR010)

Mark as Flagstrap component, defer full implementation.

---

## Migration Checklist

### Phase 1: Core Messaging (Week 1) - P0

- [ ] Update README.md - styling plane framing
- [ ] Restructure AGENTS.MD - remove token-first, add flexibility
- [ ] Update .github/copilot-instructions.md
- [x] Create this rethinking-tokens.md document ✅
- [x] Create ADR011 ✅

### Phase 2: Documentation (Week 2) - P1

- [ ] Revise docs/quickstart.md - show direct styling first
- [ ] Revise docs/architecture.md - Controls → Styles → Themes
- [ ] Revise docs/theming-guide.md - multiple approaches
- [ ] Update control documentation (4 files) - styling surface primary
- [ ] Add non-token theme example to sample app

### Phase 3: Flagstrap Context (Week 3) - P2

- [ ] Add header to docs/tokens.md noting it's Flagstrap exploration
- [ ] Update docs/token-catalog-system.md with Flagstrap context
- [ ] Update adr009 and adr010 with Flagstrap framing
- [ ] Update implementation-status.md
- [ ] Update roadmap.md

### Phase 4: Tooling Clarification (Week 4) - P3

- [ ] Update TokenGenerator README - tokens are optional implementation detail
- [ ] Update BootstrapConverter README - outputs FlagstoneUI themes
- [ ] Update mcp-bootstrap-converter.md

### Post-Implementation

- [ ] Review for any remaining token-first references
- [ ] Test AI agent guidance with updated docs
- [ ] Community communication (if applicable)

---

## Key Terminology Changes

| Old Term | New Term | Context |
|----------|----------|---------|
| "Token-first design" | "Unified styling plane" | Core value proposition |
| "Design tokens (core)" | "Tokens (optional implementation)" | Token system positioning |
| "Token-based theming" | "Styling" | Theming description |
| "Material baseline" | "Material example" | Theme positioning |
| "Token compliance" | "Styling completeness" | Validation focus |
| "Theme requirements" | "Theme = collection of styles" | Theme definition |

---

## Success Criteria

1. **README.md** frames FlagstoneUI as the unified styling plane
2. **AI agents** generate working code using direct values when appropriate
3. **New users** understand they can style controls directly
4. **Material theme** positioned as Flagstrap example, not requirement
5. **docs/quickstart.md** shows direct styling before mentioning tokens
6. **Sample themes** include at least one non-token theme
7. **Architecture docs** show Controls → Styles → Themes as core

---

## Risks and Mitigations

| Risk | Mitigation |
|------|------------|
| Existing users confused by messaging change | Add notes explaining the clarification |
| Token tooling perceived as deprecated | Clearly position as Flagstrap exploration, not deprecated |
| AI agents still generate token-first code | Test and iterate on agent guidance |
| Bootstrap converter perceived as less useful | Explain it outputs standard FlagstoneUI themes (using Flagstrap tokens internally) |
| Loss of design system guidance | Flagstrap will be developed later with proper framing |

---

## Timeline Estimate

| Phase | Duration | Priority |
|-------|----------|----------|
| Phase 1: Core Messaging | 3-4 days | P0 |
| Phase 2: Documentation | 3-4 days | P1 |
| Phase 3: Flagstrap Context | 2-3 days | P2 |
| Phase 4: Tooling Context | 2-3 days | P3 (defer) |
| **Total (P0-P2)** | **~2 weeks** | - |

---

## Resolved Questions

Based on clarifying discussion, these questions are now resolved:

1. **Is the token system part of FlagstoneUI's architecture?**

   **No.** FlagstoneUI's architecture is: Controls → Styles → Themes.
   Tokens are part of Flagstrap, which is built ON FlagstoneUI but is a separate project.

2. **Is Flagstrap built on FlagstoneUI?**

   **Yes.** Flagstrap is built on FlagstoneUI, like Bootstrap is built on CSS, or DaisyUI on Tailwind.
   However, it's a separate project - not part of FlagstoneUI's core architecture.
   Developers choose whether to use Flagstrap or style FlagstoneUI controls directly.

3. **What is the contract for Flagstrap themes?**

   The well-known style names (like `Style="Primary"`) - similar to how Bootstrap has `btn-primary`.
   Tokens are an implementation detail that makes it easier to create Flagstrap themes.

4. **Should Flagstrap be developed now?**

   **No.** Defer Flagstrap. Focus on making FlagstoneUI core robust first.
   Existing token work is reframed as "Flagstrap exploration."

---

## Open Questions (For Later)

These questions are deferred until Flagstrap development resumes:

1. **Flagstrap naming**: Is "Flagstrap" the right name?

2. **Flagstrap contract**: What style names should be in the contract?
   - `Primary`, `Secondary`, `Outlined`, `Text` for buttons?
   - Similar patterns for other controls?

3. **Package structure**: Should Flagstrap be a separate NuGet package?

4. **Material theme**: Should it remain in the main repo or move to a Flagstrap-specific location?

---

## Appendix: Key Quotes to Update

### README.md
- "Token‑based theming system" → Remove or move to Flagstrap section
- Emphasize: "Unified styling plane for .NET MAUI"

### AGENTS.MD
- "Token-First Design: All visual properties reference semantic design tokens" → **REMOVE**
- "Always use tokens for colors..." → "Use the styling approach appropriate for your project"
- "WRONG: Hardcoded values" → **REMOVE** (direct values are valid)

### copilot-instructions.md
- "Token-first styling" → "Standard .NET MAUI styling"

### docs/quickstart.md
- "token-based styling system" → "flexible styling options"
- Show direct styling example first

### docs/architecture.md
- Remove token system from core architecture diagram
- Add note that Flagstrap is separate/exploratory

---

## Phase 2: Additional Improvements (Future Work)

The following items were identified during implementation but are deferred as they involve code changes or are lower priority:

### Sample App Enhancements

1. **Add Non-Token Theme Example** (Priority 1)
   - Create `samples/FlagstoneUI.SampleApp/Themes/SimpleTheme.xaml`
   - Demonstrate pure FlagstoneUI theme without tokens
   - Use direct values in styles to show this is a valid approach
   - Add documentation explaining different theme approaches

2. **Update Sample XAML Examples**
   - Many sample app pages currently use token references (183 instances found)
   - Consider adding examples showing direct styling alongside token-based styling
   - Demonstrate multiple valid approaches in the showcase pages

### Tooling Documentation

3. **Bootstrap Converter Positioning**
   - The converter outputs FlagstoneUI-compatible themes (currently uses tokens internally)
   - Update examples/documentation to clarify tokens are an implementation detail
   - Show how converter output can be adapted for non-token themes

4. **TokenGenerator Tool Context**
   - Currently positioned for token-based theme development
   - Add documentation clarifying this is for Flagstrap-style themes
   - Consider renaming or repositioning as Flagstrap tooling when developed

### Additional Documentation

5. **Theme Creation Guide**
   - Create comprehensive guide showing theme creation with all approaches
   - Include side-by-side comparison of direct values vs. tokens
   - Provide decision framework for choosing approach

**Note**: These items are logged for future consideration but are not required for the core repositioning to be complete.
