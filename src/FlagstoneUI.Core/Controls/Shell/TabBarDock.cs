namespace FlagstoneUI.Core.Controls;

/// <summary>
/// Specifies where <see cref="FsShell"/> anchors the hosted tab bar relative to the shell's
/// content area. The per-platform renderer reads this value to position the bar and publish the
/// corresponding chrome-dimension resource.
/// </summary>
public enum TabBarDock
{
	/// <summary>Bar pinned to the bottom edge. This is the default and matches stock Shell behaviour.</summary>
	Bottom = 0,

	/// <summary>Bar pinned to the top edge.</summary>
	Top,

	/// <summary>Bar pinned to the left edge (side rail).</summary>
	Left,

	/// <summary>Bar pinned to the right edge (side rail).</summary>
	Right,

	/// <summary>
	/// Native chrome is suppressed and the bar is hosted as a full-bounds overlay with no
	/// renderer-imposed positioning. The consumer controls placement via standard MAUI layout
	/// properties on the bar (<c>HorizontalOptions</c>, <c>VerticalOptions</c>, <c>Margin</c>,
	/// etc.) — suitable for floating FABs, radial menus, or any non-edge-anchored design.
	/// </summary>
	None
}
