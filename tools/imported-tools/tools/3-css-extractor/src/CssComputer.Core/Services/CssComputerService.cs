using CssComputer.Core.Models;

namespace CssComputer.Core.Services;

/// <summary>
/// Main orchestrator for CSS computation and DLS extraction.
/// Coordinates the four-stage processing pipeline.
/// </summary>
public class CssComputerService
{
    private readonly StyleResolutionService _resolutionService;
    private readonly StyleNormalizationService _normalizationService;
    private readonly StyleGroupingService _groupingService;
    private readonly VariantDetectionService _variantService;

    public CssComputerService()
    {
        _resolutionService = new StyleResolutionService();
        _normalizationService = new StyleNormalizationService();
        _groupingService = new StyleGroupingService();
        _variantService = new VariantDetectionService();
    }

    /// <summary>
    /// Compute the canonical Design Language Specification from normalized prototype source.
    /// </summary>
    /// <param name="inputPath">Path to normalized prototype source</param>
    /// <param name="options">Computation options</param>
    /// <returns>DLS and computation report</returns>
    public async Task<(DesignLanguageSpecification Dls, ComputationReport Report)> ComputeAsync(
        string inputPath,
        ComputationOptions options)
    {
        var report = new ComputationReport();

        // Stage 1: Style Resolution
        // Resolve final computed styles by applying CSS cascade, utility classes, inline styles
        var resolvedElements = await _resolutionService.ResolveStylesAsync(inputPath, options);
        report.TotalElements = resolvedElements.Count;

        // Stage 2: Normalization
        // Canonicalize values, remove defaults, produce minimal property sets
        var normalizedElements = _normalizationService.NormalizeElements(resolvedElements);

        // Stage 3: Grouping
        // Group elements into conceptual styles based on identical/similar properties
        var (styles, groupingDecisions) = _groupingService.GroupStyles(normalizedElements, options);
        report.UniqueStyles = styles.Count;
        report.GroupingDecisions = groupingDecisions;

        // Build style contributors map
        foreach (var decision in groupingDecisions)
        {
            report.StyleContributors[decision.StyleId] = decision.GroupedElements;
        }

        // Build role and tag summaries for downstream tools
        foreach (var style in styles)
        {
            if (style.Metadata?.TryGetValue("suggestedRole", out var roleObj) == true && roleObj is string role)
            {
                report.RoleSummary.TryGetValue(role, out var roleCount);
                report.RoleSummary[role] = roleCount + 1;
            }
            
            if (style.Metadata?.TryGetValue("primaryTag", out var tagObj) == true && tagObj is string tag)
            {
                report.TagSummary.TryGetValue(tag, out var tagCount);
                report.TagSummary[tag] = tagCount + 1;
            }
        }

        // Stage 4: Variant Detection
        // Detect variants where base style exists with systematic differences
        _variantService.DetectVariants(styles, options);
        report.TotalVariants = styles.Sum(s => s.Variants?.Count ?? 0);

        var dls = new DesignLanguageSpecification
        {
            Styles = styles
        };

        return (dls, report);
    }

    /// <summary>
    /// Compute DLS from a single file.
    /// </summary>
    public async Task<(DesignLanguageSpecification Dls, ComputationReport Report)> ComputeFileAsync(
        string filePath,
        ComputationOptions options)
    {
        return await ComputeAsync(filePath, options);
    }

    /// <summary>
    /// Compute DLS from a directory tree.
    /// </summary>
    public async Task<(DesignLanguageSpecification Dls, ComputationReport Report)> ComputeDirectoryAsync(
        string directoryPath,
        ComputationOptions options)
    {
        return await ComputeAsync(directoryPath, options);
    }
}
