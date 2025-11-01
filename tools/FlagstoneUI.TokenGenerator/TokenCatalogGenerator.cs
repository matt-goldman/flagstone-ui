using System.Text.Json;
using System.Xml.Linq;

namespace FlagstoneUI.TokenGenerator;

/// <summary>
/// Generates a token catalog from Flagstone UI source files
/// </summary>
public class TokenCatalogGenerator
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<string> GenerateAsync(string sourceDirectory)
    {
        Console.WriteLine("📋 Parsing source files...");

        var tokensXamlPath = Path.Combine(sourceDirectory, "FlagstoneUI.Core", "Styles", "Tokens.xaml");
        if (!File.Exists(tokensXamlPath))
        {
            throw new FileNotFoundException($"Tokens.xaml not found at: {tokensXamlPath}");
        }

        // Parse base tokens from Tokens.xaml
        var baseTokens = ParseTokensXaml(tokensXamlPath);
        Console.WriteLine($"   ✓ Found {baseTokens.Count} base tokens");

        // Parse controls
        var controlsPath = Path.Combine(sourceDirectory, "FlagstoneUI.Core", "Controls");
        var controls = ParseControls(controlsPath);
        Console.WriteLine($"   ✓ Analyzed {controls.Count} controls");

        // Build catalog structure
        var catalog = new
        {
            Schema = "./tokens-schema.json",
            Version = "0.1.0",
            LastUpdated = DateTime.UtcNow.ToString("yyyy-MM-dd"),
            BaseTokens = baseTokens,
            Controls = controls,
            ThemeVariants = new
            {
                Material = new
                {
                    Description = "Material Design 3 theme implementation",
                    TokenOverrides = new Dictionary<string, object>()
                }
            }
        };

        Console.WriteLine("📝 Generating JSON catalog...");
        return JsonSerializer.Serialize(catalog, _jsonOptions);
    }

    private Dictionary<string, Dictionary<string, object>> ParseTokensXaml(string xamlPath)
    {
        var doc = XDocument.Load(xamlPath);
        var root = doc.Root;
        if (root == null) throw new InvalidOperationException("XAML root element not found");

        // MAUI uses the standard XAML namespace for x:
        var xNs = root.GetNamespaceOfPrefix("x");
        var mauiNs = root.Name.Namespace;

        var baseTokens = new Dictionary<string, Dictionary<string, object>>
        {
            ["colors"] = new(),
            ["spacing"] = new(),
            ["typography"] = new(),
            ["borderRadius"] = new(),
            ["borderWidth"] = new(),
            ["elevation"] = new(),
            ["padding"] = new(),
            ["opacity"] = new()
        };

        // Parse all resources - Color elements
        var colorElements = doc.Descendants(mauiNs + "Color").ToList();
        foreach (var element in colorElements)
        {
            var key = element.Attribute(xNs + "Key")?.Value;
            var value = element.Value;

            if (key != null && key.StartsWith("Color.", StringComparison.Ordinal))
            {
                baseTokens["colors"][key] = CreateColorToken(key, value);
            }
        }

        // Parse numeric tokens (spacing, typography, etc.)
        var doubleElements = doc.Descendants(xNs + "Double").ToList();
        foreach (var element in doubleElements)
        {
            var key = element.Attribute(xNs + "Key")?.Value;
            var value = element.Value;

            if (key == null) continue;

            if (double.TryParse(value, out var numericValue))
            {
                if (key.StartsWith("Space."))
                {
                    baseTokens["spacing"][key] = CreateNumericToken("spacing", numericValue, "dip", GetTokenPurpose(key));
                }
                else if (key.StartsWith("FontSize."))
                {
                    baseTokens["typography"][key] = CreateNumericToken("fontSize", numericValue, "sp", GetTokenPurpose(key));
                }
                else if (key.StartsWith("Radius."))
                {
                    baseTokens["borderRadius"][key] = CreateNumericToken("cornerRadius", numericValue, "dip", GetTokenPurpose(key));
                }
                else if (key.StartsWith("BorderWidth."))
                {
                    baseTokens["borderWidth"][key] = CreateNumericToken("borderWidth", numericValue, "dip", GetTokenPurpose(key));
                }
                else if (key.StartsWith("Elevation."))
                {
                    baseTokens["elevation"][key] = CreateNumericToken("elevation", numericValue, "dip", GetTokenPurpose(key));
                }
                else if (key.StartsWith("Padding."))
                {
                    baseTokens["padding"][key] = CreateNumericToken("padding", numericValue, "dip", GetTokenPurpose(key));
                }
                else if (key.StartsWith("Opacity.") || key.StartsWith("StateLayer."))
                {
                    baseTokens["opacity"][key] = CreateNumericToken("opacity", numericValue, "unitless", GetTokenPurpose(key));
                }
            }
        }

        return baseTokens;
    }

    private Dictionary<string, object> CreateColorToken(string key, string value)
    {
        var category = GetColorCategory(key);
        var purpose = GetTokenPurpose(key);
        var darkValue = GetDarkModeValue(key);

        var token = new Dictionary<string, object>
        {
            ["type"] = "color",
            ["defaultValue"] = value,
            ["purpose"] = purpose,
            ["category"] = category
        };

        if (darkValue != null)
        {
            token["darkValue"] = darkValue;
        }

        return token;
    }

    private Dictionary<string, object> CreateNumericToken(string type, double value, string unit, string purpose)
    {
        return new Dictionary<string, object>
        {
            ["type"] = type,
            ["value"] = value,
            ["unit"] = unit,
            ["purpose"] = purpose
        };
    }

    private Dictionary<string, object> ParseControls(string controlsPath)
    {
        var controls = new Dictionary<string, object>();

        // For now, return placeholder structure
        // TODO: Implement proper control analysis from .cs files
        var controlFiles = Directory.GetFiles(controlsPath, "Fs*.cs", SearchOption.TopDirectoryOnly);
        
        foreach (var file in controlFiles)
        {
            var controlName = Path.GetFileNameWithoutExtension(file);
            if (controlName.StartsWith("Fs", StringComparison.Ordinal) && !controlName.Contains("BorderlessEntry", StringComparison.Ordinal))
            {
                Console.WriteLine($"   → Analyzing {controlName}...");
                // Placeholder - will be implemented with proper Roslyn analysis
                controls[controlName] = new
                {
                    InheritsFrom = "Microsoft.Maui.Controls.Button", // TODO: Detect from code
                    Architecture = "subclass", // TODO: Detect from code
                    StyledProperties = new List<object>(),
                    CommonStyles = new List<object>()
                };
            }
        }

        return controls;
    }

    private string GetColorCategory(string key)
    {
        var parts = key.Split('.');
        if (parts.Length < 2 || string.IsNullOrWhiteSpace(parts[1]))
        {
            return "other";
        }
        return parts[1].ToLowerInvariant() switch
        {
            var s when s.Contains("primary", StringComparison.Ordinal) => "primary",
            var s when s.Contains("secondary", StringComparison.Ordinal) => "secondary",
            var s when s.Contains("tertiary", StringComparison.Ordinal) => "tertiary",
            var s when s.Contains("error", StringComparison.Ordinal) => "error",
            var s when s.Contains("surface", StringComparison.Ordinal) => "surface",
            var s when s.Contains("background", StringComparison.Ordinal) => "background",
            var s when s.Contains("outline", StringComparison.Ordinal) => "outline",
            var s when s.Contains("inverse", StringComparison.Ordinal) => "inverse",
            _ => "other"
        };
    }

    private string? GetDarkModeValue(string key)
    {
        // TODO: Load from Material theme or other source
        // For now, return known dark mode values for Material tokens
        return key switch
        {
            "Color.Primary" => "#D0BCFF",
            "Color.OnPrimary" => "#381E72",
            "Color.PrimaryContainer" => "#4F378B",
            "Color.OnPrimaryContainer" => "#EADDFF",
            "Color.Secondary" => "#CCC2DC",
            "Color.OnSecondary" => "#332D41",
            "Color.SecondaryContainer" => "#4A4458",
            "Color.OnSecondaryContainer" => "#E8DEF8",
            "Color.Tertiary" => "#EFB8C8",
            "Color.OnTertiary" => "#492532",
            "Color.TertiaryContainer" => "#633B48",
            "Color.OnTertiaryContainer" => "#FFD8E4",
            "Color.Error" => "#F2B8B5",
            "Color.OnError" => "#601410",
            "Color.ErrorContainer" => "#8C1D18",
            "Color.OnErrorContainer" => "#F9DEDC",
            "Color.Surface" => "#1C1B1F",
            "Color.OnSurface" => "#E6E1E5",
            "Color.SurfaceVariant" => "#49454F",
            "Color.OnSurfaceVariant" => "#CAC4D0",
            "Color.Background" => "#1C1B1F",
            "Color.OnBackground" => "#E6E1E5",
            "Color.Outline" => "#938F99",
            "Color.OutlineVariant" => "#49454F",
            "Color.InverseSurface" => "#E6E1E5",
            "Color.InverseOnSurface" => "#313033",
            "Color.InversePrimary" => "#6750A4",
            _ => null
        };
    }

    private string GetTokenPurpose(string key)
    {
        // TODO: Extract from XML comments or maintain a lookup table
        return key switch
        {
            "Color.Primary" => "Main brand color, used for primary actions and key UI elements",
            "Color.OnPrimary" => "Text and icons displayed on primary color",
            "Space.16" => "Standard spacing (most common)",
            "FontSize.BodyMedium" => "Medium body text (standard content)",
            "Radius.Small" => "Small rounding for buttons and inputs",
            _ => $"Design token for {key}"
        };
    }
}
