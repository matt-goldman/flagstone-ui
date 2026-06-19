using System.Collections.Specialized;
using System.ComponentModel;

namespace FlagstoneUI.Core.Controls;

public partial class FsTabBar : ContentView, IFsTabBar
{
	public FsTabBar()
	{
		InitializeComponent();
		BindableLayout.SetItemTemplateSelector(TabLayout, new TabItemTemplateSelector(this));
		BarBackground.SizeChanged += SetPillWidth;
	}
	
	#region ItemsSource

	/// <summary>Bindable property for <see cref="ItemsSource"/>.</summary>
	public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
		nameof(ItemsSource),
		typeof(IReadOnlyList<FsTabContext>),
		typeof(FsTabBar),
		propertyChanged: OnItemsSourceChanged);

	/// <summary>
	/// The collection of tabs to render. Auto-populated by <see cref="FsShell"/> when this bar
	/// is hosted as the default. Settable directly when used standalone or wrapped inside a
	/// custom layout.
	/// </summary>
	public IReadOnlyList<FsTabContext> ItemsSource
	{
		get => (IReadOnlyList<FsTabContext>)(GetValue(ItemsSourceProperty) ?? Array.Empty<FsTabContext>());
		set => SetValue(ItemsSourceProperty, value);
	}

	private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is not FsTabBar bar)
		{
			return;
		}


		if (oldValue is INotifyCollectionChanged oldCollection)
		{
			oldCollection.CollectionChanged -= bar.OnItemsCollectionChanged;
		}


		if (oldValue is IEnumerable<FsTabContext> oldItems)
		{

			foreach (var item in oldItems)
			{
				item.PropertyChanged -= bar.OnTabContextPropertyChanged;
			}
		}


		BindableLayout.SetItemsSource(bar.TabLayout, (System.Collections.IEnumerable?)newValue);

		if (newValue is INotifyCollectionChanged newCollection)
		{
			newCollection.CollectionChanged += bar.OnItemsCollectionChanged;
		}


		if (newValue is IEnumerable<FsTabContext> newItems)
		{

			foreach (var item in newItems)
			{
				item.PropertyChanged += bar.OnTabContextPropertyChanged;
			}
		}


		bar.PumpAllVsmStates();
	}

	private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		if (e.OldItems is not null)
		{

			foreach (FsTabContext ctx in e.OldItems)
			{
				ctx.PropertyChanged -= OnTabContextPropertyChanged;
			}
		}


		if (e.NewItems is not null)
		{

			foreach (FsTabContext ctx in e.NewItems)
			{
				ctx.PropertyChanged += OnTabContextPropertyChanged;
			}
		}
		
		PumpAllVsmStates();
	}

	private void OnTabContextPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is not (nameof(FsTabContext.IsSelected) or nameof(FsTabContext.IsEnabled)))
		{
			return;
		}


		if (sender is FsTabContext ctx)
		{
			PumpVsmState(ctx);
		}

	}

	private void PumpVsmState(FsTabContext ctx)
	{
		foreach (var child in TabLayout.Children)
		{
			if (child is VisualElement ve && ve.BindingContext == ctx)
			{
				VisualStateManager.GoToState(ve, ctx.IsSelected ? "Selected" : "Unselected");
				VisualStateManager.GoToState(ve, ctx.IsEnabled ? "Normal" : "Disabled");
				break;
			}
		}
	}

	private void PumpAllVsmStates()
	{
		foreach (var child in TabLayout.Children)
		{
			if (child is VisualElement ve && ve.BindingContext is FsTabContext ctx)
			{
				VisualStateManager.GoToState(ve, ctx.IsSelected ? "Selected" : "Unselected");
				VisualStateManager.GoToState(ve, ctx.IsEnabled ? "Normal" : "Disabled");
			}
		}

		ApplyInitialSelection();
	}

	#endregion

	#region ItemTemplate

	/// <summary>Bindable property for <see cref="ItemTemplate"/>.</summary>
	public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(
		nameof(ItemTemplate),
		typeof(DataTemplate),
		typeof(FsTabBar));

	/// <summary>
	/// Template applied to each tab. When this bar is hosted as the default inside
	/// <see cref="FsShell"/>, the template is forwarded from <see cref="FsShell.TabBarItemTemplate"/>.
	/// </summary>
	public DataTemplate? ItemTemplate
	{
		get => (DataTemplate?)GetValue(ItemTemplateProperty);
		set => SetValue(ItemTemplateProperty, value);
	}

	#endregion

	#region SelectedRoute

	/// <summary>Bindable property for <see cref="SelectedRoute"/>.</summary>
	public static readonly BindableProperty SelectedRouteProperty = BindableProperty.Create(
		nameof(SelectedRoute),
		typeof(string),
		typeof(FsTabBar),
		defaultBindingMode: BindingMode.TwoWay);

	/// <summary>
	/// The route of the currently selected tab. Setting this navigates the hosting
	/// <see cref="FsShell"/> to that route; observing this reflects external navigation back
	/// into the bar's selection state.
	/// </summary>
	public string? SelectedRoute
	{
		get => (string?)GetValue(SelectedRouteProperty);
		set => SetValue(SelectedRouteProperty, value);
	}

	#endregion

	#region PillBackground

	/// <summary>Bindable property for <see cref="PillBackground"/>.</summary>
	public static readonly BindableProperty PillBackgroundProperty = BindableProperty.Create(
		nameof(PillBackground),
		typeof(Brush),
		typeof(FsTabBar),
		new SolidColorBrush(Colors.DarkOrchid.WithAlpha(0.65f)));

	/// <summary>
	/// The brush painted behind the selected tab. Bound to the moving pill that tracks selection.
	/// </summary>
	public Brush PillBackground
	{
		get => (Brush)GetValue(PillBackgroundProperty);
		set => SetValue(PillBackgroundProperty, value);
	}

	#endregion

	#region ShowPill

	/// <summary>Bindable property for <see cref="ShowPill"/>.</summary>
	public static readonly BindableProperty ShowPillProperty = BindableProperty.Create(
		nameof(ShowPill),
		typeof(bool),
		typeof(FsTabBar),
		true,
		propertyChanged: OnShowPillChanged);

	/// <summary>
	/// Whether the selection pill is rendered behind the active tab. When <c>false</c>, the pill
	/// is hidden and its sizing/translation work is skipped entirely.
	/// </summary>
	public bool ShowPill
	{
		get => (bool)GetValue(ShowPillProperty);
		set => SetValue(ShowPillProperty, value);
	}

	private static void OnShowPillChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is FsTabBar bar && newValue is true)
		{
			// Pill was just enabled — size and place it under the current selection.
			bar.SetPillWidth();
		}
	}

	#endregion

	#region ScaleSelectedTab

	/// <summary>Bindable property for <see cref="ScaleSelectedTab"/>.</summary>
	public static readonly BindableProperty ScaleSelectedTabProperty = BindableProperty.Create(
		nameof(ScaleSelectedTab),
		typeof(bool),
		typeof(FsTabBar),
		true,
		propertyChanged: OnScaleSelectedTabChanged);

	/// <summary>
	/// Whether the selected tab is scaled up to emphasise it. When <c>false</c>, no scale
	/// animation runs and tabs are left at their natural size.
	/// </summary>
	public bool ScaleSelectedTab
	{
		get => (bool)GetValue(ScaleSelectedTabProperty);
		set => SetValue(ScaleSelectedTabProperty, value);
	}

	private static void OnScaleSelectedTabChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is not FsTabBar bar)
		{
			return;
		}

		if (newValue is true)
		{
			// Newly enabled — emphasise whatever is currently selected.
			bar.ApplySelectedScale(animated: false);
		}
		else
		{
			// Disabled — return every tab to its natural size.
			foreach (var child in bar.TabLayout.Children)
			{
				if (child is VisualElement tab)
				{
					tab.Scale = 1;
				}
			}
		}
	}

	#endregion

	/// <inheritdoc />
	public event EventHandler<FsTabBarSelectionChangedEventArgs>? ItemSelected;

	/// <summary>
	/// Invoked by template instances when the user taps a tab. Subclasses may override to
	/// customise selection behaviour.
	/// </summary>
	protected virtual void OnTabTapped(FsTabContext context)
	{
		if (context is null)
		{
			return;
		}

		SelectedRoute = context.Route;
		ItemSelected?.Invoke(this, new FsTabBarSelectionChangedEventArgs(context));
		AnimateTabs(context);
	}

	public sealed class TabItemTemplateSelector(FsTabBar owner) : DataTemplateSelector
	{
		private DataTemplate? _wrappedDefault;
		private DataTemplate? _wrappedCustom;
		private DataTemplate? _wrappedCustomFor;

		protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
		{
			var inner = owner.ItemTemplate;
			if (inner is null)
			{
				return _wrappedDefault ??= Wrap(BuildDefaultTemplate());
			}

			// Cache one wrapper per inner template instance. BindableLayout reuses the same
			// DataTemplate reference to instantiate every item, so handing back a freshly-
			// constructed wrapper per call breaks the items collection on some platforms.
			if (!ReferenceEquals(_wrappedCustomFor, inner))
			{
				_wrappedCustomFor = inner;
				_wrappedCustom = Wrap(inner);
			}

			return _wrappedCustom!;
		}

		private DataTemplate Wrap(DataTemplate inner)
		{
			return new DataTemplate(() =>
			{
				var view = (View)inner.CreateContent();
				var tap = new TapGestureRecognizer();
				tap.Tapped += (_, _) =>
				{
					if (view.BindingContext is FsTabContext ctx)
					{
						owner.OnTabTapped(ctx);
					}

				};
				view.GestureRecognizers.Add(tap);

				return view;
			});
		}

		private static DataTemplate BuildDefaultTemplate()
		{
			return new DataTemplate(() =>
			{
				var icon = new Image
				{
					HorizontalOptions = LayoutOptions.Center,
					HeightRequest = 24,
					WidthRequest = 24,
				};
				icon.SetBinding(Image.SourceProperty, nameof(FsTabContext.Icon));

				var title = new Label
				{
					HorizontalOptions = LayoutOptions.Center,
					FontSize = 12,
				};
				title.SetBinding(Label.TextProperty, nameof(FsTabContext.Title));

				var stack = new VerticalStackLayout
				{
					Padding = new Thickness(8),
					Spacing = 4,
					HorizontalOptions = LayoutOptions.Fill,
					Children = { icon, title },
				};

				return stack;
			});
		}
	}
	
	#region animations

	private const double SelectedScale = 1.3;
	private const double PillWidthFactor = 1.3;
	private const double PillOffsetY = -15;
	private const uint AnimationLength = 300;

	private double _pillWidth = 0;

	private VisualElement? _selectedTab = null;

	private void SetPillWidth(object? sender, EventArgs eventArgs) => SetPillWidth();
	private void SetPillWidth()
	{
		if (!ShowPill || ItemsSource.Count == 0)
		{
			return;
		}

		_pillWidth = BarBackground.Width / ItemsSource.Count;
		TabPill.WidthRequest = _pillWidth * PillWidthFactor;

		// Park the pill under the selected tab on (re)layout, before any tap occurs.
		MovePill(SelectedIndex(), animated: false);
	}

	/// <summary>
	/// Applies the selection visuals (scale + pill position) for the tab that is selected on init,
	/// before the user has tapped anything. Honours <see cref="ScaleSelectedTab"/> and
	/// <see cref="ShowPill"/>.
	/// </summary>
	private void ApplyInitialSelection()
	{
		_selectedTab = FindTab(ctx => ctx.IsSelected);

		ApplySelectedScale(animated: false);

		// Pill placement also needs the bar width; SetPillWidth re-runs once that is known.
		SetPillWidth();
	}

	/// <summary>Emphasises the currently selected tab when <see cref="ScaleSelectedTab"/> is enabled.</summary>
	private void ApplySelectedScale(bool animated)
	{
		if (!ScaleSelectedTab)
		{
			return;
		}

		_selectedTab ??= FindTab(ctx => ctx.IsSelected);
		if (_selectedTab is null)
		{
			return;
		}

		if (animated)
		{
			_ = _selectedTab.ScaleToAsync(SelectedScale, AnimationLength, Easing.CubicIn);
		}
		else
		{
			_selectedTab.Scale = SelectedScale;
		}
	}

	private void AnimateTabs(FsTabContext context)
	{
		if (ScaleSelectedTab)
		{
			if (_selectedTab is not null)
			{
				_ = _selectedTab.ScaleToAsync(1, AnimationLength, Easing.CubicOut);
			}

			_selectedTab = FindTab(ctx => ReferenceEquals(ctx, context));
			ApplySelectedScale(animated: true);
		}

		if (ShowPill)
		{
			MovePill(ItemsSource.ToList().IndexOf(context), animated: true);
		}
	}

	/// <summary>Translates the pill to the tab at <paramref name="index"/>.</summary>
	private void MovePill(int index, bool animated)
	{
		if (!ShowPill || index < 0)
		{
			return;
		}

		var x = index * _pillWidth;
		if (animated)
		{
			_ = TabPill.TranslateToAsync(x, PillOffsetY, AnimationLength, Easing.CubicIn);
		}
		else
		{
			TabPill.TranslationX = x;
		}
	}

	private int SelectedIndex()
	{
		for (var i = 0; i < ItemsSource.Count; i++)
		{
			if (ItemsSource[i].IsSelected)
			{
				return i;
			}
		}

		return -1;
	}

	private VisualElement? FindTab(Func<FsTabContext, bool> predicate)
	{
		foreach (var child in TabLayout.Children)
		{
			if (child is VisualElement tab && tab.BindingContext is FsTabContext ctx && predicate(ctx))
			{
				return tab;
			}
		}

		return null;
	}

	#endregion
}

