using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;

namespace FlagstoneUI.Core.Controls;

/// <summary>
/// iOS-specific Shell renderer for <see cref="FsShell"/>. Suppresses the native
/// <c>UITabBar</c> and hosts an <see cref="FsTabBar"/> in its place.
/// </summary>
/// <remarks>
/// Bootstrap stub: the suppression of the native tab bar and hosting of the
/// FlagstoneUI bar at the platform layer is a TODO for the FsShell V1 implementation.
/// See docs/archive/spec-fsshell.md.
/// </remarks>
internal sealed partial class FsShellRenderer : ShellRenderer
{
	// TODO (FsShell V1): override CreateShellItemRenderer / CreateShellSectionRenderer to
	// suppress the native UITabBar and project FsTabBar contexts into the FlagstoneUI bar.
	// Honour:
	//   - Shell.SetTabBarIsVisible(page, false) bridging.
	//   - Safe area inset (home indicator).
	//   - Keyboard avoidance (slide bar off-screen).
	//   - Modal presentation (hide bar with chrome).
}
