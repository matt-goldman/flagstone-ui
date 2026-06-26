# ADR Addendum: FsShell — Per-`ShellItem` Tab Bar Scoping

**note: this is an addendum to [ADR012 — FsShell: Stylable Shell Chrome via Subclass](./adr012-fsshell.md). It refines the tab-bar projection model described there; it does not change the subclass approach, the LSP guarantee, or the "no consumer-facing platform code" principle, all of which stand unchanged.**

**Status:** Accepted
**Date:** 2026-06-06
**Deciders:** Matt Goldman
**Scope:** FlagstoneUI.Core — V1, all platform renderers

## Context

ADR012 decided to host a FlagstoneUI bar in place of Shell's native bottom chrome, and described the reference `FsTabBar` as rendering *"one instance per `ShellContent`."* The accompanying spec (FsShell V1, §A) went further and recorded two resolutions that, taken literally, define a **single, app-global tab bar**:

- `FsShell.RebuildTabs` enumerates **every** `ShellContent` in the shell and flattens it into one `ItemsSource`.
- `FlyoutItem` children are **skipped** entirely, on the basis that flyout chrome is out of scope.

Building the first Android renderer against that model surfaced a mismatch with how Shell actually works:

- **Stock Shell's bottom bar is per-`ShellItem`, not global.** The native `BottomNavigationView` shows the **sections of the currently active `ShellItem`**, and each `ShellItem` (including each `FlyoutItem`) has its own bar. Switching items via the flyout swaps the whole bar.
- **The flat/global model broke real hierarchies.** A `FlyoutItem` containing multiple `Tab`s (its own bottom tabs) contributed *nothing* to the flattened bar (it was skipped), so suppressing the native bar left that item with **no bottom tabs at all** — a regression against stock. Conversely, several top-level `ShellContent`s (which stock renders as flyout destinations with no shared bottom bar) were incorrectly flattened into one invented bar, and navigating it fought the flyout's job and blanked content on item switches.

In short: the documented model conflated "the app's tab bar" with "the active item's bottom tabs." Stock Shell only ever has the latter. To preserve Shell's full navigation hierarchy — the explicit LSP/drop-in promise of ADR012 — the bar must mirror the active item's sections, exactly as the native bar does.

## Decision

The FlagstoneUI tab bar is **scoped to the active `ShellItem`** and mirrors that item's `ShellSection`s, identically to Shell's native bottom bar.

Concretely, at the cross-platform layer (`FsShell`):

- `RebuildTabs` projects **`CurrentItem.Items`** (the active item's sections) into `FsTabContext`s — not a global enumeration of all `ShellContent`, and with no special-casing of `FlyoutItem`. It **re-projects when the active item changes** (e.g. via the flyout).
- Selecting a tab activates that section directly: **`CurrentItem.CurrentItem = section`** (matching stock's section change; no reliance on a resolvable absolute route). Selection state is tracked by section identity.
- The bar **auto-hides when the active item has fewer than two sections**, matching stock's "no bottom bar for a single section."

This holds uniformly for both common structures:

- A **`<TabBar>`** — one `ShellItem` whose sections are the tabs.
- A **`<FlyoutItem>` with multiple `Tab`/section children** — its sections are that item's bottom tabs; the stock flyout drawer switches between items, and the stock top-tab strip still renders for any section that has multiple `ShellContent`s.

## Cross-platform invariant

Every platform renderer (iOS, Android, MacCatalyst, Windows) **must honour the same scoping**:

- Suppress the native bottom chrome and host the FlagstoneUI bar **only for items that have more than one section** — i.e. only where stock would draw a bottom bar. Items with a single section are left entirely on stock rendering.
- The bar reflects the active item's projected sections and is re-hosted/refreshed across item switches.

The Android implementation hosts the bar into the active `ShellItem` fragment's container and releases it on teardown so it survives re-hosting. The other platforms differ in mechanism but not in this contract.

## Relationship to ADR012

- **Corrects** ADR012's phrasing "one instance per `ShellContent`" and the spec §A resolutions that flattened all content and skipped `FlyoutItem`s. The correct unit of projection is the active item's `ShellSection`, scoped per `ShellItem`.
- **Preserves unchanged**: the subclass approach, the LSP/drop-in guarantee (in fact this strengthens it — flyout-plus-tabs apps now behave as they did under stock Shell), no consumer-facing platform code, the XAML-discoverable `TabBarItemTemplate` / `TabBar` surface, and the deferral of flyout-chrome and top-tab-strip *visual* replacement.

## Out of scope for V1 (deferred, not rejected)

- **A *distinct* custom tab bar instance or template per `ShellItem`.** `FsShell.TabBar` (a single `ContentView`) and `TabBarItemTemplate` apply one bar identity app-wide; the bar is *scoped* per item but not *varied* per item. Most apps want a single, consistent tab bar and do not mix flyout and tab-bar navigation the way Shell permits, so this carries little value for its cost. Supporting it would require turning `TabBar` into a template/factory — a public-API change reserved for a post-V1 release. The default `FsTabBar` and a single consumer-supplied custom bar both already work, scoped per item.

## Consequences

### Positive

- Shell's full navigation hierarchy is preserved: flyout drawer, per-item bottom tabs, and the top-tab strip coexist, with only the bottom bar restyled. Drop-in compatibility now holds for flyout-based apps, not just single-`<TabBar>` apps.
- Bar semantics match stock Shell (per-item sections, single-section items show no bar), so migration surprises are minimised.

### Negative / accepted

- The renderer manages a single shared bar instance across item fragments, which requires careful host/release lifecycle (guarded re-parenting on teardown). Accepted as contained platform-internal complexity.
- Per-item bar *customisation* is unavailable in V1 (see Out of scope).

## Verification

Validated on an Android emulator with a shell exercising all three levels at once: a `FlyoutItem` ("Dashboard") with two `Tab` sections (styled bottom bar), one of which has two `ShellContent`s (stock top-tab strip), plus a single-section `FlyoutItem` ("Profile", stock, no bottom bar), with the stock flyout drawer switching between them and round-trips re-hosting the bar correctly. The single-`<TabBar>` sample `AppShell` continues to work unchanged.

## References

- [ADR012 — FsShell: Stylable Shell Chrome via Subclass](./adr012-fsshell.md)
- FsShell V1 spec, §A (per-`ShellItem` bar scoping) and §B (Android renderer).
