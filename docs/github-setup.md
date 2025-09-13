# GitHub Setup

This file contains notes on setting up the repo and organisation to support this project.

## Labels

Create these to triage quickly:

* area:core, area:themes, area:blocks, area:docs
* good first issue, help wanted
* type:bug, type:feature, type:design, type:accessibility
* platform:android, platform:ios, platform:maccatalyst, platform:windows
* priority:high|med|low

## Milestones and initial issues

### Milestone 0.1.0 (MVP)

* Token system scaffold (Tokens.xaml)
* Neutral Button style + handler normalisation
* Neutral Entry style + validation states
* Card control (surface, elevation, radius)
* Snackbar (overlay + VSM)
* Theme loader + Material theme (light/dark)
* Sample app: control gallery and theme switch
* Docs: Quickstart, theme how-to, token reference
* CI: build, test, pack
* Release: tag and publish to NuGet

### Milestone 0.2.0 (Core primitives)

* FormField wrapper (label/helper/error)
* Switch, CheckBox, RadioButton (group)
* Picker, Slider, Rating
* Badge, Divider, ProgressBar, Avatar
* Dialog (modal), Toast
* Accessibility pass and guidelines doc
* Modern theme v1

### Milestone 0.3.0 (Blocks)

* Auth screens (sign in/up/forgot)
* CRUD master–detail block
* Settings page template
* Skeleton screens
* Gallery pages with data-bound samples

## Contribution guide highlights

`CONTRIBUTING.md`

* Use Conventional Commits (feat:, fix:, docs:, refactor:, test:, chore:)
* Coding style: .editorconfig enforced, nullable on, warnings as errors
* Tests required for controls and theme resources where possible
* Visual diffs: include screenshots or short clips for UI changes
* Accessibility: ensure focus visuals, semantics, contrast tokens
* Theme additions must use tokens only, no hard-coded colours

## Security and governance

* SECURITY.md with contact for vulnerability reporting
* MIT licence
* Contributor Covenant code of conduct

## Docs site

Build with Blake.

Keep simple first:

* docs/ Markdown, published via GitHub Pages
* Pages: Getting started, Tokens, Theming, Controls, Blocks, Roadmap
* Later you can switch to DocFX or your Blake SSG if you like

## Ready-to-create tasks (copy into issues)

1. Scaffold repo, licence, code of conduct
1. Add solution, projects, props/targets, editorconfig
1. Implement Tokens.xaml with minimal set
1. Implement Button neutral style + handler fixes
1. Implement Entry neutral style + error states
1. Implement Card control
1. Implement Snackbar overlay service
1. Material theme (light/dark) consuming tokens
1. Sample app with theme toggle and controls gallery
1. CI pipeline (build, test, pack) and Release pipeline (tag publish)
1. Docs: Quickstart + Theming guide
1. Accessibility checklist doc