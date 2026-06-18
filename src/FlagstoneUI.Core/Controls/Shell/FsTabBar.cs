using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;

namespace FlagstoneUI.Core.Controls;

/// <summary>
/// Reference tab bar implementation used by <see cref="FsShell"/> as the default bar. Public,
/// documented, and subclassable. Can also be used directly in the bar-replacement slot to
/// retain the default layout while customising the wrapping container.
/// </summary>
/// <remarks>
/// Internally uses a <see cref="BindableLayout"/>-driven single-row <see cref="Grid"/> with one
/// auto-sized column per item, so template instances participate in normal MAUI layout and can
/// be styled with the standard layout, sizing, and visual-state idioms.
/// </remarks>
public class FsTabBar : ContentView, IFsTabBar
{
	private readonly Grid _layout;
	private readonly Grid _backgroundGrid;
	private readonly BoxView _tabPill = new BoxView
	{
		BackgroundColor = Colors.Yellow,
		Margin = 0,
		HorizontalOptions = LayoutOptions.Start,
		VerticalOptions = LayoutOptions.Start
	};

	/// <summary>Initializes a new <see cref="FsTabBar"/>.</summary>
	public FsTabBar()
	{
		// Background Grid used to place animated background
		_backgroundGrid = new Grid
		{
			ColumnSpacing = 0,
			RowSpacing = 0,
			HorizontalOptions = LayoutOptions.Fill,
			VerticalOptions = LayoutOptions.Start,
			RowDefinitions = { new RowDefinition(GridLength.Auto) },
			Padding = new Thickness(0),
		};

		_backgroundGrid.SizeChanged += SetPillWidth;
		
		_backgroundGrid.Add(_tabPill);
		
		// Items are placed in a single-row Grid with one auto-sized column per item, instead
		// of a HorizontalStackLayout. The outer-HSL pattern triggered a MAUI iOS layout bug
		// where item children (especially items whose template root is also a HSL) collapsed
		// to zero height on layout passes after the first. Grid's CrossPlatformArrange
		// distributes children from its measured row/column geometry rather than re-reading
		// each child's DesiredSize directly, so the children stay at their measured size.
		_layout = new Grid
		{
			ColumnSpacing = 0,
			RowSpacing = 0,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Start,
			RowDefinitions = { new RowDefinition(GridLength.Auto) },
			Margin = new Thickness(0),
		};

		// VerticalOptions.Start on the bar itself keeps Measure(W, H) returning the bar's
		// natural content height instead of expanding to fill the H constraint. The
		// FsShell renderers position the bar against the bottom edge directly.
		VerticalOptions = LayoutOptions.Start;

		BindableLayout.SetItemTemplateSelector(_layout, new TabItemTemplateSelector(this));
		
		_backgroundGrid.Add(_layout);
		Content = _backgroundGrid;

		// BindableLayout populates Grid.Children but doesn't assign Grid.Column to each item.
		// Wire ColumnDefinitions + Grid.Column on the fly so each item lands in its own
		// auto-sized column.
		_layout.ChildAdded += OnLayoutChildAdded;
		_layout.ChildRemoved += OnLayoutChildRemoved;
	}

	private void OnLayoutChildAdded(object? sender, ElementEventArgs e)
	{
		var index = _layout.ColumnDefinitions.Count;
		_layout.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
		if (e.Element is View v)
		{
			Grid.SetColumn(v, index);
		}
		
		//SetPillWidth();
	}

	private void OnLayoutChildRemoved(object? sender, ElementEventArgs e)
	{
		var columnCount = _layout.ColumnDefinitions.Count;
		if (columnCount > 0)
		{
			_layout.ColumnDefinitions.RemoveAt(_layout.ColumnDefinitions.Count - 1);
		}
		
		//SetPillWidth();
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


		BindableLayout.SetItemsSource(bar._layout, (System.Collections.IEnumerable?)newValue);

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
		foreach (var child in _layout.Children)
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
		foreach (var child in _layout.Children)
		{
			if (child is VisualElement ve && ve.BindingContext is FsTabContext ctx)
			{
				VisualStateManager.GoToState(ve, ctx.IsSelected ? "Selected" : "Unselected");
				VisualStateManager.GoToState(ve, ctx.IsEnabled ? "Normal" : "Disabled");
			}
		}
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

	private sealed class TabItemTemplateSelector : DataTemplateSelector
	{
		private readonly FsTabBar _owner;
		private DataTemplate? _wrappedDefault;
		private DataTemplate? _wrappedCustom;
		private DataTemplate? _wrappedCustomFor;

		public TabItemTemplateSelector(FsTabBar owner)
		{
			_owner = owner;
		}

		protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
		{
			var inner = _owner.ItemTemplate;
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
						_owner.OnTabTapped(ctx);
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

	private double _pillWidth = 0;

	private void SetPillWidth(object? sender, EventArgs eventArgs)
	{
		_pillWidth = _backgroundGrid.Width / ItemsSource.Count;
		_tabPill.HeightRequest = _backgroundGrid.Height;
		if (ItemsSource.Count > 0)
		{
			_tabPill.WidthRequest = _pillWidth;
			Debug.WriteLine("Has items");
		}
		else
		{
			Debug.WriteLine("No items");
		}
		
		Debug.WriteLine($"Tab Pill Width is {_tabPill.Width},  pill Width is {_pillWidth}");
	}

	private void AnimateTabs(FsTabContext context)
	{
		var newIndex = ItemsSource.ToList().IndexOf(context);

		/*_ =*/ _tabPill.TranslationX = newIndex * _pillWidth; //.TranslateToAsync(newIndex * _tabPill.Width, 0,  300, Easing.CubicIn);
		
		Debug.WriteLine($"New index is {newIndex}, pill translation is {_tabPill.TranslationX},  pill width is {_tabPill.Width}");
		Debug.WriteLine($"Background grid width: {_backgroundGrid.Width}, layout grid width: {_layout.Width}");

		var pillX = _tabPill.X;
		var pillY = _tabPill.Y;
		
		Debug.WriteLine($"Pill x: {pillX}, Pill y: {pillY}");
	}

	#endregion
}
