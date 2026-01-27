using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using StructuralExtractor.Core.Models;

namespace StructuralExtractor.Core.Services;

/// <summary>
/// Parses normalized HTML output from Tool 1 to extract application structure.
/// Uses data attributes to identify component boundaries and metadata.
/// </summary>
public class HtmlStructureParser
{
    private readonly IHtmlParser _parser;

    public HtmlStructureParser()
    {
        var config = Configuration.Default;
        var context = BrowsingContext.New(config);
        _parser = context.GetService<IHtmlParser>()!;
    }

    /// <summary>
    /// Determines if the content is HTML (vs JSX/TSX).
    /// </summary>
    public bool IsHtmlContent(string content)
    {
        var trimmed = content.TrimStart();
        return trimmed.StartsWith("<!DOCTYPE", StringComparison.OrdinalIgnoreCase) ||
               trimmed.StartsWith("<html", StringComparison.OrdinalIgnoreCase) ||
               (trimmed.StartsWith("<") && !trimmed.Contains("import ") && !trimmed.Contains("export "));
    }

    /// <summary>
    /// Extracts page/component metadata from HTML head section.
    /// </summary>
    public HtmlFileMetadata ExtractMetadata(string htmlContent)
    {
        var metadata = new HtmlFileMetadata();

        try
        {
            var document = _parser.ParseDocument(htmlContent);

            // Extract source file from meta tag
            var sourceFileMeta = document.QuerySelector("meta[name='source-file']");
            if (sourceFileMeta != null)
            {
                metadata.SourceFile = sourceFileMeta.GetAttribute("content");
            }

            // Extract component name from meta tag
            var componentMeta = document.QuerySelector("meta[name='component']");
            if (componentMeta != null)
            {
                metadata.ComponentName = componentMeta.GetAttribute("content");
            }

            // Check for data-source on root body element
            var bodyChild = document.Body?.FirstElementChild;
            if (bodyChild != null)
            {
                var dataSource = bodyChild.GetAttribute("data-source");
                if (!string.IsNullOrEmpty(dataSource))
                {
                    metadata.DataSource = dataSource;
                }
            }
        }
        catch
        {
            // Metadata extraction failed, continue with defaults
        }

        return metadata;
    }

