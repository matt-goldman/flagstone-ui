using System.Text.Json.Nodes;
using FlagstoneUI.BootstrapConverter.Models;

namespace FlagstoneUI.BootstrapConverter.Mcp;

public record ConvertToolInput
{
    public required string[] Inputs { get; init; }
    public BootstrapFormat Format { get; init; } = BootstrapFormat.Auto;
    public DarkModeStrategy DarkMode { get; init; } = DarkModeStrategy.Auto;
    public string Namespace { get; init; } = "FlagstoneUI.Resources";
    public bool IncludeComments { get; init; } = true;

    public static JsonObject Schema => new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["inputs"] = new JsonObject
            {
                ["type"] = "array",
                ["items"] = new JsonObject { ["type"] = "string" },
                ["description"] = "Path(s) to Bootstrap CSS/SCSS file(s) or URL(s). Multiple files will be merged."
            },
            ["format"] = new JsonObject
            {
                ["type"] = "string",
                ["enum"] = new JsonArray { "css", "scss", "auto" },
                ["default"] = "auto",
                ["description"] = "Input format: css, scss, or auto"
            },
            ["darkMode"] = new JsonObject
            {
                ["type"] = "string",
                ["enum"] = new JsonArray { "auto", "manual", "none" },
                ["default"] = "auto",
                ["description"] = "Dark mode generation strategy"
            },
            ["namespace"] = new JsonObject
            {
                ["type"] = "string",
                ["default"] = "FlagstoneUI.Resources",
                ["description"] = "XAML namespace for generated resources"
            },
            ["includeComments"] = new JsonObject
            {
                ["type"] = "boolean",
                ["default"] = true,
                ["description"] = "Include purpose comments in generated XAML"
            }
        },
        ["required"] = new JsonArray { "inputs" }
    };
}

public record ConvertToolOutput
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public string? TokensXaml { get; init; }
    public string? ThemeXaml { get; init; }
    public ConversionSummary? Summary { get; init; }
}

public record ConversionSummary
{
    public int ColorTokens { get; init; }
    public int TypographyTokens { get; init; }
    public int SpacingTokens { get; init; }
    public int BorderRadiusTokens { get; init; }
    public int BorderWidthTokens { get; init; }
}

public record InfoToolInput
{
    public required string Input { get; init; }
    public BootstrapFormat Format { get; init; } = BootstrapFormat.Auto;

    public static JsonObject Schema => new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["input"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Path to Bootstrap CSS/SCSS file or URL"
            },
            ["format"] = new JsonObject
            {
                ["type"] = "string",
                ["enum"] = new JsonArray { "css", "scss", "auto" },
                ["default"] = "auto",
                ["description"] = "Input format: css, scss, or auto"
            }
        },
        ["required"] = new JsonArray { "input" }
    };
}

public record InfoToolOutput
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public string? Input { get; init; }
    public string? Format { get; init; }
    public VariablesSummary? Variables { get; init; }
}

public record VariablesSummary
{
    public int Colors { get; init; }
    public int Typography { get; init; }
    public int Spacing { get; init; }
    public int Borders { get; init; }
    public int Other { get; init; }
    public int Total { get; init; }
}

public record DocsToolInput
{
    public required string Topic { get; init; }

    public static JsonObject Schema => new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["topic"] = new JsonObject
            {
                ["type"] = "string",
                ["enum"] = new JsonArray { "tokens", "controls", "theming", "architecture", "best-practices", "all" },
                ["description"] = "Documentation topic to retrieve: tokens (token system reference), controls (available controls and their properties), theming (how to create themes), architecture (overall system architecture), best-practices (guidelines for theme and control development), or all (complete documentation)"
            }
        },
        ["required"] = new JsonArray { "topic" }
    };
}

public record DocsToolOutput
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public string? Topic { get; init; }
    public string? Content { get; init; }
    public string[]? AvailableTopics { get; init; }
}

public record AnalyzeToolInput  
{
    public required string[] Inputs { get; init; }
    public BootstrapFormat Format { get; init; } = BootstrapFormat.Auto;

    public static JsonObject Schema => new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["inputs"] = new JsonObject
            {
                ["type"] = "array",
                ["items"] = new JsonObject { ["type"] = "string" },
                ["description"] = "Path(s) to Bootstrap CSS/SCSS file(s) or URL(s). Multiple files will be merged."
            },
            ["format"] = new JsonObject
            {
                ["type"] = "string",
                ["enum"] = new JsonArray { "css", "scss", "auto" },
                ["default"] = "auto",
                ["description"] = "Input format: css, scss, or auto"
            }
        },
        ["required"] = new JsonArray { "inputs" }
    };
}

public record AnalyzeToolOutput
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public BootstrapData? Variables { get; init; }
}

public record BootstrapData
{
    public Dictionary<string, string>? Colors { get; init; }
    public Dictionary<string, string>? Typography { get; init; }
    public Dictionary<string, string>? Spacing { get; init; }
    public Dictionary<string, string>? Borders { get; init; }
    public Dictionary<string, string>? Other { get; init; }
}
