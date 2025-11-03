namespace FlagstoneUI.SampleApp;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
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
