using System.Text.RegularExpressions;

namespace FlagstoneUI.TokenGenerator;

/// <summary>
/// Extracts the styling surface from FlagstoneUI control source files.
/// Uses regex-based parsing to extract BindableProperty declarations.
/// </summary>
public class ControlSurfaceExtractor
{
    /// <summary>
    /// Represents a styled property extracted from a control.
    /// </summary>
    public class StyledProperty
    {
        public required string Name { get; init; }
        public required string Type { get; init; }
        public string? TokenCategory { get; init; }
        public string? RecommendedToken { get; init; }
        public bool Bindable { get; init; } = true;
        public string? DefaultValue { get; init; }
        public string? Description { get; init; }
    }

    /// <summary>
    /// Represents a control's styling surface.
    /// </summary>
    public class ControlSurface
    {
        public required string ControlName { get; init; }
        public required string InheritsFrom { get; init; }
        public required string Architecture { get; init; }
        public List<StyledProperty> StyledProperties { get; init; } = [];
    }

    // Regex to match BindableProperty declarations
    // Captures: PropertyName, Type, DeclaringType, DefaultValue
    private static readonly Regex BindablePropertyRegex = new(
        @"public\s+(?:new\s+)?static\s+readonly\s+BindableProperty\s+(\w+)Property\s*=\s*BindableProperty\.Create\s*\(\s*" +
        @"nameof\s*\(\s*(\w+)\s*\)\s*,\s*" +
        @"typeof\s*\(\s*([\w<>?]+)\s*\)\s*,\s*" +
        @"typeof\s*\(\s*(\w+)\s*\)\s*" +
        @"(?:,\s*([^,)]+))?",  // Optional default value
        RegexOptions.Compiled | RegexOptions.Singleline);

    // Regex to match class declaration and base class
    private static readonly Regex ClassDeclarationRegex = new(
        @"public\s+partial\s+class\s+(\w+)\s*:\s*(\w+)",
        RegexOptions.Compiled);

    // Regex to extract XML doc summary
    private static readonly Regex XmlSummaryRegex = new(
        @"/// <summary>\s*\n\s*/// (.*?)\s*\n\s*/// </summary>",
        RegexOptions.Compiled | RegexOptions.Singleline);

