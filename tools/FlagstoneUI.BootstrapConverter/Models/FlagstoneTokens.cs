namespace FlagstoneUI.BootstrapConverter.Models;

/// <summary>
/// Represents Flagstone UI tokens mapped from Bootstrap variables
/// </summary>
public class FlagstoneTokens
{
    /// <summary>
    /// Color tokens
    /// </summary>
    public Dictionary<string, ColorToken> Colors { get; set; } = [];

    /// <summary>
    /// Typography tokens
    /// </summary>
    public Dictionary<string, TypographyToken> Typography { get; set; } = [];

    /// <summary>
    /// Spacing tokens
    /// </summary>
    public Dictionary<string, NumericToken> Spacing { get; set; } = [];

    /// <summary>
    /// Border radius tokens
    /// </summary>
    public Dictionary<string, NumericToken> BorderRadius { get; set; } = [];

    /// <summary>
    /// Border width tokens
    /// </summary>
    public Dictionary<string, NumericToken> BorderWidth { get; set; } = [];

    /// <summary>
    /// Per-edge border width tokens (e.g., BorderTopWidth.Default)
    /// </summary>
    public Dictionary<string, NumericToken> BorderTopWidth { get; set; } = [];

    /// <summary>
    /// Per-edge border width tokens (e.g., BorderRightWidth.Default)
    /// </summary>
    public Dictionary<string, NumericToken> BorderRightWidth { get; set; } = [];

    /// <summary>
    /// Per-edge border width tokens (e.g., BorderBottomWidth.Default)
    /// </summary>
    public Dictionary<string, NumericToken> BorderBottomWidth { get; set; } = [];

    /// <summary>
    /// Per-edge border width tokens (e.g., BorderLeftWidth.Default)
    /// </summary>
    public Dictionary<string, NumericToken> BorderLeftWidth { get; set; } = [];

    /// <summary>
    /// Shadow tokens
    /// </summary>
    public Dictionary<string, ShadowToken> Shadows { get; set; } = [];
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

/// <summary>
/// Represents a shadow token (box-shadow)
/// </summary>
public class ShadowToken
{
    /// <summary>
    /// Token key (e.g., "Shadow.Button", "Shadow.Card")
    /// </summary>
    public required string Key { get; set; }

    /// <summary>
    /// Horizontal offset
    /// </summary>
    public double OffsetX { get; set; }

    /// <summary>
    /// Vertical offset
    /// </summary>
    public double OffsetY { get; set; }

    /// <summary>
    /// Blur radius
    /// </summary>
    public double Radius { get; set; }

    /// <summary>
    /// Shadow color (hex or rgba)
    /// </summary>
    public required string Color { get; set; }

    /// <summary>
    /// Shadow opacity (0-1)
    /// </summary>
    public double Opacity { get; set; } = 1.0;

    /// <summary>
    /// Purpose/description of this token
    /// </summary>
    public string? Purpose { get; set; }
}
