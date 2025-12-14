using Microsoft.Maui.Controls.Shapes;

namespace FlagstoneUI.Core.Controls;

/// <summary>
/// Represents a customizable card view with support for elevation, corner radius, and border color.
/// </summary>
/// <remarks>The <see cref="FsCard"/> class provides a container for displaying content with visual styling options.
/// It supports elevation for shadow effects, rounded corners, and a configurable border color.</remarks>
public partial class FsCard : ContentView
{
	public FsCard()
	{
		InitializeComponent();
    }

	#region Elevation Property
	/// <summary>
	/// Identifies the bindable property for the <see cref="Elevation"/> property.
	/// </summary>
	/// <remarks>This property is used to define the elevation level of the <see cref="FsCard"/>.  The default
	/// value is <c>0</c>.</remarks>
	public static readonly BindableProperty ElevationProperty = BindableProperty.Create(
        nameof(Elevation), typeof(int), typeof(FsCard), 0, propertyChanged: OnElevationChanged);

    /// <summary>
    /// Gets or sets the elevation value, typically representing the height or depth of an element.
    /// </summary>
    /// <remarks>This property is a dependency property, which means it supports data binding, styling, and
    /// default values. Higher elevation values create larger shadows for a greater sense of depth.</remarks>
    public int Elevation
    {
        get => (int)GetValue(ElevationProperty);
        set => SetValue(ElevationProperty, value);
    }

