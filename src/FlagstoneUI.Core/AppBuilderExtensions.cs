using FlagstoneUI.Core.Controls;

namespace FlagstoneUI.Core;

public static class AppBuilderExtensions
{
	public static MauiAppBuilder UseFlagstoneUI(this MauiAppBuilder builder)
	{
		builder.ConfigureMauiHandlers(h =>
		{
			BorderlessEditor.RegisterHandler();
			BorderlessEntry.RegisterHandler();
		});

		return builder;
	}
}
