namespace FlagstoneUI.Core.Controls;

public partial class BorderlessEntry : Entry
{
	public BorderlessEntry()
	{
		RegisterHandler();

        var transparentBackgroundSetter = new Setter
        {
            Property	= BackgroundColorProperty,
            Value		= Colors.Transparent
        };

		var focusedTrigger = new Trigger(typeof(BorderlessEntry))
		{
			Property	= IsFocusedProperty,
			Value		= true
		};
		focusedTrigger.Setters.Add(transparentBackgroundSetter);

		var hoverTrigger = new Trigger(typeof(BorderlessEntry))
		{
			Property = IsFocusedProperty,
			Value = true
		};
		hoverTrigger.Setters.Add(transparentBackgroundSetter);

        Triggers.Add(focusedTrigger);
		Triggers.Add(hoverTrigger);
    }
	
	partial void RegisterHandler();
}
