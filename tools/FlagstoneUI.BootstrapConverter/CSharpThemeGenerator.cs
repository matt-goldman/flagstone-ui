using FlagstoneUI.BootstrapConverter.Models;
using System.Globalization;
using System.Text;

namespace FlagstoneUI.BootstrapConverter;

/// <summary>
/// Generates C# ResourceDictionary theme files from Flagstone tokens
/// </summary>
public class CSharpThemeGenerator
{
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
	/// Generate Tokens.cs file from Flagstone tokens
	/// </summary>
	public string GenerateTokensCs(FlagstoneTokens tokens, ConversionOptions? options = null)
	{
		options ??= new ConversionOptions();
		var ns = options.Namespace ?? "FlagstoneUI.Resources";

		var sb = new StringBuilder();
		
		// File header
		sb.AppendLine("using Microsoft.Maui.Controls;");
		sb.AppendLine();
		sb.AppendLine($"namespace {ns};");
		sb.AppendLine();
		if (options.IncludeComments)
		{
			sb.AppendLine("/// <summary>");
			sb.AppendLine("/// Token definitions for Flagstone UI theme");
			sb.AppendLine("/// Generated from Bootstrap theme");
			sb.AppendLine("/// </summary>");
		}
		sb.AppendLine("public class Tokens : ResourceDictionary");
		sb.AppendLine("{");
		sb.AppendLine("\tpublic Tokens()");
		sb.AppendLine("\t{");

		// Add color tokens
		if (tokens.Colors.Count > 0)
		{
			if (options.IncludeComments)
			{
				sb.AppendLine("\t\t// ===== Color Tokens =====");
			}
			foreach (var (key, token) in tokens.Colors.OrderBy(kvp => kvp.Key))
			{
				AddColorToken(sb, key, token, options);
			}
			sb.AppendLine();
		}

		// Add typography tokens
		if (tokens.Typography.Count > 0)
		{
			if (options.IncludeComments)
			{
				sb.AppendLine("\t\t// ===== Typography Tokens =====");
			}
			foreach (var (key, token) in tokens.Typography.OrderBy(kvp => kvp.Key))
			{
				AddTypographyToken(sb, key, token, options);
			}
			sb.AppendLine();
		}

		// Add spacing tokens
		if (tokens.Spacing.Count > 0)
		{
			if (options.IncludeComments)
			{
				sb.AppendLine("\t\t// ===== Spacing Tokens =====");
			}
			foreach (var (key, token) in tokens.Spacing.OrderBy(kvp => kvp.Key))
			{
				AddNumericToken(sb, key, token, options);
			}
			sb.AppendLine();
		}

		// Add border radius tokens
		if (tokens.BorderRadius.Count > 0)
		{
			if (options.IncludeComments)
			{
				sb.AppendLine("\t\t// ===== Corner Radius Tokens =====");
			}
			foreach (var (key, token) in tokens.BorderRadius.OrderBy(kvp => kvp.Key))
			{
				AddNumericToken(sb, key, token, options);
			}
			sb.AppendLine();
		}

		// Add border width tokens
		if (tokens.BorderWidth.Count > 0)
		{
			if (options.IncludeComments)
			{
				sb.AppendLine("\t\t// ===== Border Width Tokens =====");
			}
			foreach (var (key, token) in tokens.BorderWidth.OrderBy(kvp => kvp.Key))
			{
				AddNumericToken(sb, key, token, options);
			}
		}

		sb.AppendLine("\t}");
		sb.AppendLine("}");

		return sb.ToString();
	}

	private void AddColorToken(StringBuilder sb, string key, ColorToken token, ConversionOptions options)
	{
		if (options.IncludeComments && !string.IsNullOrWhiteSpace(token.Purpose))
		{
			sb.AppendLine($"\t\t// {key}: {token.Purpose}");
		}
		
		sb.AppendLine($"\t\tthis[\"{key}\"] = Color.FromArgb(\"{token.Value}\");");

		if (options.IncludeComments && !string.IsNullOrWhiteSpace(token.DarkValue))
		{
			sb.AppendLine($"\t\t// Dark mode: {token.DarkValue}");
		}
	}

	private void AddTypographyToken(StringBuilder sb, string key, TypographyToken token, ConversionOptions options)
	{
		if (options.IncludeComments && !string.IsNullOrWhiteSpace(token.Purpose))
		{
			sb.AppendLine($"\t\t// {key}: {token.Purpose}");
		}

		// Determine type based on key
		if (key.Contains("FontSize", StringComparison.Ordinal) || key.Contains("LineHeight", StringComparison.Ordinal))
		{
			if (double.TryParse(token.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var numValue))
			{
				sb.AppendLine($"\t\tthis[\"{key}\"] = {numValue.ToString(CultureInfo.InvariantCulture)};");
			}
			else
			{
				sb.AppendLine($"\t\tthis[\"{key}\"] = \"{token.Value}\";");
			}
		}
		else
		{
			// Font family - string value
			sb.AppendLine($"\t\tthis[\"{key}\"] = \"{token.Value}\";");
		}
	}

