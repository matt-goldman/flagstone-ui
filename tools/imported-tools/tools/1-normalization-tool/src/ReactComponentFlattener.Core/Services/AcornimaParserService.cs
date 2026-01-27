using Acornima;
using Acornima.Ast;
using Acornima.Jsx;
using Acornima.Jsx.Ast;
using ReactComponentFlattener.Core.Models;

namespace ReactComponentFlattener.Core.Services;

/// <summary>
/// Exception thrown when JSX/TSX parsing fails
/// </summary>
public class ParserException : Exception
{
    public ParserException(string message) : base(message) { }
    public ParserException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Parser service implementation using Acornima library for JSX/TSX parsing.
/// Replaces the Node.js-based parser with a pure .NET solution.
/// </summary>
public class AcornimaParserService
{
    private readonly JsxParserOptions _parserOptions;

    public AcornimaParserService()
    {
        _parserOptions = new JsxParserOptions
        {
            EcmaVersion = EcmaVersion.Latest,
            AllowReturnOutsideFunction = true,
            Tolerant = true // Be lenient with parsing errors
        };
    }

    public Task<FileAnalysis> AnalyzeFileAsync(string code)
    {
        try
        {
            // Strip TypeScript types before parsing
            var strippedCode = TypeScriptHelper.StripTypeScriptTypes(code);
            
            var parser = new JsxParser(_parserOptions);
            var ast = parser.ParseModule(strippedCode);

            var visitor = new AnalysisVisitor();
            visitor.Visit(ast);

            var analysis = new FileAnalysis
            {
                Components = visitor.Components,
                Imports = visitor.Imports
            };

            return Task.FromResult(analysis);
        }
        catch (Exception ex)
        {
            throw new ParserException($"Parser error: {ex.Message}", ex);
        }
    }

