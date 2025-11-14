# Flagstone UI Spec & Project Plan (Historical Draft)

> **⚠️ HISTORICAL DOCUMENT**  
> This document contains the original project specification and planning draft.  
> **For current information**, see:
> - [architecture.md](architecture.md) - Current technical architecture
> - [implementation-status.md](implementation-status.md) - Current progress tracking
> - [roadmap.md](roadmap.md) - Updated roadmap and future plans
> - [control-implementation-guide.md](control-implementation-guide.md) - Control implementation standards

---

## Original Vision

Deliver an open-source, themeable UI kit for .NET MAUI apps that:

* Provides neutral, cross-platform controls with native styling stripped out.
* Applies themes purely at the MAUI layer (via Theme.xaml + design tokens).
* Ships with starter themes (Material + Tailwind-inspired) and a builder extension (UseFlagstoneUI) for easy adoption.
* Encourages community-contributed themes and blocks.

## Architecture

### Core principles

* Design tokens first: Colors, typography, radii, spacing, elevation, motion.
* Implicit styles + ControlTemplates + VisualStateManager for theme application.
* Handlers only where MAUI platform quirks require neutralisation.
* Theme switching via merged dictionaries or builder API.

### API surface

```csharp
builder.UseFlagstoneUI(ui =>
{
    ui.Theme("Themes/Material.xaml");
    ui.Density = Density.Compact;
    ui.Motion = Motion.Standard;
});
```

### Packages

* FlagstoneUI.Core – base controls, tokens, builder.
* FlagstoneUI.Themes.Material – Material theme.
* FlagstoneUI.Themes.Modern – Tailwind-inspired theme.
* FlagstoneUI.Blocks – optional blocks/templates.

## Roadmap

### Phase 1: Proof of Concept (MVP)

#### Scope

* Controls: Button, Entry, Card, Snackbar, Switch
* Theme system with Material light/dark.
* Builder extension method (UseFlagstoneUI).
* Docs: quickstart + “build your own theme” guide.
#### Deliverables

* GitHub repo with MIT license.
* CI/CD pipeline (build + NuGet publish).
* Sample app: switch between Material light/dark.

### Phase 2: Core Control Set

#### Scope

Add essential primitives:

* Forms: Editor, CheckBox, RadioButton, Picker, Slider, Rating, FormField.
* Display: Badge, Avatar, ProgressBar, Divider, Tooltip.
* Navigation: AppBar, TabBar, Drawer.
* Feedback: Dialog, Toast/Snackbar.

#### Deliverables

* Modern theme added.
* Expanded docs with accessibility guidance.
* Contribution guide for themes.

### Phase 3: Blocks

#### Scope

* Reusable page/section templates:
* Auth: sign in/up, onboarding carousel.
* CRUD: list + detail, create/edit form.
* App chrome: top bar + bottom nav, settings page.
* Commerce/content: product card grid, pricing comparison, testimonials.
* Feedback: skeleton loaders, error/retry pages.

#### Deliverables

* Blocks package.
* Samples showing full pages built with blocks.

### Phase 4: Advanced Theming & Community

#### Scope

* Animations/motion system.
* Theme density (compact, default, spacious).
* Community gallery of contributed themes.
* Theming guidelines & starter template repo.

#### Non-Goals

* No layout system (MAUI already strong).
* No giant data controls (grids, schedulers).
* No utility classes à la Tailwind (focus on tokens).

#### Risks & Mitigations

* Overlapping with MAUI Toolkit → Build on it, don’t duplicate.
* Platform quirks → Abstract with handlers early.
* Theming complexity → Keep tokens limited and consistent.
* #### Scope creep → Stick to roadmap phases.
