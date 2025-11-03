using FlagstoneUI.BootstrapConverter.Models;
using System.Text.RegularExpressions;

namespace FlagstoneUI.BootstrapConverter;

/// <summary>
/// Parses Bootstrap CSS/SCSS files and extracts variables
/// </summary>
public partial class BootstrapParser
{
    // Dictionary to store all parsed variables for resolution
    private Dictionary<string, string> _variableRegistry = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Parse multiple Bootstrap files and merge their variables
    /// </summary>
    /// <param name="filePaths">Array of file paths to parse</param>
    /// <param name="format">Expected format (auto-detected if not specified)</param>
    /// <returns>Merged Bootstrap variables</returns>
    public async Task<BootstrapVariables> ParseMultipleFilesAsync(string[] filePaths, BootstrapFormat format = BootstrapFormat.Auto)
    {
        ConverterLogger.Info($"Parsing {filePaths.Length} file(s)...");
        
        var allVariables = new BootstrapVariables();
        _variableRegistry.Clear();

        // First pass: collect all variables
        foreach (var filePath in filePaths)
        {
            ConverterLogger.Debug($"Reading file: {filePath}");
            var content = await File.ReadAllTextAsync(filePath);
            var detectedFormat = format == BootstrapFormat.Auto ? DetectFormat(content, filePath) : format;
            
            CollectVariables(content, detectedFormat);
        }

        ConverterLogger.Debug($"Collected {_variableRegistry.Count} total variables");

        // Second pass: resolve references and categorize
        foreach (var (name, value) in _variableRegistry)
        {
            var resolvedValue = ResolveVariableReferences(value);
            CategorizeVariable(allVariables, name, resolvedValue);
        }

        LogVariableSummary(allVariables);
        return allVariables;
    }
    /// <summary>
    /// Parse Bootstrap CSS from a string
    /// </summary>
    /// <param name="cssContent">CSS content containing Bootstrap variables</param>
    /// <returns>Parsed Bootstrap variables</returns>
    public BootstrapVariables ParseCss(string cssContent)
    {
        var variables = new BootstrapVariables();

        ConverterLogger.Debug("Parsing CSS content...");
        
        // Use regex to extract CSS custom properties from :root
        // Format: --property-name: value;
        var matches = CssCustomPropertyRegex().Matches(cssContent);

        ConverterLogger.Debug($"Found {matches.Count} CSS custom properties");

        foreach (Match match in matches)
        {
            var name = match.Groups[1].Value.Trim();
            var value = match.Groups[2].Value.Trim().TrimEnd(';');

            CategorizeVariable(variables, name, value);
        }

        LogVariableSummary(variables);
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

        ConverterLogger.Debug("Parsing SCSS content...");

        // Simple regex-based SCSS variable extraction
        // Format: $variable-name: value;
        var matches = ScssVariableRegex().Matches(scssContent);

        ConverterLogger.Debug($"Found {matches.Count} SCSS variables");

        foreach (Match match in matches)
        {
            var name = match.Groups[1].Value.Trim();
            var value = match.Groups[2].Value.Trim().TrimEnd(';');

            CategorizeVariable(variables, $"--bs-{name}", value);
        }

        LogVariableSummary(variables);
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
            ConverterLogger.LogVariableDiscovered("Color", normalizedName, value);
        }
        else if (IsTypographyVariable(normalizedName))
        {
            variables.Typography[normalizedName] = value;
            ConverterLogger.LogVariableDiscovered("Typography", normalizedName, value);
        }
        else if (IsSpacingVariable(normalizedName))
        {
            variables.Spacing[normalizedName] = value;
            ConverterLogger.LogVariableDiscovered("Spacing", normalizedName, value);
        }
        else if (IsBorderVariable(normalizedName))
        {
            variables.Borders[normalizedName] = value;
            ConverterLogger.LogVariableDiscovered("Border", normalizedName, value);
        }
        else
        {
            variables.Other[normalizedName] = value;
            ConverterLogger.LogVariableDiscovered("Other", normalizedName, value);
        }
    }

    private static void LogVariableSummary(BootstrapVariables variables)
    {
        ConverterLogger.Info($"Parsing complete - Colors: {variables.Colors.Count}, Typography: {variables.Typography.Count}, Spacing: {variables.Spacing.Count}, Borders: {variables.Borders.Count}, Other: {variables.Other.Count}");
    }

    private void CollectVariables(string content, BootstrapFormat format)
    {
        var matches = format == BootstrapFormat.Scss 
            ? ScssVariableRegex().Matches(content)
            : CssCustomPropertyRegex().Matches(content);

        foreach (Match match in matches)
        {
            var name = match.Groups[1].Value.Trim();
            var value = match.Groups[2].Value.Trim().TrimEnd(';');

            // Normalize name (remove --bs- prefix for CSS, add it for SCSS)
            if (format == BootstrapFormat.Scss)
            {
                name = $"--bs-{name}";
            }

            // Clean up !default and other SCSS syntax
            value = value.Replace("!default", string.Empty, StringComparison.Ordinal).Trim();

            // Store in registry (later values override earlier ones)
            _variableRegistry[name] = value;
        }
    }

    private string ResolveVariableReferences(string value)
    {
        // Resolve SCSS variable references ($variable-name)
        var scssVarRegex = new System.Text.RegularExpressions.Regex(@"\$([a-zA-Z0-9\-_]+)");
        var resolved = scssVarRegex.Replace(value, match =>
        {
            var varName = match.Groups[1].Value;
            var lookupName = $"--bs-{varName}";
            
            if (_variableRegistry.TryGetValue(lookupName, out var varValue))
            {
                // Recursively resolve nested references
                return ResolveVariableReferences(varValue);
            }
            
            ConverterLogger.Warning($"Unresolved SCSS variable reference: ${varName}");
            return match.Value; // Keep original if not found
        });

        // Resolve CSS custom property references (var(--bs-variable-name))
        var cssVarRegex = new System.Text.RegularExpressions.Regex(@"var\((--[a-zA-Z0-9\-_]+)\)");
        resolved = cssVarRegex.Replace(resolved, match =>
        {
            var varName = match.Groups[1].Value;
            
            if (_variableRegistry.TryGetValue(varName, out var varValue))
            {
                // Recursively resolve nested references
                return ResolveVariableReferences(varValue);
            }
            
            ConverterLogger.Warning($"Unresolved CSS variable reference: {varName}");
            return match.Value; // Keep original if not found
        });

        return resolved;
    }

    private static bool IsColorValue(string value)
    {
        // Check if value looks like a color (hex, rgb, rgba, named color)
        return value.StartsWith('#') 
            || value.StartsWith("rgb", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("hsl", StringComparison.OrdinalIgnoreCase)
            || IsNamedColor(value);
    }

    private static bool IsNamedColor(string value)
    {
        // Common named colors
        var namedColors = new[] { "white", "black", "gray", "grey", "red", "blue", "green", 
            "yellow", "orange", "purple", "pink", "cyan", "teal", "indigo", "brown" };
        return namedColors.Any(c => value.Equals(c, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsColorVariable(string name)
    {
        var colorKeywords = new[] { "primary", "secondary", "success", "danger", "warning", "info", "light", "dark", 
            "color", "bg", "background", "border-color", "text" };
        
        // Check for color keywords
        if (colorKeywords.Any(keyword => name.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        // Check for Bootstrap color scale names (gray-100, blue, red, etc.)
        var colorNames = new[] { "white", "black", "gray", "grey", "red", "blue", "green", 
            "yellow", "orange", "purple", "pink", "cyan", "teal", "indigo", "brown" };
        
        return colorNames.Any(color => name.StartsWith(color, StringComparison.OrdinalIgnoreCase));
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
