# Flagstone UI MCP Server (Planned)

> **📋 PLANNED - Post-Integration Update**  
> This document outlines the vision for a comprehensive Flagstone UI MCP server that will provide agent-friendly functionality for theme conversion, UI generation, and FlagstoneUI system guidance.
> 
> **Current Status**: Bootstrap Converter core library and UI app complete. Temporary MCP implementation exists but will be replaced with comprehensive FlagstoneUI MCP server.

## Vision & Goals

The Flagstone UI MCP server will enable AI agents to:

1. **Convert themes** from Bootstrap/Tailwind to Flagstone UI
2. **Generate complete UIs** using Flagstone UI controls and patterns
3. **Understand the system** through comprehensive documentation access
4. **Provide examples** showing what Bootstrap themes look like in FlagstoneUI
5. **Assist with theming** by analyzing and generating token catalogs

### Value Proposition

- **Vibe-code FlagstoneUI interfaces**: AI agents can generate complete MAUI UIs based on natural language descriptions
- **Theme intelligence**: Convert existing web themes (Bootstrap/Tailwind) to FlagstoneUI automatically
- **System understanding**: Agents have access to architecture, controls, tokens, and best practices
- **Learning by example**: Bootstrap conversions show agents how FlagstoneUI themes work
- **Accelerated development**: Rapid prototyping and theme customization through AI assistance

## Architecture

### Comprehensive MCP Server Structure

```tree
tools/FlagstoneUI.Mcp/
├── FlagstoneUI.Mcp.csproj                         # MCP server project
├── Program.cs                                     # MCP server entry point
├── FlagstoneMcpServer.cs                         # Main MCP server implementation
├── Tools/
│   ├── BootstrapConverterTool.cs                 # Bootstrap → FlagstoneUI conversion
│   ├── TailwindConverterTool.cs                  # Tailwind → FlagstoneUI conversion (future)
│   ├── TokenAnalyzerTool.cs                      # Analyze and validate tokens
│   ├── ThemeGeneratorTool.cs                     # Generate themes from descriptions
│   ├── UIGeneratorTool.cs                        # Generate UI pages/controls
│   ├── DocumentationTool.cs                      # Access Flagstone docs
│   └── ExampleProviderTool.cs                    # Show converted theme examples
├── Models/
│   ├── ToolInputs.cs                             # Input schemas for all tools
│   ├── ToolOutputs.cs                            # Output schemas for all tools
│   └── ThemeMetadata.cs                          # Theme information models
└── Services/
    ├── IBootstrapService.cs                      # Bootstrap conversion service
    ├── IThemeService.cs                          # Theme management service
    ├── IDocumentationService.cs                  # Documentation access service
    └── IUIGenerationService.cs                   # UI generation service
```

### Integration with Existing Components

```
┌─────────────────────────────────────────────────┐
│         Flagstone UI MCP Server                 │
│                                                 │
│  ┌───────────────────────────────────────────┐ │
│  │  Tools                                    │ │
│  │  ├─ Bootstrap Converter Tool              │ │
│  │  ├─ Theme Generator Tool                  │ │
│  │  ├─ UI Generator Tool                     │ │
│  │  ├─ Documentation Tool                    │ │
│  │  └─ Example Provider Tool                 │ │
│  └───────────────────────────────────────────┘ │
│                    ↓                            │
│  ┌───────────────────────────────────────────┐ │
│  │  Services (Reuse Existing Libraries)     │ │
│  │  ├─ BootstrapConverterService             │ │
│  │  │   (uses FlagstoneUI.BootstrapConverter)│ │
│  │  ├─ ThemeService                          │ │
│  │  ├─ DocumentationService                  │ │
│  │  └─ UIGenerationService                   │ │
│  └───────────────────────────────────────────┘ │
└─────────────────────────────────────────────────┘
             ↓                      ↓
┌──────────────────────┐  ┌────────────────────┐
│ FlagstoneUI.         │  │ Documentation      │
│ BootstrapConverter   │  │ Files (docs/)      │
│ (existing library)   │  │                    │
└──────────────────────┘  └────────────────────┘
```

## Current Status (December 2025)

### ✅ Completed Components

1. **FlagstoneUI.BootstrapConverter Library** (Core conversion logic)
   - ✅ SCSS/CSS parsing with variable resolution
   - ✅ Token mapping (colors, typography, spacing, borders, **shadows**, **per-edge borders**)
   - ✅ XAML generation (Tokens.xaml, Theme.xaml, Styles.xaml)
   - ✅ C# code-behind generation
   - ✅ Dark mode variant generation with **AppThemeBinding support**
   - ✅ **Light/dark mode CSS custom property extraction** (Bootstrap 5+)
   - ✅ Multi-file support
   - ✅ Font detection and handling
   - ✅ Component style generation (FsButton, FsEntry, FsEditor, FsCard)
   - ✅ **Shadow extraction** from Bootstrap variables and CSS (box-shadow parsing)
   - ✅ **Per-edge border extraction** from multi-value border-width properties

