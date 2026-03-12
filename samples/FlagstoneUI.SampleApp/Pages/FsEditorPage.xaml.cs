using FlagstoneUI.SampleApp.ViewModels;

namespace FlagstoneUI.SampleApp.Pages;

public partial class FsEditorPage : ContentPage
{
	public FsEditorPage()
	{
		InitializeComponent();
		BindingContext = new FsEditorViewModel();
	}
}
