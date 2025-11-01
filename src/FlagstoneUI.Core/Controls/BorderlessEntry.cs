namespace FlagstoneUI.Core.Controls;

public partial class BorderlessEntry : Entry
{
	public BorderlessEntry()
	{
		RegisterHandler();

		var transparentBackgroundSetter = new Setter
		{
			Property = BackgroundColorProperty,
			Value = Colors.Transparent
		};

		var focusedTrigger = new Trigger(typeof(BorderlessEntry))
		{
			Property = IsFocusedProperty,
			Value = true
		};
		focusedTrigger.Setters.Add(transparentBackgroundSetter);

		Triggers.Add(focusedTrigger);
	}

	partial void RegisterHandler();
}
