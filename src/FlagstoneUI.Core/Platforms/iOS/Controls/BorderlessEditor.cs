namespace FlagstoneUI.Core.Controls;

public partial class BorderlessEditor
{
	internal static partial void RegisterHandler()
	{
		Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping("Borderless", (handler, view) =>
		{
			if (view is not BorderlessEditor)
			{
				return;
			}

			handler.PlatformView.BackgroundColor = UIKit.UIColor.Clear;
			handler.PlatformView.Layer.BorderWidth = 0;
		});
	}
}
