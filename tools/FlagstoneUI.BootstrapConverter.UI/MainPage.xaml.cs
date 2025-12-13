using FlagstoneUI.BootstrapConverter.UI.ViewModels;

namespace FlagstoneUI.BootstrapConverter.UI;

public partial class MainPage : ContentPage
{
	public MainPage(MainViewModel viewModel)
	{
		BindingContext = viewModel;
		InitializeComponent();
	}

}
