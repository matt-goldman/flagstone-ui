namespace CssComputer.Core.Models;

/// <summary>
/// The canonical Design Language Specification (DLS) output.
/// This is the authoritative, platform-agnostic representation of resolved visual design semantics.
/// </summary>
public class DesignLanguageSpecification
{
    /// <summary>
    /// Collection of all canonical styles in the design system.
    /// </summary>
    public required List<Style> Styles { get; set; }
}
