using System.Text.Json;
using System.Text.Json.Nodes;
using FlagstoneUI.BootstrapConverter;
using FlagstoneUI.BootstrapConverter.Models;

namespace FlagstoneUI.BootstrapConverter.Mcp;

/// <summary>
/// MCP server for Bootstrap to Flagstone UI theme conversion
/// Implements the Model Context Protocol via JSON-RPC over stdio
/// </summary>
public class BootstrapConverterServer
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public async Task RunAsync()
    {
        using var reader = new StreamReader(Console.OpenStandardInput());
        using var writer = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line))
                continue;

            try
            {
                var request = JsonSerializer.Deserialize<JsonRpcRequest>(line, _jsonOptions);
                if (request == null)
                    continue;

                var response = await HandleRequestAsync(request);
                var responseJson = JsonSerializer.Serialize(response, _jsonOptions);
                await writer.WriteLineAsync(responseJson);
            }
            catch (Exception ex)
            {
                var errorResponse = new JsonRpcResponse
                {
                    Jsonrpc = "2.0",
                    Id = null,
                    Error = new JsonRpcError
                    {
                        Code = -32603,
                        Message = ex.Message
                    }
                };
                var errorJson = JsonSerializer.Serialize(errorResponse, _jsonOptions);
                await writer.WriteLineAsync(errorJson);
            }
        }
    }

    private async Task<JsonRpcResponse> HandleRequestAsync(JsonRpcRequest request)
    {
        return request.Method switch
        {
            "initialize" => HandleInitialize(request),
            "tools/list" => HandleToolsList(request),
            "tools/call" => await HandleToolsCallAsync(request),
            _ => new JsonRpcResponse
            {
                Jsonrpc = "2.0",
                Id = request.Id,
                Error = new JsonRpcError
                {
                    Code = -32601,
                    Message = $"Method not found: {request.Method}"
                }
            }
        };
    }

    private JsonRpcResponse HandleInitialize(JsonRpcRequest request)
    {
        return new JsonRpcResponse
        {
            Jsonrpc = "2.0",
            Id = request.Id,
            Result = JsonSerializer.SerializeToNode(new
            {
                protocolVersion = "2024-11-05",
                capabilities = new
                {
                    tools = new { }
                },
                serverInfo = new
                {
                    name = "bootstrap-converter",
                    version = "1.0.0"
                }
            }, _jsonOptions)
        };
    }

    private JsonRpcResponse HandleToolsList(JsonRpcRequest request)
    {
        return new JsonRpcResponse
        {
            Jsonrpc = "2.0",
            Id = request.Id,
            Result = JsonSerializer.SerializeToNode(new
            {
                tools = new[]
                {
                    new
                    {
                        name = "convert",
                        description = "Convert Bootstrap CSS/SCSS theme to Flagstone UI XAML tokens",
                        inputSchema = ConvertToolInput.Schema
                    },
                    new
                    {
                        name = "info",
                        description = "Get information about Bootstrap theme variables without converting",
                        inputSchema = InfoToolInput.Schema
                    }
                }
            }, _jsonOptions)
        };
    }

    private async Task<JsonRpcResponse> HandleToolsCallAsync(JsonRpcRequest request)
    {
        try
        {
            var callParams = JsonSerializer.Deserialize<ToolCallParams>(
                request.Params?.ToString() ?? "{}", _jsonOptions);

            if (callParams == null)
            {
                return CreateErrorResponse(request.Id, -32602, "Invalid params");
            }

            object? result = callParams.Name switch
            {
                "convert" => await ConvertAsync(callParams.Arguments),
                "info" => await GetInfoAsync(callParams.Arguments),
                _ => null
            };

            if (result == null)
            {
                return CreateErrorResponse(request.Id, -32601, $"Tool not found: {callParams.Name}");
            }

            return new JsonRpcResponse
            {
                Jsonrpc = "2.0",
                Id = request.Id,
                Result = JsonSerializer.SerializeToNode(new
                {
                    content = new[]
                    {
                        new
                        {
                            type = "text",
                            text = JsonSerializer.Serialize(result, _jsonOptions)
                        }
                    }
                }, _jsonOptions)
            };
        }
        catch (Exception ex)
        {
            return CreateErrorResponse(request.Id, -32603, ex.Message);
        }
    }

    private async Task<ConvertToolOutput> ConvertAsync(JsonNode? arguments)
    {
        try
        {
            var input = JsonSerializer.Deserialize<ConvertToolInput>(
                arguments?.ToString() ?? "{}", _jsonOptions);

            if (input == null || input.Inputs.Length == 0)
            {
                return new ConvertToolOutput
                {
                    Success = false,
                    Error = "No input files specified"
                };
            }

            var parser = new BootstrapParser();
            BootstrapVariables variables;

            // Parse input(s)
            if (input.Inputs.Length == 1)
            {
                var singleInput = input.Inputs[0];
                if (Uri.TryCreate(singleInput, UriKind.Absolute, out var uri))
                {
                    variables = await parser.ParseFromUrlAsync(uri.ToString(), input.Format);
                }
                else if (File.Exists(singleInput))
                {
                    variables = await parser.ParseFromFileAsync(singleInput, input.Format);
                }
                else
                {
                    return new ConvertToolOutput
                    {
                        Success = false,
                        Error = $"Input file not found: {singleInput}"
                    };
                }
            }
            else
            {
                variables = await parser.ParseMultipleFilesAsync(input.Inputs, input.Format);
            }

            // Map to Flagstone tokens
            var mapper = new BootstrapMapper();
            var options = new ConversionOptions
            {
                DarkModeStrategy = input.DarkMode,
                IncludeComments = input.IncludeComments,
                Namespace = input.Namespace
            };

            var tokens = mapper.MapToFlagstoneTokens(variables, options);

            // Generate XAML
            var generator = new XamlThemeGenerator();
            var tokensXaml = generator.GenerateTokensXaml(tokens, options);
            
            // Extract theme name from first input or use default
            var themeName = "Bootstrap";
            if (input.Inputs.Length > 0)
            {
                var firstInput = input.Inputs[0];
                if (!Uri.TryCreate(firstInput, UriKind.Absolute, out _))
                {
                    themeName = Path.GetFileNameWithoutExtension(firstInput);
                    if (string.IsNullOrWhiteSpace(themeName))
                    {
                        themeName = "Bootstrap";
                    }
                }
            }
            
            var themeXaml = generator.GenerateThemeXaml(tokens, themeName, options);

            return new ConvertToolOutput
            {
                Success = true,
                TokensXaml = tokensXaml,
                ThemeXaml = themeXaml,
                Summary = new ConversionSummary
                {
                    ColorTokens = tokens.Colors.Count,
                    TypographyTokens = tokens.Typography.Count,
                    SpacingTokens = tokens.Spacing.Count,
                    BorderRadiusTokens = tokens.BorderRadius.Count,
                    BorderWidthTokens = tokens.BorderWidth.Count
                }
            };
        }
        catch (Exception ex)
        {
            return new ConvertToolOutput
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

    private async Task<InfoToolOutput> GetInfoAsync(JsonNode? arguments)
    {
        try
        {
            var input = JsonSerializer.Deserialize<InfoToolInput>(
                arguments?.ToString() ?? "{}", _jsonOptions);

            if (input == null || string.IsNullOrEmpty(input.Input))
            {
                return new InfoToolOutput
                {
                    Success = false,
                    Error = "No input file specified"
                };
            }

            var parser = new BootstrapParser();
            BootstrapVariables variables;

            if (Uri.TryCreate(input.Input, UriKind.Absolute, out var uri))
            {
                variables = await parser.ParseFromUrlAsync(uri.ToString(), input.Format);
            }
            else if (File.Exists(input.Input))
            {
                variables = await parser.ParseFromFileAsync(input.Input, input.Format);
            }
            else
            {
                return new InfoToolOutput
                {
                    Success = false,
                    Error = $"Input file not found: {input.Input}"
                };
            }

            return new InfoToolOutput
            {
                Success = true,
                Input = input.Input,
                Format = input.Format.ToString(),
                Variables = new VariablesSummary
                {
                    Colors = variables.Colors.Count,
                    Typography = variables.Typography.Count,
                    Spacing = variables.Spacing.Count,
                    Borders = variables.Borders.Count,
                    Other = variables.Other.Count,
                    Total = variables.Colors.Count + variables.Typography.Count +
                            variables.Spacing.Count + variables.Borders.Count + variables.Other.Count
                }
            };
        }
        catch (Exception ex)
        {
            return new InfoToolOutput
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

    private JsonRpcResponse CreateErrorResponse(JsonNode? id, int code, string message)
    {
        return new JsonRpcResponse
        {
            Jsonrpc = "2.0",
            Id = id,
            Error = new JsonRpcError
            {
                Code = code,
                Message = message
            }
        };
    }
}

// JSON-RPC protocol models
public record JsonRpcRequest
{
    public required string Jsonrpc { get; init; }
    public JsonNode? Id { get; init; }
    public required string Method { get; init; }
    public JsonNode? Params { get; init; }
}

public record JsonRpcResponse
{
    public required string Jsonrpc { get; init; }
    public JsonNode? Id { get; init; }
    public JsonNode? Result { get; init; }
    public JsonRpcError? Error { get; init; }
}

public record JsonRpcError
{
    public required int Code { get; init; }
    public required string Message { get; init; }
}

public record ToolCallParams
{
    public required string Name { get; init; }
    public JsonNode? Arguments { get; init; }
}
