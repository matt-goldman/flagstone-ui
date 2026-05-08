using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FlagstoneUI.Core.Controls;

/// <summary>
/// Per-tab data presented as <see cref="BindableObject.BindingContext"/> to an
/// <see cref="FsShell.TabBarItemTemplate"/> instance.
/// </summary>
/// <remarks>
/// <para>
/// One <see cref="FsTabContext"/> is created by <see cref="FsShell"/> for every
/// <see cref="ShellContent"/> in its visual tree. The context exposes the metadata a tab
/// template typically needs (<see cref="Title"/>, <see cref="Icon"/>, <see cref="Route"/>)
/// plus observable state (<see cref="IsSelected"/>, <see cref="IsEnabled"/>) that templates
/// can bind to in order to drive selected-state visuals.
/// </para>
/// <para>
/// <see cref="FsTabContext"/> is deliberately distinguished from any future tab <em>control</em>
/// type: the name communicates "context for a tab", not "tab control".
/// </para>
/// </remarks>
public sealed class FsTabContext : INotifyPropertyChanged
{
	private bool _isSelected;
	private bool _isEnabled = true;
	private string? _title;
	private ImageSource? _icon;

	/// <summary>
	/// Initializes a new <see cref="FsTabContext"/> for the given Shell route.
	/// </summary>
	/// <param name="route">The Shell route this tab represents. Must not be null.</param>
	public FsTabContext(string route)
	{
		Route = route ?? throw new ArgumentNullException(nameof(route));
	}

	/// <summary>The Shell route this tab represents.</summary>
	public string Route { get; }

	/// <summary>The tab's title, sourced from <c>Shell.Title</c> on the underlying <see cref="ShellContent"/>.</summary>
	public string? Title
	{
		get => _title;
		set => SetField(ref _title, value);
	}

	/// <summary>The tab's icon, sourced from <c>Shell.Icon</c> on the underlying <see cref="ShellContent"/>.</summary>
	public ImageSource? Icon
	{
		get => _icon;
		set => SetField(ref _icon, value);
	}

	/// <summary>Whether this tab is currently the selected tab.</summary>
	public bool IsSelected
	{
		get => _isSelected;
		set => SetField(ref _isSelected, value);
	}

	/// <summary>Whether this tab is enabled. Reserved for future "disabled tab" support.</summary>
	public bool IsEnabled
	{
		get => _isEnabled;
		set => SetField(ref _isEnabled, value);
	}

	/// <inheritdoc />
	public event PropertyChangedEventHandler? PropertyChanged;

	private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
	{
		if (EqualityComparer<T>.Default.Equals(field, value))
		{
			return;
		}

		field = value;
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName!));
	}
}
