namespace FlagstoneUI.PipelineRunner;

/// <summary>
/// Configuration for a pipeline run.
/// </summary>
public class PipelineConfig
{
    /// <summary>
    /// Input directory containing the React/TSX prototype to process.
    /// </summary>
    public required string InputPath { get; init; }

    /// <summary>
    /// Output directory for final artifacts.
    /// </summary>
    public required string OutputPath { get; init; }

    /// <summary>
    /// If true, preserve the temp directory after run completes (for debugging).
    /// </summary>
    public bool PreserveTemp { get; init; }

    /// <summary>
    /// If true, skip cleanup of stale temp directories on startup.
    /// </summary>
    public bool NoCleanup { get; init; }

    /// <summary>
    /// If true, emit verbose progress output.
    /// </summary>
    public bool Verbose { get; init; }

    /// <summary>
    /// Optional: Explicit path to CSS source files (for Tool 3).
    /// If not specified, Tool 3 will look in standard build output locations.
    /// </summary>
    public string? CssSourcePath { get; init; }
}