	private void AddNumericToken(StringBuilder sb, string key, NumericToken token, ConversionOptions options)
	{
		if (options.IncludeComments && !string.IsNullOrWhiteSpace(token.Purpose))
		{
			sb.AppendLine($"\t\t// {key}: {token.Purpose}");
		}
		
		sb.AppendLine($"\t\tthis[\"{key}\"] = {token.Value.ToString(CultureInfo.InvariantCulture)};");
	}

	/// <summary>
	/// Generate Theme.cs file
	/// </summary>
	public string GenerateThemeCs(FlagstoneTokens tokens, string themeName, ConversionOptions? options = null)
	{
		options ??= new ConversionOptions();
		var ns = options.Namespace ?? "FlagstoneUI.Resources";
		var sanitizedThemeName = SanitizeThemeName(themeName);

		var sb = new StringBuilder();
		
		sb.AppendLine("using Microsoft.Maui.Controls;");
		sb.AppendLine();
		sb.AppendLine($"namespace {ns};");
		sb.AppendLine();
		if (options.IncludeComments)
		{
			sb.AppendLine("/// <summary>");
			sb.AppendLine($"/// {themeName} theme resource dictionary for Flagstone UI controls.");
			sb.AppendLine("/// Generated from Bootstrap theme.");
			sb.AppendLine("/// </summary>");
		}
		sb.AppendLine($"public class {sanitizedThemeName} : ResourceDictionary");
		sb.AppendLine("{");
		sb.AppendLine($"\tpublic {sanitizedThemeName}()");
		sb.AppendLine("\t{");
		sb.AppendLine("\t\t// Merge tokens");
		sb.AppendLine("\t\tthis.MergedDictionaries.Add(new Tokens());");
		sb.AppendLine();
		if (options.IncludeComments)
		{
			sb.AppendLine("\t\t// Base control styles can be added here");
		}
		sb.AppendLine("\t}");
		sb.AppendLine("}");

		return sb.ToString();
	}

	/// <summary>
	/// Generate Styles.cs file with control styles
	/// </summary>
	public string GenerateStylesCs(FlagstoneTokens tokens, string themeName, ConversionOptions? options = null)
	{
		options ??= new ConversionOptions();
		var ns = options.Namespace ?? "FlagstoneUI.Resources";
		var sanitizedThemeName = SanitizeThemeName(themeName);

		var sb = new StringBuilder();
		
		sb.AppendLine("using Microsoft.Maui.Controls;");
		sb.AppendLine("using FlagstoneUI.Core.Controls;");
		sb.AppendLine();
		sb.AppendLine($"namespace {ns};");
		sb.AppendLine();
		if (options.IncludeComments)
		{
			sb.AppendLine("/// <summary>");
			sb.AppendLine($"/// {themeName} control styles resource dictionary.");
			sb.AppendLine("/// Generated from Bootstrap theme.");
			sb.AppendLine("/// </summary>");
		}
		sb.AppendLine($"public class {sanitizedThemeName}Styles : ResourceDictionary");
		sb.AppendLine("{");
		sb.AppendLine($"\tpublic {sanitizedThemeName}Styles()");
		sb.AppendLine("\t{");
		sb.AppendLine("\t\t// Merge tokens");
		sb.AppendLine("\t\tthis.MergedDictionaries.Add(new Tokens());");
		sb.AppendLine();
		if (options.IncludeComments)
		{
			sb.AppendLine("\t\t// TODO: Add control styles programmatically");
			sb.AppendLine("\t\t// For complex styles with visual states, XAML is recommended");
		}
		sb.AppendLine("\t}");
		sb.AppendLine("}");

		return sb.ToString();
	}

	/// <summary>
	/// Generate all theme files (Tokens.cs, Theme.cs, Styles.cs)
	/// </summary>
	public async Task GenerateFilesAsync(FlagstoneTokens tokens, string themeName, string outputDirectory, ConversionOptions? options = null)
	{
		options ??= new ConversionOptions();

		// Create output directory if it doesn't exist
		Directory.CreateDirectory(outputDirectory);

		// Generate Tokens.cs
		var tokensCs = GenerateTokensCs(tokens, options);
		var tokensPath = Path.Combine(outputDirectory, "Tokens.cs");
		await File.WriteAllTextAsync(tokensPath, tokensCs);

		// Generate Theme.cs
		var themeCs = GenerateThemeCs(tokens, themeName, options);
		var themePath = Path.Combine(outputDirectory, "Theme.cs");
		await File.WriteAllTextAsync(themePath, themeCs);

		// Generate Styles.cs
		var stylesCs = GenerateStylesCs(tokens, themeName, options);
		var stylesPath = Path.Combine(outputDirectory, "Styles.cs");
		await File.WriteAllTextAsync(stylesPath, stylesCs);
	}
}
