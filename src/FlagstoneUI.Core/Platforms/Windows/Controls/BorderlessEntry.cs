namespace FlagstoneUI.Core.Controls;

public partial class BorderlessEntry
{
	private static bool _handlerRegistered;

	partial void RegisterHandler()
	{
		if (_handlerRegistered) return;
		_handlerRegistered = true;

		Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("Borderless", (handler, view) =>
		{
			if (view is BorderlessEntry)
			{
				handler.PlatformView.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
				handler.PlatformView.Background = null;
				handler.PlatformView.FocusVisualMargin = new Microsoft.UI.Xaml.Thickness(0);

				// Disable all focus visuals (which may trigger on hover too)
				handler.PlatformView.UseSystemFocusVisuals = false;
			}
		});
	}
}