	private static void OnElevationChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is FsCard card)
		{
			card.UpdateShadow();
		}
	}

	private void UpdateShadow()
	{
		// Map elevation levels to shadow parameters based on Material Design 3 specifications
		// Elevation 0: No shadow
		// Elevation 1-5: Progressively larger shadows
		if (Elevation <= 0)
		{
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type
			Shadow = null;
#pragma warning restore CS8625
		}
		else
		{
			// Calculate shadow parameters based on elevation level
			// These values are based on Material Design 3 elevation specifications
			var radius = Elevation * 2.0f;
			var offsetY = Elevation * 1.0f;
			var opacity = Math.Min(0.2f + (Elevation * 0.05f), 0.4f);

			Shadow = new Shadow
			{
				Brush = new SolidColorBrush(Colors.Black),
				Offset = new Point(0, offsetY),
				Radius = radius,
				Opacity = opacity
			};
		}
	}
    #endregion

    #region CornerRadius Property
    /// <summary>
    /// Identifies the <see cref="CornerRadius"/> bindable property.
    /// </summary>
    /// <remarks>This property represents the corner radius of the <see cref="FsCard"/> control.  The default
    /// value is <c>0.0</c>.</remarks>
    public static readonly BindableProperty CornerRadiusProperty = BindableProperty.Create(
        nameof(CornerRadius), typeof(double), typeof(FsCard), 0.0);

	/// <summary>
	/// Gets or sets the corner radius of the element.
	/// </summary>
	/// <remarks>A larger value results in more rounded corners. Negative values are not allowed and will
	/// throw an exception.</remarks>
	public double CornerRadius
    {
        get => (double)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    #endregion

	#region BackgroundBrush Property
	/// <summary>
	/// Identifies the bindable property for the <see cref="BackgroundBrush"/> property.
	/// </summary>
	/// <remarks>This property allows the background brush of the <see cref="FsCard"/> to be set or retrieved.
	/// The default value is a transparent solid color brush.</remarks>
	public static readonly BindableProperty BackgroundBrushProperty = BindableProperty.Create(
		nameof(BackgroundBrush), typeof(Brush), typeof(FsCard), new SolidColorBrush(Colors.Transparent));

	public Brush BackgroundBrush
	{
		get => (Brush)GetValue(BackgroundBrushProperty);
		set => SetValue(BackgroundBrushProperty, value);
	}
	#endregion

    #region BackgroundColor Property (Backward Compatibility)
    /// <summary>
    /// Identifies the bindable property for the <see cref="BackgroundColor"/> property.
    /// </summary>
    /// <remarks>This property allows the background color of the <see cref="FsCard"/> to be set or retrieved.
    /// The default value is <see cref="Colors.Transparent"/>. This property is maintained for backward compatibility.</remarks>
    public new static readonly BindableProperty BackgroundColorProperty = BindableProperty.Create(
        nameof(BackgroundColor), typeof(Color), typeof(FsCard), Colors.Transparent, propertyChanged: OnBackgroundColorChanged);

    public new Color BackgroundColor
    {
        get => (Color)GetValue(BackgroundColorProperty);
        set => SetValue(BackgroundColorProperty, value);
    }

	private static void OnBackgroundColorChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is FsCard card && newValue is Color color)
		{
			card.BackgroundBrush = new SolidColorBrush(color);
		}
	}
    #endregion

	#region Per-Edge Border Thickness Properties
	/// <summary>
	/// Identifies the BorderTopThickness bindable property.
	/// </summary>
	public static readonly BindableProperty BorderTopThicknessProperty = BindableProperty.Create(
		nameof(BorderTopThickness), typeof(double), typeof(FsCard), 0.0);

	/// <summary>
	/// Gets or sets the thickness of the top border.
	/// </summary>
	public double BorderTopThickness
	{
		get => (double)GetValue(BorderTopThicknessProperty);
		set => SetValue(BorderTopThicknessProperty, value);
	}

	/// <summary>
	/// Identifies the BorderRightThickness bindable property.
	/// </summary>
	public static readonly BindableProperty BorderRightThicknessProperty = BindableProperty.Create(
		nameof(BorderRightThickness), typeof(double), typeof(FsCard), 0.0);

	/// <summary>
	/// Gets or sets the thickness of the right border.
	/// </summary>
	public double BorderRightThickness
	{
		get => (double)GetValue(BorderRightThicknessProperty);
		set => SetValue(BorderRightThicknessProperty, value);
	}

	/// <summary>
	/// Identifies the BorderBottomThickness bindable property.
	/// </summary>
	public static readonly BindableProperty BorderBottomThicknessProperty = BindableProperty.Create(
		nameof(BorderBottomThickness), typeof(double), typeof(FsCard), 0.0);

	/// <summary>
	/// Gets or sets the thickness of the bottom border.
	/// </summary>
	public double BorderBottomThickness
	{
		get => (double)GetValue(BorderBottomThicknessProperty);
		set => SetValue(BorderBottomThicknessProperty, value);
	}

	/// <summary>
	/// Identifies the BorderLeftThickness bindable property.
	/// </summary>
	public static readonly BindableProperty BorderLeftThicknessProperty = BindableProperty.Create(
		nameof(BorderLeftThickness), typeof(double), typeof(FsCard), 0.0);

	/// <summary>
	/// Gets or sets the thickness of the left border.
	/// </summary>
	public double BorderLeftThickness
	{
		get => (double)GetValue(BorderLeftThicknessProperty);
		set => SetValue(BorderLeftThicknessProperty, value);
	}
	#endregion

	#region Per-Edge Border Brush Properties
	/// <summary>
	/// Identifies the BorderTopBrush bindable property.
	/// </summary>
	public static readonly BindableProperty BorderTopBrushProperty = BindableProperty.Create(
		nameof(BorderTopBrush), typeof(Brush), typeof(FsCard), new SolidColorBrush(Colors.Transparent));

	/// <summary>
	/// Gets or sets the brush for the top border.
	/// </summary>
	public Brush BorderTopBrush
	{
		get => (Brush)GetValue(BorderTopBrushProperty);
		set => SetValue(BorderTopBrushProperty, value);
	}

	/// <summary>
	/// Identifies the BorderRightBrush bindable property.
	/// </summary>
	public static readonly BindableProperty BorderRightBrushProperty = BindableProperty.Create(
		nameof(BorderRightBrush), typeof(Brush), typeof(FsCard), new SolidColorBrush(Colors.Transparent));

	/// <summary>
	/// Gets or sets the brush for the right border.
	/// </summary>
	public Brush BorderRightBrush
	{
		get => (Brush)GetValue(BorderRightBrushProperty);
		set => SetValue(BorderRightBrushProperty, value);
	}

	/// <summary>
	/// Identifies the BorderBottomBrush bindable property.
	/// </summary>
	public static readonly BindableProperty BorderBottomBrushProperty = BindableProperty.Create(
		nameof(BorderBottomBrush), typeof(Brush), typeof(FsCard), new SolidColorBrush(Colors.Transparent));

	/// <summary>
	/// Gets or sets the brush for the bottom border.
	/// </summary>
	public Brush BorderBottomBrush
	{
		get => (Brush)GetValue(BorderBottomBrushProperty);
		set => SetValue(BorderBottomBrushProperty, value);
	}

	/// <summary>
	/// Identifies the BorderLeftBrush bindable property.
	/// </summary>
	public static readonly BindableProperty BorderLeftBrushProperty = BindableProperty.Create(
		nameof(BorderLeftBrush), typeof(Brush), typeof(FsCard), new SolidColorBrush(Colors.Transparent));

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
		nameof(BorderStrokeCap), typeof(PenLineCap), typeof(FsCard), PenLineCap.Flat);

	/// <summary>
	/// Gets or sets the stroke line cap for border lines.
	/// </summary>
	public PenLineCap BorderStrokeCap
	{
		get => (PenLineCap)GetValue(BorderStrokeCapProperty);
		set => SetValue(BorderStrokeCapProperty, value);
	}
	#endregion

    #region BorderColor Property
    /// <summary>
    /// Identifies the <see cref="BorderColor"/> bindable property.
    /// </summary>
    /// <remarks>This property is used to define the border color of the <see cref="FsCard"/> control.  The
    /// default value is <see langword="null"/>, which indicates no specific border color is set.
    /// This property sets a uniform border on all edges. For per-edge borders (e.g., underlines, 3D effects),
    /// use the per-edge properties instead.</remarks>
    public static readonly BindableProperty BorderColorProperty = BindableProperty.Create(
        nameof(BorderColor), typeof(Color), typeof(FsCard), null, propertyChanged: OnBorderColorChanged);  

    /// <summary>
    /// Gets or sets the color of the border.
    /// </summary>
    /// <remarks>This property sets a uniform border color on all edges. 
    /// For per-edge control (e.g., underlines, 3D effects), use BorderTopBrush, BorderRightBrush, BorderBottomBrush, and BorderLeftBrush.
    /// Note: Using both this property and per-edge properties simultaneously may have unintended consequences.</remarks>
    public Color BorderColor
    {
        get => (Color)GetValue(BorderColorProperty);
        set => SetValue(BorderColorProperty, value);
    }

	private static void OnBorderColorChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is FsCard card && newValue is Color color)
		{
			var brush = new SolidColorBrush(color);
			card.BorderTopBrush = brush;
			card.BorderRightBrush = brush;
			card.BorderBottomBrush = brush;
			card.BorderLeftBrush = brush;
		}
	}
    #endregion

    #region BorderWidth Property
    /// <summary>
    /// Gets or sets the width of the border surrounding the card.
    /// </summary>
    /// <remarks>This property sets a uniform border width on all edges.
    /// For per-edge control (e.g., underlines, 3D effects), use BorderTopThickness, BorderRightThickness, BorderBottomThickness, and BorderLeftThickness.</remarks>
    public static readonly BindableProperty BorderWidthProperty = BindableProperty.Create(
        nameof(BorderWidth), typeof(double), typeof(FsCard), 1.0, propertyChanged: OnBorderWidthChanged);

    /// <summary>
    /// Gets or sets the width of the border, in device-independent units (1/96th inch per unit).
    /// </summary>
    /// <remarks>A value of 0.0 indicates that the border is not visible. Negative values are not allowed. 
    /// This property sets a uniform border width on all edges.
    /// For per-edge control (e.g., underlines, 3D effects), use the per-edge thickness properties.
    /// Note: Using both this property and per-edge properties simultaneously may have unintended consequences.</remarks>
    public double BorderWidth
    {
        get => (double)GetValue(BorderWidthProperty);
        set => SetValue(BorderWidthProperty, value);
    }

	private static void OnBorderWidthChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is FsCard card && newValue is double width)
		{
			card.BorderTopThickness = width;
			card.BorderRightThickness = width;
			card.BorderBottomThickness = width;
			card.BorderLeftThickness = width;
		}
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
		typeof(FsCard),
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
		if (bindable is FsCard card && newValue is string shorthand && !string.IsNullOrWhiteSpace(shorthand))
		{
			try
			{
				var parsed = BorderShorthand.Parse(shorthand);
				
				// Set thickness properties
				card.BorderTopThickness = parsed.Top.Thickness;
				card.BorderRightThickness = parsed.Right.Thickness;
				card.BorderBottomThickness = parsed.Bottom.Thickness;
				card.BorderLeftThickness = parsed.Left.Thickness;

				// Set brush properties
				card.BorderTopBrush = new SolidColorBrush(parsed.Top.Color);
				card.BorderRightBrush = new SolidColorBrush(parsed.Right.Color);
				card.BorderBottomBrush = new SolidColorBrush(parsed.Bottom.Color);
				card.BorderLeftBrush = new SolidColorBrush(parsed.Left.Color);
			}
			catch (ArgumentException ex)
			{
				// Log or handle parsing error
				System.Diagnostics.Debug.WriteLine($"Error parsing border shorthand: {ex.Message}");
			}
		}
	}
	#endregion
}
