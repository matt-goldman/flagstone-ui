using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using FlagstoneUI.Core;

namespace FlagstoneUI.SampleApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
				fonts.AddFont("FluentSystemIcons-Filled.ttf", "FluentIcons");
			})
			.UseMauiCommunityToolkit()
			.UseFlagstoneUI();
		

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
