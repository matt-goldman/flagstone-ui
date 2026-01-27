using StructuralExtractor.Core.Models;

namespace StructuralExtractor.Core.Services;

/// <summary>
/// Main service for extracting application structure from React/JSX/TSX files or normalized HTML.
/// </summary>
public class StructuralExtractorService
{
    private readonly FileAnalyzer _fileAnalyzer;
    private readonly JsxParser _jsxParser;
    private readonly HtmlStructureParser _htmlParser;

    public StructuralExtractorService()
    {
        _fileAnalyzer = new FileAnalyzer();
        _jsxParser = new JsxParser();
        _htmlParser = new HtmlStructureParser();
    }

    /// <summary>
    /// Processes a directory or file and extracts the application structure.
    /// Automatically detects input type (HTML vs JSX/TSX).
    /// </summary>
    public async Task<ApplicationStructure> ExtractStructureAsync(string inputPath)
    {
        var structure = new ApplicationStructure();
        var files = GetRelevantFiles(inputPath);

        // Check if we're processing HTML files (output from Tool 1)
        var htmlFiles = files.Where(f => f.EndsWith(".html", StringComparison.OrdinalIgnoreCase)).ToList();
        
        if (htmlFiles.Count > 0)
        {
            // Process HTML files from Tool 1 output
            return await ExtractStructureFromHtmlAsync(htmlFiles);
        }

        // Original JSX/TSX processing path
        return await ExtractStructureFromJsxAsync(files);
    }

    /// <summary>
    /// Extracts structure from normalized HTML files (Tool 1 output).
    /// </summary>
    private async Task<ApplicationStructure> ExtractStructureFromHtmlAsync(List<string> htmlFiles)
    {
        var structure = new ApplicationStructure();
        var discoveredComponents = new HashSet<string>();

        foreach (var file in htmlFiles)
        {
            try
            {
                var content = await File.ReadAllTextAsync(file);
                var metadata = _htmlParser.ExtractMetadata(content);
                var rootStructure = _htmlParser.ParseHtmlStructure(content);

                // Collect all component references
                var componentRefs = _htmlParser.ExtractComponentReferences(rootStructure);
                foreach (var compRef in componentRefs)
                {
                    discoveredComponents.Add(compRef);
                }

                if (metadata.IsPage)
                {
                    // This is a page file
                    var pageName = metadata.ComponentName ?? Path.GetFileNameWithoutExtension(file);
                    var page = new PageDefinition
                    {
                        Route = metadata.InferRoute() ?? "/",
                        SourceFile = metadata.SourceFile ?? file,
                        Layout = rootStructure
                    };
                    structure.Pages[pageName] = page;
                }
                else if (!string.IsNullOrEmpty(metadata.ComponentName))
                {
                    // This is a component file
                    var component = new ComponentDefinition
                    {
                        Type = DetermineComponentType(rootStructure),
                        Children = rootStructure?.Children
                    };
                    structure.Components[metadata.ComponentName] = component;
                    discoveredComponents.Remove(metadata.ComponentName); // It's defined, not just referenced
                }
            }
            catch (Exception)
            {
                // Skip files that fail to process
                continue;
            }
        }

        // Add placeholder definitions for referenced but undefined components
        foreach (var componentName in discoveredComponents)
        {
            if (!structure.Components.ContainsKey(componentName))
            {
                structure.Components[componentName] = new ComponentDefinition
                {
                    Type = "component" // Unknown type, mark as generic
                };
            }
        }

        // Extract navigation structure
        structure.Navigation = ExtractNavigationStructure(structure);

        return structure;
    }

    /// <summary>
    /// Determines component type from its structure.
    /// </summary>
    private string DetermineComponentType(StructuralElement? structure)
    {
        if (structure == null)
            return "component";

        // Check the root element type
        var type = structure.Type?.ToLowerInvariant() ?? "";

        // Layout elements
        if (type is "div" or "section" or "main" or "aside" or "header" or "footer" or "nav" or "article")
            return "container";

        // Control elements
        if (type is "button" or "input" or "select" or "textarea" or "a")
            return "control";

        // Component reference
        if (structure.Ref != null)
            return "component";

        return "component";
    }

    /// <summary>
    /// Original JSX/TSX extraction path.
    /// </summary>
    private async Task<ApplicationStructure> ExtractStructureFromJsxAsync(List<string> files)
    {
        var structure = new ApplicationStructure();

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
    /// Supports both JSX/TSX and HTML (Tool 1 output).
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
            // Check for HTML files first (Tool 1 output)
            files.AddRange(Directory.GetFiles(inputPath, "*.html", SearchOption.AllDirectories)
                .Where(IsRelevantFile));
            
            // If no HTML files, fall back to JSX/TSX
            if (files.Count == 0)
            {
                files.AddRange(Directory.GetFiles(inputPath, "*.tsx", SearchOption.AllDirectories)
                    .Where(IsRelevantFile));
                files.AddRange(Directory.GetFiles(inputPath, "*.jsx", SearchOption.AllDirectories)
                    .Where(IsRelevantFile));
            }
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
        
        // Accept HTML, TSX, and JSX
        return fileName.EndsWith(".html", StringComparison.OrdinalIgnoreCase) ||
               fileName.EndsWith(".tsx") || 
               fileName.EndsWith(".jsx");
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
