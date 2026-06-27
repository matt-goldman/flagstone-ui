# Example Apps

Real apps built with FlagstoneUI. Each lives in its own repository — clone, build, and explore. New to `FsShell`? Walk through the [Your First Shell App](getting-started/your-first-shell-app.md) tutorial first — it's built from the same patterns these apps use.

## MyContoso — building a real app with FlagstoneUI

A contrived internal company app that shows broadly how to structure and build an application with FlagstoneUI's controls: shared vs. isolated state, navigation, domain ownership, and cross-cutting concerns. Created to support a talk at [MAUI Day](https://mauiday.net/) — work through the `sprint-1`, `sprint-2`, and `sprint-3` branches in order to follow the architectural progression.

**What it demonstrates:** general app composition with FlagstoneUI controls, navigation, and app architecture.

🔗 **[github.com/matt-goldman/MyContoso](https://github.com/matt-goldman/MyContoso)**

## instagrim — FsShell with a custom tab bar

A Halloween-themed social photos app built around `FsShell`. The shell uses an undocked (floating) tab bar with a fully custom `NavBar` control, including a centre action button using the reserved-slot pattern.

**What it demonstrates:** `FsShell` with a replaceable tab bar, a floating/undocked bar, and a custom centre action button.

<table>
    <tr>
        <td><img src="https://raw.githubusercontent.com/matt-goldman/instagrim/refs/heads/main/assets/feed.png" width=400></td>
        <td><img src="https://raw.githubusercontent.com/matt-goldman/instagrim/refs/heads/main/assets/discover.png" width=400></td>
        <td><img src="https://raw.githubusercontent.com/matt-goldman/instagrim/refs/heads/main/assets/hauntings.png" width=400></td>
    </tr>
</table>

🔗 **[github.com/matt-goldman/instagrim](https://github.com/matt-goldman/instagrim)**

## Beer Driven Devs — FsShell in a media app

The (very unofficial) companion app for the [Beer Driven Devs](https://www.beerdriven.dev) podcast. Built on `FsShell`, with episode browsing, offline downloads, and a series of progressive download UX microinteractions explored across branches (`level-0` through `main`).

**What it demonstrates:** `FsShell` navigation in a content/media app, plus custom microinteractions. Companion to a [MAUI UI July](https://goforgoldman.com/posts/bdd-app-downloads/) blog post.

<table>
    <tr>
        <td><img src="https://raw.githubusercontent.com/matt-goldman/BeerDrivenDevsApp/refs/heads/main/assets/nav-open.png" width=400></td>
        <td><img src="https://raw.githubusercontent.com/matt-goldman/BeerDrivenDevsApp/refs/heads/main/assets/nav-closed.png" width=400></td>
        <td><img src="https://raw.githubusercontent.com/matt-goldman/BeerDrivenDevsApp/refs/heads/main/assets/community-nav-open.png" width=400></td>
    </tr>
</table>

🔗 **[github.com/matt-goldman/BeerDrivenDevsApp](https://github.com/matt-goldman/BeerDrivenDevsApp)**

---

Want to see all the controls and themes in one place without leaving this repo? Run the in-repo samples:

- **[FlagstoneUI.SampleApp](../samples/FlagstoneUI.SampleApp/)** — showcase of every control and theme.
- **[FlagstoneUI.ThemePlayground](../samples/FlagstoneUI.ThemePlayground/)** — experiment with custom themes.
