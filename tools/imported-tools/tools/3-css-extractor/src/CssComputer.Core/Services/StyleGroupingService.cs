using CssComputer.Core.Models;

namespace CssComputer.Core.Services;

/// <summary>
/// Stage 3: Groups elements into conceptual styles based on identical/similar properties.
/// </summary>
public class StyleGroupingService
{
    private readonly Dictionary<string, int> _styleNameCounts = new();

    /// <summary>
    /// Group normalized elements into canonical styles.
    /// </summary>
    public (List<Style> Styles, List<GroupingDecision> Decisions) GroupStyles(
        List<ResolvedElement> elements,
        ComputationOptions options)
    {
        // Clear counters to ensure deterministic style IDs per computation
        _styleNameCounts.Clear();
        
        var styles = new List<Style>();
        var decisions = new List<GroupingDecision>();

        // Group elements by identical property sets
        var propertyGroups = GroupByIdenticalProperties(elements);

        foreach (var group in propertyGroups)
        {
            var properties = ConvertToObjectDictionary(group.Properties);
            
            // Build rich metadata from source elements
            var metadata = BuildRichMetadata(group.Elements);
            
            // Generate semantic style ID from content
            var styleId = GenerateSemanticStyleId(metadata, properties);

            var style = new Style
            {
                Id = styleId,
                Properties = properties,
                Metadata = metadata
            };

            styles.Add(style);

            // Record grouping decision
            decisions.Add(new GroupingDecision
            {
                StyleId = styleId,
                GroupedElements = group.Elements.Select(e => e.ElementId).ToList(),
                Reason = "Identical property sets"
            });
        }

        // If tolerance is specified, attempt to merge similar styles
        if (options.NumericTolerance > 0)
        {
            MergeSimilarStyles(styles, decisions, options.NumericTolerance);
        }

        return (styles, decisions);
    }

    private List<PropertyGroup> GroupByIdenticalProperties(List<ResolvedElement> elements)
    {
        var groups = new Dictionary<string, PropertyGroup>();

        foreach (var element in elements)
        {
            var signature = CreatePropertySignature(element.Properties);

            if (!groups.TryGetValue(signature, out var group))
            {
                group = new PropertyGroup
                {
                    Properties = element.Properties,
                    Elements = new List<ResolvedElement>()
                };
                groups[signature] = group;
            }

            group.Elements.Add(element);
        }

        return groups.Values.ToList();
    }

    private string CreatePropertySignature(Dictionary<string, string> properties)
    {
        // Create a deterministic signature from sorted properties
        var sortedProps = properties.OrderBy(kvp => kvp.Key);
        return string.Join("|", sortedProps.Select(kvp => $"{kvp.Key}:{kvp.Value}"));
    }

    private Dictionary<string, object> ConvertToObjectDictionary(Dictionary<string, string> properties)
    {
        var result = new Dictionary<string, object>();
        foreach (var (key, value) in properties)
        {
            // Keep the original value - do not strip units
            // This preserves information like "10px" vs "10%" and prevents incorrect merging
            result[key] = value;
        }
        return result;
    }

    private void MergeSimilarStyles(
        List<Style> styles,
        List<GroupingDecision> decisions,
        double tolerance)
    {
        // Conservative merging - only merge if very similar
        // This is intentionally simple to avoid over-collapse
        // Real implementation would need more sophisticated similarity detection

        for (int i = 0; i < styles.Count - 1; i++)
        {
            for (int j = i + 1; j < styles.Count; j++)
            {
                if (AreSimilar(styles[i], styles[j], tolerance))
                {
                    // Merge j into i
                    MergeStyles(styles[i], styles[j], decisions);
                    styles.RemoveAt(j);
                    j--;
                }
            }
        }
    }

