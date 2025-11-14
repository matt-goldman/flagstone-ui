# ADR004: Cross-Assembly ResourceDictionary Loading

## Status
Accepted

## Context

Flagstone UI requires a robust mechanism for themes (in separate assemblies) to reference and consume design tokens from the core library (FlagstoneUI.Core). This is fundamental to the token-based theming architecture.

### The Problem

Initial attempts to use standard XAML `Source` property for cross-assembly resource loading failed:

```xml
<!-- This approach did NOT work -->
<ResourceDictionary.MergedDictionaries>
    <ResourceDictionary Source="/FlagstoneUI.Core;component/Styles/Tokens.xaml" />
</ResourceDictionary.MergedDictionaries>
```

This resulted in runtime failures where themes could not find or consume base tokens, requiring workarounds like duplicating token definitions in theme files.

### Requirements

1. Themes must be able to reference core tokens from FlagstoneUI.Core
2. Solution must work across all .NET MAUI platforms (Android, iOS, Windows, macOS)
3. Consumers must be able to reference themes from their applications
4. Implementation should follow Microsoft's recommended patterns

## Decision

Implement cross-assembly ResourceDictionary loading using **typed references with x:Class and code-behind files**, as recommended by Microsoft for .NET MAUI.

### Implementation Approach

1. **Add x:Class to ResourceDictionary files**:
   ```xml
   <!-- FlagstoneUI.Core/Styles/Tokens.xaml -->
   <ResourceDictionary xmlns="..."
                       x:Class="FlagstoneUI.Core.Styles.Tokens">
   ```

2. **Create code-behind files**:
   ```csharp
   // FlagstoneUI.Core/Styles/Tokens.xaml.cs
   namespace FlagstoneUI.Core.Styles;
   
   public partial class Tokens : ResourceDictionary
   {
       public Tokens()
       {
           InitializeComponent();
       }
   }
   ```

3. **Define global XAML namespaces**:
   ```csharp
   // FlagstoneUI.Core/GlobalXmlns.cs
   [assembly: XmlnsDefinition("http://flagstoneui.com/schemas/core", "FlagstoneUI.Core.Styles")]
   [assembly: XmlnsPrefix("http://flagstoneui.com/schemas/core", "tokens")]
   ```

4. **Use typed references in MergedDictionaries**:
   ```xml
   <!-- FlagstoneUI.Themes.Material/Theme.xaml -->
   <ResourceDictionary xmlns="..."
                       xmlns:tokens="clr-namespace:FlagstoneUI.Core.Styles;assembly=FlagstoneUI.Core"
                       x:Class="FlagstoneUI.Themes.Material.Theme">
       <ResourceDictionary.MergedDictionaries>
           <tokens:Tokens />
       </ResourceDictionary.MergedDictionaries>
   </ResourceDictionary>
   ```

5. **Consumers reference themes the same way**:
   ```xml
   <!-- Consumer's App.xaml -->
   <Application xmlns:material="clr-namespace:FlagstoneUI.Themes.Material;assembly=FlagstoneUI.Themes.Material">
       <Application.Resources>
           <ResourceDictionary>
               <ResourceDictionary.MergedDictionaries>
                   <material:Theme />
               </ResourceDictionary.MergedDictionaries>
           </ResourceDictionary>
       </Application.Resources>
   </Application>
   ```

## Consequences

### Positive

- ✅ **Reliable**: Works consistently across all platforms
- ✅ **Microsoft-recommended**: Follows official .NET MAUI patterns
- ✅ **Type-safe**: Compile-time checking of resource references
- ✅ **No duplication**: Themes can truly reference core tokens without copying
- ✅ **Scalable**: Pattern works for any number of themes and resource dictionaries
- ✅ **Clean**: Removes need for workarounds and duplicate token definitions

### Negative

- ⚠️ **Boilerplate**: Requires x:Class and code-behind file for each ResourceDictionary
- ⚠️ **Namespace declarations**: Consumers must declare namespaces (though .NET 10 global namespaces will improve this)
- ⚠️ **Learning curve**: Less obvious than URL-based Source property

### Migration Impact

- Removed temporary token duplications from theme files
- Updated all ResourceDictionary files to include x:Class
- Added code-behind files where needed
- Updated documentation to show correct usage pattern

## References

- [Microsoft Docs: ResourceDictionary in .NET MAUI](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/resource-dictionaries)
- [.NET MAUI GitHub Discussions on cross-assembly resources](https://github.com/dotnet/maui/discussions)

## Related ADRs

- ADR001: FsEntry Behavior and Validation Strategy
- ADR002: Project Templates (if exists)
- ADR003: Button Corner Radius Type Handling

*Date: November 2025*
