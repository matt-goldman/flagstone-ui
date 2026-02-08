using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Storage;
using Microsoft.Extensions.Logging;

namespace FlagstoneUI.BootstrapConverter.UI;

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
			})
			.UseMauiCommunityToolkit();

		builder.Services.AddSingleton<IFileSaver>(FileSaver.Default);
		builder.Services.AddHttpClient();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		builder.Services.AddSingleton<IFileService, FileService>();
		builder.Services.AddSingleton<IBootstrapService, BootstrapService>();
		builder.Services.AddSingleton<IThemeService, ThemeService>();

		builder.Services.AddSingleton<MainPage>();

		builder.Services.AddSingleton<MainViewModel>();

		return builder.Build();
	}
}
