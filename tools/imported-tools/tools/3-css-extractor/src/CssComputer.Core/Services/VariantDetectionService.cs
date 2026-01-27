using CssComputer.Core.Models;
using System.Security.Cryptography;
using System.Text;

namespace CssComputer.Core.Services;

/// <summary>
/// Stage 4: Detects variants where a base style exists with systematic differences.
/// </summary>
public class VariantDetectionService
{
    /// <summary>
    /// Detect variants in the style collection.
    /// Modifies styles in-place to add detected variants.
    /// </summary>
    public void DetectVariants(List<Style> styles, ComputationOptions options)
    {
        // Conservative variant detection
        // Only detect variants when differences are systematic and clear

        for (int i = 0; i < styles.Count; i++)
        {
            var baseStyle = styles[i];
            
            // Look for potential variants among remaining styles
            for (int j = i + 1; j < styles.Count; j++)
            {
                var potentialVariant = styles[j];

                if (IsVariantOf(baseStyle, potentialVariant, out var variantName, out var delta))
                {
                    // Add as variant instead of separate style
                    baseStyle.Variants ??= new List<StyleVariant>();
                    
                    baseStyle.Variants.Add(new StyleVariant
                    {
                        Name = variantName,
                        Properties = delta
                    });

                    // Merge metadata
                    if (baseStyle.Metadata != null && potentialVariant.Metadata != null)
                    {
                        var baseElements = (List<string>)baseStyle.Metadata["sourceElements"];
                        var variantElements = (List<string>)potentialVariant.Metadata["sourceElements"];
                        
                        if (!baseStyle.Metadata.ContainsKey("variants"))
                        {
                            baseStyle.Metadata["variants"] = new Dictionary<string, List<string>>();
                        }
                        
                        var variantMap = (Dictionary<string, List<string>>)baseStyle.Metadata["variants"];
                        variantMap[variantName] = variantElements;
                    }

                    // Remove the variant style (it's now part of base)
                    styles.RemoveAt(j);
                    j--;
                }
            }
        }
    }

    private bool IsVariantOf(
        Style baseStyle,
        Style potentialVariant,
        out string variantName,
        out Dictionary<string, object> delta)
    {
        variantName = "";
        delta = new Dictionary<string, object>();

        // Check if potentialVariant is mostly similar to base but with systematic differences
        var baseProps = baseStyle.Properties;
        var variantProps = potentialVariant.Properties;

        // Count matching, different, and missing properties
        int matching = 0;
        int different = 0;
        var differences = new Dictionary<string, object>();

        foreach (var key in baseProps.Keys)
        {
            if (variantProps.TryGetValue(key, out var variantValue))
            {
                if (Equals(baseProps[key], variantValue))
                {
                    matching++;
                }
                else
                {
                    different++;
                    differences[key] = variantValue;
                }
            }
        }

        // Properties in variant but not in base
        foreach (var key in variantProps.Keys)
        {
            if (!baseProps.ContainsKey(key))
            {
                different++;
                differences[key] = variantProps[key];
            }
        }

        // Heuristic: Consider it a variant if at least 70% of properties match
        // and there are only a few (1-3) differences
        int totalProps = baseProps.Count;
        double matchRatio = totalProps > 0 ? (double)matching / totalProps : 0;

        if (matchRatio >= 0.7 && different >= 1 && different <= 3)
        {
            delta = differences;
            variantName = InferVariantName(differences);
            return true;
        }

        return false;
    }

    private string InferVariantName(Dictionary<string, object> differences)
    {
        // Try to infer a meaningful variant name from the differences
        var keys = differences.Keys.ToList();

        if (keys.Count == 1)
        {
            var key = keys[0];
            
            // Common variant patterns
            if (key.Contains("color"))
                return "colored";
            if (key.Contains("size") || key.Contains("width") || key.Contains("height"))
                return "sized";
            if (key.Contains("padding") || key.Contains("margin"))
                return "spaced";
        }

        // Generic variant names - use deterministic hash of properties
        var propsString = string.Join(",", differences.OrderBy(kvp => kvp.Key)
            .Select(kvp => $"{kvp.Key}:{kvp.Value}"));
        var hash = ComputeDeterministicHash(propsString);
        return $"variant-{hash}";
    }

    private string ComputeDeterministicHash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes)[..8].ToLower();
    }
}
