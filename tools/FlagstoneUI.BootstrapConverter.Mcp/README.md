# Bootstrap Converter MCP Server

Model Context Protocol (MCP) server for the Bootstrap to Flagstone UI theme converter. This server exposes the converter functionality via the MCP protocol, allowing AI agents and MCP clients to convert Bootstrap themes.

## What is MCP?

The [Model Context Protocol (MCP)](https://modelcontextprotocol.io) is an open protocol that standardizes how applications provide context to LLMs. This MCP server allows AI assistants like Claude Desktop, Copilot, or other MCP clients to convert Bootstrap themes to Flagstone UI without needing to install or run the CLI directly.

## Installation

### From Source

```bash
cd tools/FlagstoneUI.BootstrapConverter.Mcp
dotnet build
```

### Configuration

Add the server to your MCP client configuration. For example, in Claude Desktop (`~/Library/Application Support/Claude/claude_desktop_config.json` on macOS):

```json
{
  "mcpServers": {
    "bootstrap-converter": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "/path/to/flagstone-ui/tools/FlagstoneUI.BootstrapConverter.Mcp/FlagstoneUI.BootstrapConverter.Mcp.csproj"
      ]
    }
  }
}
```

Or using the built executable:

```json
{
  "mcpServers": {
    "bootstrap-converter": {
      "command": "/path/to/flagstone-ui/tools/FlagstoneUI.BootstrapConverter.Mcp/bin/Release/net10.0/bootstrap-converter-mcp"
    }
  }
}
```

## Available Tools

### `convert`

Convert Bootstrap CSS/SCSS theme to Flagstone UI XAML tokens.

**Input Schema:**

```json
{
  "inputs": ["path/to/bootstrap.css"],  // Required: array of file paths or URLs
  "format": "auto",                      // Optional: "css", "scss", or "auto" (default)
  "darkMode": "auto",                    // Optional: "auto", "manual", or "none" (default: auto)
  "namespace": "FlagstoneUI.Resources",  // Optional: XAML namespace (default)
  "includeComments": true                // Optional: include comments (default: true)
}
```

**Output:**

```json
{
  "success": true,
  "tokensXaml": "<?xml version=\"1.0\"...",
  "themeXaml": "<?xml version=\"1.0\"...",
  "summary": {
    "colorTokens": 9,
    "typographyTokens": 1,
    "spacingTokens": 0,
    "borderRadiusTokens": 3,
    "borderWidthTokens": 0
  }
}
```

**Example Usage (via MCP client):**

```
Convert the Bootstrap theme from https://example.com/bootstrap.css to Flagstone UI tokens
```

The AI assistant will call the `convert` tool with appropriate parameters and return the generated XAML.

### `info`

Get information about Bootstrap theme variables without converting.

**Input Schema:**

```json
{
  "input": "path/to/bootstrap.css",  // Required: file path or URL
  "format": "auto"                    // Optional: "css", "scss", or "auto" (default)
}
```

**Output:**

```json
{
  "success": true,
  "input": "path/to/bootstrap.css",
  "format": "Scss",
  "variables": {
    "colors": 52,
    "typography": 2,
    "spacing": 2,
    "borders": 4,
    "other": 2,
    "total": 62
  }
}
```

**Example Usage (via MCP client):**

```
Show me information about the Bootstrap theme at https://example.com/bootstrap.css
```

## Features

- **Multi-file support**: Convert multiple Bootstrap source files (e.g., `_variables.scss` + `_bootswatch.scss`)
- **Variable resolution**: Automatically resolves SCSS variable references (e.g., `$success: $green`)
- **URL support**: Fetch themes directly from URLs
- **Dark mode**: Auto-generate dark color variants
- **Comprehensive token extraction**: Colors, typography, spacing, borders

## Protocol Implementation

This server implements the [Model Context Protocol](https://spec.modelcontextprotocol.io/specification/2024-11-05/) version 2024-11-05.

**Supported Capabilities:**
- `tools` - Exposes convert and info tools

**JSON-RPC Methods:**
- `initialize` - Initialize the server
- `tools/list` - List available tools
- `tools/call` - Call a tool with arguments

## Development

### Running the Server

```bash
dotnet run --project tools/FlagstoneUI.BootstrapConverter.Mcp
```

The server listens on stdin and writes to stdout using JSON-RPC over stdio.

### Testing

You can test the server using the MCP Inspector or by sending JSON-RPC messages to stdin:

```bash
# Initialize
echo '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{}}' | dotnet run

# List tools
echo '{"jsonrpc":"2.0","id":2,"method":"tools/list","params":{}}' | dotnet run

# Call convert tool
echo '{"jsonrpc":"2.0","id":3,"method":"tools/call","params":{"name":"convert","arguments":{"inputs":["bootstrap.css"]}}}' | dotnet run
```

## Related Projects

- **FlagstoneUI.BootstrapConverter** - Core conversion library
- **FlagstoneUI.BootstrapConverter.Cli** - Command-line interface
- **Flagstone UI** - Cross-platform UI framework for .NET MAUI

## License

MIT License - See LICENSE file in repository root.

## Resources

- [Model Context Protocol](https://modelcontextprotocol.io)
- [MCP Specification](https://spec.modelcontextprotocol.io)
- [Flagstone UI Documentation](../../README.md)
