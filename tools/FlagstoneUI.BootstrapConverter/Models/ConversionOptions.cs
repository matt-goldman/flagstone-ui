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

    /// <summary>
    /// Include font information in conversion result (default: false)
    /// </summary>
    public bool IncludeFonts { get; set; } = false;
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

/// <summary>
/// Configuration for the conversion process
/// </summary>
public record ConversionRequest
{
	/// <summary>
	/// Input file paths or URLs
	/// </summary>
	public required string[] Inputs { get; init; }

	/// <summary>
	/// Bootstrap format (CSS, SCSS, or Auto-detect)
	/// </summary>
	public BootstrapFormat Format { get; init; } = BootstrapFormat.Auto;

	/// <summary>
	/// Analysis strategy to use
	/// </summary>
	public AnalysisStrategy Strategy { get; init; } = AnalysisStrategy.Hybrid;

	/// <summary>
	/// Conversion options (dark mode, comments, namespace)
	/// </summary>
	public ConversionOptions? Options { get; init; }

	/// <summary>
	/// Enable debug logging
	/// </summary>
	public bool EnableDebugLogging { get; init; }
}

/// <summary>
/// Result of the conversion process
/// </summary>
public record ConversionResult
{
	/// <summary>
	/// Extracted/generated tokens
	/// </summary>
	public required FlagstoneTokens Tokens { get; init; }

	/// <summary>
	/// Component styles (if CSS analysis was used)
	/// </summary>
	public BootstrapComponentStyles? ComponentStyles { get; init; }

	/// <summary>
	/// Theme name extracted from input
	/// </summary>
	public required string ThemeName { get; init; }

	/// <summary>
	/// Statistics about the conversion
	/// </summary>
	public required ConversionStatistics Statistics { get; init; }

	/// <summary>
	/// Font information (if IncludeFonts option was enabled)
	/// </summary>
	public FontInformation? Fonts { get; init; }
}

/// <summary>
/// Statistics about the conversion process
/// </summary>
public record ConversionStatistics
{
	public int ColorTokens { get; init; }
	public int TypographyTokens { get; init; }
	public int SpacingTokens { get; init; }
	public int BorderRadiusTokens { get; init; }
	public int BorderWidthTokens { get; init; }
	public int BorderTopWidthTokens { get; init; }
	public int BorderRightWidthTokens { get; init; }
	public int BorderBottomWidthTokens { get; init; }
	public int BorderLeftWidthTokens { get; init; }
	public int ShadowTokens { get; init; }
	public int ComponentStylesExtracted { get; init; }
	public int VariablesParsed { get; init; }
}

/// <summary>
/// Analysis strategy for conversion
/// </summary>
public enum AnalysisStrategy
{
	/// <summary>
	/// Use only CSS class analysis (top-down)
	/// </summary>
	CssOnly,

	/// <summary>
	/// Use only variable parsing (bottom-up)
	/// </summary>
	VariablesOnly,

	/// <summary>
	/// Use both CSS and variables (recommended)
	/// </summary>
	Hybrid
}
