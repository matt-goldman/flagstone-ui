namespace CssComputer.Core.Models;

/// <summary>
/// Represents a canonical style in the Design Language Specification.
/// </summary>
public class Style
{
    /// <summary>
    /// Unique identifier for this style.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Base properties for this style (resolved, normalized, canonical values).
    /// </summary>
    public required Dictionary<string, object> Properties { get; set; }

    /// <summary>
    /// Variants of this style (represented as deltas from base).
    /// </summary>
    public List<StyleVariant>? Variants { get; set; }

    /// <summary>
    /// Metadata about this style (elements, grouping decisions, etc.).
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}
