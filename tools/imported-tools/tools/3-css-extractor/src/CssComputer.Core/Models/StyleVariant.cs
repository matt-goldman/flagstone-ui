namespace CssComputer.Core.Models;

/// <summary>
/// Represents a style variant as a delta from a base style.
/// </summary>
public class StyleVariant
{
    /// <summary>
    /// The name of the variant (e.g., "hover", "active", "large").
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Properties that differ from the base style (deltas only).
    /// </summary>
    public required Dictionary<string, object> Properties { get; set; }
}
