namespace FlagstoneUI.BootstrapConverter.Models;

/// <summary>
/// Font information extracted from Bootstrap theme
/// </summary>
public class FontInformation
{
	/// <summary>
	/// Font families discovered in the theme
	/// </summary>
	public List<FontFamily> Families { get; set; } = [];

	/// <summary>
	/// URLs to download fonts (e.g., Google Fonts)
	/// </summary>
	public List<string> DownloadUrls { get; set; } = [];

	/// <summary>
	/// Whether fonts were found in the theme
	/// </summary>
	public bool HasFonts => Families.Count > 0;
}

/// <summary>
/// Represents a font family with its variants
/// </summary>
public class FontFamily
{
	/// <summary>
	/// Font family name (e.g., "Roboto")
	/// </summary>
	public required string Name { get; set; }

	/// <summary>
	/// Source of the font (GoogleFonts, Local, System, etc.)
	/// </summary>
	public FontSource Source { get; set; }

	/// <summary>
	/// Font weights discovered (e.g., 400, 700)
	/// </summary>
	public List<int> Weights { get; set; } = [];

	/// <summary>
	/// Whether italic variant was found
	/// </summary>
	public bool HasItalic { get; set; }

	/// <summary>
	/// Suggested alias for MAUI registration
	/// </summary>
	public string SuggestedAlias => SanitizeFontName(Name);

	private static string SanitizeFontName(string name)
	{
		// Remove quotes and extra whitespace
		var sanitized = name.Trim('\'', '"', ' ');

		// For multi-word fonts, use the first word or remove spaces
		// Example: "Segoe UI" -> "SegoeUI"
		sanitized = sanitized.Replace(" ", string.Empty, StringComparison.Ordinal);

		return sanitized;
	}
}

/// <summary>
/// Font source type
/// </summary>
public enum FontSource
{
	/// <summary>
	/// Google Fonts (can be downloaded from URL)
	/// </summary>
	GoogleFonts,

	/// <summary>
	/// Local file referenced in @font-face
	/// </summary>
	LocalFile,

	/// <summary>
	/// System font (no download needed)
	/// </summary>
	System,

	/// <summary>
	/// Unknown/other source
	/// </summary>
	Unknown
}
