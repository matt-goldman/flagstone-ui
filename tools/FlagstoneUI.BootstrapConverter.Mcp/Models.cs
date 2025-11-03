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
