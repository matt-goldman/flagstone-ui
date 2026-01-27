using Acornima;
using Acornima.Ast;
using Acornima.Jsx;
using Acornima.Jsx.Ast;
using System.Linq;
using System.Text;
using System.Net;

namespace ReactComponentFlattener.Core.Services;

/// <summary>
/// Service that converts JSX/TSX code to HTML with embedded component metadata.
/// This produces clean HTML that can be consumed by downstream pipeline tools (e.g., Tool 3).
/// </summary>
public class HtmlEmitterService
{
    private readonly JsxParserOptions _parserOptions;

    public HtmlEmitterService()
    {
        _parserOptions = new JsxParserOptions
        {
            EcmaVersion = EcmaVersion.Latest,
            AllowReturnOutsideFunction = true,
            Tolerant = true
        };
    }

    /// <summary>
    /// Convert JSX/TSX code to HTML with component metadata preserved as data attributes.
    /// </summary>
    /// <param name="jsxCode">The flattened JSX/TSX code</param>
    /// <param name="sourceFilePath">Original source file path for metadata</param>
    /// <param name="componentName">The main component name (if known)</param>
    /// <returns>HTML string with data-component attributes</returns>
    public string ConvertToHtml(string jsxCode, string sourceFilePath, string? componentName = null)
    {
        try
        {
            // Strip TypeScript types before parsing
            var strippedCode = TypeScriptHelper.StripTypeScriptTypes(jsxCode);
            
            var parser = new JsxParser(_parserOptions);
            var ast = parser.ParseModule(strippedCode);

            // Find the component's return statement JSX
            var jsxExtractor = new JsxExtractor();
            jsxExtractor.Visit(ast);

            // Check for JSX element or fragment
            if (jsxExtractor.MainJsx == null && jsxExtractor.MainFragment == null)
            {
                // No JSX found - return empty
                return "";
            }

            // Convert JSX to HTML
            var htmlBuilder = new StringBuilder();
            var converter = new JsxToHtmlConverter(sourceFilePath, componentName ?? jsxExtractor.ComponentName);
            
            // Prefer MainJsx, but use MainFragment if that's what the component returns
            var html = jsxExtractor.MainJsx != null 
                ? converter.Convert(jsxExtractor.MainJsx)
                : converter.Convert(jsxExtractor.MainFragment!);

            // Wrap in basic HTML structure
            htmlBuilder.AppendLine("<!DOCTYPE html>");
            htmlBuilder.AppendLine("<html>");
            htmlBuilder.AppendLine("<head>");
            htmlBuilder.AppendLine($"  <meta charset=\"UTF-8\">");
            htmlBuilder.AppendLine($"  <meta name=\"source-file\" content=\"{WebUtility.HtmlEncode(sourceFilePath)}\">");
            if (!string.IsNullOrEmpty(componentName ?? jsxExtractor.ComponentName))
            {
                htmlBuilder.AppendLine($"  <meta name=\"component\" content=\"{WebUtility.HtmlEncode(componentName ?? jsxExtractor.ComponentName ?? "")}\">");
            }
            htmlBuilder.AppendLine("</head>");
            htmlBuilder.AppendLine("<body>");
            htmlBuilder.AppendLine(html);
            htmlBuilder.AppendLine("</body>");
            htmlBuilder.AppendLine("</html>");

            return htmlBuilder.ToString();
        }
        catch (Exception ex)
        {
            // On parse error, return a comment with the error
            return $"<!-- HTML conversion failed: {WebUtility.HtmlEncode(ex.Message)} -->";
        }
    }

    /// <summary>
    /// Visitor that extracts the main JSX element from a component
    /// </summary>
    private class JsxExtractor : JsxAstVisitor
    {
        public JsxElement? MainJsx { get; private set; }
        public JsxFragment? MainFragment { get; private set; }
        public string? ComponentName { get; private set; }

        protected override object? VisitFunctionDeclaration(FunctionDeclaration node)
        {
            // Check if this looks like a React component (PascalCase name)
            if (node.Id?.Name != null && node.Id.Name.Length > 0 && char.IsUpper(node.Id.Name[0]))
            {
                ComponentName ??= node.Id.Name;
            }
            return base.VisitFunctionDeclaration(node);
        }

