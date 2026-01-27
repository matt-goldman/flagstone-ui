using ReactComponentFlattener.Core.Models;

namespace ReactComponentFlattener.Core.Services;

public class ComponentGraph
{
    public Dictionary<string, ComponentNode> Nodes { get; } = [];
    public string FilePath { get; set; } = string.Empty;
}

public class ComponentNode
{
    public ComponentInfo Info { get; set; } = null!;
    public List<string> UsageLocations { get; set; } = [];
    public int UsageCount { get; set; }
    public bool ShouldFlatten { get; set; }
    public string FlatteningReason { get; set; } = string.Empty;
}

public class ComponentGraphBuilder
{
    public static ComponentGraph BuildGraph(FileAnalysis analysis, string filePath)
    {
        var graph = new ComponentGraph { FilePath = filePath };

        // Add all components to the graph
        foreach (var component in analysis.Components)
        {
            graph.Nodes[component.Name] = new ComponentNode
            {
                Info            = component,
                UsageCount      = 0,
                UsageLocations  = []
            };
        }

        // Count usages within the same file
        foreach (var component in analysis.Components)
        {
            foreach (var usedComponent in component.UsedComponents)
            {
                if (graph.Nodes.ContainsKey(usedComponent))
                {
                    graph.Nodes[usedComponent].UsageCount++;
                    graph.Nodes[usedComponent].UsageLocations.Add(component.Name);
                }
            }
        }

        return graph;
    }

    public void DetermineFlattening(ComponentGraph graph)
    {
        foreach (var (name, node) in graph.Nodes)
        {
            // Determine if component should be flattened based on spec rules
            
            // Preserve if exported (likely used elsewhere)
            if (node.Info.IsExported)
            {
                node.ShouldFlatten = false;
                node.FlatteningReason = "exported component, likely reusable";
                continue;
            }

            // Preserve if uses hooks
            if (node.Info.UsesHooks)
            {
                node.ShouldFlatten = false;
                node.FlatteningReason = "contains hooks or state logic";
                continue;
            }

            // Preserve if uses context
            if (node.Info.UsesContext)
            {
                node.ShouldFlatten = false;
                node.FlatteningReason = "uses context providers or consumers";
                continue;
            }

            // Preserve if used in multiple locations
            if (node.UsageCount > 1)
            {
                node.ShouldFlatten = false;
                node.FlatteningReason = "used in multiple locations";
                continue;
            }

            // Preserve if not used at all (might be used externally)
            if (node.UsageCount == 0)
            {
                node.ShouldFlatten = false;
                node.FlatteningReason = "unused, potentially external usage";
                continue;
            }

            // Preserve if accepts children (unless it just passes them through)
            // For v1, we'll be conservative and preserve components with children
            if (node.Info.HasChildren)
            {
                node.ShouldFlatten = false;
                node.FlatteningReason = "accepts children props";
                continue;
            }

            // Flatten: single-use, no hooks, no context, presentational
            node.ShouldFlatten = true;
            node.FlatteningReason = "single-use, presentational, no hooks";
        }
    }
}
