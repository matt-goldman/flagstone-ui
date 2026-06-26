# FlagstoneUI Documentation

FlagstoneUI is a unified styling plane for .NET MAUI — enhanced, neutral controls that give you full visual control from shared code, with the same behaviour on every platform. No renderers, no handlers, no platform quirks.

> 👈 **New here?** Start with the [project README](../README.md) for the why, then jump into the [Quickstart](getting-started/quickstart.md).

## Get Started

- **[Quickstart](getting-started/quickstart.md)** ⭐ — Install from NuGet, add your first controls, apply a theme.
- **[Your First Shell App](getting-started/your-first-shell-app.md)** 🧭 — Build a tabbed app with `FsShell`, from the built-in bar to a fully custom navigation bar.
- **[Architecture Overview](getting-started/architecture.md)** — How the controls, styling, and themes fit together.

## Controls

The core control set. Each page covers what the control is, common recipes, and its styling properties.

- **[FsShell](controls/FsShell.md)** 🧭 — `Shell` subclass with a replaceable tab bar and configurable dock position (bottom, top, left, right, none, or floating). The foundation for app navigation.
- **[FsTabBar](controls/FsTabBar.md)** — Default tab bar for FsShell, with sliding pill animation and per-tab templates.
- **[FsButton](controls/FsButton.md)** — Fully stylable button.
- **[FsEntry](controls/FsEntry.md)** — Single-line text entry with full border/shape control.
- **[FsEditor](controls/FsEditor.md)** — Multi-line text editor.
- **[FsCard](controls/FsCard.md)** — Card container.
- **[FsBorder](controls/FsBorder.md)** — Border with per-edge control.

## Theming

- **[Theming Guide](guides/theming-guide.md)** 🎨 — Style controls inline, with implicit/explicit styles, or with a full token-based theme.
- **[Design Tokens](reference/tokens.md)** — Colour, spacing, typography, and shape tokens you can bind to.

## Integrations

- **[MAUI Community Toolkit](integrations/mct-integrations.md)** — Optional `FlagstoneUI.Integrations.MCT` package (validation adapter, animated editor border).

## Examples

- **[Example Apps](examples.md)** — Real apps built with FlagstoneUI, showing the controls and FsShell in practice.

## For Contributors

Building or extending FlagstoneUI itself? See **[Contributing](contributing/)** — control implementation standards, the visual-state pattern, and the testing guides.

## Archive

Earlier design notes, ADRs, and tooling docs (theme generators, the Bootstrap converter, the machine-readable token catalog) live under **[archive/](archive/)**. They're kept for history; the converter and theme tooling now live in their own repositories and aren't part of the core library.

---

*FlagstoneUI — A unified styling plane for .NET MAUI*
