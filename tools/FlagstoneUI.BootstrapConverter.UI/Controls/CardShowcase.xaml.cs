namespace FlagstoneUI.BootstrapConverter.UI.Controls;

public partial class CardShowcase : ContentView
{
	public CardShowcase()
	{
		InitializeComponent();
	}

	#region StyleName Bindable Property
	public static readonly BindableProperty StyleNameProperty =
		BindableProperty.Create(
			nameof(StyleName),
			typeof(string),
			typeof(CardShowcase),
			string.Empty,
			propertyChanged: OnStyleNameChanged);

	public string StyleName
	{
		get => (string)GetValue(StyleNameProperty);
		set => SetValue(StyleNameProperty, value);
	}

	public static void OnStyleNameChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is CardShowcase cardShowcase && newValue is string styleName)
		{
			var fullyQualifiedFsCardName = typeof(FsCard).FullName;

			if (styleName == fullyQualifiedFsCardName)
			{
				styleName = "default";
			}

			cardShowcase.StyleNameLabel.Text = styleName;

			if (string.IsNullOrEmpty(styleName) || styleName == "default")
			{
				return;
			}

			var style = (Application.Current?.Resources?.TryGetValue(styleName, out var foundStyle) ?? false)
				? foundStyle as Style
				: null;

			if (style is not null)
			{
				cardShowcase.NormalCard.Style = style;
				cardShowcase.DisabledCard.Style = style;
			}
		}
	}
	#endregion
}
