using FlagstoneUI.BootstrapConverter.UI.Resources.Styles;
using Colors = FlagstoneUI.BootstrapConverter.UI.Resources.Styles.Colors;

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

		// clear existing merged dictionaries
		app.Resources.MergedDictionaries.Clear();

		// reload default resources
		app.Resources.MergedDictionaries.Add(new Colors());
		app.Resources.MergedDictionaries.Add(new Styles());
		app.Resources.MergedDictionaries.Add(new SlatePro());

		// add preview theme
		app.Resources.MergedDictionaries.Add(dictionary);
	}
}
