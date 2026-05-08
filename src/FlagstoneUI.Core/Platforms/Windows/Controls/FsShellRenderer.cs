using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;

namespace FlagstoneUI.Core.Controls;

/// <summary>
/// Windows-specific Shell renderer for <see cref="FsShell"/>. Suppresses or replaces the WinUI
/// <c>NavigationView</c>-based chrome that stock Shell produces and hosts an
/// <see cref="FsTabBar"/> as a bottom bar.
/// </summary>
/// <remarks>
/// Bootstrap stub: V1 implementation pending. See docs/archive/spec-fsshell.md.
/// </remarks>
internal sealed partial class FsShellRenderer : ShellRenderer
{
	// TODO (FsShell V1): suppress / replace the WinUI NavigationView chrome and host an
	// FsTabBar at the bottom. Note: this is a different platform path from
	// iOS / Android / MacCatalyst.
}
