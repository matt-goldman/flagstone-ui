namespace FlagstoneUI.BootstrapConverter.Models;

/// <summary>
/// Represents parsed Bootstrap theme variables
/// </summary>
public class BootstrapVariables
{
	/// <summary>
	/// Color variables (e.g., primary, secondary, success, danger)
	/// </summary>
	public Dictionary<string, string> Colors { get; set; } = [];

	/// <summary>
	/// Typography variables (font families, sizes, weights, line heights)
	/// </summary>
	public Dictionary<string, string> Typography { get; set; } = [];

	/// <summary>
	/// Spacing variables (spacers, margins, paddings)
	/// </summary>
	public Dictionary<string, string> Spacing { get; set; } = [];

	/// <summary>
	/// Border variables (radius, width, color)
	/// </summary>
	public Dictionary<string, string> Borders { get; set; } = [];

	/// <summary>
	/// All other variables not categorized above
	/// </summary>
	public Dictionary<string, string> Other { get; set; } = [];
}
