<div align="center">
  <img src="assets/logov1.svg" alt="Flagstone UI Logo" width="400" height="400">
  <h1>Flagstone UI</h1>
  <p><strong>A customisable UI framework for .NET MAUI</strong></p>
  <h2>⚠️ WARNING! Experimental ⚠️</h2>
</div>

**Bootstrap for .NET MAUI.** A token-based theming system that makes it easy to create beautiful, consistent UIs without deep platform knowledge.

![Demo Video](assets/Flagstone-UI.gif)

## Current Status: Experimental POC

Flagstone UI is an early prototype exploring a theming system for .NET MAUI apps. It provides a foundation for defining design tokens (colors, spacing, typography) and applying them to enhanced MAUI controls via XAML resource dictionaries.

It's available now for testing and experimentation, but there's still a lot to do. Available now:

* Four core controls: `FsButton`, `FsEntry`, `FsCard`, `FsEditor`
* Material theme `*`
* .NET MAUI Community Toolkit integration
    - `ValidationBehaviorAdapter`: Let's you re-use validators from the CommunityToolkit with FlagstoneUI input controls (`FsEntry` and `FsEditor`). See example implementation in the sample app.
    - Border animation for `FsEditor`. This is mostly just for fun and was used to create the `AiEditor` in the sample app. But you could easily adapt this for your own use case.

`*` Note that there are several themes included in the sample app. You can use these in your apps too if you wish, just copy the code. Themes are just resource dictionaries so you can just copy them into your own app. However the Material theme is the only one available as a NuGet package at this time. ALSO: feel free to create and share your own themes!

## Quick Start

**📚 [Full Documentation & Guides](docs/README.md)** | **🚀 [Quickstart Guide](docs/quickstart.md)**

This is still a very early prototype but you can start playing with it now.

### Build from Source

```bash
# Clone and explore
git clone https://github.com/matt-goldman/flagstone-ui.git
cd flagstone-ui

# Run the sample app (requires .NET 10 SDK + MAUI workload)
dotnet build
dotnet run --project samples/FlagstoneUI.SampleApp
```

Or reference the `FlagstoneUI.Core` project in your own MAUI app to start theming your controls.

### NugGet Package (Preview)

```bash
# Install Flagstone UI Core package
dotnet add package FlagstoneUI.Core --version 0.0.1-preview1

# (Optional) Install MAUI Community Toolkit integration package
dotnet add package FlagstoneUI.Integrations.MCT --version 0.0.1-preview1

# (Optional) Install Material theme package
dotnet add package FlagstoneUI.Themes.Material --version 0.0.1-preview1
```

## What Does It Look Like?

```xml
<!-- Simple themed button with consistent styling across platforms -->
<FsButton
    Text="Click Me"
    BackgroundColor="{DynamicResource Color.Primary}"
    CornerRadius="{DynamicResource Shape.CornerRadius.Medium}" />

<!-- Themed text entry with validation (using Community Toolkit) -->
<FsEntry
    Placeholder="Enter email"
    BackgroundColor="{DynamicResource Color.Surface}"
    BorderColor="{DynamicResource Color.Border}">
    <FsEntry.Behaviors>
        <toolkit:EmailValidationBehavior />
    </FsEntry.Behaviors>
</FsEntry>

<!-- Card container with theme tokens -->
<FsCard
    BackgroundColor="{DynamicResource Color.Surface}"
    CornerRadius="{DynamicResource Shape.CornerRadius.Large}"
    Padding="{DynamicResource Spacing.Medium}">
    <Label Text="Card Content" />
</FsCard>
```

**Note:** Above assumes you're using [XAML global and implicit namespaces](https://learn.microsoft.com/dotnet/maui/whats-new/dotnet-10?view=net-maui-10.0#implicit-and-global-xml-namespaces). Without that you would consume these with a namespace prefix like `<fs:FsButton>...</fs:FsButton>`.

## Key Concepts

**Design Tokens** → Define your design system once (colors, spacing, typography, shapes)
**Theme Files** → Apply tokens to controls via XAML resource dictionaries
**Flagstone Controls** → Enhanced MAUI controls that expose themable properties

Think of it like Bootstrap for web dev: you're still using standard HTML elements, but with consistent, customizable styling.

## Why Flagstone UI?

| Without Flagstone | With Flagstone |
|------------------|----------------|
| Write platform-specific handlers for styling | Use XAML properties that work everywhere |
| Different code for iOS/Android/Windows borders | One `BorderColor` property |
| Scattered styling across codebehind | Centralized theme tokens |
| Reinvent styling for each app | Reusable, shareable themes |

**Example:** Getting a rounded, bordered text entry:

```xml
<!-- Traditional MAUI: requires custom handlers for each platform -->
<Entry Placeholder="Email" />
<!-- + C# handler code for iOS UITextField styling -->
<!-- + C# handler code for Android EditText styling -->
<!-- + C# handler code for Windows styling -->

<!-- Flagstone: works everywhere out of the box -->
<FsEntry
    Placeholder="Email"
    CornerRadius="8"
    BorderColor="#2196F3"
    BorderWidth="2" />
```

**Pairs perfectly with [MAUI Community Toolkit](https://github.com/CommunityToolkit/Maui)** - Flagstone handles theming, MCT provides behaviors/converters.

**Learn more:** [Architecture](docs/architecture.md) | [Technical Plan](docs/technical-plan.md)

## Current Status

**🎯 Available Now:**
- ✅ Token system foundation
- ✅ Four core controls: `FsButton`, `FsEntry`, `FsCard`, `FsEditor`
- ✅ Material theme included
- ✅ Sample app with multiple themes
- ✅ [Complete documentation](docs/README.md)

**🚧 In Progress:**
- 🔨 Additional controls (labels, lists, navigation)
- 🔨 Bootstrap theme converter (convert web design systems to Flagstone themes)
- 🔨 AI-powered theme generation tooling

**🔮 Planned:**
- Visual theme generator (web & native)
- Theme sharing gallery
- Figma/~~Adobe XD~~ to Flagstone converters

See the full [roadmap](docs/roadmap.md) for details.

## Project Structure

```
flagstone-ui/
├── src/
│   ├── FlagstoneUI.Core/          # Core controls and token system
│   ├── FlagstoneUI.Themes.Material/ # Material theme
│   └── FlagstoneUI.Blocks/        # Reusable app screens (planned for MVP)
├── samples/
│   ├── FlagstoneUI.SampleApp/     # Main showcase app
│   └── FlagstoneUI.ThemePlayground/ # Theme experimentation
├── docs/                          # 📚 Complete documentation
└── tools/                         # AI tooling & converters
```

**Note**: The Blocks project will contain common UI building blocks (signup/signin forms, basic CRUD, etc.) and is planned as an extension for the MVP milestone. Currently at POC stage.

## Contributing

**This is an early experiment - feedback is gold!** 🙏

Most important: **Is this useful?** Tell me if you'd use it (or why you wouldn't). This helps validate the project direction.

**Ways to help:**
- 💬 Try the samples and share feedback (Issues welcome!)
- 🐛 Report bugs or suggest features
- 💻 Submit PRs (bug fixes, docs, new controls)
- 🎨 Create and share themes
- 📖 Improve documentation

**Questions?** Open a [Discussion](../../discussions) or ping [@matt-goldman](https://github.com/matt-goldman)

---

**License:** MIT | **Status:** Experimental POC | **Compatibility:** .NET 10 + MAUI
