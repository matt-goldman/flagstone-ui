using FlagstoneUI.Core.Controls;
using FlagstoneUI.SampleApp.ViewModels;

namespace FlagstoneUI.SampleApp;

public partial class AppShell : FsShell
{
	public AppShell()
	{
		InitializeComponent();
		BindingContext = new AppShellViewModel();
	}

    private void OnThemeRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        var theme = (sender as RadioButton)?.ContentAsString();

        if (theme is not null && e.Value)
        {
            App.SwitchTheme(theme);
        }
    }
}