    // Map CLR types to contract schema types
    private static readonly Dictionary<string, string> TypeMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Color"] = "color",
        ["Brush"] = "brush",
        ["SolidColorBrush"] = "brush",
        ["double"] = "double",
        ["Double"] = "double",
        ["int"] = "int",
        ["Int32"] = "int",
        ["float"] = "double",
        ["Single"] = "double",
        ["Thickness"] = "thickness",
        ["string"] = "string",
        ["String"] = "string",
        ["bool"] = "bool",
        ["Boolean"] = "bool",
        ["CornerRadius"] = "cornerRadius",
        ["TextAlignment"] = "enum",
        ["Keyboard"] = "enum",
        ["PenLineCap"] = "enum",
        ["EditorAutoSizeOption"] = "enum",
    };

    // Map property names to token categories
    private static readonly Dictionary<string, string> PropertyTokenCategoryMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["BackgroundColor"] = "colors",
        ["Background"] = "colors",
        ["BackgroundBrush"] = "colors",
        ["TextColor"] = "colors",
        ["PlaceholderColor"] = "colors",
        ["BorderColor"] = "colors",
        ["BorderBrush"] = "colors",
        ["BorderTopBrush"] = "colors",
        ["BorderRightBrush"] = "colors",
        ["BorderBottomBrush"] = "colors",
        ["BorderLeftBrush"] = "colors",
        ["Stroke"] = "colors",
        ["CornerRadius"] = "borderRadius",
        ["BorderWidth"] = "borderWidth",
        ["StrokeThickness"] = "borderWidth",
        ["BorderTopThickness"] = "borderWidth",
        ["BorderRightThickness"] = "borderWidth",
        ["BorderBottomThickness"] = "borderWidth",
        ["BorderLeftThickness"] = "borderWidth",
        ["Elevation"] = "elevation",
        ["FontSize"] = "typography",
        ["Padding"] = "spacing",
        ["Opacity"] = "opacity",
    };

    // Map property names to recommended tokens
    private static readonly Dictionary<string, string> PropertyRecommendedTokenMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["BackgroundColor"] = "Color.Surface",
        ["Background"] = "Color.SurfaceVariant",
        ["TextColor"] = "Color.OnSurface",
        ["PlaceholderColor"] = "Color.OnSurfaceVariant",
        ["BorderColor"] = "Color.Outline",
        ["BorderBrush"] = "Color.Outline",
        ["CornerRadius"] = "Radius.Medium",
        ["BorderWidth"] = "BorderWidth.None",
        ["Elevation"] = "Elevation.Level1",
        ["FontSize"] = "FontSize.BodyLarge",
        ["Padding"] = "Space.16",
        ["Opacity"] = "Opacity.Full",
    };

    // Properties that are styling-relevant (vs behavioral)
    private static readonly HashSet<string> StylingProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        // Colors
        "BackgroundColor", "Background", "BackgroundBrush",
        "TextColor", "PlaceholderColor",
        "BorderColor", "BorderBrush",
        "BorderTopBrush", "BorderRightBrush", "BorderBottomBrush", "BorderLeftBrush",
        "Stroke",
        
        // Sizing/Spacing
        "CornerRadius", "BorderWidth", "StrokeThickness",
        "BorderTopThickness", "BorderRightThickness", "BorderBottomThickness", "BorderLeftThickness",
        "Elevation", "Padding",
        "HeightRequest", "MinimumHeightRequest", "WidthRequest",
        
        // Typography
        "FontSize", "FontFamily", "FontAttributes",
        
        // Text Alignment (visual)
        "HorizontalTextAlignment", "VerticalTextAlignment",
        
        // Opacity
        "Opacity",
        
        // Shorthand
        "Border",
    };

    /// <summary>
    /// Extracts the styling surface from a control source file.
    /// </summary>
    public ControlSurface? ExtractFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            return null;

        var content = File.ReadAllText(filePath);
        return ExtractFromSource(content, Path.GetFileNameWithoutExtension(filePath));
    }

    /// <summary>
    /// Extracts the styling surface from control source code.
    /// </summary>
    public ControlSurface? ExtractFromSource(string sourceCode, string fileName)
    {
        // Extract class declaration
        var classMatch = ClassDeclarationRegex.Match(sourceCode);
        if (!classMatch.Success)
            return null;

        var controlName = classMatch.Groups[1].Value;
        var baseClass = classMatch.Groups[2].Value;

        // Skip non-Fs controls and helper classes
        if (!controlName.StartsWith("Fs", StringComparison.Ordinal))
            return null;

        // Determine architecture
        var architecture = baseClass switch
        {
            "ContentView" => "wrapper",
            "Button" => "subclass",
            "Entry" => "subclass",
            "Editor" => "subclass",
            _ => "subclass"
        };

        // Map base class to full MAUI type name
        var inheritsFrom = baseClass switch
        {
            "ContentView" => "Microsoft.Maui.Controls.ContentView",
            "Button" => "Microsoft.Maui.Controls.Button",
            "Entry" => "Microsoft.Maui.Controls.Entry",
            "Editor" => "Microsoft.Maui.Controls.Editor",
            _ => $"Microsoft.Maui.Controls.{baseClass}"
        };

        // Extract BindableProperty declarations
        var styledProperties = new List<StyledProperty>();
        var matches = BindablePropertyRegex.Matches(sourceCode);

        foreach (Match match in matches)
        {
            var propertyName = match.Groups[2].Value;
            var propertyType = match.Groups[3].Value;
            var defaultValue = match.Groups.Count > 5 ? match.Groups[5].Value?.Trim() : null;

            // Filter to styling-relevant properties only
            if (!StylingProperties.Contains(propertyName))
                continue;

            // Map type
            var mappedType = MapType(propertyType);
            
            // Get token category and recommended token
            PropertyTokenCategoryMap.TryGetValue(propertyName, out var tokenCategory);
            PropertyRecommendedTokenMap.TryGetValue(propertyName, out var recommendedToken);

            // Extract description from XML doc (look backwards from the match)
            var description = ExtractDescription(sourceCode, match.Index, propertyName);

            styledProperties.Add(new StyledProperty
            {
                Name = propertyName,
                Type = mappedType,
                TokenCategory = tokenCategory,
                RecommendedToken = recommendedToken,
                Bindable = true,
                DefaultValue = CleanDefaultValue(defaultValue),
                Description = description
            });
        }

        // For FsButton (subclass of Button), we need to add inherited styling properties
        if (controlName == "FsButton")
        {
            AddInheritedButtonProperties(styledProperties);
        }

        return new ControlSurface
        {
            ControlName = controlName,
            InheritsFrom = inheritsFrom,
            Architecture = architecture,
            StyledProperties = styledProperties
        };
    }

    /// <summary>
    /// Extracts all control surfaces from a controls directory.
    /// </summary>
    public Dictionary<string, ControlSurface> ExtractFromDirectory(string controlsPath)
    {
        var results = new Dictionary<string, ControlSurface>();

        if (!Directory.Exists(controlsPath))
            return results;

        // Get all Fs*.cs files (including .xaml.cs)
        var controlFiles = Directory.GetFiles(controlsPath, "Fs*.cs", SearchOption.TopDirectoryOnly)
            .Where(f => !f.Contains("BorderlessEntry", StringComparison.OrdinalIgnoreCase) &&
                       !f.Contains("BorderlessEditor", StringComparison.OrdinalIgnoreCase));

        foreach (var file in controlFiles)
        {
            var surface = ExtractFromFile(file);
            if (surface != null)
            {
                // Use the control name without .xaml suffix
                var key = surface.ControlName;
                
                // If we already have this control (from .cs file), merge properties from .xaml.cs
                if (results.TryGetValue(key, out var existing))
                {
                    // Add any new properties not already present
                    foreach (var prop in surface.StyledProperties)
                    {
                        if (!existing.StyledProperties.Any(p => p.Name == prop.Name))
                        {
                            existing.StyledProperties.Add(prop);
                        }
                    }
                }
                else
                {
                    results[key] = surface;
                }
            }
        }

        return results;
    }

    private static string MapType(string clrType)
    {
        // Handle nullable types
        var cleanType = clrType.TrimEnd('?');
        
        // Handle generic types like Brush?
        if (TypeMap.TryGetValue(cleanType, out var mapped))
            return mapped;

        return "string"; // Default fallback
    }

    private static string? CleanDefaultValue(string? defaultValue)
    {
        if (string.IsNullOrWhiteSpace(defaultValue))
            return null;

        // Remove common patterns
        defaultValue = defaultValue.Trim();
        
        if (defaultValue.StartsWith("new ", StringComparison.Ordinal))
            return null; // Complex default, skip
            
        if (defaultValue.Contains("Colors.", StringComparison.Ordinal))
            return defaultValue.Replace("Colors.", "", StringComparison.Ordinal);
            
        return defaultValue;
    }

    private static string? ExtractDescription(string sourceCode, int matchIndex, string propertyName)
    {
        // Look for XML summary comment before the property
        var searchStart = Math.Max(0, matchIndex - 500);
        var searchText = sourceCode[searchStart..matchIndex];
        
        var summaryMatch = XmlSummaryRegex.Matches(searchText).LastOrDefault();
        if (summaryMatch != null)
        {
            var summary = summaryMatch.Groups[1].Value.Trim();
            // Clean up the summary
            summary = summary.Replace("Gets or sets ", "", StringComparison.Ordinal);
            summary = summary.Replace("Identifies the ", "", StringComparison.Ordinal);
            if (summary.Length > 100)
                summary = summary[..100] + "...";
            return summary;
        }

        return null;
    }

    private static void AddInheritedButtonProperties(List<StyledProperty> properties)
    {
        // FsButton inherits from Button, which has these styling properties
        var inheritedProps = new[]
        {
            new StyledProperty { Name = "BackgroundColor", Type = "color", TokenCategory = "colors", RecommendedToken = "Color.Primary", Bindable = true },
            new StyledProperty { Name = "TextColor", Type = "color", TokenCategory = "colors", RecommendedToken = "Color.OnPrimary", Bindable = true },
            new StyledProperty { Name = "BorderColor", Type = "color", TokenCategory = "colors", RecommendedToken = "Color.Outline", Bindable = true },
            new StyledProperty { Name = "BorderWidth", Type = "double", TokenCategory = "borderWidth", RecommendedToken = "BorderWidth.None", Bindable = true },
            new StyledProperty { Name = "CornerRadius", Type = "int", TokenCategory = "borderRadius", RecommendedToken = "Radius.Button.Medium", Bindable = true },
            new StyledProperty { Name = "FontSize", Type = "double", TokenCategory = "typography", RecommendedToken = "FontSize.LabelLarge", Bindable = true },
            new StyledProperty { Name = "FontFamily", Type = "string", Bindable = true },
            new StyledProperty { Name = "FontAttributes", Type = "string", Bindable = true },
            new StyledProperty { Name = "Padding", Type = "thickness", TokenCategory = "spacing", Bindable = true },
            new StyledProperty { Name = "HeightRequest", Type = "double", Bindable = true },
        };

        foreach (var prop in inheritedProps)
        {
            if (!properties.Any(p => p.Name == prop.Name))
            {
                properties.Add(prop);
            }
        }
    }
}
