namespace FlagstoneUI.BootstrapConverter.Models;

/// <summary>
/// Represents Flagstone UI tokens mapped from Bootstrap variables
/// </summary>
public class FlagstoneTokens
{
    /// <summary>
    /// Color tokens
    /// </summary>
    public Dictionary<string, ColorToken> Colors { get; set; } = new();

    /// <summary>
    /// Typography tokens
    /// </summary>
    public Dictionary<string, TypographyToken> Typography { get; set; } = new();

    /// <summary>
    /// Spacing tokens
    /// </summary>
    public Dictionary<string, NumericToken> Spacing { get; set; } = new();

    /// <summary>
    /// Border radius tokens
    /// </summary>
    public Dictionary<string, NumericToken> BorderRadius { get; set; } = new();

    /// <summary>
    /// Border width tokens
    /// </summary>
    public Dictionary<string, NumericToken> BorderWidth { get; set; } = new();
}

/// <summary>
/// Represents a color token with optional dark mode variant
/// </summary>
public class ColorToken
{
    /// <summary>
    /// Token key (e.g., "Color.Primary")
    /// </summary>
    public required string Key { get; set; }

    /// <summary>
    /// Light mode color value (hex, rgb, or named color)
    /// </summary>
    public required string Value { get; set; }

    /// <summary>
    /// Dark mode color value (optional)
    /// </summary>
    public string? DarkValue { get; set; }

    /// <summary>
    /// Purpose/description of this token
    /// </summary>
    public string? Purpose { get; set; }
}

/// <summary>
/// Represents a typography token
/// </summary>
public class TypographyToken
{
    /// <summary>
    /// Token key (e.g., "FontFamily.Default")
    /// </summary>
    public required string Key { get; set; }

    /// <summary>
    /// Token value (font family, size, weight, etc.)
    /// </summary>
    public required string Value { get; set; }

    /// <summary>
    /// Unit for numeric values (e.g., "px", "pt")
    /// </summary>
    public string? Unit { get; set; }

    /// <summary>
    /// Purpose/description of this token
    /// </summary>
    public string? Purpose { get; set; }
}

/// <summary>
/// Represents a numeric token (spacing, border, etc.)
/// </summary>
public class NumericToken
{
    /// <summary>
    /// Token key (e.g., "Spacing.Medium")
    /// </summary>
    public required string Key { get; set; }

    /// <summary>
    /// Numeric value
    /// </summary>
    public required double Value { get; set; }

    /// <summary>
    /// Unit (e.g., "px", "rem")
    /// </summary>
    public string Unit { get; set; } = "px";

    /// <summary>
    /// Purpose/description of this token
    /// </summary>
    public string? Purpose { get; set; }
}
