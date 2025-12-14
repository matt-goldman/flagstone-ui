using Microsoft.Maui.Controls.Shapes;

namespace FlagstoneUI.Core.Controls;

/// <summary>
/// Represents a customizable entry control with support for various visual and behavioral properties.
/// </summary>
/// <remarks>The <see cref="FsEntry"/> control extends the functionality of a standard entry field by providing
/// additional customization options such as border color, border width, corner radius, and text alignment. It also
/// supports events for text changes and completion, making it suitable for scenarios requiring enhanced user input
/// handling.  This control is bindable and supports data binding for its properties, making it easy to integrate into
/// MVVM-based applications.</remarks>
public partial class FsEntry : ContentView
{
	public FsEntry()
	{
		InitializeComponent();
		ViewWrapper.BindingContext = this;
		_borderShape = new RoundRectangle { CornerRadius = CornerRadius };
    }

    #region Events
	/// <summary>
	/// Occurs when the operation has completed.
	/// </summary>
	public event EventHandler? Completed;
	void OnCompleted(object? sender, EventArgs e) => Completed?.Invoke(this, e);

	/// <summary>
	/// Occurs when the text value changes.
	/// </summary>
	/// <remarks>This event is raised whenever the text value is modified. Subscribers can use this event  to
	/// respond to changes in the text, such as updating the UI or performing validation.</remarks>
	public event EventHandler<TextChangedEventArgs>? TextChanged;
	void OnTextChanged(object? sender, TextChangedEventArgs e) => TextChanged?.Invoke(this, e);

	/// <summary>
	/// Occurs when the control receives input focus.
	/// </summary>
	/// <remarks>Subscribe to this event to be notified when the control becomes the active element for user input.
	/// This event is typically raised when the user interacts with the control using the keyboard or mouse, or when focus
	/// is set programmatically.</remarks>
	public new event EventHandler? Focused;
	void OnFocused(object? sender, FocusEventArgs e) => Focused?.Invoke(this, e);

	/// <summary>
	/// Occurs when the control loses input focus.
	/// </summary>
	/// <remarks>Subscribe to this event to be notified when the control is no longer the active element. This event
	/// is typically used to perform actions such as validation or cleanup when the user navigates away from the
	/// control.</remarks>
    public new event EventHandler? Unfocused;
	void OnUnfocused(object? sender, FocusEventArgs e) => Unfocused?.Invoke(this, e);
    #endregion

