using CssComputer.Core.Models;
using AngleSharp.Html.Parser;
using ExCSS;
using System.Text.RegularExpressions;

namespace CssComputer.Core.Services;

/// <summary>
/// Stage 1: Resolves final computed styles via CSS cascade from HTML and CSS files.
/// Supports both traditional HTML sites and React/Next.js applications.
/// </summary>
public class StyleResolutionService
{
    private readonly HtmlParser _htmlParser;
    private readonly StylesheetParser _cssParser;
    
    // Regex patterns for JSX/TSX processing
    // Matches all common CSS import forms:
    // - import "./styles.css"
    // - import styles from "./styles.css"
    // - import * as styles from "./styles.css"
    // - import { button } from "./styles.css"
    // - import type { Theme } from "./styles.css"
    private static readonly Regex CssImportPattern = new(
        @"import\s+(?:[^""']+\s+from\s+)?[""']([^""']+\.css)[""'];?",
        RegexOptions.Compiled);
    private static readonly Regex ClassNamePattern = new(
        @"\bclassName\s*=\s*",
        RegexOptions.Compiled);
    private static readonly Regex JsxExpressionInAttributePattern = new(
        @"(\bclass\s*=\s*)\{[^}]*\}",
        RegexOptions.Compiled);
    private static readonly Regex JsxSelfClosingPattern = new(
        @"<([A-Z][\w]*)([^>]*)/>",
        RegexOptions.Compiled);
    private static readonly Regex JsxComponentPattern = new(
        @"<([A-Z][\w]*)([^>]*)>([\s\S]*?)</\1>",
        RegexOptions.Compiled);
    
    // Regex patterns for extracting component names from React/TSX files
    // Matches: export function ComponentName, export const ComponentName, function ComponentName
    private static readonly Regex ExportedComponentPattern = new(
        @"export\s+(?:default\s+)?(?:function|const)\s+([A-Z][a-zA-Z0-9]*)",
        RegexOptions.Compiled);
    private static readonly Regex FunctionComponentPattern = new(
        @"(?:function|const)\s+([A-Z][a-zA-Z0-9]*)\s*[=:(]",
        RegexOptions.Compiled);
    
    // Regex patterns for CSS preprocessing (stripping unsupported @layer directives)
    // Matches: @layer name; (empty layer declaration)
    private static readonly Regex LayerDeclarationPattern = new(
        @"@layer\s+[\w\-]+\s*;",
        RegexOptions.Compiled);
    // Constant for @layer keyword comparison
    private static readonly ReadOnlyMemory<char> LayerKeyword = "@layer".AsMemory();

    public StyleResolutionService()
    {
        _htmlParser = new HtmlParser();
        _cssParser = new StylesheetParser();
    }

    /// <summary>
    /// Resolve styles for all elements in the input via CSS cascade.
    /// </summary>
    public async Task<List<ResolvedElement>> ResolveStylesAsync(string inputPath, ComputationOptions options)
    {
        var elements = new List<ResolvedElement>();
        
        // Collect CSS rules from either the separate CSS source path or the input path
        // If no CssSourcePath is provided and inputPath is a file, use its directory as the base
        var cssSourcePath = !string.IsNullOrEmpty(options.CssSourcePath) 
            ? options.CssSourcePath 
            : (File.Exists(inputPath) ? Path.GetDirectoryName(Path.GetFullPath(inputPath)) ?? "." : inputPath);
        var sharedCssRules = await CollectGlobalCssRulesAsync(cssSourcePath);

        if (File.Exists(inputPath))
        {
            elements.AddRange(await ResolveFileStylesAsync(inputPath, sharedCssRules));
        }
        else if (Directory.Exists(inputPath))
        {
            elements.AddRange(await ResolveDirectoryStylesAsync(inputPath, sharedCssRules));
        }

        return elements;
    }

