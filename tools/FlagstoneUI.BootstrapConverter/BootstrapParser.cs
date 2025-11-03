using FlagstoneUI.BootstrapConverter.Models;
using System.Text.RegularExpressions;

namespace FlagstoneUI.BootstrapConverter;

/// <summary>
/// Parses Bootstrap CSS/SCSS files and extracts variables
/// </summary>
public partial class BootstrapParser
{
    /// <summary>
    /// Parse Bootstrap CSS from a string
    /// </summary>
    /// <param name="cssContent">CSS content containing Bootstrap variables</param>
    /// <returns>Parsed Bootstrap variables</returns>
    public BootstrapVariables ParseCss(string cssContent)
    {
        var variables = new BootstrapVariables();

        // Use regex to extract CSS custom properties from :root
        // Format: --property-name: value;
        var matches = CssCustomPropertyRegex().Matches(cssContent);

        foreach (Match match in matches)
        {
            var name = match.Groups[1].Value.Trim();
            var value = match.Groups[2].Value.Trim().TrimEnd(';');

            CategorizeVariable(variables, name, value);
        }

        return variables;
    }

    /// <summary>
    /// Parse Bootstrap SCSS from a string
    /// </summary>
    /// <param name="scssContent">SCSS content containing Bootstrap variables</param>
    /// <returns>Parsed Bootstrap variables</returns>
    public BootstrapVariables ParseScss(string scssContent)
    {
        var variables = new BootstrapVariables();

        // Simple regex-based SCSS variable extraction
        // Format: $variable-name: value;
        var matches = ScssVariableRegex().Matches(scssContent);

        foreach (Match match in matches)
        {
            var name = match.Groups[1].Value.Trim();
            var value = match.Groups[2].Value.Trim().TrimEnd(';');

            CategorizeVariable(variables, $"--bs-{name}", value);
        }

        return variables;
    }

    /// <summary>
    /// Parse Bootstrap theme from a URL
    /// </summary>
    /// <param name="url">URL to Bootstrap CSS/SCSS file</param>
    /// <param name="format">Expected format (auto-detected if not specified)</param>
    /// <returns>Parsed Bootstrap variables</returns>
    public async Task<BootstrapVariables> ParseFromUrlAsync(string url, BootstrapFormat format = BootstrapFormat.Auto)
    {
        using var httpClient = new HttpClient();
        var content = await httpClient.GetStringAsync(url);

        return ParseContent(content, format, url);
    }

    /// <summary>
    /// Parse Bootstrap theme from a file
    /// </summary>
    /// <param name="filePath">Path to Bootstrap CSS/SCSS file</param>
    /// <param name="format">Expected format (auto-detected if not specified)</param>
    /// <returns>Parsed Bootstrap variables</returns>
    public async Task<BootstrapVariables> ParseFromFileAsync(string filePath, BootstrapFormat format = BootstrapFormat.Auto)
    {
        var content = await File.ReadAllTextAsync(filePath);
        return ParseContent(content, format, filePath);
    }

    /// <summary>
    /// Parse Bootstrap theme content
    /// </summary>
    /// <param name="content">Bootstrap CSS/SCSS content</param>
    /// <param name="format">Expected format (auto-detected if not specified)</param>
    /// <param name="source">Source identifier for error messages</param>
    /// <returns>Parsed Bootstrap variables</returns>
    public BootstrapVariables ParseContent(string content, BootstrapFormat format = BootstrapFormat.Auto, string source = "content")
    {
        // Auto-detect format
        if (format == BootstrapFormat.Auto)
        {
            format = DetectFormat(content, source);
        }

        return format switch
        {
            BootstrapFormat.Css => ParseCss(content),
            BootstrapFormat.Scss => ParseScss(content),
            _ => throw new ArgumentException($"Unsupported format: {format}")
        };
    }

    private BootstrapFormat DetectFormat(string content, string source)
    {
        // Check file extension if source is a file path
        if (source.EndsWith(".scss", StringComparison.OrdinalIgnoreCase))
            return BootstrapFormat.Scss;
        if (source.EndsWith(".css", StringComparison.OrdinalIgnoreCase))
            return BootstrapFormat.Css;

        // Check content for SCSS variables
        if (ScssVariableRegex().IsMatch(content))
            return BootstrapFormat.Scss;

        // Default to CSS
        return BootstrapFormat.Css;
    }

    private void CategorizeVariable(BootstrapVariables variables, string name, string value)
    {
        // Normalize Bootstrap variable names
        var normalizedName = name.Replace("--bs-", string.Empty, StringComparison.Ordinal).Replace("$", string.Empty, StringComparison.Ordinal);

        // Categorize based on variable name
        if (IsColorVariable(normalizedName))
        {
            variables.Colors[normalizedName] = value;
        }
        else if (IsTypographyVariable(normalizedName))
        {
            variables.Typography[normalizedName] = value;
        }
        else if (IsSpacingVariable(normalizedName))
        {
            variables.Spacing[normalizedName] = value;
        }
        else if (IsBorderVariable(normalizedName))
        {
            variables.Borders[normalizedName] = value;
        }
        else
        {
            variables.Other[normalizedName] = value;
        }
    }

    private static bool IsColorVariable(string name)
    {
        var colorKeywords = new[] { "primary", "secondary", "success", "danger", "warning", "info", "light", "dark", 
            "color", "bg", "background", "border-color", "text" };
        return colorKeywords.Any(keyword => name.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsTypographyVariable(string name)
    {
        var typographyKeywords = new[] { "font", "text", "line-height", "letter-spacing" };
        return typographyKeywords.Any(keyword => name.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsSpacingVariable(string name)
    {
        var spacingKeywords = new[] { "spacer", "margin", "padding", "gap" };
        return spacingKeywords.Any(keyword => name.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsBorderVariable(string name)
    {
        var borderKeywords = new[] { "border-radius", "border-width", "rounded" };
        return borderKeywords.Any(keyword => name.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    [GeneratedRegex(@"(--[a-zA-Z0-9\-_]+)\s*:\s*([^;]+);", RegexOptions.Multiline)]
    private static partial Regex CssCustomPropertyRegex();

    [GeneratedRegex(@"\$([a-zA-Z0-9\-_]+)\s*:\s*([^;]+);", RegexOptions.Multiline)]
    private static partial Regex ScssVariableRegex();
}

/// <summary>
/// Bootstrap file format
/// </summary>
public enum BootstrapFormat
{
    /// <summary>
    /// Auto-detect format based on content/file extension
    /// </summary>
    Auto,

    /// <summary>
    /// CSS format (--bs-* custom properties)
    /// </summary>
    Css,

    /// <summary>
    /// SCSS format ($variable-name)
    /// </summary>
    Scss
}
