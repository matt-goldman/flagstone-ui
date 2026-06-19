namespace FlagstoneUI.Core.Controls;

/// <summary>
/// Reference <see cref="FsTabBarBase"/> implementation: a horizontal bar that slides a rounded pill
/// behind the selected tab and scales the selected tab up to emphasise it. Both effects are
/// independently toggleable via <see cref="ShowPill"/> and <see cref="ScaleSelectedTab"/>.
/// </summary>
public partial class FsTabBar : FsTabBarBase
{
	public FsTabBar()
	{
		InitializeComponent();
		InitializeTabContainer();
		BarBackground.SizeChanged += SetPillWidth;
	}

	/// <inheritdoc />
	protected override Layout TabContainer => TabLayout;

	/// <inheritdoc />
	protected override void OnSelectionChanged(FsTabContext context, bool animated) => AnimateTabs(context, animated);

	/// <inheritdoc />
	protected override void OnSelectionInitialized() => ApplyInitialSelection();

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

	private void AnimateTabs(FsTabContext context, bool animated)
	{
		if (ScaleSelectedTab)
		{
			if (_selectedTab is not null)
			{
				if (animated)
				{
					_ = _selectedTab.ScaleToAsync(1, AnimationLength, Easing.CubicOut);
				}
				else
				{
					_selectedTab.Scale = 1;
				}
			}

			_selectedTab = FindTab(ctx => ReferenceEquals(ctx, context));
			ApplySelectedScale(animated);
		}

		if (ShowPill)
		{
			MovePill(ItemsSource.ToList().IndexOf(context), animated);
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

	#endregion
}