    private async Task<List<ResolvedElement>> ResolveFileStylesAsync(string filePath, List<CssRule>? sharedCssRules = null)
    {
        var elements = new List<ResolvedElement>();
        var content = await File.ReadAllTextAsync(filePath);
        var directory = Path.GetDirectoryName(filePath) ?? "";
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        var isReactFile = extension == ".jsx" || extension == ".tsx";
        
        // Extract component name from React/TSX files
        var componentName = isReactFile ? ExtractComponentName(content, filePath) : "";

        // Preprocess JSX/TSX to make it parseable as HTML
        var preprocessedContent = content;
        if (isReactFile)
        {
            preprocessedContent = PreprocessJsxContent(content);
        }

        // Parse HTML/JSX content
        var document = await _htmlParser.ParseDocumentAsync(preprocessedContent);

        // Collect all CSS rules from external stylesheets and inline styles
        var cssRules = sharedCssRules != null ? new List<CssRule>(sharedCssRules) : new List<CssRule>();
        
        // Find and parse external CSS files from <link> tags (traditional HTML)
        var stylesheetHrefs = document.QuerySelectorAll("link[rel='stylesheet']")
            .Select(link => link.GetAttribute("href"))
            .Where(href => !string.IsNullOrEmpty(href))
            .Select(href => href!); // Non-null assertion after filtering
        
        var stylesheetRules = await Task.WhenAll(stylesheetHrefs
            .Select(href => Path.Combine(directory, href))
            .Where(File.Exists)
            .Select(async cssPath =>
        {
            var cssContent = await File.ReadAllTextAsync(cssPath);
            cssContent = PreprocessCssForParsing(cssContent);
            var stylesheet = _cssParser.Parse(cssContent);
            return GetStyleRules(stylesheet);
        }));
        cssRules.AddRange(stylesheetRules.SelectMany(rules => rules));

        // For React/JSX/TSX files, also look for CSS imports in the source
        // Use the original content (not preprocessed) to find imports
        if (isReactFile)
        {
            var importRules = await Task.WhenAll(ExtractCssImports(content, directory)
                .Where(File.Exists)
                .Select(async cssPath =>
            {
                var cssContent = await File.ReadAllTextAsync(cssPath);
                cssContent = PreprocessCssForParsing(cssContent);
                var stylesheet = _cssParser.Parse(cssContent);
                return GetStyleRules(stylesheet);
            }));
            cssRules.AddRange(importRules.SelectMany(rules => rules));
        }

        // Parse <style> tags
        cssRules.AddRange(
            document.QuerySelectorAll("style")
                .Select(styleElement => styleElement.TextContent)
                .Select(cssContent => PreprocessCssForParsing(cssContent))
                .Select(preprocessedCss => _cssParser.Parse(preprocessedCss))
                .SelectMany(stylesheet => GetStyleRules(stylesheet))
        );

        // Get all elements - try Body first, fall back to all elements if Body is null/empty
        // This handles JSX/TSX files which don't have a proper HTML structure
        var elementsToProcess = document.Body?.QuerySelectorAll("*")?.ToList() ?? new List<AngleSharp.Dom.IElement>();
        
        // If no body elements found, try getting all elements from the document
        if (elementsToProcess.Count == 0)
        {
            elementsToProcess = document.QuerySelectorAll("*").ToList();
        }

        // Process each visual element
        int elementIndex = 0;
        foreach (var element in elementsToProcess.Where(e => !IsNonVisualElement(e.TagName)))
        {

            // Compute styles via CSS cascade
            var computedStyles = ComputeStylesForElement(element, cssRules);

            // Add inline styles (highest specificity)
            var inlineStyle = element.GetAttribute("style");
            if (!string.IsNullOrEmpty(inlineStyle))
            {
                var inlineDeclarations = ParseInlineStyle(inlineStyle);
                foreach (var (property, value) in inlineDeclarations)
                {
                    computedStyles[property] = value;
                }
            }

            // Get class names from the element
            var classes = element.ClassName ?? element.GetAttribute("class") ?? "";

            // Include element only if it has computed styles
            // Note: For Tailwind/utility-class frameworks, users must run the CSS build
            // step first to generate compiled CSS that this tool can process
            if (computedStyles.Count > 0)
            {
                // Try to get per-element component from data-component attribute (set by JSX preprocessing)
                var elementComponent = element.GetAttribute("data-component");
                
                // If no per-element component, fall back to file-level component name
                var effectiveComponent = !string.IsNullOrEmpty(elementComponent) 
                    ? elementComponent 
                    : componentName;
                
                // Create a meaningful element ID
                // If we have a component name, use it; otherwise use filename
                var prefix = !string.IsNullOrEmpty(effectiveComponent) 
                    ? effectiveComponent 
                    : Path.GetFileNameWithoutExtension(filePath);
                
                elements.Add(new ResolvedElement
                {
                    ElementId = $"{prefix}:{element.TagName.ToLower()}:{elementIndex}",
                    Properties = computedStyles,
                    SourceInfo = new Dictionary<string, string>
                    {
                        ["file"] = filePath,
                        ["filename"] = Path.GetFileName(filePath),
                        ["tag"] = element.TagName.ToLower(),
                        ["index"] = elementIndex.ToString(),
                        ["id"] = element.Id ?? "",
                        ["classes"] = classes,
                        ["component"] = effectiveComponent ?? "",  // Per-element component from data-component or file-level fallback
                        ["fileComponent"] = componentName ?? ""    // Store file-level component separately
                    }
                });
            }

            elementIndex++;
        }

        return elements;
    }
    
