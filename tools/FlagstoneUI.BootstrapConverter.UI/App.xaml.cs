namespace FlagstoneUI.BootstrapConverter.UI;

public partial class App : Application
{
	readonly MainPage _page;

	public App(MainPage page)
	{
		InitializeComponent();
		_page = page;
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(_page);
	}
}
