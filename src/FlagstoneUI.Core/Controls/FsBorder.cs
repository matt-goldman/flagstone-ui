using MauiBorder = Microsoft.Maui.Controls.Border;
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
	private readonly MauiBorder _innerBorder;
	private readonly ContentPresenter _contentPresenter;
	private Line? _topLine;
	private Line? _rightLine;
	private Line? _bottomLine;
	private Line? _leftLine;

	public FsBorder()
	{
		_layoutRoot = [];
		_innerBorder = new MauiBorder();
		_contentPresenter = new ContentPresenter();

		_innerBorder.Content = _contentPresenter;
		_layoutRoot.Children.Add(_innerBorder);

		Content = _layoutRoot;

		// Bind content presenter to this control's content
		_contentPresenter.SetBinding(ContentPresenter.ContentProperty, new Binding(nameof(BorderContent), source: this));
		_innerBorder.SetBinding(VisualElement.BackgroundProperty, new Binding(nameof(Background), source: this));
		_innerBorder.SetBinding(MauiBorder.PaddingProperty, new Binding(nameof(Padding), source: this));
		_innerBorder.SetBinding(MauiBorder.StrokeShapeProperty, new Binding(nameof(StrokeShape), source: this));
		
		// Bind uniform border properties to inner border
		// These will be disabled when per-edge mode is active
		_innerBorder.SetBinding(MauiBorder.StrokeProperty, new Binding(nameof(Stroke), source: this));
		_innerBorder.SetBinding(MauiBorder.StrokeThicknessProperty, new Binding(nameof(StrokeThickness), source: this));
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
		var strokeCap = BorderStrokeCap;

		// Top border
		if (BorderTopThickness > 0)
		{
			if (_topLine == null)
			{
				_topLine = CreateBorderLine(strokeCap);
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
		else
		{
			_topLine?.IsVisible = false;
		}

		// Bottom border
		if (BorderBottomThickness > 0)
		{
			if (_bottomLine == null)
			{
				_bottomLine = CreateBorderLine(strokeCap);
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
		else
		{
			_bottomLine?.IsVisible = false;
		}

		// Left border
		if (BorderLeftThickness > 0)
		{
			if (_leftLine == null)
			{
				_leftLine = CreateBorderLine(strokeCap);
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
		else
		{
			_leftLine?.IsVisible = false;
		}

		// Right border
		if (BorderRightThickness > 0)
		{
			if (_rightLine == null)
			{
				_rightLine = CreateBorderLine(strokeCap);
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
		else
		{
			_rightLine?.IsVisible = false;
		}
	}

	private Line CreateBorderLine(PenLineCap strokeCap)
	{
		return new Line
		{
			StrokeLineCap = strokeCap,
			InputTransparent = true,
			ZIndex = -1  // Render behind content (use negative ZIndex)
		};
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

	#region Border Shorthand Property
	/// <summary>
	/// Identifies the Border bindable property.
	/// </summary>
	/// <remarks>
	/// This property provides a string shorthand for defining per-edge borders.
	/// Syntax: "thickness color" values separated by commas.
	/// - 1 value: applies to all edges (e.g., "1 Black")
	/// - 2 values: vertical, horizontal (e.g., "1 Black, 2 Grey")
	/// - 4 values: top, right, bottom, left (e.g., "1 White, 3 Black, 3 Black, 1 White")
	/// </remarks>
	public static readonly BindableProperty BorderProperty = BindableProperty.Create(
		nameof(Border),
		typeof(string),
		typeof(FsBorder),
		null,
		propertyChanged: OnBorderShorthandChanged);

	/// <summary>
	/// Gets or sets the border using shorthand syntax.
	/// </summary>
	/// <remarks>
	/// Supports 1, 2, or 4 comma-separated values. Each value is "thickness color".
	/// Examples:
	/// - "1 Black" - uniform 1px black border
	/// - "1 Black, 2 Grey" - 1px black top/bottom, 2px grey left/right
	/// - "1 White, 3 Black, 3 Black, 1 White" - inset effect
	/// For advanced scenarios (gradients, etc.), use the explicit per-edge properties.
	/// </remarks>
	public string? Border
	{
		get => (string?)GetValue(BorderProperty);
		set => SetValue(BorderProperty, value);
	}

	private static void OnBorderShorthandChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is FsBorder border && newValue is string shorthand && !string.IsNullOrWhiteSpace(shorthand))
		{
			try
			{
				var parsed = BorderShorthand.Parse(shorthand);
				
				// Set thickness properties
				border.BorderTopThickness = parsed.Top.Thickness;
				border.BorderRightThickness = parsed.Right.Thickness;
				border.BorderBottomThickness = parsed.Bottom.Thickness;
				border.BorderLeftThickness = parsed.Left.Thickness;

				// Set brush properties
				border.BorderTopBrush = new SolidColorBrush(parsed.Top.Color);
				border.BorderRightBrush = new SolidColorBrush(parsed.Right.Color);
				border.BorderBottomBrush = new SolidColorBrush(parsed.Bottom.Color);
				border.BorderLeftBrush = new SolidColorBrush(parsed.Left.Color);
			}
			catch (ArgumentException ex)
			{
				// Log or handle parsing error
				System.Diagnostics.Debug.WriteLine($"Error parsing border shorthand: {ex.Message}");
			}
		}
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
		propertyChanged: OnBorderThicknessPropertyChanged);

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
		propertyChanged: OnBorderThicknessPropertyChanged);

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
		propertyChanged: OnBorderThicknessPropertyChanged);

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
		propertyChanged: OnBorderThicknessPropertyChanged);

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
		propertyChanged: OnBorderVisualPropertyChanged);

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
		propertyChanged: OnBorderVisualPropertyChanged);

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
		propertyChanged: OnBorderVisualPropertyChanged);

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
		propertyChanged: OnBorderVisualPropertyChanged);

	/// <summary>
	/// Gets or sets the brush for the left border.
	/// </summary>
	public Brush BorderLeftBrush
	{
		get => (Brush)GetValue(BorderLeftBrushProperty);
		set => SetValue(BorderLeftBrushProperty, value);
	}
	#endregion

	#region Uniform Border Properties
	/// <summary>
	/// Identifies the Stroke bindable property.
	/// </summary>
	/// <remarks>
	/// This property defines the brush used for uniform borders.
	/// When any per-edge border property is set, this property is ignored and per-edge mode is activated.
	/// </remarks>
	public static readonly BindableProperty StrokeProperty = BindableProperty.Create(
		nameof(Stroke),
		typeof(Brush),
		typeof(FsBorder),
		null,
		propertyChanged: OnUniformBorderPropertyChanged);

	/// <summary>
	/// Gets or sets the brush used for uniform borders.
	/// </summary>
	/// <remarks>
	/// This property is only active when no per-edge border properties are set.
	/// Setting any per-edge border activates per-edge mode and disables this property.
	/// Supports corner radius via StrokeShape.
	/// </remarks>
	public Brush? Stroke
	{
		get => (Brush?)GetValue(StrokeProperty);
		set => SetValue(StrokeProperty, value);
	}

	/// <summary>
	/// Identifies the StrokeThickness bindable property.
	/// </summary>
	/// <remarks>
	/// This property defines the thickness used for uniform borders.
	/// When any per-edge border property is set, this property is ignored and per-edge mode is activated.
	/// </remarks>
	public static readonly BindableProperty StrokeThicknessProperty = BindableProperty.Create(
		nameof(StrokeThickness),
		typeof(double),
		typeof(FsBorder),
		0d,
		propertyChanged: OnUniformBorderPropertyChanged);

	/// <summary>
	/// Gets or sets the thickness used for uniform borders.
	/// </summary>
	/// <remarks>
	/// This property is only active when no per-edge border properties are set.
	/// Setting any per-edge border activates per-edge mode and disables this property.
	/// </remarks>
	public double StrokeThickness
	{
		get => (double)GetValue(StrokeThicknessProperty);
		set => SetValue(StrokeThicknessProperty, value);
	}

	private static void OnUniformBorderPropertyChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is FsBorder border)
		{
			border.UpdateBorderMode();
		}
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
		propertyChanged: OnBorderVisualPropertyChanged);

	/// <summary>
	/// Gets or sets the stroke line cap for border lines.
	/// </summary>
	public PenLineCap BorderStrokeCap
	{
		get => (PenLineCap)GetValue(BorderStrokeCapProperty);
		set => SetValue(BorderStrokeCapProperty, value);
	}
	#endregion

	private static void OnBorderThicknessPropertyChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is FsBorder border)
		{
			// Per-edge thickness changes activate per-edge mode
			border.UpdateBorderMode();
			// Thickness changes can affect layout
			border.InvalidateMeasure();
		}
	}

	private static void OnBorderVisualPropertyChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is FsBorder border)
		{
			// Per-edge brush changes activate per-edge mode
			border.UpdateBorderMode();
			
			if (border.Width > 0 && border.Height > 0)
			{
				// Visual-only changes don't affect layout, just update border rendering
				border.UpdateBorderLines(border.Width, border.Height);
			}
		}
	}

	/// <summary>
	/// Determines whether per-edge border mode is active and updates the inner border accordingly.
	/// </summary>
	/// <remarks>
	/// Per-edge mode is active when any per-edge border property has a non-zero thickness.
	/// When active, the uniform border (Stroke/StrokeThickness) is disabled.
	/// When inactive, per-edge lines are hidden and uniform border is enabled.
	/// </remarks>
	private void UpdateBorderMode()
	{
		bool isPerEdgeMode = BorderTopThickness > 0 || BorderRightThickness > 0 ||
			BorderBottomThickness > 0 || BorderLeftThickness > 0;

		if (isPerEdgeMode)
		{
			// Per-edge mode: disable uniform border stroke
			_innerBorder.Stroke = Colors.Transparent;
			_innerBorder.StrokeThickness = 0;
		}
		else
		{
			// Uniform mode: use Stroke and StrokeThickness properties
			// The bindings will handle this automatically, so just ensure per-edge lines are hidden
			if (Width > 0 && Height > 0)
			{
				if (_topLine != null) _topLine.IsVisible = false;
				if (_rightLine != null) _rightLine.IsVisible = false;
				if (_bottomLine != null) _bottomLine.IsVisible = false;
				if (_leftLine != null) _leftLine.IsVisible = false;
			}
		}

		// Trigger re-render if size is allocated
		if (Width > 0 && Height > 0)
		{
			UpdateBorderLines(Width, Height);
		}
	}
}
