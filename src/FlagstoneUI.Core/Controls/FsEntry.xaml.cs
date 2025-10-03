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
	public event EventHandler? Completed;
	void OnCompleted(object? sender, EventArgs e) => Completed?.Invoke(this, e);

	/// <summary>
	/// Occurs when the text value changes.
	/// </summary>
	/// <remarks>This event is raised whenever the text value is modified. Subscribers can use this event  to
	/// respond to changes in the text, such as updating the UI or performing validation.</remarks>
	public event EventHandler<TextChangedEventArgs>? TextChanged;
	void OnTextChanged(object? sender, TextChangedEventArgs e) => TextChanged?.Invoke(this, e);
    #endregion

    #region BorderColorProperty
	/// <summary>
	/// Identifies the BorderColor bindable property.
	/// </summary>
	/// <remarks>This property determines the color of the border for the <see cref="FsEntry"/> control.  The
	/// default value is <see cref="Colors.Transparent"/>.</remarks>
    public static readonly BindableProperty BorderColorProperty = BindableProperty.Create(
		nameof(BorderColor),
		typeof(Color),
		typeof(FsEntry),
		Colors.Transparent);

	/// <summary>
	/// Gets or sets the color of the border.
	/// </summary>
	/// <remarks>Setting this property updates the visual appearance of the border. Ensure the color is appropriate
	/// for the application's theme or design.</remarks>
	public Color BorderColor
	{
		get { return (Color)GetValue(BorderColorProperty); }
		set { SetValue(BorderColorProperty, value); }
	}
	#endregion

	#region BorderWidthProperty
	/// <summary>
	/// Identifies the BorderWidth bindable property.
	/// </summary>
	/// <remarks>This property specifies the width of the border for the control. The default value is 0.</remarks>
	public static readonly BindableProperty BorderWidthProperty = BindableProperty.Create(
		nameof(BorderWidth),
		typeof(double),
		typeof(FsEntry),
		0d,
		BindingMode.OneWay);

	/// <summary>
	/// Gets or sets the width of the border, in device-independent units (1/96th inch per unit).
	/// </summary>
	/// <remarks>A value of 0.0 indicates that the border is not visible. Values must be non-negative.</remarks>
	public double BorderWidth
	{
		get => (double)GetValue(BorderWidthProperty);
		set => SetValue(BorderWidthProperty, value);
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
			OnParentChanged();
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

    #region TextColorProperty
	/// <summary>
	/// Identifies the bindable property for the text color of the entry.
	/// </summary>
	/// <remarks>This property allows binding to the text color of the entry. The default value is <see
	/// cref="Colors.Black"/>. Changes to this property trigger the <c>OnTextColorChanged</c> callback.</remarks>
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

    #region BackgroundColorProperty
	/// <summary>
	/// Identifies the <see cref="BackgroundColor"/> bindable property.
	/// </summary>
	/// <remarks>This property allows binding to the background color of the <see cref="FsEntry"/> control. The
	/// default value is <see cref="Colors.Transparent"/>. Changes to this property will trigger the
	/// <c>OnBackgroundColorChanged</c> callback.</remarks>
	public new static readonly BindableProperty BackgroundColorProperty = BindableProperty.Create(
		nameof(BackgroundColor),
		typeof(Color),
		typeof(FsEntry),
		Colors.Transparent,
		BindingMode.OneWay);

	/// <summary>
	/// Gets or sets the background color of the element.
	/// </summary>
	public new Color BackgroundColor
	{
		get => (Color)GetValue(BackgroundColorProperty);
		set => SetValue(BackgroundColorProperty, value);
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
}