    /// <summary>
    /// Extract the React component name from a JSX/TSX file.
    /// Looks for exported function or const components.
    /// </summary>
    private string ExtractComponentName(string content, string filePath)
    {
        // First, try to find exported components (preferred)
        var exportMatch = ExportedComponentPattern.Match(content);
        if (exportMatch.Success)
        {
            return exportMatch.Groups[1].Value;
        }
        
        // Fall back to any PascalCase function/const
        var functionMatch = FunctionComponentPattern.Match(content);
        if (functionMatch.Success)
        {
            return functionMatch.Groups[1].Value;
        }
        
        // Last resort: derive from filename if it looks like a component
        var filename = Path.GetFileNameWithoutExtension(filePath);
        if (filename.Length > 0 && (char.IsUpper(filename[0]) || filename.Contains('-')))
        {
            // Convert kebab-case to PascalCase if needed
            return string.Join("", filename.Split('-')
                .Where(part => part.Length > 0)
                .Select(part => char.ToUpper(part[0]) + part[1..]));
        }
        
        return "";
    }

    /// <summary>
    /// Directories to exclude from source file scanning.
    /// These contain build artifacts, not source code.
    /// </summary>
    private static readonly HashSet<string> ExcludedDirectories = new(StringComparer.OrdinalIgnoreCase)
    {
        ".next",
        "node_modules",
        "dist",
        "build",
        "out",
        ".git",
        ".vercel",
        "coverage",
        "__pycache__",
        "bin",
        "obj"
    };

    /// <summary>
    /// Check if a file path is within an excluded directory.
    /// </summary>
    private bool IsInExcludedDirectory(string filePath, string rootPath)
    {
        var relativePath = Path.GetRelativePath(rootPath, filePath);
        var parts = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return parts.Any(part => ExcludedDirectories.Contains(part));
    }

    private async Task<List<ResolvedElement>> ResolveDirectoryStylesAsync(string directoryPath, List<CssRule> sharedCssRules)
    {
        var elements = new List<ResolvedElement>();
        
        // Process HTML files (output from Tool 1 --emit-html)
        var htmlFiles = Directory.GetFiles(directoryPath, "*.html", SearchOption.AllDirectories)
            .Where(f => !IsInExcludedDirectory(f, directoryPath))
            .ToList();

        foreach (var file in htmlFiles)
        {
            var fileElements = await ResolveFileStylesAsync(file, sharedCssRules);
            elements.AddRange(fileElements);
        }
        
        // If no HTML files found, fall back to processing JSX/TSX source files
        if (elements.Count == 0)
        {
            var jsxFiles = Directory.GetFiles(directoryPath, "*.jsx", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(directoryPath, "*.tsx", SearchOption.AllDirectories))
                .Where(f => !IsInExcludedDirectory(f, directoryPath));

            foreach (var file in jsxFiles)
            {
                elements.AddRange(await ResolveFileStylesAsync(file, sharedCssRules));
            }
        }

        return elements;
    }

