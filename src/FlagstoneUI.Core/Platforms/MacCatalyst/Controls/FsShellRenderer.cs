using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;

namespace FlagstoneUI.Core.Controls;

/// <summary>
/// MacCatalyst-specific Shell renderer for <see cref="FsShell"/>. Suppresses the native
/// <c>UITabBar</c> and hosts an <see cref="FsTabBar"/> in its place.
/// </summary>
/// <remarks>
/// Bootstrap stub: V1 implementation pending. See docs/archive/spec-fsshell.md.
/// </remarks>
internal sealed partial class FsShellRenderer : ShellRenderer
{
	// TODO (FsShell V1): equivalent treatment to iOS, with whatever differences from iOS prove
	// necessary on MacCatalyst.
}
