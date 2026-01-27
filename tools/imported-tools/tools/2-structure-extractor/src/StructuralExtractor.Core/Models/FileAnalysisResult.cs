namespace StructuralExtractor.Core.Models;

/// <summary>
/// Result of analyzing a single file for structural content.
/// </summary>
public class FileAnalysisResult
{
    /// <summary>
    /// File path that was analyzed.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Whether this file represents a page.
    /// </summary>
    public bool IsPage { get; set; }

    /// <summary>
    /// Whether this file exports components.
    /// </summary>
    public bool HasComponents { get; set; }

    /// <summary>
    /// Detected route for this file (if it's a page).
    /// </summary>
    public string? Route { get; set; }

    /// <summary>
    /// Exported component names from this file.
    /// </summary>
    public List<string> ExportedComponents { get; set; } = new();

    /// <summary>
    /// Whether analysis encountered errors.
    /// </summary>
    public bool HasErrors { get; set; }

    /// <summary>
    /// Error message if analysis failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