    /// <summary>
    /// Collect global CSS rules from common Next.js/React locations.
    /// </summary>
    private async Task<List<CssRule>> CollectGlobalCssRulesAsync(string directoryPath)
    {
        var cssRules = new List<CssRule>();
        
        // PRIORITY 1: Look for compiled/built CSS output first
        // This is the authoritative source for Tailwind and other utility-class frameworks
        var compiledCssPaths = new[]
        {
            // Next.js production build output (various versions)
            ".next/static/css",
            ".next/static/chunks",      // Next.js with Turbopack
            ".next/static/media",       // Some Next.js versions
            // Next.js development build output (Turbopack dev server)
            ".next/dev/static/chunks",  // Dev mode compiled CSS
            ".next/dev/static/css",     // Dev mode CSS directory
            // Vite build output
            "dist/assets",
            // Generic build outputs
            "dist/css",
            "build/static/css",
            "out/_next/static/css",
            "out/_next/static/chunks",
            // Direct compiled output from Tailwind CLI
            "dist",
            "build"
        };

        foreach (var fullPath in compiledCssPaths.Select(relativePath => Path.Combine(directoryPath, relativePath)).Where(Directory.Exists))
        {
            var cssFiles = Directory.GetFiles(fullPath, "*.css", SearchOption.AllDirectories);
            var fileRules = await Task.WhenAll(cssFiles.Select(async cssFile =>
            {
                var cssContent = await File.ReadAllTextAsync(cssFile);
                // Preprocess CSS to remove @layer directives (not supported by ExCSS)
                cssContent = PreprocessCssForParsing(cssContent);
                var stylesheet = _cssParser.Parse(cssContent);
                return GetStyleRules(stylesheet);
            }));
            cssRules.AddRange(fileRules.SelectMany(rules => rules));
        }

        // If we found compiled CSS, return it - it's the authoritative source
        if (cssRules.Count > 0)
        {
            return cssRules;
        }

        // PRIORITY 2: Fall back to source CSS files (for non-Tailwind projects or pre-build analysis)
        // Common global CSS file locations in Next.js/React projects
        var globalCssPaths = new[]
        {
            "app/globals.css",
            "styles/globals.css",
            "src/app/globals.css",
            "src/styles/globals.css",
            "styles/global.css",
            "src/styles/global.css",
            "app/global.css",
            "src/app/global.css",
            "styles/index.css",
            "src/index.css",
            "index.css"
        };

        var globalFileRules = await Task.WhenAll(
            globalCssPaths
                .Select(relativePath => Path.Combine(directoryPath, relativePath))
                .Where(File.Exists)
                .Select(async fullPath =>
                {
                    var cssContent = await File.ReadAllTextAsync(fullPath);
                    cssContent = PreprocessCssForParsing(cssContent);
                    var stylesheet = _cssParser.Parse(cssContent);
                    return GetStyleRules(stylesheet);
                }));
        cssRules.AddRange(globalFileRules.SelectMany(rules => rules));

        // Also find any CSS files directly referenced in layout files
        var layoutFiles = new[]
        {
            "app/layout.tsx",
            "app/layout.jsx",
            "src/app/layout.tsx",
            "src/app/layout.jsx",
            "pages/_app.tsx",
            "pages/_app.jsx",
            "src/pages/_app.tsx",
            "src/pages/_app.jsx"
        };

        var processedCssPaths = new HashSet<string>();
        var fullPaths = layoutFiles.Select(relativePath => Path.Combine(directoryPath, relativePath)).ToList();
        foreach (var fullPath in fullPaths.Where(File.Exists))
        {
            var content = await File.ReadAllTextAsync(fullPath);
            var layoutDir = Path.GetDirectoryName(fullPath) ?? directoryPath;
            var cssImports = ExtractCssImports(content, layoutDir);
            foreach (var cssPath in cssImports.Where(path => !processedCssPaths.Contains(path) && File.Exists(path)))
            {
                processedCssPaths.Add(cssPath);
                var cssContent = await File.ReadAllTextAsync(cssPath);
                cssContent = PreprocessCssForParsing(cssContent);
                var stylesheet = _cssParser.Parse(cssContent);
                cssRules.AddRange(GetStyleRules(stylesheet));
            }
        }

        return cssRules;
    }

