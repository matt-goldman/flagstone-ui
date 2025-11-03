namespace FlagstoneUI.BootstrapConverter.Models;

/// <summary>
/// Options for converting Bootstrap themes to Flagstone UI
/// </summary>
public class ConversionOptions
{
    /// <summary>
    /// Strategy for generating dark mode variants
    /// </summary>
    public DarkModeStrategy DarkModeStrategy { get; set; } = DarkModeStrategy.Auto;

    /// <summary>
    /// Include purpose comments in generated XAML
    /// </summary>
    public bool IncludeComments { get; set; } = true;

    /// <summary>
    /// Namespace to use in generated XAML (if applicable)
    /// </summary>
    public string? Namespace { get; set; }
}

/// <summary>
/// Strategy for handling dark mode color generation
/// </summary>
public enum DarkModeStrategy
{
    /// <summary>
    /// Automatically generate dark mode colors by darkening/lightening
    /// </summary>
    Auto,

    /// <summary>
    /// Manually specify dark mode colors (must be provided)
    /// </summary>
    Manual,

    /// <summary>
    /// No dark mode support (single theme only)
    /// </summary>
    None
}
