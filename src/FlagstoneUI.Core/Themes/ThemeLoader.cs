using Microsoft.Maui.Controls;

namespace FlagstoneUI.Core.Themes;

public static class ThemeLoader
{
    public static void Register(ResourceDictionary appResources)
    {
        appResources.MergedDictionaries.Add(new ResourceDictionary
        {
            Source = new Uri("/FlagstoneUI.Core;component/Styles/Tokens.xaml", UriKind.Relative)
        });
    }
}
