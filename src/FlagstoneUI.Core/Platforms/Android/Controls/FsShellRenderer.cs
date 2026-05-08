using Android.Content;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;

namespace FlagstoneUI.Core.Controls;

/// <summary>
/// Android-specific Shell renderer for <see cref="FsShell"/>. Suppresses the native
/// <c>BottomNavigationView</c> and hosts an <see cref="FsTabBar"/> in its place.
/// </summary>
/// <remarks>
/// Bootstrap stub: V1 implementation pending. See docs/archive/spec-fsshell.md.
/// </remarks>
internal sealed partial class FsShellRenderer : ShellRenderer
{
	public FsShellRenderer(Context context) : base(context)
	{
	}

	// TODO (FsShell V1): override CreateBottomNavViewAppearanceTracker / fragment factories
	// to suppress BottomNavigationView and project FsTabBar contexts into the FlagstoneUI bar.
	// Honour:
	//   - Shell.SetTabBarIsVisible(page, false) bridging.
	//   - Keyboard avoidance.
	//   - Modal presentation.
}
