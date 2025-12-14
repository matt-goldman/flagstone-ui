using CommunityToolkit.Mvvm.ComponentModel;

namespace FlagstoneUI.BootstrapConverter.UI.Controls;

public partial class ButtonShowcase : ContentView
{
	public ButtonShowcase()
	{
		InitializeComponent();
	}

	// TODO: replace with source generator when MAUI Community Toolkit releases it

	#region StyleName Bindable Property
	public static readonly BindableProperty StyleNameProperty =
		BindableProperty.Create(
			nameof(StyleName),
			typeof(string),
			typeof(ButtonShowcase),
			string.Empty);

	public string StyleName
	{
		get => (string)GetValue(StyleNameProperty);
		set => SetValue(StyleNameProperty, value);
	}

	public static void OnStyleNameChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is ButtonShowcase buttonShowcase && newValue is string styleName)
		{
			if (string.IsNullOrEmpty(styleName) || styleName == "default")
			{
				return;
			}

			var style = Application.Current?.Resources?.TryGetValue(styleName, out var foundStyle) == true
				? foundStyle as Style
				: null;

			if (style is not null)
			{
				buttonShowcase.StyleNameLabel.Text = styleName;
				buttonShowcase.NormalButton.Style = style;
				buttonShowcase.DisabledButton.Style = style;
			}
		}
	}
	#endregion
}
