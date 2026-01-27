using System.Diagnostics;
using System.Text.Json;
using CssComputer.Core.Models;
using CssComputer.Core.Services;
using ReactComponentFlattener.Core.Services;
using StructuralExtractor.Core.Services;

namespace FlagstoneUI.PipelineRunner;

/// <summary>
/// Orchestrates the full pipeline from prototype to design tokens.
/// </summary>
public class PipelineOrchestrator
{
    private readonly Action<string>? _logger;
    private readonly bool _verbose;

    public PipelineOrchestrator(bool verbose = false, Action<string>? logger = null)
    {
        _verbose = verbose;
        _logger = logger ?? Console.WriteLine;
    }

    /// <summary>
    /// Run the full pipeline.
    /// </summary>
    public async Task<PipelineManifest> RunAsync(PipelineConfig config)
    {
        var (tempDir, runId) = TempDirectoryManager.CreateWorkingDirectory();

        var manifest = new PipelineManifest
        {
            RunId = runId,
            StartedAt = DateTimeOffset.UtcNow,
            InputPath = Path.GetFullPath(config.InputPath),
            OutputPath = Path.GetFullPath(config.OutputPath)
        };

        Log($"Pipeline Run: {runId}");
        Log($"  Input:  {manifest.InputPath}");
        Log($"  Output: {manifest.OutputPath}");
        Log($"  Temp:   {tempDir}");
        Log("");

        try
        {
            // Stage 1: Normalize
            var stage1Result = await RunStage1NormalizeAsync(config, tempDir);
            manifest.Stages.Add(stage1Result);

            if (stage1Result.Status == StageStatus.Failed)
            {
                throw new PipelineException("Stage 1 (Normalize) failed", stage1Result.Error);
            }

            // Stage 2: Extract Structure
            var stage2Result = await RunStage2StructureAsync(stage1Result.OutputPath!, tempDir);
            manifest.Stages.Add(stage2Result);

            if (stage2Result.Status == StageStatus.Failed)
            {
                throw new PipelineException("Stage 2 (Structure) failed", stage2Result.Error);
            }

            // Stage 3: Compute CSS/DLS
            // Use explicit CssSourcePath if provided, otherwise fall back to original input path
            // (the normalized output doesn't contain .next/dist build artifacts with compiled CSS)
            var effectiveCssSourcePath = config.CssSourcePath ?? config.InputPath;
            var stage3Result = await RunStage3CssComputeAsync(stage1Result.OutputPath!, effectiveCssSourcePath, tempDir);
            manifest.Stages.Add(stage3Result);

            if (stage3Result.Status == StageStatus.Failed)
            {
                throw new PipelineException("Stage 3 (CSS Compute) failed", stage3Result.Error);
            }

            // Stage 4: Token Generation (placeholder for now)
            var stage4Result = await RunStage4TokenGeneratorAsync(stage3Result.OutputPath!, tempDir);
            manifest.Stages.Add(stage4Result);

            // Even if stage 4 fails, we still have useful outputs from earlier stages

            // Copy outputs to final destination
            await CopyOutputsAsync(tempDir, config.OutputPath, manifest);

            // Determine overall status
            manifest.Status = manifest.Stages.Any(s => s.Status == StageStatus.Failed)
                ? PipelineStatus.Failed
                : manifest.Stages.Any(s => s.Warnings.Count > 0)
                    ? PipelineStatus.CompletedWithWarnings
                    : PipelineStatus.Completed;
        }
        catch (PipelineException ex)
        {
            manifest.Status = PipelineStatus.Failed;
            manifest.Errors.Add(ex.Message);
            if (ex.Details != null)
            {
                manifest.Errors.Add(ex.Details);
            }
            Log($"❌ Pipeline failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            manifest.Status = PipelineStatus.Failed;
            manifest.Errors.Add($"Unexpected error: {ex.Message}");
            Log($"❌ Unexpected error: {ex.Message}");
            if (_verbose)
            {
                Log(ex.StackTrace ?? "");
            }
        }
        finally
        {
            manifest.CompletedAt = DateTimeOffset.UtcNow;

            // Write manifest to output
            var manifestPath = Path.Combine(config.OutputPath, "pipeline-manifest.json");
            Directory.CreateDirectory(config.OutputPath);
            await manifest.WriteAsync(manifestPath);
            Log($"\n📋 Manifest written to: {manifestPath}");

            // Clean up temp directory unless preserving
            if (!config.PreserveTemp)
            {
                TempDirectoryManager.DeleteWorkingDirectory(tempDir);
                Log($"🗑️  Temp directory cleaned up");
            }
            else
            {
                Log($"📁 Temp directory preserved: {tempDir}");
            }
        }

        return manifest;
    }

    private async Task<StageResult> RunStage1NormalizeAsync(PipelineConfig config, string tempDir)
    {
        var result = new StageResult
        {
            StageNumber = 1,
            StageName = "Normalize (React Component Flattener)",
            Status = StageStatus.Running
        };

        var sw = Stopwatch.StartNew();
        var outputPath = Path.Combine(tempDir, "1-normalized");

        try
        {
            Log("📦 Stage 1: Normalizing prototype...");

            var parserService = new AcornimaParserService();
            var flattener = new ComponentFlattener(parserService);

            var options = new FlatteningOptions
            {
                DryRun = false,
                PreserveComments = true,
                CopyCss = true,           // Copy CSS for Stage 3
                CopyAll = true,           // Copy all files to preserve structure
                EditInPlace = false,
                UnusedComponents = UnusedComponentAction.Remove,
                EmitHtml = true           // Emit HTML for better downstream processing
            };

            var reportPath = Path.Combine(tempDir, "1-normalization-report.json");
            
            await flattener.ProcessDirectoryAsync(
                config.InputPath,
                outputPath,
                reportPath,
                options);

            // Read report to get metadata
            if (File.Exists(reportPath))
            {
                var reportJson = await File.ReadAllTextAsync(reportPath);
                var report = JsonDocument.Parse(reportJson);
                
                if (report.RootElement.TryGetProperty("filesProcessed", out var filesProcessed))
                {
                    result.Metadata["filesProcessed"] = filesProcessed.GetInt32();
                }
            }

            result.OutputPath = outputPath;
            result.Status = StageStatus.Completed;
            Log($"   ✅ Normalized to: {outputPath}");
        }
        catch (Exception ex)
        {
            result.Status = StageStatus.Failed;
            result.Error = ex.Message;
            Log($"   ❌ Failed: {ex.Message}");
        }

        sw.Stop();
        result.DurationMs = sw.ElapsedMilliseconds;
        return result;
    }

    private async Task<StageResult> RunStage2StructureAsync(string normalizedPath, string tempDir)
    {
        var result = new StageResult
        {
            StageNumber = 2,
            StageName = "Extract Structure",
            Status = StageStatus.Running
        };

        var sw = Stopwatch.StartNew();
        var outputPath = Path.Combine(tempDir, "2-structure.json");

        try
        {
            Log("🏗️  Stage 2: Extracting structure...");

            var extractor = new StructuralExtractorService();
            var outputService = new OutputService();

            var structure = await extractor.ExtractStructureAsync(normalizedPath);

            await outputService.WriteStructureAsync(structure, outputPath, OutputFormat.Json);

            result.Metadata["componentsFound"] = structure.Components.Count;
            result.Metadata["pagesFound"] = structure.Pages.Count;

            result.OutputPath = outputPath;
            result.Status = StageStatus.Completed;
            Log($"   ✅ Structure extracted: {structure.Components.Count} components, {structure.Pages.Count} pages");
        }
        catch (Exception ex)
        {
            result.Status = StageStatus.Failed;
            result.Error = ex.Message;
            Log($"   ❌ Failed: {ex.Message}");
        }

        sw.Stop();
        result.DurationMs = sw.ElapsedMilliseconds;
        return result;
    }

    private async Task<StageResult> RunStage3CssComputeAsync(string normalizedPath, string? cssSourcePath, string tempDir)
    {
        var result = new StageResult
        {
            StageNumber = 3,
            StageName = "CSS Compute (DLS Extraction)",
            Status = StageStatus.Running
        };

        var sw = Stopwatch.StartNew();
        var outputPath = Path.Combine(tempDir, "3-dls.json");

        try
        {
            Log("🎨 Stage 3: Computing CSS / extracting DLS...");

            var computerService = new CssComputerService();

            var options = new ComputationOptions
            {
                NumericTolerance = 0.0,
                EmitCssProjection = false,
                IncludeSourceMetadata = true,
                CssSourcePath = cssSourcePath
            };

            var (dls, report) = await computerService.ComputeAsync(normalizedPath, options);

            // Write DLS
            var dlsJson = JsonSerializer.Serialize(dls, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            await File.WriteAllTextAsync(outputPath, dlsJson);

            // Write report
            var reportPath = Path.Combine(tempDir, "3-computation-report.json");
            var reportJson = JsonSerializer.Serialize(report, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            await File.WriteAllTextAsync(reportPath, reportJson);

            result.Metadata["totalElements"] = report.TotalElements;
            result.Metadata["uniqueStyles"] = report.UniqueStyles;
            result.Metadata["totalVariants"] = report.TotalVariants;

            result.OutputPath = outputPath;
            result.Status = StageStatus.Completed;
            Log($"   ✅ DLS extracted: {report.UniqueStyles} styles, {report.TotalVariants} variants");
        }
        catch (Exception ex)
        {
            result.Status = StageStatus.Failed;
            result.Error = ex.Message;
            Log($"   ❌ Failed: {ex.Message}");
        }

        sw.Stop();
        result.DurationMs = sw.ElapsedMilliseconds;
        return result;
    }

    private async Task<StageResult> RunStage4TokenGeneratorAsync(string dlsPath, string tempDir)
    {
        var result = new StageResult
        {
            StageNumber = 4,
            StageName = "Token Generation",
            Status = StageStatus.Running
        };

        var sw = Stopwatch.StartNew();
        var outputPath = Path.Combine(tempDir, "4-tokens");

        try
        {
            Log("🔧 Stage 4: Generating tokens...");

            Directory.CreateDirectory(outputPath);

            // TODO: Integrate with FlagstoneUI.TokenGenerator
            // For now, this is a placeholder that copies the DLS and notes what would happen
            
            // The TokenGenerator has several commands:
            // - generate: XAML → JSON (extracts from existing XAML)
            // - validate: validates XAML or JSON tokens
            // - xaml: JSON → XAML (generates XAML from JSON)
            //
            // For this pipeline, we need a new path: DLS → Tokens
            // This would involve:
            // 1. Analyzing DLS styles to extract color/spacing/typography patterns
            // 2. Mapping patterns to token definitions
            // 3. Generating tokens-catalog.json
            // 4. Optionally generating Tokens.xaml
            
            // For now, just copy the DLS as a starting point
            var tokenInputPath = Path.Combine(outputPath, "input-dls.json");
            File.Copy(dlsPath, tokenInputPath);

            // Create a placeholder tokens file
            var placeholderTokens = new
            {
                note = "Token generation from DLS not yet implemented",
                dlsSource = "input-dls.json",
                nextSteps = new[]
                {
                    "Analyze DLS styles for color patterns",
                    "Extract spacing values",
                    "Identify typography scales",
                    "Map to Flagstone UI token schema",
                    "Generate tokens-catalog.json",
                    "Generate Tokens.xaml"
                }
            };

            var placeholderPath = Path.Combine(outputPath, "tokens-placeholder.json");
            await File.WriteAllTextAsync(placeholderPath, JsonSerializer.Serialize(placeholderTokens, new JsonSerializerOptions
            {
                WriteIndented = true
            }));

            result.Warnings.Add("Token generation from DLS not yet implemented - placeholder created");
            result.OutputPath = outputPath;
            result.Status = StageStatus.Completed;
            Log($"   ⚠️  Token generation placeholder created (integration pending)");
        }
        catch (Exception ex)
        {
            result.Status = StageStatus.Failed;
            result.Error = ex.Message;
            Log($"   ❌ Failed: {ex.Message}");
        }

        sw.Stop();
        result.DurationMs = sw.ElapsedMilliseconds;
        return result;
    }

    private async Task CopyOutputsAsync(string tempDir, string outputPath, PipelineManifest manifest)
    {
        Log("\n📤 Copying outputs to final destination...");

        Directory.CreateDirectory(outputPath);

        // Copy each stage's output
        var filesToCopy = new List<(string Source, string RelativePath, string Type, string Description)>
        {
            (Path.Combine(tempDir, "1-normalization-report.json"), "reports/1-normalization-report.json", "report", "Normalization stage report"),
            (Path.Combine(tempDir, "2-structure.json"), "structure.json", "structure", "Application structure"),
            (Path.Combine(tempDir, "3-dls.json"), "dls.json", "dls", "Design Language Specification"),
            (Path.Combine(tempDir, "3-computation-report.json"), "reports/3-computation-report.json", "report", "CSS computation report"),
            (Path.Combine(tempDir, "4-tokens", "input-dls.json"), "tokens/input-dls.json", "intermediate", "DLS input for token generation"),
            (Path.Combine(tempDir, "4-tokens", "tokens-placeholder.json"), "tokens/tokens-placeholder.json", "placeholder", "Token generation placeholder")
        };

        foreach (var (source, relativePath, type, description) in filesToCopy)
        {
            if (File.Exists(source))
            {
                var destPath = Path.Combine(outputPath, relativePath);
                var destDir = Path.GetDirectoryName(destPath);
                if (!string.IsNullOrEmpty(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }

                File.Copy(source, destPath, overwrite: true);

                var fileInfo = new FileInfo(destPath);
                manifest.Artifacts.Add(new ArtifactInfo
                {
                    Type = type,
                    RelativePath = relativePath,
                    SizeBytes = fileInfo.Length,
                    Description = description
                });

                LogVerbose($"   Copied: {relativePath}");
            }
        }

        // Copy normalized sources if they exist
        var normalizedDir = Path.Combine(tempDir, "1-normalized");
        if (Directory.Exists(normalizedDir))
        {
            var normalizedOutputDir = Path.Combine(outputPath, "normalized");
            await CopyDirectoryAsync(normalizedDir, normalizedOutputDir);

            manifest.Artifacts.Add(new ArtifactInfo
            {
                Type = "normalized-source",
                RelativePath = "normalized/",
                Description = "Normalized prototype source files"
            });

            LogVerbose($"   Copied: normalized/");
        }

        Log($"   ✅ {manifest.Artifacts.Count} artifacts written to: {outputPath}");
    }

    private static async Task CopyDirectoryAsync(string sourceDir, string destDir)
    {
        Directory.CreateDirectory(destDir);

        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var destFile = Path.Combine(destDir, Path.GetFileName(file));
            File.Copy(file, destFile, overwrite: true);
        }

        foreach (var dir in Directory.GetDirectories(sourceDir))
        {
            var destSubDir = Path.Combine(destDir, Path.GetFileName(dir));
            await CopyDirectoryAsync(dir, destSubDir);
        }
    }

    private void Log(string message)
    {
        _logger?.Invoke(message);
    }

    private void LogVerbose(string message)
    {
        if (_verbose)
        {
            _logger?.Invoke(message);
        }
    }
}

/// <summary>
/// Exception thrown when a pipeline stage fails.
/// </summary>
public class PipelineException : Exception
{
    public string? Details { get; }

    public PipelineException(string message, string? details = null) : base(message)
    {
        Details = details;
    }
}
