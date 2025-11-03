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
    private const string MauiNamespace = "http://schemas.microsoft.com/dotnet/2021/maui";
    private const string XamlNamespace = "http://schemas.microsoft.com/winfx/2009/xaml";

    /// <summary>
    /// Sanitizes a theme name to create a valid C# identifier
    /// </summary>
    private static string SanitizeThemeName(string themeName)
    {
        if (string.IsNullOrWhiteSpace(themeName))
            return "Theme";

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
            result = "_" + result;

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
    public async Task GenerateFilesAsync(FlagstoneTokens tokens, string themeName, string outputDirectory, ConversionOptions? options = null)
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
        var stylesXaml = GenerateStylesXaml(tokens, themeName, options);
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

            var colorElement = new XElement(mauiNs + "Color",
                new XAttribute(xNs + "Key", token.Key),
                token.Value
            );

            root.Add(colorElement);

            // Add dark mode value as comment if available
            if (options.IncludeComments && !string.IsNullOrWhiteSpace(token.DarkValue))
            {
                root.Add(new XComment($" Dark mode: {token.DarkValue} "));
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

        foreach (var (_, token) in tokens.OrderBy(kvp => kvp.Key))
        {
            // Add purpose as comment if available
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
        options ??= new ConversionOptions();

        // Sanitize theme name for class name
        var sanitizedThemeName = SanitizeThemeName(themeName);
        var doc = CreateXamlDocument(withClass: true, className: $"{options.Namespace}.{sanitizedThemeName}Styles");
        var root = doc.Root!;

        var mauiNs = root.Name.Namespace;
        var xNs = root.GetNamespaceOfPrefix("x") ?? XNamespace.Get(XamlNamespace);

        // Add Flagstone namespace for controls
        root.Add(new XAttribute(XNamespace.Xmlns + "fs", "clr-namespace:FlagstoneUI.Core.Controls;assembly=FlagstoneUI.Core"));

        // Add comment header
        root.Add(new XComment($" {themeName} Styles - Generated from Bootstrap "));
        root.Add(new XComment(" Control styles that use theme tokens "));
        root.Add(new XText("\n\n"));

        // Add button styles
        AddButtonStyles(root, tokens, options);

        return FormatXamlDocument(doc);
    }

    private void AddButtonStyles(XElement root, FlagstoneTokens tokens, ConversionOptions options)
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
        defaultStyle.Add(CreateSetter(mauiNs, "BackgroundColor", "{DynamicResource Color.Primary}"));
        
        // Try to find OnPrimary color, fallback to white
        var textColor = tokens.Colors.ContainsKey("Color.OnPrimary") 
            ? "{DynamicResource Color.OnPrimary}" 
            : "#FFFFFF";
        defaultStyle.Add(CreateSetter(mauiNs, "TextColor", textColor));

        // Corner radius
        if (tokens.BorderRadius.ContainsKey("Radius.Large"))
        {
            defaultStyle.Add(CreateSetter(mauiNs, "CornerRadius", "{DynamicResource Radius.Large}"));
        }

        // Padding - use spacing if available
        var padding = tokens.Spacing.ContainsKey("Spacing.Medium") 
            ? "{DynamicResource Spacing.Medium}" 
            : "24,10";
        defaultStyle.Add(CreateSetter(mauiNs, "Padding", padding));

        // Font size if available
        if (tokens.Typography.ContainsKey("FontSize.Body"))
        {
            defaultStyle.Add(CreateSetter(mauiNs, "FontSize", "{DynamicResource FontSize.Body}"));
        }

        defaultStyle.Add(CreateSetter(mauiNs, "HeightRequest", "40"));

        // Add disabled visual state
        AddDisabledVisualState(defaultStyle, mauiNs, xNs);

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
        outlinedStyle.Add(CreateSetter(mauiNs, "TextColor", "{DynamicResource Color.Primary}"));
        
        // Border
        var borderColor = tokens.Colors.ContainsKey("Color.Outline") 
            ? "{DynamicResource Color.Outline}" 
            : "{DynamicResource Color.Primary}";
        outlinedStyle.Add(CreateSetter(mauiNs, "BorderColor", borderColor));
        
        if (tokens.BorderWidth.ContainsKey("BorderWidth.Default"))
        {
            outlinedStyle.Add(CreateSetter(mauiNs, "BorderWidth", "{DynamicResource BorderWidth.Default}"));
        }
        else
        {
            outlinedStyle.Add(CreateSetter(mauiNs, "BorderWidth", "1"));
        }

        if (tokens.BorderRadius.ContainsKey("Radius.Large"))
        {
            outlinedStyle.Add(CreateSetter(mauiNs, "CornerRadius", "{DynamicResource Radius.Large}"));
        }

        outlinedStyle.Add(CreateSetter(mauiNs, "Padding", padding));
        
        if (tokens.Typography.ContainsKey("FontSize.Body"))
        {
            outlinedStyle.Add(CreateSetter(mauiNs, "FontSize", "{DynamicResource FontSize.Body}"));
        }

        outlinedStyle.Add(CreateSetter(mauiNs, "HeightRequest", "40"));
        AddDisabledVisualState(outlinedStyle, mauiNs, xNs);

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

        if (tokens.BorderRadius.ContainsKey("Radius.Large"))
        {
            textButtonStyle.Add(CreateSetter(mauiNs, "CornerRadius", "{DynamicResource Radius.Large}"));
        }

        textButtonStyle.Add(CreateSetter(mauiNs, "Padding", "12,10"));
        
        if (tokens.Typography.ContainsKey("FontSize.Body"))
        {
            textButtonStyle.Add(CreateSetter(mauiNs, "FontSize", "{DynamicResource FontSize.Body}"));
        }

        textButtonStyle.Add(CreateSetter(mauiNs, "HeightRequest", "40"));
        AddDisabledVisualState(textButtonStyle, mauiNs, xNs);

        root.Add(textButtonStyle);
        root.Add(new XText("\n"));
    }

    private static XElement CreateSetter(XNamespace mauiNs, string property, string value)
    {
        return new XElement(mauiNs + "Setter",
            new XAttribute("Property", property),
            new XAttribute("Value", value)
        );
    }

    private static void AddDisabledVisualState(XElement style, XNamespace mauiNs, XNamespace xNs)
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
