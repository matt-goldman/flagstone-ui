namespace FlagstoneUI.Core.Controls;

public partial class BorderlessEditor : Editor
{
	public BorderlessEditor()
	{
		var transparentBackgroundSetter = new Setter
		{
			Property = BackgroundColorProperty,
			Value = Colors.Transparent
		};

		var focusedTrigger = new Trigger(typeof(BorderlessEditor))
		{
			Property = IsFocusedProperty,
			Value = true
		};
		focusedTrigger.Setters.Add(transparentBackgroundSetter);

		var hoverTrigger = new Trigger(typeof(BorderlessEditor))
		{
			Property = IsFocusedProperty,
			Value = true
		};
		hoverTrigger.Setters.Add(transparentBackgroundSetter);

		Triggers.Add(focusedTrigger);
		Triggers.Add(hoverTrigger);
	}

	internal static partial void RegisterHandler();

#if !(ANDROID || WINDOWS || IOS)
	internal static partial void RegisterHandler() { }
#endif
}