2. **FlagstoneUI.BootstrapConverter.Cli** (Command-line interface)
   - ✅ Convert command with comprehensive options
   - ✅ Info command for theme analysis
   - ✅ Validation and error handling
   - ✅ Multiple output formats (XAML, C#)

3. **FlagstoneUI.BootstrapConverter.UI** (MAUI desktop app)
   - ✅ Visual theme conversion interface
   - ✅ File and URL input modes
   - ✅ Live theme preview with Flagstone controls
   - ✅ Configuration options (analysis modes, dark mode, namespace)
   - ✅ Font detection and download capability
   - ✅ Save/export functionality using CommunityToolkit.Maui.Storage
   - ✅ Real-time conversion results display

4. **Documentation**
   - ✅ Architecture documentation
   - ✅ Token reference (tokens.md)
   - ✅ Control implementation guide
   - ✅ Theming guide for designers
   - ✅ Quickstart guide
   - ✅ ADRs (Architecture Decision Records)

### 🚧 Temporary Implementation

- **FlagstoneUI.BootstrapConverter.Mcp** (Temporary test server)
  - Proof of concept for MCP protocol
  - Basic `convert` and `info` tools
  - Will be **replaced** with comprehensive FlagstoneUI.Mcp server

## Planned MCP Tools

### 1. Bootstrap Converter Tool ✅ (Existing, Will Integrate)

**Purpose**: Convert Bootstrap themes to FlagstoneUI

**Capabilities**:
- Parse SCSS variables and CSS classes
- Generate complete theme files (Tokens, Styles, Theme)
- Support multiple analysis strategies
- Handle fonts (detection, download URLs)
- Generate both XAML and C# output

**Input Schema**:
```json
{
  "inputs": ["array of file paths or URLs"],
  "format": "auto|css|scss",
  "strategy": "variables|css|hybrid",
  "darkMode": "auto|manual|none",
  "namespace": "string",
  "includeComments": "boolean",
  "includeFonts": "boolean",
  "outputFormat": "xaml|csharp"
}
```

**Output**: Complete theme files with statistics

### 2. Theme Analyzer Tool 📋 (Planned)

**Purpose**: Analyze existing FlagstoneUI themes or token catalogs

**Capabilities**:
- Parse tokens-catalog.json
- Validate token completeness
- Identify missing tokens
- Suggest token values based on color theory
- Compare themes

**Example Use Cases**:
- "Analyze this theme and tell me what tokens are missing"
- "Compare the Material and Modern themes"
- "What colors are used in this theme?"

### 3. UI Generator Tool 📋 (Planned)

**Purpose**: Generate complete MAUI pages using FlagstoneUI controls

**Capabilities**:
- Generate XAML pages from natural language descriptions
- Use appropriate FlagstoneUI controls (FsButton, FsCard, FsEntry, etc.)
- Apply proper token bindings
- Follow FlagstoneUI patterns and best practices
- Generate code-behind files

**Example Use Cases**:
- "Create a login page with email and password fields"
- "Generate a settings page with toggle switches"
- "Build a product card grid layout"

**Input Schema**:
```json
{
  "description": "natural language description of UI",
  "pageType": "ContentPage|Shell|etc",
  "theme": "Material|Modern|Custom",
  "includeCodeBehind": "boolean",
  "namespace": "string"
}
```

### 4. Documentation Tool 📋 (Planned)

**Purpose**: Provide comprehensive FlagstoneUI documentation to agents

**Capabilities**:
- Access tokens.md (token reference)
- Access control-implementation-guide.md
- Access architecture.md
- Access theming-guide.md
- Access ADRs (architecture decisions)
- Search documentation by topic

**Topics**:
- `tokens` - Complete token system reference
- `controls` - Available controls and properties
- `theming` - How to create themes
- `architecture` - System architecture
- `best-practices` - Development guidelines
- `patterns` - Common UI patterns
- `all` - Complete documentation

**Example Use Cases**:
- "What tokens are available for button styling?"
- "How do I create a custom theme?"
- "What controls does FlagstoneUI provide?"

### 5. Example Provider Tool 📋 (Planned)

**Purpose**: Show examples of converted themes and generated UIs

**Capabilities**:
- Provide before/after Bootstrap → FlagstoneUI examples
- Show complete theme files from popular Bootstrap themes (Bootswatch)
- Demonstrate control usage patterns
- Show token binding examples

**Example Use Cases**:
- "Show me what the Bootstrap Darkly theme looks like in FlagstoneUI"
- "Give me an example of an FsButton with outlined style"
- "How do I bind to theme tokens?"

### 6. Token Generator Tool 📋 (Future)

**Purpose**: Generate token catalogs from design specifications

**Capabilities**:
- Generate tokens-catalog.json from color palette
- Create semantic token mappings
- Generate dark mode variants
- Export to XAML

**Example Use Cases**:
- "Create a theme with primary color #FF6B6B"
- "Generate a dark mode variant of this theme"

### 7. Tailwind Converter Tool 📋 (Future)

**Purpose**: Convert Tailwind CSS themes to FlagstoneUI

**Capabilities**:
- Parse Tailwind config
- Map utility classes to tokens
- Generate visual states from pseudo-classes
- Handle gradients and shadows

**Example Use Cases**:
- "Convert this Tailwind theme to FlagstoneUI"

## Implementation Phases

### Phase 1: Foundation ✅ (Completed)

- ✅ Bootstrap converter core library
- ✅ Bootstrap converter CLI
- ✅ Bootstrap converter UI app
- ✅ Documentation system
- ✅ Token catalog generation tools

### Phase 2: MCP Integration 📋 (Current Priority)

**Target**: Q1 2025

**Tasks**:
1. Create `FlagstoneUI.Mcp` project
2. Implement MCP server protocol (replace example JSON-RPC over stdio with official MCP .NET SDK)
3. Integrate Bootstrap converter as a tool
4. Implement Documentation tool
5. Implement Example Provider tool
6. Implement Theme Analyzer tool
7. Test with Claude Desktop and other MCP clients
8. Document MCP server usage and configuration

**Deliverables**:
- Working MCP server with 4+ tools
- Configuration examples for popular MCP clients
- Integration tests
- Usage documentation

### Phase 3: Advanced Tools 📋 (Future)

**Target**: Q2 2026

**Tasks**:
1. Implement UI Generator tool
2. Implement Token Generator tool
3. Enhance Documentation tool with search
4. Add Tailwind converter tool
5. Add theme comparison capabilities

**Deliverables**:
- Full suite of 7+ MCP tools
- Comprehensive agent capabilities
- Advanced UI generation

### Phase 4: Community & Ecosystem 📋 (Future)

**Target**: Q3 2026

**Tasks**:
1. Theme marketplace integration
2. Community theme sharing
3. Plugin system for custom tools
4. VS Code extension integration

## Benefits for Agent-Assisted Development

### For Developers

- **Rapid prototyping**: "Create a settings page" → Complete XAML generated
- **Theme migration**: "Convert my Bootstrap theme" → Ready-to-use FlagstoneUI theme
- **Learning**: Agents show examples and explain patterns
- **Consistency**: Generated code follows best practices

### For Designers

- **Theme exploration**: See what existing themes look like in FlagstoneUI
- **Color experimentation**: Generate themes from color palettes
- **Token understanding**: Learn the token system through examples

### For Teams

- **Shared knowledge**: Documentation accessible to agents and humans
- **Code generation**: Consistent UI code across team
- **Theme management**: Centralized theme conversion and validation

## Integration Examples

### Claude Desktop

```json
{
  "mcpServers": {
    "flagstone-ui": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "/path/to/FlagstoneUI.Mcp/FlagstoneUI.Mcp.csproj"
      ]
    }
  }
}
```

**Example Prompts**:
- "Convert the Bootstrap Darkly theme to FlagstoneUI"
- "Create a login page using FlagstoneUI controls"
- "What tokens should I use for a primary button?"
- "Show me the FlagstoneUI architecture documentation"

### VS Code Copilot (Future)

Integration with GitHub Copilot via MCP protocol for inline code generation.

### Custom Agents

Any MCP-compatible client can use the Flagstone UI MCP server.

## Testing Strategy

### MCP Protocol Tests

- JSON-RPC request/response validation
- Tool invocation tests
- Error handling tests
- Schema validation

### Integration Tests

- End-to-end tool execution
- Multi-tool workflows
- Client compatibility tests

### Agent Behavior Tests

- Validate agent understanding
- Test generated code quality
- Ensure documentation accessibility

## Success Metrics

### Phase 2 (MCP Integration)

- [ ] 4+ tools available
- [ ] Compatible with 2+ MCP clients
- [ ] Documentation tool provides comprehensive coverage
- [ ] Bootstrap converter fully integrated

### Phase 3 (Advanced Tools)

- [ ] UI generation produces valid XAML
- [ ] Generated UIs follow best practices
- [ ] Theme generation creates valid tokens
- [ ] Agent can explain FlagstoneUI concepts

### Phase 4 (Ecosystem)

- [ ] Community adoption
- [ ] Theme marketplace integration
- [ ] VS Code extension released

## Related Documentation

- [Bootstrap Converter Library README](../tools/FlagstoneUI.BootstrapConverter/README.md)
- [Bootstrap Converter CLI README](../tools/FlagstoneUI.BootstrapConverter.Cli/README.md)
- [Token Catalog System](token-catalog-system.md)
- [Architecture](architecture.md)
- [Roadmap](roadmap.md)

## Next Steps

1. ✅ Complete Bootstrap converter UI app
2. ✅ Merge UI app changes to main branch
3. 📋 Integrate other branch changes
4. 📋 Create comprehensive `FlagstoneUI.Mcp` server
5. 📋 Implement core tools (Bootstrap converter, Documentation, Examples)
6. 📋 Test with Claude Desktop
7. 📋 Document MCP server usage
8. 📋 Release preview version

---

*This is a living document and will be updated as the MCP server is developed.*

**Last Updated**: December 2024  
**Next Review**: After Phase 2 completion
