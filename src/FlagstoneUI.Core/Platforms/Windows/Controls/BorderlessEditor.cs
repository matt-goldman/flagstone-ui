namespace FlagstoneUI.Core.Controls;

public partial class BorderlessEditor
{
	partial void RegisterHandler()
	{
		Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping("Borderless", (handler, view) =>
		{
			if (view is BorderlessEditor)
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
