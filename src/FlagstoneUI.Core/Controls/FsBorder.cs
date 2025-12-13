using Microsoft.Maui.Controls.Shapes;

namespace FlagstoneUI.Core.Controls;

/// <summary>
/// A border control with support for per-edge thickness and brushes.
/// </summary>
/// <remarks>
/// FsBorder extends the standard MAUI Border with per-edge border primitives.
/// Borders are rendered using Line elements positioned in OnSizeAllocated.
/// Lines are only materialized when the corresponding edge thickness > 0.
/// 
/// Note: Rounded corners with per-edge borders may clip or approximate corners.
/// Advanced corner joins (angled bevels, custom joins) are out of scope.
/// </remarks>
public partial class FsBorder : ContentView
{
	private readonly Grid _layoutRoot;
	private readonly Border _innerBorder;
	private readonly ContentPresenter _contentPresenter;
	private Line? _topLine;
	private Line? _rightLine;
	private Line? _bottomLine;
	private Line? _leftLine;

	public FsBorder()
	{
		_layoutRoot = new Grid();
		_innerBorder = new Border();
		_contentPresenter = new ContentPresenter();

		_innerBorder.Content = _contentPresenter;
		_layoutRoot.Children.Add(_innerBorder);

		Content = _layoutRoot;

		// Bind content presenter to this control's content
		_contentPresenter.SetBinding(ContentPresenter.ContentProperty, new Binding(nameof(BorderContent), source: this));
		_innerBorder.SetBinding(Border.BackgroundProperty, new Binding(nameof(Background), source: this));
		_innerBorder.SetBinding(Border.PaddingProperty, new Binding(nameof(Padding), source: this));
		_innerBorder.SetBinding(Border.StrokeShapeProperty, new Binding(nameof(StrokeShape), source: this));
	}

	protected override void OnSizeAllocated(double width, double height)
	{
		base.OnSizeAllocated(width, height);

		if (width > 0 && height > 0)
		{
			UpdateBorderLines(width, height);
		}
	}

	private void UpdateBorderLines(double width, double height)
	{
		// Top border
		if (BorderTopThickness > 0)
		{
			if (_topLine == null)
			{
				_topLine = new Line
				{
					StrokeLineCap = BorderStrokeCap
				};
				_layoutRoot.Children.Add(_topLine);
			}
			_topLine.X1 = 0;
			_topLine.Y1 = BorderTopThickness / 2;
			_topLine.X2 = width;
			_topLine.Y2 = BorderTopThickness / 2;
			_topLine.Stroke = BorderTopBrush;
			_topLine.StrokeThickness = BorderTopThickness;
			_topLine.IsVisible = true;
		}
		else if (_topLine != null)
		{
			_topLine.IsVisible = false;
		}

		// Bottom border
		if (BorderBottomThickness > 0)
		{
			if (_bottomLine == null)
			{
				_bottomLine = new Line
				{
					StrokeLineCap = BorderStrokeCap
				};
				_layoutRoot.Children.Add(_bottomLine);
			}
			_bottomLine.X1 = 0;
			_bottomLine.Y1 = height - (BorderBottomThickness / 2);
			_bottomLine.X2 = width;
			_bottomLine.Y2 = height - (BorderBottomThickness / 2);
			_bottomLine.Stroke = BorderBottomBrush;
			_bottomLine.StrokeThickness = BorderBottomThickness;
			_bottomLine.IsVisible = true;
		}
		else if (_bottomLine != null)
		{
			_bottomLine.IsVisible = false;
		}

		// Left border
		if (BorderLeftThickness > 0)
		{
			if (_leftLine == null)
			{
				_leftLine = new Line
				{
					StrokeLineCap = BorderStrokeCap
				};
				_layoutRoot.Children.Add(_leftLine);
			}
			_leftLine.X1 = BorderLeftThickness / 2;
			_leftLine.Y1 = 0;
			_leftLine.X2 = BorderLeftThickness / 2;
			_leftLine.Y2 = height;
			_leftLine.Stroke = BorderLeftBrush;
			_leftLine.StrokeThickness = BorderLeftThickness;
			_leftLine.IsVisible = true;
		}
		else if (_leftLine != null)
		{
			_leftLine.IsVisible = false;
		}

		// Right border
		if (BorderRightThickness > 0)
		{
			if (_rightLine == null)
			{
				_rightLine = new Line
				{
					StrokeLineCap = BorderStrokeCap
				};
				_layoutRoot.Children.Add(_rightLine);
			}
			_rightLine.X1 = width - (BorderRightThickness / 2);
			_rightLine.Y1 = 0;
			_rightLine.X2 = width - (BorderRightThickness / 2);
			_rightLine.Y2 = height;
			_rightLine.Stroke = BorderRightBrush;
			_rightLine.StrokeThickness = BorderRightThickness;
			_rightLine.IsVisible = true;
		}
		else if (_rightLine != null)
		{
			_rightLine.IsVisible = false;
		}
	}

