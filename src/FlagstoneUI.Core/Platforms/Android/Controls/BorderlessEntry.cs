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

			handler.PlatformView.Background = null;
			handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent);
			handler.PlatformView.BackgroundTintList =
				Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
		});
	}
}
