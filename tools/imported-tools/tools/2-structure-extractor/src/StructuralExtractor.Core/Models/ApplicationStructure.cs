namespace StructuralExtractor.Core.Models;

/// <summary>
/// Represents the complete application structure contract.
/// This is the root output of the structural extraction process.
/// </summary>
public class ApplicationStructure
{
    /// <summary>
    /// Named visual components (primitives) that can be referenced and reused.
    /// </summary>
    public Dictionary<string, ComponentDefinition> Components { get; set; } = new();

    /// <summary>
    /// Named pages/screens in the application.
    /// </summary>
    public Dictionary<string, PageDefinition> Pages { get; set; } = new();

    /// <summary>
    /// Navigation structure and entry points.
    /// </summary>
    public NavigationStructure? Navigation { get; set; }
}

/// <summary>
/// Defines a reusable visual component.
/// </summary>
public class ComponentDefinition
{
    /// <summary>
    /// Component type/role (e.g., container, control, layout).
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Named slots that accept other components.
    /// </summary>
    public Dictionary<string, string>? Slots { get; set; }

    /// <summary>
    /// Properties that the component accepts.
    /// </summary>
    public Dictionary<string, string>? Props { get; set; }

    /// <summary>
    /// Direct children structure if component has explicit children.
    /// </summary>
    public List<StructuralElement>? Children { get; set; }

    /// <summary>
    /// Source file information for traceability.
    /// </summary>
    public string? SourceFile { get; set; }
}

/// <summary>
/// Defines a page/screen in the application.
/// </summary>
public class PageDefinition
{
    /// <summary>
    /// Route associated with this page (e.g., "/", "/about").
    /// </summary>
    public string? Route { get; set; }

    /// <summary>
    /// Root layout/structure of the page.
    /// </summary>
    public StructuralElement? Layout { get; set; }

    /// <summary>
    /// Source file information for traceability.
    /// </summary>
    public string? SourceFile { get; set; }
}

/// <summary>
/// Represents a structural element in the visual hierarchy.
/// </summary>
public class StructuralElement
{
    /// <summary>
    /// Element type (e.g., "div", "section", component reference).
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Reference to a component (e.g., "#/components/Card").
    /// </summary>
    public string? Ref { get; set; }

    /// <summary>
    /// Properties/attributes on this element.
    /// </summary>
    public Dictionary<string, object>? Props { get; set; }

    /// <summary>
    /// Child elements.
    /// </summary>
    public List<StructuralElement>? Children { get; set; }

    /// <summary>
    /// Text content if this is a text node.
    /// </summary>
    public string? Text { get; set; }
}

/// <summary>
/// Navigation structure and entry points.
/// </summary>
public class NavigationStructure
{
    /// <summary>
    /// Initial/default page.
    /// </summary>
    public string? Initial { get; set; }

    /// <summary>
    /// Links between pages.
    /// </summary>
    public List<NavigationLink>? Links { get; set; }
}

/// <summary>
/// Represents a navigation link between pages.
/// </summary>
public class NavigationLink
{
    /// <summary>
    /// Source page.
    /// </summary>
    public string? From { get; set; }

    /// <summary>
    /// Target page or route.
    /// </summary>
    public string? To { get; set; }

    /// <summary>
    /// Link label if available.
    /// </summary>
    public string? Label { get; set; }
}