	#region BorderContent Property
	/// <summary>
	/// Identifies the BorderContent bindable property.
	/// </summary>
	public static readonly BindableProperty BorderContentProperty = BindableProperty.Create(
		nameof(BorderContent),
		typeof(View),
		typeof(FsBorder),
		null);

	/// <summary>
	/// Gets or sets the content displayed within the border.
	/// </summary>
	public View? BorderContent
	{
		get => (View?)GetValue(BorderContentProperty);
		set => SetValue(BorderContentProperty, value);
	}
	#endregion

	#region Background Property
	/// <summary>
	/// Identifies the Background bindable property.
	/// </summary>
	public new static readonly BindableProperty BackgroundProperty = BindableProperty.Create(
		nameof(Background),
		typeof(Brush),
		typeof(FsBorder),
		new SolidColorBrush(Colors.Transparent));

	/// <summary>
	/// Gets or sets the background brush.
	/// </summary>
	public new Brush Background
	{
		get => (Brush)GetValue(BackgroundProperty);
		set => SetValue(BackgroundProperty, value);
	}
	#endregion

	#region Padding Property
	/// <summary>
	/// Identifies the Padding bindable property.
	/// </summary>
	public new static readonly BindableProperty PaddingProperty = BindableProperty.Create(
		nameof(Padding),
		typeof(Thickness),
		typeof(FsBorder),
		new Thickness(0));

	/// <summary>
	/// Gets or sets the padding inside the border.
	/// </summary>
	public new Thickness Padding
	{
		get => (Thickness)GetValue(PaddingProperty);
		set => SetValue(PaddingProperty, value);
	}
	#endregion

	#region StrokeShape Property
	/// <summary>
	/// Identifies the StrokeShape bindable property.
	/// </summary>
	public static readonly BindableProperty StrokeShapeProperty = BindableProperty.Create(
		nameof(StrokeShape),
		typeof(IShape),
		typeof(FsBorder),
		null);

	/// <summary>
	/// Gets or sets the shape used for the border stroke.
	/// </summary>
	/// <remarks>
	/// Note: When using rounded corners with per-edge borders, corners may clip or approximate.
	/// </remarks>
	public IShape? StrokeShape
	{
		get => (IShape?)GetValue(StrokeShapeProperty);
		set => SetValue(StrokeShapeProperty, value);
	}
	#endregion

	#region BorderTopThickness Property
	/// <summary>
	/// Identifies the BorderTopThickness bindable property.
	/// </summary>
	public static readonly BindableProperty BorderTopThicknessProperty = BindableProperty.Create(
		nameof(BorderTopThickness),
		typeof(double),
		typeof(FsBorder),
		0d,
		propertyChanged: OnBorderPropertyChanged);

	/// <summary>
	/// Gets or sets the thickness of the top border.
	/// </summary>
	public double BorderTopThickness
	{
		get => (double)GetValue(BorderTopThicknessProperty);
		set => SetValue(BorderTopThicknessProperty, value);
	}
	#endregion

	#region BorderRightThickness Property
	/// <summary>
	/// Identifies the BorderRightThickness bindable property.
	/// </summary>
	public static readonly BindableProperty BorderRightThicknessProperty = BindableProperty.Create(
		nameof(BorderRightThickness),
		typeof(double),
		typeof(FsBorder),
		0d,
		propertyChanged: OnBorderPropertyChanged);

	/// <summary>
	/// Gets or sets the thickness of the right border.
	/// </summary>
	public double BorderRightThickness
	{
		get => (double)GetValue(BorderRightThicknessProperty);
		set => SetValue(BorderRightThicknessProperty, value);
	}
	#endregion

	#region BorderBottomThickness Property
	/// <summary>
	/// Identifies the BorderBottomThickness bindable property.
	/// </summary>
	public static readonly BindableProperty BorderBottomThicknessProperty = BindableProperty.Create(
		nameof(BorderBottomThickness),
		typeof(double),
		typeof(FsBorder),
		0d,
		propertyChanged: OnBorderPropertyChanged);