    public Task<string> FlattenComponentsAsync(string code, List<string> componentsToFlatten)
    {
        if (componentsToFlatten == null || componentsToFlatten.Count == 0)
        {
            return Task.FromResult(code);
        }

        try
        {
            // Strip TypeScript types before parsing
            var strippedCode = TypeScriptHelper.StripTypeScriptTypes(code);
            
            // Parse the code to AST
            var parser = new JsxParser(_parserOptions);
            var ast = parser.ParseModule(strippedCode);

            // First pass: collect component definitions
            var componentCollector = new ComponentDefinitionCollector(componentsToFlatten);
            componentCollector.Visit(ast);

            // Second pass: transform the AST (inline usages and remove definitions)
            var transformer = new ComponentFlattenerTransformer(componentCollector.ComponentDefinitions);
            var transformedAst = (Module)transformer.Visit(ast)!;

            // Generate code from transformed AST with formatting
            var formattingOptions = new KnRJavaScriptTextFormatterOptions
            {
                Indent = "  "
            };
            var generatedCode = transformedAst.ToJsx(formattingOptions);
            
            return Task.FromResult(generatedCode);
        }
        catch (Exception ex)
        {
            throw new ParserException($"Flattening error: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Visitor that analyzes the AST to extract component and import information
    /// </summary>
    private class AnalysisVisitor : JsxAstVisitor
    {
        public List<ComponentInfo> Components { get; } = new();
        public List<ImportInfo> Imports { get; } = new();
        private HashSet<string> _processedComponentNames = new();
        private List<string> _defaultExportNames = new();

        public override object? Visit(Node? node)
        {
            if (node == null) return null;
            
            var result = base.Visit(node);
            
            // After visiting the entire tree, mark default exports
            if (node is Module)
            {
                foreach (var exportName in _defaultExportNames)
                {
                    var component = Components.FirstOrDefault(c => c.Name == exportName);
                    if (component != null)
                    {
                        component.IsExported = true;
                        component.IsDefaultExport = true;
                    }
                }
            }
            
            return result;
        }

        private bool IsComponentAlreadyProcessed(string? componentName)
        {
            return componentName != null && _processedComponentNames.Contains(componentName);
        }

        private void MarkComponentAsProcessed(string? componentName)
        {
            if (componentName != null)
            {
                _processedComponentNames.Add(componentName);
            }
        }

        private string? GetComponentName(VariableDeclarator declarator)
        {
            return declarator.Id is Identifier id ? id.Name : null;
        }

        protected override object? VisitImportDeclaration(ImportDeclaration importDeclaration)
        {
            if (importDeclaration.Source is StringLiteral sourceLiteral)
            {
                var import = new ImportInfo
                {
                    Source = sourceLiteral.Value
                };

                foreach (var specifier in importDeclaration.Specifiers)
                {
                    var spec = new Models.ImportSpecifier
                    {
                        Type = specifier.Type.ToString(),
                        Local = specifier.Local.Name
                    };

                    if (specifier is Acornima.Ast.ImportSpecifier impSpec && impSpec.Imported is Identifier importedId)
                    {
                        spec.Imported = importedId.Name;
                    }

                    import.Specifiers.Add(spec);
                }

                Imports.Add(import);
            }

            return base.VisitImportDeclaration(importDeclaration);
        }

        protected override object? VisitFunctionDeclaration(FunctionDeclaration functionDeclaration)
        {
            if (!IsComponentAlreadyProcessed(functionDeclaration.Id?.Name) && IsReactComponent(functionDeclaration))
            {
                var component = AnalyzeComponent(functionDeclaration, "FunctionDeclaration");
                Components.Add(component);
                MarkComponentAsProcessed(functionDeclaration.Id?.Name);
            }

            return base.VisitFunctionDeclaration(functionDeclaration);
        }

        protected override object? VisitVariableDeclaration(VariableDeclaration variableDeclaration)
        {
            foreach (var declarator in variableDeclaration.Declarations)
            {
                if (!IsComponentAlreadyProcessed(GetComponentName(declarator)) && 
                    (declarator.Init is ArrowFunctionExpression || declarator.Init is FunctionExpression))
                {
                    if (IsReactComponent(declarator.Init))
                    {
                        var component = AnalyzeVariableComponent(declarator);
                        Components.Add(component);
                        MarkComponentAsProcessed(GetComponentName(declarator));
                    }
                }
            }

            return base.VisitVariableDeclaration(variableDeclaration);
        }

        protected override object? VisitExportDefaultDeclaration(ExportDefaultDeclaration exportDefaultDeclaration)
        {
            if (exportDefaultDeclaration.Declaration is FunctionDeclaration funcDecl)
            {
                if (!IsComponentAlreadyProcessed(funcDecl.Id?.Name) && IsReactComponent(funcDecl))
                {
                    var component = AnalyzeComponent(funcDecl, "FunctionDeclaration");
                    component.IsExported = true;
                    component.IsDefaultExport = true;
                    Components.Add(component);
                    MarkComponentAsProcessed(funcDecl.Id?.Name);
                }
            }
            else if (exportDefaultDeclaration.Declaration is Identifier identifier)
            {
                // Track the name for later marking as default export
                _defaultExportNames.Add(identifier.Name);
            }

            return base.VisitExportDefaultDeclaration(exportDefaultDeclaration);
        }

        protected override object? VisitExportNamedDeclaration(ExportNamedDeclaration exportNamedDeclaration)
        {
            if (exportNamedDeclaration.Declaration is FunctionDeclaration funcDecl)
            {
                if (!IsComponentAlreadyProcessed(funcDecl.Id?.Name) && IsReactComponent(funcDecl))
                {
                    var component = AnalyzeComponent(funcDecl, "FunctionDeclaration");
                    component.IsExported = true;
                    Components.Add(component);
                    MarkComponentAsProcessed(funcDecl.Id?.Name);
                }
            }
            else if (exportNamedDeclaration.Declaration is VariableDeclaration varDecl)
            {
                foreach (var declarator in varDecl.Declarations)
                {
                    if (!IsComponentAlreadyProcessed(GetComponentName(declarator)) &&
                        (declarator.Init is ArrowFunctionExpression || declarator.Init is FunctionExpression))
                    {
                        if (IsReactComponent(declarator.Init))
                        {
                            var component = AnalyzeVariableComponent(declarator);
                            component.IsExported = true;
                            Components.Add(component);
                            MarkComponentAsProcessed(GetComponentName(declarator));
                        }
                    }
                }
            }

            return base.VisitExportNamedDeclaration(exportNamedDeclaration);
        }

        private bool IsReactComponent(Node node)
        {
            // Check if the node returns JSX
            var jsxChecker = new JsxReturnChecker();
            jsxChecker.Visit(node);
            return jsxChecker.HasJsxReturn;
        }

        private ComponentInfo AnalyzeComponent(FunctionDeclaration funcDecl, string type)
        {
            var component = new ComponentInfo
            {
                Name = funcDecl.Id?.Name ?? "Anonymous",
                Type = type,
                Loc = ConvertLocation(funcDecl.Location)
            };

            AnalyzeComponentBody(funcDecl.Params, funcDecl.Body, component);

            return component;
        }

        private ComponentInfo AnalyzeVariableComponent(VariableDeclarator declarator)
        {
            var component = new ComponentInfo
            {
                Name = declarator.Id is Identifier id ? id.Name : "Anonymous",
                Type = "VariableDeclarator",
                Loc = ConvertLocation(declarator.Location)
            };

            if (declarator.Init is ArrowFunctionExpression arrowFunc)
            {
                AnalyzeComponentBody(arrowFunc.Params, arrowFunc.Body, component);
            }
            else if (declarator.Init is FunctionExpression funcExpr)
            {
                AnalyzeComponentBody(funcExpr.Params, funcExpr.Body, component);
            }

            return component;
        }

        private void AnalyzeComponentBody(NodeList<Node> parameters, StatementOrExpression body, ComponentInfo component)
        {
            // Extract parameters
            foreach (var param in parameters)
            {
                if (param is Identifier id)
                {
                    component.Params.Add(id.Name);
                }
                else if (param is ObjectPattern objPattern)
                {
                    foreach (var prop in objPattern.Properties)
                    {
                        if (prop is AssignmentProperty assignProp && assignProp.Key is Identifier propId)
                        {
                            component.Params.Add(propId.Name);
                            if (propId.Name == "children")
                            {
                                component.HasChildren = true;
                            }
                        }
                    }
                }
            }

            // Analyze body for hooks and used components
            var bodyAnalyzer = new ComponentBodyAnalyzer();
            bodyAnalyzer.Visit(body);

            component.UsesHooks = bodyAnalyzer.UsesHooks;
            component.UsedComponents = bodyAnalyzer.UsedComponents.ToList();
        }

        private Models.Location? ConvertLocation(SourceLocation location)
        {
            return new Models.Location
            {
                Start = new Models.Position
                {
                    Line = location.Start.Line,
                    Column = location.Start.Column
                },
                End = new Models.Position
                {
                    Line = location.End.Line,
                    Column = location.End.Column
                }
            };
        }
    }

    /// <summary>
    /// Checks if a function/component returns JSX
    /// </summary>
    private class JsxReturnChecker : JsxAstVisitor
    {
        public bool HasJsxReturn { get; private set; }

        protected override object? VisitArrowFunctionExpression(ArrowFunctionExpression arrowFunctionExpression)
        {
            // Check for implicit JSX return
            if (arrowFunctionExpression.Body is JsxElement || arrowFunctionExpression.Body is JsxFragment)
            {
                HasJsxReturn = true;
            }
            return base.VisitArrowFunctionExpression(arrowFunctionExpression);
        }

        protected override object? VisitReturnStatement(ReturnStatement returnStatement)
        {
            if (returnStatement.Argument is JsxElement || returnStatement.Argument is JsxFragment)
            {
                HasJsxReturn = true;
            }
            return base.VisitReturnStatement(returnStatement);
        }
    }

    /// <summary>
    /// Analyzes component body for hooks usage and component references
    /// </summary>
    private class ComponentBodyAnalyzer : JsxAstVisitor
    {
        public bool UsesHooks { get; private set; }
        public HashSet<string> UsedComponents { get; } = new();

        protected override object? VisitCallExpression(CallExpression callExpression)
        {
            // Check for React hooks (functions starting with "use")
            string? hookName = null;

            if (callExpression.Callee is Identifier calleeId)
            {
                hookName = calleeId.Name;
            }
            else if (callExpression.Callee is MemberExpression memberExpr &&
                     memberExpr.Property is Identifier propId)
            {
                hookName = propId.Name;
            }

            if (hookName != null && hookName.StartsWith("use"))
            {
                UsesHooks = true;
            }

            return base.VisitCallExpression(callExpression);
        }

        public override object? VisitJsxElement(JsxElement jsxElement)
        {
            // Extract component name from JSX element
            if (jsxElement.OpeningElement.Name is JsxIdentifier identifier)
            {
                var name = identifier.Name;
                // Only track if it's a component (starts with uppercase)
                if (!string.IsNullOrEmpty(name) && char.IsUpper(name[0]))
                {
                    UsedComponents.Add(name);
                }
            }
            else if (jsxElement.OpeningElement.Name is JsxMemberExpression memberExpr)
            {
                // Handle member expressions like <Module.Component />
                var fullName = GetJsxMemberExpressionName(memberExpr);
                if (!string.IsNullOrEmpty(fullName))
                {
                    UsedComponents.Add(fullName);
                }
            }

            return base.VisitJsxElement(jsxElement);
        }

        private string GetJsxMemberExpressionName(JsxMemberExpression expr)
        {
            var parts = new List<string>();
            JsxName? current = expr;

            while (current != null)
            {
                if (current is JsxMemberExpression member)
                {
                    if (member.Property is JsxIdentifier propId)
                    {
                        parts.Insert(0, propId.Name);
                    }
                    current = member.Object;
                }
                else if (current is JsxIdentifier id)
                {
                    parts.Insert(0, id.Name);
                    break;
                }
                else
                {
                    break;
                }
            }

            return string.Join(".", parts);
        }
    }

    /// <summary>
    /// Represents a component definition with its body for flattening
    /// </summary>
    private class ComponentDefinition
    {
        public string Name { get; set; } = string.Empty;
        public NodeList<Node> Params { get; set; } = new NodeList<Node>();
        public StatementOrExpression Body { get; set; } = null!;
    }

    /// <summary>
    /// Collects component definitions that need to be flattened
    /// </summary>
    private class ComponentDefinitionCollector : JsxAstVisitor
    {
        private readonly HashSet<string> _componentsToFlatten;
        public Dictionary<string, ComponentDefinition> ComponentDefinitions { get; } = new();

        public ComponentDefinitionCollector(List<string> componentsToFlatten)
        {
            _componentsToFlatten = new HashSet<string>(componentsToFlatten);
        }

        protected override object? VisitFunctionDeclaration(FunctionDeclaration functionDeclaration)
        {
            if (functionDeclaration.Id != null && _componentsToFlatten.Contains(functionDeclaration.Id.Name))
            {
                ComponentDefinitions[functionDeclaration.Id.Name] = new ComponentDefinition
                {
                    Name = functionDeclaration.Id.Name,
                    Params = functionDeclaration.Params,
                    Body = functionDeclaration.Body
                };
            }
            return base.VisitFunctionDeclaration(functionDeclaration);
        }

        protected override object? VisitVariableDeclaration(VariableDeclaration variableDeclaration)
        {
            foreach (var declarator in variableDeclaration.Declarations)
            {
                if (declarator.Id is Identifier id && _componentsToFlatten.Contains(id.Name))
                {
                    if (declarator.Init is ArrowFunctionExpression arrowFunc)
                    {
                        ComponentDefinitions[id.Name] = new ComponentDefinition
                        {
                            Name = id.Name,
                            Params = arrowFunc.Params,
                            Body = arrowFunc.Body
                        };
                    }
                    else if (declarator.Init is FunctionExpression funcExpr)
                    {
                        ComponentDefinitions[id.Name] = new ComponentDefinition
                        {
                            Name = id.Name,
                            Params = funcExpr.Params,
                            Body = funcExpr.Body
                        };
                    }
                }
            }
            return base.VisitVariableDeclaration(variableDeclaration);
        }
    }

    /// <summary>
    /// Transforms AST by inlining components and removing their definitions
    /// </summary>
    private class ComponentFlattenerTransformer : JsxAstRewriter
    {
        private readonly Dictionary<string, ComponentDefinition> _componentDefinitions;

        public ComponentFlattenerTransformer(Dictionary<string, ComponentDefinition> componentDefinitions)
        {
            _componentDefinitions = componentDefinitions;
        }

        protected override object? VisitProgram(Acornima.Ast.Program program)
        {
            // Visit the program to transform JSX usages first
            var result = (Acornima.Ast.Program)base.VisitProgram(program)!;
            
            // Filter out component definitions
            var filteredStatements = new List<Statement>();
            foreach (var statement in result.Body)
            {
                if (ShouldRemoveStatement(statement))
                {
                    continue; // Skip this statement
                }
                filteredStatements.Add(statement);
            }
            
            // Return updated program with filtered statements
            if (filteredStatements.Count != result.Body.Count)
            {
                return result.UpdateWith(NodeList.From(filteredStatements));
            }
            
            return result;
        }
        
        private bool ShouldRemoveStatement(Statement statement)
        {
            // Check if this is a function declaration to remove
            if (statement is FunctionDeclaration funcDecl && 
                funcDecl.Id != null && 
                _componentDefinitions.ContainsKey(funcDecl.Id.Name))
            {
                return true;
            }
            
            // Check if this is a variable declaration to remove
            if (statement is VariableDeclaration varDecl)
            {
                // Remove if all declarators are components to flatten
                var allShouldRemove = true;
                foreach (var declarator in varDecl.Declarations)
                {
                    if (declarator.Id is Identifier id && _componentDefinitions.ContainsKey(id.Name))
                    {
                        // This one should be removed
                        continue;
                    }
                    else
                    {
                        allShouldRemove = false;
                        break;
                    }
                }
                return allShouldRemove;
            }
            
            return false;
        }

        public override object? VisitJsxElement(JsxElement jsxElement)
        {
            // Check if this is a component usage that needs to be inlined
            if (jsxElement.OpeningElement.Name is JsxIdentifier identifier)
            {
                var componentName = identifier.Name;
                if (_componentDefinitions.TryGetValue(componentName, out var definition))
                {
                    // Inline the component here
                    return InlineComponent(definition, jsxElement);
                }
            }

            return base.VisitJsxElement(jsxElement);
        }

        private object? InlineComponent(ComponentDefinition definition, JsxElement jsxElement)
        {
            // Build a map of prop names to values
            var propMap = new Dictionary<string, Expression>();
            foreach (var attr in jsxElement.OpeningElement.Attributes)
            {
                if (attr is JsxAttribute jsxAttr && jsxAttr.Name is JsxIdentifier attrName)
                {
                    if (jsxAttr.Value is JsxExpressionContainer exprContainer && exprContainer.Expression is Expression expr)
                    {
                        propMap[attrName.Name] = expr;
                    }
                    else if (jsxAttr.Value is StringLiteral stringValue)
                    {
                        propMap[attrName.Name] = stringValue;
                    }
                }
            }

            // Get the JSX body from the component definition
            var bodyJsx = ExtractJsxFromBody(definition.Body);
            if (bodyJsx == null)
            {
                return jsxElement; // Can't inline, return unchanged
            }

            // Substitute props in the JSX body
            var substituter = new PropSubstituter(definition.Params, propMap);
            var inlinedJsx = substituter.Visit(bodyJsx);

            return inlinedJsx;
        }

        private Node? ExtractJsxFromBody(StatementOrExpression body)
        {
            // If arrow function with implicit return
            if (body is JsxElement || body is JsxFragment)
            {
                return body;
            }

            // If block statement, find the return statement
            if (body is BlockStatement blockStmt)
            {
                foreach (var stmt in blockStmt.Body)
                {
                    if (stmt is ReturnStatement returnStmt)
                    {
                        return returnStmt.Argument;
                    }
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Substitutes prop references with actual values in JSX
    /// </summary>
    private class PropSubstituter : JsxAstRewriter
    {
        private readonly Dictionary<string, string> _propParamNames;
        private readonly Dictionary<string, Expression> _propValues;

        public PropSubstituter(NodeList<Node> parameters, Dictionary<string, Expression> propValues)
        {
            _propParamNames = new Dictionary<string, string>();
            _propValues = propValues;

            // Map parameter names (handle destructuring)
            // Note: This implementation only handles destructured props ({ prop1, prop2 }).
            // Single props parameter (props) is not currently supported as it would require
            // more complex member expression substitution (e.g., props.label -> value).
            // Most React components use destructuring, so this covers the common case.
            foreach (var param in parameters)
            {
                if (param is ObjectPattern objPattern)
                {
                    // Destructured props: { label, onClick }
                    foreach (var prop in objPattern.Properties)
                    {
                        if (prop is AssignmentProperty assignProp && assignProp.Key is Identifier propId)
                        {
                            var paramName = propId.Name;
                            _propParamNames[paramName] = paramName;
                        }
                    }
                }
                // Note: Single props parameter case (param is Identifier) is intentionally not handled
            }
        }

        public override object? VisitJsxAttribute(JsxAttribute jsxAttribute)
        {
            // Check if the attribute value is an expression container with a substitutable identifier
            if (jsxAttribute.Value is JsxExpressionContainer exprContainer &&
                exprContainer.Expression is Identifier id &&
                _propParamNames.ContainsKey(id.Name) &&
                _propValues.TryGetValue(id.Name, out var value))
            {
                // If substituting with a string literal, convert to string attribute (not expression)
                if (value is StringLiteral stringLit)
                {
                    return jsxAttribute.UpdateWith(jsxAttribute.Name, stringLit);
                }
                else
                {
                    // For other expressions, keep the expression container with the new value
                    var newExprContainer = exprContainer.UpdateWith(value);
                    return jsxAttribute.UpdateWith(jsxAttribute.Name, newExprContainer);
                }
            }
            
            return base.VisitJsxAttribute(jsxAttribute);
        }

        public override object? VisitJsxExpressionContainer(JsxExpressionContainer jsxExpressionContainer)
        {
            // Replace prop references with actual values in children (text content)
            if (jsxExpressionContainer.Expression is Identifier id && _propParamNames.ContainsKey(id.Name))
            {
                // Found a prop reference, substitute it
                if (_propValues.TryGetValue(id.Name, out var value))
                {
                    // If the value is a string literal, we can return it directly as text
                    // instead of wrapping it in an expression container
                    if (value is StringLiteral stringLit)
                    {
                        // Return a JsxText node with the string value
                        return new JsxText(stringLit.Value, stringLit.Value);
                    }
                    else
                    {
                        // For other expressions, wrap in the expression container
                        return jsxExpressionContainer.UpdateWith(value);
                    }
                }
            }

            return base.VisitJsxExpressionContainer(jsxExpressionContainer);
        }
    }
}

