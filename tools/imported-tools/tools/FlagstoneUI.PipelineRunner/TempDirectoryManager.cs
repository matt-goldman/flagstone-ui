namespace FlagstoneUI.PipelineRunner;

/// <summary>
/// Manages temporary directories for pipeline runs.
/// </summary>
public static class TempDirectoryManager
{
    /// <summary>
    /// Base path for all pipeline temp directories.
    /// </summary>
    public static readonly string BasePath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".fs-pipeline");

    /// <summary>
    /// Default max age for stale directory cleanup.
    /// </summary>
    public static readonly TimeSpan DefaultMaxAge = TimeSpan.FromHours(24);

    /// <summary>
    /// Clean up stale temporary directories.
    /// </summary>
    /// <param name="maxAge">Maximum age of directories to keep. Directories older than this will be deleted.</param>
    /// <returns>Number of directories cleaned up.</returns>
    public static int CleanupStaleDirectories(TimeSpan? maxAge = null)
    {
        maxAge ??= DefaultMaxAge;

        if (!Directory.Exists(BasePath))
        {
            return 0;
        }

        var cutoff = DateTime.UtcNow - maxAge.Value;
        var cleanedCount = 0;

        foreach (var dir in Directory.GetDirectories(BasePath))
        {
            try
            {
                var dirInfo = new DirectoryInfo(dir);
                if (dirInfo.CreationTimeUtc < cutoff)
                {
                    Directory.Delete(dir, recursive: true);
                    cleanedCount++;
                }
            }
            catch (Exception)
            {
                // Ignore errors during cleanup - directory may be in use
            }
        }

        return cleanedCount;
    }

    /// <summary>
    /// Create a new working directory for a pipeline run.
    /// </summary>
    /// <returns>The path to the new working directory and its run ID.</returns>
    public static (string Path, string RunId) CreateWorkingDirectory()
    {
        // Use a short GUID for the run ID (first 8 characters)
        var runId = Guid.NewGuid().ToString("N")[..8];
        var path = Path.Combine(BasePath, runId);

        Directory.CreateDirectory(path);

        // Create a marker file with timestamp for age tracking
        var markerPath = Path.Combine(path, ".pipeline-run");
        File.WriteAllText(markerPath, DateTimeOffset.UtcNow.ToString("O"));

        return (path, runId);
    }

    /// <summary>
    /// Delete a working directory.
    /// </summary>
    /// <param name="path">Path to the working directory.</param>
    /// <returns>True if deleted successfully, false otherwise.</returns>
    public static bool DeleteWorkingDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
                return true;
            }
        }
        catch (Exception)
        {
            // Ignore errors - may be in use or already deleted
        }

        return false;
    }

    /// <summary>
    /// Get information about all existing pipeline run directories.
    /// </summary>
    public static IEnumerable<(string Path, string RunId, DateTime CreatedAt)> GetExistingRuns()
    {
        if (!Directory.Exists(BasePath))
        {
            yield break;
        }

        foreach (var dir in Directory.GetDirectories(BasePath))
        {
            var dirInfo = new DirectoryInfo(dir);
            yield return (dir, dirInfo.Name, dirInfo.CreationTimeUtc);
        }
    }
}