    private bool AreSimilar(Style style1, Style style2, double tolerance)
    {
        // Must have same set of properties
        if (style1.Properties.Count != style2.Properties.Count)
            return false;

        if (!style1.Properties.Keys.OrderBy(k => k).SequenceEqual(
             style2.Properties.Keys.OrderBy(k => k)))
            return false;

        // Check if values are within tolerance
        foreach (var key in style1.Properties.Keys)
        {
            var val1 = style1.Properties[key];
            var val2 = style2.Properties[key];

            // Try to parse numeric values with units for tolerance comparison
            if (val1 is string str1 && val2 is string str2 &&
                TryParseValueWithUnit(str1, out var num1, out var unit1) &&
                TryParseValueWithUnit(str2, out var num2, out var unit2) &&
                unit1 == unit2)
            {
                var diff = Math.Abs(num1 - num2);
                var avg = (Math.Abs(num1) + Math.Abs(num2)) / 2;
                // Handle the case where both values are zero or very close to zero
                if (diff < double.Epsilon)
                {
                    continue; // Both are effectively equal
                }
                if (avg > double.Epsilon && diff / avg > tolerance)
                    return false;
                continue;
            }
            
            // For non-numeric values or different units, must be exactly equal
            if (!Equals(val1, val2))
            {
                return false;
            }
        }

        return true;
    }

    private bool TryParseValueWithUnit(string value, out double number, out string unit)
    {
        // Try to extract numeric value and unit from strings like "10px", "1.5rem", "50%"
        var trimmedValue = value.Trim();
        
        // Try common CSS units (ordered by length descending to match longer units first, e.g., "rem" before "em")
        string[] units = { "vmin", "vmax", "rem", "px", "em", "vh", "vw", "pt", "pc", "cm", "mm", "in", "%" };
        
        // Use explicit LINQ filtering to find matching unit
        var matchingUnit = units.FirstOrDefault(u => trimmedValue.EndsWith(u));
        if (matchingUnit != null)
        {
            var numPart = trimmedValue[..^matchingUnit.Length];
            if (double.TryParse(numPart, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out number))
            {
                unit = matchingUnit;
                return true;
            }
        }
        
        // Try parsing as unitless number
        if (double.TryParse(trimmedValue, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out number))
        {
            unit = "";
            return true;
        }
        
        number = 0;
        unit = "";
        return false;
    }

    private void MergeStyles(Style target, Style source, List<GroupingDecision> decisions)
    {
        // Merge metadata
        if (target.Metadata != null && source.Metadata != null)
        {
            var targetElements = (List<string>)target.Metadata["sourceElements"];
            var sourceElements = (List<string>)source.Metadata["sourceElements"];
            targetElements.AddRange(sourceElements);
            target.Metadata["elementCount"] = targetElements.Count;
        }

        // Update decision
        var targetDecision = decisions.FirstOrDefault(d => d.StyleId == target.Id);
        var sourceDecision = decisions.FirstOrDefault(d => d.StyleId == source.Id);
        
        if (targetDecision != null && sourceDecision != null)
        {
            targetDecision.GroupedElements.AddRange(sourceDecision.GroupedElements);
            targetDecision.Reason = "Merged similar styles within tolerance";
            decisions.Remove(sourceDecision);
        }
    }

    private class PropertyGroup
    {
        public required Dictionary<string, string> Properties { get; set; }
        public required List<ResolvedElement> Elements { get; set; }
    }

