using StructuralExtractor.Core.Models;

namespace StructuralExtractor.Core.Services;

/// <summary>
/// Main service for extracting application structure from React/JSX/TSX files.
/// </summary>
public class StructuralExtractorService
{
    private readonly FileAnalyzer _fileAnalyzer;
    private readonly JsxParser _jsxParser;

    public StructuralExtractorService()
    {
        _fileAnalyzer = new FileAnalyzer();
        _jsxParser = new JsxParser();
    }

    /// <summary>
    /// Processes a directory or file and extracts the application structure.
    /// </summary>
    public async Task<ApplicationStructure> ExtractStructureAsync(string inputPath)
    {
        var structure = new ApplicationStructure();
        var files = GetRelevantFiles(inputPath);

        // First pass: Analyze all files
        var analysisResults = new List<FileAnalysisResult>();
        foreach (var file in files)
        {
            var result = await _fileAnalyzer.AnalyzeFileAsync(file);
            if (!result.HasErrors)
            {
                analysisResults.Add(result);
            }
        }

        // Second pass: Extract structure
        foreach (var analysis in analysisResults)
        {
            try
            {
                var content = await File.ReadAllTextAsync(analysis.FilePath);

                if (analysis.IsPage)
                {
                    // Extract page structure
                    var page = await ExtractPageStructureAsync(content, analysis);
                    if (page != null)
                    {
                        var pageName = GetPageName(analysis);
                        structure.Pages[pageName] = page;
                    }
                }

                // Extract components
                foreach (var componentName in analysis.ExportedComponents)
                {
                    var component = ExtractComponentStructure(content, componentName);
                    if (component != null)
                    {
                        structure.Components[componentName] = component;
                    }
                }
            }
            catch (Exception)
            {
                // Skip files that fail to process
                continue;
            }
        }

        // Extract navigation structure
        structure.Navigation = ExtractNavigationStructure(structure);

        return structure;
    }

    /// <summary>
    /// Gets all relevant files from the input path.
    /// </summary>
    private List<string> GetRelevantFiles(string inputPath)
    {
        var files = new List<string>();

        if (File.Exists(inputPath))
        {
            if (IsRelevantFile(inputPath))
            {
                files.Add(inputPath);
            }
        }
        else if (Directory.Exists(inputPath))
        {
            files.AddRange(Directory.GetFiles(inputPath, "*.tsx", SearchOption.AllDirectories)
                .Where(IsRelevantFile));
            files.AddRange(Directory.GetFiles(inputPath, "*.jsx", SearchOption.AllDirectories)
                .Where(IsRelevantFile));
        }

        return files;
    }

    /// <summary>
    /// Determines if a file should be processed.
    /// </summary>
    private bool IsRelevantFile(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        
        // Exclude test files
        if (fileName.Contains(".test.") || fileName.Contains(".spec."))
            return false;
        
        // Exclude story files
        if (fileName.Contains(".stories."))
            return false;
        
        // Only .tsx and .jsx
        return fileName.EndsWith(".tsx") || fileName.EndsWith(".jsx");
    }

    /// <summary>
    /// Extracts page structure from file content.
    /// </summary>
    private async Task<PageDefinition?> ExtractPageStructureAsync(string content, FileAnalysisResult analysis)
    {
        var page = new PageDefinition
        {
            Route = analysis.Route,
            SourceFile = analysis.FilePath
        };

        // Find the main component
        var componentName = analysis.ExportedComponents.FirstOrDefault();
        if (componentName == null)
            return null;

        // Extract JSX structure
        var jsx = _jsxParser.ExtractJsxFromFunction(content, componentName);
        if (jsx != null)
        {
            page.Layout = _jsxParser.ParseJsx(jsx);
        }

        return page;
    }

    /// <summary>
    /// Extracts component structure from file content.
    /// </summary>
    private ComponentDefinition? ExtractComponentStructure(string content, string componentName)
    {
        var component = new ComponentDefinition();

        // Extract JSX structure
        var jsx = _jsxParser.ExtractJsxFromFunction(content, componentName);
        if (jsx != null)
        {
            var structure = _jsxParser.ParseJsx(jsx);
            if (structure != null)
            {
                // Determine component type
                if (_jsxParser.IsLayoutElement(structure.Type))
                {
                    component.Type = "container";
                }
                else if (_jsxParser.IsControlElement(structure.Type))
                {
                    component.Type = "control";
                }
                else
                {
                    component.Type = "component";
                }

                component.Children = structure.Children;
                
                return component;
            }
        }

        return null;
    }

    /// <summary>
    /// Extracts navigation structure from the application.
    /// </summary>
    private NavigationStructure? ExtractNavigationStructure(ApplicationStructure structure)
    {
        var navigation = new NavigationStructure();

        // Find the root/home page
        var homePage = structure.Pages.FirstOrDefault(p => p.Value.Route == "/");
        if (homePage.Key != null)
        {
            navigation.Initial = homePage.Key;
        }

        // Extract links from pages
        var links = new List<NavigationLink>();
        // This would require more sophisticated parsing to extract Link components
        // For MVP, we'll keep it simple and just set the initial page

        navigation.Links = links.Count > 0 ? links : null;

        return navigation;
    }

    /// <summary>
    /// Generates a page name from the analysis result.
    /// </summary>
    private string GetPageName(FileAnalysisResult analysis)
    {
        // Use the exported component name if available
        var exportedName = analysis.ExportedComponents.FirstOrDefault();
        if (exportedName != null)
        {
            return exportedName;
        }

        // Generate from route
        if (analysis.Route != null)
        {
            if (analysis.Route == "/")
                return "HomePage";
            
            var routeParts = analysis.Route.TrimStart('/').Split('/')
                .Where(p => !string.IsNullOrEmpty(p))
                .Select(p => char.ToUpper(p[0]) + p.Substring(1));
            return string.Join("", routeParts) + "Page";
        }

        // Generate from file name
        var fileName = Path.GetFileNameWithoutExtension(analysis.FilePath);
        return fileName;
    }
}