        protected override object? VisitVariableDeclarator(VariableDeclarator node)
        {
            // Check for const Component = () => ... pattern
            if (node.Id is Identifier id && id.Name.Length > 0 && char.IsUpper(id.Name[0]))
            {
                ComponentName ??= id.Name;
            }
            return base.VisitVariableDeclarator(node);
        }

        protected override object? VisitReturnStatement(ReturnStatement node)
        {
            // Capture the JSX from return statements
            if (node.Argument is JsxElement jsx)
            {
                MainJsx ??= jsx;
            }
            else if (node.Argument is JsxFragment fragment)
            {
                MainFragment ??= fragment;
            }
            else if (node.Argument is ParenthesizedExpression paren)
            {
                if (paren.Expression is JsxElement jsxInner)
                {
                    MainJsx ??= jsxInner;
                }
                else if (paren.Expression is JsxFragment fragmentInner)
                {
                    MainFragment ??= fragmentInner;
                }
            }
            return base.VisitReturnStatement(node);
        }
    }

    /// <summary>
    /// Converts JSX AST nodes to HTML strings
    /// </summary>
    private class JsxToHtmlConverter
    {
        private readonly string _sourceFile;
        private readonly string? _componentName;
        private int _indentLevel = 0;

        public JsxToHtmlConverter(string sourceFile, string? componentName)
        {
            _sourceFile = sourceFile;
            _componentName = componentName;
        }

        public string Convert(Node node)
        {
            return node switch
            {
                JsxElement element => ConvertElement(element),
                JsxFragment fragment => ConvertFragment(fragment),
                JsxText text => ConvertText(text),
                JsxExpressionContainer expr => ConvertExpression(expr),
                _ => ""
            };
        }

        private string ConvertElement(JsxElement element)
        {
            var sb = new StringBuilder();
            var indent = new string(' ', _indentLevel * 2);

            // Get tag name
            var tagName = GetTagName(element.OpeningElement);
            var isComponent = tagName.Length > 0 && char.IsUpper(tagName[0]);

            // Convert React components to div with data-component
            var htmlTag = isComponent ? "div" : tagName;
            
            sb.Append($"{indent}<{htmlTag}");

            // Add data-component for React components
            if (isComponent)
            {
                sb.Append($" data-component=\"{WebUtility.HtmlEncode(tagName)}\"");
            }

            // Add data-source for the root element
            if (_indentLevel == 0 && !string.IsNullOrEmpty(_sourceFile))
            {
                sb.Append($" data-source=\"{WebUtility.HtmlEncode(Path.GetFileName(_sourceFile))}\"");
            }

            // Convert attributes
            foreach (var attrHtml in element.OpeningElement.Attributes
                .Select(ConvertAttribute)
                .Where(html => !string.IsNullOrEmpty(html)))
            {
                sb.Append($" {attrHtml}");
            }

            // Self-closing check
            if (element.OpeningElement.SelfClosing || element.Children.Count == 0)
            {
                // Use self-closing for void elements, otherwise close normally
                if (IsVoidElement(htmlTag))
                {
                    sb.AppendLine(" />");
                }
                else
                {
                    sb.AppendLine($"></{htmlTag}>");
                }
                return sb.ToString();
            }

            sb.AppendLine(">");

            // Convert children
            _indentLevel++;
            var childrenHtml = element.Children
                .Select(child => Convert(child))
                .Where(childHtml => !string.IsNullOrWhiteSpace(childHtml));
            
            foreach (var childHtml in childrenHtml)
            {
                sb.Append(childHtml);
            }
            _indentLevel--;

            sb.AppendLine($"{indent}</{htmlTag}>");
            return sb.ToString();
        }

        private string ConvertFragment(JsxFragment fragment)
        {
            var sb = new StringBuilder();
            foreach (var child in fragment.Children)
            {
                sb.Append(Convert(child));
            }
            return sb.ToString();
        }

        private string ConvertText(JsxText text)
        {
            var trimmed = text.Value?.Trim();
            if (string.IsNullOrEmpty(trimmed))
            {
                return "";
            }
            var indent = new string(' ', _indentLevel * 2);
            return $"{indent}{WebUtility.HtmlEncode(trimmed)}\n";
        }

