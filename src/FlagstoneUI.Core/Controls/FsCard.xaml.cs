
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
        nameof(Elevation), typeof(int), typeof(FsCard), 0);

    /// <summary>
    /// Gets or sets the elevation value, typically representing the height or depth of an element.
    /// </summary>
    /// <remarks>This property is a dependency property, which means it supports data binding, styling, and
    /// default values.</remarks>
    public int Elevation
    {
        get => (int)GetValue(ElevationProperty);
        set => SetValue(ElevationProperty, value);
    }

    // TODO: add propertyChanged handler when Shadow implementation is available
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

    #region BorderColor Property
    /// <summary>
    /// Identifies the <see cref="BorderColor"/> bindable property.
    /// </summary>
    /// <remarks>This property is used to define the border color of the <see cref="FsCard"/> control.  The
    /// default value is <see langword="null"/>, which indicates no specific border color is set.</remarks>
    public static readonly BindableProperty BorderColorProperty = BindableProperty.Create(
        nameof(BorderColor), typeof(Color), typeof(FsCard), null);  

    /// <summary>
    /// Gets or sets the color of the border.
    /// </summary>
    public Color BorderColor
    {
        get => (Color)GetValue(BorderColorProperty);
        set => SetValue(BorderColorProperty, value);
    }
    #endregion

    #region BackgroundColor Property
    /// <summary>
    /// Identifies the bindable property for the <see cref="BackgroundColor"/> property.
    /// </summary>
    /// <remarks>This property allows the background color of the <see cref="FsCard"/> to be set or retrieved.
    /// The default value is <see cref="Colors.Transparent"/>.</remarks>
    public new static readonly BindableProperty BackgroundColorProperty = BindableProperty.Create(
        nameof(BackgroundColor), typeof(Color), typeof(FsCard), Colors.Transparent);

    public new Color BackgroundColor
    {
        get => (Color)GetValue(BackgroundColorProperty);
        set => SetValue(BackgroundColorProperty, value);
    }
    #endregion

    #region BorderWidth Property
    /// <summary>
    /// Gets or sets the width of the border surrounding the card.
    /// </summary>
    public static readonly BindableProperty BorderWidthProperty = BindableProperty.Create(
        nameof(BorderWidth), typeof(double), typeof(FsCard), 1.0);

    /// <summary>
    /// Gets or sets the width of the border, in device-independent units (1/96th inch per unit).
    /// </summary>
    /// <remarks>A value of 0.0 indicates that the border is not visible. Negative values are not
    /// allowed.</remarks>
    public double BorderWidth
    {
        get => (double)GetValue(BorderWidthProperty);
        set => SetValue(BorderWidthProperty, value);
    }
    #endregion
}
