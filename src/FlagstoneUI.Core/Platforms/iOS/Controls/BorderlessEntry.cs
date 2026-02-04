namespace FlagstoneUI.Core.Controls;

public partial class BorderlessEntry
{
	partial void RegisterHandler()
	{
		Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("Borderless", (handler, view) =>
		{
			if (view is not BorderlessEntry)
			{
				return;
			}

			handler.PlatformView.BackgroundColor = UIKit.UIColor.Clear;
			handler.PlatformView.Layer.BorderWidth = 0;
			handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
		});
	}
}
