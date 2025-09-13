using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace FlagstoneUI.SampleApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new NavigationPage(new Pages.HomePage());
    }
}
