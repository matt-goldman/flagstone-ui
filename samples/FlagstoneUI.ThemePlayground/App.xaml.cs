using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace FlagstoneUI.ThemePlayground;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new ContentPage
        {
            Content = new VerticalStackLayout
            {
                Padding = 24,
                Children =
                {
                    new Label { Text = "Theme Playground" },
                    new Button { Text = "Primary" }
                }
            }
        };
    }
}
