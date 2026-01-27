using System.Text.RegularExpressions;
using StructuralExtractor.Core.Models;

namespace StructuralExtractor.Core.Services;

/// <summary>
/// Analyzes React/JSX/TSX files to identify pages, components, and routes.
/// </summary>
public partial class FileAnalyzer
{
    [GeneratedRegex(@"export\s+default\s+function\s+(\w+)", RegexOptions.Compiled)]
    private static partial Regex DefaultExportFunctionRegex();
    
    [GeneratedRegex(@"export\s+function\s+(\w+)", RegexOptions.Compiled)]
    private static partial Regex NamedExportFunctionRegex();
    
    [GeneratedRegex(@"export\s+(?:const|let)\s+(\w+)\s*=", RegexOptions.Compiled)]
    private static partial Regex ExportConstRegex();

    /// <summary>
    /// Determines if a file path represents a page based on naming conventions.
    /// </summary>
    public bool IsPageFile(string filePath)
    {
        var fileName = Path.GetFileName(filePath).ToLowerInvariant();
        var directoryName = Path.GetFileName(Path.GetDirectoryName(filePath) ?? "").ToLowerInvariant();
        
        // Next.js App Router convention: page.tsx, page.jsx
        if (fileName == "page.tsx" || fileName == "page.jsx")
            return true;
        
        // Pages directory convention
        if (directoryName == "pages" || filePath.Contains("/pages/") || filePath.Contains("\\pages\\"))
            return true;
        
        // Route/App directory convention
        if (directoryName == "app" || filePath.Contains("/app/") || filePath.Contains("\\app\\"))
            return true;
        
        // Named page files
        if (fileName.Contains("page") && (fileName.EndsWith(".tsx") || fileName.EndsWith(".jsx")))
            return true;
        
        return false;
    }

    /// <summary>
    /// Infers the route from a file path.
    /// </summary>
    public string? InferRoute(string filePath)
    {
        // Normalize path separators
        var normalizedPath = filePath.Replace('\\', '/');
        
        // For Next.js App Router: app/page.tsx -> "/"
        if (normalizedPath.Contains("/app/"))
        {
            var appIndex = normalizedPath.LastIndexOf("/app/");
            var routePart = normalizedPath.Substring(appIndex + 5); // Skip "/app/"
            
            // Remove page.tsx or page.jsx
            routePart = routePart.Replace("/page.tsx", "").Replace("/page.jsx", "");
            
            // Root page
            if (string.IsNullOrEmpty(routePart))
                return "/";
            
            // Dynamic routes: [param] -> :param
            routePart = Regex.Replace(routePart, @"\[(\w+)\]", ":$1");
            
            return "/" + routePart;
        }
        
        // For pages directory
        if (normalizedPath.Contains("/pages/"))
        {
            var pagesIndex = normalizedPath.LastIndexOf("/pages/");
            var routePart = normalizedPath.Substring(pagesIndex + 7); // Skip "/pages/"
            
            // Remove file extension
            routePart = Regex.Replace(routePart, @"\.(tsx|jsx)$", "");
            
            // index -> /
            if (routePart == "index")
                return "/";
            
            // Dynamic routes: [param] -> :param
            routePart = Regex.Replace(routePart, @"\[(\w+)\]", ":$1");
            
            return "/" + routePart;
        }
        
        return null;
    }

    /// <summary>
    /// Analyzes a file to extract components and determine if it's a page.
    /// </summary>
    public async Task<FileAnalysisResult> AnalyzeFileAsync(string filePath)
    {
        var result = new FileAnalysisResult
        {
            FilePath = filePath
        };

        try
        {
            if (!File.Exists(filePath))
            {
                result.HasErrors = true;
                result.ErrorMessage = "File does not exist";
                return result;
            }

            var content = await File.ReadAllTextAsync(filePath);
            
            // Check if it's a page
            result.IsPage = IsPageFile(filePath);
            if (result.IsPage)
            {
                result.Route = InferRoute(filePath);
            }

            // Extract exported components
            var exportedComponents = new HashSet<string>();

            // Default exports
            var defaultMatch = DefaultExportFunctionRegex().Match(content);
            if (defaultMatch.Success)
            {
                exportedComponents.Add(defaultMatch.Groups[1].Value);
            }

            // Named function exports
            foreach (Match match in NamedExportFunctionRegex().Matches(content))
            {
                exportedComponents.Add(match.Groups[1].Value);
            }

            // Named const/let exports
            foreach (Match match in ExportConstRegex().Matches(content))
            {
                exportedComponents.Add(match.Groups[1].Value);
            }

            result.ExportedComponents = exportedComponents.ToList();
            result.HasComponents = exportedComponents.Count > 0;
        }
        catch (Exception ex)
        {
            result.HasErrors = true;
            result.ErrorMessage = ex.Message;
        }

        return result;
    }
}
