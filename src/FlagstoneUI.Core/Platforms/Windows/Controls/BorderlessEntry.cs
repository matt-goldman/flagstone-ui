namespace FlagstoneUI.Core.Controls;

public partial class BorderlessEntry
{
	partial void RegisterHandler()
	{
		Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("Borderless", (handler, view) =>
		{
			if (view is BorderlessEntry)
			{
				handler.PlatformView.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
                handler.PlatformView.Background = null;
                handler.PlatformView.FocusVisualMargin = new Microsoft.UI.Xaml.Thickness(0);
			}
		});
	}
}
