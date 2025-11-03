using FlagstoneUI.Core.Themes;
using FlagstoneUI.SampleApp.Resources.Styles;

namespace FlagstoneUI.SampleApp;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}

	public static void SwitchTheme(string theme)
	{
		var newTheme = Themes.FirstOrDefault(t => t.Name == theme);

		if (newTheme is null)
		{
			return;
		}

		Current!.Resources.MergedDictionaries.Clear();

		// re-add default styles and colors
		Current!.Resources.MergedDictionaries.Add(new Resources.Styles.Colors());
        Current.Resources.MergedDictionaries.Add(new Styles());

        Current.Resources.MergedDictionaries.Add(newTheme.Tokens);
    }

	//private static readonly ResourceDictionary _colors = new Resources.Styles.Colors();
	//private static readonly ResourceDictionary _styles = new Styles();

    public static List<Theme> Themes { get; } =
	[
		new Theme
		{
			Name	= "Material",
			Tokens	= new Themes.Material.Theme()
		},
		new Theme
		{
			Name	= "Litera",
			Tokens	= new Litera()
		},
		new Theme
		{
			Name	= "Brite",
			Tokens	= new Brite()
        },
		new Theme
		{
			Name	= "Minty",
			Tokens	= new Minty()
        }
    ];
}