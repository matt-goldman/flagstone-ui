using CssComputer.Core.Models;
using System.Globalization;

namespace CssComputer.Core.Services;

/// <summary>
/// Stage 2: Normalizes resolved styles by canonicalizing values and removing defaults.
/// </summary>
public class StyleNormalizationService
{
    private static readonly HashSet<string> DefaultValues = new()
    {
        "inherit", "initial", "unset", "auto"
    };

    /// <summary>
    /// Normalize all resolved elements.
    /// </summary>
    public List<ResolvedElement> NormalizeElements(List<ResolvedElement> elements)
    {
        var normalized = new List<ResolvedElement>();

        foreach (var element in elements)
        {
            var normalizedProps = NormalizeProperties(element.Properties);
            
            if (normalizedProps.Count > 0)
            {
                normalized.Add(new ResolvedElement
                {
                    ElementId = element.ElementId,
                    Properties = normalizedProps,
                    SourceInfo = element.SourceInfo
                });
            }
        }

        return normalized;
    }

    private Dictionary<string, string> NormalizeProperties(Dictionary<string, string> properties)
    {
        var normalized = new Dictionary<string, string>();

        foreach (var (key, value) in properties)
        {
            // Skip default/insignificant values
            if (string.IsNullOrWhiteSpace(value) || DefaultValues.Contains(value.ToLower()))
                continue;

            var normalizedValue = NormalizeValue(key, value);
            if (normalizedValue != null)
            {
                normalized[key] = normalizedValue;
            }
        }

        return normalized;
    }

    private string? NormalizeValue(string property, string value)
    {
        // Normalize colors
        if (property.Contains("color") || property.Contains("background"))
        {
            return NormalizeColor(value);
        }

        // Normalize numeric values with units
        if (IsNumericProperty(property))
        {
            return NormalizeNumeric(value);
        }

        // Normalize whitespace
        return value.Trim();
    }

    private string NormalizeColor(string color)
    {
        color = color.Trim().ToLower();

        // Convert named colors to hex
        if (NamedColors.TryGetValue(color, out var hex))
        {
            return hex;
        }

        // Normalize hex colors (e.g., #abc -> #aabbcc)
        if (color.StartsWith("#"))
        {
            if (color.Length == 4)
            {
                return $"#{color[1]}{color[1]}{color[2]}{color[2]}{color[3]}{color[3]}";
            }
            return color;
        }

        // Normalize rgb/rgba
        if (color.StartsWith("rgb"))
        {
            return NormalizeRgb(color);
        }

        return color;
    }

    private string NormalizeRgb(string rgb)
    {
        // Convert rgb(r, g, b) to hex
        var match = System.Text.RegularExpressions.Regex.Match(rgb, @"rgba?\((\d+),\s*(\d+),\s*(\d+)");
        if (match.Success)
        {
            var r = int.Parse(match.Groups[1].Value);
            var g = int.Parse(match.Groups[2].Value);
            var b = int.Parse(match.Groups[3].Value);
            return $"#{r:x2}{g:x2}{b:x2}";
        }
        return rgb;
    }

    private string NormalizeNumeric(string value)
    {
        value = value.Trim();

        // Extract number and unit
        var match = System.Text.RegularExpressions.Regex.Match(value, @"^([-+]?\d*\.?\d+)([a-z%]*)$");
        if (match.Success)
        {
            var number = double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            var unit = match.Groups[2].Value;

            // Normalize zero values (remove unit)
            if (number == 0)
            {
                return "0";
            }

            // Normalize units
            unit = NormalizeUnit(unit);

            return $"{number}{unit}";
        }

        return value;
    }

    private string NormalizeUnit(string unit)
    {
        // Prefer rem over px for relative sizing
        return unit.ToLower();
    }

    private bool IsNumericProperty(string property)
    {
        var numericProps = new[]
        {
            "width", "height", "padding", "margin", "border-width",
            "font-size", "line-height", "top", "right", "bottom", "left"
        };
        return numericProps.Any(p => property.Contains(p));
    }

    private static readonly Dictionary<string, string> NamedColors = new()
    {
        ["black"] = "#000000",
        ["white"] = "#ffffff",
        ["red"] = "#ff0000",
        ["green"] = "#008000",
        ["blue"] = "#0000ff",
        ["yellow"] = "#ffff00",
        ["cyan"] = "#00ffff",
        ["magenta"] = "#ff00ff",
        ["gray"] = "#808080",
        ["grey"] = "#808080"
    };
}
