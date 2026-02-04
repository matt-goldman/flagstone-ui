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
			if (view is not BorderlessEntry)
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
