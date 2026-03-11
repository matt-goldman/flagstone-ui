using FlagstoneUI.SampleApp.ViewModels;

namespace FlagstoneUI.SampleApp.Pages;

public partial class SignInPage : ContentPage
{
	public SignInPage()
	{
		InitializeComponent();
		SignInForm.BindingContext = new SignInViewModel();
	}
}
