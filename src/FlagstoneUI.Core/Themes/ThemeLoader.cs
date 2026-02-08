namespace FlagstoneUI.Core.Themes;

/// <summary>
/// Provides methods to register core FlagstoneUI resource dictionaries (tokens) into a .NET MAUI application's resources.
/// </summary>
public static class ThemeLoader
{
    /// <summary>
    /// Registers the FlagstoneUI token resource dictionary into the specified application resources.
    /// </summary>
    /// <param name="appResources">The application's <see cref="ResourceDictionary"/> to which the tokens will be merged.</param>
    public static void Register(ResourceDictionary appResources)
    {
        appResources.MergedDictionaries.Add(new ResourceDictionary
        {
            Source = new Uri("/FlagstoneUI.Core;component/Styles/Tokens.xaml", UriKind.Relative)
        });
    }
}
