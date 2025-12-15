namespace FlagstoneUI.BootstrapConverter.UI.Controls;

public partial class EditorShowcase : ContentView
{
	public EditorShowcase()
	{
		InitializeComponent();
	}

	#region StyleName Bindable Property
	public static readonly BindableProperty StyleNameProperty =
		BindableProperty.Create(
			nameof(StyleName),
			typeof(string),
			typeof(EditorShowcase),
			string.Empty,
			propertyChanged: OnStyleNameChanged);

	public string StyleName
	{
		get => (string)GetValue(StyleNameProperty);
		set => SetValue(StyleNameProperty, value);
	}

	public static void OnStyleNameChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is EditorShowcase editorShowcase && newValue is string styleName)
		{
			var fullyQualifiedFsEditorName = typeof(FsEditor).FullName;

			if (styleName == fullyQualifiedFsEditorName)
			{
				styleName = "default";
			}

			editorShowcase.StyleNameLabel.Text = styleName;

			if (string.IsNullOrEmpty(styleName) || styleName == "default")
			{
				return;
			}

			var style = (Application.Current?.Resources?.TryGetValue(styleName, out var foundStyle) ?? false)
				? foundStyle as Style
				: null;

			if (style is not null)
			{
				editorShowcase.NormalEditor.Style = style;
				editorShowcase.DisabledEditor.Style = style;
			}
		}
	}
	#endregion
}
