using System.Text.RegularExpressions;

namespace ReactComponentFlattener.Core.Services;

/// <summary>
/// Utility methods for handling TypeScript code
/// </summary>
internal static class TypeScriptHelper
{
    /// <summary>
    /// Strip TypeScript types from code to make it parseable as JavaScript/JSX
    /// </summary>
    public static string StripTypeScriptTypes(string code)
    {
        // Remove type-only imports (entire import statement)
        // Handles: import type X from "...";
        // Handles: import type { X } from "...";
        // Handles: import type { X, Y } from "...";
        code = Regex.Replace(code, @"import\s+type\s+(?:\{[^}]*\}|[A-Z]\w*)\s+from\s+['""][^'""]+['""];?\s*\n?", "", RegexOptions.Multiline);
        
        // Remove inline type imports from mixed imports
        // Handles: import { type X, Y } -> import { Y }
        // Handles: import { X, type Y, Z } -> import { X, Z }
        // Handles: import { type X } -> import { } (will be cleaned up later)
        code = Regex.Replace(code, @",\s*type\s+\w+(?:\s+as\s+\w+)?(?=\s*[,}])", "");
        code = Regex.Replace(code, @"type\s+\w+(?:\s+as\s+\w+)?\s*,\s*", "");
        code = Regex.Replace(code, @"\{\s*type\s+\w+(?:\s+as\s+\w+)?\s*\}", "{ }");
        
        // Remove empty imports: import { } from "..." or import { } from '...'
        code = Regex.Replace(code, @"import\s*\{\s*\}\s*from\s*['""][^'""]+['""];?\s*\n?", "");
        
        // Remove type alias declarations: type Name = Type;
        // Handle multi-line type aliases with intersection/union types and object types
        code = RemoveTypeAliases(code);
        
        // Remove interface declarations: interface Name { ... }
        // Handle simple interfaces and interfaces with nested braces
        code = RemoveInterfaces(code);
        
        // Remove function parameter type annotations (complex, multi-line)
        // This handles patterns like: }: Type) or }: Type & Type & { ... })
        code = RemoveDestructuredParameterTypeAnnotations(code);
        
        // Remove simple parameter type annotations: (param: Type) -> (param)
        // Only match when the type looks like an actual type (starts with uppercase, or is a primitive/array type)
        // This avoids matching object literal properties like: { key: 'value' }
        // Primitive types pattern
        code = Regex.Replace(code, @"(\(|,)\s*([a-zA-Z_$][\w$]*)\s*:\s*(string|number|boolean|any|unknown|never|void|null|undefined|symbol|bigint|object)\s*(?=[,)])", "$1 $2");
        // Type reference (PascalCase) pattern - match on same line only
        code = Regex.Replace(code, @"(\(|,)\s*([a-zA-Z_$][\w$]*)\s*:\s*([A-Z][\w\.]*(?:<[^>]+>)?(?:\s*\[\])?)\s*(?=[,)])", "$1 $2");
        // Rest/spread parameter type: ...name: Type[] -> ...name
        code = Regex.Replace(code, @"(\.\.\.[a-zA-Z_$][\w$]*)\s*:\s*[A-Z][\w\.]*(?:<[^>]+>)?(?:\s*\[\])?", "$1");
        
        // Remove variable type annotations: const x: Type = -> const x =
        code = Regex.Replace(code, @"(const|let|var)\s+([a-zA-Z_$][\w$]*)\s*:\s*[^=]+\s*=", "$1 $2 =");
        
        // Remove return type annotations: ): Type => -> ) =>  and ): Type { -> ) {
        code = Regex.Replace(code, @"\)\s*:\s*[^=>{]+\s*(=>|\{)", ") $1");
        
        // Remove generic type parameters: functionName<T> -> functionName
        code = RemoveGenericTypeParameters(code);
        
        // Remove type assertions: "as Type" (but not import/export aliases like "import * as name" or "Name as Alias")
        code = RemoveTypeAssertions(code);
        
        // Remove satisfies expressions: satisfies Type
        code = Regex.Replace(code, @"\s+satisfies\s+[A-Z][\w.]*(?:<[^>]+>)?", "");
        
        return code;
    }

