using System.Text.Json;
using ReactComponentFlattener.Core.Models;

namespace ReactComponentFlattener.Core.Services;

public enum UnusedComponentAction
{
    Retain,   // Keep unused components in output
    Remove,   // Don't include unused components in output
    Archive   // Move unused components to _archive folder (edit-in-place only)
}

public class FlatteningOptions
{
    public bool DryRun { get; set; }
    public bool PreserveComments { get; set; } = true;
    public int? MaxDepth { get; set; }
    public bool CopyCss { get; set; }
    public bool CopyAll { get; set; }
    public bool EditInPlace { get; set; }
    public UnusedComponentAction UnusedComponents { get; set; } = UnusedComponentAction.Remove;
    
    /// <summary>
    /// When true, emits HTML files instead of JSX/TSX.
    /// The HTML includes data-component attributes to preserve component metadata.
    /// This is useful for pipeline integration with downstream tools like Tool 3.
    /// </summary>
    public bool EmitHtml { get; set; }
}

public class ComponentFlattener(AcornimaParserService parserService)
{
    private readonly ComponentGraphBuilder _graphBuilder = new ComponentGraphBuilder();
    private readonly HtmlEmitterService _htmlEmitter = new HtmlEmitterService();

    private static readonly string[] ExcludePatterns = new[]
    {
        "node_modules",
        "bin",
        "obj",
        ".git",
        ".vs",
        ".vscode"
    };

    public async Task<(string FlattenedCode, FlatteningReport Report)> FlattenFileAsync(
        string filePath,
        FlatteningOptions options)
    {
        var code = await File.ReadAllTextAsync(filePath);
        
        // Analyze the file
        var analysis = await parserService.AnalyzeFileAsync(code);
        
        // Build component graph
        var graph = ComponentGraphBuilder.BuildGraph(analysis, filePath);
        
        // Determine what to flatten
        _graphBuilder.DetermineFlattening(graph);
        
        // Get components to flatten
        var componentsToFlatten = graph.Nodes
            .Where(n => n.Value.ShouldFlatten)
            .Select(n => n.Key)
            .ToList();
        
        // Generate report
        var report = GenerateReport(graph, filePath);
        
        // If dry run, return original code
        if (options.DryRun)
        {
            return (code, report);
        }
        
        // Flatten components if there are any
        string flattenedCode = code;
        if (componentsToFlatten.Any())
        {
            flattenedCode = await parserService.FlattenComponentsAsync(code, componentsToFlatten);
        }
        
        // If EmitHtml is enabled, convert the flattened JSX to HTML
        if (options.EmitHtml)
        {
            // Get the main component name (prefer exported, then first preserved)
            var mainComponentName = report.Preserved
                .FirstOrDefault(p => p.Reason?.Contains("exported") == true)?.Component
                ?? report.Preserved.FirstOrDefault()?.Component;
            
            flattenedCode = _htmlEmitter.ConvertToHtml(flattenedCode, filePath, mainComponentName);
        }
        
        return (flattenedCode, report);
    }