    /// <summary>
    /// Preprocess JSX/TSX content to make it parseable by an HTML parser.
    /// </summary>
    private string PreprocessJsxContent(string content)
    {
        // Step 1: Try to extract just the JSX from the return statement
        // This avoids feeding imports, type definitions, etc. to the HTML parser
        var jsxContent = ExtractJsxFromReturn(content);
        if (string.IsNullOrEmpty(jsxContent))
        {
            // Fallback to the whole content if we can't find a return statement
            jsxContent = content;
        }
        
        // Convert className to class for HTML parsing
        jsxContent = ClassNamePattern.Replace(jsxContent, "class=");
        
        // Replace JSX expressions in class attributes with placeholder
        // e.g., class={cn("...", condition && "...")} -> class="jsx-expression"
        jsxContent = JsxExpressionInAttributePattern.Replace(jsxContent, "$1\"jsx-dynamic-class\"");
        
        // Convert self-closing JSX components to divs for parsing
        // e.g., <Button /> -> <div data-component="Button" />
        jsxContent = JsxSelfClosingPattern.Replace(jsxContent, m =>
        {
            var componentName = m.Groups[1].Value;
            var attributes = m.Groups[2].Value;
            // Only convert PascalCase components (React components)
            if (componentName.Length > 0 && char.IsUpper(componentName[0]))
            {
                return $"<div data-component=\"{componentName}\"{attributes}></div>";
            }
            return m.Value;
        });
        
        // Convert JSX components with children to divs
        // e.g., <Button>Click</Button> -> <div data-component="Button">Click</div>
        jsxContent = JsxComponentPattern.Replace(jsxContent, m =>
        {
            var componentName = m.Groups[1].Value;
            var attributes = m.Groups[2].Value;
            var children = m.Groups[3].Value;
            // Only convert PascalCase components (React components)
            if (componentName.Length > 0 && char.IsUpper(componentName[0]))
            {
                return $"<div data-component=\"{componentName}\"{attributes}>{children}</div>";
            }
            return m.Value;
        });
        
        // Remove JSX expressions that would break HTML parsing
        // e.g., {someValue} -> (removed or replaced with placeholder text)
        jsxContent = Regex.Replace(jsxContent, @"\{[^{}]*\}", " ");
        
        // Wrap in HTML structure so parser has something to work with
        return $"<!DOCTYPE html><html><body>{jsxContent}</body></html>";
    }
    
    /// <summary>
    /// Extract the JSX content from a React component's return statement.
    /// </summary>
    private string ExtractJsxFromReturn(string content)
    {
        // Try to find return ( ... ) pattern
        // This handles: return ( <div>...</div> )
        var returnMatch = Regex.Match(content, @"return\s*\(\s*(<[\s\S]*?>[\s\S]*</[^>]+>|<[^>]+\s*/>)\s*\)", RegexOptions.Singleline);
        if (returnMatch.Success)
        {
            return returnMatch.Groups[1].Value;
        }
        
        // Try: return <div>...</div> (without parentheses)
        returnMatch = Regex.Match(content, @"return\s+(<[\s\S]*?>[\s\S]*</[^>]+>|<[^>]+\s*/>)", RegexOptions.Singleline);
        if (returnMatch.Success)
        {
            return returnMatch.Groups[1].Value;
        }
        
        // Try to find JSX fragments: return ( <> ... </> ) or return ( <Fragment> ... </Fragment> )
        returnMatch = Regex.Match(content, @"return\s*\(\s*<(?:>|Fragment[^>]*>)([\s\S]*?)</(?:>|Fragment)>\s*\)", RegexOptions.Singleline);
        if (returnMatch.Success)
        {
            return returnMatch.Groups[1].Value;
        }
        
        return string.Empty;
    }

    /// <summary>
    /// Extract CSS file paths from import statements in JSX/TSX files.
    /// </summary>
    private List<string> ExtractCssImports(string content, string directory)
    {
        var matches = CssImportPattern.Matches(content);
        
        // Use explicit LINQ mapping to extract and resolve CSS import paths
        var cssFiles = matches.Cast<Match>()
            .Where(m => m.Groups[1].Success)
            .Select(match => match.Groups[1].Value)
            .Where(importPath => !string.IsNullOrEmpty(importPath))
            .Select(importPath => ResolveImportPath(importPath, directory))
            .Where(cssPath => !string.IsNullOrEmpty(cssPath))
            .ToList();

        // Also check for require statements
        var requirePattern = new Regex("require\\([\"']([^\"']+\\.css)[\"']\\)");
        var requireMatches = requirePattern.Matches(content);
        cssFiles.AddRange(
            requireMatches.Cast<Match>()
                .Where(match => match.Groups[1].Success && !string.IsNullOrEmpty(match.Groups[1].Value))
                .Select(match => ResolveImportPath(match.Groups[1].Value, directory))
                .Where(cssPath => !string.IsNullOrEmpty(cssPath))
        );

        return cssFiles;
    }

