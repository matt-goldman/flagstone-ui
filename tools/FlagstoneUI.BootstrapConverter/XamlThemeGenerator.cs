using FlagstoneUI.BootstrapConverter.Models;
using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace FlagstoneUI.BootstrapConverter;

/// <summary>
/// Generates XAML theme files from Flagstone tokens
/// </summary>
public class XamlThemeGenerator
{
	// TODO: Replace with the new single global MAUI namespace
    private const string MauiNamespace = "http://schemas.microsoft.com/dotnet/maui/global";
    private const string XamlNamespace = "http://schemas.microsoft.com/winfx/2009/xaml";

    /// <summary>
    /// Sanitizes a theme name to create a valid C# identifier
    /// </summary>
    private static string SanitizeThemeName(string themeName)
    {
        if (string.IsNullOrWhiteSpace(themeName))
		{
			return "Theme";
		}

		var sanitized = new StringBuilder();
        var needsCapital = true;

        foreach (var ch in themeName)
        {
            if (char.IsLetterOrDigit(ch))
            {
                sanitized.Append(needsCapital ? char.ToUpper(ch) : ch);
                needsCapital = false;
            }
            else if (ch == '_')
            {
                sanitized.Append('_');
                needsCapital = false;
            }
            else
            {
                // Skip invalid characters and capitalize next letter
                needsCapital = true;
            }
        }

        var result = sanitized.ToString();
        
        // Ensure it starts with a letter or underscore
        if (result.Length > 0 && char.IsDigit(result[0]))
		{
			result = "_" + result;
		}

		return string.IsNullOrEmpty(result) ? "Theme" : result;
    }

    /// <summary>
    /// Generate Tokens.xaml file from Flagstone tokens
    /// </summary>
    /// <param name="tokens">Flagstone tokens</param>
    /// <param name="options">Conversion options</param>
    /// <returns>XAML content as string</returns>
    public string GenerateTokensXaml(FlagstoneTokens tokens, ConversionOptions? options = null)
    {
        options ??= new ConversionOptions();

        ConverterLogger.Debug($"GenerateTokensXaml: Colors={tokens.Colors.Count}, Typography={tokens.Typography.Count}, Shadows={tokens.Shadows.Count}");

        var doc = CreateXamlDocument();
        var root = doc.Root!;

        // Add color tokens
        if (tokens.Colors.Count > 0)
        {
            AddColorTokens(root, tokens.Colors, options);
        }

        // Add typography tokens
        if (tokens.Typography.Count > 0)
        {
            AddTypographyTokens(root, tokens.Typography, options);
        }

        // Add spacing tokens
        if (tokens.Spacing.Count > 0)
        {
            AddNumericTokens(root, tokens.Spacing, "Spacing", options);
        }

        // Add border radius tokens
        if (tokens.BorderRadius.Count > 0)
        {
            AddNumericTokens(root, tokens.BorderRadius, "Corner Radius", options);
        }

        // Add border width tokens
        if (tokens.BorderWidth.Count > 0)
        {
            AddNumericTokens(root, tokens.BorderWidth, "Border Width", options);
        }

        // Add per-edge border width tokens
        if (tokens.BorderTopWidth.Count > 0)
        {
            AddNumericTokens(root, tokens.BorderTopWidth, "Border Top Width", options);
        }

        if (tokens.BorderRightWidth.Count > 0)
        {
            AddNumericTokens(root, tokens.BorderRightWidth, "Border Right Width", options);
        }

        if (tokens.BorderBottomWidth.Count > 0)
        {
            AddNumericTokens(root, tokens.BorderBottomWidth, "Border Bottom Width", options);
        }

        if (tokens.BorderLeftWidth.Count > 0)
        {
            AddNumericTokens(root, tokens.BorderLeftWidth, "Border Left Width", options);
        }

        // Add shadow tokens
        if (tokens.Shadows.Count > 0)
        {
            AddShadowTokens(root, tokens.Shadows, options);
        }

        return FormatXamlDocument(doc);
    }

    /// <summary>
    /// Generate Theme.xaml file from Flagstone tokens
    /// </summary>
    /// <param name="tokens">Flagstone tokens</param>
    /// <param name="themeName">Theme name</param>
    /// <param name="options">Conversion options</param>
    /// <returns>XAML content as string</returns>
    public string GenerateThemeXaml(FlagstoneTokens tokens, string themeName, ConversionOptions? options = null)
    {
        options ??= new ConversionOptions();

        // Sanitize theme name for class name
        var sanitizedThemeName = SanitizeThemeName(themeName);
        var doc = CreateXamlDocument(withClass: true, className: $"{options.Namespace}.{sanitizedThemeName}");
        var root = doc.Root!;

        var mauiNs = root.Name.Namespace;

        // Add Flagstone namespace for controls
        root.Add(new XAttribute(XNamespace.Xmlns + "fs", "clr-namespace:FlagstoneUI.Core.Controls;assembly=FlagstoneUI.Core"));

        // Add comment header
        root.Add(new XComment($" {themeName} Theme - Generated from Bootstrap "));
        root.Add(new XComment(" This theme imports tokens and provides base styles for controls "));
        root.Add(new XText("\n\n"));

        // Add merged dictionaries for tokens
        var mergedDictionaries = new XElement(mauiNs + "ResourceDictionary.MergedDictionaries",
            new XElement(mauiNs + "ResourceDictionary",
                new XAttribute("Source", "Tokens.xaml")
            )
        );
        root.Add(mergedDictionaries);

        root.Add(new XText("\n\n"));
        root.Add(new XComment(" Base control styles can be added here "));

        return FormatXamlDocument(doc);
    }

    /// <summary>
    /// Generate both Tokens.xaml and Theme.xaml files
    /// </summary>
    /// <param name="tokens">Flagstone tokens</param>
    /// <param name="themeName">Theme name</param>
    /// <param name="outputDirectory">Output directory path</param>
    /// <param name="options">Conversion options</param>
    /// <param name="componentStyles">Optional Bootstrap component computed styles (used to better match sizing/padding where available)</param>
    [Obsolete("File I/O should be handled by consumers. Use GenerateTokensXaml(), GenerateThemeXaml(), GenerateStylesXaml(), and GenerateCodeBehind() to get strings, then write files as needed.")]
    public async Task GenerateFilesAsync(FlagstoneTokens tokens, string themeName, string outputDirectory, ConversionOptions? options = null, BootstrapComponentStyles? componentStyles = null)
    {
        options ??= new ConversionOptions();

        // Sanitize theme name for use in class names
        var sanitizedThemeName = SanitizeThemeName(themeName);

        // Create output directory if it doesn't exist
        Directory.CreateDirectory(outputDirectory);

        // Generate Tokens.xaml
        var tokensXaml = GenerateTokensXaml(tokens, options);
        var tokensPath = Path.Combine(outputDirectory, "Tokens.xaml");
        await File.WriteAllTextAsync(tokensPath, tokensXaml);

        // Generate Theme.xaml with code-behind
        var themeXaml = GenerateThemeXaml(tokens, themeName, options);
        var themePath = Path.Combine(outputDirectory, "Theme.xaml");
        await File.WriteAllTextAsync(themePath, themeXaml);

        // Generate Theme.xaml.cs code-behind
        var themeClassName = $"{options.Namespace}.{sanitizedThemeName}";
        var themeCodeBehind = GenerateCodeBehind(themeClassName, themeName);
        var themeCodeBehindPath = Path.Combine(outputDirectory, "Theme.xaml.cs");
        await File.WriteAllTextAsync(themeCodeBehindPath, themeCodeBehind);

        // Generate Styles.xaml with code-behind
        var stylesXaml = GenerateStylesXaml(tokens, themeName, componentStyles, options);
        var stylesPath = Path.Combine(outputDirectory, "Styles.xaml");
        await File.WriteAllTextAsync(stylesPath, stylesXaml);

        // Generate Styles.xaml.cs code-behind
        var stylesClassName = $"{options.Namespace}.{sanitizedThemeName}Styles";
        var stylesCodeBehind = GenerateCodeBehind(stylesClassName, $"{themeName} Styles");
        var stylesCodeBehindPath = Path.Combine(outputDirectory, "Styles.xaml.cs");
        await File.WriteAllTextAsync(stylesCodeBehindPath, stylesCodeBehind);
    }

