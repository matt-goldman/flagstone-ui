---
description: Find and understand MAUI's platform-specific source code for any control. Use when customizing, extending, or debugging platform-level behavior of MAUI controls (Shell, handlers, renderers, platform views).
---

# MAUI Platform Source Lookup

When you need to understand how a MAUI control works on a specific platform, follow this procedure to find the right source files in the `dotnet/maui` repo.

## Step 1: Determine handler vs renderer

MAUI has two rendering architectures that coexist. **You must determine which one the platform uses for the control in question** before looking at source code.

| Architecture | Namespace | Used by |
|---|---|---|
| **Handlers** (modern) | `Microsoft.Maui.Controls.Handlers` | Windows Shell, most non-Shell controls on all platforms |
| **Compatibility Renderers** (legacy) | `Microsoft.Maui.Controls.Handlers.Compatibility` | iOS Shell, Android Shell, MacCatalyst Shell |

**Critical:** A single control can use handlers on one platform and renderers on another. Shell is the prime example — Windows uses `ShellHandler`, while iOS/Android/Mac use the compatibility `ShellRenderer`. Never assume all platforms use the same architecture.

To check which architecture a platform uses for a given control:
1. Look for files in both locations (see Step 2)
2. If only one location has platform-specific files, that's the architecture
3. If both exist, check which one is registered in the handler/renderer collection

## Step 2: Locate the source files

Base URL: `https://raw.githubusercontent.com/dotnet/maui/refs/heads/main/src/Controls/src/Core/`

### Handlers (modern architecture)

```
Handlers/{ControlName}/                          # Shared handler
Handlers/{ControlName}/{ControlName}Handler.cs   # Cross-platform handler
Handlers/{ControlName}/{ControlName}Handler.{Platform}.cs  # Platform partial
Handlers/{ControlName}/{Platform}/               # Platform-specific helpers
```

For Shell specifically:
- Handler: `Handlers/Shell/ShellHandler.Windows.cs`
- Platform views: `Handlers/Shell/Windows/ShellView.cs`, `ShellSplitView.cs`, etc.
- Item handler: `Handlers/Shell/ShellItemHandler.Windows.cs`
- Section handler: `Handlers/Shell/ShellSectionHandler.Windows.cs`

### Compatibility Renderers (legacy architecture)

```
Compatibility/Handlers/{ControlName}/
Compatibility/Handlers/{ControlName}/{Platform}/
```

For Shell specifically:
- iOS: `Compatibility/Handlers/Shell/iOS/ShellRenderer.cs`
- Android: `Compatibility/Handlers/Shell/Android/ShellRenderer.cs`
- **No Windows directory exists** — Windows Shell never used renderers

### Platform infrastructure (MauiNavigationView, etc.)

```
# These are in the Core project, not Controls:
https://raw.githubusercontent.com/dotnet/maui/refs/heads/main/src/Core/src/Platform/{Platform}/
```

Key Windows files:
- `src/Core/src/Platform/Windows/MauiNavigationView.cs` — base class for Shell views
- `src/Core/src/Platform/Windows/RootNavigationView.cs` — Shell's outer NavigationView

## Step 3: Fetch and read the source

Use `WebFetch` with the raw GitHub URL:

```
https://raw.githubusercontent.com/dotnet/maui/refs/heads/main/src/Controls/src/Core/{path}
```

To browse a directory listing first:
```
https://github.com/dotnet/maui/tree/main/src/Controls/src/Core/{path}
```

## Step 4: Map the class hierarchy

For each platform, trace the inheritance chain and identify:

1. **What creates what** — e.g., `ShellHandler.CreatePlatformView()` creates a `ShellView`
2. **What virtual methods exist** — these are your extension points
3. **What internal state is managed** — fields you can't access but need to work around
4. **What mapper entries exist** — property mappings that drive platform updates (e.g., `MapCurrentItem`)

## Shell architecture cheat sheet

### Windows (Handler)
```
ShellHandler : ViewHandler<Shell, ShellView>
  └── creates ShellView (extends RootNavigationView → MauiNavigationView → NavigationView)
        └── ShellView.SwitchShellItem() creates ShellItemHandler
              └── ShellItemHandler.PlatformView = MauiNavigationView (PaneDisplayMode.Top)
                    ├── TopNavArea (StackPanel) — native tab strip
                    ├── ContentGrid (Grid) — page content area
                    └── Content = ShellSectionHandler.PlatformView
```

### iOS (Compatibility Renderer)
```
ShellRenderer : UIViewController
  └── CreateShellItemRenderer() → ShellItemRenderer : UITabBarController
        ├── UITabBar — native tab bar (suppress via TabBar.Hidden = true)
        └── View — host FsTabBar as subview
```

### Android (Compatibility Renderer)
```
ShellRenderer(Context) : Fragment
  └── CreateShellItemRenderer() → ShellItemRenderer : Fragment
        └── OnCreateView() returns LinearLayout
              ├── NavigationTarget (content) — weight 1
              └── BottomNavigationView — native tabs (suppress via Visibility.Gone)
```
