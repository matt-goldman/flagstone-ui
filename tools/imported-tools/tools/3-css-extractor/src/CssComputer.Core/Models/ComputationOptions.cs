namespace CssComputer.Core.Models;

/// <summary>
/// Configuration options for CSS computation.
/// </summary>
public class ComputationOptions
{
    /// <summary>
    /// Tolerance for grouping similar numeric values (e.g., 0.1 for 10% tolerance).
    /// </summary>
    public double NumericTolerance { get; set; } = 0.0;

    /// <summary>
    /// Whether to emit optional CSS projection output.
    /// </summary>
    public bool EmitCssProjection { get; set; } = false;

    /// <summary>
    /// Whether to preserve source metadata in the output.
    /// </summary>
    public bool IncludeSourceMetadata { get; set; } = true;

    /// <summary>
    /// Maximum depth for variant detection.
    /// </summary>
    public int MaxVariantDepth { get; set; } = 2;

    /// <summary>
    /// Separate path to CSS source files (e.g., for Tailwind compiled output).
    /// When set, CSS rules are loaded from this path while HTML elements are loaded from the main input path.
    /// </summary>
    public string? CssSourcePath { get; set; }
}
