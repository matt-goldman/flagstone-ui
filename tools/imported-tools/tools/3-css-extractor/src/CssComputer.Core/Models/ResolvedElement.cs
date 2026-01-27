namespace CssComputer.Core.Models;

/// <summary>
/// Represents a visual element and its resolved styles before grouping.
/// </summary>
public class ResolvedElement
{
    /// <summary>
    /// Element identifier (from source).
    /// </summary>
    public required string ElementId { get; set; }

    /// <summary>
    /// Resolved CSS properties for this element.
    /// </summary>
    public required Dictionary<string, string> Properties { get; set; }

    /// <summary>
    /// Source information (file, line, selector, etc.).
    /// </summary>
    public Dictionary<string, string>? SourceInfo { get; set; }
}
