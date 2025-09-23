using Microsoft.Maui.Controls.Shapes;

namespace FlagstoneUI.Core.Controls;

public partial class FsEntry : ContentView
{
	public FsEntry()
	{
		InitializeComponent();
		ViewWrapper.BindingContext = this;
	}
	
#region BorderColorProperty
	public static readonly BindableProperty BorderColorProperty = BindableProperty.Create(
		nameof(BorderColor),
		typeof(Color),
		typeof(FsEntry),
		Colors.Transparent,
		propertyChanged: OnBorderColorChanged);

	static void OnBorderColorChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is FsEntry entry && newValue is Color borderColor)
		{
			entry.WrapperBorder.Stroke = borderColor;
		}
	}

	public Color BorderColor
	{
		get { return (Color)GetValue(BorderColorProperty); }
		set { SetValue(BorderColorProperty, value); }
	}
#endregion

#region BorderWidthProperty
	public static readonly BindableProperty BorderWidthProperty = BindableProperty.Create(
		nameof(BorderWidth),
		typeof(double),
		typeof(FsEntry),
		0,
		BindingMode.OneWay,
		propertyChanged: OnBorderWidthChanged);

	static void OnBorderWidthChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is FsEntry entry && newValue is double value)
		{
			entry.WrapperBorder.StrokeThickness = value;
		}
	}

	public double BorderWidth { get; set; }
#endregion

#region CornerRadiusProperty
	public static readonly BindableProperty CornderRadiusProperty = BindableProperty.Create(
		nameof(CornerRadius),
		typeof(double),
		typeof(FsEntry),
		0,
		BindingMode.OneWay,
		propertyChanged: OnCornerRadiusChanged);

	public double CornerRadius
	{
		get => (double)GetValue(CornderRadiusProperty);
		set => SetValue(CornderRadiusProperty, value);
	}

	public static void OnCornerRadiusChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is FsEntry entry && newValue is double value)
		{
			var shape = new RoundRectangle();
			shape.CornerRadius = value;
			entry.WrapperBorder.StrokeShape = shape;
		}
	}
#endregion

#region ValueProperty
	public static readonly BindableProperty ValueProperty = BindableProperty.Create(
		nameof(Value),
		typeof(string),
		typeof(FsEntry),
		string.Empty,
		BindingMode.TwoWay);

	public string Value
	{
		get => (string)GetValue(ValueProperty);
		set => SetValue(ValueProperty, value);
	}
#endregion

#region PlaceholderProperty
	public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(
		nameof(Placeholder),
		typeof(string),
		typeof(FsEntry),
		string.Empty,
		BindingMode.TwoWay);

	public string Placeholder
	{
		get => (string)GetValue(PlaceholderProperty);
		set => SetValue(PlaceholderProperty, value);
	}
#endregion
}