	#region Per-Edge Border Thickness Properties
	/// <summary>
	/// Identifies the BorderTopThickness bindable property.
	/// </summary>
	public static readonly BindableProperty BorderTopThicknessProperty = BindableProperty.Create(
		nameof(BorderTopThickness), typeof(double), typeof(FsEntry), 0d);

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
		nameof(BorderRightThickness), typeof(double), typeof(FsEntry), 0d);

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
		nameof(BorderBottomThickness), typeof(double), typeof(FsEntry), 0d);

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
		nameof(BorderLeftThickness), typeof(double), typeof(FsEntry), 0d);

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
		nameof(BorderTopBrush), typeof(Brush), typeof(FsEntry), new SolidColorBrush(Colors.Transparent));

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
		nameof(BorderRightBrush), typeof(Brush), typeof(FsEntry), new SolidColorBrush(Colors.Transparent));

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
		nameof(BorderBottomBrush), typeof(Brush), typeof(FsEntry), new SolidColorBrush(Colors.Transparent));

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
		nameof(BorderLeftBrush), typeof(Brush), typeof(FsEntry), new SolidColorBrush(Colors.Transparent));

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
		nameof(BorderStrokeCap), typeof(PenLineCap), typeof(FsEntry), PenLineCap.Flat);

	/// <summary>
	/// Gets or sets the stroke line cap for border lines.
	/// </summary>
	public PenLineCap BorderStrokeCap
	{
		get => (PenLineCap)GetValue(BorderStrokeCapProperty);
		set => SetValue(BorderStrokeCapProperty, value);
	}
	#endregion

	#region BorderBrush Property
	/// <summary>
	/// Identifies the BorderBrush bindable property.
	/// </summary>
	/// <remarks>This property determines the brush used for the border of the <see cref="FsEntry"/> control.
	/// The default value is a transparent solid color brush. This property sets a uniform border on all edges.
	/// For per-edge borders (e.g., underlines, 3D effects), use the per-edge properties instead.</remarks>
	public static readonly BindableProperty BorderBrushProperty = BindableProperty.Create(
		nameof(BorderBrush),
		typeof(Brush),
		typeof(FsEntry),
		new SolidColorBrush(Colors.Transparent),
		propertyChanged: OnBorderBrushChanged);

	/// <summary>
	/// Gets or sets the brush used to paint the border.
	/// </summary>
	/// <remarks>Setting this property updates the visual appearance of the border. The brush can be a solid color,
	/// gradient, or other brush type. This property sets a uniform border on all edges.
	/// For per-edge control (e.g., underlines, 3D effects), use BorderTopBrush, BorderRightBrush, BorderBottomBrush, and BorderLeftBrush.
	/// Note: Using both this property and per-edge properties simultaneously may have unintended consequences.</remarks>
	public Brush BorderBrush
	{
		get { return (Brush)GetValue(BorderBrushProperty); }
		set { SetValue(BorderBrushProperty, value); }
	}

	private static void OnBorderBrushChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is FsEntry entry && newValue is Brush brush)
		{
			entry.BorderTopBrush = brush;
			entry.BorderRightBrush = brush;
			entry.BorderBottomBrush = brush;
			entry.BorderLeftBrush = brush;
		}
	}
	#endregion

	#region BorderWidth Property
	/// <summary>
	/// Identifies the BorderWidth bindable property.
	/// </summary>
	/// <remarks>This property specifies the width of the border for the control. The default value is 0.
	/// This property sets a uniform border width on all edges.
	/// For per-edge control (e.g., underlines, 3D effects), use the per-edge thickness properties.</remarks>
	public static readonly BindableProperty BorderWidthProperty = BindableProperty.Create(
		nameof(BorderWidth),
		typeof(double),
		typeof(FsEntry),
		0d,
		BindingMode.OneWay,
		propertyChanged: OnBorderWidthChanged);

	/// <summary>
	/// Gets or sets the width of the border, in device-independent units (1/96th inch per unit).
	/// </summary>
	/// <remarks>A value of 0.0 indicates that the border is not visible. Values must be non-negative.
	/// This property sets a uniform border width on all edges.
	/// For per-edge control (e.g., underlines, 3D effects), use BorderTopThickness, BorderRightThickness, BorderBottomThickness, and BorderLeftThickness.
	/// Note: Using both this property and per-edge properties simultaneously may have unintended consequences.</remarks>
	public double BorderWidth
	{
		get => (double)GetValue(BorderWidthProperty);
		set => SetValue(BorderWidthProperty, value);
    }

	private static void OnBorderWidthChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is FsEntry entry && newValue is double width)
		{
			entry.BorderTopThickness = width;
			entry.BorderRightThickness = width;
			entry.BorderBottomThickness = width;
			entry.BorderLeftThickness = width;
		}
	}
	#endregion

	#region CornerRadiusProperty
	/// <summary>
	/// Identifies the <see cref="CornerRadius"/> bindable property.
	/// </summary>
	/// <remarks>This property represents the corner radius of the control. The default value is 0.</remarks>
	public static readonly BindableProperty CornerRadiusProperty = BindableProperty.Create(
		nameof(CornerRadius),
		typeof(double),
		typeof(FsEntry),
		0d,
		BindingMode.OneWay,
		propertyChanged: OnCornerRadiusChanged);

	/// <summary>
	/// Gets or sets the radius of the corners for the element.
	/// </summary>
	/// <remarks>A larger value results in more rounded corners. Setting this property to 0 will produce square
	/// corners.</remarks>
	public double CornerRadius
	{
		get => (double)GetValue(CornerRadiusProperty);
		set => SetValue(CornerRadiusProperty, value);
	}

	public static void OnCornerRadiusChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is FsEntry entry && newValue is double value)
		{
			entry.BorderShape = new RoundRectangle
			{
				CornerRadius = value
			};
		}
	}

	private RoundRectangle _borderShape;
	public RoundRectangle BorderShape
	{
		get => _borderShape;
		
		set
		{
			_borderShape = value;
			OnPropertyChanged();
		}
	}
    #endregion

    #region TextProperty
    /// <summary>
    /// Identifies the bindable property for the <see cref="Text"/> property.
    /// </summary>
    /// <remarks>This property is used to enable data binding for the <see cref="Text"/> property of the <see
    /// cref="FsEntry"/> class. The default value is an empty string (<see cref="string.Empty"/>).</remarks>
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
		nameof(Text),
		typeof(string),
		typeof(FsEntry),
		string.Empty,
		BindingMode.TwoWay);

	/// <summary>
	/// Gets or sets the text content associated with this instance.
	/// </summary>
	public string Text
	{
		get => (string)GetValue(TextProperty);
		set => SetValue(TextProperty, value);
	}
	#endregion

	#region PlaceholderProperty
	/// <summary>
	/// Identifies the bindable property for the placeholder text of the entry.
	/// </summary>
	/// <remarks>This property is used to define the placeholder text displayed in the entry when no value is
	/// entered. It supports two-way data binding.</remarks>
	public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(
		nameof(Placeholder),
		typeof(string),
		typeof(FsEntry),
		string.Empty,
		BindingMode.TwoWay);

	/// <summary>
	/// Gets or sets the placeholder text displayed when the input field is empty.
	/// </summary>
	public string Placeholder
	{
		get => (string)GetValue(PlaceholderProperty);
		set => SetValue(PlaceholderProperty, value);
	}
    #endregion
    
    #region PlaceholderColorProperty
    /// <summary>
    /// Identifies the bindable property for the placeholder text color of the entry.
    /// </summary>
    /// <remarks>This property allows binding to the color of the placeholder text. The default value is <see
	/// cref="Colors.Gray"/>.</remarks>
    public static readonly BindableProperty PlaceholderColorProperty = BindableProperty.Create(
		nameof(PlaceholderColor),
		typeof(Color),
		typeof(FsEntry),
		Colors.Gray,
		BindingMode.OneWay);

    /// <summary>
    /// Gets or sets the color of the placeholder text displayed by the control.
    /// </summary>
    public Color PlaceholderColor
    {
	    get => (Color)GetValue(PlaceholderColorProperty);
	    set => SetValue(PlaceholderColorProperty, value);
    }
    #endregion

    #region TextColorProperty
	/// <summary>
	/// Identifies the bindable property for the text color of the entry.
	/// </summary>
	/// <remarks>This property allows binding to the text color of the entry. The default value is <see
	/// cref="Colors.Black"/>.</remarks>
	public static readonly BindableProperty TextColorProperty = BindableProperty.Create(
		nameof(TextColor),
		typeof(Color),
		typeof(FsEntry),
		Colors.Black,
		BindingMode.OneWay);

	/// <summary>
	/// Gets or sets the color of the text displayed by the control.
	/// </summary>
	public Color TextColor
		{
		get => (Color)GetValue(TextColorProperty);
		set => SetValue(TextColorProperty, value);
	}
    #endregion

	#region BackgroundProperty
	/// <summary>
	/// Identifies the <see cref="Background"/> bindable property.
	/// </summary>
	/// <remarks>This property allows binding to the background brush of the <see cref="FsEntry"/> control. The
	/// default value is a transparent solid color brush.</remarks>
	public new static readonly BindableProperty BackgroundProperty = BindableProperty.Create(
		nameof(Background),
		typeof(Brush),
		typeof(FsEntry),
		new SolidColorBrush(Colors.Transparent),
		BindingMode.OneWay);

	/// <summary>
	/// Gets or sets the brush that fills the background of the element.
	/// </summary>
	/// <remarks>The brush can be a solid color, gradient, or other brush type.</remarks>
	public new Brush Background
	{
		get => (Brush)GetValue(BackgroundProperty);
		set => SetValue(BackgroundProperty, value);
	}
	#endregion

	#region HorizontalTextAlignmentProperty
	/// <summary>
	/// Identifies the bindable property for the horizontal text alignment of the entry.
	/// </summary>
	/// <remarks>This property determines the horizontal alignment of the text within the entry.  The default value
	/// is <see cref="TextAlignment.Start"/>. This property supports one-way data binding.</remarks>
	public static readonly BindableProperty HorizontalTextAlignmentProperty = BindableProperty.Create(
		nameof(HorizontalTextAlignment),
		typeof(TextAlignment),
		typeof(FsEntry),
		TextAlignment.Start,
		BindingMode.OneWay);
	
	/// <summary>
	/// Gets or sets the horizontal alignment of the text within the element.
	/// </summary>
	public TextAlignment HorizontalTextAlignment
	{
		get => (TextAlignment)GetValue(HorizontalTextAlignmentProperty);
		set => SetValue(HorizontalTextAlignmentProperty, value);
	}
    #endregion

    #region VerticalTextAlignmentProperty
	/// <summary>
	/// Identifies the <see cref="VerticalTextAlignment"/> bindable property, which determines the vertical alignment of
	/// text within the control.
	/// </summary>
	/// <remarks>The default value for this property is <see cref="TextAlignment.Center"/>. This property supports
	/// one-way data binding.</remarks>
	public static readonly BindableProperty VerticalTextAlignmentProperty = BindableProperty.Create(
		nameof(VerticalTextAlignment),
		typeof(TextAlignment),
		typeof(FsEntry),
		TextAlignment.Center,
		BindingMode.OneWay);

	/// <summary>
	/// Gets or sets the vertical alignment of text within the element.
	/// </summary>
	public TextAlignment VerticalTextAlignment
    {
		get => (TextAlignment)GetValue(VerticalTextAlignmentProperty);
		set => SetValue(VerticalTextAlignmentProperty, value);
    }
    #endregion

    #region IsPasswordProperty
	/// <summary>
	/// Identifies the bindable property that determines whether the entry should mask its input as a password.
	/// </summary>
	/// <remarks>This property is used to indicate whether the input in the associated entry control should be
	/// obscured,  typically for password fields. The default value is <see langword="false"/>.</remarks>
	public static readonly BindableProperty IsPasswordProperty = BindableProperty.Create(
		nameof(IsPassword),
		typeof(bool),
		typeof(FsEntry),
		false,
		BindingMode.OneWay);

	/// <summary>
	/// Gets or sets a value indicating whether the input field is treated as a password field.
	/// </summary>
	public bool IsPassword
    {
		get => (bool)GetValue(IsPasswordProperty);
		set => SetValue(IsPasswordProperty, value);
    }
    #endregion

    #region KeyboardProperty
	/// <summary>
	/// Identifies the bindable property for the <see cref="Keyboard"/> property.
	/// </summary>
	/// <remarks>This property is used to define the keyboard type for the <see cref="FsEntry"/> control.  The
	/// default value is <see cref="Keyboard.Default"/>. The binding mode is set to <see
	/// cref="BindingMode.OneWay"/>.</remarks>
	public static readonly BindableProperty KeyboardProperty = BindableProperty.Create(
		nameof(Keyboard),
		typeof(Keyboard),
		typeof(FsEntry),
		Keyboard.Default,
		BindingMode.OneWay);

	/// <summary>
	/// Gets or sets the keyboard input behavior for the control.
	/// </summary>
	/// <remarks>Use this property to customize the keyboard input behavior for the control. For example, you can
	/// specify a numeric keyboard layout or other specialized input configurations.</remarks>
	public Keyboard Keyboard
    {
		get => (Keyboard)GetValue(KeyboardProperty);
		set => SetValue(KeyboardProperty, value);
    }
    #endregion

    #region FontSizeProperty
	/// <summary>
	/// Identifies the FontSize bindable property.
	/// </summary>
	/// <remarks>This property specifies the font size for the text displayed in the <see cref="FsEntry"/> control. 
	/// The default value is 14.0. The property supports one-way data binding.</remarks>
    public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(
		nameof(FontSize),
		typeof(double),
		typeof(FsEntry),
		14.0d,
		BindingMode.OneWay);
	
	/// <summary>
	/// Gets or sets the font size for the text content.
	/// </summary>
	/// <remarks>Setting this property updates the visual appearance of the text to reflect the specified font size.
	/// Ensure the value is a positive number to avoid unexpected behavior.</remarks>
	public double FontSize
	{
		get => (double)GetValue(FontSizeProperty);
		set => SetValue(FontSizeProperty, value);
	}
    #endregion

    #region PaddingProperty
	/// <summary>
	/// Identifies the <see cref="Padding"/> bindable property.
	/// </summary>
	/// <remarks>This property represents the padding applied to the <see cref="FsEntry"/> control.  The default
	/// value is a <see cref="Thickness"/> of 5. The property supports one-way binding.</remarks>
	public new static readonly BindableProperty PaddingProperty = BindableProperty.Create(
		nameof(Padding),
		typeof(Thickness),
		typeof(FsEntry),
		new Thickness(5),
		BindingMode.OneWay);

	/// <summary>
	/// Gets or sets the padding inside the element.
	/// </summary>
	/// <remarks>The padding determines the spacing between the content of the element and its border. This property
	/// is typically used to adjust the layout of the element's content.</remarks>
	public new Thickness Padding
	{
		get => (Thickness)GetValue(PaddingProperty);
		set => SetValue(PaddingProperty, value);
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
		typeof(FsEntry),
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
		if (bindable is FsEntry entry && newValue is string shorthand && !string.IsNullOrWhiteSpace(shorthand))
		{
			try
			{
				var parsed = BorderShorthand.Parse(shorthand);
				
				// Set thickness properties
				entry.BorderTopThickness = parsed.Top.Thickness;
				entry.BorderRightThickness = parsed.Right.Thickness;
				entry.BorderBottomThickness = parsed.Bottom.Thickness;
				entry.BorderLeftThickness = parsed.Left.Thickness;

				// Set brush properties
				entry.BorderTopBrush = new SolidColorBrush(parsed.Top.Color);
				entry.BorderRightBrush = new SolidColorBrush(parsed.Right.Color);
				entry.BorderBottomBrush = new SolidColorBrush(parsed.Bottom.Color);
				entry.BorderLeftBrush = new SolidColorBrush(parsed.Left.Color);
			}
			catch (ArgumentException ex)
			{
				// Log or handle parsing error
				System.Diagnostics.Debug.WriteLine($"Error parsing border shorthand: {ex.Message}");
			}
		}
	}
	#endregion

	private void BorderlessEntry_Focused(object sender, FocusEventArgs e)
	{
		VisualStateManager.GoToState(this, VisualStateManager.CommonStates.Focused);
		Focused?.Invoke(this, EventArgs.Empty);
	}

	private void BorderlessEntry_Unfocused(object sender, FocusEventArgs e)
	{
		VisualStateManager.GoToState(this, VisualStateManager.CommonStates.Normal);
		Unfocused?.Invoke(this, EventArgs.Empty);
	}
}
