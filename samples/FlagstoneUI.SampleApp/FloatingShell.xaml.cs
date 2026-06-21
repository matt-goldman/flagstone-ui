using FlagstoneUI.Core.Controls;
using FlagstoneUI.SampleApp.ViewModels;

namespace FlagstoneUI.SampleApp;

public partial class FloatingShell : FsShell
{
	public FloatingShell()
	{
		InitializeComponent();
		BindingContext = new AppShellViewModel();
	}
}
