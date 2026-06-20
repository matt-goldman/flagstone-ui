namespace FlagstoneUI.Core.Controls;

/// <summary>
/// Shell renderer for <see cref="FsShell"/>. Suppresses the platform's native tab bar chrome
/// (UITabBar, BottomNavigationView, NavigationView) and hosts an <see cref="FsTabBar"/> in
/// its place. The platform-specific implementation lives in the per-platform partial under
/// <c>Platforms/*/Controls/FsShellRenderer.cs</c>.
/// </summary>
/// <remarks>
/// On the shared <c>net10.0</c> TFM this type is an empty stub so that
/// <c>typeof(FsShellRenderer)</c> can be referenced from cross-platform code (notably the
/// <c>MauiAppBuilder.UseFlagstoneUI</c> registration). It is never instantiated on that TFM.
/// </remarks>
internal sealed partial class FsShellRenderer
{
}
