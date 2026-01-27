using System.Text.Json;
using System.Text.Json.Serialization;

namespace FlagstoneUI.PipelineRunner;

/// <summary>
/// Manifest describing the outputs of a pipeline run.
/// </summary>
public class PipelineManifest
{
    /// <summary>
    /// Unique identifier for this pipeline run.
    /// </summary>
    public required string RunId { get; init; }

    /// <summary>
    /// Timestamp when the pipeline started.
    /// </summary>
    public required DateTimeOffset StartedAt { get; init; }

    /// <summary>
    /// Timestamp when the pipeline completed.
    /// </summary>
    public DateTimeOffset? CompletedAt { get; set; }

    /// <summary>
    /// Total duration of the pipeline run.
    /// </summary>
    [JsonIgnore]
    public TimeSpan? Duration => CompletedAt.HasValue ? CompletedAt.Value - StartedAt : null;

    /// <summary>
    /// Duration in milliseconds (for JSON serialization).
    /// </summary>
    public long? DurationMs => Duration.HasValue ? (long)Duration.Value.TotalMilliseconds : null;

    /// <summary>
    /// Input path that was processed.
    /// </summary>
    public required string InputPath { get; init; }

    /// <summary>
    /// Output path where artifacts were written.
    /// </summary>
    public required string OutputPath { get; init; }

    /// <summary>
    /// Overall status of the pipeline run.
    /// </summary>
    public PipelineStatus Status { get; set; } = PipelineStatus.Running;

    /// <summary>
    /// Results from each stage of the pipeline.
    /// </summary>
    public List<StageResult> Stages { get; init; } = [];

    /// <summary>
    /// Output artifacts produced by the pipeline.
    /// </summary>
    public List<ArtifactInfo> Artifacts { get; init; } = [];

    /// <summary>
    /// Any errors that occurred during the run.
    /// </summary>
    public List<string> Errors { get; init; } = [];

    /// <summary>
    /// Serialize the manifest to JSON.
    /// </summary>
    public string ToJson()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        });
    }

    /// <summary>
    /// Write the manifest to a file.
    /// </summary>
    public async Task WriteAsync(string path)
    {
        await File.WriteAllTextAsync(path, ToJson());
    }
}

/// <summary>
/// Overall status of the pipeline.
/// </summary>
public enum PipelineStatus
{
    Running,
    Completed,
    CompletedWithWarnings,
    Failed
}

/// <summary>
/// Result from a single pipeline stage.
/// </summary>
public class StageResult
{
    /// <summary>
    /// Stage number (1-4).
    /// </summary>
    public required int StageNumber { get; init; }

    /// <summary>
    /// Human-readable stage name.
    /// </summary>
    public required string StageName { get; init; }

    /// <summary>
    /// Status of this stage.
    /// </summary>
    public StageStatus Status { get; set; } = StageStatus.Pending;

    /// <summary>
    /// Duration of this stage in milliseconds.
    /// </summary>
    public long? DurationMs { get; set; }

    /// <summary>
    /// Output path for this stage's artifacts.
    /// </summary>
    public string? OutputPath { get; set; }

    /// <summary>
    /// Any warnings from this stage.
    /// </summary>
    public List<string> Warnings { get; init; } = [];

    /// <summary>
    /// Error message if the stage failed.
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Additional metadata from the stage (e.g., counts, statistics).
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = [];
}

/// <summary>
/// Status of a pipeline stage.
/// </summary>
public enum StageStatus
{
    Pending,
    Running,
    Completed,
    Skipped,
    Failed
}

/// <summary>
/// Information about an output artifact.
/// </summary>
public class ArtifactInfo
{
    /// <summary>
    /// Type of artifact.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Relative path from output directory.
    /// </summary>
    public required string RelativePath { get; init; }

    /// <summary>
    /// File size in bytes.
    /// </summary>
    public long? SizeBytes { get; init; }

    /// <summary>
    /// Brief description of the artifact.
    /// </summary>
    public string? Description { get; init; }
}
