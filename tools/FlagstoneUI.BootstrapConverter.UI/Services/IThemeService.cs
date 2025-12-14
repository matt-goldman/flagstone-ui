using FlagstoneUI.BootstrapConverter.UI.Resources.Styles;

namespace FlagstoneUI.BootstrapConverter.UI.Services;

public interface IThemeService
{
	void ReloadThemes(ResourceDictionary dictionary);
}

internal class ThemeService : IThemeService
{
	public void ReloadThemes(ResourceDictionary dictionary)
	{
		// get app
		var app = Application.Current;

		if (app is null)
		{
			return;
		}

		// Define the core resource dictionary sources that should always be present
		var coreSources = new HashSet<string>
		{
			"Resources/Styles/Colors.xaml",
			"Resources/Styles/Styles.xaml",
			"Resources/Styles/SlatePro.xaml"
		};

		// Remove only non-core dictionaries (i.e., preview themes)
		var toRemove = app.Resources.MergedDictionaries
			.Where(d => d.Source != null && !coreSources.Contains(d.Source.OriginalString))
			.ToList();

		foreach (var dict in toRemove)
		{
			app.Resources.MergedDictionaries.Remove(dict);
		}

		// add preview theme
		app.Resources.MergedDictionaries.Add(dictionary);
	}
}
