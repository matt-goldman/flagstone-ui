namespace ReactComponentFlattener.Core.Models;

public class FlatteningReport
{
    public List<FlattenedComponent> Flattened { get; set; } = new();
    public List<PreservedComponent> Preserved { get; set; } = new();
}

public class FlattenedComponent
{
    public string Component { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string OriginalFile { get; set; } = string.Empty;
    public string? NewLocation { get; set; }
    public LineRange? LineRange { get; set; }
}

public class PreservedComponent
{
    public string Component { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string File { get; set; } = string.Empty;
}

public class LineRange
{
    public int Start { get; set; }
    public int End { get; set; }
}
