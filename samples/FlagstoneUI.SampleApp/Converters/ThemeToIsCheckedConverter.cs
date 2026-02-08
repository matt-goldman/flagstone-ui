using System.Globalization;

namespace FlagstoneUI.SampleApp.Converters;

/// <summary>
/// Converts between a theme name and a boolean IsChecked value for radio buttons.
/// The converter parameter specifies the theme name to match against.
/// </summary>
public class ThemeToIsCheckedConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string selectedTheme && parameter is string themeName)
        {
            return selectedTheme.Equals(themeName, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is true && parameter is string themeName)
        {
            return themeName;
        }
        return Binding.DoNothing;
    }
}