    /// <summary>
    /// Parses HTML content and extracts structural elements.
    /// </summary>
    public StructuralElement? ParseHtmlStructure(string htmlContent)
    {
        try
        {
            var document = _parser.ParseDocument(htmlContent);
            var body = document.Body;

            if (body == null || !body.HasChildNodes)
                return null;

            // Find the root element (first direct child of body that's not whitespace)
            var rootElement = body.Children.FirstOrDefault();
            if (rootElement == null)
                return null;

            return ParseElement(rootElement);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Recursively parses an HTML element into a structural element.
    /// </summary>
    private StructuralElement ParseElement(IElement element)
    {
        var structural = new StructuralElement();

        // Check for data-component attribute (represents a React component)
        var dataComponent = element.GetAttribute("data-component");
        if (!string.IsNullOrEmpty(dataComponent))
        {
            structural.Type = "component";
            structural.Ref = $"#/components/{dataComponent}";

            // Extract component props from other attributes
            var props = ExtractProps(element, excludeDataComponent: true);
            if (props.Count > 0)
            {
                structural.Props = props;
            }
        }
        else
        {
            // Regular HTML element
            structural.Type = element.LocalName;

            // Extract relevant props (class, id, key attributes)
            var props = ExtractProps(element, excludeDataComponent: false);
            if (props.Count > 0)
            {
                structural.Props = props;
            }
        }

        // Parse children (skip text-only nodes and dynamic expression placeholders)
        var children = new List<StructuralElement>();
        foreach (var child in element.Children)
        {
            // Skip dynamic expression placeholders
            if (child.GetAttribute("data-expression") == "dynamic")
                continue;

            var childStructure = ParseElement(child);
            children.Add(childStructure);
        }

        if (children.Count > 0)
        {
            structural.Children = children;
        }

        return structural;
    }

    /// <summary>
    /// Extracts relevant props from an element's attributes.
    /// </summary>
    private Dictionary<string, object> ExtractProps(IElement element, bool excludeDataComponent)
    {
        var props = new Dictionary<string, object>();

        foreach (var attr in element.Attributes)
        {
            var name = attr.Name;
            var value = attr.Value;

            // Skip data-component if requested
            if (excludeDataComponent && name == "data-component")
                continue;

            // Skip internal data attributes
            if (name == "data-source" || name == "data-expression")
                continue;

            // Include className (from class attribute)
            if (name == "class" && !string.IsNullOrEmpty(value))
            {
                props["className"] = value;
                continue;
            }

            // Include id
            if (name == "id" && !string.IsNullOrEmpty(value))
            {
                props["id"] = value;
                continue;
            }

            // Include href for links
            if (name == "href" && !string.IsNullOrEmpty(value))
            {
                props["href"] = value;
                continue;
            }

            // Include data attributes that represent component props
            if (name.StartsWith("data-") && name != "data-slot")
            {
                // Convert data-prop-name to propName
                var propName = ConvertDataAttributeToPropName(name);
                props[propName] = value;
                continue;
            }

            // Include specific semantic attributes
            if (name is "role" or "aria-label" or "type" or "name" or "placeholder" or 
                "disabled" or "required" or "readonly" or "checked" or "selected" or
                "variant" or "size" or "asChild")
            {
                if (!string.IsNullOrEmpty(value))
                {
                    props[name] = value;
                }
                else
                {
                    // Boolean attribute
                    props[name] = true;
                }
            }
        }

        return props;
    }

    /// <summary>
    /// Converts a data attribute name to a camelCase prop name.
    /// e.g., "data-my-prop" -> "myProp"
    /// </summary>
    private string ConvertDataAttributeToPropName(string dataAttr)
    {
        // Remove "data-" prefix
        var propName = dataAttr.Substring(5);

        // Convert kebab-case to camelCase
        var parts = propName.Split('-');
        if (parts.Length == 1)
            return parts[0];

        return parts[0] + string.Concat(parts.Skip(1).Select(p =>
            p.Length > 0 ? char.ToUpper(p[0]) + p.Substring(1) : ""));
    }

    /// <summary>
    /// Extracts all unique component references from a structural tree.
    /// </summary>
    public HashSet<string> ExtractComponentReferences(StructuralElement? root)
    {
        var components = new HashSet<string>();
        ExtractComponentReferencesRecursive(root, components);
        return components;
    }

    private void ExtractComponentReferencesRecursive(StructuralElement? element, HashSet<string> components)
    {
        if (element == null)
            return;

        if (!string.IsNullOrEmpty(element.Ref) && element.Ref.StartsWith("#/components/"))
        {
            var componentName = element.Ref.Substring("#/components/".Length);
            components.Add(componentName);
        }

        if (element.Children != null)
        {
            foreach (var child in element.Children)
            {
                ExtractComponentReferencesRecursive(child, components);
            }
        }
    }
}

/// <summary>
/// Metadata extracted from HTML file headers.
/// </summary>
public class HtmlFileMetadata
{
    public string? SourceFile { get; set; }
    public string? ComponentName { get; set; }
    public string? DataSource { get; set; }

    /// <summary>
    /// Determines if this file represents a page based on source file path.
    /// </summary>
    public bool IsPage
    {
        get
        {
            if (string.IsNullOrEmpty(SourceFile) && string.IsNullOrEmpty(DataSource))
                return false;

            var source = (SourceFile ?? DataSource ?? "").ToLowerInvariant();
            return source.Contains("/app/") || source.Contains("\\app\\") ||
                   source.Contains("/pages/") || source.Contains("\\pages\\") ||
                   source.Contains("page.tsx") || source.Contains("page.jsx");
        }
    }

    /// <summary>
    /// Infers the route from the source file path.
    /// </summary>
    public string? InferRoute()
    {
        var source = SourceFile ?? "";
        var normalizedPath = source.Replace('\\', '/');

        // For Next.js App Router: app/page.tsx -> "/"
        if (normalizedPath.Contains("/app/"))
        {
            var appIndex = normalizedPath.LastIndexOf("/app/");
            var routePart = normalizedPath.Substring(appIndex + 5);

            // Remove page.tsx or page.jsx
            routePart = routePart.Replace("/page.tsx", "").Replace("/page.jsx", "");

            if (string.IsNullOrEmpty(routePart))
                return "/";

            // Dynamic routes: [param] -> :param
            routePart = System.Text.RegularExpressions.Regex.Replace(routePart, @"\[(\w+)\]", ":$1");

            return "/" + routePart;
        }

        // For pages directory
        if (normalizedPath.Contains("/pages/"))
        {
            var pagesIndex = normalizedPath.LastIndexOf("/pages/");
            var routePart = normalizedPath.Substring(pagesIndex + 7);

            // Remove file extension
            routePart = System.Text.RegularExpressions.Regex.Replace(routePart, @"\.(tsx|jsx)$", "");

            if (routePart == "index")
                return "/";

            routePart = System.Text.RegularExpressions.Regex.Replace(routePart, @"\[(\w+)\]", ":$1");

            return "/" + routePart;
        }

        return null;
    }
}