    /// <summary>
    /// Build rich metadata from source elements for downstream tool consumption.
    /// </summary>
    private Dictionary<string, object> BuildRichMetadata(List<ResolvedElement> elements)
    {
        var metadata = new Dictionary<string, object>();
        
        // Basic counts
        metadata["elementCount"] = elements.Count;
        metadata["sourceElements"] = elements.Select(e => e.ElementId).ToList();
        
        // Tag distribution - helps downstream tools understand what this style applies to
        var tagCounts = elements
            .Where(e => e.SourceInfo?.ContainsKey("tag") == true)
            .GroupBy(e => e.SourceInfo!["tag"])
            .ToDictionary(g => g.Key, g => g.Count());
        
        if (tagCounts.Count > 0)
        {
            metadata["tagDistribution"] = tagCounts;
            metadata["primaryTag"] = tagCounts.OrderByDescending(kv => kv.Value).First().Key;
        }
        
        // Collect unique CSS class names from all elements
        var allClasses = elements
            .Where(e => e.SourceInfo?.ContainsKey("classes") == true && !string.IsNullOrWhiteSpace(e.SourceInfo["classes"]))
            .SelectMany(e => e.SourceInfo!["classes"].Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Distinct()
            .OrderBy(c => c)
            .ToList();
        
        if (allClasses.Count > 0)
        {
            metadata["cssClasses"] = allClasses;
        }
        
        // Collect component names (from data-component attribute or similar)
        var components = elements
            .Where(e => e.SourceInfo?.ContainsKey("component") == true && !string.IsNullOrWhiteSpace(e.SourceInfo["component"]))
            .Select(e => e.SourceInfo!["component"])
            .Distinct()
            .ToList();
        
        if (components.Count > 0)
        {
            metadata["components"] = components;
        }
        
        // Collect element IDs (HTML id attributes)
        var elementIds = elements
            .Where(e => e.SourceInfo?.ContainsKey("id") == true && !string.IsNullOrWhiteSpace(e.SourceInfo["id"]))
            .Select(e => e.SourceInfo!["id"])
            .Distinct()
            .ToList();
        
        if (elementIds.Count > 0)
        {
            metadata["htmlIds"] = elementIds;
        }
        
        // Source files
        var sourceFiles = elements
            .Where(e => e.SourceInfo?.ContainsKey("file") == true)
            .Select(e => Path.GetFileName(e.SourceInfo!["file"]))
            .Distinct()
            .ToList();
        
        if (sourceFiles.Count > 0)
        {
            metadata["sourceFiles"] = sourceFiles;
        }
        
        // Suggested semantic role based on tag and properties analysis
        metadata["suggestedRole"] = InferSemanticRole(elements, tagCounts);
        
        return metadata;
    }

    /// <summary>
    /// Infer a semantic role for downstream tools based on element characteristics.
    /// </summary>
    private string InferSemanticRole(List<ResolvedElement> elements, Dictionary<string, int> tagCounts)
    {
        if (tagCounts.Count == 0) return "unknown";
        
        var primaryTag = tagCounts.OrderByDescending(kv => kv.Value).First().Key;
        
        // Check for component hints in class names
        var allClasses = elements
            .Where(e => e.SourceInfo?.ContainsKey("classes") == true)
            .SelectMany(e => (e.SourceInfo!["classes"] ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Select(c => c.ToLower())
            .ToHashSet();
        
        // Check class names for semantic hints
        if (allClasses.Any(c => c.Contains("btn") || c.Contains("button"))) return "button";
        if (allClasses.Any(c => c.Contains("card"))) return "card";
        if (allClasses.Any(c => c.Contains("modal") || c.Contains("dialog"))) return "modal";
        if (allClasses.Any(c => c.Contains("nav"))) return "navigation";
        if (allClasses.Any(c => c.Contains("header"))) return "header";
        if (allClasses.Any(c => c.Contains("footer"))) return "footer";
        if (allClasses.Any(c => c.Contains("input") || c.Contains("field"))) return "input";
        if (allClasses.Any(c => c.Contains("label"))) return "label";
        if (allClasses.Any(c => c.Contains("badge"))) return "badge";
        if (allClasses.Any(c => c.Contains("avatar"))) return "avatar";
        if (allClasses.Any(c => c.Contains("icon"))) return "icon";
        if (allClasses.Any(c => c.Contains("list"))) return "list";
        if (allClasses.Any(c => c.Contains("table"))) return "table";
        if (allClasses.Any(c => c.Contains("form"))) return "form";
        if (allClasses.Any(c => c.Contains("link"))) return "link";
        if (allClasses.Any(c => c.Contains("container") || c.Contains("wrapper"))) return "container";
        if (allClasses.Any(c => c.Contains("flex") || c.Contains("grid"))) return "layout";
        
        // Fall back to tag-based inference
        return primaryTag switch
        {
            "button" => "button",
            "a" => "link",
            "h1" or "h2" or "h3" or "h4" or "h5" or "h6" => "heading",
            "p" => "paragraph",
            "span" => "text",
            "img" => "image",
            "input" or "textarea" or "select" => "input",
            "label" => "label",
            "form" => "form",
            "table" => "table",
            "th" or "td" => "table-cell",
            "tr" => "table-row",
            "ul" or "ol" => "list",
            "li" => "list-item",
            "nav" => "navigation",
            "header" => "header",
            "footer" => "footer",
            "main" => "main",
            "section" => "section",
            "article" => "article",
            "aside" => "sidebar",
            "div" => "container",
            "code" or "pre" => "code",
            "i" or "em" => "emphasis",
            "b" or "strong" => "strong",
            _ => "element"
        };
    }

    /// <summary>
    /// Generate a semantic style ID instead of arbitrary numbering.
    /// </summary>
    private string GenerateSemanticStyleId(Dictionary<string, object> metadata, Dictionary<string, object> properties)
    {
        var parts = new List<string>();
        
        // PRIORITY 1: Use component name if available and consistent
        // This gives us IDs like "Card-container" or "Button-primary"
        if (metadata.TryGetValue("components", out var componentsObj) && componentsObj is List<string> components && components.Count == 1)
        {
            // Single component - use it as the primary identifier
            parts.Add(components[0]);
        }
        else
        {
            // PRIORITY 2: Use primary tag if no single component
            if (metadata.TryGetValue("primaryTag", out var tagObj) && tagObj is string tag)
            {
                parts.Add(tag);
            }
        }
        
        // Add semantic role (but not if it duplicates the component/tag)
        if (metadata.TryGetValue("suggestedRole", out var roleObj) && roleObj is string role && role != "unknown" && role != "element")
        {
            var roleLower = role.ToLower();
            var existingLower = parts.FirstOrDefault()?.ToLower() ?? "";
            if (!existingLower.Contains(roleLower) && !roleLower.Contains(existingLower))
            {
                parts.Add(role);
            }
        }
        
        // Add key visual characteristics for differentiation
        if (properties.TryGetValue("display", out var display))
        {
            var displayStr = display.ToString()!;
            if (displayStr == "flex") parts.Add("flex");
            else if (displayStr == "grid") parts.Add("grid");
            else if (displayStr == "inline-block") parts.Add("inline");
        }
        
        // Check for specific patterns
        if (HasCenteredLayout(properties)) parts.Add("centered");
        if (properties.TryGetValue("position", out var position) && position.ToString() == "fixed") parts.Add("fixed");
        
        // Handle font-weight safely (can be numeric or keyword like "bold")
        if (properties.TryGetValue("font-weight", out var fontWeight))
        {
            var fontWeightValue = fontWeight.ToString();
            if (fontWeightValue == "bold" || fontWeightValue == "bolder" ||
                (double.TryParse(fontWeightValue, out var weight) && weight >= 700))
            {
                parts.Add("bold");
            }
        }
        
        // Build the base name
        var baseName = parts.Count > 0 ? string.Join("-", parts.Take(3)) : "style";
        
        // Ensure uniqueness with counter
        if (!_styleNameCounts.ContainsKey(baseName))
        {
            _styleNameCounts[baseName] = 0;
        }
        _styleNameCounts[baseName]++;
        
        var count = _styleNameCounts[baseName];
        return count == 1 ? baseName : $"{baseName}-{count}";
    }

    private bool HasCenteredLayout(Dictionary<string, object> properties)
    {
        // Check for centering patterns
        if (properties.TryGetValue("text-align", out var textAlign) && textAlign.ToString() == "center")
            return true;
        if (properties.TryGetValue("justify-content", out var jc) && jc.ToString() == "center")
            return true;
        if (properties.TryGetValue("align-items", out var ai) && ai.ToString() == "center")
            return true;
        if (properties.TryGetValue("margin", out var margin) && margin.ToString()?.Contains("auto") == true)
            return true;
        return false;
    }
}
