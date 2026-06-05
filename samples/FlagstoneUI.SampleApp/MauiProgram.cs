using CommunityToolkit.Maui;
using FlagstoneUI.Core.Builders;
using Microsoft.Extensions.Logging;
#if DEBUG
using Microsoft.Maui.DevFlow.Agent;
#endif

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
		builder.AddMauiDevFlowAgent();
#endif

		return builder.Build();
	}
}