    /// <summary>
    /// Resolve an import path to an absolute file path.
    /// </summary>
    private string ResolveImportPath(string importPath, string currentDirectory)
    {
        // Handle relative paths (./ or ../)
        if (importPath.StartsWith("./") || importPath.StartsWith("../"))
        {
            return Path.GetFullPath(Path.Combine(currentDirectory, importPath));
        }
        
        // Handle paths starting with @ (common Next.js alias for src/)
        if (importPath.StartsWith("@/"))
        {
            // Try common src locations
            var relativePath = importPath[2..]; // Remove @/
            var possiblePaths = new[]
            {
                Path.Combine(currentDirectory, "..", relativePath),
                Path.Combine(currentDirectory, "..", "..", relativePath),
                Path.Combine(currentDirectory, "..", "..", "src", relativePath),
            };
            
            var existingPath = possiblePaths
                .Select(path => Path.GetFullPath(path))
                .FirstOrDefault(File.Exists);
            
            if (existingPath != null)
            {
                return existingPath;
            }
        }
        
        // Try as relative path from current directory
        var directPath = Path.Combine(currentDirectory, importPath);
        if (File.Exists(directPath))
        {
            return Path.GetFullPath(directPath);
        }
        
        return string.Empty;
    }

    /// <summary>
    /// Preprocesses CSS to remove @layer directives which are not supported by ExCSS.
    /// This strips @layer wrappers while preserving the rules inside.
    /// </summary>
    private string PreprocessCssForParsing(string cssContent, int depth = 0)
    {
        // Prevent infinite recursion with depth limit
        const int MaxDepth = 10;
        if (depth >= MaxDepth)
        {
            return cssContent;
        }
        
        // Remove empty @layer declarations like: @layer components;
        cssContent = LayerDeclarationPattern.Replace(cssContent, "");
        
        // Handle @layer blocks by removing the @layer wrapper but keeping content
        // Use a simple state machine to track brace depth
        var result = new System.Text.StringBuilder(cssContent.Length);
        var i = 0;
        
        while (i < cssContent.Length)
        {
            // Look for @layer followed by identifier and opening brace
            if (i + 6 <= cssContent.Length && cssContent.AsSpan(i, 6).SequenceEqual(LayerKeyword.Span))
            {
                // Skip @layer
                var layerStart = i;
                i += 6;
                
                // Skip whitespace and layer name
                while (i < cssContent.Length && (char.IsWhiteSpace(cssContent[i]) || char.IsLetterOrDigit(cssContent[i]) || cssContent[i] == '-' || cssContent[i] == '_'))
                {
                    i++;
                }
                
                // If we hit an opening brace, extract the content inside
                if (i < cssContent.Length && cssContent[i] == '{')
                {
                    i++; // Skip opening brace
                    var braceCount = 1;
                    var contentStart = i;
                    
                    while (i < cssContent.Length && braceCount > 0)
                    {
                        if (cssContent[i] == '{') braceCount++;
                        else if (cssContent[i] == '}') braceCount--;
                        i++;
                    }
                    
                    // Extract content inside (excluding the final closing brace)
                    if (braceCount == 0)
                    {
                        var contentLength = i - contentStart - 1;
                        if (contentLength > 0)
                        {
                            var content = cssContent.Substring(contentStart, contentLength);
                            // Recursively preprocess in case of nested @layer with depth tracking
                            result.Append(PreprocessCssForParsing(content, depth + 1));
                        }
                    }
                    continue;
                }
                else
                {
                    // Not a block, might be @layer name; - already handled by regex above
                    // Reset and continue normally
                    if (i > layerStart)
                    {
                        result.Append(cssContent.Substring(layerStart, i - layerStart));
                    }
                }
            }
            else
            {
                result.Append(cssContent[i]);
                i++;
            }
        }
        
        return result.ToString();
    }

    private List<CssRule> GetStyleRules(Stylesheet stylesheet)
    {
        var rules = new List<CssRule>();
        
        // ExCSS stores rules in StyleRules property - top level only
        // We need to recursively extract rules from @layer, @supports, @media blocks
        ExtractRulesRecursively(stylesheet.Children, rules);
        
        return rules;
    }

    private void ExtractRulesRecursively(IEnumerable<IStylesheetNode> nodes, List<CssRule> rules)
    {
        foreach (var node in nodes)
        {
            if (node is IStyleRule styleRule)
            {
                // Split comma-separated selectors and create separate rules for each
                // This ensures specificity is calculated correctly for each individual selector
                var selectors = styleRule.SelectorText.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                
                foreach (var selector in selectors)
                {
                    rules.Add(new CssRule
                    {
                        Selector = selector,
                        Specificity = CalculateSpecificity(selector),
                        Declarations = styleRule.Style.ToDictionary(
                            d => d.Name,
                            d => d.Value
                        )
                    });
                }
            }
            else if (node is IGroupingRule groupingRule)
            {
                // Recursively process @media, @supports, @layer, etc.
                // The rules inside will use parent conditions, but for DLS extraction
                // we care about the final computed values, not the conditions
                ExtractRulesRecursively(groupingRule.Rules, rules);
            }
            else if (node is IRule rule && rule.Children != null)
            {
                // Other rules that might have children
                ExtractRulesRecursively(rule.Children, rules);
            }
        }
    }