    private static XDocument CreateXamlDocument(bool withClass = false, string? className = null)
    {
        var mauiNs = XNamespace.Get(MauiNamespace);
        var xNs = XNamespace.Get(XamlNamespace);

        var rootElement = new XElement(mauiNs + "ResourceDictionary",
            new XAttribute(XNamespace.Xmlns + "x", xNs.NamespaceName)
        );

        // Add x:Class attribute if requested
        if (withClass && !string.IsNullOrWhiteSpace(className))
        {
            rootElement.Add(new XAttribute(xNs + "Class", className));
        }

        var doc = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            rootElement
        );

        // Add XAML compilation processing instruction if class is specified
        if (withClass)
        {
            doc.AddFirst(new XProcessingInstruction("xaml-comp", "compile=\"true\""));
        }

        return doc;
    }

    /// <summary>
    /// Create a resource reference for a color token, using AppThemeBinding if dark mode values exist
    /// </summary>
    /// <param name="tokenKey">Color token key (e.g., "Color.Primary")</param>
    /// <param name="tokens">Flagstone tokens to check for dark mode values</param>
    /// <returns>XAML value string - either DynamicResource or AppThemeBinding</returns>
    private string CreateColorResourceReference(string tokenKey, FlagstoneTokens tokens)
    {
        // Check if this token has a dark mode value
        if (tokens.Colors.TryGetValue(tokenKey, out var colorToken) && 
            !string.IsNullOrWhiteSpace(colorToken.DarkValue))
        {
            // Use AppThemeBinding
            return $"{{AppThemeBinding Light={{DynamicResource {tokenKey}}}, Dark={{DynamicResource {tokenKey}.Dark}}}}";
        }
        
        // Use simple DynamicResource
        return $"{{DynamicResource {tokenKey}}}";
    }

    private void AddColorTokens(XElement root, Dictionary<string, ColorToken> colors, ConversionOptions options)
    {
        var mauiNs = root.Name.Namespace;
        var xNs = root.GetNamespaceOfPrefix("x") ?? XNamespace.Get(XamlNamespace);

        root.Add(new XComment(" ===== Color Tokens ===== "));

        foreach (var (_, token) in colors.OrderBy(kvp => kvp.Key))
        {
            // Add purpose as comment if available
            if (options.IncludeComments && !string.IsNullOrWhiteSpace(token.Purpose))
            {
                root.Add(new XComment($" {token.Key}: {token.Purpose} "));
            }

            // Add light mode color
            var colorElement = new XElement(mauiNs + "Color",
                new XAttribute(xNs + "Key", token.Key),
                token.Value
            );
            root.Add(colorElement);

            // Add dark mode color if available
            if (!string.IsNullOrWhiteSpace(token.DarkValue))
            {
                var darkColorElement = new XElement(mauiNs + "Color",
                    new XAttribute(xNs + "Key", $"{token.Key}.Dark"),
                    token.DarkValue
                );
                root.Add(darkColorElement);
            }
        }

        root.Add(new XText("\n"));
    }

    private void AddTypographyTokens(XElement root, Dictionary<string, TypographyToken> typography, ConversionOptions options)
    {
        var xNs = root.GetNamespaceOfPrefix("x") ?? XNamespace.Get(XamlNamespace);

        root.Add(new XComment(" ===== Typography Tokens ===== "));

        foreach (var (_, token) in typography.OrderBy(kvp => kvp.Key))
        {
            // Add purpose as comment if available
            if (options.IncludeComments && !string.IsNullOrWhiteSpace(token.Purpose))
            {
                root.Add(new XComment($" {token.Key}: {token.Purpose} "));
            }

            // Determine x:DataType based on token key
            XName dataTypeName = token.Key switch
            {
                var k when k.Contains("FontSize", StringComparison.Ordinal) => xNs + "Double",
                var k when k.Contains("LineHeight", StringComparison.Ordinal) => xNs + "Double",
                _ => xNs + "String" // FontFamily and other string types
            };

            var element = new XElement(dataTypeName,
                new XAttribute(xNs + "Key", token.Key),
                token.Value
            );

            root.Add(element);
        }

        root.Add(new XText("\n"));
    }

    private void AddNumericTokens(XElement root, Dictionary<string, NumericToken> tokens, string categoryName, ConversionOptions options)
    {
        var xNs = root.GetNamespaceOfPrefix("x") ?? XNamespace.Get(XamlNamespace);

        root.Add(new XComment($" ===== {categoryName} Tokens ===== "));

        // If this is Corner Radius tokens, generate both Double and Int32 versions
        if (categoryName == "Corner Radius")
        {
            // First add standard Double-typed radius tokens for FsCard, FsEntry, FsEditor
            foreach (var (_, token) in tokens.OrderBy(kvp => kvp.Key))
            {
                if (options.IncludeComments && !string.IsNullOrWhiteSpace(token.Purpose))
                {
                    root.Add(new XComment($" {token.Key}: {token.Purpose} "));
                }

                var element = new XElement(xNs + "Double",
                    new XAttribute(xNs + "Key", token.Key),
                    token.Value.ToString(CultureInfo.InvariantCulture)
                );

                root.Add(element);
            }

            root.Add(new XText("\n"));
            root.Add(new XComment(" Button-specific radius tokens (Int32 for Button.CornerRadius) "));

            // Now add Int32-typed button radius tokens
            foreach (var (key, token) in tokens.OrderBy(kvp => kvp.Key))
            {
                // Generate button-specific key (e.g., Radius.Small -> Radius.Button.Small)
                var buttonKey = key.Replace("Radius.", "Radius.Button.", StringComparison.Ordinal);
                
                if (options.IncludeComments && !string.IsNullOrWhiteSpace(token.Purpose))
                {
                    root.Add(new XComment($" {buttonKey}: {token.Purpose} (for buttons) "));
                }

                var element = new XElement(xNs + "Int32",
                    new XAttribute(xNs + "Key", buttonKey),
                    ((int)token.Value).ToString(CultureInfo.InvariantCulture)
                );

                root.Add(element);
            }
        }
        else
        {
            // For other numeric tokens (spacing, border width), just generate Double version
            foreach (var (_, token) in tokens.OrderBy(kvp => kvp.Key))
            {
                if (options.IncludeComments && !string.IsNullOrWhiteSpace(token.Purpose))
                {
                    root.Add(new XComment($" {token.Key}: {token.Purpose} "));
                }

                var element = new XElement(xNs + "Double",
                    new XAttribute(xNs + "Key", token.Key),
                    token.Value.ToString(CultureInfo.InvariantCulture)
                );

                root.Add(element);
            }
        }

        root.Add(new XText("\n"));
    }

    private void AddShadowTokens(XElement root, Dictionary<string, ShadowToken> shadows, ConversionOptions options)
    {
        var xNs = root.GetNamespaceOfPrefix("x") ?? XNamespace.Get(XamlNamespace);
        var mauiNs = root.Name.Namespace;

        root.Add(new XComment(" ===== Shadow Tokens ===== "));

        foreach (var (_, token) in shadows.OrderBy(kvp => kvp.Key))
        {
            if (options.IncludeComments && !string.IsNullOrWhiteSpace(token.Purpose))
            {
                root.Add(new XComment($" {token.Key}: {token.Purpose} "));
            }

            // Create Shadow element with properties
            var shadowElement = new XElement(mauiNs + "Shadow",
                new XAttribute(xNs + "Key", token.Key)
            );

            // Add Offset property (combining OffsetX and OffsetY)
            shadowElement.Add(new XElement(mauiNs + "Shadow.Offset",
                new XText($"{token.OffsetX.ToString(CultureInfo.InvariantCulture)}, {token.OffsetY.ToString(CultureInfo.InvariantCulture)}")
            ));

            // Add Radius property
            shadowElement.Add(new XElement(mauiNs + "Shadow.Radius",
                new XText(token.Radius.ToString(CultureInfo.InvariantCulture))
            ));

            // Add Brush property with color and opacity
            var brush = new XElement(mauiNs + "Shadow.Brush",
                new XElement(mauiNs + "SolidColorBrush",
                    new XAttribute("Color", token.Color),
                    new XAttribute("Opacity", token.Opacity.ToString(CultureInfo.InvariantCulture))
                )
            );
            shadowElement.Add(brush);

            root.Add(shadowElement);
        }

        root.Add(new XText("\n"));
    }

    /// <summary>
    /// Generate .xaml.cs code-behind file for a theme
    /// </summary>
    /// <param name="className">Full class name including namespace</param>
    /// <param name="themeName">Theme name for documentation</param>
    /// <returns>C# code as string</returns>
    public string GenerateCodeBehind(string className, string themeName)
    {
        var lastDot = className.LastIndexOf('.');
        var namespaceName = lastDot > 0 ? className.Substring(0, lastDot) : className;
        var simpleClassName = lastDot > 0 ? className.Substring(lastDot + 1) : className;

        var sb = new StringBuilder();
        sb.AppendLine($"namespace {namespaceName};");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// {themeName} theme resource dictionary for Flagstone UI controls.");
        sb.AppendLine("/// Generated from Bootstrap theme.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"public partial class {simpleClassName} : ResourceDictionary");
        sb.AppendLine("{");
        sb.AppendLine($"\tpublic {simpleClassName}()");
        sb.AppendLine("\t{");
        sb.AppendLine("\t\tInitializeComponent();");
        sb.AppendLine("\t}");
        sb.AppendLine("}");

        return sb.ToString();
    }

    /// <summary>
    /// Generate Styles.xaml file with button styles
    /// </summary>
    /// <param name="tokens">Flagstone tokens</param>
    /// <param name="themeName">Theme name</param>
    /// <param name="options">Conversion options</param>
    /// <returns>XAML content as string</returns>
    public string GenerateStylesXaml(FlagstoneTokens tokens, string themeName, ConversionOptions? options = null)
    {
        return GenerateStylesXaml(tokens, themeName, componentStyles: null, options);
    }

    /// <summary>
    /// Generate Styles.xaml file with styles for FlagstoneUI controls.
    /// </summary>
    /// <param name="tokens">Flagstone tokens</param>
    /// <param name="themeName">Theme name</param>
    /// <param name="componentStyles">Optional computed Bootstrap component styles (used for size/padding defaults where available)</param>
    /// <param name="options">Conversion options</param>
    /// <param name="includeMergedDictionaries">If true, includes MergedDictionaries reference to Tokens.xaml. Set to false for in-memory operations.</param>
    /// <returns>XAML content as string</returns>
    public string GenerateStylesXaml(FlagstoneTokens tokens, string themeName, BootstrapComponentStyles? componentStyles, ConversionOptions? options, bool includeMergedDictionaries)
    {
        options ??= new ConversionOptions();

        // Sanitize theme name for class name
        var sanitizedThemeName = SanitizeThemeName(themeName);
        var doc = CreateXamlDocument(withClass: true, className: $"{options.Namespace}.{sanitizedThemeName}Styles");
        var root = doc.Root!;

        // Add Flagstone namespace for controls
        root.Add(new XAttribute(XNamespace.Xmlns + "fs", "clr-namespace:FlagstoneUI.Core.Controls;assembly=FlagstoneUI.Core"));

        // Add comment header
        root.Add(new XComment($" {themeName} Styles - Generated from Bootstrap "));
        root.Add(new XComment(" Control styles that use theme tokens "));
        root.Add(new XText("\n\n"));

        // Merge Tokens.xaml to avoid duplication (only if generating files)
        if (includeMergedDictionaries)
        {
            var mauiNs = root.Name.Namespace;
            var mergedDictionaries = new XElement(mauiNs + "ResourceDictionary.MergedDictionaries");
            var tokensDictionary = new XElement(mauiNs + "ResourceDictionary",
                new XAttribute("Source", "Tokens.xaml"));
            mergedDictionaries.Add(tokensDictionary);
            root.AddFirst(mergedDictionaries);
            root.AddFirst(new XText("\n"));
        }

        // Add control styles
        AddButtonStyles(root, tokens, componentStyles, options);
        AddEntryStyles(root, tokens, componentStyles, options);
        AddEditorStyles(root, tokens, componentStyles, options);
        AddCardStyles(root, tokens, componentStyles, options);

        return FormatXamlDocument(doc);
    }

    /// <summary>
    /// Generate Styles.xaml file with styles for FlagstoneUI controls.
    /// </summary>
    /// <param name="tokens">Flagstone tokens</param>
    /// <param name="themeName">Theme name</param>
    /// <param name="componentStyles">Optional computed Bootstrap component styles (used for size/padding defaults where available)</param>
    /// <param name="options">Conversion options</param>
    /// <returns>XAML content as string</returns>
    public string GenerateStylesXaml(FlagstoneTokens tokens, string themeName, BootstrapComponentStyles? componentStyles, ConversionOptions? options = null)
    {
        return GenerateStylesXaml(tokens, themeName, componentStyles, options, includeMergedDictionaries: true);
    }

    private void AddButtonStyles(XElement root, FlagstoneTokens tokens, BootstrapComponentStyles? componentStyles, ConversionOptions options)
    {
        var mauiNs = root.Name.Namespace;
        var xNs = root.GetNamespaceOfPrefix("x") ?? XNamespace.Get(XamlNamespace);

        root.Add(new XComment(" ========== FsButton Styles ========== "));
        root.Add(new XText("\n\n"));

        // Default filled button style
        if (options.IncludeComments)
        {
            root.Add(new XComment(" Base Button Style (Filled Button - Primary) "));
        }

        var defaultStyle = new XElement(mauiNs + "Style",
            new XAttribute("TargetType", "fs:FsButton")
        );

        // Background and text colors
        defaultStyle.Add(CreateSetter(mauiNs, "BackgroundColor", CreateColorResourceReference("Color.Primary", tokens)));
        
        // Try to find OnPrimary color, fallback to white
        var textColor = tokens.Colors.ContainsKey("Color.OnPrimary") 
            ? CreateColorResourceReference("Color.OnPrimary", tokens)
            : "#FFFFFF";
        defaultStyle.Add(CreateSetter(mauiNs, "TextColor", textColor));

        // Corner radius - only use if theme provides radius tokens
        if (tokens.BorderRadius.Count > 0)
        {
            // Use the first available radius token from the theme
            var radiusKey = tokens.BorderRadius.Keys.First();
            defaultStyle.Add(CreateSetter(mauiNs, "CornerRadius", $"{{DynamicResource {radiusKey}}}"));
        }

        // Padding - use spacing if available
        var padding = tokens.Spacing.ContainsKey("Spacing.Medium") 
            ? "{DynamicResource Spacing.Medium}" 
            : "24,10";
        defaultStyle.Add(CreateSetter(mauiNs, "Padding", padding));

        // Font size if available - prefer LabelLarge for buttons
        if (tokens.Typography.ContainsKey("FontSize.LabelLarge"))
        {
            defaultStyle.Add(CreateSetter(mauiNs, "FontSize", "{DynamicResource FontSize.LabelLarge}"));
        }
        else if (tokens.Typography.ContainsKey("FontSize.Body"))
        {
            defaultStyle.Add(CreateSetter(mauiNs, "FontSize", "{DynamicResource FontSize.Body}"));
        }

        defaultStyle.Add(CreateSetter(mauiNs, "MinimumHeightRequest", "40"));

        // Add shadow if available - prefer Shadow.Button, fallback to Shadow.Default
        if (tokens.Shadows.ContainsKey("Shadow.Button"))
        {
            defaultStyle.Add(CreateSetter(mauiNs, "Shadow", "{DynamicResource Shadow.Button}"));
        }
        else if (tokens.Shadows.ContainsKey("Shadow.Default"))
        {
            defaultStyle.Add(CreateSetter(mauiNs, "Shadow", "{DynamicResource Shadow.Default}"));
        }

        // Add disabled visual state
        AddButtonVisualStates(defaultStyle, mauiNs, xNs);

        root.Add(defaultStyle);
        root.Add(new XText("\n\n"));

        // Outlined button style
        if (options.IncludeComments)
        {
            root.Add(new XComment(" Outlined Button Style "));
        }

        var outlinedStyle = new XElement(mauiNs + "Style",
            new XAttribute(xNs + "Key", "OutlinedButton"),
            new XAttribute("TargetType", "fs:FsButton")
        );

        outlinedStyle.Add(CreateSetter(mauiNs, "BackgroundColor", "Transparent"));
        outlinedStyle.Add(CreateSetter(mauiNs, "TextColor", CreateColorResourceReference("Color.Primary", tokens)));
        
        // Border
        var borderColor = tokens.Colors.ContainsKey("Color.Outline") 
            ? CreateColorResourceReference("Color.Outline", tokens)
            : CreateColorResourceReference("Color.Primary", tokens);
        outlinedStyle.Add(CreateSetter(mauiNs, "BorderColor", borderColor));
        
        if (tokens.BorderWidth.ContainsKey("BorderWidth.Thin"))
        {
            outlinedStyle.Add(CreateSetter(mauiNs, "BorderWidth", "{DynamicResource BorderWidth.Thin}"));
        }
        else if (tokens.BorderWidth.ContainsKey("BorderWidth.Default"))
        {
            outlinedStyle.Add(CreateSetter(mauiNs, "BorderWidth", "{DynamicResource BorderWidth.Default}"));
        }
        else
        {
            outlinedStyle.Add(CreateSetter(mauiNs, "BorderWidth", "1"));
        }

        // Use the same corner radius logic as default style
        if (tokens.BorderRadius.Count > 0)
        {
            var radiusKey = tokens.BorderRadius.Keys.First();
            outlinedStyle.Add(CreateSetter(mauiNs, "CornerRadius", $"{{DynamicResource {radiusKey}}}"));
        }

        outlinedStyle.Add(CreateSetter(mauiNs, "Padding", padding));
        
        if (tokens.Typography.ContainsKey("FontSize.LabelLarge"))
        {
            outlinedStyle.Add(CreateSetter(mauiNs, "FontSize", "{DynamicResource FontSize.LabelLarge}"));
        }
        else if (tokens.Typography.ContainsKey("FontSize.Body"))
        {
            outlinedStyle.Add(CreateSetter(mauiNs, "FontSize", "{DynamicResource FontSize.Body}"));
        }

        outlinedStyle.Add(CreateSetter(mauiNs, "MinimumHeightRequest", "40"));

        // Add shadow if available - prefer Shadow.Button, fallback to Shadow.Default
        if (tokens.Shadows.ContainsKey("Shadow.Button"))
        {
            outlinedStyle.Add(CreateSetter(mauiNs, "Shadow", "{DynamicResource Shadow.Button}"));
        }
        else if (tokens.Shadows.ContainsKey("Shadow.Default"))
        {
            outlinedStyle.Add(CreateSetter(mauiNs, "Shadow", "{DynamicResource Shadow.Default}"));
        }

        AddButtonVisualStates(outlinedStyle, mauiNs, xNs);

        root.Add(outlinedStyle);
        root.Add(new XText("\n\n"));

        // Text button style
        if (options.IncludeComments)
        {
            root.Add(new XComment(" Text Button Style (No background or border) "));
        }

        var textButtonStyle = new XElement(mauiNs + "Style",
            new XAttribute(xNs + "Key", "TextButton"),
            new XAttribute("TargetType", "fs:FsButton")
        );

        textButtonStyle.Add(CreateSetter(mauiNs, "BackgroundColor", "Transparent"));
        textButtonStyle.Add(CreateSetter(mauiNs, "TextColor", "{DynamicResource Color.Primary}"));
        
        // Use the same corner radius logic as other styles
        if (tokens.BorderRadius.Count > 0)
        {
            var radiusKey = tokens.BorderRadius.Keys.First();
            textButtonStyle.Add(CreateSetter(mauiNs, "CornerRadius", $"{{DynamicResource {radiusKey}}}"));
        }

        textButtonStyle.Add(CreateSetter(mauiNs, "Padding", "12,10"));
        
        if (tokens.Typography.ContainsKey("FontSize.LabelLarge"))
        {
            textButtonStyle.Add(CreateSetter(mauiNs, "FontSize", "{DynamicResource FontSize.LabelLarge}"));
        }
        else if (tokens.Typography.ContainsKey("FontSize.Body"))
        {
            textButtonStyle.Add(CreateSetter(mauiNs, "FontSize", "{DynamicResource FontSize.Body}"));
        }

        textButtonStyle.Add(CreateSetter(mauiNs, "MinimumHeightRequest", "40"));

        // Add shadow if available - prefer Shadow.Button, fallback to Shadow.Default
        if (tokens.Shadows.ContainsKey("Shadow.Button"))
        {
            textButtonStyle.Add(CreateSetter(mauiNs, "Shadow", "{DynamicResource Shadow.Button}"));
        }
        else if (tokens.Shadows.ContainsKey("Shadow.Default"))
        {
            textButtonStyle.Add(CreateSetter(mauiNs, "Shadow", "{DynamicResource Shadow.Default}"));
        }

        AddButtonVisualStates(textButtonStyle, mauiNs, xNs);

        root.Add(textButtonStyle);
        root.Add(new XText("\n"));

        // Additional Bootstrap-like variants
        root.Add(new XText("\n\n"));
        if (options.IncludeComments)
        {
            root.Add(new XComment(" Semantic button variants " ));
        }

        var filledVariants = new (string KeySuffix, string BgToken, string? OnToken)[]
        {
            ("Secondary", "Color.Secondary", "Color.OnSecondary"),
            ("Success", "Color.Success", "Color.OnSuccess"),
            ("Danger", "Color.Error", "Color.OnError"),
            ("Warning", "Color.Warning", "Color.OnWarning"),
            ("Info", "Color.Info", "Color.OnInfo"),
            ("Light", tokens.Colors.ContainsKey("Color.Light") ? "Color.Light" : "Color.Surface", tokens.Colors.ContainsKey("Color.OnLight") ? "Color.OnLight" : "Color.OnBackground"),
            ("Dark", tokens.Colors.ContainsKey("Color.Dark") ? "Color.Dark" : "Color.SurfaceVariant.Dark", tokens.Colors.ContainsKey("Color.OnDark") ? "Color.OnDark" : "Color.OnBackground")
        };

        foreach (var (keySuffix, bgToken, onToken) in filledVariants)
        {
            AddButtonFilledVariantStyle(root, tokens, keySuffix, bgToken, onToken, options);
        }

        root.Add(new XText("\n\n"));
        if (options.IncludeComments)
        {
            root.Add(new XComment(" Outline button variants " ));
        }

        var outlineVariants = new (string KeySuffix, string Token)[]
        {
            ("Primary", "Color.Primary"),
            ("Secondary", "Color.Secondary"),
            ("Success", "Color.Success"),
            ("Danger", "Color.Error"),
            ("Warning", "Color.Warning"),
            ("Info", "Color.Info"),
            ("Light", tokens.Colors.ContainsKey("Color.Light") ? "Color.Light" : "Color.Surface"),
            ("Dark", tokens.Colors.ContainsKey("Color.Dark") ? "Color.Dark" : "Color.SurfaceVariant.Dark")
        };

        foreach (var (keySuffix, tokenKey) in outlineVariants)
        {
            AddButtonOutlineVariantStyle(root, tokens, keySuffix, tokenKey, options);
        }

        root.Add(new XText("\n\n"));
        if (options.IncludeComments)
        {
            root.Add(new XComment(" Size variants " ));
        }
        AddButtonSizeStyles(root, tokens, componentStyles, options);
    }

    private void AddButtonFilledVariantStyle(XElement root, FlagstoneTokens tokens, string keySuffix, string backgroundTokenKey, string? onTokenKey, ConversionOptions options)
    {
        if (!tokens.Colors.ContainsKey(backgroundTokenKey))
            return;

        var mauiNs = root.Name.Namespace;
        var xNs = root.GetNamespaceOfPrefix("x") ?? XNamespace.Get(XamlNamespace);

        var style = new XElement(mauiNs + "Style",
            new XAttribute(xNs + "Key", $"Button{keySuffix}"),
            new XAttribute("TargetType", "fs:FsButton")
        );

        style.Add(CreateSetter(mauiNs, "BackgroundColor", $"{{DynamicResource {backgroundTokenKey}}}"));
        var textColor = (onTokenKey != null && tokens.Colors.ContainsKey(onTokenKey))
            ? $"{{DynamicResource {onTokenKey}}}"
            : (tokens.Colors.ContainsKey($"Color.On{keySuffix}") ? $"{{DynamicResource Color.On{keySuffix}}}" : "#FFFFFF");
        style.Add(CreateSetter(mauiNs, "TextColor", textColor));

        // Shared geometry/spacing/typography settings (match base style defaults)
        var radiusKey = GetPreferredRadiusKey(tokens);
        if (!string.IsNullOrWhiteSpace(radiusKey))			
        {
            style.Add(CreateSetter(mauiNs, "CornerRadius", $"{{DynamicResource {radiusKey}}}"));
        }

        style.Add(CreateSetter(mauiNs, "Padding", GetPreferredButtonPadding(tokens)));
        AddButtonTypography(style, mauiNs, tokens);
        style.Add(CreateSetter(mauiNs, "MinimumHeightRequest", "40"));
        AddButtonVisualStates(style, mauiNs, xNs);

        root.Add(style);
        root.Add(new XText("\n\n"));
    }

    private void AddButtonOutlineVariantStyle(XElement root, FlagstoneTokens tokens, string keySuffix, string tokenKey, ConversionOptions options)
    {
        if (!tokens.Colors.ContainsKey(tokenKey))
            return;

        var mauiNs = root.Name.Namespace;
        var xNs = root.GetNamespaceOfPrefix("x") ?? XNamespace.Get(XamlNamespace);

        var style = new XElement(mauiNs + "Style",
            new XAttribute(xNs + "Key", $"ButtonOutline{keySuffix}"),
            new XAttribute("TargetType", "fs:FsButton")
        );

        style.Add(CreateSetter(mauiNs, "BackgroundColor", "Transparent"));
        style.Add(CreateSetter(mauiNs, "TextColor", $"{{DynamicResource {tokenKey}}}"));
        style.Add(CreateSetter(mauiNs, "BorderColor", $"{{DynamicResource {tokenKey}}}"));
        style.Add(CreateSetter(mauiNs, "BorderWidth", GetPreferredBorderWidth(tokens)));

        var radiusKey = GetPreferredRadiusKey(tokens);
        if (!string.IsNullOrWhiteSpace(radiusKey))
        {
            style.Add(CreateSetter(mauiNs, "CornerRadius", $"{{DynamicResource {radiusKey}}}"));
        }

        style.Add(CreateSetter(mauiNs, "Padding", GetPreferredButtonPadding(tokens)));
        AddButtonTypography(style, mauiNs, tokens);
        style.Add(CreateSetter(mauiNs, "MinimumHeightRequest", "40"));
        AddButtonVisualStates(style, mauiNs, xNs);

        root.Add(style);
        root.Add(new XText("\n\n"));
    }

    private void AddButtonSizeStyles(XElement root, FlagstoneTokens tokens, BootstrapComponentStyles? componentStyles, ConversionOptions options)
    {
        var mauiNs = root.Name.Namespace;
        var xNs = root.GetNamespaceOfPrefix("x") ?? XNamespace.Get(XamlNamespace);

        var (smPadding, lgPadding) = GetButtonSizePaddings(componentStyles);

        var smallStyle = new XElement(mauiNs + "Style",
            new XAttribute(xNs + "Key", "ButtonSmall"),
            new XAttribute("TargetType", "fs:FsButton")
        );
        smallStyle.Add(CreateSetter(mauiNs, "Padding", smPadding));
        if (tokens.Typography.ContainsKey("FontSize.Button"))
        {
            smallStyle.Add(CreateSetter(mauiNs, "FontSize", "{DynamicResource FontSize.Button}"));
        }
        root.Add(smallStyle);
        root.Add(new XText("\n\n"));

        var largeStyle = new XElement(mauiNs + "Style",
            new XAttribute(xNs + "Key", "ButtonLarge"),
            new XAttribute("TargetType", "fs:FsButton")
        );
        largeStyle.Add(CreateSetter(mauiNs, "Padding", lgPadding));
        if (tokens.Typography.ContainsKey("FontSize.Button"))
        {
            largeStyle.Add(CreateSetter(mauiNs, "FontSize", "{DynamicResource FontSize.Button}"));
        }
        root.Add(largeStyle);
        root.Add(new XText("\n"));
    }

    private static (string SmallPadding, string LargePadding) GetButtonSizePaddings(BootstrapComponentStyles? componentStyles)
    {
        var sm = TryGetPaddingFromButtonSize(componentStyles?.ButtonSmall) ?? "16,8";
        var lg = TryGetPaddingFromButtonSize(componentStyles?.ButtonLarge) ?? "32,14";
        return (sm, lg);
    }

    private static string? TryGetPaddingFromButtonSize(ComputedStyle? style)
    {
        if (style == null)
            return null;

        // Bootstrap often expresses padding via CSS custom properties for buttons.
        var py = style.GetProperty("--bs-btn-padding-y");
        var px = style.GetProperty("--bs-btn-padding-x");
        if (!string.IsNullOrWhiteSpace(px) && !string.IsNullOrWhiteSpace(py))
        {
            var x = CssLengthToPixels(px);
            var y = CssLengthToPixels(py);
            if (x > 0 && y > 0)
                return $"{x.ToString(CultureInfo.InvariantCulture)},{y.ToString(CultureInfo.InvariantCulture)}";
        }

        var padding = style.GetProperty("padding");
        return TryParseCssPaddingToThickness(padding);
    }

    private static void AddButtonTypography(XElement style, XNamespace mauiNs, FlagstoneTokens tokens)
    {
        if (tokens.Typography.ContainsKey("FontSize.Button"))
        {
            style.Add(CreateSetter(mauiNs, "FontSize", "{DynamicResource FontSize.Button}"));
            return;
        }

        if (tokens.Typography.ContainsKey("FontSize.LabelLarge"))
        {
            style.Add(CreateSetter(mauiNs, "FontSize", "{DynamicResource FontSize.LabelLarge}"));
        }
        else if (tokens.Typography.ContainsKey("FontSize.Body"))
        {
            style.Add(CreateSetter(mauiNs, "FontSize", "{DynamicResource FontSize.Body}"));
        }
    }

    private static string GetPreferredButtonPadding(FlagstoneTokens tokens)
    {
        return tokens.Spacing.ContainsKey("Spacing.Button")
            ? "{DynamicResource Spacing.Button}"
            : (tokens.Spacing.ContainsKey("Spacing.Medium") ? "{DynamicResource Spacing.Medium}" : "24,10");
    }

    private static string GetPreferredBorderWidth(FlagstoneTokens tokens)
    {
        if (tokens.BorderWidth.ContainsKey("BorderWidth.Button"))
            return "{DynamicResource BorderWidth.Button}";
        if (tokens.BorderWidth.ContainsKey("BorderWidth.Default"))
            return "{DynamicResource BorderWidth.Default}";
        return "1";
    }

    private static string? GetPreferredRadiusKey(FlagstoneTokens tokens)
    {
        // For buttons, we need to use Radius.Button.* tokens (Int32) not Radius.* (Double)
        var preferredKeys = new[]
        {
            "Radius.Button.Medium",  // Try button-specific tokens first
            "Radius.Button.Small",
            "Radius.Button.Large",
            "Radius.Button.Default",
        };

        foreach (var key in preferredKeys)
        {
            // Check if a corresponding non-button radius exists and return button version
            var baseKey = key.Replace("Radius.Button.", "Radius.", StringComparison.Ordinal);
            if (tokens.BorderRadius.ContainsKey(baseKey))
                return key;
        }

        // Fallback: if we have any border radius, use the button version of the first one
        if (tokens.BorderRadius.Count > 0)
        {
            var firstKey = tokens.BorderRadius.Keys.First();
            return firstKey.Replace("Radius.", "Radius.Button.", StringComparison.Ordinal);
        }

        return null;
    }

    private static double CssLengthToPixels(string value)
    {
        value = value.Trim().ToLowerInvariant();
        if (value.EndsWith("px", StringComparison.Ordinal))
        {
            return double.TryParse(value.Replace("px", string.Empty, StringComparison.Ordinal), CultureInfo.InvariantCulture, out var px)
                ? px
                : 0;
        }
        if (value.EndsWith("rem", StringComparison.Ordinal))
        {
            return double.TryParse(value.Replace("rem", string.Empty, StringComparison.Ordinal), CultureInfo.InvariantCulture, out var rem)
                ? rem * 16.0
                : 0;
        }
        if (value.EndsWith("em", StringComparison.Ordinal))
        {
            return double.TryParse(value.Replace("em", string.Empty, StringComparison.Ordinal), CultureInfo.InvariantCulture, out var em)
                ? em * 16.0
                : 0;
        }
        return double.TryParse(value, CultureInfo.InvariantCulture, out var number) ? number : 0;
    }

    private static string? TryParseCssPaddingToThickness(string? cssPadding)
    {
        if (string.IsNullOrWhiteSpace(cssPadding))
            return null;

        // Expect "vertical horizontal" or "top right bottom left".
        var parts = cssPadding.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 1)
        {
            var v = CssLengthToPixels(parts[0]);
            return v > 0 ? v.ToString(CultureInfo.InvariantCulture) : null;
        }
        if (parts.Length == 2)
        {
            var y = CssLengthToPixels(parts[0]);
            var x = CssLengthToPixels(parts[1]);
            return (x > 0 && y > 0)
                ? $"{x.ToString(CultureInfo.InvariantCulture)},{y.ToString(CultureInfo.InvariantCulture)}"
                : null;
        }
        if (parts.Length == 4)
        {
            var top = CssLengthToPixels(parts[0]);
            var right = CssLengthToPixels(parts[1]);
            var bottom = CssLengthToPixels(parts[2]);
            var left = CssLengthToPixels(parts[3]);
            return (top > 0 && right > 0 && bottom > 0 && left > 0)
                ? $"{left.ToString(CultureInfo.InvariantCulture)},{top.ToString(CultureInfo.InvariantCulture)},{right.ToString(CultureInfo.InvariantCulture)},{bottom.ToString(CultureInfo.InvariantCulture)}"
                : null;
        }

        return null;
    }

    private void AddEntryStyles(XElement root, FlagstoneTokens tokens, BootstrapComponentStyles? componentStyles, ConversionOptions options)
    {
        var mauiNs = root.Name.Namespace;
        var xNs = root.GetNamespaceOfPrefix("x") ?? XNamespace.Get(XamlNamespace);

        root.Add(new XText("\n\n"));
        root.Add(new XComment(" ========== FsEntry Styles ========== "));
        root.Add(new XText("\n\n"));

        var baseStyle = new XElement(mauiNs + "Style",
            new XAttribute("TargetType", "fs:FsEntry")
        );

        // Background + border
        if (tokens.Colors.ContainsKey("Color.Background"))
        {
            baseStyle.Add(CreateSetter(mauiNs, "Background", "{DynamicResource Color.Background}"));
        }
        if (tokens.Colors.ContainsKey("Color.Outline"))
        {
            baseStyle.Add(CreateSetter(mauiNs, "BorderBrush", "{DynamicResource Color.Outline}"));
        }
        baseStyle.Add(CreateSetter(mauiNs, "BorderWidth", GetPreferredBorderWidth(tokens)));
        var radiusKey = GetPreferredRadiusKey(tokens);
        if (!string.IsNullOrWhiteSpace(radiusKey))
        {
            baseStyle.Add(CreateSetter(mauiNs, "CornerRadius", $"{{DynamicResource {radiusKey}}}"));
        }

        // Text
        if (tokens.Colors.ContainsKey("Color.OnBackground"))
        {
            baseStyle.Add(CreateSetter(mauiNs, "TextColor", "{DynamicResource Color.OnBackground}"));
            baseStyle.Add(CreateSetter(mauiNs, "PlaceholderColor", "{DynamicResource Color.OnBackground}"));
        }

        // Padding (prefer .form-control padding if available)
        var entryPadding = TryParseCssPaddingToThickness(componentStyles?.FormControl?.GetProperty("padding")) ?? "12,10";
        baseStyle.Add(CreateSetter(mauiNs, "Padding", entryPadding));

        // Typography
        if (tokens.Typography.ContainsKey("FontSize.Body"))
        {
            baseStyle.Add(CreateSetter(mauiNs, "FontSize", "{DynamicResource FontSize.Body}"));
        }

		// Focus/disabled states
		var focusedBorder = tokens.Colors.ContainsKey("Color.Primary")
			? "{DynamicResource Color.Primary}"
			: (tokens.Colors.ContainsKey("Color.Outline") ? "{DynamicResource Color.Outline}" : null);
		if (!string.IsNullOrWhiteSpace(focusedBorder))
		{
			AddInputVisualStates(baseStyle, mauiNs, xNs, focusedBorder);
		}

        root.Add(baseStyle);
        root.Add(new XText("\n\n"));

        // Validation styles (for MCT integrations)
        AddValidationStyle(root, mauiNs, xNs, tokens, targetType: "fs:FsEntry", key: "EntryValid", borderToken: "Color.Success");
        AddValidationStyle(root, mauiNs, xNs, tokens, targetType: "fs:FsEntry", key: "EntryInvalid", borderToken: "Color.Error");
    }

    private void AddEditorStyles(XElement root, FlagstoneTokens tokens, BootstrapComponentStyles? componentStyles, ConversionOptions options)
    {
        var mauiNs = root.Name.Namespace;
        var xNs = root.GetNamespaceOfPrefix("x") ?? XNamespace.Get(XamlNamespace);

        root.Add(new XText("\n\n"));
        root.Add(new XComment(" ========== FsEditor Styles ========== "));
        root.Add(new XText("\n\n"));

        var baseStyle = new XElement(mauiNs + "Style",
            new XAttribute("TargetType", "fs:FsEditor")
        );

        if (tokens.Colors.ContainsKey("Color.Background"))
        {
            baseStyle.Add(CreateSetter(mauiNs, "Background", "{DynamicResource Color.Background}"));
        }
        if (tokens.Colors.ContainsKey("Color.Outline"))
        {
            baseStyle.Add(CreateSetter(mauiNs, "BorderBrush", "{DynamicResource Color.Outline}"));
        }
        baseStyle.Add(CreateSetter(mauiNs, "BorderWidth", GetPreferredBorderWidth(tokens)));
        var radiusKey = GetPreferredRadiusKey(tokens);
        if (!string.IsNullOrWhiteSpace(radiusKey))
        {
            baseStyle.Add(CreateSetter(mauiNs, "CornerRadius", $"{{DynamicResource {radiusKey}}}"));
        }

        if (tokens.Colors.ContainsKey("Color.OnBackground"))
        {
            baseStyle.Add(CreateSetter(mauiNs, "TextColor", "{DynamicResource Color.OnBackground}"));
            baseStyle.Add(CreateSetter(mauiNs, "PlaceholderColor", "{DynamicResource Color.OnBackground}"));
        }

        var editorPadding = TryParseCssPaddingToThickness(componentStyles?.FormControl?.GetProperty("padding")) ?? "12,10";
        baseStyle.Add(CreateSetter(mauiNs, "Padding", editorPadding));

        if (tokens.Typography.ContainsKey("FontSize.Body"))
        {
            baseStyle.Add(CreateSetter(mauiNs, "FontSize", "{DynamicResource FontSize.Body}"));
        }

        // Focus/disabled states
        var focusedBorder = tokens.Colors.ContainsKey("Color.Primary")
            ? "{DynamicResource Color.Primary}"
            : (tokens.Colors.ContainsKey("Color.Outline") ? "{DynamicResource Color.Outline}" : null);
        if (!string.IsNullOrWhiteSpace(focusedBorder))
        {
            AddInputVisualStates(baseStyle, mauiNs, xNs, focusedBorder);
        }

        root.Add(baseStyle);
        root.Add(new XText("\n\n"));

        AddValidationStyle(root, mauiNs, xNs, tokens, targetType: "fs:FsEditor", key: "EditorValid", borderToken: "Color.Success");
        AddValidationStyle(root, mauiNs, xNs, tokens, targetType: "fs:FsEditor", key: "EditorInvalid", borderToken: "Color.Error");
    }

    private static void AddValidationStyle(XElement root, XNamespace mauiNs, XNamespace xNs, FlagstoneTokens tokens, string targetType, string key, string borderToken)
    {
        if (!tokens.Colors.ContainsKey(borderToken))
            return;

        var style = new XElement(mauiNs + "Style",
            new XAttribute(xNs + "Key", key),
            new XAttribute("TargetType", targetType)
        );
        var border = $"{{DynamicResource {borderToken}}}";
        style.Add(CreateSetter(mauiNs, "BorderBrush", border));
        // Keep the same border color when focused, and also include Disabled state.
        AddInputVisualStates(style, mauiNs, xNs, border);
        root.Add(style);
        root.Add(new XText("\n\n"));
    }

    private static void AddInputVisualStates(XElement style, XNamespace mauiNs, XNamespace xNs, string focusedBorderBrush)
    {
        var visualStateManager = new XElement(mauiNs + "Setter",
            new XAttribute("Property", "VisualStateManager.VisualStateGroups"),
            new XElement(mauiNs + "VisualStateGroupList",
                new XElement(mauiNs + "VisualStateGroup",
                    new XAttribute(xNs + "Name", "CommonStates"),
                    new XElement(mauiNs + "VisualState",
                        new XAttribute(xNs + "Name", "Normal")
                    ),
                    new XElement(mauiNs + "VisualState",
                        new XAttribute(xNs + "Name", "Focused"),
                        new XElement(mauiNs + "VisualState.Setters",
                            new XElement(mauiNs + "Setter",
                                new XAttribute("Property", "BorderBrush"),
                                new XAttribute("Value", focusedBorderBrush)
                            )
                        )
                    ),
                    new XElement(mauiNs + "VisualState",
                        new XAttribute(xNs + "Name", "Disabled"),
                        new XElement(mauiNs + "VisualState.Setters",
                            new XElement(mauiNs + "Setter",
                                new XAttribute("Property", "Opacity"),
                                new XAttribute("Value", "0.38")
                            )
                        )
                    )
                )
            )
        );

        style.Add(visualStateManager);
    }

    private void AddCardStyles(XElement root, FlagstoneTokens tokens, BootstrapComponentStyles? componentStyles, ConversionOptions options)
    {
        var mauiNs = root.Name.Namespace;
        root.Add(new XText("\n\n"));
        root.Add(new XComment(" ========== FsCard Styles ========== "));
        root.Add(new XText("\n\n"));

        var baseStyle = new XElement(mauiNs + "Style",
            new XAttribute("TargetType", "fs:FsCard")
        );

        // Bootstrap cards are typically surface containers with borders
        if (tokens.Colors.ContainsKey("Color.Background"))
        {
            baseStyle.Add(CreateSetter(mauiNs, "BackgroundColor", CreateColorResourceReference("Color.Background", tokens)));
        }
        if (tokens.Colors.ContainsKey("Color.Outline"))
        {
            baseStyle.Add(CreateSetter(mauiNs, "BorderColor", CreateColorResourceReference("Color.Outline", tokens)));
        }
        baseStyle.Add(CreateSetter(mauiNs, "BorderWidth", GetPreferredBorderWidth(tokens)));

        var radiusKey = GetPreferredRadiusKey(tokens);
        if (!string.IsNullOrWhiteSpace(radiusKey))
        {
            baseStyle.Add(CreateSetter(mauiNs, "CornerRadius", $"{{DynamicResource {radiusKey}}}"));
        }

        // Prefer card padding if available in CSS, otherwise a reasonable default
        var cardPadding = TryParseCssPaddingToThickness(componentStyles?.Card?.GetProperty("padding"))
            ?? (tokens.Spacing.ContainsKey("Spacing.Medium") ? "{DynamicResource Spacing.Medium}" : "16");
        baseStyle.Add(CreateSetter(mauiNs, "Padding", cardPadding));

        // Add shadow if available - prefer Shadow.Small, fallback to Shadow.Default or any available shadow
        if (tokens.Shadows.ContainsKey("Shadow.Small"))
        {
            baseStyle.Add(CreateSetter(mauiNs, "Shadow", "{DynamicResource Shadow.Small}"));
        }
        else if (tokens.Shadows.ContainsKey("Shadow.Default"))
        {
            baseStyle.Add(CreateSetter(mauiNs, "Shadow", "{DynamicResource Shadow.Default}"));
        }
        else if (tokens.Shadows.Count > 0)
        {
            // Use first available shadow token
            var firstShadowKey = tokens.Shadows.Keys.First();
            baseStyle.Add(CreateSetter(mauiNs, "Shadow", $"{{DynamicResource {firstShadowKey}}}"));
        }

        root.Add(baseStyle);
        root.Add(new XText("\n"));
    }

    private static XElement CreateSetter(XNamespace mauiNs, string property, string value)
    {
        return new XElement(mauiNs + "Setter",
            new XAttribute("Property", property),
            new XAttribute("Value", value)
        );
    }

    private static void AddButtonVisualStates(XElement style, XNamespace mauiNs, XNamespace xNs)
    {
        var visualStateManager = new XElement(mauiNs + "Setter",
            new XAttribute("Property", "VisualStateManager.VisualStateGroups"),
            new XElement(mauiNs + "VisualStateGroupList",
                new XElement(mauiNs + "VisualStateGroup",
                    new XAttribute(xNs + "Name", "CommonStates"),
                    new XElement(mauiNs + "VisualState",
                        new XAttribute(xNs + "Name", "Normal")
                    ),
                    new XElement(mauiNs + "VisualState",
                        new XAttribute(xNs + "Name", "Pressed"),
                        new XElement(mauiNs + "VisualState.Setters",
                            new XElement(mauiNs + "Setter",
                                new XAttribute("Property", "Opacity"),
                                new XAttribute("Value", "0.90")
                            )
                        )
                    ),
                    new XElement(mauiNs + "VisualState",
                        new XAttribute(xNs + "Name", "Disabled"),
                        new XElement(mauiNs + "VisualState.Setters",
                            new XElement(mauiNs + "Setter",
                                new XAttribute("Property", "Opacity"),
                                new XAttribute("Value", "0.38")
                            )
                        )
                    )
                )
            )
        );

        style.Add(visualStateManager);
    }

    private static string FormatXamlDocument(XDocument doc)
    {
        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "    ",
            OmitXmlDeclaration = false,
            Encoding = Encoding.UTF8,
            NewLineChars = "\n",
            NewLineHandling = NewLineHandling.Replace
        };

        using var stringWriter = new StringWriter();
        using (var xmlWriter = XmlWriter.Create(stringWriter, settings))
        {
            doc.Save(xmlWriter);
        }

        return stringWriter.ToString();
    }
}
