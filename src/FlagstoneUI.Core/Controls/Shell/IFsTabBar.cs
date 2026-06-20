namespace FlagstoneUI.Core.Controls;

/// <summary>
/// Minimal contract a custom tab bar may implement to participate as the bar slot of an
/// <see cref="FsShell"/>. Implementing this interface is the explicit form of the bar
/// replacement contract; views that do not implement it fall back to convention-based
/// binding lookup (a bindable property named <c>ItemsSource</c> and either
/// <c>SelectedRoute</c> or an <c>ItemSelected</c> event).
/// </summary>
public interface IFsTabBar
{
	/// <summary>The collection of tabs to render. Set by <see cref="FsShell"/>.</summary>
	IReadOnlyList<FsTabContext> ItemsSource { get; set; }

	/// <summary>
	/// The route of the currently selected tab. Two-way: assignment by <see cref="FsShell"/>
	/// reflects external navigation back into the bar; assignment by the bar (in response to a
	/// user tap) requests navigation to the new route.
	/// </summary>
	string? SelectedRoute { get; set; }

	/// <summary>Raised when the user selects a tab in the bar.</summary>
	event EventHandler<FsTabBarSelectionChangedEventArgs>? ItemSelected;
}

/// <summary>Event data for <see cref="IFsTabBar.ItemSelected"/>.</summary>
public sealed class FsTabBarSelectionChangedEventArgs : EventArgs
{
	/// <summary>Initializes a new <see cref="FsTabBarSelectionChangedEventArgs"/>.</summary>
	public FsTabBarSelectionChangedEventArgs(FsTabContext selected)
	{
		Selected = selected ?? throw new ArgumentNullException(nameof(selected));
	}

	/// <summary>The tab the user selected.</summary>
	public FsTabContext Selected { get; }
}