    public async Task ProcessDirectoryAsync(
        string inputPath,
        string outputPath,
        string? reportPath,
        FlatteningOptions options)
    {
        var allReports = new List<FlatteningReport>();
        var filesToArchive = new List<string>();
        
        // Find all JSX/TSX files, excluding node_modules and other common exclusions
        var files = Directory.GetFiles(inputPath, "*.*", SearchOption.AllDirectories)
            .Where(f => f.EndsWith(".jsx") || f.EndsWith(".tsx") || f.EndsWith(".js") || f.EndsWith(".ts"))
            .Where(f => !ShouldExcludePath(f, inputPath))
            .ToList();
        
        Console.WriteLine($"Found {files.Count} files to process");
        Console.WriteLine($"Mode: {(options.EditInPlace ? "Edit in place" : "Output to new location")}");
        Console.WriteLine($"Unused components: {options.UnusedComponents}");
        Console.WriteLine();
        
        foreach (var file in files)
        {
            try
            {
                Console.WriteLine($"Processing: {file}");
                
                var (flattenedCode, report) = await FlattenFileAsync(file, options);
                
                allReports.Add(report);
                
                // Determine if file should be retained based on whether it has any preserved components
                var hasPreservedComponents = report.Preserved.Any();
                var shouldRetainFile = hasPreservedComponents || options.UnusedComponents == UnusedComponentAction.Retain;
                
                // Write output
                if (!options.DryRun)
                {
                    if (options.EditInPlace)
                    {
                        // Edit in place mode
                        if (shouldRetainFile)
                        {
                            await File.WriteAllTextAsync(file, flattenedCode);
                        }
                        else if (options.UnusedComponents == UnusedComponentAction.Archive)
                        {
                            // Mark for archiving
                            filesToArchive.Add(file);
                        }
                        else // Remove
                        {
                            // Delete the file
                            File.Delete(file);
                            Console.WriteLine($"  Removed: {file} (all components flattened)");
                        }
                    }
                    else
                    {
                        // Output to new location mode
                        if (options.UnusedComponents == UnusedComponentAction.Retain || shouldRetainFile)
                        {
                            var relativePath = Path.GetRelativePath(inputPath, file);
                            var outputFile = Path.Combine(outputPath, relativePath);
                            
                            // Change extension to .html if emitting HTML
                            if (options.EmitHtml)
                            {
                                outputFile = Path.ChangeExtension(outputFile, ".html");
                            }
                            
                            var outputDir = Path.GetDirectoryName(outputFile);
                            
                            if (!string.IsNullOrEmpty(outputDir))
                            {
                                Directory.CreateDirectory(outputDir);
                            }
                            
                            await File.WriteAllTextAsync(outputFile, flattenedCode);
                        }
                        // else: Don't write file (remove mode - file not included in output)
                    }
                }
                
                // Report summary
                Console.WriteLine($"  Flattened: {report.Flattened.Count}, Preserved: {report.Preserved.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Error: {ex.Message}");
            }
        }
        
        // Handle archiving for edit-in-place mode
        if (!options.DryRun && options.EditInPlace && options.UnusedComponents == UnusedComponentAction.Archive && filesToArchive.Any())
        {
            Console.WriteLine($"\nArchiving {filesToArchive.Count} files with only flattened components...");
            
            foreach (var file in filesToArchive)
            {
                try
                {
                    var directory = Path.GetDirectoryName(file);
                    if (string.IsNullOrEmpty(directory)) continue;
                    
                    var archiveDir = Path.Combine(directory, "_archive");
                    Directory.CreateDirectory(archiveDir);
                    
                    var fileName = Path.GetFileName(file);
                    var archivePath = Path.Combine(archiveDir, fileName);
                    
                    // Move file to archive (overwrite if exists)
                    File.Move(file, archivePath, overwrite: true);
                    Console.WriteLine($"  Archived: {file} -> {archivePath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  Warning: Could not archive {file}: {ex.Message}");
                }
            }
        }
        
        // Copy CSS files if requested (unless CopyAll is enabled, which already copies everything)
        // Note: CSS copying only applies to output mode, not edit-in-place
        if (!options.EditInPlace && options.CopyCss && !options.CopyAll && !options.DryRun)
        {
            CopyCssFiles(inputPath, outputPath);
        }
        
        // Copy all non-processed files if requested
        // Note: CopyAll only applies to output mode, not edit-in-place
        if (!options.EditInPlace && options.CopyAll && !options.DryRun)
        {
            CopyAllFiles(inputPath, outputPath, files);
        }
        
        // Write combined report
        if (!string.IsNullOrEmpty(reportPath))
        {
            var combinedReport = new
            {
                timestamp = DateTime.UtcNow,
                filesProcessed = files.Count,
                mode = options.EditInPlace ? "edit-in-place" : "output",
                unusedComponentAction = options.UnusedComponents.ToString().ToLowerInvariant(),
                reports = allReports
            };
            
            var reportJson = JsonSerializer.Serialize(combinedReport, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            
            await File.WriteAllTextAsync(reportPath, reportJson);
            Console.WriteLine($"\nReport written to: {reportPath}");
        }
    }
    
    private static void CopyCssFiles(string inputPath, string outputPath)
    {
        var cssFiles = Directory.GetFiles(inputPath, "*.*", SearchOption.AllDirectories)
            .Where(f => f.EndsWith(".css", StringComparison.OrdinalIgnoreCase))
            .ToList();
        
        Console.WriteLine($"\nCopying {cssFiles.Count} CSS files...");
        
        foreach (var file in cssFiles)
        {
            var relativePath = Path.GetRelativePath(inputPath, file);
            var outputFile = Path.Combine(outputPath, relativePath);
            var outputDir = Path.GetDirectoryName(outputFile);
            
            if (!string.IsNullOrEmpty(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }
            
            File.Copy(file, outputFile, true);
            Console.WriteLine($"  Copied: {relativePath}");
        }
    }
    
    private static void CopyAllFiles(string inputPath, string outputPath, List<string> processedFiles)
    {
        var allFiles = Directory.GetFiles(inputPath, "*.*", SearchOption.AllDirectories).ToList();
        var filesToCopy = allFiles.Except(processedFiles).ToList();
        
        Console.WriteLine($"\nCopying {filesToCopy.Count} additional files...");
        
        foreach (var file in filesToCopy)
        {
            var relativePath = Path.GetRelativePath(inputPath, file);
            var outputFile = Path.Combine(outputPath, relativePath);
            var outputDir = Path.GetDirectoryName(outputFile);
            
            if (!string.IsNullOrEmpty(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }
            
            File.Copy(file, outputFile, true);
            Console.WriteLine($"  Copied: {relativePath}");
        }
    }

    private FlatteningReport GenerateReport(ComponentGraph graph, string filePath)
    {
        var report = new FlatteningReport();
        
        foreach (var (name, node) in graph.Nodes)
        {
            if (node.ShouldFlatten)
            {
                report.Flattened.Add(new FlattenedComponent
                {
                    Component = name,
                    Reason = node.FlatteningReason,
                    OriginalFile = filePath,
                    NewLocation = node.UsageLocations.FirstOrDefault(),
                    LineRange = node.Info.Loc != null ? new LineRange
                    {
                        Start = node.Info.Loc.Start?.Line ?? 0,
                        End = node.Info.Loc.End?.Line ?? 0
                    } : null
                });
            }
            else
            {
                report.Preserved.Add(new PreservedComponent
                {
                    Component = name,
                    Reason = node.FlatteningReason,
                    File = filePath
                });
            }
        }
        
        return report;
    }

    private static bool ShouldExcludePath(string filePath, string inputPath)
    {
        // Normalize paths for comparison
        var relativePath = Path.GetRelativePath(inputPath, filePath);
        var pathParts = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        
        // Check if any part of the path matches an exclude pattern
        return pathParts.Any(part => ExcludePatterns.Contains(part, StringComparer.OrdinalIgnoreCase));
    }
}
