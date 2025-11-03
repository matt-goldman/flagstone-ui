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

        var doc = CreateXamlDocument();
        var root = doc.Root!;

        var mauiNs = root.Name.Namespace;

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

        // Create output directory if it doesn't exist
        Directory.CreateDirectory(outputDirectory);

        // Generate Tokens.xaml
        var tokensXaml = GenerateTokensXaml(tokens, options);
        var tokensPath = Path.Combine(outputDirectory, "Tokens.xaml");
        await File.WriteAllTextAsync(tokensPath, tokensXaml);

        // Generate Theme.xaml
        var themeXaml = GenerateThemeXaml(tokens, themeName, options);
        var themePath = Path.Combine(outputDirectory, "Theme.xaml");
        await File.WriteAllTextAsync(themePath, themeXaml);
    }

    private static XDocument CreateXamlDocument()
    {
        var mauiNs = XNamespace.Get(MauiNamespace);
        var xNs = XNamespace.Get(XamlNamespace);

        var doc = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement(mauiNs + "ResourceDictionary",
                new XAttribute(XNamespace.Xmlns + "x", xNs.NamespaceName)
            )
        );

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
