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

    /// <summary>
    /// Output format for resource dictionaries (XAML or C#)
    /// </summary>
    public ResourceDictionaryFormat OutputFormat { get; set; } = ResourceDictionaryFormat.Xaml;
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

/// <summary>
/// Output format for resource dictionaries
/// </summary>
public enum ResourceDictionaryFormat
{
    /// <summary>
    /// Generate XAML resource dictionaries
    /// </summary>
    Xaml,

    /// <summary>
    /// Generate C# resource dictionaries
    /// </summary>
    CSharp
}