    /// <summary>
    /// Remove type alias declarations, handling multi-line and complex types.
    /// </summary>
    private static string RemoveTypeAliases(string code)
    {
        // Pattern for type aliases - handle various forms:
        // type X = string;
        // type X = { ... };
        // type X = A & B;
        // type X = A | B;
        // type X<T> = ...;
        
        var result = new System.Text.StringBuilder();
        var lines = code.Split('\n');
        var inTypeAlias = false;
        var braceDepth = 0;
        
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var trimmed = line.TrimStart();
            
            // Check for start of type alias
            if (!inTypeAlias && Regex.IsMatch(trimmed, @"^(?:export\s+)?type\s+[A-Z]\w*"))
            {
                inTypeAlias = true;
                braceDepth = 0;
                
                // Count braces on this line
                foreach (var ch in line)
                {
                    if (ch == '{') braceDepth++;
                    else if (ch == '}') braceDepth--;
                }
                
                // Check if type alias ends on this line (semicolon at end, or braces balanced)
                if (trimmed.EndsWith(';') || (braceDepth == 0 && (line.Contains('=') || line.Contains('}') || line.Contains('>'))))
                {
                    inTypeAlias = false;
                }
                continue; // Skip this line
            }
            
            if (inTypeAlias)
            {
                // Continue counting braces
                foreach (var ch in line)
                {
                    if (ch == '{') braceDepth++;
                    else if (ch == '}') braceDepth--;
                }
                
                // Check for end of type alias
                if (braceDepth <= 0 && (trimmed.EndsWith(';') || trimmed.EndsWith('}') || trimmed.EndsWith('>')))
                {
                    inTypeAlias = false;
                }
                continue; // Skip this line
            }
            
            result.AppendLine(line);
        }
        
