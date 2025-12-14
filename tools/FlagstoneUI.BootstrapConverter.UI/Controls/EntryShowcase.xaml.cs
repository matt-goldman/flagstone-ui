namespace FlagstoneUI.BootstrapConverter.UI.Controls;

public partial class EntryShowcase : ContentView
{
	public EntryShowcase()
	{
		InitializeComponent();
	}

	#region StyleName Bindable Property
	public static readonly BindableProperty StyleNameProperty =
		BindableProperty.Create(
			nameof(StyleName),
			typeof(string),
			typeof(EntryShowcase),
			string.Empty,
			propertyChanged: OnStyleNameChanged);

	public string StyleName
	{
		get => (string)GetValue(StyleNameProperty);
		set => SetValue(StyleNameProperty, value);
	}

	public static void OnStyleNameChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is EntryShowcase entryShowcase && newValue is string styleName)
		{
			var fullyQualifiedFsEntryName = typeof(FsEntry).FullName;

			if (styleName == fullyQualifiedFsEntryName)
			{
				styleName = "default";
			}

			entryShowcase.StyleNameLabel.Text = styleName;

			if (string.IsNullOrEmpty(styleName) || styleName == "default")
			{
				return;
			}

			var style = Application.Current?.Resources?.TryGetValue(styleName, out var foundStyle) == true
				? foundStyle as Style
				: null;

			if (style is not null)
			{
				entryShowcase.NormalEntry.Style = style;
				entryShowcase.DisabledEntry.Style = style;
			}
		}
	}
	#endregion
}
