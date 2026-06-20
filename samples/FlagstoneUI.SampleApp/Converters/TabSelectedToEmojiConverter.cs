using System.Globalization;

namespace FlagstoneUI.SampleApp.Converters;

public class TabSelectedToEmojiConverter : IValueConverter
{
	public string SelectedValue { get; set; } = string.Empty;
	public string UnselectedValue { get; set; } = string.Empty;
	
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is bool isSelected)
		{
			return (isSelected ? SelectedValue : UnselectedValue);
		}

		throw new NotImplementedException();
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