	/// <summary>
	/// Gets or sets the thickness of the bottom border.
	/// </summary>
	public double BorderBottomThickness
	{
		get => (double)GetValue(BorderBottomThicknessProperty);
		set => SetValue(BorderBottomThicknessProperty, value);
	}
	#endregion

	#region BorderLeftThickness Property
	/// <summary>
	/// Identifies the BorderLeftThickness bindable property.
	/// </summary>
	public static readonly BindableProperty BorderLeftThicknessProperty = BindableProperty.Create(
		nameof(BorderLeftThickness),
		typeof(double),
		typeof(FsBorder),
		0d,
		propertyChanged: OnBorderPropertyChanged);

	/// <summary>
	/// Gets or sets the thickness of the left border.
	/// </summary>
	public double BorderLeftThickness
	{
		get => (double)GetValue(BorderLeftThicknessProperty);
		set => SetValue(BorderLeftThicknessProperty, value);
	}
	#endregion

	#region BorderTopBrush Property
	/// <summary>
	/// Identifies the BorderTopBrush bindable property.
	/// </summary>
	public static readonly BindableProperty BorderTopBrushProperty = BindableProperty.Create(
		nameof(BorderTopBrush),
		typeof(Brush),
		typeof(FsBorder),
		new SolidColorBrush(Colors.Transparent),
		propertyChanged: OnBorderPropertyChanged);

	/// <summary>
	/// Gets or sets the brush for the top border.
	/// </summary>
	public Brush BorderTopBrush
	{
		get => (Brush)GetValue(BorderTopBrushProperty);
		set => SetValue(BorderTopBrushProperty, value);
	}
	#endregion

	#region BorderRightBrush Property
	/// <summary>
	/// Identifies the BorderRightBrush bindable property.
	/// </summary>
	public static readonly BindableProperty BorderRightBrushProperty = BindableProperty.Create(
		nameof(BorderRightBrush),
		typeof(Brush),
		typeof(FsBorder),
		new SolidColorBrush(Colors.Transparent),
		propertyChanged: OnBorderPropertyChanged);

	/// <summary>
	/// Gets or sets the brush for the right border.
	/// </summary>
	public Brush BorderRightBrush
	{
		get => (Brush)GetValue(BorderRightBrushProperty);
		set => SetValue(BorderRightBrushProperty, value);
	}
	#endregion

	#region BorderBottomBrush Property
	/// <summary>
	/// Identifies the BorderBottomBrush bindable property.
	/// </summary>
	public static readonly BindableProperty BorderBottomBrushProperty = BindableProperty.Create(
		nameof(BorderBottomBrush),
		typeof(Brush),
		typeof(FsBorder),
		new SolidColorBrush(Colors.Transparent),
		propertyChanged: OnBorderPropertyChanged);

	/// <summary>
	/// Gets or sets the brush for the bottom border.
	/// </summary>
	public Brush BorderBottomBrush
	{
		get => (Brush)GetValue(BorderBottomBrushProperty);
		set => SetValue(BorderBottomBrushProperty, value);
	}
	#endregion

	#region BorderLeftBrush Property
	/// <summary>
	/// Identifies the BorderLeftBrush bindable property.
	/// </summary>
	public static readonly BindableProperty BorderLeftBrushProperty = BindableProperty.Create(
		nameof(BorderLeftBrush),
		typeof(Brush),
		typeof(FsBorder),
		new SolidColorBrush(Colors.Transparent),
		propertyChanged: OnBorderPropertyChanged);

	/// <summary>
	/// Gets or sets the brush for the left border.
	/// </summary>
	public Brush BorderLeftBrush
	{
		get => (Brush)GetValue(BorderLeftBrushProperty);
		set => SetValue(BorderLeftBrushProperty, value);
	}
	#endregion

	#region BorderStrokeCap Property
	/// <summary>
	/// Identifies the BorderStrokeCap bindable property.
	/// </summary>
	public static readonly BindableProperty BorderStrokeCapProperty = BindableProperty.Create(
		nameof(BorderStrokeCap),
		typeof(PenLineCap),
		typeof(FsBorder),
		PenLineCap.Flat,
		propertyChanged: OnBorderPropertyChanged);

	/// <summary>
	/// Gets or sets the stroke line cap for border lines.
	/// </summary>
	public PenLineCap BorderStrokeCap
	{
		get => (PenLineCap)GetValue(BorderStrokeCapProperty);
		set => SetValue(BorderStrokeCapProperty, value);
	}
	#endregion

	private static void OnBorderPropertyChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is FsBorder border)
		{
			border.InvalidateMeasure();
		}
	}
}
