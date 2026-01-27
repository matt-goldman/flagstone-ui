namespace CssComputer.Core.Models;

/// <summary>
/// Report containing metadata about the computation process.
/// </summary>
public class ComputationReport
{
    /// <summary>
    /// Total number of elements processed.
    /// </summary>
    public int TotalElements { get; set; }

    /// <summary>
    /// Number of unique styles identified.
    /// </summary>
    public int UniqueStyles { get; set; }

    /// <summary>
    /// Number of variants detected.
    /// </summary>
    public int TotalVariants { get; set; }

    /// <summary>
    /// Summary of styles by semantic role (e.g., "button": 3, "heading": 5).
    /// Helps downstream tools understand what types of components have been extracted.
    /// </summary>
    public Dictionary<string, int> RoleSummary { get; set; } = new();

    /// <summary>
    /// Summary of styles by primary tag (e.g., "div": 10, "button": 3).
    /// </summary>
    public Dictionary<string, int> TagSummary { get; set; } = new();

    /// <summary>
    /// Elements that contributed to each style.
    /// </summary>
    public Dictionary<string, List<string>> StyleContributors { get; set; } = new();

    /// <summary>
    /// Styles that were grouped together.
    /// </summary>
    public List<GroupingDecision> GroupingDecisions { get; set; } = new();

    /// <summary>
    /// Conflicts or ambiguities detected.
    /// </summary>
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// Represents a decision to group multiple elements into a single style.
/// </summary>
public class GroupingDecision
{
    /// <summary>
    /// The resulting style ID.
    /// </summary>
    public required string StyleId { get; set; }

    /// <summary>
    /// Elements that were grouped.
    /// </summary>
    public required List<string> GroupedElements { get; set; }

    /// <summary>
    /// Reason for grouping.
    /// </summary>
    public required string Reason { get; set; }
}
