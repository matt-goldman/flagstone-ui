using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace FlagstoneUI.TokenGenerator;

/// <summary>
/// Generates design system contracts from FlagstoneUI source files.
/// </summary>
public class ContractGenerator
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly ControlSurfaceExtractor _extractor = new();

    /// <summary>
    /// Generates the minimal contract from source files.
    /// </summary>
    public async Task<string> GenerateMinimalContractAsync(string sourceDirectory)
    {
        Console.WriteLine("📋 Generating minimal contract...");

        var tokensPath = Path.Combine(sourceDirectory, "FlagstoneUI.Core", "Styles", "Tokens.xaml");
        var controlsPath = Path.Combine(sourceDirectory, "FlagstoneUI.Core", "Controls");

        // Extract base token schema
        var tokenSchema = ExtractTokenSchema(tokensPath);
        Console.WriteLine($"   ✓ Extracted {tokenSchema.Count} token definitions");

        // Extract control surfaces
        var controls = _extractor.ExtractFromDirectory(controlsPath);
        Console.WriteLine($"   ✓ Analyzed {controls.Count} controls");

        foreach (var (name, surface) in controls)
        {
            Console.WriteLine($"      → {name}: {surface.StyledProperties.Count} styled properties");
        }

        // Build the minimal contract
        var contract = new
        {
            Schema = "../schemas/design-system-contract.schema.json",
            Name = "minimal",
            Version = "1.0.0",
            Description = "The minimum viable contract for a valid FlagstoneUI theme. Requires implicit styles for all Fs* controls.",
            Layer = "theme",
            StylingSurface = new
            {
                Controls = controls.ToDictionary(
                    kvp => kvp.Key,
                    kvp => new
                    {
                        InheritsFrom = kvp.Value.InheritsFrom,
                        Architecture = kvp.Value.Architecture,
                        StyledProperties = kvp.Value.StyledProperties.Select(p => new
                        {
                            Name = p.Name,
                            Type = p.Type,
                            TokenCategory = p.TokenCategory,
                            RecommendedToken = p.RecommendedToken,
                            Bindable = p.Bindable
                        }).ToList()
                    }
                ),
                BaseTokenSchema = tokenSchema
            },
            RequiredStyles = controls.ToDictionary(
                kvp => kvp.Key,
                kvp => new { Implicit = true }
            ),
            Metadata = new
            {
                Author = "FlagstoneUI",
                License = "MIT",
                Repository = "https://github.com/AnnexCreative/flagstone-ui",
                Documentation = "https://flagstoneui.dev/docs/contracts/minimal"
            }
        };

        Console.WriteLine("📝 Generating JSON contract...");
        return JsonSerializer.Serialize(contract, _jsonOptions);
    }

    /// <summary>
    /// Generates a design system contract from an existing theme.
    /// </summary>
    public async Task<string> GenerateDesignSystemContractAsync(
        string themeXamlPath, 
        string contractName,
        string? extends = "minimal")
    {
        Console.WriteLine($"📋 Generating {contractName} contract from theme...");

        if (!File.Exists(themeXamlPath))
        {
            throw new FileNotFoundException($"Theme XAML not found: {themeXamlPath}");
        }

        // Parse theme XAML to extract named styles
        var namedStyles = ExtractNamedStyles(themeXamlPath);
        Console.WriteLine($"   ✓ Found {namedStyles.Sum(kvp => kvp.Value.Count)} named styles across {namedStyles.Count} controls");

        // Build required styles from extracted named styles
        var requiredStyles = new Dictionary<string, object>();
        foreach (var (controlName, styles) in namedStyles)
        {
            var styleRequirements = new Dictionary<string, object>
            {
                ["implicit"] = true
            };

            if (styles.Count > 0)
            {
                styleRequirements["named"] = styles.Select(s => new
                {
                    Name = s.Name,
                    Description = s.Description
                }).ToList();
            }

            requiredStyles[controlName] = styleRequirements;
        }

        var contract = new
        {
            Schema = "../schemas/design-system-contract.schema.json",
            Name = contractName,
            Version = "1.0.0",
            Description = $"{contractName} design system contract. Extends {extends} with named style variants.",
            Layer = "design-system",
            Extends = extends,
            RequiredStyles = requiredStyles,
            Metadata = new
            {
                Author = "FlagstoneUI",
                License = "MIT",
                GeneratedFrom = Path.GetFileName(themeXamlPath),
                GeneratedAt = DateTime.UtcNow.ToString("yyyy-MM-dd")
            }
        };

        Console.WriteLine("📝 Generating JSON contract...");
        return JsonSerializer.Serialize(contract, _jsonOptions);
    }

    /// <summary>
    /// Extracts the token schema from Tokens.xaml (keys and types only, no values).
    /// </summary>
    private Dictionary<string, object> ExtractTokenSchema(string tokensPath)
    {
        if (!File.Exists(tokensPath))
        {
            Console.WriteLine($"   ⚠️ Tokens.xaml not found at {tokensPath}");
            return new Dictionary<string, object>();
        }

        var doc = XDocument.Load(tokensPath);
        var root = doc.Root;
        if (root == null) return new Dictionary<string, object>();

        var xNs = root.GetNamespaceOfPrefix("x");
        var mauiNs = root.Name.Namespace;

        var schema = new Dictionary<string, object>();

        // Parse Color elements
        foreach (var element in doc.Descendants(mauiNs + "Color"))
        {
            var key = element.Attribute(xNs + "Key")?.Value;
            if (key != null && key.StartsWith("Color.", StringComparison.Ordinal))
            {
                schema[key] = new
                {
                    Type = "color",
                    Category = GetColorCategory(key),
                    Purpose = GetTokenPurpose(key)
                };
            }
        }

        // Parse x:Double elements
        foreach (var element in doc.Descendants(xNs + "Double"))
        {
            var key = element.Attribute(xNs + "Key")?.Value;
            if (key != null)
            {
                var (category, purpose) = GetNumericTokenInfo(key);
                schema[key] = new
                {
                    Type = "double",
                    Category = category,
                    Purpose = purpose
                };
            }
        }

        // Parse x:Int32 elements
        foreach (var element in doc.Descendants(xNs + "Int32"))
        {
            var key = element.Attribute(xNs + "Key")?.Value;
            if (key != null)
            {
                var (category, purpose) = GetNumericTokenInfo(key);
                schema[key] = new
                {
                    Type = "int",
                    Category = category,
                    Purpose = purpose
                };
            }
        }

        return schema;
    }

    private record NamedStyle(string Name, string? Description);

    /// <summary>
    /// Extracts named styles from a theme XAML file.
    /// </summary>
    private Dictionary<string, List<NamedStyle>> ExtractNamedStyles(string themeXamlPath)
    {
        var doc = XDocument.Load(themeXamlPath);
        var root = doc.Root;
        if (root == null) return new Dictionary<string, List<NamedStyle>>();

        var xNs = root.GetNamespaceOfPrefix("x");
        var mauiNs = root.Name.Namespace;

        var results = new Dictionary<string, List<NamedStyle>>();

        // Find all Style elements
        foreach (var styleElement in doc.Descendants(mauiNs + "Style"))
        {
            var targetType = styleElement.Attribute("TargetType")?.Value;
            var styleKey = styleElement.Attribute(xNs + "Key")?.Value;

            if (string.IsNullOrEmpty(targetType))
                continue;

            // Extract control name from TargetType (e.g., "fs:FsButton" -> "FsButton")
            var controlName = targetType.Contains(':', StringComparison.Ordinal) 
                ? targetType.Split(':').Last() 
                : targetType;

            // Only track Fs* controls
            if (!controlName.StartsWith("Fs", StringComparison.Ordinal))
                continue;

            if (!results.ContainsKey(controlName))
            {
                results[controlName] = new List<NamedStyle>();
            }

            // Named styles have x:Key, implicit styles don't
            if (!string.IsNullOrEmpty(styleKey))
            {
                // Look for description in comments above the style
                var description = GetStyleDescription(styleElement);
                results[controlName].Add(new NamedStyle(styleKey, description));
            }
        }

        return results;
    }

    private static string? GetStyleDescription(XElement styleElement)
    {
        // Try to find a comment before the style element
        var previousNode = styleElement.PreviousNode;
        while (previousNode != null)
        {
            if (previousNode is XComment comment)
            {
                var text = comment.Value.Trim();
                if (!text.StartsWith("=", StringComparison.Ordinal)) // Skip separator comments
                    return text;
            }
            else if (previousNode is XElement)
            {
                break; // Stop at previous element
            }
            previousNode = previousNode.PreviousNode;
        }
        return null;
    }

    private static string GetColorCategory(string key)
    {
        return key.ToLowerInvariant() switch
        {
            var s when s.Contains("primary", StringComparison.Ordinal) => "primary",
            var s when s.Contains("secondary", StringComparison.Ordinal) => "secondary",
            var s when s.Contains("tertiary", StringComparison.Ordinal) => "tertiary",
            var s when s.Contains("error", StringComparison.Ordinal) => "error",
            var s when s.Contains("surface", StringComparison.Ordinal) => "surface",
            var s when s.Contains("background", StringComparison.Ordinal) => "background",
            var s when s.Contains("outline", StringComparison.Ordinal) => "outline",
            var s when s.Contains("inverse", StringComparison.Ordinal) => "surface",
            _ => "other"
        };
    }

    private static (string category, string purpose) GetNumericTokenInfo(string key)
    {
        return key switch
        {
            var k when k.StartsWith("Space.") => ("spacing", $"Spacing token ({key.Split('.').Last()}dp)"),
            var k when k.StartsWith("FontSize.") => ("typography", $"Font size for {key.Split('.').Last()} text"),
            var k when k.StartsWith("Radius.Button.") => ("borderRadius", $"Button corner radius ({key.Split('.').Last()})"),
            var k when k.StartsWith("Radius.") => ("borderRadius", $"Corner radius ({key.Split('.').Last()})"),
            var k when k.StartsWith("BorderWidth.") => ("borderWidth", $"Border width ({key.Split('.').Last()})"),
            var k when k.StartsWith("Elevation.") => ("elevation", $"Elevation level {key.Split('.').Last()}"),
            var k when k.StartsWith("Padding.") => ("padding", $"Padding ({key.Split('.').Last()})"),
            var k when k.StartsWith("Opacity.") => ("opacity", $"Opacity for {key.Split('.').Last()} state"),
            var k when k.StartsWith("StateLayer.") => ("opacity", $"State layer opacity for {key.Split('.').Last()} state"),
            _ => ("other", $"Token {key}")
        };
    }

    private static string GetTokenPurpose(string key)
    {
        // Specific purposes for well-known tokens
        return key switch
        {
            "Color.Primary" => "Main brand color for primary actions",
            "Color.OnPrimary" => "Content color on primary surfaces",
            "Color.PrimaryContainer" => "Container color for primary elements",
            "Color.OnPrimaryContainer" => "Content color on primary containers",
            "Color.Secondary" => "Secondary brand color",
            "Color.OnSecondary" => "Content color on secondary surfaces",
            "Color.SecondaryContainer" => "Container color for secondary elements",
            "Color.OnSecondaryContainer" => "Content color on secondary containers",
            "Color.Tertiary" => "Tertiary accent color",
            "Color.OnTertiary" => "Content color on tertiary surfaces",
            "Color.TertiaryContainer" => "Container color for tertiary elements",
            "Color.OnTertiaryContainer" => "Content color on tertiary containers",
            "Color.Error" => "Error state color",
            "Color.OnError" => "Content color on error surfaces",
            "Color.ErrorContainer" => "Container color for error elements",
            "Color.OnErrorContainer" => "Content color on error containers",
            "Color.Surface" => "Default surface/card background",
            "Color.OnSurface" => "Content color on surfaces",
            "Color.SurfaceVariant" => "Alternative surface color",
            "Color.OnSurfaceVariant" => "Content color on surface variants",
            "Color.Background" => "Page/screen background",
            "Color.OnBackground" => "Content color on background",
            "Color.Outline" => "Border and divider color",
            "Color.OutlineVariant" => "Subtle border color",
            "Color.InverseSurface" => "Inverse surface (e.g., dark tooltip in light mode)",
            "Color.InverseOnSurface" => "Content on inverse surface",
            "Color.InversePrimary" => "Primary color for inverse surfaces",
            _ => $"Design token for {key}"
        };
    }
}
