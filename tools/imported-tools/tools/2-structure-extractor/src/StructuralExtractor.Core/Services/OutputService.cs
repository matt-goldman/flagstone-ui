using System.Text.Json;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using StructuralExtractor.Core.Models;

namespace StructuralExtractor.Core.Services;

/// <summary>
/// Handles serialization of application structure to YAML or JSON.
/// </summary>
public class OutputService
{
    /// <summary>
    /// Writes the application structure to a file in the specified format.
    /// </summary>
    public async Task WriteStructureAsync(ApplicationStructure structure, string outputPath, OutputFormat format = OutputFormat.Yaml)
    {
        string content = format == OutputFormat.Yaml 
            ? SerializeToYaml(structure) 
            : SerializeToJson(structure);

        await File.WriteAllTextAsync(outputPath, content);
    }

    /// <summary>
    /// Serializes the application structure to YAML format.
    /// </summary>
    public string SerializeToYaml(ApplicationStructure structure)
    {
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
            .Build();

        return serializer.Serialize(structure);
    }

    /// <summary>
    /// Serializes the application structure to JSON format.
    /// </summary>
    public string SerializeToJson(ApplicationStructure structure)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        return JsonSerializer.Serialize(structure, options);
    }
}

/// <summary>
/// Output format options.
/// </summary>
public enum OutputFormat
{
    Yaml,
    Json
}