        private string ConvertExpression(JsxExpressionContainer expr)
        {
            // For expressions like {variable} or {condition && <Component />}
            // We'll output a placeholder or try to extract nested JSX
            if (expr.Expression is JsxElement nestedJsx)
            {
                return Convert(nestedJsx);
            }
            
            // For other expressions, output a data-bound placeholder
            var indent = new string(' ', _indentLevel * 2);
            return $"{indent}<span data-expression=\"dynamic\">{{...}}</span>\n";
        }

        private string GetTagName(JsxOpeningElement opening)
        {
            return opening.Name switch
            {
                JsxIdentifier id => id.Name,
                JsxMemberExpression member => GetMemberExpressionName(member),
                JsxNamespacedName ns => $"{ns.Namespace.Name}:{ns.Name.Name}",
                _ => "div"
            };
        }

        private string GetMemberExpressionName(JsxMemberExpression member)
        {
            // Handle things like Card.Header -> Card-Header
            var obj = member.Object switch
            {
                JsxIdentifier id => id.Name,
                JsxMemberExpression nested => GetMemberExpressionName(nested),
                _ => "Unknown"
            };
            return $"{obj}-{member.Property.Name}";
        }

        private string ConvertAttribute(Node attr)
        {
            if (attr is JsxAttribute jsxAttr)
            {
                var name = jsxAttr.Name switch
                {
                    JsxIdentifier id => id.Name,
                    JsxNamespacedName ns => $"{ns.Namespace.Name}:{ns.Name.Name}",
                    _ => null
                };

                if (name == null) return "";

                // Convert React-specific attributes to HTML equivalents
                var htmlName = name switch
                {
                    "className" => "class",
                    "htmlFor" => "for",
                    "tabIndex" => "tabindex",
                    "readOnly" => "readonly",
                    "autoFocus" => "autofocus",
                    "autoComplete" => "autocomplete",
                    _ => name
                };

                // Skip event handlers and React-specific props
                if (htmlName.StartsWith("on") && htmlName.Length > 2 && char.IsUpper(htmlName[2]))
                {
                    return ""; // Skip onClick, onChange, etc.
                }
                if (htmlName == "ref" || htmlName == "key" || htmlName == "dangerouslySetInnerHTML")
                {
                    return "";
                }

                // Handle value
                if (jsxAttr.Value == null)
                {
                    // Boolean attribute
                    return htmlName;
                }

                var value = jsxAttr.Value switch
                {
                    StringLiteral str => str.Value,
                    JsxExpressionContainer container => GetExpressionValue(container),
                    _ => null
                };

                if (value != null)
                {
                    return $"{htmlName}=\"{WebUtility.HtmlEncode(value)}\"";
                }

                return "";
            }

            if (attr is JsxSpreadAttribute)
            {
                // Can't represent spread in HTML, skip it
                return "";
            }

            return "";
        }

        private string GetExpressionValue(JsxExpressionContainer container)
        {
            return container.Expression switch
            {
                StringLiteral str => str.Value,
                NumericLiteral num => num.Value.ToString(System.Globalization.CultureInfo.InvariantCulture),
                BooleanLiteral boolean => boolean.Value.ToString().ToLower(),
                TemplateLiteral template => GetTemplateLiteralValue(template),
                // For complex expressions, use a placeholder
                _ => "{{dynamic}}"
            };
        }

        private string GetTemplateLiteralValue(TemplateLiteral template)
        {
            // For template literals like `class1 ${condition ? 'class2' : ''}`
            // Just return the quasi parts concatenated
            var parts = template.Quasis
                .Where(quasi => !string.IsNullOrEmpty(quasi.Value.Cooked))
                .Select(quasi => quasi.Value.Cooked)
                .ToList();
            return string.Join(" ", parts).Trim();
        }

        private bool IsVoidElement(string tag)
        {
            return tag.ToLower() switch
            {
                "area" or "base" or "br" or "col" or "embed" or "hr" or 
                "img" or "input" or "link" or "meta" or "source" or 
                "track" or "wbr" => true,
                _ => false
            };
        }
    }
}
