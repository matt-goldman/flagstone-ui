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
	/// <summary>
	/// Resource key under which <see cref="FsShell"/> publishes the measured height of the
	/// currently hosted bottom chrome (typically <see cref="FsTabBar"/>) into
	/// <see cref="Application.Resources"/>. Pages can consume it via
	/// <c>{DynamicResource FsBottomChromeHeight}</c> — most easily through the
	/// <see cref="FsLayout.BottomChromePaddingProperty"/> attached property — to leave room for
	/// the chrome without any platform-specific safe-area code.
	/// </summary>
	/// <remarks>
	/// Written as a <see cref="double"/>. Updates whenever the bar's measured size changes or its
	/// <see cref="VisualElement.IsVisible"/> flips, and drops to 0 when no bar is hosted.
	/// </remarks>
	public const string BottomChromeHeightResourceKey = "FsBottomChromeHeight";

	private readonly ObservableCollection<FsTabContext> _tabs = [];
	private readonly Dictionary<ShellSection, FsTabContext> _sectionContextMap = [];
	private readonly Dictionary<FsTabContext, ShellSection> _contextSectionMap = [];
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
	/// Live collection of contexts for the bottom tabs of the current <see cref="ShellItem"/> — one
	/// per <see cref="ShellSection"/>, mirroring the tabs stock Shell would show on the active item.
	/// Re-projected when the active item changes (e.g. via the flyout) and exposed to the bar slot
	/// via <see cref="IFsTabBar.ItemsSource"/>.
	/// </summary>
	/// <remarks>
	/// The tab bar is scoped to a single <see cref="ShellItem"/> exactly as Shell's native bottom bar
	/// is: a <c>TabBar</c> (or a <c>FlyoutItem</c> with multiple <c>Tab</c>/<c>ShellSection</c> children)
	/// projects its sections here, while the flyout chrome and top-tab strip continue to be rendered by
	/// stock Shell. This preserves Shell's full navigation hierarchy unchanged.
	/// </remarks>
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

		if (oldValue is ContentView oldView)
		{
			oldView.SizeChanged -= shell.OnBarSizeChanged;
			oldView.PropertyChanged -= shell.OnBarPropertyChanged;
		}


		if (newValue is ContentView newView)
		{
			newView.SizeChanged += shell.OnBarSizeChanged;
			newView.PropertyChanged += shell.OnBarPropertyChanged;
			shell.AttachBar(newView);
			shell.PublishBottomChromeHeight();

			if (shell.TabBarItemTemplate is not null && newValue is not FsTabBar)
			{
				System.Diagnostics.Debug.WriteLine(
					"[FsShell] TabBarItemTemplate is set but a custom TabBar replaces the default bar; the template will be ignored.");
			}
		}
		else
		{
			shell.PublishBottomChromeHeight();
		}
	}

	private void OnBarSizeChanged(object? sender, EventArgs e) => PublishBottomChromeHeight();

	private void OnBarPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(VisualElement.IsVisible))
		{
			PublishBottomChromeHeight();
		}
	}

	/// <summary>
	/// Writes the bar's current measured height into <see cref="Application.Resources"/> under
	/// <see cref="BottomChromeHeightResourceKey"/>. Pages that opt in via
	/// <see cref="FsLayout.BottomChromePaddingProperty"/> (or read the resource directly) reserve
	/// room for the bar without touching any platform code.
	/// </summary>
	private void PublishBottomChromeHeight()
	{
		var height = TabBar is { IsVisible: true } bar ? bar.Height : 0;
		if (double.IsNaN(height) || double.IsInfinity(height) || height < 0)
		{
			height = 0;
		}

		if (Application.Current is { } app)
		{
			app.Resources[BottomChromeHeightResourceKey] = height;
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

	#region HideTabBarOnKeyboard

	/// <summary>Bindable property for <see cref="HideTabBarOnKeyboard"/>.</summary>
	public static readonly BindableProperty HideTabBarOnKeyboardProperty = BindableProperty.Create(
		nameof(HideTabBarOnKeyboard),
		typeof(bool),
		typeof(FsShell),
		defaultValue: true);

	/// <summary>
	/// When <see langword="true"/> (the default), the tab bar slides off-screen when the soft
	/// keyboard is presented and restores when the keyboard dismisses. The per-platform renderer
	/// reads this property to gate keyboard-avoidance behaviour.
	/// </summary>
	public bool HideTabBarOnKeyboard
	{
		get => (bool)GetValue(HideTabBarOnKeyboardProperty);
		set => SetValue(HideTabBarOnKeyboardProperty, value);
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

	/// <summary>
	/// Rebuilds the <see cref="Tabs"/> collection from the current <see cref="ShellItem"/>'s
	/// <see cref="ShellSection"/>s — the same set stock Shell renders in its bottom bar for the
	/// active item. Called when the active item changes and on visual-tree changes; subclasses may
	/// override to customise tab projection.
	/// </summary>
	protected virtual void RebuildTabs()
	{
		// The bottom tab bar is scoped to the active ShellItem: its sections are the tabs. This holds
		// for both a <TabBar> (sections = the ShellContent children) and a <FlyoutItem> with multiple
		// <Tab>/section children. Switching between items happens through stock flyout chrome.
		var sections = CurrentItem?.Items?.ToList() ?? [];

		// Unsubscribe from sections no longer in the active item.
		foreach (var removed in _sectionContextMap.Keys.Except(sections).ToList())
		{
			removed.PropertyChanged -= OnShellSectionPropertyChanged;
			if (_sectionContextMap.Remove(removed, out var removedCtx))
			{
				_contextSectionMap.Remove(removedCtx);
			}
		}

		// Create or update an FsTabContext for each section in the active item.
		foreach (var section in sections)
		{
			if (!_sectionContextMap.TryGetValue(section, out var ctx))
			{
				ctx = new FsTabContext(!string.IsNullOrEmpty(section.Route) ? section.Route : string.Empty);
				_sectionContextMap[section] = ctx;
				_contextSectionMap[ctx] = section;
				section.PropertyChanged += OnShellSectionPropertyChanged;
			}

			ApplySectionMetadata(section, ctx);
		}

		// Synchronise the observable collection in-place so that existing FsTabContext instances
		// are preserved across rebuilds — BindableLayout will not recreate template instances for
		// contexts that stay at the same position.
		SyncTabs(sections.Select(s => _sectionContextMap[s]).ToList());

		if (TabBar is IFsTabBar bar)
		{
			bar.ItemsSource = _tabs;
			bar.SelectedRoute = CurrentRoute();
		}

		UpdateSelectedFlags();
		BridgeTabBarVisibility();
	}

	private static void ApplySectionMetadata(ShellSection section, FsTabContext ctx)
	{
		var content = section.CurrentItem ?? section.Items?.FirstOrDefault();
		var parentItem = section.Parent as ShellItem;
		ctx.Title = section.Title ?? content?.Title ?? parentItem?.Title ?? ctx.Route;
		ctx.Icon = section.Icon ?? content?.Icon ?? parentItem?.Icon;
	}

	private void SyncTabs(List<FsTabContext> desired)
	{
		for (var i = _tabs.Count - 1; i >= 0; i--)
		{
			if (!desired.Contains(_tabs[i]))
			{
				_tabs.RemoveAt(i);
			}
		}

		for (var i = 0; i < desired.Count; i++)
		{
			var ctx = desired[i];
			var current = _tabs.IndexOf(ctx);
			if (current < 0)
			{
				_tabs.Insert(i, ctx);
			}
			else if (current != i)
			{
				_tabs.Move(current, i);
			}

		}
	}

	private void OnShellSectionPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (sender is not ShellSection section || !_sectionContextMap.TryGetValue(section, out var ctx))
		{
			return;
		}

		// Title/Icon (including the current content's, which feed the fallback) refresh the tab; the
		// current-item change also updates which content supplies the fallback metadata.
		if (e.PropertyName is nameof(ShellSection.Title)
			or nameof(ShellSection.Icon)
			or nameof(ShellSection.CurrentItem))
		{
			ApplySectionMetadata(section, ctx);
		}
	}

	private void BridgeTabBarVisibility()
	{
		if (TabBar is not ContentView barView)
		{
			return;
		}

		// Match stock Shell: the bottom bar only appears when the active item has more than one
		// section, and honours per-page Shell.SetTabBarIsVisible.
		var pageAllowsBar = CurrentPage is not { } page || Shell.GetTabBarIsVisible(page);
		barView.IsVisible = _tabs.Count > 1 && pageAllowsBar;
	}

	/// <summary>The route of the active section — the selected bottom tab.</summary>
	private string? CurrentRoute() => CurrentItem?.CurrentItem?.Route;

	private void UpdateSelectedFlags()
	{
		// Selection is the active section, matched by identity rather than route so that empty or
		// duplicate section routes do not confuse the highlight.
		var selectedSection = CurrentItem?.CurrentItem;
		var newIndex = -1;
		for (var i = 0; i < _tabs.Count; i++)
		{
			var match = _contextSectionMap.TryGetValue(_tabs[i], out var section)
				&& ReferenceEquals(section, selectedSection);
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
			var outgoingPage = TryGetPageForTab(previousIndex);
			var incomingPage = TryGetPageForTab(newIndex);
			var context = new FsTabTransitionContext(this, outgoingPage, incomingPage, previousIndex, newIndex);
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

	/// <summary>
	/// Resolves the materialised <see cref="Page"/> for the tab at <paramref name="tabIndex"/>, or
	/// returns <see langword="null"/> if the index is out of range, the tab has no mapped
	/// <see cref="ShellSection"/>, or the section's content has not been materialised yet.
	/// </summary>
	/// <remarks>
	/// Used by <see cref="RunTransitionAsync"/> to populate the outgoing/incoming pages on
	/// <see cref="FsTabTransitionContext"/>. The lookup goes via the cached section map rather
	/// than via Shell's navigation graph so it doesn't depend on the current navigation state.
	/// </remarks>
	private Page? TryGetPageForTab(int tabIndex)
	{
		if (tabIndex < 0 || tabIndex >= _tabs.Count)
		{
			return null;
		}

		if (!_contextSectionMap.TryGetValue(_tabs[tabIndex], out var section))
		{
			return null;
		}

		// ShellContent.Content holds the materialised Page once the tab has been entered. For an
		// outgoing tab we are guaranteed it has been entered (we are leaving it); for an incoming
		// tab Shell materialises the content as part of activating it. An unmaterialised content
		// surfaces here as null and the animator can decide whether to skip or wait.
		return section.CurrentItem?.Content as Page;
	}

	private void OnShellNavigated(object? sender, ShellNavigatedEventArgs e)
	{
		// The active item may have changed (e.g. via the flyout), so re-project its sections before
		// refreshing selection/visibility. RebuildTabs also calls UpdateSelectedFlags and
		// BridgeTabBarVisibility, and pushes the current route to the bar.
		RebuildTabs();

		if (TabBar is IFsTabBar bar)
		{
			bar.SelectedRoute = CurrentRoute();
		}
	}

	private void OnBarItemSelected(object? sender, FsTabBarSelectionChangedEventArgs e)
	{
		// Selecting a bottom tab activates its section within the current item — the same effect as
		// tapping stock Shell's native bottom bar. Direct property assignment keeps OnNavigating /
		// OnNavigated semantics intact without needing a resolvable absolute route.
		if (CurrentItem is { } item
			&& _contextSectionMap.TryGetValue(e.Selected, out var section)
			&& !ReferenceEquals(item.CurrentItem, section))
		{
			item.CurrentItem = section;
		}
	}
}