        return result.ToString();
    }

    /// <summary>
    /// Remove interface declarations, handling nested braces.
    /// </summary>
    private static string RemoveInterfaces(string code)
    {
        var result = new System.Text.StringBuilder();
        var lines = code.Split('\n');
        var inInterface = false;
        var braceDepth = 0;
        
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var trimmed = line.TrimStart();
            
            // Check for start of interface
            if (!inInterface && Regex.IsMatch(trimmed, @"^(?:export\s+)?interface\s+[A-Z]\w*"))
            {
                inInterface = true;
                braceDepth = 0;
                
                // Count braces on this line
                foreach (var ch in line)
                {
                    if (ch == '{') braceDepth++;
                    else if (ch == '}') braceDepth--;
                }
                
                // Check if interface ends on this line
                if (braceDepth == 0 && line.Contains('}'))
                {
                    inInterface = false;
                }
                continue; // Skip this line
            }
            
            if (inInterface)
            {
                // Continue counting braces
                foreach (var ch in line)
                {
                    if (ch == '{') braceDepth++;
                    else if (ch == '}') braceDepth--;
                }
                
                // Check for end of interface
                if (braceDepth == 0)
                {
                    inInterface = false;
                }
                continue; // Skip this line
            }
            
            result.AppendLine(line);
        }
        
        return result.ToString();
    }

    /// <summary>
    /// Remove destructured parameter type annotations that span multiple lines.
    /// Handles patterns like: }: Type & AnotherType & { prop?: value })
    /// </summary>
    private static string RemoveDestructuredParameterTypeAnnotations(string code)
    {
        var result = new System.Text.StringBuilder(code.Length);
        var i = 0;
        
        while (i < code.Length)
        {
            // Look for the pattern }: followed by a type annotation before )
            if (i + 1 < code.Length && code[i] == '}' && code[i + 1] == ':')
            {
                // Scan ahead to find if this is a destructured parameter type annotation
                // We need to find the closing ) while tracking braces, parens, and angle brackets
                var typeStart = i + 2;
                var j = typeStart;
                var braceDepth = 0;
                var parenDepth = 0;
                var angleDepth = 0;
                var foundClosingParen = false;
                
                // Skip whitespace after :
                while (j < code.Length && char.IsWhiteSpace(code[j]))
                {
                    j++;
                }
                
                // Scan through the type annotation
                while (j < code.Length)
                {
                    var ch = code[j];
                    
                    // Handle string literals
                    if (ch == '"' || ch == '\'' || ch == '`')
                    {
                        var quote = ch;
                        j++;
                        while (j < code.Length)
                        {
                            if (code[j] == '\\' && j + 1 < code.Length)
                            {
                                j += 2;
                            }
                            else if (code[j] == quote)
                            {
                                j++;
                                break;
                            }
                            else
                            {
                                j++;
                            }
                        }
                        continue;
                    }
                    
                    if (ch == '{')
                    {
                        braceDepth++;
                    }
                    else if (ch == '}')
                    {
                        if (braceDepth > 0)
                        {
                            braceDepth--;
                        }
                        else
                        {
                            // This } isn't part of the type, stop here
                            break;
                        }
                    }
                    else if (ch == '<')
                    {
                        angleDepth++;
                    }
                    else if (ch == '>')
                    {
                        if (angleDepth > 0)
                        {
                            angleDepth--;
                        }
                    }
                    else if (ch == '(')
                    {
                        parenDepth++;
                    }
                    else if (ch == ')')
                    {
                        if (parenDepth > 0)
                        {
                            parenDepth--;
                        }
                        else if (braceDepth == 0 && angleDepth == 0)
                        {
                            // This is the closing paren of the function parameters
                            foundClosingParen = true;
                            break;
                        }
                    }
                    
                    j++;
                }
                
                if (foundClosingParen)
                {
                    // Replace "}: TypeAnnotation)" with "})"
                    result.Append('}');
                    i = j; // Skip to the closing paren (which we'll add in the next iteration)
                    continue;
                }
            }
            
            result.Append(code[i]);
            i++;
        }
        
        return result.ToString();
    }

    /// <summary>
    /// Remove type assertions while preserving import aliases.
    /// Handles patterns like: expr as Type, value as Type['key'], } as React.CSSProperties
    /// Does NOT remove import aliases like: { Name as Alias }
    /// </summary>
    private static string RemoveTypeAssertions(string code)
    {
        var result = new System.Text.StringBuilder(code.Length);
        var i = 0;
        
        while (i < code.Length)
        {
            // Look for " as " pattern
            if (i + 4 < code.Length && 
                char.IsWhiteSpace(code[i]) && 
                code[i + 1] == 'a' && 
                code[i + 2] == 's' && 
                char.IsWhiteSpace(code[i + 3]))
            {
                // Check what comes before - if it's part of "import * as" or inside import braces, skip
                var beforeStart = Math.Max(0, i - 50);
                var before = code.Substring(beforeStart, i - beforeStart);
                
                // Check if this is "import * as name" pattern
                if (before.TrimEnd().EndsWith("*"))
                {
                    result.Append(code[i]);
                    i++;
                    continue;
                }
                
                // Check if we're inside an import statement's braces
                // Look for the context: find last unmatched { and see if import precedes it
                var lastOpenBrace = before.LastIndexOf('{');
                var lastCloseBrace = before.LastIndexOf('}');
                if (lastOpenBrace > lastCloseBrace)
                {
                    // We're inside braces, check if "import" precedes
                    var beforeBrace = before.Substring(0, lastOpenBrace);
                    if (beforeBrace.TrimEnd().EndsWith("import") || 
                        System.Text.RegularExpressions.Regex.IsMatch(beforeBrace, @"import\s*$") ||
                        System.Text.RegularExpressions.Regex.IsMatch(beforeBrace, @"import\s+\w+\s*,\s*$") ||
                        System.Text.RegularExpressions.Regex.IsMatch(beforeBrace, @"from\s+['""][^'""]+['""]\s*$"))
                    {
                        // Inside import braces, preserve the alias
                        result.Append(code[i]);
                        i++;
                        continue;
                    }
                }
                
                // This is a type assertion - scan and remove the type
                var typeStart = i + 4; // Skip " as "
                while (typeStart < code.Length && char.IsWhiteSpace(code[typeStart]))
                {
                    typeStart++;
                }
                
                // Find the end of the type (handle Type, Type.Nested, Type<Generic>, Type['key'])
                var typeEnd = typeStart;
                var bracketDepth = 0;
                var angleDepth = 0;
                
                while (typeEnd < code.Length)
                {
                    var ch = code[typeEnd];
                    
                    if (ch == '[')
                    {
                        bracketDepth++;
                    }
                    else if (ch == ']')
                    {
                        if (bracketDepth > 0)
                        {
                            bracketDepth--;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else if (ch == '<')
                    {
                        angleDepth++;
                    }
                    else if (ch == '>')
                    {
                        if (angleDepth > 0)
                        {
                            angleDepth--;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else if (bracketDepth == 0 && angleDepth == 0)
                    {
                        // Outside brackets/angles, check for type terminators
                        if (ch == ',' || ch == ')' || ch == '}' || ch == ';' || 
                            ch == '\n' || ch == '\r' || ch == '/' || ch == '&' || ch == '|')
                        {
                            break;
                        }
                        // Continue if it's part of a type identifier (letter, digit, dot, underscore)
                        if (!char.IsLetterOrDigit(ch) && ch != '.' && ch != '_' && ch != '\'' && ch != '"')
                        {
                            break;
                        }
                    }
                    
                    typeEnd++;
                }
                
                // Skip the " as Type" part entirely
                i = typeEnd;
                continue;
            }
            
            result.Append(code[i]);
            i++;
        }
        
        return result.ToString();
    }

    /// <summary>
    /// Remove generic type parameters using a balanced angle-bracket scanner.
    /// Handles nested generics like Map&lt;K, Array&lt;V&gt;&gt; correctly.
    /// </summary>
    private static string RemoveGenericTypeParameters(string code)
    {
        var result = new System.Text.StringBuilder(code.Length);
        var i = 0;
        
        while (i < code.Length)
        {
            // Look for an identifier followed by <
            if (IsIdentifierChar(code, i))
            {
                var identifierStart = i;
                
                // Scan the full identifier (including dots for namespaced identifiers)
                while (i < code.Length && (IsIdentifierChar(code, i) || code[i] == '.'))
                {
                    i++;
                }
                
                var identifierEnd = i;
                
                // Check if we have a < immediately after (no whitespace - TypeScript generics don't have space before <)
                // The < must not be followed by / (to avoid JSX closing tags like </Button>)
                if (i < code.Length && code[i] == '<' && (i + 1 >= code.Length || code[i + 1] != '/'))
                {
                    // Try to match balanced angle brackets
                    var genericEnd = FindBalancedAngleBracketEnd(code, i);
                    
                    if (genericEnd > i)
                    {
                        // We found a balanced generic type parameter, skip it
                        // Add the identifier without the generic part
                        result.Append(code.Substring(identifierStart, identifierEnd - identifierStart));
                        i = genericEnd + 1; // Skip past the closing >
                        continue;
                    }
                }
                
                // Not a generic type, add everything up to current position and continue
                result.Append(code.Substring(identifierStart, identifierEnd - identifierStart));
            }
            else
            {
                result.Append(code[i]);
                i++;
            }
        }
        
        return result.ToString();
    }
    
    /// <summary>
    /// Find the matching closing > for a balanced angle bracket expression.
    /// Returns the index of the closing >, or -1 if not found.
    /// </summary>
    private static int FindBalancedAngleBracketEnd(string code, int startIndex)
    {
        if (startIndex >= code.Length || code[startIndex] != '<')
        {
            return -1;
        }
        
        var depth = 0;
        var i = startIndex;
        
        while (i < code.Length)
        {
            var ch = code[i];
            
            // Handle string literals - skip their content
            if (ch == '"' || ch == '\'' || ch == '`')
            {
                var quote = ch;
                i++;
                while (i < code.Length)
                {
                    if (code[i] == '\\' && i + 1 < code.Length)
                    {
                        i += 2; // Skip escaped character
                    }
                    else if (code[i] == quote)
                    {
                        i++;
                        break;
                    }
                    else
                    {
                        i++;
                    }
                }
                continue;
            }
            
            // Check for operators that use < or > but aren't generics
            if (ch == '<')
            {
                // Check if this is part of <=, <<, <<=
                if (i + 1 < code.Length)
                {
                    var next = code[i + 1];
                    if (next == '=' || next == '<')
                    {
                        // This is a comparison or shift operator, not a generic
                        // If we're at depth 0, this isn't a generic type at all
                        if (depth == 0)
                        {
                            return -1;
                        }
                        i++;
                        continue;
                    }
                }
                depth++;
            }
            else if (ch == '>')
            {
                // In TypeScript generics, >> at the end is two closing brackets, not a shift operator
                // Check for >= or >> operators
                if (i + 1 < code.Length)
                {
                    var next = code[i + 1];
                    var isOperator = (next == '=') || (next == '>' && depth == 0);
                    
                    if (isOperator)
                    {
                        // At depth 0: >= or >> means this isn't a generic at all
                        // At depth 1: >= might be an operator, not a generic
                        if (depth == 0 || (next == '=' && depth == 1))
                        {
                            return -1;
                        }
                    }
                    // For >> at depth > 0, allow it - it's likely nested closing brackets
                }
                
                depth--;
                
                if (depth == 0)
                {
                    return i; // Found the matching closing >
                }
            }
            
            i++;
        }
        
        return -1; // No matching closing > found
    }
    
    /// <summary>
    /// Check if the character at the given index could be part of an identifier.
    /// </summary>
    private static bool IsIdentifierChar(string code, int index)
    {
        if (index >= code.Length)
        {
            return false;
        }
        
        var ch = code[index];
        return char.IsLetterOrDigit(ch) || ch == '_' || ch == '$';
    }
}