    private Dictionary<string, string> ComputeStylesForElement(AngleSharp.Dom.IElement element, List<CssRule> cssRules)
    {
        var computedStyles = new Dictionary<string, string>();

        // Find all matching rules and sort by specificity, then by source order
        var matchingRules = cssRules
            .Select((rule, index) => new { Rule = rule, Index = index })
            .Where(r => ElementMatchesSelector(element, r.Rule.Selector))
            .OrderBy(r => r.Rule.Specificity)
            .ThenBy(r => r.Index) // Source order for equal specificity
            .Select(r => r.Rule)
            .ToList();

        // Apply rules in order of specificity and source order (cascade)
        foreach (var rule in matchingRules)
        {
            foreach (var (property, value) in rule.Declarations)
            {
                computedStyles[property] = value;
            }
        }

        return computedStyles;
    }

    private bool ElementMatchesSelector(AngleSharp.Dom.IElement element, string selector)
    {
        try
        {
            // Use AngleSharp's built-in selector matching
            return element.Matches(selector);
        }
        catch
        {
            // If selector parsing fails, skip this rule
            return false;
        }
    }

    private int CalculateSpecificity(string selector)
    {
        // Simplified but more robust specificity calculation
        // Real specificity: (a, b, c, d) where:
        // a = inline styles (not applicable here, always 0)
        // b = ID selectors
        // c = class selectors, attribute selectors, pseudo-classes
        // d = element selectors, pseudo-elements
        //
        // We return as integer: b*10000 + c*100 + d for easy comparison

        int ids = 0;
        int classes = 0;
        int elements = 0;

        // Simple heuristic approach (not perfect but reasonable for prototype)
        // Count occurrences of ID selectors (#)
        var idMatches = System.Text.RegularExpressions.Regex.Matches(selector, @"#[\w-]+");
        ids = idMatches.Count;

        // Count class selectors (.)
        var classMatches = System.Text.RegularExpressions.Regex.Matches(selector, @"\.[\w-]+");
        classes = classMatches.Count;

        // Count attribute selectors ([])
        var attrMatches = System.Text.RegularExpressions.Regex.Matches(selector, @"\[[^\]]+\]");
        classes += attrMatches.Count;

        // Count pseudo-classes (:) but not pseudo-elements (::)
        var pseudoClassMatches = System.Text.RegularExpressions.Regex.Matches(selector, @":(?!:)[\w-]+");
        classes += pseudoClassMatches.Count;

        // Count element selectors (simplified - words not preceded by # or .)
        var elementMatches = System.Text.RegularExpressions.Regex.Matches(selector, @"(?<![#\.])(?:^|\s|>|\+|~)([a-z][\w-]*)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        elements = elementMatches.Count;

        // Count pseudo-elements (::)
        var pseudoElementMatches = System.Text.RegularExpressions.Regex.Matches(selector, @"::[\w-]+");
        elements += pseudoElementMatches.Count;

        // Return combined specificity as integer for easy sorting
        return (ids * 10000) + (classes * 100) + elements;
    }

    private Dictionary<string, string> ParseInlineStyle(string styleAttribute)
    {
        var declarations = styleAttribute.Split(';', StringSplitOptions.RemoveEmptyEntries);

        return declarations
            .Select(declaration => declaration.Split(':', 2))
            .Where(parts => parts.Length == 2)
            .ToDictionary(
                parts => parts[0].Trim(),
                parts => parts[1].Trim());
    }

    private bool IsNonVisualElement(string tagName)
    {
        // Elements that don't participate in visual rendering
        var nonVisual = new[] { "SCRIPT", "STYLE", "META", "HEAD", "TITLE", "LINK", "NOSCRIPT" };
        return nonVisual.Contains(tagName.ToUpper());
    }

    private class CssRule
    {
        public required string Selector { get; set; }
        public required int Specificity { get; set; }
        public required Dictionary<string, string> Declarations { get; set; }
    }
}
