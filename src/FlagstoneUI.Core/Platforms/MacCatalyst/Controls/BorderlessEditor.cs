namespace FlagstoneUI.Core.Controls;

public partial class BorderlessEditor
{
	private static bool _handlerRegistered;

	partial void RegisterHandler()
	{
		if (_handlerRegistered) return;
		_handlerRegistered = true;

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
