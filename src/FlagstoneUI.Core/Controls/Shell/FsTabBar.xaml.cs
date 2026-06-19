using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;

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
		
		SetPillWidth();
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

	private double _pillWidth = 0;

	private VisualElement? _selectedTab = null;

	private void SetPillWidth(object? sender, EventArgs eventArgs) => SetPillWidth();
	private void SetPillWidth()
	{
		//TabPill.HeightRequest = BarBackground.Height;
		if (ItemsSource.Count > 0)
		{
			var firstItem = TabLayout.Children[0];
			_pillWidth = BarBackground.Width / ItemsSource.Count;
			TabPill.WidthRequest = _pillWidth * 1.3;
			Debug.WriteLine("Has items");
		}
		else
		{
			Debug.WriteLine("No items");
		}
		
		Debug.WriteLine($"Tab Pill Width is {TabPill.Width},  pill Width is {_pillWidth}");
	}

	private void AnimateTabs(FsTabContext context)
	{
		var newIndex = ItemsSource.ToList().IndexOf(context);
		VisualElement? newTab = null;
		
		foreach (var child in TabLayout.Children)
		{
			if (child is not VisualElement tab)
			{
				continue;
			}

			if (tab == _selectedTab)
			{
				_ = tab.ScaleToAsync(1, 300, Easing.CubicOut);
			}

			if (tab.BindingContext is FsTabContext ctx && ctx == context)
			{
				newTab = tab;
			}
		}

		if (newTab is not null)
		{
			_selectedTab = newTab;
			_ = _selectedTab.ScaleToAsync(1.3, 300, Easing.CubicIn);
		}

		_ = TabPill.TranslateToAsync(newIndex * _pillWidth, -15,  300, Easing.CubicIn);
		
		Debug.WriteLine($"New index is {newIndex}, pill translation is {TabPill.TranslationX},  pill width is {TabPill.Width}");
		Debug.WriteLine($"Background grid width: {BarBackground.Width}, layout grid width: {TabLayout.Width}");
	}

	#endregion
}

