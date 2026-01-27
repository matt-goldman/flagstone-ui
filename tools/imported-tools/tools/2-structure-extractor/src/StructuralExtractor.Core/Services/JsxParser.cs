using System.Text.RegularExpressions;
using StructuralExtractor.Core.Models;

namespace StructuralExtractor.Core.Services;

/// <summary>
/// Parses JSX/TSX content to extract structural elements.
/// This is a simplified parser for MVP - focuses on basic structure extraction.
/// </summary>
public partial class JsxParser
{
    [GeneratedRegex(@"<(\w+)([^>]*)>(.*?)</\1>", RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex ElementWithClosingTagRegex();
    
    [GeneratedRegex(@"<(\w+)([^>]*)/>", RegexOptions.Compiled)]
    private static partial Regex SelfClosingElementRegex();
    
    [GeneratedRegex(@"(\w+)=\{([^}]+)\}|(\w+)=""([^""]+)""|(\w+)='([^']+)'|(\w+)", RegexOptions.Compiled)]
    private static partial Regex AttributeRegex();

    /// <summary>
    /// Extracts the JSX return statement from a component function.
    /// </summary>
    public string? ExtractJsxFromFunction(string content, string functionName)
    {
        // Look for function definition - more flexible patterns
        var patterns = new[]
        {
            $@"export\s+default\s+function\s+{functionName}\s*\([^)]*\)",
            $@"function\s+{functionName}\s*\([^)]*\)",
            $@"const\s+{functionName}\s*=\s*\([^)]*\)\s*(?::\s*[^=>]+)?\s*=>",
            $@"export\s+const\s+{functionName}\s*=\s*\([^)]*\)\s*(?::\s*[^=>]+)?\s*=>",
        };

        int functionStart = -1;
        foreach (var pattern in patterns)
        {
            var match = Regex.Match(content, pattern, RegexOptions.Singleline);
            if (match.Success)
            {
                functionStart = match.Index + match.Length;
                break;
            }
        }
        
        if (functionStart == -1)
            return null;

        // Look for return statement
        var returnMatch = Regex.Match(content.Substring(functionStart), @"return\s*\(?", RegexOptions.Singleline);
        if (!returnMatch.Success)
            return null;

        var jsxStart = functionStart + returnMatch.Index + returnMatch.Length;
        
        // Skip opening parenthesis if present
        while (jsxStart < content.Length && (content[jsxStart] == '(' || char.IsWhiteSpace(content[jsxStart])))
            jsxStart++;
        
        if (jsxStart >= content.Length || content[jsxStart] != '<')
            return null;
        
        // Extract JSX with balanced tags
        var jsx = ExtractBalancedJsx(content, jsxStart);
        return string.IsNullOrEmpty(jsx) ? null : jsx;
    }

    /// <summary>
    /// Extracts JSX with balanced opening/closing tags.
    /// </summary>
    private string ExtractBalancedJsx(string content, int startIndex)
    {
        if (startIndex >= content.Length || content[startIndex] != '<')
            return string.Empty;

        var depth = 0;
        var inTag = false;
        var inString = false;
        char stringChar = '\0';
        var i = startIndex;

        while (i < content.Length)
        {
            var c = content[i];

            // Handle strings
            if ((c == '"' || c == '\'' || c == '`') && (i == 0 || content[i - 1] != '\\'))
            {
                if (!inString)
                {
                    inString = true;
                    stringChar = c;
                }
                else if (c == stringChar)
                {
                    inString = false;
                }
            }

            if (!inString)
            {
                if (c == '<')
                {
                    // Check if it's a closing tag
                    if (i + 1 < content.Length && content[i + 1] == '/')
                    {
                        depth--;
                        if (depth == 0)
                        {
                            // Find the end of this closing tag
                            var endTag = content.IndexOf('>', i);
                            if (endTag != -1)
                            {
                                return content.Substring(startIndex, endTag - startIndex + 1);
                            }
                        }
                    }
                    else
                    {
                        // Opening tag or self-closing
                        depth++;
                        inTag = true;
                    }
                }
                else if (c == '>' && inTag)
                {
                    inTag = false;
                    // Check if it's self-closing
                    if (i > 0 && content[i - 1] == '/')
                    {
                        depth--;
                        if (depth == 0)
                        {
                            return content.Substring(startIndex, i - startIndex + 1);
                        }
                    }
                }
            }

            i++;
        }

        return string.Empty;
    }

    /// <summary>
    /// Parses JSX content into a structural element tree.
    /// Improved implementation with better nesting support.
    /// </summary>
    public StructuralElement? ParseJsx(string jsx)
    {
        if (string.IsNullOrWhiteSpace(jsx))
            return null;

        jsx = jsx.Trim();
        
        // Handle fragments
        if (jsx.StartsWith("<>") && jsx.EndsWith("</>"))
        {
            var fragmentContent = jsx.Substring(2, jsx.Length - 5);
            return new StructuralElement
            {
                Type = "Fragment",
                Children = ParseMultipleElements(fragmentContent)
            };
        }

        // Find the first tag
        var tagMatch = Regex.Match(jsx, @"^<(\w+)([^>]*)(/?)>");
        if (!tagMatch.Success)
            return null;

        var tagName = tagMatch.Groups[1].Value;
        var attributes = tagMatch.Groups[2].Value;
        var isSelfClosing = tagMatch.Groups[3].Value == "/";

        var element = new StructuralElement
        {
            Type = tagName,
            Props = ParseAttributes(attributes)
        };

        // Determine if this is a component reference (starts with uppercase)
        if (!string.IsNullOrEmpty(tagName) && char.IsUpper(tagName[0]))
        {
            element.Ref = $"#/components/{tagName}";
        }

        if (isSelfClosing)
        {
            return element;
        }

        // Find the matching closing tag
        var openTag = $"<{tagName}";
        var closeTag = $"</{tagName}>";
        
        var content = ExtractContentBetweenTags(jsx, tagName, tagMatch.Index + tagMatch.Length);
        
        if (!string.IsNullOrWhiteSpace(content))
        {
            // Check if content contains nested elements
            if (content.TrimStart().StartsWith('<'))
            {
                element.Children = ParseMultipleElements(content);
            }
            else if (!string.IsNullOrWhiteSpace(content) && !content.Contains('{'))
            {
                // Plain text content (skip JSX expressions for now)
                element.Text = content.Trim();
            }
        }

        return element;
    }

    /// <summary>
    /// Extracts content between opening and closing tags.
    /// </summary>
    private string ExtractContentBetweenTags(string jsx, string tagName, int contentStart)
    {
        var closeTag = $"</{tagName}>";
        var depth = 1;
        var i = contentStart;
        
        var openTagPattern = $"<{tagName}[\\s>]";
        
        while (i < jsx.Length && depth > 0)
        {
            if (jsx.Substring(i).StartsWith(closeTag))
            {
                depth--;
                if (depth == 0)
                {
                    return jsx.Substring(contentStart, i - contentStart);
                }
                i += closeTag.Length;
            }
            else if (Regex.IsMatch(jsx.Substring(i), openTagPattern))
            {
                depth++;
                i++;
            }
            else
            {
                i++;
            }
        }
        
        return string.Empty;
    }

    /// <summary>
    /// Parses multiple JSX elements from content.
    /// </summary>
    private List<StructuralElement>? ParseMultipleElements(string content)
    {
        var children = new List<StructuralElement>();
        content = content.Trim();
        
        var i = 0;
        while (i < content.Length)
        {
            // Skip whitespace and newlines
            while (i < content.Length && char.IsWhiteSpace(content[i]))
                i++;
                
            if (i >= content.Length)
                break;
            
            // Skip JSX expressions
            if (content[i] == '{')
            {
                var depth = 1;
                i++;
                while (i < content.Length && depth > 0)
                {
                    if (content[i] == '{') depth++;
                    else if (content[i] == '}') depth--;
                    i++;
                }
                continue;
            }
            
            // Look for element start
            if (content[i] == '<')
            {
                // Extract this element
                var elementJsx = ExtractBalancedJsx(content, i);
                if (!string.IsNullOrEmpty(elementJsx))
                {
                    var child = ParseJsx(elementJsx);
                    if (child != null)
                    {
                        children.Add(child);
                    }
                    i += elementJsx.Length;
                }
                else
                {
                    i++;
                }
            }
            else
            {
                // Plain text between elements - skip for now
                i++;
            }
        }

        return children.Count > 0 ? children : null;
    }

    /// <summary>
    /// Parses HTML/JSX attributes into a dictionary.
    /// </summary>
    private Dictionary<string, object>? ParseAttributes(string attributes)
    {
        if (string.IsNullOrWhiteSpace(attributes))
            return null;

        var props = new Dictionary<string, object>();
        var matches = AttributeRegex().Matches(attributes);

        foreach (Match match in matches)
        {
            string? key = null;
            string? value = null;

            // {expression} syntax
            if (!string.IsNullOrEmpty(match.Groups[1].Value))
            {
                key = match.Groups[1].Value;
                value = match.Groups[2].Value;
            }
            // Double quotes
            else if (!string.IsNullOrEmpty(match.Groups[3].Value))
            {
                key = match.Groups[3].Value;
                value = match.Groups[4].Value;
            }
            // Single quotes
            else if (!string.IsNullOrEmpty(match.Groups[5].Value))
            {
                key = match.Groups[5].Value;
                value = match.Groups[6].Value;
            }
            // Boolean attribute
            else if (!string.IsNullOrEmpty(match.Groups[7].Value))
            {
                key = match.Groups[7].Value;
                value = "true";
            }

            if (!string.IsNullOrEmpty(key))
            {
                props[key] = value ?? "true";
            }
        }

        return props.Count > 0 ? props : null;
    }

    /// <summary>
    /// Identifies if an element represents a layout component.
    /// </summary>
    public bool IsLayoutElement(string? type)
    {
        if (string.IsNullOrEmpty(type))
            return false;
            
        var layoutTypes = new[] { "div", "section", "main", "header", "footer", "nav", "article", "aside" };
        return layoutTypes.Contains(type.ToLowerInvariant());
    }

    /// <summary>
    /// Identifies if an element represents a control/interactive component.
    /// </summary>
    public bool IsControlElement(string? type)
    {
        if (string.IsNullOrEmpty(type))
            return false;
            
        var controlTypes = new[] { "button", "input", "select", "textarea", "a", "Link" };
        return controlTypes.Contains(type);
    }
}
