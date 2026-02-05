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

			handler.PlatformView.Background = null;
			handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent);
			handler.PlatformView.BackgroundTintList = 
				Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
			
			// Remove native padding that can cause text clipping
			handler.PlatformView.SetPadding(0, 0, 0, 0);
		});
	}
}
