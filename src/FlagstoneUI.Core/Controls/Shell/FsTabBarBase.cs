using System.Collections.Specialized;
using System.ComponentModel;

namespace FlagstoneUI.Core.Controls;

/// <summary>
/// Reusable base for a custom <see cref="FsShell"/> tab bar. Owns the bar contract
/// (<see cref="IFsTabBar"/>), item materialisation, tap routing, and visual-state pumping, leaving
/// the visual treatment (selection animations, decorations) to derived types.
/// </summary>
/// <remarks>
/// A derived bar supplies a <see cref="TabContainer"/> (the layout that hosts the instantiated tab
/// views) and overrides <see cref="OnSelectionChanged"/> and/or <see cref="OnSelectionInitialized"/>
/// to react to selection. Because XAML-backed subclasses only create their named elements inside
/// <c>InitializeComponent</c> — which runs after this base constructor — derived constructors must
/// call <see cref="InitializeTabContainer"/> once the container exists.
/// <para>
/// <see cref="FsTabBar"/> is the reference subclass; it adds a sliding pill and a scaling selected
/// tab on top of this base.
/// </para>
/// </remarks>
public abstract class FsTabBarBase : ContentView, IFsTabBar
{
	/// <summary>
	/// The layout that hosts the instantiated tab views. Supplied by the derived bar (typically a
	/// named element from its XAML). Used for item binding and visual-state pumping.
	/// </summary>
	protected abstract Layout TabContainer { get; }

	/// <summary>
	/// Installs the tap-wrapping template selector on <see cref="TabContainer"/>. Call once from the
	/// derived constructor after the container has been created (e.g. after <c>InitializeComponent</c>).
	/// </summary>
	protected void InitializeTabContainer()
	{
		BindableLayout.SetItemTemplateSelector(TabContainer, new TabItemTemplateSelector(this));
	}

	#region ItemsSource

	/// <summary>Bindable property for <see cref="ItemsSource"/>.</summary>
	public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
		nameof(ItemsSource),
		typeof(IReadOnlyList<FsTabContext>),
		typeof(FsTabBarBase),
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
		if (bindable is not FsTabBarBase bar)
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

		BindableLayout.SetItemsSource(bar.TabContainer, (System.Collections.IEnumerable?)newValue);

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
		foreach (var child in TabContainer.Children)
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
		foreach (var child in TabContainer.Children)
		{
			if (child is VisualElement ve && ve.BindingContext is FsTabContext ctx)
			{
				VisualStateManager.GoToState(ve, ctx.IsSelected ? "Selected" : "Unselected");
				VisualStateManager.GoToState(ve, ctx.IsEnabled ? "Normal" : "Disabled");
			}
		}

		OnSelectionInitialized();
	}

	#endregion

	#region ItemTemplate

	/// <summary>Bindable property for <see cref="ItemTemplate"/>.</summary>
	public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(
		nameof(ItemTemplate),
		typeof(DataTemplate),
		typeof(FsTabBarBase));

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
		typeof(FsTabBarBase),
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
	/// Invoked by template instances when the user taps a tab. Updates <see cref="SelectedRoute"/>,
	/// raises <see cref="ItemSelected"/>, then notifies <see cref="OnSelectionChanged"/>. Subclasses
	/// may override to customise selection behaviour.
	/// </summary>
	protected virtual void OnTabTapped(FsTabContext context)
	{
		if (context is null)
		{
			return;
		}

		SelectedRoute = context.Route;
		ItemSelected?.Invoke(this, new FsTabBarSelectionChangedEventArgs(context));
		OnSelectionChanged(context, animated: true);
	}

	/// <summary>
	/// Called when the selected tab changes in response to a user tap. Override to animate or
	/// restyle the bar. The base implementation does nothing.
	/// </summary>
	/// <param name="context">The newly selected tab.</param>
	/// <param name="animated">Whether the change should be animated (always <c>true</c> for taps).</param>
	protected virtual void OnSelectionChanged(FsTabContext context, bool animated)
	{
	}

	/// <summary>
	/// Called after tab views are (re)materialised, so the bar can apply visuals for the tab that is
	/// already selected — before the user has tapped anything. Override to place initial selection
	/// state. The base implementation does nothing.
	/// </summary>
	protected virtual void OnSelectionInitialized()
	{
	}

	/// <summary>
	/// Builds the default item template used when <see cref="ItemTemplate"/> is not set. Override to
	/// supply different default tab content.
	/// </summary>
	protected virtual DataTemplate BuildDefaultItemTemplate()
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

	/// <summary>Returns the first materialised tab view whose context matches <paramref name="predicate"/>.</summary>
	protected VisualElement? FindTab(Func<FsTabContext, bool> predicate)
	{
		foreach (var child in TabContainer.Children)
		{
			if (child is VisualElement tab && tab.BindingContext is FsTabContext ctx && predicate(ctx))
			{
				return tab;
			}
		}

		return null;
	}

	/// <summary>Returns the index of the selected tab within <see cref="ItemsSource"/>, or -1 if none.</summary>
	protected int SelectedIndex()
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

	/// <summary>
	/// Wraps each tab view in a tap gesture that routes through <see cref="OnTabTapped"/>, falling
	/// back to <see cref="BuildDefaultItemTemplate"/> when no <see cref="ItemTemplate"/> is supplied.
	/// </summary>
	protected sealed class TabItemTemplateSelector(FsTabBarBase owner) : DataTemplateSelector
	{
		private DataTemplate? _wrappedDefault;
		private DataTemplate? _wrappedCustom;
		private DataTemplate? _wrappedCustomFor;

		protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
		{
			var inner = owner.ItemTemplate;
			if (inner is null)
			{
				return _wrappedDefault ??= Wrap(owner.BuildDefaultItemTemplate());
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
	}
}
