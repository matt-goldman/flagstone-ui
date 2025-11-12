namespace FlagstoneUI.BootstrapConverter.Models;

/// <summary>
/// Represents parsed Bootstrap theme variables
/// </summary>
public class BootstrapVariables
{
    /// <summary>
    /// Color variables (e.g., primary, secondary, success, danger)
    /// </summary>
    public Dictionary<string, string> Colors { get; set; } = new();

    /// <summary>
    /// Typography variables (font families, sizes, weights, line heights)
    /// </summary>
    public Dictionary<string, string> Typography { get; set; } = new();

    /// <summary>
    /// Spacing variables (spacers, margins, paddings)
    /// </summary>
    public Dictionary<string, string> Spacing { get; set; } = new();

    /// <summary>
    /// Border variables (radius, width, color)
    /// </summary>
    public Dictionary<string, string> Borders { get; set; } = new();

    /// <summary>
    /// All other variables not categorized above
    /// </summary>
    public Dictionary<string, string> Other { get; set; } = new();
}
