using FlagstoneUI.SampleApp.ViewModels;

namespace FlagstoneUI.SampleApp;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		BindingContext = new AppShellViewModel();
	}
}
