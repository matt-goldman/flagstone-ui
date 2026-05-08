using System.Collections.ObjectModel;
using System.ComponentModel;

namespace FlagstoneUI.Core.Controls;

/// <summary>
/// A drop-in subclass of <see cref="Shell"/> that allows the bottom tab bar to be fully styled
/// in shared XAML/C# without consumer-facing platform code, while preserving Shell's routing,
/// navigation, and lifecycle semantics unchanged.
/// </summary>
/// <remarks>
/// <para>
/// Two layers of customisation are exposed through one extension point:
/// </para>
/// <list type="number">
///   <item>
///     Default tab bar with a custom item template via <see cref="TabBarItemTemplate"/>. The
///     reference <see cref="FsTabBar"/> is used as the bar; it instantiates the template once
///     per tab and binds it to an <see cref="FsTabContext"/>.
///   </item>
///   <item>
///     Replace the bar entirely via <see cref="TabBar"/>. The supplied view is expected to
///     honour the bar contract (see <see cref="IFsTabBar"/>).
///   </item>
/// </list>
/// <para>
/// The platform-level work required to suppress Shell's native chrome and host the
/// FlagstoneUI bar lives inside the library, behind this type. Consumers do not write
/// platform-conditional code or register handlers/renderers themselves.
/// </para>
/// </remarks>
public partial class FsShell : Shell
{
	private readonly ObservableCollection<FsTabContext> _tabs = new();
	private CancellationTokenSource? _transitionCts;
	private int _previousIndex = -1;

	/// <summary>
	/// Initializes a new <see cref="FsShell"/>.
	/// </summary>
	public FsShell()
	{
		Navigated += OnShellNavigated;
	}

	/// <summary>
	/// Live collection of contexts for every <see cref="ShellContent"/> in this shell. Updated
	/// as the visual tree changes. Exposed to the bar slot via <see cref="IFsTabBar.ItemsSource"/>.
	/// </summary>
	public IReadOnlyList<FsTabContext> Tabs => _tabs;

	#region TabBarItemTemplate

	/// <summary>Bindable property for <see cref="TabBarItemTemplate"/>.</summary>
	public static readonly BindableProperty TabBarItemTemplateProperty = BindableProperty.Create(
		nameof(TabBarItemTemplate),
		typeof(DataTemplate),
		typeof(FsShell),
		propertyChanged: OnTabBarItemTemplateChanged);

	/// <summary>
	/// Template applied to each tab in the default <see cref="FsTabBar"/>. Receives an
	/// <see cref="FsTabContext"/> as <see cref="BindableObject.BindingContext"/>. If null, the
	/// default item template that ships with the library is used.
	/// </summary>
	public DataTemplate? TabBarItemTemplate
	{
		get => (DataTemplate?)GetValue(TabBarItemTemplateProperty);
		set => SetValue(TabBarItemTemplateProperty, value);
	}

	private static void OnTabBarItemTemplateChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is FsShell shell && shell.TabBar is FsTabBar defaultBar)
		{
			defaultBar.ItemTemplate = (DataTemplate?)newValue;
		}
	}

	#endregion

	#region TabBar

	/// <summary>Bindable property for <see cref="TabBar"/>.</summary>
	public static readonly BindableProperty TabBarProperty = BindableProperty.Create(
		nameof(TabBar),
		typeof(ContentView),
		typeof(FsShell),
		propertyChanged: OnTabBarChanged);

	/// <summary>
	/// Optional. Replaces the entire bar with a consumer-supplied <see cref="ContentView"/>.
	/// When set, <see cref="TabBarItemTemplate"/> is ignored and a debug-level warning is logged.
	/// The supplied view is expected to honour the bar contract (see <see cref="IFsTabBar"/>).
	/// </summary>
	public ContentView? TabBar
	{
		get => (ContentView?)GetValue(TabBarProperty);
		set => SetValue(TabBarProperty, value);
	}

	private static void OnTabBarChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is not FsShell shell)
		{
			return;
		}

		if (oldValue is IFsTabBar oldBar)
		{
			oldBar.ItemSelected -= shell.OnBarItemSelected;
		}

		if (newValue is ContentView newView)
		{
			shell.AttachBar(newView);

			if (shell.TabBarItemTemplate is not null && newValue is not FsTabBar)
			{
				System.Diagnostics.Debug.WriteLine(
					"[FsShell] TabBarItemTemplate is set but a custom TabBar replaces the default bar; the template will be ignored.");
			}
		}
	}

	#endregion

	#region TabTransitionAnimator

	/// <summary>Bindable property for <see cref="TabTransitionAnimator"/>.</summary>
	public static readonly BindableProperty TabTransitionAnimatorProperty = BindableProperty.Create(
		nameof(TabTransitionAnimator),
		typeof(ITabTransitionAnimator),
		typeof(FsShell));

	/// <summary>
	/// Optional. Invoked on tab selection changes to drive a transition between the outgoing and
	/// incoming tab content. If null, content swaps instantly (current Shell behaviour).
	/// </summary>
	public ITabTransitionAnimator? TabTransitionAnimator
	{
		get => (ITabTransitionAnimator?)GetValue(TabTransitionAnimatorProperty);
		set => SetValue(TabTransitionAnimatorProperty, value);
	}

	#endregion

	/// <inheritdoc />
	protected override void OnParentSet()
	{
		base.OnParentSet();
		RebuildTabs();
		EnsureBar();
	}

	/// <inheritdoc />
	protected override void OnChildAdded(Element child)
	{
		base.OnChildAdded(child);
		RebuildTabs();
	}

	/// <inheritdoc />
	protected override void OnChildRemoved(Element child, int oldLogicalIndex)
	{
		base.OnChildRemoved(child, oldLogicalIndex);
		RebuildTabs();
	}

	private void EnsureBar()
	{
		if (TabBar is null)
		{
			TabBar = new FsTabBar { ItemTemplate = TabBarItemTemplate };
		}
		else
		{
			AttachBar(TabBar);
		}
	}

	private void AttachBar(ContentView bar)
	{
		if (bar is IFsTabBar tabBar)
		{
			tabBar.ItemsSource = _tabs;
			tabBar.ItemSelected -= OnBarItemSelected;
			tabBar.ItemSelected += OnBarItemSelected;
			tabBar.SelectedRoute = CurrentRoute();
		}
	}

	private void RebuildTabs()
	{
		_tabs.Clear();

		foreach (var item in this.Items)
		{
			foreach (var section in item.Items)
			{
				foreach (var content in section.Items)
				{
					var route = !string.IsNullOrEmpty(content.Route) ? content.Route : item.Route;
					var ctx = new FsTabContext(route)
					{
						Title = Shell.GetTabBarIsVisible(content) ? null : null,
					};

					ctx.Title = (content.Title ?? section.Title ?? item.Title) ?? route;
					ctx.Icon = content.Icon ?? section.Icon ?? item.Icon;

					_tabs.Add(ctx);
				}
			}
		}

		// Keep bar in sync if attached.
		if (TabBar is IFsTabBar bar)
		{
			bar.ItemsSource = _tabs;
			bar.SelectedRoute = CurrentRoute();
		}

		UpdateSelectedFlags();
	}

	private string? CurrentRoute()
	{
		var current = CurrentItem?.CurrentItem?.CurrentItem;
		return current?.Route;
	}

	private void UpdateSelectedFlags()
	{
		var route = CurrentRoute();
		var newIndex = -1;
		for (var i = 0; i < _tabs.Count; i++)
		{
			var match = _tabs[i].Route == route;
			_tabs[i].IsSelected = match;
			if (match)
			{
				newIndex = i;
			}
		}

		if (newIndex >= 0 && newIndex != _previousIndex)
		{
			_ = RunTransitionAsync(_previousIndex, newIndex);
			_previousIndex = newIndex;
		}
	}

	private async Task RunTransitionAsync(int previousIndex, int newIndex)
	{
		var animator = TabTransitionAnimator;
		if (animator is null)
		{
			return;
		}

		_transitionCts?.Cancel();
		_transitionCts = new CancellationTokenSource();
		var token = _transitionCts.Token;

		try
		{
			var context = new FsTabTransitionContext(this, outgoingView: null, incomingView: null, previousIndex, newIndex);
			await animator.AnimateAsync(context, token).ConfigureAwait(false);
		}
		catch (OperationCanceledException)
		{
			// Superseded by a newer transition; nothing to do.
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"[FsShell] Tab transition animator threw: {ex}");
		}
	}

	private void OnShellNavigated(object? sender, ShellNavigatedEventArgs e)
	{
		UpdateSelectedFlags();

		if (TabBar is IFsTabBar bar)
		{
			bar.SelectedRoute = CurrentRoute();
		}
	}

	private async void OnBarItemSelected(object? sender, FsTabBarSelectionChangedEventArgs e)
	{
		try
		{
			await GoToAsync($"//{e.Selected.Route}").ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"[FsShell] Navigation to '{e.Selected.Route}' failed: {ex}");
		}
	}
}
