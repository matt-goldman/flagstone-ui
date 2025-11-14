# AI Prompts for Flagstone UI

This directory contains prompts for AI-powered design tools (v0, GitHub Spark, Claude, ChatGPT, etc.) to help generate themes, palettes, and design assets for Flagstone UI.

**Audience**: These prompts are tooling for **theme creators and contributors**, not end users. If you're using Flagstone UI in your application (not creating themes), see the [Quickstart Guide](../quickstart.md) instead.

## Available Prompts

- **[generate-modern-theme.md](./generate-modern-theme.md)** - Generate a complete Modern theme palette with Material Design 3 semantic tokens

## Usage

1. Copy the prompt content
2. Paste into your AI tool (v0, Spark, Claude, etc.)
3. Attach reference files mentioned in the prompt
4. Review and iterate on the generated output
5. Validate with Flagstone UI tools:
   ```bash
   dotnet run --project tools/FlagstoneUI.TokenGenerator -- validate --input theme.json --json
   ```

## Tips for Best Results

- **Provide context**: Attach `tokens-catalog.json` and `tokens-schema.json` as reference
- **Be specific**: Describe the mood, industry, and use case for the theme
- **Iterate**: Use validation feedback to refine the output
- **Test contrast**: Ensure color combinations meet WCAG accessibility standards

## Contributing Prompts

When adding new prompts:
- Include clear objective and success criteria
- Reference the token schema structure
- Provide example output format
- List required attachments/context files
- Document validation steps
