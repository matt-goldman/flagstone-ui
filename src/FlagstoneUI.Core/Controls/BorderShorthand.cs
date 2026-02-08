namespace FlagstoneUI.Core.Controls;

/// <summary>
/// Represents a parsed border shorthand value with thickness and color.
/// </summary>
public class BorderEdgeValue
{
	public double Thickness { get; set; }
	public Color Color { get; set; } = Colors.Transparent;

	public BorderEdgeValue(double thickness, Color color)
	{
		Thickness = thickness;
		Color = color;
	}
}

/// <summary>
/// Represents the parsed border shorthand with values for all four edges.
/// </summary>
public class BorderShorthand(BorderEdgeValue top, BorderEdgeValue right, BorderEdgeValue bottom, BorderEdgeValue left)
{
	public BorderEdgeValue Top { get; set; } = top;
	public BorderEdgeValue Right { get; set; } = right;
	public BorderEdgeValue Bottom { get; set; } = bottom;
	public BorderEdgeValue Left { get; set; } = left;

	/// <summary>
	/// Parses a border shorthand string into a BorderShorthand object.
	/// </summary>
	/// <param name="value">Border shorthand string (e.g., "1 Black", "1 Black, 2 Grey", "1 White, 3 Black, 3 Black, 1 White")</param>
	/// <returns>Parsed BorderShorthand object</returns>
	public static BorderShorthand Parse(string value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			var transparent = new BorderEdgeValue(0, Colors.Transparent);
			return new BorderShorthand(transparent, transparent, transparent, transparent);
		}

		// Split by comma to get individual edge values
		var parts = value.Split(',')
			.Select(p => p.Trim())
			.Where(p => !string.IsNullOrEmpty(p))
			.ToArray();

		if (parts.Length == 0)
		{
			var transparent = new BorderEdgeValue(0, Colors.Transparent);
			return new BorderShorthand(transparent, transparent, transparent, transparent);
		}

		var edgeValues = parts.Select(ParseEdgeValue).ToArray();

		return edgeValues.Length switch
		{
			1 => new BorderShorthand(edgeValues[0], edgeValues[0], edgeValues[0], edgeValues[0]),
			2 => new BorderShorthand(edgeValues[0], edgeValues[1], edgeValues[0], edgeValues[1]),
			4 => new BorderShorthand(edgeValues[0], edgeValues[1], edgeValues[2], edgeValues[3]),
			_ => throw new ArgumentException($"Invalid border shorthand syntax. Expected 1, 2, or 4 values, but got {edgeValues.Length}. Value: {value}")
		};
	}

	private static BorderEdgeValue ParseEdgeValue(string edgeSpec)
	{
		// Each edge spec is "thickness color" (e.g., "1 Black", "2 #FF0000")
		var tokens = edgeSpec.Split(' ', StringSplitOptions.RemoveEmptyEntries);

		if (tokens.Length < 2)
		{
			throw new ArgumentException($"Invalid edge specification. Expected 'thickness color', but got: {edgeSpec}");
		}

		if (!double.TryParse(tokens[0], out var thickness))
		{
			throw new ArgumentException($"Invalid thickness value: {tokens[0]}");
		}

		// Join remaining tokens back together for color parsing (handles colors with spaces)
		var colorString = string.Join(" ", tokens.Skip(1));

		Color color;
		try
		{
			// Try hex color first
			if (colorString.StartsWith("#"))
			{
				color = Color.FromRgba(colorString);
			}
			// Try named color by looking it up in Colors static class
			else
			{
				color = ParseNamedColor(colorString);
			}
		}
		catch
		{
			throw new ArgumentException($"Invalid color value: {colorString}");
		}

		return new BorderEdgeValue(thickness, color);
	}

	/// <summary>
	/// Parses a named color string (e.g., "Red", "Blue") using reflection on the Colors class.
	/// </summary>
	private static Color ParseNamedColor(string colorName)
	{
		var colorField = typeof(Colors).GetField(colorName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.IgnoreCase);
		if (colorField != null && colorField.FieldType == typeof(Color))
		{
			return (Color)colorField.GetValue(null)!;
		}

		throw new ArgumentException($"Unknown color name: {colorName}");
	}
}
