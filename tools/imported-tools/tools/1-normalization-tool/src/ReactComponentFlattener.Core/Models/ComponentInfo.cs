namespace ReactComponentFlattener.Core.Models;

public class ComponentInfo
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public List<string> Params { get; set; } = new();
    public bool UsesHooks { get; set; }
    public bool UsesContext { get; set; }
    public List<string> UsedComponents { get; set; } = new();
    public bool HasChildren { get; set; }
    public bool IsExported { get; set; }
    public bool IsDefaultExport { get; set; }
    public Location? Loc { get; set; }
}

public class Location
{
    public Position? Start { get; set; }
    public Position? End { get; set; }
}

public class Position
{
    public int Line { get; set; }
    public int Column { get; set; }
}

public class FileAnalysis
{
    public List<ComponentInfo> Components { get; set; } = new();
    public List<ImportInfo> Imports { get; set; } = new();
}

public class ImportInfo
{
    public string Source { get; set; } = string.Empty;
    public List<ImportSpecifier> Specifiers { get; set; } = new();
}

public class ImportSpecifier
{
    public string Type { get; set; } = string.Empty;
    public string? Imported { get; set; }
    public string Local { get; set; } = string.Empty;
}
